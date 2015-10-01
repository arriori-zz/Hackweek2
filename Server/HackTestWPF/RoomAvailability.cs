using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.HackTestWPF
{
    public class RoomAvailability
    {
        public IList<BusyTimeWindow> BusyPeriods { get; set; }
        public IDictionary<string, IList<BusyTimeWindow>> AttendeesBusyPeriods;
        public IList<DateTime> SuggestedTimes;
    }
}
