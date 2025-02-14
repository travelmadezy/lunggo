﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Lunggo.ApCommon.Flight.Constant;
using Lunggo.ApCommon.Hotel.Model;
using Lunggo.ApCommon.Hotel.Model.Logic;
using Lunggo.ApCommon.Hotel.Constant;
using Lunggo.ApCommon.Hotel.Wrapper.HotelBeds;
using Lunggo.ApCommon.Identity.Auth;
using Lunggo.ApCommon.Identity.Users;
using Lunggo.ApCommon.Payment.Constant;
using Lunggo.ApCommon.Payment.Model;
using Lunggo.ApCommon.Payment.Service;
using Lunggo.ApCommon.Product.Constant;
using Lunggo.ApCommon.Product.Model;
using Lunggo.ApCommon.Sequence;
using Lunggo.Framework.Context;
using BookingStatusCd = Lunggo.ApCommon.Hotel.Constant.BookingStatusCd;

namespace Lunggo.ApCommon.Hotel.Service
{
    public partial class HotelService
    {
        public BookHotelOutput BookHotel(BookHotelInput input)
        {
            var bookInfo = GetSelectedHotelDetailsFromCache(input.Token);
            if (bookInfo == null || bookInfo.Rooms == null || bookInfo.Rooms.Count == 0)
            {
                return new BookHotelOutput
                {
                    IsValid = false
                };
            }

            var oldPrice = bookInfo.Rooms.Sum(room => room.Rates.Sum(rate => rate.Price.Supplier));

            //Refresh RateKey
            var occupancies = new List<Occupancy>();

            foreach (var room in bookInfo.Rooms)
            {
                occupancies.AddRange(room.Rates.Select(rate => new Occupancy
                {
                    RoomCount = rate.RateCount,
                    AdultCount = rate.AdultCount,
                    ChildCount = rate.ChildCount,
                    ChildrenAges = rate.ChildrenAges
                }));
            }

            var rateFound = Enumerable.Repeat(false, occupancies.Count).ToList();

            occupancies = occupancies.Distinct().ToList();
            var checkin = bookInfo.Rooms[0].Rates[0].RateKey.Split('|')[0];
            var checkout = bookInfo.Rooms[0].Rates[0].RateKey.Split('|')[1];
            var allCurrency = Currency.GetAllCurrencies();
            Guid generatedSearchId = Guid.NewGuid();
            SaveAllCurrencyToCache(generatedSearchId.ToString(), allCurrency);
            
            var request = new SearchHotelCondition
            {
                Occupancies = occupancies,
                CheckIn = new DateTime(Convert.ToInt32(checkin.Substring(0, 4)), Convert.ToInt32(checkin.Substring(4, 2)),
                    Convert.ToInt32(checkin.Substring(6, 2))),
                Checkout = new DateTime(Convert.ToInt32(checkout.Substring(0, 4)), Convert.ToInt32(checkout.Substring(4, 2)),
                    Convert.ToInt32(checkout.Substring(6, 2))),
                Nights = bookInfo.NightCount,
                HotelCode = bookInfo.HotelCode,
                SearchId = generatedSearchId.ToString()
            };

            var hotelbeds = new HotelBedsSearchHotel();
            var searchResult = hotelbeds.SearchHotel(request);
            AddPriceMargin(searchResult.HotelDetails);
            if (searchResult.HotelDetails == null || searchResult.HotelDetails.Count == 0)
            {
                return new BookHotelOutput
                {
                    IsValid = false
                };
            }

            if (searchResult.HotelDetails.Any(hotel => hotel.Rooms == null || hotel.Rooms.Count == 0))
            {
                return new BookHotelOutput
                {
                    IsValid = false
                };
            }

            if (searchResult.HotelDetails.Any(hotel => hotel.Rooms.Any(room => room.Rates == null || room.Rates.Count == 0)))
            {
                return new BookHotelOutput
                {
                    IsValid = false
                };
            }

            var rates = bookInfo.Rooms.SelectMany(room => room.Rates).ToList();
            foreach (var rate in rates)
            {
                var sampleRatekey = rate.RateKey.Split('|');
                var roomCd = sampleRatekey[5];
                var someData = sampleRatekey[6];
                var board = sampleRatekey[7];
                var roomCount = rate.RateCount;
                var adultCount = rate.AdultCount;
                var childCount = rate.ChildCount;
                var childrenAges = "";
                var index = rates.IndexOf(rate);

                if (rate.ChildCount != 0)
                {
                    childrenAges = rate.ChildrenAges.Take(childCount).Aggregate(childrenAges, (current, age) => current + (age + "~"));
                    childrenAges = childrenAges.Substring(0, childrenAges.Length - 1);
                }

                foreach (var hotel in searchResult.HotelDetails)
                {
                    foreach (var room in hotel.Rooms)
                    {
                        foreach (var ratea in room.Rates)
                        {
                            var ratekey = ratea.RateKey.Split('|');
                            if (Convert.ToInt32(ratekey[4]) != bookInfo.HotelCode || ratekey[5] != roomCd ||
                                //ratekey[6] != someData || 
                                ratekey[7] != board ||
                                Convert.ToInt32(ratekey[9].Split('~')[0]) != roomCount
                                || Convert.ToInt32(ratekey[9].Split('~')[1]) != adultCount
                                || Convert.ToInt32(ratekey[9].Split('~')[2]) != childCount
                                ) continue;

                            if (rate.ChildrenAges != null && (rate.ChildrenAges == null || ratekey[10] != childrenAges))
                                continue;
                            rate.RateKey = ratea.RateKey;
                            rate.Price = ratea.Price;
                            rate.PaymentType = ratea.PaymentType;
                            rate.Type = ratea.Type;
                            rate.RateCommentsId = ratea.RateCommentsId;
                            rate.NightCount = ratea.NightCount;
                            rate.TermAndCondition =
                                GetRateCommentFromTableStorage(ratea.RateCommentsId, hotel.CheckInDate)
                                    .Select(x => x.Description)
                                    .ToList();
                            rateFound[index] = true;
                        }
                    }
                }
            }

            //Recheck for every rate with rate type = recheck
            foreach (var rate in bookInfo.Rooms.SelectMany(room => room.Rates))
            {
                if (BookingStatusCd.Mnemonic(rate.Type) == CheckRateStatus.Recheck)
                {
                    var revalidateResult = CheckRate(rate.RateKey, rate.Price.Supplier);
                    if (revalidateResult.IsPriceChanged)
                    {
                        rate.Price.SetSupplier(revalidateResult.NewPrice.GetValueOrDefault(), rate.Price.SupplierCurrency);
                        rate.Price.CalculateFinalAndLocal(rate.Price.LocalCurrency);
                    }
                }
            }

            var newPrice = bookInfo.Rooms.Sum(room => room.Rates.Sum(rate => rate.Price.Supplier));

            if (rateFound.Any(r => r == false))
            {
                return new BookHotelOutput
                {
                    IsValid = false,
                    
                };
            }

            bookInfo.Rooms.ForEach(ro => ro.Rates = BundleRates(ro.Rates));

            SaveSelectedHotelDetailsToCache(input.Token, bookInfo);
            if (oldPrice != newPrice)
                return new BookHotelOutput
                {
                    IsPriceChanged = true,
                    IsValid = true,
                    NewPrice = bookInfo.Rooms.Sum(room => room.Rates.Sum(rate => rate.Price.Local))
                };
            var rsvDetail = CreateHotelReservation(input, bookInfo);
            InsertHotelRsvToDb(rsvDetail);
            ExpireReservationWhenTimeout(rsvDetail.RsvNo, rsvDetail.Payment.TimeLimit);
            return new BookHotelOutput
            {
                IsPriceChanged = false,
                IsValid = true,
                RsvNo = rsvDetail.RsvNo,
                TimeLimit = rsvDetail.Payment.TimeLimit
            };
        }

        private RevalidateHotelResult CheckRate(string rateKey, decimal ratePrice)
        {
            var hb = new HotelBedsCheckRate();
            var revalidateInfo = new HotelRevalidateInfo
            {
                Price = ratePrice,
                RateKey = rateKey
            };
            return hb.CheckRateHotel(revalidateInfo);
        }

        private HotelReservation CreateHotelReservation(BookHotelInput input, HotelDetailsBase bookInfo)
        {
            var rsvNo = RsvNoSequence.GetInstance().GetNext(ProductType.Hotel);

            var ciDate = bookInfo.Rooms[0].Rates[0].RateKey.Split('|')[0];
            var coDate = bookInfo.Rooms[0].Rates[0].RateKey.Split('|')[1];
            var checkindate = new DateTime(Convert.ToInt32(ciDate.Substring(0, 4)),
                Convert.ToInt32(ciDate.Substring(4, 2)), Convert.ToInt32(ciDate.Substring(6, 2)));
            var checkoutdate = new DateTime(Convert.ToInt32(coDate.Substring(0, 4)),
                Convert.ToInt32(coDate.Substring(4, 2)), Convert.ToInt32(coDate.Substring(6, 2)));

            var hotelInfo = new HotelDetail
            {
                AccomodationType = bookInfo.AccomodationType,
                CheckInDate = checkindate,
                CheckOutDate = checkoutdate,
                City = bookInfo.City,
                CountryCode = bookInfo.CountryCode,
                DestinationCode = bookInfo.DestinationCode,
                NetTotalFare = bookInfo.Rooms.SelectMany(r => r.Rates).Sum(r => r.Price.Local),
                OriginalTotalFare = bookInfo.Rooms.SelectMany(r => r.Rates).Sum(r => r.GetApparentOriginalPrice()),
                NetCheapestFare = bookInfo.Rooms.SelectMany(r => r.Rates).Min(r => r.Price.Local),
                NetCheapestTotalFare = bookInfo.Rooms.SelectMany(r => r.Rates).Min(r => r.Price.Local),
                OriginalCheapestTotalFare = bookInfo.Rooms.SelectMany(r => r.Rates).Min(r => r.GetApparentOriginalPrice()),
                OriginalCheapestFare = bookInfo.Rooms.SelectMany(r => r.Rates).Min(r => r.GetApparentOriginalPrice()),
                TotalAdult = input.Passengers.Count(p => p.Type == PaxType.Adult),
                TotalChildren = input.Passengers.Count(p => p.Type == PaxType.Child),
                SpecialRequest = input.SpecialRequest,
                HotelName = bookInfo.HotelName,
                HotelCode = bookInfo.HotelCode,
                Rooms = bookInfo.Rooms,
                Address = bookInfo.Address,
                PhonesNumbers = bookInfo.PhonesNumbers,
                StarRating = bookInfo.StarRating,
                AreaCode = bookInfo.AreaCode,
                ZoneCode = bookInfo.ZoneCode
            };

            var identity = HttpContext.Current.User.Identity as ClaimsIdentity ?? new ClaimsIdentity();
            var clientId = identity.Claims.Single(claim => claim.Type == "Client ID").Value;
            var platform = Client.GetPlatformType(clientId);
            var deviceId = identity.Claims.Single(claim => claim.Type == "Device ID").Value;

            var rsvDetail = new HotelReservation
            {
                RsvNo = rsvNo,
                Contact = input.Contact,
                HotelDetails = hotelInfo,
                Pax = input.Passengers,
                Payment = new PaymentDetails
                {
                    Status = PaymentStatus.Pending,
                    LocalCurrency = new Currency(OnlineContext.GetActiveCurrencyCode()),
                    OriginalPriceIdr = bookInfo.Rooms.SelectMany(r => r.Rates).Sum(r => r.Price.Local),
                    TimeLimit = DateTime.UtcNow.AddHours(1)
                    //TimeLimit = bookInfo.Rooms.SelectMany(r => r.Rates).Min(order => order.TimeLimit).AddMinutes(-10),
                },
                RsvStatus = RsvStatus.InProcess,
                RsvTime = DateTime.UtcNow,
                State = new ReservationState
                {
                    Platform = platform,
                    DeviceId = deviceId,
                    Language = "id", //OnlineContext.GetActiveLanguageCode();
                    Currency = new Currency("IDR"), //OnlineContext.GetActiveCurrencyCode());
                },
                User = HttpContext.Current.User.Identity.IsUserAuthorized()
                    ? HttpContext.Current.User.Identity.GetUser()
                    : null
            };

            PaymentService.GetInstance().GetUniqueCode(rsvDetail.RsvNo, null, null);

            return rsvDetail;
        }
    }
}
