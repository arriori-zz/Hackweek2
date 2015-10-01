using Server.HackTestWPF;
using Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;

namespace Server.Controllers
{
   [Authorize]
    public class ExchangeController : ApiController
    {
        Location[] locations = new Location[]
        {
            new Location {Id = 1, Name = "Montevideo, UY"},
            new Location {Id = 2,  Name = "Cranbury, US" },
            new Location {Id = 3,  Name = "Sofia, BG"}
        };

        Room[] rooms = new Room[]
        {
            new Room {Id = "1", Name = "Mon A", Location= "1"},
            new Room {Id = "2",  Name = "Mon B", Location= "1" },
            new Room {Id = "3",  Name = "Mon C (No lifesize)", Location= "1"},
            new Room {Id = "4",  Name = "Mon D (No lifesize)", Location= "1"}
        };

        [Route("api/exchange/login")]
        [HttpPost]
        public IHttpActionResult Login(LoginCredentials credentials)
        {
            var loginResult = new LoginResult() {   };
            return Ok(loginResult);
        }

        [Route("api/exchange/bookRoom")]
        [HttpPost]
        public IHttpActionResult BookRoom(BookRoomParam param)
        {
            var credentials = GetLoginCredentials();
            var service = new HackExchangeService();
            HackExchangeContext context = new HackExchangeContext()
            {
                Credentials = new NetworkCredential(credentials.UserName, credentials.Password),
                Endpoint = credentials.Endpoint
            };
            var rooms = service.GetRooms(context).Where(r => r.Location == "Montevideo, Uruguay" || r.Location == "Montevideo");

            var freeRoom = AvailableNow(service, context, rooms, param.Time);

            if (freeRoom.Booked)
            {
                service.CreateAppointment(context, "Meeting", "Meeting scheduled through the new astonishing app", freeRoom.Start, freeRoom.End, freeRoom.Room, new List<string> { User.Identity.Name });
            }

            return Ok(freeRoom);
        }

        public BookResult AvailableNow(HackExchangeService service, HackExchangeContext context, IEnumerable<Room> rooms, int minutes)
        {
            int periodsNeeded = minutes / 15;
            var startingTime = DateTime.Now;
            var now = DateTime.Now;
            var today = new DateTime(now.Year, now.Month, now.Day, 00, 00, 00);
            var isFree = false;

            while (!isFree && today.Day == startingTime.Day)
            {
                foreach (var room in rooms)
                {
                    var periods = periodsOfDay();
                    var availability = service.GetRoomAvailability(context, room.EmailAddress, null, minutes, true);
                    foreach (var meetingTime in availability.BusyPeriods)
                    {
                        var start = meetingTime.StartTime;
                        var end = meetingTime.EndTime;
                        while (start < end)
                        {
                            var period = periods.FirstOrDefault(p => p.Start <= start && p.End >= start);
                            period.Free = false;
                            start = start.AddMinutes(15);
                        }
                    }
                    var actualSlot = periods.FirstOrDefault(p => startingTime >= p.Start && startingTime <= p.End);
                    var finallSlot = periods.FirstOrDefault(p => startingTime.AddMinutes(minutes) >= p.Start && startingTime.AddMinutes(minutes) <= p.End);
                    isFree = IsFree(periods, actualSlot, periodsNeeded);

                    if (isFree)
                    {
                        if (startingTime == now)
                            return new BookResult() { Booked = true, Room = room, Start = startingTime, End = finallSlot.End };
                        else
                            return new BookResult() { Booked = false, Room = room, Start = startingTime, End = finallSlot.End};
                    }


                }
                startingTime = startingTime.AddMinutes(15);
            }
            return null;
        }

        public bool IsFree(IEnumerable<Period> periods, Period start, int periodsNeeded)
        {
            while (periodsNeeded >= 0)
            {
                if (!start.Free)
                    return false;
                periodsNeeded--;
                start = periods.FirstOrDefault(p => p.Start == start.End);
            }
            return true;
        }

        public IEnumerable<Period> periodsOfDay()
        {
            var now = DateTime.Now;
            var today = new DateTime(now.Year, now.Month, now.Day, 00, 00, 00);
            var periods = new List<Period>();

            while (today.Day == now.Day)
            {
                var period = new Period();
                period.Start = today;
                period.Free = true;
                var end = today.AddMinutes(15);
                period.End = end;
                periods.Add(period);
                today = end;
            }

            return periods;
        }

        public IHttpActionResult BookThisRoom(BookResult book)
        {
            var credentials = GetLoginCredentials();
            var service = new HackExchangeService();
            HackExchangeContext context = new HackExchangeContext()
            {
                Credentials = new NetworkCredential(credentials.UserName, credentials.Password),
                Endpoint = credentials.Endpoint
            };
            book.Booked = true;
            service.CreateAppointment(context, "Meeting", "Meeting scheduled through the new astonishing app", book.Start, book.End, book.Room, new List<string> { User.Identity.Name });
            return Ok(book);
        }

        [Route("api/exchange/getrooms/{locationId}")]
        [HttpGet]
        public IEnumerable<Room> GetRoomsForLocation(string locationId)
        {
            Thread.Sleep(2000);
            return rooms.Where(r => r.Location == locationId);
        }

        [Route("api/exchange/freeThisRoom/{roomId}")]
        [HttpPost]
        public void FreeThisRoom(int roomId)
        {
        }

        [Route("api/exchange/addMinutes/{roomId}")]
        [HttpPost]
        public void addMinutes(int roomId, int minutes)
        {
        }

        [Route("api/exchange/getlocations")]
        [HttpGet]
        public IEnumerable<Location> GetLocations()
        {
            //var credentials = GetLoginCredentials();

            return locations;
        }

        private LoginCredentials GetLoginCredentials()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;

            var endpoint = claims.FirstOrDefault(c => c.Type == "Endpoint").Value;
            var password = claims.FirstOrDefault(c => c.Type == "Password").Value;

            var userName = User.Identity.Name;

            return new LoginCredentials() { Endpoint = endpoint, UserName = userName, Password = password };
        }
    }

    public class LoginCredentials
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Endpoint { get; set; }
    }

    public class BookRoomParam
    {
        public bool LifeSize { get; set; }

        public int Time { get; set; }
    }

    public class LoginResult
    {
        public string UserName { get; set; }
    }

    public class BookResult
    {
        public bool Booked { get; set; }

        public Room Room { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }

    public class Period
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Free { get; set; }
    }

}
