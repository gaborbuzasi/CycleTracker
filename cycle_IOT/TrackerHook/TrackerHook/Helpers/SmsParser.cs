using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackerHook.API.Models;

namespace TrackerHook.API.Helpers
{
    public static class SmsParser
    {
        public static TrackerModel ParseSmsBody(string body)
        {
            var data = body.Split(';');

            var model = new TrackerModel
            {
                CommandId = (Command)Enum.Parse(typeof(Command), data[0]),
                Message = data[1]
            };

            return model;
        }
    }
}
