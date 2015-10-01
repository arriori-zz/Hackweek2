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
            new Room {Id = 1, Name = "Mon A", LocationId = 1},
            new Room {Id = 2,  Name = "Mon B", LocationId = 1 },
            new Room {Id = 3,  Name = "Mon C (No lifesize)", LocationId = 1},
            new Room {Id = 4,  Name = "Mon D (No lifesize)", LocationId = 1}
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
            Thread.Sleep(2000);

            // If there is room available
            var bookResult = new BookResult() { Booked = true, Room = rooms[0], Start = DateTime.Now, End = DateTime.Now.AddMinutes(param.Time)};

            // If there is not, then it will find a possible slot.
            var negativeResult = new BookResult() { Booked = false, Room = rooms[0], Start = DateTime.Now.AddMinutes(10), End = DateTime.Now.AddMinutes(param.Time) };

            return Ok(negativeResult);
        }

        public IHttpActionResult BookThisRoom(BookResult book)
        {
            book.Booked = true;
            Thread.Sleep(2000);

            return Ok(book);
        }

        [Route("api/exchange/getrooms/{locationId}")]
        [HttpGet]
        public IEnumerable<Room> GetRoomsForLocation(int locationId)
        {
            Thread.Sleep(2000);
            return rooms.Where(r => r.LocationId == locationId);
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
            var credentials = GetLoginCredentials();

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

}
