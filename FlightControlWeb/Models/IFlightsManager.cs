﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    interface IFlightsManager
    {
        Flight[] GetServerFlights(String dt);
        Flight[] GetAllFlights(String dt);
        void AddFlightPlan(FlightPlan fp);
        FlightPlan GetFlight(String id);
        void DeleteFlight(Flight f);
    }
}
