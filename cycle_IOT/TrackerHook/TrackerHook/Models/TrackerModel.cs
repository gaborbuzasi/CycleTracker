using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrackerHook.API.Models
{
    public class TrackerModel
    {
        public Command CommandId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string SatellitePrecision { get; set; }
        public int TrackerId { get; set; }
    }
    public enum Command
    {
        SET_INITIAL_STATE,
        SET_LOCATION,
        SET_TAMPER,
        SET_THEFT,
        SET_INACTIVITY
    }
}
