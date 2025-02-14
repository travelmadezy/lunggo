﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.UI;
using Lunggo.ApCommon.Payment.Model;
using Lunggo.ApCommon.Product.Model;
using Lunggo.Framework.Extension;
using Lunggo.Framework.Log;
using Newtonsoft.Json;
using CsQuery;
using CsQuery.StringScanner.ExtensionMethods;
using Lunggo.ApCommon.Constant;
using Lunggo.ApCommon.Flight.Constant;
using Lunggo.ApCommon.Flight.Model;
using Lunggo.ApCommon.Flight.Service;
using RestSharp;
using CabinClass = Lunggo.ApCommon.Flight.Constant.CabinClass;
using FareType = Lunggo.ApCommon.Flight.Constant.FareType;
using FlightSegment = Lunggo.ApCommon.Flight.Model.FlightSegment;

namespace Lunggo.ApCommon.Flight.Wrapper.LionAir
{
    internal partial class LionAirWrapper
    {
        internal override SearchFlightResult SearchFlight(SearchFlightConditions conditions)
        {
            return Client.SearchFlight(conditions);
        }

        private partial class LionAirClientHandler
        {
            internal SearchFlightResult SearchFlight(SearchFlightConditions conditions)
            {
                var log = LogService.GetInstance();
                if (conditions.Trips.Count > 1)
                    return new SearchFlightResult
                    {
                        IsSuccess = true,
                        Itineraries = new List<FlightItinerary>()
                    };

                var client = CreateCustomerClient();

                if (conditions.AdultCount == 0)
                {
                    //log.Post("[Lion Air Search]request must be at least one adult passenger", "#logging-dev");
                    return new SearchFlightResult
                    {
                        Errors = new List<FlightError> { FlightError.InvalidInputData },
                        ErrorMessages =
                            new List<string> { "[Lion Air] There must be at least one adult passenger" }
                    };
                }
                if (conditions.AdultCount + conditions.ChildCount > 7)
                {
                    //log.Post("[Lion Air Search] Total adult and children passenger must be not more than seven", "#logging-dev");
                    return new SearchFlightResult
                    {
                        Errors = new List<FlightError> { FlightError.InvalidInputData },
                        ErrorMessages =
                            new List<string> { "[Lion Air] Total adult and children passenger must be not more than seven" }
                    };
                }
                if (conditions.AdultCount < conditions.InfantCount)
                {
                    //log.Post("[Lion Air Search] Every infant must be accompanied by one adult", "#logging-dev");
                    return new SearchFlightResult
                    {
                        Errors = new List<FlightError> { FlightError.InvalidInputData },
                        ErrorMessages =
                            new List<string> { "[Lion Air] Every infant must be accompanied by one adult" }
                    };
                }
                if (conditions.Trips[0].DepartureDate > DateTime.Now.AddDays(331).Date)
                {
                    //log.Post("[Lion Air Search] Booking is allowed to max 331 days before the departure date", "#logging-dev");
                    return new SearchFlightResult
                    {
                        Errors = new List<FlightError> { FlightError.InvalidInputData },
                        ErrorMessages =
                            new List<string> { "[Lion Air] Booking is allowed to max 331 days before the departure date" }
                    };
                }

                // Airport Generalizing
                var trip0 = new FlightTrip
                {
                    OriginAirport = conditions.Trips[0].OriginAirport,
                    DestinationAirport = conditions.Trips[0].DestinationAirport,
                    DepartureDate = conditions.Trips[0].DepartureDate
                };
                if (trip0.OriginAirport == "JKT")
                    trip0.OriginAirport = "CGK";
                if (trip0.DestinationAirport == "JKT")
                    trip0.DestinationAirport = "CGK";
                if (trip0.OriginAirport == "SHA")
                    trip0.OriginAirport = "PVG";
                if (trip0.DestinationAirport == "SHA")
                    trip0.DestinationAirport = "PVG";
                if (trip0.OriginAirport == "BKK")
                    trip0.OriginAirport = "DMK";
                if (trip0.DestinationAirport == "BKK")
                    trip0.DestinationAirport = "DMK";

                var originAirport = trip0.OriginAirport;
                var destinationAirport = trip0.DestinationAirport;

                // [GET] Search Flight
                var originCountry = FlightService.GetInstance().GetAirportCountryCode(trip0.OriginAirport);
                var destinationCountry = FlightService.GetInstance().GetAirportCountryCode(trip0.DestinationAirport);
                CQ availableFares;
                CQ departureDate;
                string scr;
                var depDateText = "";

                {
                    // Calling The Zeroth Page
                    client.BaseUrl = new Uri("http://www.lionair.co.id");
                    const string url0 = @"Default.aspx";
                    var searchRequest0 = new RestRequest(url0, Method.GET);
                    searchRequest0.AddHeader("Referer", "http://www.lionair.co.id");

                    var searchResponse0 = client.Execute(searchRequest0);

                    if (searchResponse0.ResponseUri.AbsolutePath != "/Default.aspx" &&
                        (searchResponse0.StatusCode == HttpStatusCode.OK ||
                         searchResponse0.StatusCode == HttpStatusCode.Redirect))
                    {
                        //log.Post("[Lion Air Search] Error while requesting at Default.aspx. Unexpected RensponseUri absolute path", "#logging-dev");
                        return new SearchFlightResult
                        {
                            Errors = new List<FlightError> { FlightError.InvalidInputData },
                            ErrorMessages = new List<string> { "[Lion Air] || " + searchResponse0.Content }
                        };
                    }


                    //if (originCountry == "ID")
                    //{
                    // Calling The First Page
                    client.BaseUrl = new Uri("https://secure2.lionair.co.id");
                    const string url = @"lionairibe2/OnlineBooking.aspx";
                    var searchRequest = new RestRequest(url, Method.GET);
                    searchRequest.AddHeader("Referer", "http://www.lionair.co.id");
                    searchRequest.AddQueryParameter("trip_type", "one way");
                    searchRequest.AddQueryParameter("date_flexibility", "fixed");
                    searchRequest.AddQueryParameter("depart", originAirport);
                    searchRequest.AddQueryParameter("dest.1", destinationAirport);
                    searchRequest.AddQueryParameter("date.0", trip0.DepartureDate.ToString("ddMMM"));
                    searchRequest.AddQueryParameter("date.1", trip0.DepartureDate.ToString("ddMMM"));
                    searchRequest.AddQueryParameter("persons.0",
                        conditions.AdultCount.ToString(CultureInfo.InvariantCulture));
                    searchRequest.AddQueryParameter("persons.1",
                        conditions.ChildCount.ToString(CultureInfo.InvariantCulture));
                    searchRequest.AddQueryParameter("persons.2",
                        conditions.InfantCount.ToString(CultureInfo.InvariantCulture));
                    //searchRequest.AddQueryParameter("origin", "EN");
                    //searchRequest.AddQueryParameter("usercountry", "ID");

                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, certificate, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                           SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                    var searchResponse = client.Execute(searchRequest);

                    if (searchResponse.ResponseUri.AbsolutePath != "/lionairibe2/OnlineBooking.aspx" &&
                        (searchResponse.StatusCode == HttpStatusCode.OK ||
                         searchResponse.StatusCode == HttpStatusCode.Redirect))
                    {
                        //log.Post("[Lion Air Search] Error while requesting at lionairibe2/OnlineBooking.aspx at first page. Unexpected RensponseUri absolute path", "#logging-dev");
                        return new SearchFlightResult
                        {
                            Errors = new List<FlightError> { FlightError.InvalidInputData },
                            ErrorMessages = new List<string> { "[Lion Air] || " + searchResponse.Content }
                        };
                    }

                    //}
                    //else
                    //{
                    //    return new SearchFlightResult
                    //    {
                    //        IsSuccess = true,
                    //        Itineraries = new List<FlightItinerary>()
                    //    };
                    //}
                    try
                    {
                        //Calling The Second Page
                        const string url2 = @"lionairibe2/OnlineBooking.aspx";
                        var searchRequest2 = new RestRequest(url2, Method.GET);
                        searchRequest2.AddHeader("Referer", "https://secure2.lionair.co.id");

                        ServicePointManager.ServerCertificateValidationCallback +=
                            (sender, certificate, chain, sslPolicyErrors) => true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                               SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                        var searchResponse2 = client.Execute(searchRequest2);


                        if (searchResponse2.ResponseUri.AbsolutePath != "/lionairibe2/OnlineBooking.aspx" &&
                            (searchResponse2.StatusCode == HttpStatusCode.OK ||
                             searchResponse2.StatusCode == HttpStatusCode.Redirect))
                        {
                            //log.Post("[Lion Air Search] Error while requesting at lionairibe2/OnlineBooking.aspx at second Page. Unexpected RensponseUri absolute path", "#logging-dev");
                            return new SearchFlightResult
                            {
                                Errors = new List<FlightError> { FlightError.InvalidInputData },
                                ErrorMessages = new List<string> { "[Lion Air] Error while requesting at lionairibe2/OnlineBooking.aspx at second Page. Unexpected Rensponse path or status code || " + searchResponse2.Content }
                            };
                        }

                        var html2 = searchResponse2.Content;
                        var searchedHtml = (CQ)html2;
                        //Getting rows of all flights
                        availableFares = searchedHtml[".flight-matrix-row"];

                        //Getting departure date in DD MMM YYYY format (23 Jun 2016)
                        departureDate = searchedHtml[
                            ".box-content.searchdetails .row .col-md-6.col-sm-12.col-xs-12.border-right.rel-pos .row " +
                            ".col-md-6.col-sm-6.col-xs-6.pr20>p>label"].Text();
                        var departureDateText = departureDate.Text().Trim(' ');
                        var departureDateText1 = departureDateText.Trim('\n').Trim(' ').Split(' ');
                        depDateText = String.Join(" ", departureDateText1.Skip(1));

                        //Getting price formula script
                        var pageScript = html2;
                        var startIndex = pageScript.IndexOf("{'fares'");
                        var endIndex = pageScript.IndexOf("})");
                        scr = pageScript.Substring(startIndex, endIndex + 1 - startIndex);

                        var x = availableFares.Length;
                        var y = availableFares.Count();


                        var pricefunc = new GetLionAirPrice(scr);

                        //var retidx = 0;
                        var fareIds = new List<IDomObject>();
                        //var index = new List<int>();
                        switch (conditions.CabinClass)
                        {
                            case CabinClass.Economy:
                                {
                                    var j = 0;
                                    var it = 0;

                                    while (it < availableFares.Length)
                                    {
                                        if (availableFares[it].ChildElements.ToList().Count == 4)
                                        {
                                            if ((availableFares[it].ChildElements.ToList()[1].GetAttribute("class") !=
                                                 "flight-class sold-flight" &&
                                                 availableFares[it].ChildElements.ToList()[1].GetAttribute("class") !=
                                                 "flight-class not-available")
                                                || (availableFares[it].ChildElements.ToList()[2].GetAttribute("class") !=
                                                 "flight-class sold-flight" &&
                                                 availableFares[it].ChildElements.ToList()[2].GetAttribute("class") !=
                                                 "flight-class not-available"))
                                            {
                                                fareIds.Add(availableFares[it]);
                                                j = it + 1;
                                                while ((j != availableFares.Count()) &&
                                                           (availableFares[j - 1].GetAttribute("id")
                                                               .SubstringBetween(0,
                                                                   (availableFares[j - 1].GetAttribute("id").Length - 2))
                                                            ==
                                                            availableFares[j].GetAttribute("id")
                                                                .SubstringBetween(0,
                                                                    availableFares[j].GetAttribute("id").Length - 2))
                                                    )
                                                {
                                                    fareIds.Add(availableFares[j]);
                                                    j += 1;
                                                }
                                            }
                                            else
                                            {
                                                j = it + 1;
                                                //Skipping the second etc rows after getting the first row, because price is 1st row
                                                while ((j != availableFares.Count()) &&
                                                       (availableFares[j - 1].GetAttribute("id")
                                                           .SubstringBetween(0,
                                                               (availableFares[j - 1].GetAttribute("id").Length - 2))
                                                        ==
                                                        availableFares[j].GetAttribute("id")
                                                                .SubstringBetween(0,
                                                                    availableFares[j].GetAttribute("id").Length - 2))
                                                       && j != availableFares.Count())
                                                {
                                                    j += 1;
                                                }
                                            }
                                        }
                                        it = j;
                                    }
                                }
                                break;
                            case CabinClass.Business:
                                {
                                    var j = 0;
                                    var it = 0;
                                    while (it < availableFares.Count())
                                    {
                                        if (availableFares[it].ChildElements.ToList().Count == 4)
                                        {
                                            if ((availableFares[it].ChildElements.ToList()[3].GetAttribute("class") !=
                                                 "flight-class sold-flight" &&
                                                 availableFares[it].ChildElements.ToList()[3].GetAttribute("class") !=
                                                 "flight-class not-available"))
                                            {
                                                fareIds.Add(availableFares[it]);
                                                j = it + 1;

                                                while ((j != availableFares.Count()) &&
                                                        (availableFares[j - 1].GetAttribute("id")
                                                            .SubstringBetween(0,
                                                                (availableFares[j - 1].GetAttribute("id").Length - 2))
                                                            ==
                                                            availableFares[j].GetAttribute("id")
                                                                .SubstringBetween(0,
                                                                    availableFares[j].GetAttribute("id").Length - 2)))
                                                {
                                                    fareIds.Add(availableFares[j]);
                                                    j += 1;
                                                }

                                            }
                                            else
                                            {
                                                j = it + 1;
                                                while ((j != availableFares.Count()) &&
                                                       (availableFares[j - 1].GetAttribute("id")
                                                           .SubstringBetween(0,
                                                               (availableFares[j - 1].GetAttribute("id").Length - 2))
                                                        ==
                                                        availableFares[j].GetAttribute("id")
                                                                .SubstringBetween(0,
                                                                    availableFares[j].GetAttribute("id").Length - 2))
                                                       && j != availableFares.Count())
                                                {
                                                    j += 1;
                                                }
                                            }
                                        }
                                        it = j;
                                    }
                                }
                                break;
                            default:
                                fareIds = new List<IDomObject>();
                                break;
                        }

                        var itins = new List<FlightItinerary>();

                        for (var ind = 0; ind < fareIds.Count; ind++)
                        {
                            //For index 0 or all segment 1
                            var segments = new List<FlightSegment>();
                            var cityArrival2 = "";
                            var airportArrival2 = "";
                            //var cityDeparture2 = "";
                            var listFlightNo = new List<string>();
                            var listDepHr = new List<string>();
                            var subst1 = fareIds.ElementAt(ind).GetAttribute("id");
                            if (ind == 0 ||
                                (subst1.SubstringBetween(0, subst1.Length - 2) !=
                                 fareIds.ElementAt(ind - 1)
                                     .GetAttribute("id")
                                     .SubstringBetween(0, fareIds.ElementAt(ind - 1).GetAttribute("id").Length - 2)))
                            {
                                // Column 0a (Departure Data)
                                var departureInfo =
                                    fareIds.ElementAt(ind).ChildElements.ToList()[0].ChildElements.ToList()[0];
                                var airportDeparture =
                                    departureInfo.ChildElements.ToList()[0].ChildElements.ToList()[0].InnerText;
                                var cityDeparture = FlightService.GetInstance().GetAirportCity(airportDeparture);
                                var timeDeparture = departureInfo.ChildElements.ToList()[1].InnerText;
                                listDepHr.Add(timeDeparture);
                                var flightNo = departureInfo.ChildElements.ToList()[2].InnerText.TrimEnd(' ');
                                listFlightNo.Add(flightNo);
                                var airplaneName =
                                    departureInfo.ChildElements.ToList()[2].ChildElements.ToList()[0].InnerText;

                                const string format = "dd MMM yyyy HH:mm";
                                var provider = CultureInfo.InvariantCulture;
                                var depDates = depDateText + " " + timeDeparture; // 28 Jan 2016 10:30 //
                                var depDate = DateTime.SpecifyKind(DateTime.ParseExact(depDates, format, provider),
                                    DateTimeKind.Utc);

                                // Column 0c (Arrival Data)

                                var arrivalInfo =
                                    fareIds.ElementAt(ind).ChildElements.ToList()[0].ChildElements.ToList()[2];
                                var airportArrival =
                                    arrivalInfo.ChildElements.ToList()[0].ChildElements.ToList()[0].InnerText;
                                var cityArrival = FlightService.GetInstance().GetAirportCity(airportArrival);
                                var timeArrival = arrivalInfo.ChildElements.ToList()[1].InnerText;
                                var duration = arrivalInfo.ChildElements.ToList()[2].InnerText.Split(' ');
                                var durHour = Int32.Parse(duration[1].SubstringBetween(0, 1));
                                var durMin = Int32.Parse(duration[2].SubstringBetween(0, 2));
                                var dur = new TimeSpan(0, durHour, durMin, 0, 0);

                                var jamdatang = Convert.ToInt32(timeArrival.Split(':')[0]);
                                var jambrgkt = Convert.ToInt32(timeDeparture.Split(':')[0]);
                                DateTime arrDate;
                                if (jamdatang < jambrgkt)
                                {
                                    var W = depDate.AddDays(1);
                                    arrDate = DateTime.SpecifyKind(new DateTime(W.Year, W.Month, W.Day,
                                    Convert.ToInt32(timeArrival.Split(':')[0]),
                                    Convert.ToInt32(timeArrival.Split(':')[1]), 0), DateTimeKind.Utc);
                                }
                                else
                                {
                                    arrDate =
                                        DateTime.SpecifyKind(new DateTime(depDate.Year, depDate.Month, depDate.Day,
                                    Convert.ToInt32(timeArrival.Split(':')[0]),
                                    Convert.ToInt32(timeArrival.Split(':')[1]), 0), DateTimeKind.Utc);
                                }


                                //Calculate Price

                                string priceId; //FR00_C0_SLOT0
                                if (conditions.CabinClass == CabinClass.Economy)
                                {
                                    if (fareIds.ElementAt(ind).ChildElements.ToList()[1].InnerText != "Sold Out" &&
                                        fareIds.ElementAt(ind).ChildElements.ToList()[1].InnerText != "N/A")
                                    {
                                        priceId = fareIds.ElementAt(ind).ChildElements.ToList()[1].GetAttribute("id");
                                    }
                                    else
                                    {
                                        priceId = fareIds.ElementAt(ind).ChildElements.ToList()[2].GetAttribute("id");
                                    }
                                }
                                else
                                {
                                    priceId = fareIds.ElementAt(ind).ChildElements.ToList()[3].GetAttribute("id");
                                }

                                pricefunc.SetId(priceId);
                                var adultPrice = pricefunc.GetAdultPrice(conditions.AdultCount);
                                var childPrice = pricefunc.GetChildPrice(conditions.ChildCount);
                                var infantPrice = pricefunc.GetInfantPrice(conditions.InfantCount, conditions.ChildCount);
                                var price = adultPrice + childPrice + infantPrice;

                                var originCountry1 = FlightService.GetInstance().GetAirportCountryCode(airportDeparture);
                                var destinationCountry1 = FlightService.GetInstance().GetAirportCountryCode(airportArrival);

                                var baggage = GetBaggage(flightNo.Split(' ')[0], conditions.CabinClass, airportDeparture, airportArrival, originCountry1, destinationCountry1);
                                var isBaggageIncluded = false;

                                if (baggage != null)
                                {
                                    isBaggageIncluded = true;
                                }

                                segments.Add(new FlightSegment
                                {
                                    AirlineCode = flightNo.Split(' ')[0],
                                    FlightNumber = flightNo.Split(' ')[1],
                                    CabinClass = conditions.CabinClass,
                                    AirlineType = AirlineType.Lcc,
                                    DepartureAirport = airportDeparture,
                                    DepartureTime = depDate, //ASK
                                    ArrivalAirport = airportArrival,
                                    ArrivalTime = arrDate, //ASK
                                    OperatingAirlineCode = flightNo.Split(' ')[0],
                                    //StopQuantity = Convert.ToInt32(stopNo),
                                    Duration = dur,
                                    //AircraftCode = aircraftNo,
                                    DepartureCity = cityDeparture,
                                    ArrivalCity = cityArrival,
                                    AirlineName = airplaneName,
                                    OperatingAirlineName = airplaneName,
                                    IsMealIncluded = flightNo.Split(' ')[0] == "ID",
                                    IsPscIncluded = true,
                                    IsBaggageIncluded = isBaggageIncluded,
                                    BaggageCapacity = baggage

                                });
                                var j = ind + 1;
                                while ((j != fareIds.Count) && (subst1.SubstringBetween(0, subst1.Length - 2) ==
                                                                    fareIds.ElementAt(j)
                                                                        .GetAttribute("id")
                                                                        .SubstringBetween(0,
                                                                            fareIds.ElementAt(ind + 1)
                                                                                .GetAttribute("id")
                                                                                .Length - 2)))
                                {
                                    // Column 0.a (Departure)
                                    departureInfo =
                                        fareIds.ElementAt(j).ChildElements.ToList()[0].ChildElements.ToList()[0];
                                    var airportDeparture2 =
                                        departureInfo.ChildElements.ToList()[0].ChildElements.ToList()[0].InnerText;
                                    var cityDeparture2 = FlightService.GetInstance().GetAirportCity(airportDeparture2);
                                    var timeDeparture2 = departureInfo.ChildElements.ToList()[1].InnerText;
                                    listDepHr.Add(timeDeparture2);
                                    flightNo = departureInfo.ChildElements.ToList()[2].InnerText.TrimEnd(' ');
                                    listFlightNo.Add(flightNo);
                                    airplaneName =
                                        departureInfo.ChildElements.ToList()[2].ChildElements.ToList()[0].InnerText;

                                    jamdatang = Convert.ToInt32(timeArrival.Split(':')[0]);
                                    jambrgkt = Convert.ToInt32(timeDeparture2.Split(':')[0]);
                                    DateTime depDate2;
                                    if (jambrgkt < jamdatang)
                                    {
                                        DateTime date_a = arrDate.AddDays(1);
                                        depDate2 = DateTime.SpecifyKind(new DateTime(date_a.Year, date_a.Month, date_a.Day,
                                        Convert.ToInt32(timeDeparture2.Split(':')[0]),
                                        Convert.ToInt32(timeDeparture2.Split(':')[1]), 0), DateTimeKind.Utc);
                                    }
                                    else
                                    {
                                        depDate2 =
                                            DateTime.SpecifyKind(new DateTime(arrDate.Year, arrDate.Month, arrDate.Day,
                                        Convert.ToInt32(timeDeparture2.Split(':')[0]),
                                        Convert.ToInt32(timeDeparture2.Split(':')[1]), 0), DateTimeKind.Utc);
                                    }

                                    // Column 0.c (Arrival)

                                    arrivalInfo =
                                        fareIds.ElementAt(j).ChildElements.ToList()[0].ChildElements.ToList()[2];
                                    airportArrival2 =
                                        arrivalInfo.ChildElements.ToList()[0].ChildElements.ToList()[0].InnerText;
                                    cityArrival2 = FlightService.GetInstance().GetAirportCity(airportArrival2);
                                    var timeArrival2 = arrivalInfo.ChildElements.ToList()[1].InnerText;
                                    duration = arrivalInfo.ChildElements.ToList()[2].InnerText.Split(' ');
                                    durHour = Int32.Parse(duration[1].SubstringBetween(0, 1));
                                    durMin = Int32.Parse(duration[2].SubstringBetween(0, 2));
                                    dur = new TimeSpan(0, durHour, durMin, 0, 0);

                                    jamdatang = Convert.ToInt32(timeArrival2.Split(':')[0]);
                                    jambrgkt = Convert.ToInt32(timeDeparture2.Split(':')[0]);

                                    DateTime arrDate2;
                                    if (jamdatang < jambrgkt)
                                    {
                                        var date_a = depDate2.AddDays(1);
                                        arrDate2 = DateTime.SpecifyKind(new DateTime(date_a.Year, date_a.Month, date_a.Day,
                                        Convert.ToInt32(timeArrival2.Split(':')[0]),
                                        Convert.ToInt32(timeArrival2.Split(':')[1]), 0), DateTimeKind.Utc);
                                    }
                                    else
                                    {
                                        arrDate2 =
                                            DateTime.SpecifyKind(
                                                new DateTime(depDate2.Year, depDate2.Month, depDate2.Day,
                                        Convert.ToInt32(timeArrival2.Split(':')[0]),
                                        Convert.ToInt32(timeArrival2.Split(':')[1]), 0), DateTimeKind.Utc);
                                    }

                                    arrDate = arrDate2;
                                    timeArrival = timeArrival2;

                                    var originCountry2 = FlightService.GetInstance().GetAirportCountryCode(airportDeparture2);
                                    var destinationCountry2 = FlightService.GetInstance().GetAirportCountryCode(airportArrival2);

                                    var baggage2 = GetBaggage(flightNo.Split(' ')[0], conditions.CabinClass, airportDeparture2, airportArrival2, originCountry2, destinationCountry2);
                                    var isBaggageIncluded2 = false;

                                    if (baggage2 != null)
                                    {
                                        isBaggageIncluded2 = true;
                                    }

                                    segments.Add(new FlightSegment
                                    {
                                        AirlineCode = flightNo.Split(' ')[0],
                                        FlightNumber = flightNo.Split(' ')[1],
                                        CabinClass = conditions.CabinClass,
                                        DepartureAirport = airportDeparture2,
                                        DepartureTime =
                                            depDate2,
                                        ArrivalAirport = airportArrival2,
                                        ArrivalTime = arrDate2,
                                        OperatingAirlineCode = flightNo.Split(' ')[0],
                                        //StopQuantity = Int32.Parse(stopNo),
                                        Duration = dur,
                                        //AircraftCode = aircraftNo,
                                        DepartureCity = cityDeparture2,
                                        ArrivalCity = cityArrival2,
                                        AirlineName = airplaneName,
                                        OperatingAirlineName = airplaneName,
                                        IsMealIncluded = flightNo.Split(' ')[0] == "ID",
                                        IsBaggageIncluded = isBaggageIncluded2,
                                        BaggageCapacity = baggage2,
                                        IsPscIncluded = true
                                    });
                                    j += 1;
                                }

                                var depHrJoin = String.Join("|", listDepHr.ToArray());
                                var flightNoJoin = String.Join("|", listFlightNo.ToArray());

                                string lastDest;
                                string lastAirport;
                                if (segments.Count > 1)
                                {
                                    lastDest = cityArrival2;
                                    lastAirport = airportArrival2;
                                }
                                else
                                {
                                    lastDest = cityArrival;
                                    lastAirport = airportArrival;
                                }

                                var importantData = originAirport + "+"
                                                       + destinationAirport + "+"
                                                       + depDateText + "+"
                                                       + conditions.AdultCount + "+"
                                                       + conditions.ChildCount + "+"
                                                       + conditions.InfantCount + "+"
                                                       + FlightService.GetInstance().ParseCabinClass(conditions.CabinClass) + "+"
                                                       + price + "+" + priceId.SubstringBetween(3, priceId.Length) + "+" +
                                                       +segments.Count + "+" + flightNoJoin + "+" + depHrJoin;

                                var isInternational = CheckInternationality(segments);

                                var itin = new FlightItinerary
                                {
                                    AdultCount = conditions.AdultCount,
                                    ChildCount = conditions.ChildCount,
                                    InfantCount = conditions.InfantCount,
                                    CanHold = true,
                                    FareType = FareType.Published,
                                    RequireBirthDate = isInternational,
                                    RequirePassport = isInternational,
                                    RequireSameCheckIn = false,
                                    RequireNationality = isInternational,
                                    RequestedCabinClass = conditions.CabinClass,
                                    TripType = TripType.OneWay,
                                    Supplier = Supplier.LionAir,
                                    Price = new Price(),
                                    AdultPricePortion = adultPrice / price,
                                    ChildPricePortion = childPrice / price,
                                    InfantPricePortion = infantPrice / price,
                                    FareId = importantData,
                                    Trips = new List<FlightTrip>
                                {
                                    new FlightTrip
                                    {
                                        OriginAirport = airportDeparture,
                                        DestinationAirport = lastAirport,
                                        DepartureDate = depDate.Date,
                                        DestinationCity = lastDest,
                                        OriginCity = cityDeparture,
                                        Segments = segments
                                    }
                                }
                                };

                                Currency currclass;
                                var currencyList = Currency.GetAllCurrencies(Payment.Constant.Supplier.LionAir);
                                if (!currencyList.TryGetValue(pricefunc.GetCurrency(), out currclass))
                                {
                                    return new SearchFlightResult
                                    {
                                        IsSuccess = true,
                                        Itineraries = new List<FlightItinerary>()
                                    };
                                }

                                itin.Price.SetSupplier(price, new Currency(pricefunc.GetCurrency(), Payment.Constant.Supplier.LionAir));
                                itins.Add(itin);
                            }
                        }

                        //itins = itins.Where(itin => !itin.Trips[0].Segments.Exists(seg => seg.AirlineCode == "ID")).ToList();
                        //itins = itins.Where(itin => !itin.Trips[0].Segments.Exists(seg => seg.AirlineCode == "OD")).ToList();
                        //itins = itins.Where(itin => !itin.Trips[0].Segments.Exists(seg => seg.AirlineCode == "SL")).ToList();
                        if (trip0.DestinationAirport != "JKT")
                        {
                            itins =
                                itins.Where(
                                    itin => itin.Trips[0].Segments.Last().ArrivalAirport == trip0.DestinationAirport)
                                    .ToList();
                        }

                        if (trip0.OriginAirport != "JKT")
                        {
                            itins =
                                itins.Where(
                                    itin => itin.Trips[0].Segments.First().DepartureAirport == trip0.OriginAirport)
                                    .ToList();
                        }


                        return new SearchFlightResult
                        {
                            IsSuccess = true,
                            Itineraries = itins.Where(itin => itin.Price.SupplierCurrency.Rate != 0).ToList()
                        };
                    }
                    catch (Exception e)
                    {
                        //log.Post("[Lion Air Search] Technical Error happened. Exception : " + e.Message, "#logging-dev");
                        return new SearchFlightResult
                        {
                            Errors = new List<FlightError> { FlightError.TechnicalError },
                            ErrorMessages = new List<string> { "[Lion Air] " + e.Message }
                        };
                    }
                }
            }

            private bool CheckInternationality(List<FlightSegment> segments)
            {
                var segmentDepartureAirports = segments.Select(s => s.DepartureAirport);
                var segmentArrivalAirports = segments.Select(s => s.ArrivalAirport);
                var segmentAirports = segmentDepartureAirports.Concat(segmentArrivalAirports);
                var segmentCountries = segmentAirports.Select(FlightService.GetInstance().GetAirportCountryCode).Distinct();
                return segmentCountries.Count() > 1;
            }

            private class GetLionAirPrice
            {
                private readonly dynamic _priceScript;
                private string _id;

                public void SetId(string id)
                {
                    _id = id.SubstringBetween(21, id.Length);
                }

                public GetLionAirPrice(string scr)
                {
                    _priceScript = JsonConvert.DeserializeObject(scr, typeof(object));
                }

                private object WorkOutTripTotals()
                {

                    //GET WHICH FARE USED
                    var codeFlight = ParseId(_id);
                    var id1 = "";
                    var id2 = "";
                    /*var outId = "";
                    var inId = "";*/
                    if (codeFlight[0] == 0)
                    {
                        //outId = Id;
                        id1 = _id;
                    }
                    else
                    {
                        //inId = Id;
                        id2 = _id;
                    }

                    return SearchForBreakdown(id1, id2);
                }

                private static List<int> ParseId(string id)
                {
                    var codeFlight = new List<int>();

                    var splitId = id.Split('_');
                    if (splitId.Length == 1 && splitId[0] == "")
                    {
                        codeFlight.Add(1000);
                    }
                    else
                    {
                        codeFlight.Add(Convert.ToInt32(splitId[0].Substring(2)));
                        codeFlight.Add(Convert.ToInt32(splitId[1].Substring(1)));
                        codeFlight.Add(Convert.ToInt32(splitId[2].Substring(4)));
                    }
                    return codeFlight;
                }

                private object SearchForBreakdown(string outId, string inId)
                {
                    var ps = _priceScript.fares;
                    if (ps.Count == 0)
                    {
                        return null;
                    }

                    var noOutId = ParseId(outId);
                    var noInId = ParseId(inId);
                    if (noOutId.Count == 1)
                    {
                        noOutId.Clear();
                        noOutId.Add(0);
                        noOutId.Add(-1);
                        noOutId.Add(-1);
                    }
                    if (noInId.Count == 1)
                    {
                        noInId.Clear();
                        noInId.Add(1);
                        noInId.Add(-1);
                        noInId.Add(-1);
                    }

                    var temp = new object();
                    for (var i = 0; i < _priceScript.fares.Count; i++)
                    {
                        var fare = _priceScript.fares[i];
                        for (var j = 0; j < fare.Indices.Count; j++)
                        {
                            var ind = fare.Indices[j];
                            if (Convert.ToInt32(ind.obI) == noOutId[1] && Convert.ToInt32(ind.obJ) == noOutId[2]
                                && Convert.ToInt32(ind.ibI) == noInId[1] && Convert.ToInt32(ind.ibJ) == noInId[2])
                                temp = fare;
                        }
                    }
                    return temp;
                }

                public bool IsPscIncluded()
                {
                    dynamic fare = WorkOutTripTotals();
                    return fare.TaxBreakdown.D5 != null && fare.TaxBreakdown.D5 != "0";
                }

                public string GetCurrency()
                {
                    var a = _priceScript.curr;
                    return _priceScript.curr;
                }

                public decimal GetAdultPrice(int adult)
                {
                    if (adult == 0)
                        return 0M;

                    dynamic fare = WorkOutTripTotals();
                    var adultPrice1 = Convert.ToDecimal(fare.PaxFares[0].Base) +
                                     Convert.ToDecimal(fare.PaxFares[0].Taxes) +
                                     Convert.ToDecimal(fare.PaxFares[0].GST);
                    var adultPrice = adultPrice1 * adult;
                    return adultPrice;
                }

                public decimal GetChildPrice(int child)
                {
                    if (child == 0)
                        return 0M;

                    dynamic fare = WorkOutTripTotals();
                    var childPrice1 = Convert.ToDecimal(fare.PaxFares[1].Base) +
                                     Convert.ToDecimal(fare.PaxFares[1].Taxes) +
                                     Convert.ToDecimal(fare.PaxFares[1].GST);
                    var childPrice = childPrice1 * child;
                    return childPrice;
                }

                public decimal GetInfantPrice(int infant, int child)
                {
                    if (infant == 0)
                        return 0M;

                    dynamic fare = WorkOutTripTotals();
                    var idx = child > 0 ? 2 : 1;
                    var infantPrice1 = Convert.ToDecimal(fare.PaxFares[idx].Base) +
                                     Convert.ToDecimal(fare.PaxFares[idx].Taxes) +
                                     Convert.ToDecimal(fare.PaxFares[idx].GST);
                    var infantPrice = infantPrice1 * infant;
                    return infantPrice;
                }
            }
        }

    }
}

//}

