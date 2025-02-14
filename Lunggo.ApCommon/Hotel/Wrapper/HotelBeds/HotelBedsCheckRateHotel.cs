﻿using System.Collections.Generic;
using Lunggo.ApCommon.Hotel.Constant;
using Lunggo.ApCommon.Hotel.Model;
using Lunggo.ApCommon.Hotel.Wrapper.HotelBeds.Sdk;
using Lunggo.ApCommon.Hotel.Wrapper.HotelBeds.Sdk.helpers;

namespace Lunggo.ApCommon.Hotel.Wrapper.HotelBeds
{
    public class HotelBedsCheckRate
    {
        public RevalidateHotelResult CheckRateHotel(HotelRevalidateInfo hotelRate)
        {
            //var client = new HotelApiClient("p8zy585gmgtkjvvecb982azn", "QrwuWTNf8a", "https://api.test.hotelbeds.com/hotel-api");
            var client = new HotelApiClient(HotelApiType.BookingApi);
            var confirmRoom = new ConfirmRoom {details = new List<RoomDetail>()};

            var bookingCheck = new BookingCheck();
            bookingCheck.addRoom(hotelRate.RateKey, confirmRoom);
            var checkRateRq = bookingCheck.toCheckRateRQ();
            var checkRateResult = new RevalidateHotelResult();

            if (checkRateRq != null)
            {
                var responseCheckRate = client.doCheck(checkRateRq);

                if (responseCheckRate != null && responseCheckRate.error == null)
                {
                    checkRateResult.IsValid = true;
                    if (responseCheckRate.hotel.rooms[0].rates[0].net == hotelRate.Price)
                    {
                        checkRateResult.IsPriceChanged = false;
                        checkRateResult.RateKey = hotelRate.RateKey;

                    }
                    else
                    {
                        checkRateResult.IsPriceChanged = true;
                        checkRateResult.NewPrice = responseCheckRate.hotel.rooms[0].rates[0].net;
                        checkRateResult.RateKey = responseCheckRate.hotel.rooms[0].rates[0].rateKey;
                    }
                }
                else
                {
                    checkRateResult.IsValid = false;
                }
                
            }
            else
            {
                checkRateResult.IsValid = false;
            }
            return checkRateResult;
        }
    }
}
