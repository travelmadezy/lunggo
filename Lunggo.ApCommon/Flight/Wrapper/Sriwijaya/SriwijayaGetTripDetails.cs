﻿using System.Collections.Generic;
using System.Linq;
using Lunggo.ApCommon.Flight.Model;
using Lunggo.ApCommon.Flight.Service;

namespace Lunggo.ApCommon.Flight.Wrapper.Sriwijaya
{
    internal partial class SriwijayaWrapper
    {
        internal override GetTripDetailsResult GetTripDetails(TripDetailsConditions conditions)
        {
            var rsvNo = FlightService.GetRsvNoByBookingIdFromDb(new List<string> { conditions.BookingId }).Single();
            var reservation = FlightService.GetInstance().GetReservationFromDb(rsvNo);
            var itinerary = reservation.Itineraries.Single(itin => itin.BookingId == conditions.BookingId);
            var segments = itinerary.Trips.SelectMany(trip => trip.Segments).ToList();
            segments.ForEach(segment => segment.Pnr = conditions.BookingId);
            return new GetTripDetailsResult
            {
                IsSuccess = true,
                BookingId = conditions.BookingId,
                Itinerary = itinerary,
                FlightSegmentCount = segments.Count
            };
        }
    }
}
