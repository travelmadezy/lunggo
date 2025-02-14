﻿using System.Collections.Generic;
using System.Linq;
using Lunggo.ApCommon.Flight.Constant;
using Lunggo.ApCommon.Flight.Model;

namespace Lunggo.ApCommon.Flight.Service
{
    public partial class FlightService
    {
        public TripType ParseTripType(string searchId)
        {
            var conditions = DecodeSearchConditions(searchId);
            return ParseTripType(conditions.Trips);
        }

        public TripType ParseTripType(List<FlightTrip> trips)
        {
            switch (trips.Count)
            {
                case 0:
                    return TripType.Undefined;
                case 1:
                    return TripType.OneWay;
                case 2:
                    return (trips.First().DestinationAirport == trips.Last().OriginAirport &&
                            trips.First().OriginAirport == trips.Last().DestinationAirport)
                        ? TripType.RoundTrip
                        : TripType.OpenJaw;
                default:
                    var circling = true;
                    for (var i = 1; i < trips.Count; i++)
                        if (trips[i].OriginAirport != trips[i - 1].DestinationAirport)
                            circling = false;
                    if (trips.First().OriginAirport != trips.Last().DestinationAirport)
                        circling = false;
                    return circling ? TripType.Circle : TripType.OpenJaw;
            }
        }

        public CabinClass ParseCabinClass(string code)
        {
            switch (code.ToUpper())
            {
                case "Y":
                    return CabinClass.Economy;
                case "C":
                    return CabinClass.Business;
                case "F":
                    return CabinClass.First;
                default:
                    return CabinClass.Undefined;
            }
        }

        public string ParseCabinClass(CabinClass cabinClass)
        {
            switch (cabinClass)
            {
                case CabinClass.Economy:
                    return "y";
                case CabinClass.Business:
                    return "c";
                case CabinClass.First:
                    return "f";
                default:
                    return "";
            }
        }
    }
}
