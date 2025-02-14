﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using CsQuery;
using CsQuery.StringScanner.ExtensionMethods;
using Lunggo.ApCommon.Flight.Constant;
using Lunggo.ApCommon.Flight.Model;
using Lunggo.ApCommon.Flight.Service;
using Lunggo.ApCommon.Payment.Model;
using Lunggo.ApCommon.Product.Constant;
using Lunggo.ApCommon.Product.Model;
using Lunggo.Framework.Config;
using RestSharp;
using System.Globalization;

namespace Lunggo.ApCommon.Flight.Wrapper.LionAir
{
    internal partial class LionAirWrapper
    {
        internal override BookFlightResult BookFlight(FlightBookingInfo bookInfo)
        {
            return Client.BookFlight(bookInfo);
        }

        private partial class LionAirClientHandler
        {
            internal BookFlightResult BookFlight(FlightBookingInfo bookInfo)
            {
                if (bookInfo.Itinerary.FareId == null)
                {
                    //throw new Exception("haloooo 1");
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Errors = new List<FlightError> { FlightError.InvalidInputData },
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        ErrorMessages = new List<string> { "[Lion Air] FareId is null" }
                    };
                }

                var provider = CultureInfo.InvariantCulture;

                string origin, dest, flightId;
                int segmentCount;
                DateTime depdate;
                int adultCount, childCount, infantCount;
                var bookingTimeLimit = "";
                var bookingReference = "";
                List<string> listflight;
                CabinClass cabinClass;
                var splittedFareId = bookInfo.Itinerary.FareId.Split('+');

                try
                {
                    origin = splittedFareId[0]; //CGK
                    dest = splittedFareId[1]; // SIN
                    depdate = Convert.ToDateTime(splittedFareId[2]);
                    adultCount = Convert.ToInt32(splittedFareId[3]);
                    childCount = Convert.ToInt32(splittedFareId[4]);
                    infantCount = Convert.ToInt32(splittedFareId[5]);
                    cabinClass = FlightService.GetInstance().ParseCabinClass(splittedFareId[6]);
                    flightId = splittedFareId[8];
                    segmentCount = Convert.ToInt32(splittedFareId[9]);
                    listflight = splittedFareId[10].Split('|').ToList();
                }
                catch
                {
                    return new BookFlightResult
                    {
                        Errors = new List<FlightError> { FlightError.FareIdNoLongerValid },
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "[Lion Air] FareId is no longer valid" }
                    };
                }

                List<string> errorMessages;
                CommonInputCheck(bookInfo.Passengers, depdate, out errorMessages);
                if (errorMessages.Count > 0)
                {
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.InvalidInputData },
                        ErrorMessages = errorMessages
                    };
                }

                // [GET] Search Flight

                var client = CreateAgentClient();
                var flight = FlightService.GetInstance();
                var originCountry = flight.GetAirportCountryCode(origin);
                var destinationCountry = flight.GetAirportCountryCode(dest);
                var itin = new FlightItinerary();
                var listDepHr = new List<string>();
                var listFlightNo = new List<string>();
                string errorMessage;
                string userId;
                string userName;

                var succeedLogin = Login(client, out userName, out userId, out errorMessage);
                if (!succeedLogin)
                {
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { errorMessage }
                    };
                }

                //GET PAGE CONST ID
                var startind = userId.IndexOf("consID");
                var cid = userId.SubstringBetween(startind, userId.Length);
                var url2 = @"/LionAirAgentsIBE/OnlineBooking.aspx?" + cid;
                var searchRequest2 = new RestRequest(url2, Method.GET);
                searchRequest2.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                searchRequest2.AddHeader("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                searchRequest2.AddHeader("Referer",
                    "https://agent.lionair.co.id/LionAirAgentsPortal/Agents/Welcome.aspx");
                Thread.Sleep(3000);
                var searchResponse2 = client.Execute(searchRequest2);
                if ((searchResponse2.ResponseUri != null && searchResponse2.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/OnlineBooking.aspx") ||
                    (searchResponse2.StatusCode != HttpStatusCode.OK &&
                     searchResponse2.StatusCode != HttpStatusCode.Found && searchResponse2.StatusCode != HttpStatusCode.Redirect ))
                {
                    LogOut(cid, client);
                    TurnInUsername(userName);
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Lion Air] Error in requesting searchRequest2 at /LionAirAgentsIBE/OnlineBooking.aspx.Unexpected absolute path response or status code." }
                    };
                }
                var tempSearchResponse = searchResponse2;

                //GET PAGE ONLINE BOOKING (PAGE MILIH PESAWAT)
                const string url3 = @"/LionAirAgentsIBE/OnlineBooking.aspx";
                var searchRequest3 = new RestRequest(url3, Method.GET);
                searchRequest3.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                searchRequest3.AddHeader("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                searchRequest3.AddHeader("Referer",
                    "https://agent.lionair.co.id/LionAirAgentsPortal/Agents/Welcome.aspx");
                Thread.Sleep(3000);
                var searchResponse3 = client.Execute(searchRequest3);
                if ((searchResponse3.ResponseUri != null && searchResponse3.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/OnlineBooking.aspx") ||
                    (searchResponse3.StatusCode != HttpStatusCode.OK &&
                     searchResponse3.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse3.StatusCode != HttpStatusCode.Found))
                {
                    LogOut(cid, client);
                    TurnInUsername(userName);
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Lion Air] Error in requesting searchRequest3 at Get Page Online Booking (Choosing Flight) in /LionAirAgentsIBE/OnlineBooking.aspx.Unexpected absolute path response or status code." }
                    };
                }
                //var vs = new CQ();
                var html3 = searchResponse3.Content;
                var vs = (CQ)html3;
                var vs4 = HttpUtility.UrlEncode(vs["#__VIEWSTATE"].Attr("value"));

                //POST FOR PAGE AVAILABLE FLIGHTS AND PRICE 

                const string url4 = @"LionAirAgentsIBE/Step1.aspx";
                var searchRequest4 = new RestRequest(url4, Method.POST);
                searchRequest4.AddHeader("Accept-Encoding", "gzip, deflate");
                searchRequest4.AddHeader("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                searchRequest4.AddHeader("Referer", "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                searchRequest4.AddHeader("Origin", "https://agent.lionair.co.id");

                var cityairport = new GetCityAirportPair();
                string cityDep;
                string cityArr;

                var check1 = cityairport.GetCity(origin, out cityDep);
                var check2 = cityairport.GetCity(dest, out cityArr);
                var postData4 =
                    @"__EVENTTARGET=UcFlightSelection%24lbSearch" + @"&__EVENTARGUMENT=" + @"&__VIEWSTATE=" + vs4 +
                    @"&UcFlightSelection%24TripType=rbOneWay" +
                    @"&UcFlightSelection%24DateFlexibility=rbMustTravel" +
                    @"&UcFlightSelection%24txtSelOri=" + origin +
                    @"&UcFlightSelection%24txtOri=" + cityDep + "+%28" + origin + "%29" +
                    @"&UcFlightSelection%24ddlDepMonth=" + depdate.ToString("MMM") + "+" + depdate.Year +
                    @"&UcFlightSelection%24ddlDepDay=" + depdate.Day +
                    @"&UcFlightSelection%24ddlADTCount=" + adultCount +
                    @"&UcFlightSelection%24txtSelDes=" + dest +
                    @"&UcFlightSelection%24txtDes=" + cityArr + "+%28" + dest + "%29" +
                    @"&UcFlightSelection%24ddlCNNCount=" + childCount +
                    @"&UcFlightSelection%24ddlINFCount=" + infantCount +
                    @"&UcFlightSelection%24txtDepartureDate=" + depdate.Day + "+" + depdate.ToString("MMM") + "+" +
                    depdate.Year +
                    @"&UcFlightSelection%24txtReturnDate=" + depdate.Day + "+" + depdate.ToString("MMM") + "+" +
                    depdate.Year;
                Thread.Sleep(3000);
                searchRequest4.AddParameter("application/x-www-form-urlencoded", postData4, ParameterType.RequestBody);
                var searchResponse4 = client.Execute(searchRequest4);
                if ((searchResponse4.ResponseUri != null && searchResponse4.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/Step1.aspx") ||
                    (searchResponse4.StatusCode != HttpStatusCode.OK &&
                     searchResponse4.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse4.StatusCode != HttpStatusCode.Found))
                {
                    LogOut(cid, client);
                    TurnInUsername(userName);
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Lion Air] Error in searchRequest4 Posting for Page Available Flights And Price at LionAirAgentsIBE/Step1.aspx.Unexpected absolute path response or status code." }
                    };
                }
                tempSearchResponse = searchResponse4;
                // GET THE PAGE OF FLIGHTS (ONLINE BOOKING)

                const string url5 = @"/LionAirAgentsIBE/OnlineBooking.aspx";
                var searchRequest5 = new RestRequest(url5, Method.GET);
                searchRequest5.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                searchRequest5.AddHeader("Content-Encoding", "gzip");
                searchRequest5.AddHeader("Host", "agent.lionair.co.id");
                searchRequest5.AddHeader("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                searchRequest5.AddHeader("Referer", "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                Thread.Sleep(3000);
                var searchResponse5 = client.Execute(searchRequest5);
                if ((searchResponse5.ResponseUri != null && searchResponse5.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/OnlineBooking.aspx") ||
                    (searchResponse5.StatusCode != HttpStatusCode.OK &&
                     searchResponse5.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse5.StatusCode != HttpStatusCode.Found))
                {
                    LogOut(cid, client);
                    TurnInUsername(userName);
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Lion Air] Error in searchRequest5 GET THE PAGE OF FLIGHTS (ONLINE BOOKING) at /LionAirAgentsIBE/OnlineBooking.aspx.Unexpected absolute path response or status code." }
                    };
                }
                var html5 = searchResponse5.Content;

                var pageFlight = (CQ)html5;

                try
                {
                    var rows = pageFlight["#tblOutFlightBlocks > tbody"].Children();
                    var selectedRows = new List<IDomObject>();
                    var v = 2;

                    while (v < rows.Count())
                    {

                        if (selectedRows.Count == segmentCount)
                        {
                            break;
                        }
                        var plane = rows[v].ChildElements.ToList()[0].ChildElements.ToList()[0].InnerText;
                        var w = 0;
                        if (plane == listflight.ElementAt(w))
                        {
                            selectedRows.Add(rows[v]);
                            var z = v + 1;
                            w += 1;
                            while (z < rows.Count() && z < v + listflight.Count())
                            {
                                plane = rows[z].ChildElements.ToList()[0].ChildElements.ToList()[0].InnerText;
                                if (plane == listflight.ElementAt(w))
                                {
                                    selectedRows.Add(rows[z]);
                                }
                                else
                                {
                                    selectedRows.Clear();
                                    break;
                                }
                                z += 1;
                                w += 1;
                            }
                            v = z;
                        }
                        else
                        {
                            v = v + 1;
                        }
                    }

                    var txt_OBNNRowID = selectedRows.Last().Id;
                    var passengers = adultCount + childCount; //jumlah adult + children
                    var colCollection = new List<List<String>>();
                    var seatCollection = new List<List<String>>();

                    //PILIH COLUMN YG MASIH AVAILABLE 
                    switch (cabinClass)
                    {
                        case CabinClass.Economy:
                            {
                                for (var i = 0; i < selectedRows.Count; i++)
                                {
                                    colCollection.Add(new List<String>());
                                    seatCollection.Add(new List<String>());
                                    var selectedColumns =
                                        selectedRows[i].ChildElements.ToList().GetRange(9, 18).ToList();
                                    foreach (IDomElement t in selectedColumns)
                                    {
                                        if (t.GetAttribute("class") != "step2_soldcell fareInfo_middle_tconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_middle_bconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_middle"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_middle_mconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_right_tconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_right_bconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_right"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_right_mconx")
                                        {
                                            if (t.InnerText != "No Fares")
                                            {
                                                if (t.ChildElements.ToList()[1].ChildElements.ToList().Count() == 1
                                                    && Convert.ToInt32(
                                                        t.ChildElements.ToList()[0].ChildElements.ToList()[1].InnerText) >= passengers)
                                                {
                                                    colCollection[i].Add(t.GetAttribute("id").SubstringBetween(0,
                                                                t.GetAttribute("id").Length - 3));
                                                    seatCollection[i].Add(t.ChildElements.ToList()[0].GetAttribute("title"));
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case CabinClass.Business:
                            {
                                for (var i = 0; i < selectedRows.Count; i++)
                                {
                                    colCollection.Add(new List<string>());
                                    seatCollection.Add(new List<string>());
                                    var selectedColumns = selectedRows[i].ChildElements.ToList().GetRange(5, 5).ToList();
                                    foreach (IDomElement t in selectedColumns)
                                    {
                                        if (t.GetAttribute("class") != "step2_soldcell fareInfo_middle_tconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_middle_bconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_middle"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_middle_mconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_right_tconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_right_bconx"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_right"
                                            && t.GetAttribute("class") != "step2_soldcell fareInfo_right_mconx")
                                        {
                                            if (t.InnerText != "No Fares")
                                            {
                                                if (
                                                    t.ChildElements.ToList()[1].ChildElements.ToList().Count() == 1
                                                    && Convert.ToInt32(
                                                        t.ChildElements.ToList()[0].ChildElements.ToList()[1].InnerText) >= passengers)
                                                {
                                                    colCollection[i].Add(t.GetAttribute("id")
                                                            .SubstringBetween(0, t.GetAttribute("id").Length - 3));
                                                    seatCollection[i].Add(t.ChildElements.ToList()[0].GetAttribute("title"));
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                    }

                    List<String> priceCollections;
                    string seat;
                    //priceCollections = list of string, isinya kolom paling kanan/terakhir dari tiap segmen
                    if (colCollection.Count != 0)
                    {
                        priceCollections = colCollection.Select(seg => seg[seg.Count - 1]).ToList();
                        seat = string.Join("|", seatCollection.Select(seg => seg[seg.Count - 1]).ToArray());
                    }
                    else
                    {
                        LogOut(cid, client);
                        TurnInUsername(userName);
                        return new BookFlightResult
                        {
                            IsSuccess = false,
                            Errors = new List<FlightError> { FlightError.TechnicalError },
                            Status = null,
                            ErrorMessages = new List<string> { "[Lion Air] Flight is not available anymore" },
                            IsValid = true
                        };
                    }

                    if (priceCollections.Count != 0) // Kalau casenya cabin business kdg2 suka habis
                    {
                        var postdata = new CreatePostData();
                        var colpost = postdata.Create(rows, priceCollections);
                        const string garbled = "ScriptManager1=upnlTotalTripCost%7CbtnPriceSelection&__EVENTTARGET=btnPriceSelection&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE=";
                        //if (originCountry == "ID")
                        //{
                        var data =
                            "&txtUpdateInsurance=no" +
                            "&Insurance%24rblInsurance=No" +
                            "&Insurance%24txtInsPostbackRequired=no" +
                            "&txtPricingResponse=OK" + "" +
                            "&txtOutFBCsUsed=" + //seat +
                            "&txtInFBCsUsed=" +
                            "&txtTaxBreakdown=" +
                            "&UcFlightSelection%24TripType=rbOneWay" + "" +
                            "&UcFlightSelection%24DateFlexibility=rbMustTravel" +
                            "&UcFlightSelection%24txtSelOri=" + origin + //CHANGE
                            "&UcFlightSelection%24txtOri=" + cityDep + "%20(" + origin + ")" + //CHANGE
                            "&UcFlightSelection%24ddlDepMonth=" + depdate.ToString("MMM") + "%20" + depdate.Year +
                            "&UcFlightSelection%24ddlDepDay=" + depdate.Day + //CHANGE
                            "&UcFlightSelection%24ddlADTCount=" + adultCount + //CHANGE
                            "&UcFlightSelection%24txtSelDes=" + dest + //CHANGE
                            "&UcFlightSelection%24txtDes=" + cityArr + "%20(" + dest + ")" + //CHANGE
                            "&UcFlightSelection%24ddlCNNCount=" + childCount + //CHANGE
                            "&UcFlightSelection%24ddlINFCount=" + infantCount + //CHANGE
                            "&UcFlightSelection%24txtDepartureDate=" + depdate.ToString("dd") + "%20" + depdate.ToString("MMM") +
                            "%20" + depdate.Year + //CHANGE
                            "&UcFlightSelection%24txtReturnDate=" + depdate.ToString("dd") + "%20" + depdate.ToString("MMM") +
                            "%20" + depdate.Year + //CHANGE
                            "&txtOBNNCellID=" + string.Join("|", priceCollections.ToArray()) +
                            "&txtIBNNCellID=oneway" +
                            "&txtOBNNRowID=" + txt_OBNNRowID +
                            "&txtIBNNRowID=" +
                            "&txtUserSelectedOneway=" +
                            "&__ASYNCPOST=true&";

                        var b =
                            "&txtUpdateInsurance=no" +
                            "&Insurance%24rblInsurance=No" +
                            "&Insurance%24txtInsPostbackRequired=no" +
                            "&txtPricingResponse=OK" + "" +
                            "&txtOutFBCsUsed=" + seat +
                            "&txtInFBCsUsed=" +
                            "&txtTaxBreakdown=" +
                            "&lbContinue.x=39&lbContinue.y=11" +
                            "&UcFlightSelection%24TripType=rbOneWay" + "" +
                            "&UcFlightSelection%24DateFlexibility=rbMustTravel" +
                            "&UcFlightSelection%24txtSelOri=" + origin + //CHANGE
                            "&UcFlightSelection%24txtOri=" + cityDep + "%20(" + origin + ")" + //CHANGE
                            "&UcFlightSelection%24ddlDepMonth=" + depdate.ToString("MMM") + "%20" + depdate.Year +
                            "&UcFlightSelection%24ddlDepDay=" + depdate.Day + //CHANGE
                            "&UcFlightSelection%24ddlADTCount=" + adultCount + //CHANGE
                            "&UcFlightSelection%24txtSelDes=" + dest + //CHANGE
                            "&UcFlightSelection%24txtDes=" + cityArr + "%20(" + dest + ")" + //CHANGE
                            "&UcFlightSelection%24ddlCNNCount=" + childCount + //CHANGE
                            "&UcFlightSelection%24ddlINFCount=" + infantCount + //CHANGE
                            "&UcFlightSelection%24txtDepartureDate=" + depdate.ToString("dd") + "%20" + depdate.ToString("MMM") +
                            "%20" + depdate.Year + //CHANGE
                            "&UcFlightSelection%24txtReturnDate=" + depdate.ToString("dd") + "%20" + depdate.ToString("MMM") +
                            "%20" + depdate.Year + //CHANGE
                            "&txtOBNNCellID=" + string.Join("|", priceCollections.ToArray()) +
                            "&txtIBNNCellID=oneway" +
                            "&txtOBNNRowID=" + txt_OBNNRowID +
                            "&txtIBNNRowID=" +
                            "&txtUserSelectedOneway=";

                        // POST BUAT DAPETIN HARGA
                        const string url6 = @"LionAirAgentsIBE/Step2Availability.aspx";
                        var searchRequest6 = new RestRequest(url6, Method.POST);
                        searchRequest6.AddHeader("Accept-Encoding", "gzip, deflate");
                        searchRequest6.AddHeader("Accept", "*///*");
                        searchRequest6.AddHeader("Referer",
                            "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                        searchRequest6.AddHeader("Origin", "https://agent.lionair.co.id");
                        searchRequest6.AddHeader("Cache-Control", "no-cache");
                        searchRequest6.AddHeader("X-Requested-With", "XMLHttpRequest");
                        searchRequest6.AddHeader("X-MicrosoftAjax", "Delta=true");
                        var vs5 = HttpUtility.UrlEncode(pageFlight["#__VIEWSTATE"].Attr("value"));
                        var postData6 = garbled + vs5 + colpost + data;
                        searchRequest6.AddParameter("application/x-www-form-urlencoded", postData6,
                            ParameterType.RequestBody);
                        //Thread.Sleep(1000);
                        var searchResponse6 = client.Execute(searchRequest6);
                        if ((searchResponse6.ResponseUri != null && searchResponse6.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/Step2Availability.aspx") ||
                    (searchResponse6.StatusCode != HttpStatusCode.OK &&
                     searchResponse6.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse6.StatusCode != HttpStatusCode.Found))
                        {

                            return new BookFlightResult
                            {
                                IsSuccess = false,
                                Status = new BookingStatusInfo
                                {
                                    BookingStatus = BookingStatus.Failed
                                },
                                Errors = new List<FlightError> { FlightError.TechnicalError },
                                ErrorMessages = new List<string> { "[Lion Air] Error searchRequest6 in getting price at LionAirAgentsIBE/Step2Availability.aspx.Unexpected absolute path response or status code." }
                            };
                        }
                        var html6 = searchResponse6.Content;
                        var pagePrice = (CQ)html6;
                        var revalidateFare = pagePrice["#tdAmtTotal"].Text();
                        var priceText = pagePrice.Text();
                        var startvs = priceText.IndexOf("__VIEWSTATE");
                        var xyz = priceText.SubstringBetween(startvs + 12, priceText.Length);
                        var myvs = HttpUtility.UrlEncode(xyz.Split('|')[0]);
                        var agentprice = decimal.Parse(revalidateFare.Replace(",", ""));
                        var agentcurr = pagePrice["#tdCurrTotal"].Text();

                        // POST SEBELUM PAGE PASSENGER
                        const string url7 = @"LionAirAgentsIBE/Step2Availability.aspx";
                        var searchRequest7 = new RestRequest(url7, Method.POST);
                        searchRequest7.AddHeader("Accept-Encoding", "gzip, deflate");
                        searchRequest7.AddHeader("Accept",
                            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*///*;q=0.8");
                        searchRequest7.AddHeader("Referer",
                            "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                        searchRequest7.AddHeader("Origin", "https://agent.lionair.co.id");
                        searchRequest7.AddHeader("Cache-Control", "max-age=0");
                        var postData7 = colpost.SubstringBetween(1, colpost.Length) + b +
                                        "&__EVENTTARGET=&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE=" + myvs;
                        searchRequest7.AddParameter("application/x-www-form-urlencoded", postData7,
                            ParameterType.RequestBody);
                        //Thread.Sleep(1000);
                        var searchResponse7 = client.Execute(searchRequest7);
                        if ((searchResponse7.ResponseUri != null && searchResponse7.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/Step2Availability.aspx") ||
                    (searchResponse7.StatusCode != HttpStatusCode.OK &&
                     searchResponse7.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse7.StatusCode != HttpStatusCode.Found))
                        {
                            LogOut(cid, client);
                            TurnInUsername(userName);
                            return new BookFlightResult
                            {
                                IsSuccess = false,
                                Status = new BookingStatusInfo
                                {
                                    BookingStatus = BookingStatus.Failed
                                },
                                Errors = new List<FlightError> { FlightError.TechnicalError },
                                ErrorMessages = new List<string> { "[Lion Air] Error searchRequest7 in requesting before page Passenger at LionAirAgentsIBE/Step2Availability.aspx.Unexpected absolute path response or status code." }
                            };
                        }
                        tempSearchResponse = searchResponse7;
                        // GET PAGE DATA Passenger
                        const string url8 = @"/LionAirAgentsIBE/OnlineBooking.aspx";
                        var searchRequest8 = new RestRequest(url8, Method.GET);
                        searchRequest8.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                        searchRequest8.AddHeader("Content-Encoding", "gzip");
                        searchRequest8.AddHeader("Host", "agent.lionair.co.id");
                        searchRequest8.AddHeader("Accept",
                            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                        searchRequest8.AddHeader("Referer",
                            "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                        //Thread.Sleep(1000);
                        var searchResponse8 = client.Execute(searchRequest8);
                        if ((searchResponse8.ResponseUri != null && searchResponse8.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/OnlineBooking.aspx") ||
                    (searchResponse8.StatusCode != HttpStatusCode.OK &&
                     searchResponse8.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse8.StatusCode != HttpStatusCode.Found))
                        {
                            LogOut(cid, client);
                            TurnInUsername(userName);
                            return new BookFlightResult
                            {
                                IsSuccess = false,
                                Status = new BookingStatusInfo
                                {
                                    BookingStatus = BookingStatus.Failed
                                },
                                Errors = new List<FlightError> { FlightError.TechnicalError },
                                ErrorMessages = new List<string> { "[Lion Air] Error in searchRequest8 Getting Page Data Passenger at /LionAirAgentsIBE/OnlineBooking.aspx.Unexpected absolute path response or status code." }
                            };
                        }
                        var html8 = searchResponse8.Content;

                        var pageBooking = (CQ)html8;
                        var vs9 = HttpUtility.UrlEncode(pageBooking["#__VIEWSTATE"].Attr("value"));
                        var beginning = "__EVENTTARGET=btnContinue&__EVENTARGUMENT=&__VIEWSTATE=" + vs9;// + "&__VIEWSTATEGENERATOR=B80F8107";
                        const string ending =
                            "&txtRemark=&payDet=rbPay_HOLD&CreditCardDisplay1%24CreditCardType=VI&CreditCardDisplay1%24txtCardHolderName=&CreditCardDisplay1%24CreditCardNumber=&CreditCardDisplay1%24CreditCardExpiryMonth=MM&CreditCardDisplay1%24CreditCardExpiryYear=YY&CreditCardDisplay1%24CVVNumber=&AcceptFareConditions=on&FlightInfo=&AXTotal=&DCTotal=&OtherTotal=&nameMismatch=";

                        //NEW ITINERARY TAKEN FROM PASSENGER DATA PAGE

                        var flightTableRows = pageBooking["#FlightTable > tbody"].Children().ToList();
                        var itinRows = flightTableRows.GetRange(1, flightTableRows.Count - 2);
                        var newSegments = new List<FlightSegment>();
                        foreach (var row in itinRows)
                        {
                            //Column 0
                            var column0 = row.ChildElements.ToList()[0];
                            var flightNo = column0.ChildElements.ToList()[0].InnerText;
                            string airplaneCode = column0.ChildElements.ToList().Count == 1 ? null : column0.ChildElements.ToList()[1].InnerText;

                            const string format1 = "dd MMM";

                            //Column 1
                            var column1 = row.ChildElements.ToList()[1];
                            var insideText1 = column1.InnerText.Split(')');
                            var departureAirport =
                                insideText1[insideText1.Length - 2].
                                    SubstringBetween(insideText1[insideText1.Length - 2].Length - 3,
                                        insideText1[insideText1.Length - 2].Length);
                            var departureHour = insideText1.Last().SubstringBetween(0, 5);
                            var departureDate = column1.ChildElements.ToList()[1].InnerText;
                            var dateDeparture = DateTime.SpecifyKind(DateTime.ParseExact(departureDate, format1, provider), DateTimeKind.Utc);
                            if (dateDeparture < DateTime.Now)
                            {
                                dateDeparture = DateTime.SpecifyKind(new DateTime(DateTime.Now.AddYears(1).Year, dateDeparture.Month, dateDeparture.Day,
                                    Convert.ToInt32(departureHour.Split(':')[0]), Convert.ToInt32(departureHour.Split(':')[1]), 0), DateTimeKind.Utc);
                            }
                            else
                            {
                                dateDeparture = DateTime.SpecifyKind(new DateTime(DateTime.Now.Year, dateDeparture.Month, dateDeparture.Day,
                                     Convert.ToInt32(departureHour.Split(':')[0]), Convert.ToInt32(departureHour.Split(':')[1]), 0), DateTimeKind.Utc);
                            }

                            //Column 2
                            var column2 = row.ChildElements.ToList()[2];
                            var insideText2 = column2.InnerText.Split(')');
                            var arrivalAirport = insideText2[insideText2.Length - 2].
                               SubstringBetween(insideText2[insideText2.Length - 2].Length - 3,
                               insideText2[insideText2.Length - 2].Length);
                            var arrivalHour = insideText2.Last().SubstringBetween(0, 5);
                            var arrivalDate = column2.ChildElements.ToList()[1].InnerText;
                            var dateArrival = DateTime.SpecifyKind(DateTime.ParseExact(arrivalDate, format1, provider), DateTimeKind.Utc);
                            if (dateArrival < DateTime.Now)
                            {
                                dateArrival = DateTime.SpecifyKind(new DateTime(DateTime.Now.AddYears(1).Year, dateArrival.Month, dateArrival.Day,
                                    Convert.ToInt32(arrivalHour.Split(':')[0]), Convert.ToInt32(arrivalHour.Split(':')[1]), 0), DateTimeKind.Utc);
                            }
                            else
                            {
                                dateArrival = DateTime.SpecifyKind(new DateTime(DateTime.Now.Year, dateArrival.Month, dateArrival.Day,
                                     Convert.ToInt32(arrivalHour.Split(':')[0]), Convert.ToInt32(arrivalHour.Split(':')[1]), 0), DateTimeKind.Utc);
                            }

                            //Column 3

                            var column3 = row.ChildElements.ToList()[3];
                            var column3Text = column3.InnerText.Split(new string[] { "stops", "stop" }, StringSplitOptions.RemoveEmptyEntries);
                            var stopsno = column3Text[0];
                            var dur = column3Text[1].Split(' ');
                            TimeSpan duration;
                            if (dur.Length != 1)
                            {
                                duration = new TimeSpan(
                                Convert.ToInt32(dur[0].SubstringBetween(0, dur[0].Length - 1)),
                                Convert.ToInt32(dur[1].SubstringBetween(0, dur[1].Length - 1)), 0);
                            }
                            else
                            {
                                duration = new TimeSpan(0,
                                Convert.ToInt32(dur[0].SubstringBetween(0, dur[0].Length - 1)), 0);
                            }

                            listFlightNo.Add(flightNo);
                            listDepHr.Add(departureHour);

                            newSegments.Add(new FlightSegment
                            {
                                AircraftCode = airplaneCode,
                                AirlineCode = flightNo.Split()[0],
                                ArrivalAirport = arrivalAirport,
                                ArrivalTime = dateArrival,
                                CabinClass = cabinClass,
                                DepartureAirport = departureAirport,
                                DepartureTime = dateDeparture,
                                Duration = duration,
                                FlightNumber = flightNo.Split()[1],
                                OperatingAirlineCode = flightNo.Split()[0],
                                StopQuantity = Convert.ToInt32(stopsno),
                                IsMealIncluded = flightNo.Split()[0] == "ID",
                                IsPscIncluded = true
                            });

                        }

                        var depHrJoin = String.Join("|", listDepHr.ToArray());
                        var flightNoJoin = String.Join("|", listFlightNo.ToArray());
                        var isInternational = CheckInternationality(newSegments);

                        itin = new FlightItinerary
                        {
                            AdultCount = adultCount,
                            ChildCount = childCount,
                            InfantCount = infantCount,
                            CanHold = true,
                            FareType = FareType.Published,
                            RequireBirthDate = isInternational,
                            RequirePassport = isInternational,
                            RequireSameCheckIn = false,
                            RequireNationality = isInternational,
                            RequestedCabinClass = CabinClass.Economy,
                            RequestedTripType = bookInfo.Itinerary.RequestedTripType,
                            TripType = TripType.OneWay,
                            Supplier = Supplier.LionAir,
                            Price = new Price(),
                            AdultPricePortion = bookInfo.Itinerary.AdultPricePortion,
                            ChildPricePortion = bookInfo.Itinerary.ChildPricePortion,
                            InfantPricePortion = bookInfo.Itinerary.InfantPricePortion,
                            FareId = bookInfo.Itinerary.FareId,
                            Trips = new List<FlightTrip>
                            {
                                new FlightTrip
                                {
                                    OriginAirport = origin,
                                    DestinationAirport = dest,
                                    DepartureDate = depdate.Date,
                                    Segments = newSegments
                                }
                            }
                        };

                        Currency currclass;
                        var currencyList = Currency.GetAllCurrencies(Payment.Constant.Supplier.LionAir);
                        if (!currencyList.TryGetValue(agentcurr, out currclass))
                        {
                            LogOut(cid, client);
                            TurnInUsername(userName);
                            return new BookFlightResult
                            {
                                IsSuccess = false,
                                Errors = new List<FlightError> { FlightError.TechnicalError },
                                Status = null,
                                ErrorMessages = new List<string> { "[Lion Air] Currency is not available" },
                                IsValid = true,
                                NewItinerary = itin,

                            };
                        }
                        itin.Price.SetSupplier(agentprice, new Currency(agentcurr, Payment.Constant.Supplier.LionAir));

                        var isItinChanged = !itin.Identical(bookInfo.Itinerary);

                        //Cek Jika Itinerary berubah
                        if (isItinChanged)
                        {
                            LogOut(cid, client);
                            TurnInUsername(userName);

                            return new BookFlightResult
                            {
                                IsSuccess = false,
                                //Errors = new List<FlightError> { FlightError.TechnicalError },
                                Status = null,
                                ErrorMessages = new List<string> { "[Lion Air] Itinerary is Changed!" },
                                NewPrice = agentprice,
                                IsValid = true,
                                IsPriceChanged = agentprice != bookInfo.Itinerary.Price.Supplier,
                                NewItinerary = itin,
                                IsItineraryChanged = isItinChanged

                            };
                        }

                        var harga2 = agentprice;
                        var orderedPassengers = bookInfo.Passengers.OrderBy(x => x.Type).ToList();
                        var dataPassenger = "";

                        if (originCountry == destinationCountry)
                        {
                            var infpax = 1;
                            for (var i = 0; i < orderedPassengers.Count; i++)
                            {
                                var title = "";
                                string mealrequest;
                                switch (orderedPassengers[i].Title)
                                {
                                    case Title.Miss:
                                        title = "Miss";
                                        break;
                                    case Title.Mister:
                                        title = orderedPassengers.ElementAt(i).Type == PaxType.Adult ? "Mr" : "Mstr";
                                        break;
                                    case Title.Mistress:
                                        title = "Mrs";
                                        break;
                                }


                                switch (orderedPassengers[i].Type)
                                {
                                    case PaxType.Adult:
                                        mealrequest = "No+Preference";
                                        dataPassenger +=
                                            "&NameBlock" + (i + 1) + "%24ddlTitle=" + title +
                                            "&NameBlock" + (i + 1) + "%24txtFirstName=" +
                                            String.Join("+", orderedPassengers[i].FirstName.Split(' ')) +
                                            "&NameBlock" + (i + 1) + "%24txtLastName=" +
                                            orderedPassengers[i].LastName +
                                            "&NameBlock" + (i + 1) + "%24ddlAirline=JT" + //codeFlight + //NOT YET
                                            "&NameBlock" + (i + 1) + "%24ddlSpecRequest=NA" +
                                            "&NameBlock" + (i + 1) + "%24txtFFNo=" +
                                            "&NameBlock" + (i + 1) + "%24ddlMealRequest=" + mealrequest;
                                        break;
                                    case PaxType.Child:
                                        mealrequest = "No+Preference";
                                        dataPassenger +=
                                            "&NameBlock" + (i + 1) + "%24ddlTitle=" + title +
                                            "&NameBlock" + (i + 1) + "%24txtFirstName=" +
                                            String.Join("+", orderedPassengers[i].FirstName.Split(' ')) +
                                            "&NameBlock" + (i + 1) + "%24txtLastName=" +
                                            orderedPassengers[i].LastName +
                                            "&NameBlock" + (i + 1) + "%24ddlAirline=JT" +// codeFlight + //NOT YET
                                            "&NameBlock" + (i + 1) + "%24ddlSpecRequest=NA" +
                                            "&NameBlock" + (i + 1) + "%24txtFFNo=" +
                                            "&NameBlock" + (i + 1) + "%24ddlDOBDay=" +
                                            orderedPassengers[i].DateOfBirth.GetValueOrDefault().ToString("dd") +
                                            "&NameBlock" + (i + 1) + "%24ddlDOBMonth=" +
                                            orderedPassengers[i].DateOfBirth.GetValueOrDefault().ToString("MMM") +
                                            "&NameBlock" + (i + 1) + "%24ddlDOBYear=" +
                                            orderedPassengers[i].DateOfBirth.GetValueOrDefault().Year +
                                            "&NameBlock" + (i + 1) + "%24ddlMealRequest=" + mealrequest;
                                        break;
                                    case PaxType.Infant:
                                        mealrequest = "BBML";
                                        dataPassenger +=
                                            "&NameBlock" + (i + 1) + "%24ddlTitle=" + title +
                                            "&NameBlock" + (i + 1) + "%24txtFirstName=" +
                                            String.Join("+", orderedPassengers[i].FirstName.Split(' ')) +
                                            "&NameBlock" + (i + 1) + "%24txtLastName=" +
                                            orderedPassengers[i].LastName +
                                            "&NameBlock" + (i + 1) + "%24ddlINFPaxAssoc=" + infpax +
                                            "&NameBlock" + (i + 1) + "%24ddlSpecRequest=NA" +
                                            "&NameBlock" + (i + 1) + "%24txtFFNo=" +
                                            "&NameBlock" + (i + 1) + "%24ddlDOBDay=" +
                                            orderedPassengers[i].DateOfBirth.GetValueOrDefault().ToString("dd") +
                                            "&NameBlock" + (i + 1) + "%24ddlDOBMonth=" +
                                            orderedPassengers[i].DateOfBirth.GetValueOrDefault().ToString("MMM") +
                                            "&NameBlock" + (i + 1) + "%24ddlDOBYear=" +
                                            orderedPassengers[i].DateOfBirth.GetValueOrDefault().Year +
                                            "&NameBlock" + (i + 1) + "$ddlMealRequest=" + mealrequest;
                                        infpax += 1;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            var infpax = 1;
                            for (var i = 0; i < orderedPassengers.Count; i++)
                            {
                                var gender = "";
                                var title = "";
                                string mealrequest;
                                switch (orderedPassengers[i].Gender)
                                {
                                    case Gender.Male:
                                        gender = "M";
                                        break;
                                    case Gender.Female:
                                        gender = "F";
                                        break;
                                }

                                switch (orderedPassengers[i].Title)
                                {
                                    case Title.Miss:
                                        title = "Miss";
                                        break;
                                    case Title.Mister:
                                        title = orderedPassengers.ElementAt(i).Type == PaxType.Adult ? "Mr" : "Mstr";
                                        break;
                                    case Title.Mistress:
                                        title = "Mrs";
                                        break;
                                }

                                if (orderedPassengers.ElementAt(i).Type == PaxType.Infant)
                                {
                                    mealrequest = "BBML";
                                    dataPassenger +=
                                        "&NameBlock" + (i + 1) + "%24ddlTitle=" + title +
                                        "&NameBlock" + (i + 1) + "%24txtFirstName=" +
                                        String.Join("+", orderedPassengers[i].FirstName.Split(' ')) +
                                        "&NameBlock" + (i + 1) + "%24txtLastName=" +
                                        orderedPassengers[i].LastName +
                                        "&NameBlock" + (i + 1) + "%24ddlGender=" + gender +
                                        "&NameBlock" + (i + 1) + "%24ddlINFPaxAssoc=" + infpax +
                                        "&NameBlock" + (i + 1) + "%24ddlSpecRequest=NA" +
                                        "&NameBlock" + (i + 1) + "%24ddlDOBDay=" +
                                        orderedPassengers[i].DateOfBirth.GetValueOrDefault().ToString("dd") +
                                        "&NameBlock" + (i + 1) + "%24ddlDOBMonth=" +
                                        orderedPassengers[i].DateOfBirth.GetValueOrDefault().ToString("MMM") +
                                        "&NameBlock" + (i + 1) + "%24ddlDOBYear=" +
                                        orderedPassengers[i].DateOfBirth.GetValueOrDefault().Year +
                                        "&NameBlock" + (i + 1) + "%24ddlMealRequest=" + mealrequest +
                                        "&NameBlock" + (i + 1) + "%24txtPassportNumber=" +
                                        orderedPassengers[i].PassportNumber +
                                        "&NameBlock" + (i + 1) + "%24ddlPassportExpDay=" +
                                        orderedPassengers[i].PassportExpiryDate.GetValueOrDefault().ToString("dd") +
                                        "&NameBlock" + (i + 1) + "%24ddlPassportExpMon=" +
                                        orderedPassengers[i].PassportExpiryDate.GetValueOrDefault().ToString("MMM") +
                                        "&NameBlock" + (i + 1) + "%24ddlPassportExpYear=" +
                                        orderedPassengers[i].PassportExpiryDate.GetValueOrDefault().Year +
                                        "&NameBlock" + (i + 1) + "%24ddlDocCountry=" +
                                        orderedPassengers[i].PassportCountry +
                                        "&NameBlock" + (i + 1) + "%24ddlPaxCountry=" +
                                        orderedPassengers[i].Nationality; //orderedPassengers[i].Nationality
                                    infpax += 1;
                                }
                                else
                                {
                                    mealrequest = "No+Preference";
                                    dataPassenger +=
                                        "&NameBlock" + (i + 1) + "%24ddlTitle=" + title +
                                        "&NameBlock" + (i + 1) + "%24txtFirstName=" +
                                        String.Join("+", orderedPassengers[i].FirstName) +
                                        "&NameBlock" + (i + 1) + "%24txtLastName=" +
                                        orderedPassengers[i].LastName +
                                        "&NameBlock" + (i + 1) + "%24ddlGender=" + gender +
                                        "&NameBlock" + (i + 1) + "%24ddlAirline=JT" + //codeFlight +
                                        "&NameBlock" + (i + 1) + "%24ddlSpecRequest=NA" +
                                        "&NameBlock" + (i + 1) + "%24txtFFNo=" +
                                        "&NameBlock" + (i + 1) + "%24ddlDOBDay=" +
                                        orderedPassengers[i].DateOfBirth.GetValueOrDefault().ToString("dd") +
                                        "&NameBlock" + (i + 1) + "%24ddlDOBMonth=" +
                                        orderedPassengers[i]
                                            .DateOfBirth.GetValueOrDefault()
                                            .ToString("MMM") +
                                        "&NameBlock" + (i + 1) + "%24ddlDOBYear=" +
                                        orderedPassengers[i].DateOfBirth.GetValueOrDefault().Year +
                                        "&NameBlock" + (i + 1) + "%24ddlMealRequest=" + mealrequest +
                                        "&NameBlock" + (i + 1) + "%24txtPassportNumber=" +
                                        orderedPassengers[i].PassportNumber +
                                        "&NameBlock" + (i + 1) + "%24ddlPassportExpDay=" +
                                        orderedPassengers[i]
                                            .PassportExpiryDate.GetValueOrDefault()
                                            .ToString("dd") +
                                        "&NameBlock" + (i + 1) + "%24ddlPassportExpMon=" +
                                        orderedPassengers[i]
                                            .PassportExpiryDate.GetValueOrDefault()
                                            .ToString("MMM") +
                                        "&NameBlock" + (i + 1) + "%24ddlPassportExpYear=" +
                                        orderedPassengers[i].PassportExpiryDate.GetValueOrDefault().Year +
                                        "&NameBlock" + (i + 1) + "%24ddlDocCountry=" +
                                        orderedPassengers[i].PassportCountry +
                                        "&NameBlock" + (i + 1) + "%24ddlPaxCountry=" +
                                        orderedPassengers[i].Nationality;
                                }
                            }
                        }

                        var middle = "";
                        for (var y = 0; y < adultCount + childCount; y++)
                        {
                            for (var x = 0; x < newSegments.Count; x++)
                            {
                                middle += "&Bag_" + newSegments.ElementAt(x).DepartureAirport + "%24"
                                    + newSegments.ElementAt(x).ArrivalAirport + "%24" + x + "%24" + y + "=";
                            }
                        }

                        var middle1 = "&hdnBagSelection=";
                        for (var x = 0; x < adultCount + childCount; x++)
                        {
                            middle1 += "%2C%7C";
                        }
                        middle1 += "&hdnDepPorts=&hdnArrPorts=";

                        var c = bookInfo.Contact.Name.Trim();
                        string contactFirstname, contactLastname;
                        var contactTitle = "";
                        if (c.Split(' ').Count() == 1)
                        {
                            contactFirstname = c;
                            contactLastname = c;
                        }
                        else
                        {
                            contactLastname = c.Split(' ').Last();
                            contactFirstname = String.Join("+", c.Split(' ').ToList().Take(c.Split(' ').Length - 1));
                        }

                        var cntct =
                            "&ContactTitle=Mr" +
                            "&ContactFirstName=" + contactFirstname +
                            "&ContactLastName=" + contactLastname +
                            "&txtAddress1=" +
                            "&txtAddress2=" +
                            "&ddlCountry=ID" + //bookInfo.ContactData.CountryCode + ///change?///
                            "&txtCity=" +
                            "&txtPostCode=" +
                            "&txtCountryCode1=" + bookInfo.Contact.CountryCallingCode +
                            "&txtAreaCode1=" +
                            "&txtPhoneNumber1=" + bookInfo.Contact.Phone +
                            "&ddlOriNumber=M" +
                            "&txtCountryCode3=" +
                            "&txtPhoneNumber3=" +
                            "&txtEmailAddress1=" + HttpUtility.UrlEncode("dwi.agustina@travelmadezy.com") + //email agent
                            "&txtEmailAddress2=" + HttpUtility.UrlEncode("dwi.agustina@travelmadezy.com");

                        Thread.Sleep(1000);
                        //var postDataBooking = beginning + dataPassenger + middle + middle1 + cntct + ending;
                        var postDataBooking = beginning + dataPassenger + cntct;

                        const string urlBooking = @"LionAirAgentsIBE/CustomerContact.aspx";
                        var searchRequestBooking = new RestRequest(urlBooking, Method.POST);
                        searchRequestBooking.AddHeader("Accept-Encoding", "gzip, deflate");
                        searchRequestBooking.AddHeader("Accept",
                            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                        searchRequestBooking.AddHeader("Referer",
                            "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                        searchRequestBooking.AddHeader("Origin", "https://agent.lionair.co.id");
                        searchRequestBooking.AddHeader("Cache-Control", "max-age=0");
                        searchRequestBooking.AddParameter("application/x-www-form-urlencoded", postDataBooking,
                            ParameterType.RequestBody);
                        Thread.Sleep(3000);
                        var searchResponseBooking = client.Execute(searchRequestBooking);
                        string newPrice;

                        var responseBookingHtml = (CQ) searchResponseBooking.Content;
                        var vs10 = HttpUtility.UrlEncode(responseBookingHtml["#__VIEWSTATE"].Attr("value"));
                        var postDataAddOns =
                            "__EVENTTARGET=btnContinue&" +
                            "__EVENTARGUMENT=" +
                            "__VIEWSTATE=" + vs10 +
                            middle + middle1;

                        const string urlAddOns = @"LionAirAgentsIBE/AddOns.aspx";
                        var searchRequestAddOns = new RestRequest(urlAddOns, Method.POST);
                        searchRequestAddOns.AddHeader("Accept-Encoding", "gzip, deflate");
                        searchRequestAddOns.AddHeader("Accept",
                            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                        searchRequestAddOns.AddHeader("Referer",
                            "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                        searchRequestAddOns.AddHeader("Origin", "https://agent.lionair.co.id");
                        searchRequestAddOns.AddHeader("Cache-Control", "max-age=0");
                        searchRequestAddOns.AddParameter("application/x-www-form-urlencoded", postDataAddOns,
                            ParameterType.RequestBody);
                        Thread.Sleep(3000);
                        var searchResponseAddOns = client.Execute(searchRequestAddOns);

                        var responseAddOnsHtml = (CQ)searchResponseAddOns.Content;
                        var vs11 = HttpUtility.UrlEncode(responseAddOnsHtml["#__VIEWSTATE"].Attr("value"));
                        var postDataPayment =
                            "__EVENTTARGET=lbContinue&" +
                            "__EVENTARGUMENT=" +
                            "__VIEWSTATE=" + vs11 +
                            ending;

                        const string urlPayment = @"LionAirAgentsIBE/Payment.aspx";
                        var searchRequestPayment = new RestRequest(urlPayment, Method.POST);
                        searchRequestPayment.AddHeader("Accept-Encoding", "gzip, deflate");
                        searchRequestPayment.AddHeader("Accept",
                            "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                        searchRequestPayment.AddHeader("Referer",
                            "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                        searchRequestPayment.AddHeader("Origin", "https://agent.lionair.co.id");
                        searchRequestPayment.AddHeader("Cache-Control", "max-age=0");
                        searchRequestPayment.AddParameter("application/x-www-form-urlencoded", postDataPayment,
                            ParameterType.RequestBody);
                        Thread.Sleep(3000);
                        var searchResponsePayment = client.Execute(searchRequestPayment);

                        //HARGA BARU DI UJUNG
                        try
                        {
                            var htmlBookingResult = (CQ)searchResponseBooking.Content;
                            newPrice = htmlBookingResult["#lblTotalFares"].Text().Replace(",", "");
                            var vsPriceAcceptance =
                                HttpUtility.UrlEncode(htmlBookingResult["#__VIEWSTATE"].Attr("value"));

                            //Kondisi dimana ada perubahan harga namun udah sama dengan harga kita
                            if (newPrice.Length != 0)
                            {
                                if (decimal.Parse(newPrice) != bookInfo.Itinerary.Price.Supplier)
                                {
                                    LogOut(cid, client);
                                    TurnInUsername(userName);

                                    var newFareId = origin + "+" + dest + "+" + splittedFareId[2] + "+" + adultCount +
                                                    "+" +
                                                    childCount + "+" + infantCount + "+" + splittedFareId[6] + "+" +
                                                    newPrice + "+" +
                                                    flightId + "+" + newSegments.Count + "+" + flightNoJoin + "+" +
                                                    depHrJoin;

                                    itin.Price.SetSupplier(decimal.Parse(newPrice), new Currency("IDR"
                                        , Payment.Constant.Supplier.LionAir));
                                    itin.FareId = newFareId;

                                    return new BookFlightResult
                                    {
                                        IsSuccess = false,
                                        //Errors = new List<FlightError> { FlightError.TechnicalError },
                                        Status = null,
                                        ErrorMessages = new List<string> { "[Lion Air] Price is changed!" },
                                        NewPrice = decimal.Parse(newPrice),
                                        IsValid = true,
                                        IsPriceChanged = true,
                                        NewItinerary = itin,
                                        IsItineraryChanged = isItinChanged

                                    };
                                }
                                if (decimal.Parse(newPrice) == bookInfo.Itinerary.Price.Supplier)
                                {
                                    var postDataPriceAcceptance = "__EVENTTARGET=lbContinue&__EVENTARGUMENT=&__VIEWSTATE="
                                                                  + vsPriceAcceptance;
                                    const string urlPriceAcceptance = @"LionAirAgentsIBE/PriceAcceptance.aspx";
                                    var searchRequestPriceAcceptance = new RestRequest(urlPriceAcceptance, Method.POST);
                                    searchRequestPriceAcceptance.AddHeader("Accept-Encoding", "gzip, deflate");
                                    searchRequestPriceAcceptance.AddHeader("Accept",
                                        "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                                    searchRequestPriceAcceptance.AddHeader("Referer",
                                        "https://agent.lionair.co.id/LionAirAgentsIBE/Step3.aspx");
                                    searchRequestPriceAcceptance.AddHeader("Origin", "https://agent.lionair.co.id");
                                    searchRequestPriceAcceptance.AddHeader("Cache-Control", "max-age=0");
                                    searchRequestPriceAcceptance.AddParameter
                                        ("application/x-www-form-urlencoded", postDataPriceAcceptance,
                                            ParameterType.RequestBody);
                                    Thread.Sleep(3000);
                                    var searchResponsPriceAcceptance = client.Execute(searchRequestPriceAcceptance);
                                    tempSearchResponse = searchResponsPriceAcceptance;
                                }
                            }
                            // kalau harga setelah dipilih di page yg banyak radio button beda dengan yg pertama
                            // kali di search

                            else if (bookInfo.Itinerary.Price.Supplier != harga2)
                            {
                                var newFareId = origin + "+" + dest + "+" + splittedFareId[2] + "+" + adultCount
                                    + "+" + childCount + "+" + infantCount + "+" + splittedFareId[6] + "+" + harga2
                                    + "+" + flightId + "+" + newSegments.Count + "+" + flightNoJoin + "+" + depHrJoin;

                                itin.Price.SetSupplier(harga2, new Currency("IDR",
                                    Payment.Constant.Supplier.LionAir));
                                itin.FareId = newFareId;

                                LogOut(cid, client);
                                TurnInUsername(userName);

                                //newPrice itu harga nya null, padahal dicoba untuk parse
                                return new BookFlightResult
                                {
                                    IsSuccess = false,
                                    //Errors = new List<FlightError> { FlightError.TechnicalError },
                                    Status = null,
                                    ErrorMessages = new List<string> { "[Lion Air] Price is changed!2" },
                                    NewPrice = harga2,
                                    IsValid = true,
                                    IsPriceChanged = true,
                                    NewItinerary = itin,
                                    IsItineraryChanged = isItinChanged

                                };
                            }
                        }
                        catch
                        {
                            LogOut(cid, client);
                            TurnInUsername(userName);
                            return new BookFlightResult
                            {
                                IsSuccess = false,
                                Errors = new List<FlightError> { FlightError.TechnicalError },
                                ErrorMessages = new List<string> { "[Lion Air] Error di bagian validasi harga di halaman akhir booking" },
                                Status = new BookingStatusInfo
                                {
                                    BookingStatus = BookingStatus.Failed
                                }
                            };
                        }

                        //TODO Batas Test Booking
                        if (bookInfo.Test)
                        {
                            return new BookFlightResult
                            {
                                IsSuccess = true,
                                Status = new BookingStatusInfo
                                {
                                    BookingStatus = BookingStatus.Booked
                                },
                                IsValid = true,

                            };
                        }

                        if (newPrice.Length == 0)
                        {
                            // Kalau normal2 aja

                            var newFareId = origin + "+" + dest + "+" + splittedFareId[2] + "+" + adultCount +
                                                   "+" + childCount + "+" + infantCount + "+" + splittedFareId[6] +
                                                   "+" + harga2 + "+" + flightId + "+" + newSegments.Count +
                                                   "+" + flightNoJoin + "+" + depHrJoin;

                            itin.Price.SetSupplier(harga2, new Currency("IDR",
                                Payment.Constant.Supplier.LionAir));
                            itin.FareId = newFareId;

                            const string url9 = @"/LionAirAgentsIBE/OnlineBooking.aspx";
                            var searchRequest9 = new RestRequest(url9, Method.GET);
                            searchRequest9.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                            searchRequest9.AddHeader("Content-Encoding", "gzip");
                            searchRequest9.AddHeader("Host", "agent.lionair.co.id");
                            searchRequest9.AddHeader("Accept",
                                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                            searchRequest9.AddHeader("Referer",
                                "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                            Thread.Sleep(3000);
                            var searchResponse9 = client.Execute(searchRequest9);
                            if ((searchResponse9.ResponseUri != null && searchResponse9.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/OnlineBooking.aspx") ||
                    (searchResponse9.StatusCode != HttpStatusCode.OK &&
                     searchResponse9.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse9.StatusCode != HttpStatusCode.Found))
                            {
                                LogOut(cid, client);
                                TurnInUsername(userName);
                                return new BookFlightResult
                                {
                                    IsSuccess = false,
                                    Status = new BookingStatusInfo
                                    {
                                        BookingStatus = BookingStatus.Failed
                                    },
                                    Errors = new List<FlightError> { FlightError.TechnicalError },
                                    ErrorMessages = new List<string> { "[Lion Air] Error in searchRequest9 requesting in request9 at /LionAirAgentsIBE/OnlineBooking.aspx.Unexpected absolute path response or status code." }
                                };
                            }
                            tempSearchResponse = searchResponse9;

                            const string url10 = @"/LionAirAgentsIBE/OnlineBooking.aspx";
                            var searchRequest10 = new RestRequest(url10, Method.GET);
                            searchRequest10.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                            searchRequest10.AddHeader("Content-Encoding", "gzip");
                            searchRequest10.AddHeader("Host", "agent.lionair.co.id");
                            searchRequest10.AddHeader("Accept",
                                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                            searchRequest10.AddHeader("Referer",
                                "https://agent.lionair.co.id/LionAirAgentsIBE/OnlineBooking.aspx");
                            Thread.Sleep(3000);
                            var searchResponse10 = client.Execute(searchRequest10);
                            if ((searchResponse10.ResponseUri != null && searchResponse10.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/OnlineBooking.aspx") ||
                    (searchResponse10.StatusCode != HttpStatusCode.OK &&
                     searchResponse10.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse10.StatusCode != HttpStatusCode.Found))
                            {
                                LogOut(cid, client);
                                TurnInUsername(userName);
                                return new BookFlightResult
                                {
                                    IsSuccess = false,
                                    Status = new BookingStatusInfo
                                    {
                                        BookingStatus = BookingStatus.Failed
                                    },
                                    Errors = new List<FlightError> { FlightError.TechnicalError },
                                    ErrorMessages = new List<string> { "[Lion Air] Error searchRequest10 in requesting in request10 at /LionAirAgentsIBE/OnlineBooking.aspx.Unexpected absolute path response or status code." }
                                };
                            }
                            var html10 = searchResponse10.Content;
                            var bookingData = (CQ)html10;

                            bookingReference = bookingData["#lblRefNumber"].Text();
                            bookingTimeLimit = bookingData["#lblPayByDate"].Text();
                        }
                        else
                        {
                            var newFareId = origin + "+" + dest + "+" + splittedFareId[2] + "+" + adultCount +
                                                   "+" + childCount + "+" + infantCount + "+" + splittedFareId[6] +
                                                   "+" + newPrice + "+" + flightId + "+" + newSegments.Count +
                                                   "+" + flightNoJoin + "+" + depHrJoin;
                            itin.Price.SetSupplier(decimal.Parse(newPrice), new Currency("IDR",
                                Payment.Constant.Supplier.LionAir));
                            itin.FareId = newFareId;

                            // kalau ini booking ke 2 (setelah booking 1, yang harga naik di ujung)
                            const string url9 = @"/LionAirAgentsIBE/OnlineBooking.aspx";
                            var searchRequest9 = new RestRequest(url9, Method.GET);
                            searchRequest9.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                            searchRequest9.AddHeader("Content-Encoding", "gzip");
                            searchRequest9.AddHeader("Host", "agent.lionair.co.id");
                            searchRequest9.AddHeader("Accept",
                                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                            searchRequest9.AddHeader("Referer",
                                "https://agent.lionair.co.id/LionAirAgentsIBE/Step3.aspx");
                            Thread.Sleep(3000);
                            var searchResponse9 = client.Execute(searchRequest9);
                            if ((searchResponse9.ResponseUri != null && searchResponse9.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/OnlineBooking.aspx") ||
                    (searchResponse9.StatusCode != HttpStatusCode.OK &&
                     searchResponse9.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse9.StatusCode != HttpStatusCode.Found))
                            {
                                LogOut(cid, client);
                                TurnInUsername(userName);

                                return new BookFlightResult
                                {
                                    IsSuccess = false,
                                    Status = new BookingStatusInfo
                                    {
                                        BookingStatus = BookingStatus.Failed
                                    },
                                    Errors = new List<FlightError> { FlightError.TechnicalError },
                                    ErrorMessages = new List<string> { "[Lion Air] Error searchRequest9 in requesting in request9 at /LionAirAgentsIBE/OnlineBooking.aspx.Unexpected absolute path response or status code." }
                                };
                            }
                            tempSearchResponse = searchResponse9;
                            const string url10 = @"/LionAirAgentsIBE/OnlineBooking.aspx";
                            var searchRequest10 = new RestRequest(url10, Method.GET);
                            searchRequest10.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                            searchRequest10.AddHeader("Content-Encoding", "gzip");
                            searchRequest10.AddHeader("Host", "agent.lionair.co.id");
                            searchRequest10.AddHeader("Accept",
                                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                            searchRequest10.AddHeader("Referer",
                                "https://agent.lionair.co.id/LionAirAgentsIBE/Step3.aspx");
                            Thread.Sleep(3000);
                            var searchResponse10 = client.Execute(searchRequest10);
                            if ((searchResponse10.ResponseUri != null && searchResponse10.ResponseUri.AbsolutePath != "/LionAirAgentsIBE/OnlineBooking.aspx") ||
                    (searchResponse10.StatusCode != HttpStatusCode.OK &&
                     searchResponse10.StatusCode != HttpStatusCode.Redirect &&
                     searchResponse10.StatusCode != HttpStatusCode.Found))
                            {
                                LogOut(cid, client);
                                TurnInUsername(userName);
                                return new BookFlightResult
                                {
                                    IsSuccess = false,
                                    Status = new BookingStatusInfo
                                    {
                                        BookingStatus = BookingStatus.Failed
                                    },
                                    Errors = new List<FlightError> { FlightError.TechnicalError },
                                    ErrorMessages = new List<string> { "[Lion Air] Error searchRequest10 in requesting in request1 fot getting booking time limeit at /LionAirAgentsIBE/OnlineBooking.aspx.Unexpected absolute path response or status code." }
                                };
                            }
                            var html10 = searchResponse10.Content;
                            var bookingData = (CQ)html10;

                            bookingReference = bookingData["#lblRefNumber"].Text();
                            bookingTimeLimit = bookingData["#lblPayByDate"].Text();
                        }

                        LogOut(cid, client);
                        TurnInUsername(userName);
                    }

                    const string format = "dd MMMM', 'yyyy', 'HH:mm";
                    if (bookingReference.Length != 0)
                    {
                        return new BookFlightResult
                        {
                            IsSuccess = true,
                            Status = new BookingStatusInfo
                            {
                                BookingId = bookingReference,
                                TimeLimit =
                                    DateTime.SpecifyKind(
                                        DateTime.ParseExact(
                                            bookingTimeLimit.SubstringBetween(0, bookingTimeLimit.Length - 3), format,
                                            provider), DateTimeKind.Utc).AddHours(-7),
                                BookingStatus = BookingStatus.Booked
                            }
                        };
                    }
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        ErrorMessages = new List<string> { "[Lion Air] Web Layout Changed! Failed to get Booking Reference" }
                    };
                }
                catch (Exception e)
                {
                    LogOut(cid, client);
                    TurnInUsername(userName);
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Lion Air] " + e.Message + "|||" + e.StackTrace },
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        }
                    };
                }
            }

            private void LogOut(string accountId, RestClient client)
            {
                const string url15 = @"/LionAirAgentsPortal/Logout.aspx";
                var searchRequest15 = new RestRequest(url15, Method.GET);
                searchRequest15.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                searchRequest15.AddHeader("Content-Encoding", "gzip");
                searchRequest15.AddHeader("Host", "agent.lionair.co.id");
                searchRequest15.AddHeader("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                searchRequest15.AddHeader("Referer",
                    "https://agent.lionair.co.id/LionAirAgentsPortal/Agents/Welcome.aspx?" + accountId);
                Thread.Sleep(1000);
                var searchResponse15 = client.Execute(searchRequest15);

                //GET PAGE DEFAULT(HOME)

                const string url16 = @"/LionAirAgentsPortal/Default.aspx";
                var searchRequest16 = new RestRequest(url16, Method.GET);
                searchRequest16.AddHeader("Accept-Encoding", "gzip, deflate, sdch");
                searchRequest16.AddHeader("Content-Encoding", "gzip");
                searchRequest16.AddHeader("Host", "agent.lionair.co.id");
                searchRequest16.AddHeader("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                searchRequest16.AddHeader("Referer",
                    "https://agent.lionair.co.id/LionAirAgentsPortal/Agents/Welcome.aspx?" + accountId);
                Thread.Sleep(1000);
                var searchResponse16 = client.Execute(searchRequest16);
            }
        }
    }
}

