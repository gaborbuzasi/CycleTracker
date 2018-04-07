using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrackerHook.API.Models
{
    public class TrackerModel
    {
        public Command CommandId { get; set; }
        public string Message { get; set; }
    }

    public enum Command
    {
        SET_INITIAL_STATE,
        SET_LOCATION
    }
}
