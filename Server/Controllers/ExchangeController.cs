using HackTestWPF;
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
        public HackExchangeService ExchangeService
        {
            get
            {
                return new HackExchangeService();
            }
        }


        Location[] locations = new Location[]
        {
            new Location {Id = 1, Name = "Montevideo, UY", InternalNames = new [] { "Montevideo, Uruguay", "Montevideo"} },
            new Location {Id = 2,  Name = "Cranbury, US", InternalNames = new [] {"Cranbury, NJ", "Cranbury" } },
            new Location {Id = 3,  Name = "Sofia, BG", InternalNames = new [] { "Sofia, Bulgaria", "Sofia" } }
        };

        [Route("api/exchange/login")]
        [HttpPost]
        public IHttpActionResult Login(LoginCredentials credentials)
        {
            var loginResult = new LoginResult() { };
            return Ok(loginResult);
        }

        [Route("api/exchange/bookRoom")]
        [HttpPost]
        public IHttpActionResult BookRoom(BookRoomParam param)
        {
            var context = GetExchangeContext();

            var locationPossibleNames = locations.FirstOrDefault(l => l.Id == param.LocationId).InternalNames;

            var rooms = ExchangeService.GetRooms(context).Where(r => locationPossibleNames.Contains(r.Location));

            if (param.LifeSize && param.LocationId == 1)
            {
                rooms = rooms.Where(r => !r.Name.Contains("No Lifesize") && !r.Name.Contains("Huddle"));
            }

            var preferedRoomName = GetPreferedRoomName(param.PreferedRoom);
            if (!string.IsNullOrEmpty(preferedRoomName))
            {
                var preferedRoom = rooms.FirstOrDefault(r => r.Name == preferedRoomName);
                if (preferedRoom != null)
                {
                    var preferedRoomList = new List<Room>();
                    preferedRoomList.Add(preferedRoom);
                    var freePreferedRoom = AvailableNow(ExchangeService, context, preferedRoomList, param.Time);
                    if (freePreferedRoom.Booked)
                    {
                        var calendarItem = ExchangeService.CreateAppointment(context, "Your meeting - by MeetMeNow", "Meeting scheduled through a new astonishing app", freePreferedRoom.Start, freePreferedRoom.End, freePreferedRoom.Room, new List<string> { User.Identity.Name });
                        freePreferedRoom.CalendarItem = calendarItem;
                        return Ok(freePreferedRoom);
                    }
                }
            }

            var freeRoom = AvailableNow(ExchangeService, context, rooms, param.Time);

            if (freeRoom != null && freeRoom.Booked)
            {
                var calendarItem = ExchangeService.CreateAppointment(context, "Your meeting - by MeetMeNow", "Meeting scheduled through a new astonishing app", freeRoom.Start, freeRoom.End, freeRoom.Room, new List<string> { User.Identity.Name });
                freeRoom.CalendarItem = calendarItem;
            }

            return Ok(freeRoom);
        }

        [Route("api/exchange/bookThisRoom")]
        [HttpPost]
        public IHttpActionResult BookThisRoom(BookResult book)
        {
            var context = GetExchangeContext();

            var calendarItem = ExchangeService.CreateAppointment(context, "Meeting", "Meeting scheduled through the new astonishing app", book.Start, book.End, book.Room, new List<string> { User.Identity.Name });
            book.Booked = true;
            book.CalendarItem = calendarItem;

            return Ok(book);
        }

        [Route("api/exchange/freeThisRoom/{roomId}")]
        [HttpPost]
        public void FreeThisRoom(string roomId, BookResult book)
        {
            var context = GetExchangeContext();
            ExchangeService.CancelAppointment(context, book.CalendarItem);
        }

        [Route("api/exchange/addMinutes/{roomId}/{minutes}")]
        [HttpPost]
        public BookResult addMinutes(string roomId, int minutes, BookResult book)
        {
            var context = GetExchangeContext();
            var end = book.End.AddMinutes(minutes);
            var result = ExchangeService.UpdateAppointment(context, book.CalendarItem, book.Start, end);
            if (result)
            {
                book.End = end;
            }

            return book;
        }

        [Route("api/exchange/getlocations")]
        [HttpGet]
        public IEnumerable<Location> GetLocations()
        {
            return locations;
        }

        [Route("api/exchange/getrooms/{locationId}")]
        [HttpGet]
        public IEnumerable<Room> GetRoomsForLocation(string locationId)
        {
            //Thread.Sleep(2000);
            //return rooms.Where(r => r.Location == locationId);
            return new Room[0];
        }

        private HackExchangeContext GetExchangeContext()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;

            var endpoint = claims.FirstOrDefault(c => c.Type == "Endpoint").Value;
            var password = claims.FirstOrDefault(c => c.Type == "Password").Value;

            var userName = User.Identity.Name;



            HackExchangeContext context = new HackExchangeContext() { Credentials = new NetworkCredential(userName, password), Endpoint = endpoint };

            return context;
        }

        private BookResult AvailableNow(HackExchangeService service, HackExchangeContext context, IEnumerable<Room> rooms, int minutes)
        {
            int periodsNeeded = minutes / 15;

            var startingTime = DateTime.UtcNow;
            var now = DateTime.UtcNow;

            var startingTimeUY = DateTime.UtcNow.AddHours(-3);
            var nowUY = DateTime.UtcNow.AddHours(-3);

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
                        while (start <= end)
                        {
                            var period = periods.FirstOrDefault(p => p.Start <= start && p.End >= start);
                            period.Free = false;
                            start = start.AddMinutes(15);
                        }
                    }
                    var actualSlot = periods.FirstOrDefault(p => startingTimeUY >= p.Start && startingTimeUY <= p.End);
                    var finallSlot = periods.FirstOrDefault(p => startingTimeUY.AddMinutes(minutes) >= p.Start && startingTimeUY.AddMinutes(minutes) <= p.End);
                    isFree = IsFree(periods, actualSlot, periodsNeeded);

                    if (isFree)
                    {
                        if (startingTime == now)
                            return new BookResult() { Booked = true, Room = room, Start = startingTime, End = finallSlot.End };
                        else
                            return new BookResult() { Booked = false, Room = room, Start = startingTime, End = finallSlot.End };
                    }


                }
                startingTime = startingTime.AddMinutes(15);
            }
            return null;
        }

        private bool IsFree(IEnumerable<Period> periods, Period start, int periodsNeeded)
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

        private IEnumerable<Period> periodsOfDay()
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

        private string GetPreferedRoomName(string code)
        {
            if (code == "54480")
            {
                return "MON - B";
            }
            else if (code == "54481")
            {
                return "MON - C - No Lifesize";
            }
            else if (code == "54483")
            {
                return "MON - A";
            }
            else if (code == "54484")
            {
                return "MON - D (No Lifesize)";
            }

            return string.Empty;
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

        public string PreferedRoom { get; set; }

        public int LocationId { get; set; }
    }

    public class LoginResult
    {
        public string UserName { get; set; }
    }

    public class BookResult
    {
        public CalendarItem CalendarItem { get; set; }

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
