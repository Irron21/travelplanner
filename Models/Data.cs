using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginSystem.Models
{
    public class Data
    {
        public required string user_id { get; set; }
        // public required string email { get; set; }
        public required List<Trip> trips { get; set; } = new();
    }

    public class Trip
    {
        public required string trip_id { get; set; }
        //public required string user_id { get; set; } 
        public required string trip_name { get; set; }
        public required string trip_country { get; set; }   
        public required DateTime trip_start { get; set; }
        public required DateTime trip_end { get; set; }
        //public required List<Destination> destinations { get; set; } = new();
    }

    public class Destination
    {
        public required string destination_id { get; set; }
        public required string dest_state { get; set; }
        public required string dest_city { get; set; }
        public required DateTime dest_arr { get; set; }
        public required DateTime dest_dept { get; set; }
        public List<ActivityModel> Activities { get; set; } = new();
    }


    public class ActivityModel
    {
        public required string activity_id { get; set; }
        public required string activity_title { get; set; }
        public required string activity_description { get; set; }
        public required int activity_day { get; set; }
        public required string activity_place { get; set; }
        public required DateTime activity_start { get; set; }
        public required DateTime activity_end { get; set; }
        //public required List<Destination> destinations { get; set; } = new();
    }
}


