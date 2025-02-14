﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using CsQuery;
using Lunggo.ApCommon.Flight.Constant;
using Lunggo.ApCommon.Flight.Model;
using Lunggo.ApCommon.Payment.Model;
using Lunggo.ApCommon.Product.Constant;
using Lunggo.Framework.Web;
using RestSharp;
using System.Diagnostics;
using Lunggo.ApCommon.Constant;
using Supplier = Lunggo.ApCommon.Payment.Constant.Supplier;

namespace Lunggo.ApCommon.Flight.Wrapper.Citilink
{
    internal partial class CitilinkWrapper
    {
        internal override BookFlightResult BookFlight(FlightBookingInfo bookInfo)
        {
            return Client.BookFlight(bookInfo);
        }

        private partial class CitilinkClientHandler
        {
            internal BookFlightResult BookFlight(FlightBookingInfo bookInfo)
            {
                //RevalidateConditions conditions = new RevalidateConditions
                //{
                //    Itinerary = bookInfo.Itinerary
                //};
                //conditions.Itinerary = bookInfo.Itinerary;
                //RevalidateFareResult revalidateResult = RevalidateFare(conditions);
                //if (revalidateResult.IsItineraryChanged || revalidateResult.IsPriceChanged || (!revalidateResult.IsValid))
                //{
                //    return new BookFlightResult
                //    {
                //        IsValid = revalidateResult.IsValid,
                //        Errors = revalidateResult.Errors,
                //        ErrorMessages = revalidateResult.ErrorMessages,
                //        IsItineraryChanged = revalidateResult.IsItineraryChanged,
                //        IsPriceChanged = revalidateResult.IsPriceChanged,
                //        IsSuccess = false,
                //        NewItinerary = revalidateResult.NewItinerary,
                //        NewPrice = revalidateResult.NewPrice,
                //        Status = null
                //    };
                //}
                //bookInfo.Itinerary = revalidateResult.NewItinerary;
                var client = CreateAgentClient();
                Login(client);

                string origin, dest, coreFareId;
                DateTime date;
                int adultCount, childCount, infantCount;
                CabinClass cabinClass;
                decimal price;
                try
                {
                    var splittedFareId = bookInfo.Itinerary.FareId.Split('.').ToList();
                    date = new DateTime(int.Parse(splittedFareId[4]), int.Parse(splittedFareId[3]), int.Parse(splittedFareId[2]));
                    adultCount = int.Parse(splittedFareId[5]);
                    childCount = int.Parse(splittedFareId[6]);
                    infantCount = int.Parse(splittedFareId[7]);
                    string airlineCode = splittedFareId[8];
                    string flightNumber = splittedFareId[9];
                    coreFareId = splittedFareId[11];
                    var splitcoreFareId = coreFareId.Split('~').ToList();
                    int index;
                    if (splitcoreFareId.Count <= 16)
                    {
                        origin = splitcoreFareId[11];
                        dest = splitcoreFareId[13];
                        
                    }
                    else if (splitcoreFareId.Count <= 24)
                    {
                        index = 11;
                        origin = splitcoreFareId[index];
                        dest = splitcoreFareId[21];
                    }
                    else
                    {
                        origin = splitcoreFareId[18];
                        dest = splitcoreFareId[28];
                    }
                }
                catch 
                {
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.FareIdNoLongerValid },
                        ErrorMessages = new List<string> { "[Citilink] Error while splitting the fareid" }
                    };
                }
                




                // SEARCH
                var date2 = date.AddDays(1);
                string searchUrl = @"BookingListTravelAgent.aspx";

                var searchRequest = new RestRequest(searchUrl, Method.POST);
                var searchPostData =
                     @"AvailabilitySearchInputBookingListTravelAgentView$ButtonSubmit=Find+Flights" +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$DropDownListMarketDay1=" + date.Day +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$DropDownListMarketDay2=" + date2.Day +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$DropDownListMarketMonth1=" + date.Year + "-" + date.Month +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$DropDownListMarketMonth2=" + date.Year + "-" + date.Month +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$DropDownListPassengerType_ADT=" + adultCount +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$DropDownListPassengerType_CHD=" + childCount +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$DropDownListPassengerType_INFANT=" + infantCount +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$DropDownListSearchBy=columnView" +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$RadioButtonMarketStructure=OneWay" +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$TextBoxMarketDestination1=" + dest +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$TextBoxMarketDestination2=" +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$TextBoxMarketOrigin1=" + origin +
                     @"&AvailabilitySearchInputBookingListTravelAgentView$TextBoxMarketOrigin2=" +
                     @"&AvailabilitySearchInputBookingListTravelAgentViewdestinationStation1=" + dest +
                     @"&AvailabilitySearchInputBookingListTravelAgentViewdestinationStation2=" +
                     @"&AvailabilitySearchInputBookingListTravelAgentVieworiginStation1=" + origin +
                     @"&AvailabilitySearchInputBookingListTravelAgentVieworiginStation2=" +
                     @"&ControlGroupBookingListTravelAgentView$BookingListBookingListTravelAgentView$DropDownListTypeOfSearch=0" +
                     @"&ControlGroupBookingListTravelAgentView$BookingListBookingListTravelAgentView$Search=ForAgent" +
                     @"&ControlGroupBookingListTravelAgentView$BookingListBookingListTravelAgentView$TextBoxKeyword=" +
                     @"&__EVENTARGUMENT=" +
                     @"&__EVENTTARGET=" +
                    //@"&__VIEWSTATE=/wEPDwUBMGQYAQUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgMFWkNvbnRyb2xHcm91cEJvb2tpbmdMaXN0VHJhdmVsQWdlbnRWaWV3JEJvb2tpbmdMaXN0Qm9va2luZ0xpc3RUcmF2ZWxBZ2VudFZpZXckUmFkaW9Gb3JBZ2VudAVbQ29udHJvbEdyb3VwQm9va2luZ0xpc3RUcmF2ZWxBZ2VudFZpZXckQm9va2luZ0xpc3RCb29raW5nTGlzdFRyYXZlbEFnZW50VmlldyRSYWRpb0ZvckFnZW5jeQVbQ29udHJvbEdyb3VwQm9va2luZ0xpc3RUcmF2ZWxBZ2VudFZpZXckQm9va2luZ0xpc3RCb29raW5nTGlzdFRyYXZlbEFnZW50VmlldyRSYWRpb0ZvckFnZW5jeTXhy2ltZry/VwOL4DGYiD+r/S9H" +
                     @"&date_picker=" + date.Year + "-" + date.Month + "-" + date.Day +
                     @"&date_picker=" + date2.Day + "-" + date2.Month + "-" + date2.Day +
                     @"&pageToken=";
                searchRequest.AddParameter("application/x-www-form-urlencoded", searchPostData, ParameterType.RequestBody);
                var searchResponse = client.Execute(searchRequest);

                if (searchResponse.ResponseUri.AbsolutePath != "/ScheduleSelect.aspx")
                {
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Citilink] Error in requesting at BookingListTravelAgent.aspx.Unexpected absolute path response or status code || " + searchResponse.StatusCode + " " + searchResponse.ResponseUri + " || " + searchResponse.Content }
                    };
                }

                // SELECT

                var selectUri = @"ScheduleSelect.aspx";

                var selectRequest = new RestRequest(selectUri, Method.POST);
                var selectPostData =
                   @"AvailabilitySearchInputScheduleSelectView$DdlCurrencyDynamic=IDR" +
                   @"&AvailabilitySearchInputScheduleSelectView$DropDownListMarketDay1=" + date.Day +
                   @"&AvailabilitySearchInputScheduleSelectView$DropDownListMarketDay2=" + date2.Day +
                   @"&AvailabilitySearchInputScheduleSelectView$DropDownListMarketMonth1=" + date.Year + "-" + date.Month +
                   @"&AvailabilitySearchInputScheduleSelectView$DropDownListMarketMonth2=" + date2.Year + "-" + date.Month +
                   @"&AvailabilitySearchInputScheduleSelectView$DropDownListPassengerType_ADT=" + adultCount +
                   @"&AvailabilitySearchInputScheduleSelectView$DropDownListPassengerType_CHD=" + childCount +
                   @"&AvailabilitySearchInputScheduleSelectView$DropDownListPassengerType_INFANT=" + infantCount +
                   @"&AvailabilitySearchInputScheduleSelectView$DropDownListSearchBy=columnView" +
                   @"&AvailabilitySearchInputScheduleSelectView$RadioButtonMarketStructure=OneWay" +
                   @"&AvailabilitySearchInputScheduleSelectView$TextBoxMarketDestination1=" + dest +
                   @"&AvailabilitySearchInputScheduleSelectView$TextBoxMarketDestination2=" +
                   @"&AvailabilitySearchInputScheduleSelectView$TextBoxMarketOrigin1=" + origin +
                   @"&AvailabilitySearchInputScheduleSelectView$TextBoxMarketOrigin2=" +
                   @"&AvailabilitySearchInputScheduleSelectViewdestinationStation1=" + dest +
                   @"&AvailabilitySearchInputScheduleSelectViewdestinationStation2=" +
                   @"&AvailabilitySearchInputScheduleSelectVieworiginStation1=" + origin +
                   @"&AvailabilitySearchInputScheduleSelectVieworiginStation2=" +
                   @"&ControlGroupScheduleSelectView$AvailabilityInputScheduleSelectView$HiddenFieldTabIndex1=1" +
                   @"&ControlGroupScheduleSelectView$AvailabilityInputScheduleSelectView$market1=" + HttpUtility.UrlEncode(coreFareId) +
                   @"&ControlGroupScheduleSelectView$ButtonSubmit=Lanjutkan" +
                   @"&__EVENTARGUMENT=" +
                   @"&__EVENTTARGET=" +
                    //@"&__VIEWSTATE=/wEPDwUBMGRkBsrCYiDYbQKCOcoq/UTudEf14vk=" +
                   @"&date_picker=" + date.Year + "-" + date.Month + "-" + date.Day +
                   @"&date_picker=" + date2.Year + "-" + date2.Month + "-" + date2.Day +
                   @"&pageToken=";

                
                selectRequest.AddParameter("application/x-www-form-urlencoded", selectPostData, ParameterType.RequestBody);
                var selectResponse = client.Execute(selectRequest);

                if (selectResponse.ResponseUri.AbsolutePath != "/Passenger.aspx")
                {
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Citilink] Error in requesting at ScheduleSelect.aspx.Unexpected absolute path response or status code || " + selectResponse.StatusCode + " " + selectResponse.ResponseUri + " || " + selectResponse.Content }
                    };
                }

                // INPUT DATA (TRAVELER)

                string passUrl = @"Passenger.aspx";

                var passRequest = new RestRequest(passUrl, Method.POST);
                var passPostData =
                    @"__EVENTTARGET=" +
                    @"&__EVENTARGUMENT=" +
                    @"&__VIEWSTATE=/wEPDwUBMGQYAQUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgIFR0NPTlRST0xHUk9VUFBBU1NFTkdFUiRQYXNzZW5nZXJJbnB1dFZpZXdQYXNzZW5nZXJWaWV3JENoZWNrQm94SW5zdXJhbmNlBUFDT05UUk9MR1JPVVBQQVNTRU5HRVIkUGFzc2VuZ2VySW5wdXRWaWV3UGFzc2VuZ2VyVmlldyRDaGVja0JveFBNSdyEEzFRd8wxFzhO4NwRln1a8cAL" +
                    @"&pageToken=" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$DropDownListTitle=MR" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxFirstName=Yoga" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxMiddleName=Dwi" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxLastName=Sukma" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxAddressLine2=Equity Tower, 25th Floor" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxAddressLine1=Jl. Jend Sudirman Kav. 52-53" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxAddressLine3=SCBD Lot 9" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxCity=Jakarta Selatan" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$DropDownListCountry=" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxPostalCode=zip/postal" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxWorkPhone=0811351793" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxHomePhone=085360343300"+
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxFax=021-29035099" +
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$RadioButtonInsurance=No"+
                    @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView$TextBoxEmailAddress=dwi.agustina@travelmadezy.com";
                    //@"CONTROLGROUPPASSENGER$ButtonSubmit=Continue" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$ContactInputPassengerView%24DropDownListCountry=" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$DropDownListStateProvince=" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$DropDownListTitle=MR" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxAddressLine1=Jl. Jend Sudirman Kav. 52-53" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxAddressLine2=Equity Tower, 25th Floor" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxAddressLine3=SCBD Lot 9" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxCity=Jakarta Selatan" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxEmailAddress=dwi.agustina@travelmadezy.com" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxFax=021-29035099" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxFirstName=Yoga" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxHomePhone=" + bookInfo.Contact.CountryCallingCode + bookInfo.Contact.Phone +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxLastName=Sukma" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxMiddleName=Dwi" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxPostalCode=zip/postal" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$$ContactInputPassengerView$TextBoxWorkPhone=085728755848" +
                    ////@"&CONTROLGROUPPASSENGER$ItineraryDistributionInputPassengerView$Distribution=2" +
                    //@"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$RadioButtonInsurance=No";
                int i = 0;
                foreach (var passenger in bookInfo.Passengers.Where(p => p.Type == PaxType.Adult))
                {
                    var title = passenger.Title == Title.Mister ? "MR" : "MS";
                    passPostData +=
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListTitle_" + i + "=" + title +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$TextBoxFirstName_" + i + "=" + passenger.FirstName +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$TextBoxLastName_" + i + "=" + passenger.LastName +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$TextBoxMiddleName_" + i + "=middle" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListGender_" + i + "=1" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListNationality_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListResidentCountry_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListBirthDateDay_" + i + "=1" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListBirthDateMonth_" + i + "=1" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListBirthDateYear_" + i + "=1970" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentType0_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$TextBoxDocumentNumber0_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentCountry0_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentDateDay0_" + i + "=1" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentDateMonth0_" + i + "=1" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentDateYear0_" + i + "=" + date.Year +
                        @"&CONTROLGROUPPASSENGER$ButtonSubmit=Continue";

                    i++;
                }
                foreach (var passenger in bookInfo.Passengers.Where(p => p.Type == PaxType.Child))
                {
                    var title = passenger.Title == Title.Mister ? "MR" : "MS";
                    passPostData +=
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListTitle_" + i + "=" + title +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$TextBoxFirstName_" + i + "=" + passenger.FirstName +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$TextBoxLastName_" + i + "=" + passenger.LastName +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$TextBoxMiddleName_" + i + "=middle" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListGender_" + i + "=1" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListNationality_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListResidentCountry_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListBirthDateDay_" + i + "=" + passenger.DateOfBirth.GetValueOrDefault().Day +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListBirthDateMonth_" + i + "=" + passenger.DateOfBirth.GetValueOrDefault().Month +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListBirthDateYear_" + i + "=" + passenger.DateOfBirth.GetValueOrDefault().Year +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentType0_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$TextBoxDocumentNumber0_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentCountry0_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentDateDay0_" + i + "=1" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentDateMonth0_" + i + "=1" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListDocumentDateYear0_" + i + "=" + date.Year;

                    i++;
                }
                i = 0;
                foreach (var passenger in bookInfo.Passengers.Where(p => p.Type == PaxType.Infant))
                {
                    passPostData +=
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListTitle_" + i + "_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListAssign_" + i + "_" + i + "=" + i +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListBirthDateDay_" + i + "_" + i + "=" + passenger.DateOfBirth.GetValueOrDefault().Day +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListBirthDateMonth_" + i + "_" + i + "=" + passenger.DateOfBirth.GetValueOrDefault().Month +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListBirthDateYear_" + i + "_" + i + "=" + passenger.DateOfBirth.GetValueOrDefault().Year +
                        @"&CONTROLGROUPPASSENGER%24PassengerInputViewPassengerView%24TextBoxFirstName_" + i + "_" + i + "=" + passenger.FirstName +
                        @"&CONTROLGROUPPASSENGER%24PassengerInputViewPassengerView%24TextBoxLastName_" + i + "_" + i + "=" + passenger.LastName +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListGender_" + i + "_" + i + "=" + (int)passenger.Gender +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListNationality_" + i + "_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$DropDownListResidentCountry_" + i + "_" + i + "=" +
                        @"&CONTROLGROUPPASSENGER$PassengerInputViewPassengerView$TextBoxMiddleName_" + i + "_" + i + "=middle";
                    i++;
                }

                //passPostData +=
                //    @"&__EVENTARGUMENT=" +
                //    @"&__EVENTTARGET=" +
                //    //@"&__VIEWSTATE=/wEPDwUBMGQYAQUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgIFR0NPTlRST0xHUk9VUFBBU1NFTkdFUiRQYXNzZW5nZXJJbnB1dFZpZXdQYXNzZW5nZXJWaWV3JENoZWNrQm94SW5zdXJhbmNlBUFDT05UUk9MR1JPVVBQQVNTRU5HRVIkUGFzc2VuZ2VySW5wdXRWaWV3UGFzc2VuZ2VyVmlldyRDaGVja0JveFBNSXZkWh6Cdtm1mad5oP+7VGz2nQKe
                //    @"&pageToken=";

                passRequest.AddParameter("application/x-www-form-urlencoded", passPostData, ParameterType.RequestBody);
                var passResponse = client.Execute(passRequest);

                if (passResponse.ResponseUri.AbsolutePath != "/SeatMap.aspx")
                {
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.InvalidInputData },
                        ErrorMessages = new List<string> { "[Citilink] Error while posting passenger data at Passenger.aspx.Unexpected absolute path response or status code || " + passResponse.StatusCode + " " + passResponse.ResponseUri + " || " + passResponse.Content }
                    };
                }

                // SELECT SEAT (UNITMAP)

                var seatUrl = @"SeatMap.aspx";
                var seatRequest = new RestRequest(seatUrl, Method.POST);

                var seatPostData =
                          @"ControlGroupUnitMapView$UnitMapViewControl$compartmentDesignatorInput=" +
                          @"&ControlGroupUnitMapView$UnitMapViewControl$deckDesignatorInput=1" +
                          @"&ControlGroupUnitMapView$UnitMapViewControl$passengerInput=0" +
                          @"&ControlGroupUnitMapView$UnitMapViewControl$tripInput=0" +
                          @"&__EVENTARGUMENT=" +
                          @"&__EVENTTARGET=ControlGroupUnitMapView$UnitMapViewControl$LinkButtonAssignUnit" +
                    //"&__VIEWSTATE=/wEPDwUBMGQYAQUeX19Db250cm9sc1JlcXVpcmVQb3N0QmFja0tleV9fFgEFN0NvbnRyb2xHcm91cFVuaXRNYXBWaWV3JFVuaXRNYXBWaWV3Q29udHJvbCRDaGVja0JveFNlYXRC9WoNScpqWuAJVhj4Iqw3MUfIjw=="
                          @"&pageToken=";
                int j = 0;
                foreach (var passenger in bookInfo.Passengers.Where(p => p.Type == PaxType.Adult))
                {
                    seatPostData +=
                         @"&ControlGroupUnitMapView$UnitMapViewControl$EquipmentConfiguration_0_PassengerNumber_" + j + "=" +
                         @"&ControlGroupUnitMapView$UnitMapViewControl$HiddenEquipmentConfiguration_0_PassengerNumber_" + j + "=";
                    j++;
                }
                foreach (var passenger in bookInfo.Passengers.Where(p => p.Type == PaxType.Child))
                {
                    seatPostData +=
                         @"&ControlGroupUnitMapView$UnitMapViewControl$EquipmentConfiguration_0_PassengerNumber_" + j + "=" +
                         @"&ControlGroupUnitMapView$UnitMapViewControl$HiddenEquipmentConfiguration_0_PassengerNumber_" + j + "=";
                    j++;
                }
                seatRequest.AddParameter("application/x-www-form-urlencoded", seatPostData, ParameterType.RequestBody);
                var seatResponse = client.Execute(seatRequest);

                if (seatResponse.ResponseUri.AbsolutePath != "/Payment.aspx")
                {
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Citilink] Error in SeatMap.aspx.Unexpected absolute path response or status code || " + seatResponse.StatusCode + " " + seatResponse.ResponseUri + " || " + seatResponse.Content }
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

                /*Buat dapat Info Itinerary dan Harga*/
                var getPaymenturl = @"Payment.aspx";
                var paymentGetRequest = new RestRequest(getPaymenturl, Method.GET);
                paymentGetRequest.AddHeader("Referer", "https://book.citilink.co.id/SeatMap.aspx");
                var paymentGetresponse = client.Execute(paymentGetRequest);
                var html = paymentGetresponse.Content;
                CQ detailFlight = (CQ)html;
                var getPrice = detailFlight[".total>dd"].Text().Trim().Split('.');
                var harga = getPrice[1].Trim().Replace(",", "");
                var fixPrice = decimal.Parse(harga);

                //Cek Harga di Final
                if (bookInfo.Itinerary.Price.Supplier != fixPrice) 
                {
                    var fixItin = bookInfo.Itinerary;
                    fixItin.Price.SetSupplier(fixPrice, new Currency("IDR", Supplier.Citilink));
                    fixItin.FareId = fixItin.FareId.Replace(bookInfo.Itinerary.Price.Supplier.ToString(),fixPrice.ToString());
                    
                    return new BookFlightResult
                    {
                        IsValid = true,
                        IsItineraryChanged = false,
                        IsPriceChanged = true,
                        IsSuccess = false,
                        //Errors = new List<FlightError> { FlightError.FareIdNoLongerValid },
                        ErrorMessages = new List<string> { "[Citilink] Price is changed!" },
                        NewItinerary = fixItin,
                        NewPrice = fixPrice,
                        Status = null
                    };
                }
                

                // SELECT HOLD (PAYMENT)

                var paymentUrl = @"Payment.aspx";
                var paymentRequest = new RestRequest(paymentUrl, Method.POST);

                var paymentPostData =
                   @"__EVENTTARGET=" +
                   @"&__EVENTARGUMENT=" +
                   @"&__VIEWSTATE=%2FwEPDwUBMGRkBsrCYiDYbQKCOcoq%2FUTudEf14vk%3D" +
                   @"&pageToken=" +
                   @"&DropDownListPaymentMethodCode=AgencyAccount%3AAG" +
                   @"&DropDownListPaymentMethodCode=PrePaid%3AKB" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24TxtCardNumber=" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24TxtVcc=" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24DdlExpMonth=1 " +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24DdlExpYear=2015" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24TxtCardHolderName=Dwi+Agustina" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24TxtCardHolderAddress=Jl.+Jend+Sudirman+Kav.+52-53" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24TxtCardHolderCity=Jakarta+Selatan" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24TxtCardHolderProvince=" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24TxtCardHolderZipCode=" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24DdlCardHolderCountry=AD" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24TxtCardHolderEmail=dwi.agustina%40travelmadezy.com" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24TxtCardHolderPhone=" + bookInfo.Contact.CountryCallingCode + bookInfo.Contact.Phone +
                   @"&DropDownListPaymentMethodCode=ExternalAccount%3AMC" +
                   @"&DropDownListPaymentMethodCode=Voucher%3AVO" +
                   @"&TextBoxVoucherAccount_VO_ACCTNO=" +
                   @"&AMOUNT=" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24DropDownListPaymentMethodCode=PrePaid%3AHOLD" +
                   @"&DropDownListPaymentMethodCode=PrePaid%3AHOLD" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24AgreementPaymentInputViewPaymentView%24CheckBoxAgreement=on" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ControlGroupPaymentInputViewPaymentView%24storedPaymentId=" +
                   @"&CONTROLGROUPPAYMENTBOTTOM%24ButtonSubmit=Lanjutkan";
                
                paymentRequest.AddParameter("application/x-www-form-urlencoded", paymentPostData, ParameterType.RequestBody);
                var paymentResponse = client.Execute(paymentRequest);


                if (paymentResponse.ResponseUri.AbsolutePath != "/Wait.aspx")
                {
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Citilink] Error in SeatMap.aspx.Unexpected absolute path response or status code || " + paymentResponse.StatusCode + " " + paymentResponse.ResponseUri + " || " + paymentResponse.Content }
                    };

                }

                
                // WAIT

                var waitUrl = "Wait.aspx";
                var waitRequest = new RestRequest(waitUrl, Method.GET);
                var waitResponse = client.Execute(waitRequest);

                while (!(waitResponse.ResponseUri.AbsolutePath.Contains("Itinerary.aspx")))
                {
                    waitResponse = client.Execute(waitRequest);
                }

                //var htmlResult = System.IO.File.ReadAllText(@"C:\Users\User\Documents\Kerja\Crawl\Itin.txt");

                var ambilDataItin = (CQ)waitResponse.Content;


                var hasil = new BookFlightResult();

                try 
                {
                    var tunjukDataNomorBooking = ambilDataItin.MakeRoot()["#SpanRecordLocator"];
                    var NomorBooking = tunjukDataNomorBooking.Select(x => x.Cq().Text()).FirstOrDefault();
                    var tunjukDataTimeLimit = ambilDataItin.MakeRoot()["#itineraryBody>p"];
                    //var tunjukDataTimeLimit1 = tunjukDataTimeLimit["p:nth-child(1)"];
                    var ambilTimeLimit = tunjukDataTimeLimit.Select(x => x.Cq().Text()).FirstOrDefault();
                    var timelimitIndex = ambilTimeLimit.IndexOf("[");
                    var timelimitIndex2 = ambilTimeLimit.IndexOf("]");
                    var timelimitParse3 = ambilTimeLimit.Split(' ');
                    var timelimitString = ambilTimeLimit.Substring(timelimitIndex + 1, timelimitIndex2 - timelimitIndex - 1);
                    var timelimitSplitComma = timelimitString.Split(',');
                    var timelimit1SplitSpace = timelimitSplitComma[1].Trim().Split(' ');
                    var timelimit2SplitSpace = timelimitSplitComma[2].Trim().Split(' ');
                    var timelimitDate = timelimit1SplitSpace[1].Trim();
                    var timelimitMonth = timelimit1SplitSpace[0].Trim();
                    var timelimitYear = timelimit2SplitSpace[0].Trim();
                    var timelimitTime = timelimit2SplitSpace[28].Trim();
                    string tahun;
                    if (timelimitYear.Length > 4)
                    {
                        tahun = timelimitYear.Substring(0, 4);
                    }
                    else
                    {
                        tahun = timelimitYear;
                    }


                    if (timelimitMonth == "Nop")
                        timelimitMonth = "Nov";

                    if (timelimitMonth == "Agust")
                        timelimitMonth = "Agu";

                    var timelimit = DateTime.Parse(timelimitDate + "-" + timelimitMonth + "-" + tahun + " " + timelimitTime, CultureInfo.CreateSpecificCulture("id-ID"));

                    var status = new BookingStatusInfo();
                    status.BookingId = NomorBooking;
                    status.BookingStatus = BookingStatus.Booked;
                    status.TimeLimit = DateTime.SpecifyKind(timelimit, DateTimeKind.Utc);

                    hasil.Status = status;
                    hasil.IsSuccess = true;
                    return hasil;
                }
                catch 
                {
                    return new BookFlightResult
                    {
                        IsSuccess = false,
                        Status = new BookingStatusInfo
                        {
                            BookingStatus = BookingStatus.Failed
                        },
                        Errors = new List<FlightError> { FlightError.TechnicalError },
                        ErrorMessages = new List<string> { "[Citilink] Failed to get Booking Id and timelimit booking" }
                    };
                }
                
            }
        }
    }
}
