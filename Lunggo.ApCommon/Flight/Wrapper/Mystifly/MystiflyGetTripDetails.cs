﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lunggo.ApCommon.Flight.Constant;
using Lunggo.ApCommon.Flight.Model;
using Lunggo.ApCommon.Flight.Service;
using Lunggo.ApCommon.Mystifly.OnePointService.Flight;
using Lunggo.ApCommon.Product.Constant;
using Lunggo.ApCommon.Product.Model;
using FlightSegment = Lunggo.ApCommon.Flight.Model.FlightSegment;
using PassengerType = Lunggo.ApCommon.Mystifly.OnePointService.Flight.PassengerType;

namespace Lunggo.ApCommon.Flight.Wrapper.Mystifly
{
    internal partial class MystiflyWrapper
    {
        internal override GetTripDetailsResult GetTripDetails(TripDetailsConditions conditions)
        {
            var request = new AirTripDetailsRQ
            {
                UniqueID = conditions.BookingId,
                SendOnlyTicketed = false,
                SessionId = Client.SessionId,
                Target = Client.Target,
                ExtensionData = null
            };
            var result = new GetTripDetailsResult();
            var retry = 0;
            var done = false;
            while (!done)
            {
                var response = Client.TripDetails(request);
                done = true;
                if (response.Success && !response.Errors.Any())
                {
                    result = MapResult(response, conditions);
                    result.IsSuccess = true;
                    result.Errors = null;
                    result.ErrorMessages = null;
                }
                else
                {
                    if (response.Errors.Any())
                    {
                        result.Errors = new List<FlightError>();
                        result.ErrorMessages = new List<string>();
                        foreach (var error in response.Errors)
                        {
                            if (error.Code == "ERTDT001" || error.Code == "ERTDT002")
                            {
                                Client.CreateSession();
                                request.SessionId = Client.SessionId;
                                retry++;
                                if (retry <= 3)
                                {
                                    done = false;
                                    break;
                                }
                            }
                        }
                        if (done)
                            MapError(response, result);
                    }
                    result.IsSuccess = false;
                }
            }
            return result;
        }

        private static GetTripDetailsResult MapResult(AirTripDetailsRS response, TripDetailsConditions conditions)
        {
            var result = new GetTripDetailsResult();
            result.BookingId = response.TravelItinerary.UniqueID;
            result.FlightSegmentCount = response.TravelItinerary.ItineraryInfo.ReservationItems.Count();
            result.BookingNotes = response.TravelItinerary.BookingNotes.ToList();
            result.Itinerary = new FlightItinerary
            {
                BookingId = response.TravelItinerary.UniqueID,
                Trips = MapDetailsFlightTrips(response, conditions),
            };
            result.Passengers = MapDetailsPassengerInfo(response);
            result.TotalFare =
                decimal.Parse(response.TravelItinerary.ItineraryInfo.ItineraryPricing.TotalFare.Amount);
            result.Currency = response.TravelItinerary.ItineraryInfo.ItineraryPricing.TotalFare.CurrencyCode;
            MapDetailsPtcFareBreakdowns(response, result);
            return result;
        }

        private static List<FlightTrip> MapDetailsFlightTrips(AirTripDetailsRS response, ConditionsBase conditions)
        {
            try
            {
                var flightTrips = new List<FlightTrip>();
                var segments = response.TravelItinerary.ItineraryInfo.ReservationItems;
                var flight = FlightService.GetInstance();
                var i = 0;
                foreach (var tripInfo in conditions.Trips)
                {
                    var flightTrip = new FlightTrip
                    {
                        OriginAirport = tripInfo.OriginAirport,
                        DestinationAirport = tripInfo.DestinationAirport,
                        DepartureDate = DateTime.SpecifyKind(tripInfo.DepartureDate,DateTimeKind.Utc),
                        Segments = new List<FlightSegment>(),
                    };
                    do
                    {
                        flightTrip.Segments.Add(MapFlightSegmentDetails(segments[i]));
                        i++;
                    } while (i < segments.Count() &&
                             segments[i - 1].ArrivalAirportLocationCode != tripInfo.DestinationAirport &&
                             flight.GetAirportCityCode(segments[i - 1].ArrivalAirportLocationCode) !=
                             tripInfo.DestinationAirport);
                    flightTrips.Add(flightTrip);
                }
                return flightTrips;
            }
            catch (IndexOutOfRangeException)
            {
                var flightTrips = new List<FlightTrip>();
                var segments = response.TravelItinerary.ItineraryInfo.ReservationItems;
                foreach (var segment in segments)
                {
                    var flightTrip = new FlightTrip
                    {
                        OriginAirport = segment.DepartureAirportLocationCode,
                        DestinationAirport = segment.ArrivalAirportLocationCode,
                        DepartureDate = DateTime.SpecifyKind(segment.DepartureDateTime.Date,DateTimeKind.Utc),
                        Segments = new List<FlightSegment>(),
                    };
                    flightTrip.Segments.Add(MapFlightSegmentDetails(segment));
                    flightTrips.Add(flightTrip);
                }
                return flightTrips;
            }
        }

        private static FlightSegment MapFlightSegmentDetails(ReservationItem item)
        {
            var flight = FlightService.GetInstance();
            item.OperatingAirlineCode =
                (item.OperatingAirlineCode != "  " && item.OperatingAirlineCode != " " &&
                 item.OperatingAirlineCode != "" && item.OperatingAirlineCode != null)
                    ? item.OperatingAirlineCode
                    : item.MarketingAirlineCode;
            return new FlightSegment
            {
                DepartureAirport = item.DepartureAirportLocationCode,
                DepartureAirportName = flight.GetAirportName(item.DepartureAirportLocationCode),
                DepartureCity = flight.GetAirportCity(item.DepartureAirportLocationCode),
                DepartureTerminal = item.DepartureTerminal,
                DepartureTime = DateTime.SpecifyKind(item.DepartureDateTime,DateTimeKind.Utc),
                ArrivalAirport = item.ArrivalAirportLocationCode,
                ArrivalAirportName = flight.GetAirportName(item.ArrivalAirportLocationCode),
                ArrivalCity = flight.GetAirportCity(item.ArrivalAirportLocationCode),
                ArrivalTime = DateTime.SpecifyKind(item.ArrivalDateTime,DateTimeKind.Utc),
                ArrivalTerminal = item.ArrivalTerminal,
                Duration = TimeSpan.FromMinutes(double.Parse(item.JourneyDuration)),
                AirlineCode = item.MarketingAirlineCode,
                AirlineName = flight.GetAirlineName(item.MarketingAirlineCode),
                AirlineLogoUrl = flight.GetAirlineLogoUrl(item.MarketingAirlineCode),
                FlightNumber = item.FlightNumber,
                OperatingAirlineCode = item.OperatingAirlineCode,
                OperatingAirlineName = flight.GetAirlineName(item.OperatingAirlineCode),
                OperatingAirlineLogoUrl = flight.GetAirlineLogoUrl(item.OperatingAirlineCode),
                AircraftCode = item.AirEquipmentType,
                Rbd = item.ResBookDesigCode,
                StopQuantity = item.StopQuantity,
                BaggageCapacity = item.Baggage,
                Pnr = item.AirlinePNR
            };
        }

        private static List<Pax> MapDetailsPassengerInfo(AirTripDetailsRS response)
        {
            var passengerInfoDetails = new List<Pax>();
            foreach (var customerInfo in response.TravelItinerary.ItineraryInfo.CustomerInfos)
            {
                var passengerInfo = new Pax
                {
                    Title = MapDetailsPassengerTitle(customerInfo),
                    FirstName = customerInfo.Customer.PaxName.PassengerFirstName,
                    LastName = customerInfo.Customer.PaxName.PassengerLastName,
                    Type = MapDetailsPassengerType(customerInfo),
                };
                if (customerInfo.Customer.PassportNumber != null)
                    passengerInfo.PassportNumber = customerInfo.Customer.PassportNumber;
                passengerInfoDetails.Add(passengerInfo);
            }
            return passengerInfoDetails;
        }

        private static Title MapDetailsPassengerTitle(CustomerInfo customerInfo)
        {
            switch (customerInfo.Customer.PaxName.PassengerTitle.ToLower())
            {
                case "mr":
                    return Title.Mister;
                case "mrs":
                    return Title.Mistress;
                case "miss":
                    return Title.Miss;
                default:
                    return Title.Mister;
            }
        }

        private static Flight.Constant.PaxType MapDetailsPassengerType(CustomerInfo customerInfo)
        {
            switch (customerInfo.Customer.PassengerType)
            {
                case PassengerType.ADT:
                    return Flight.Constant.PaxType.Adult;
                case PassengerType.CHD:
                    return Flight.Constant.PaxType.Child;
                case PassengerType.INF:
                    return Flight.Constant.PaxType.Infant;
                default:
                    return Flight.Constant.PaxType.Adult;
            }
        }

        private static void MapDetailsPtcFareBreakdowns(AirTripDetailsRS response, GetTripDetailsResult result)
        {
            foreach (var breakdown in response.TravelItinerary.ItineraryInfo.TripDetailsPTC_FareBreakdowns)
            {
                switch (breakdown.PassengerTypeQuantity.Code)
                {
                    case PassengerType.ADT:
                        result.AdultTotalFare =
                            decimal.Parse(breakdown.TripDetailsPassengerFare.TotalFare.Amount);
                        break;
                    case PassengerType.CHD:
                        result.ChildTotalFare =
                            decimal.Parse(breakdown.TripDetailsPassengerFare.TotalFare.Amount);
                        break;
                    case PassengerType.INF:
                        result.InfantTotalFare =
                            decimal.Parse(breakdown.TripDetailsPassengerFare.TotalFare.Amount);
                        break;
                }
            }
        }

        private static void MapError(AirTripDetailsRS response, ResultBase result)
        {
            foreach (var error in response.Errors)
            {
                switch (error.Code)
                {
                    case "ERTDT003":
                        goto case "InvalidInputData";
                    case "ERTDT004":
                    case "ERTDT005":
                    case "ERTDT006":
                        goto case "BookingIdNoLongerValid";
                    case "ERTDT001":
                    case "ERTDT002":
                        if (result.ErrorMessages == null)
                            result.ErrorMessages = new List<string>();
                        if (!result.ErrorMessages.Contains("[Mystifly] Invalid account information!"))
                            result.ErrorMessages.Add("[Mystifly] Invalid account information!");
                        goto case "TechnicalError";
                    case "ERGEN006":
                        if (result.ErrorMessages == null)
                            result.ErrorMessages = new List<string>();
                        if (!result.ErrorMessages.Contains("[Mystifly] Unexpected error on the other end!"))
                            result.ErrorMessages.Add("[Mystifly] Unexpected error on the other end!");
                        goto case "TechnicalError";
                    case "ERMAI001":
                        if (result.ErrorMessages == null)
                            result.ErrorMessages = new List<string>();
                        if (!result.ErrorMessages.Contains("[Mystifly] Mystifly is under maintenance!"))
                            result.ErrorMessages.Add("[Mystifly] Mystifly is under maintenance!");
                        goto case "TechnicalError";

                    case "InvalidInputData":
                        if (!result.Errors.Contains(FlightError.InvalidInputData))
                            result.Errors.Add(FlightError.InvalidInputData);
                        break;
                    case "BookingIdNoLongerValid":
                        if (!result.Errors.Contains(FlightError.BookingIdNoLongerValid))
                            result.AddError(FlightError.BookingIdNoLongerValid, "[Mystifly] " + error.Code);
                        break;
                    case "TechnicalError":
                        if (!result.Errors.Contains(FlightError.TechnicalError))
                            result.Errors.Add(FlightError.TechnicalError);
                        break;
                }
            }
        }
    }
}
