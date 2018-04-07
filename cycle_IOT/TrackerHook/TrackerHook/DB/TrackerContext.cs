using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TrackerHook
{
    public class TrackerContext : DbContext
    {
        public TrackerContext(DbContextOptions<TrackerContext> options) : base(options)
        {

        }
            
        public DbSet<TrackerEvent> TrackerEvents { get; set; }
    }

    public class TrackerEvent
    {
        [Key]
        public int Id { get; set; }
        public int TrackerId { get; set; }
        public DateTime Time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsAlert { get; set; }

        public Tracker Tracker { get; set; }
    }

    public class Tracker
    {
        [Key]
        public int Id { get; set; }
        public string DevicePhoneNumber { get; set; }
        public string DeviceNickName { get; set; }

        public ICollection<TrackerEvent> TrackerEvents { get; set; }
    }
}
