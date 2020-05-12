﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        [Key] public int Id { get; set; }
        [ForeignKey("Flight")] public string FlightId { get; set; }
        [Required] public int Passengers { get; set; }
        [Required] public string CompanyName { get; set; }

        [Required] public bool isExternal { get; set; }

        //public InitialLocation InitLocation { get; set; }
        //public IEnumerable<Segment> Segments { get; set; }
    }
}
