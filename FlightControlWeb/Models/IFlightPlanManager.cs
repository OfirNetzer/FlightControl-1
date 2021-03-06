﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public interface IFlightPlanManager
    {
        // this method should create a new flight plan and add it to our DBs
        public FlightPlan AddFlightPlan(Flight newFlight, int passengers, string companyName);
    }
}
