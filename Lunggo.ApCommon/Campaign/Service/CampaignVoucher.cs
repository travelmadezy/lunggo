﻿using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Lunggo.ApCommon.Campaign.Constant;
using Lunggo.ApCommon.Campaign.Model;
using System;
using Lunggo.ApCommon.Constant;
using Lunggo.ApCommon.Flight.Model;
using Lunggo.ApCommon.Flight.Service;
using Lunggo.ApCommon.Hotel.Service;
using Lunggo.ApCommon.Identity.Auth;
using Lunggo.ApCommon.Identity.Users;
using Lunggo.ApCommon.Payment.Constant;
using Lunggo.ApCommon.Payment.Model;
using Lunggo.ApCommon.Product.Constant;
using Lunggo.ApCommon.Product.Model;
using Lunggo.Framework.Http;

namespace Lunggo.ApCommon.Campaign.Service
{
    public partial class CampaignService
    {
        public CampaignVoucher GetCampaignVoucher(string voucherCode)
        {
            return GetDb.GetCampaignVoucher(voucherCode);
        }

        public VoucherResponse ValidateVoucherRequest(string rsvNo, string voucherCode, PaymentDetails paymentDetails = null)
        {
            CampaignVoucher voucher;
            return ValidateVoucherRequest(rsvNo, voucherCode, out voucher, paymentDetails);
        }

        public VoucherResponse ValidateVoucherRequest(string rsvNo, string voucherCode, out CampaignVoucher voucher, PaymentDetails paymentDetails = null)
        {
            var response = new VoucherResponse();
            var user = HttpContext.Current.User;

            voucher = GetDb.GetCampaignVoucher(voucherCode);

            if (voucher == null)
            {
                response.VoucherStatus = VoucherStatus.VoucherNotFound;
                return response;
            }

            if (voucher.ProductType != null && !voucher.ProductType.Contains(rsvNo[0]))
            {
                response.VoucherStatus = VoucherStatus.ProductNotEligible;
                return response;
            }

            if (user.Identity.IsAuthenticated && user.Identity.IsUserAuthorized())
                response.Email = user.Identity.GetEmail();
            response.VoucherCode = voucherCode;

            var contact = Contact.GetFromDb(rsvNo);
            if (contact == null)
            {
                response.VoucherStatus = VoucherStatus.ReservationNotFound;
                return response;
            }

            if (voucher.CampaignTypeCd == CampaignTypeCd.Mnemonic(CampaignType.Public) && 
                !IsPhoneAndEmailEligibleInCache(voucherCode, contact.CountryCallingCode + contact.Phone, contact.Email))
            {
                response.VoucherStatus = VoucherStatus.EmailNotEligible;
                return response;
            }

            if (paymentDetails == null)
            {
                paymentDetails = PaymentDetails.GetFromDb(rsvNo);
                if (paymentDetails == null)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotFound;
                    return response;
                }
            }

            var price = paymentDetails.OriginalPriceIdr * paymentDetails.LocalCurrency.Rate;

            var validationStatus = ValidateVoucher(voucher, price, response.Email, voucherCode);

            if (validationStatus == VoucherStatus.Success)
            {
                CalculateVoucherDiscount(voucher, price, response);
                response.Discount = new UsedDiscount
                {
                    Name = voucher.CampaignName,
                    Description = voucher.CampaignDescription,
                    DisplayName = voucher.DisplayName,
                    Percentage = voucher.ValuePercentage.GetValueOrDefault(),
                    Constant = voucher.ValueConstant.GetValueOrDefault(),
                    Currency = new Currency("IDR"),
                    IsFlat = false
                };
            }

            ReservationBase rsv;
            if (rsvNo.StartsWith("1"))
                rsv = FlightService.GetInstance().GetReservation(rsvNo);
            else
                rsv = HotelService.GetInstance().GetReservation(rsvNo);

            var cost = rsv.GetTotalSupplierPrice();
            if (voucher.MaxBudget.HasValue &&
                (voucher.MaxBudget - voucher.UsedBudget < cost - paymentDetails.FinalPriceIdr * 0.97M))
            {
                response.VoucherStatus = VoucherStatus.NoBudgetRemaining;
                return response;
            }

            //////////////  HARDCODED VALIDATION /////////////////

            if (voucher.CampaignId == 66 || voucher.CampaignName == "Good Monday") // Good Monday
            {
                var valid = true;

                var identity = HttpContext.Current.User.Identity as ClaimsIdentity ?? new ClaimsIdentity();
                var clientId = identity.Claims.Single(claim => claim.Type == "Client ID").Value;
                var platform = Client.GetPlatformType(clientId);
                if (platform == PlatformType.AndroidApp || platform == PlatformType.IosApp)
                {
                    response.VoucherStatus = VoucherStatus.PlatformNotEligible;
                    valid = false;
                }


                if ((rsv as FlightReservation).Itineraries.SelectMany(i => i.Trips)
                    .Any(t =>
                        FlightService.GetInstance().GetAirportCountryCode(t.OriginAirport) != "ID" ||
                        FlightService.GetInstance().GetAirportCountryCode(t.DestinationAirport) != "ID"))
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                if (!(new[] { "JKT", "CGK", "HLP", "SRG", "JOG", "TNJ", "SUB" }
                    .Contains((rsv as FlightReservation).Itineraries[0].Trips[0].DestinationAirport)))
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                var airlines =
                    (rsv as FlightReservation).Itineraries.SelectMany(i => i.Trips)
                        .SelectMany(t => t.Segments)
                        .Select(s => s.AirlineCode);
                foreach (var airline in airlines)
                {
                    if (!(new[] { "QG", "SJ", "IN", "ID" }.Contains(airline)))
                    {
                        response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                        valid = false;
                    }
                }

                if (DateTime.UtcNow.AddHours(7).DayOfWeek != DayOfWeek.Monday)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                if (!valid)
                {
                    return response;
                }
            }

            if (voucher.CampaignId == 71 || voucher.CampaignName == "Selasa Spesial") // Selasa Spesial
            {
                var valid = true;

                if (DateTime.UtcNow.AddHours(7).DayOfWeek != DayOfWeek.Tuesday)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                if (!valid)
                {
                    return response;
                }
            }

            if (voucher.CampaignId == 67 || voucher.CampaignName == "Promo Rabu") // Promo Rabu
            {
                var valid = true;

                var identity = HttpContext.Current.User.Identity as ClaimsIdentity ?? new ClaimsIdentity();
                var clientId = identity.Claims.Single(claim => claim.Type == "Client ID").Value;
                var platform = Client.GetPlatformType(clientId);
                if (platform == PlatformType.AndroidApp || platform == PlatformType.IosApp)
                {
                    response.VoucherStatus = VoucherStatus.PlatformNotEligible;
                    valid = false;
                }


                if ((rsv as FlightReservation).Itineraries.SelectMany(i => i.Trips)
                    .Any(t =>
                        FlightService.GetInstance().GetAirportCountryCode(t.OriginAirport) != "ID" ||
                        FlightService.GetInstance().GetAirportCountryCode(t.DestinationAirport) != "ID"))
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                var airlines =
                    (rsv as FlightReservation).Itineraries.SelectMany(i => i.Trips)
                        .SelectMany(t => t.Segments)
                        .Select(s => s.AirlineCode);
                foreach (var airline in airlines)
                {
                    if (!(new[] { "SJ", "IN" }.Contains(airline)))
                    {
                        response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                        valid = false;
                    }
                }

                if (paymentDetails.Method != PaymentMethod.BankTransfer &&
                    paymentDetails.Method != PaymentMethod.VirtualAccount &&
                    paymentDetails.Method != PaymentMethod.Undefined)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }

                if (DateTime.UtcNow.AddHours(7).DayOfWeek != DayOfWeek.Wednesday)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                if (!valid)
                {
                    return response;
                }
            }

            if (voucher.CampaignId == 68 || voucher.CampaignName == "Kamis Ceria") // Kamis Ceria
            {
                var valid = true;

                var identity = HttpContext.Current.User.Identity as ClaimsIdentity ?? new ClaimsIdentity();
                var clientId = identity.Claims.Single(claim => claim.Type == "Client ID").Value;
                var platform = Client.GetPlatformType(clientId);
                if (platform == PlatformType.AndroidApp || platform == PlatformType.IosApp)
                {
                    response.VoucherStatus = VoucherStatus.PlatformNotEligible;
                    valid = false;
                }


                if ((rsv as FlightReservation).Itineraries.SelectMany(i => i.Trips)
                    .Any(t =>
                        FlightService.GetInstance().GetAirportCountryCode(t.OriginAirport) != "ID" ||
                        FlightService.GetInstance().GetAirportCountryCode(t.DestinationAirport) != "ID"))
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                if (!(new[] { "DPS", "LOP", "LBJ", "BTH", "BTJ" }
                    .Contains((rsv as FlightReservation).Itineraries[0].Trips[0].DestinationAirport)))
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                var airlines =
                    (rsv as FlightReservation).Itineraries.SelectMany(i => i.Trips)
                        .SelectMany(t => t.Segments)
                        .Select(s => s.AirlineCode);
                foreach (var airline in airlines)
                {
                    if (!(new[] { "QG", "SJ", "IN", "ID" }.Contains(airline)))
                    {
                        response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                        valid = false;
                    }
                }


                if (DateTime.UtcNow.AddHours(7).DayOfWeek != DayOfWeek.Thursday)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                if (!valid)
                {
                    return response;
                }
            }

            if (voucher.CampaignId == 72 || voucher.CampaignName == "Jumat Hemat") // Jumat Hemat
            {
                var valid = true;

                var identity = HttpContext.Current.User.Identity as ClaimsIdentity ?? new ClaimsIdentity();
                var userId = identity.Name == "anonymous" ? null : identity.GetUser().Id;
                var userEmail = identity.Name == "anonymous" ? null : identity.GetEmail();
                var rsvs1 = FlightService.GetInstance()
                    .GetOverviewReservationsByUserIdOrEmail(userId, contact.Email, null, null, null, null)
                    .Where(r => r.Payment.Status == PaymentStatus.Settled);
                var rsvs2 = FlightService.GetInstance()
                    .GetOverviewReservationsByUserIdOrEmail(userId, userEmail, null, null, null, null)
                    .Where(r => r.Payment.Status == PaymentStatus.Settled);
                if (!rsvs1.Concat(rsvs2).Any())
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }

                if (DateTime.UtcNow.AddHours(7).DayOfWeek != DayOfWeek.Friday)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }

                if (!valid)
                {
                    return response;
                }
            }

            if (voucher.CampaignId == 69 || voucher.CampaignName == "Jalan-Jalan Sabtu") // Jalan-Jalan Sabtu
            {
                var valid = true;

                var identity = HttpContext.Current.User.Identity as ClaimsIdentity ?? new ClaimsIdentity();
                var clientId = identity.Claims.Single(claim => claim.Type == "Client ID").Value;
                var platform = Client.GetPlatformType(clientId);
                if (platform == PlatformType.AndroidApp || platform == PlatformType.IosApp)
                {
                    response.VoucherStatus = VoucherStatus.PlatformNotEligible;
                    valid = false;
                }


                if ((rsv as FlightReservation).Itineraries.SelectMany(i => i.Trips)
                    .Any(t =>
                        FlightService.GetInstance().GetAirportCountryCode(t.OriginAirport) != "ID" ||
                        FlightService.GetInstance().GetAirportCountryCode(t.DestinationAirport) != "ID"))
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                var airlines =
                    (rsv as FlightReservation).Itineraries.SelectMany(i => i.Trips)
                        .SelectMany(t => t.Segments)
                        .Select(s => s.AirlineCode);
                foreach (var airline in airlines)
                {
                    if (!(new[] { "QG", "SJ", "IN", "ID" }.Contains(airline)))
                    {
                        response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                        valid = false;
                    }
                }


                if (paymentDetails.Method != PaymentMethod.BankTransfer &&
                    paymentDetails.Method != PaymentMethod.VirtualAccount &&
                    paymentDetails.Method != PaymentMethod.Undefined)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                if (DateTime.UtcNow.AddHours(7).DayOfWeek != DayOfWeek.Saturday)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                if (!valid)
                {
                    return response;
                }
            }

            if (voucher.CampaignId == 73 || voucher.CampaignName == "Sunday Funday") // Sunday Funday
            {
                var valid = true;

                if (DateTime.UtcNow.AddHours(7).DayOfWeek != DayOfWeek.Sunday)
                {
                    response.VoucherStatus = VoucherStatus.ReservationNotEligible;
                    valid = false;
                }


                if (!valid)
                {
                    return response;
                }
            }

            //////////////  HARDCODED VALIDATION /////////////////

            response.VoucherStatus = validationStatus;
            return response;
        }

        public VoucherResponse UseVoucherRequest(string rsvNo, string voucherCode, PaymentDetails paymentDetails = null)
        {
            CampaignVoucher voucher;
            var response = ValidateVoucherRequest(rsvNo, voucherCode, out voucher, paymentDetails);
            if (response.VoucherStatus == VoucherStatus.Success)
            {
                var isUseBudgetSuccess = !rsvNo.StartsWith("2") || UseHotelBudget(voucherCode, rsvNo);
                var isVoucherDecrementSuccess = VoucherDecrement(voucherCode);
                if (isUseBudgetSuccess && isVoucherDecrementSuccess)
                {
                    response.VoucherStatus = VoucherStatus.Success;
                    var contact = Contact.GetFromDb(rsvNo);
                    if (voucher.CampaignTypeCd == CampaignTypeCd.Mnemonic(CampaignType.Public))
                        SavePhoneAndEmailInCache(voucherCode, contact.CountryCallingCode + contact.Phone, contact.Email);
                }
                else
                    response.VoucherStatus = VoucherStatus.UpdateError;
            }
            return response;
        }

        private VoucherStatus ValidateVoucher(CampaignVoucher voucher, decimal price, string email, string voucherCode)
        {
            var currentTime = DateTime.Now;
            if (voucher == null)
                return VoucherStatus.VoucherNotFound;
            if (voucher.StartDate >= currentTime)
                return VoucherStatus.CampaignNotStartedYet;
            if (voucher.EndDate <= currentTime)
                return VoucherStatus.CampaignHasEnded;
            if (voucher.RemainingCount < 1)
                return VoucherStatus.NoVoucherRemaining;
            if (voucher.MinSpendValue > price)
                return VoucherStatus.BelowMinimumSpend;
            if (voucher.CampaignTypeCd == CampaignTypeCd.Mnemonic(CampaignType.Member))
            {
                if (!GetDb.IsMember(email))
                    return VoucherStatus.EmailNotEligible;
            }
            if (voucher.CampaignTypeCd == CampaignTypeCd.Mnemonic(CampaignType.Private))
            {
                if (!GetDb.IsEligibleForVoucher(voucherCode, email))
                    return VoucherStatus.EmailNotEligible;
            }
            if (voucher.CampaignStatus == false)
                return VoucherStatus.CampaignInactive;
            if (voucher.IsSingleUsage != null && voucher.IsSingleUsage == true)
            {
                if (GetDb.CheckVoucherUsage(voucherCode, email) > 0)
                    return VoucherStatus.VoucherAlreadyUsed;
            }
            return VoucherStatus.Success;
        }

        private void CalculateVoucherDiscount(CampaignVoucher voucher, decimal price, VoucherResponse response)
        {
            response.OriginalPrice = price;
            response.TotalDiscount = 0;

            if (voucher.ValuePercentage != null && voucher.ValuePercentage > 0)
                response.TotalDiscount += (response.OriginalPrice * (decimal)voucher.ValuePercentage / 100M);

            if (voucher.ValueConstant != null && voucher.ValueConstant > 0)
                response.TotalDiscount += (decimal)voucher.ValueConstant;

            if (voucher.MaxDiscountValue != null && voucher.MaxDiscountValue > 0
                && response.TotalDiscount > voucher.MaxDiscountValue)
                response.TotalDiscount = (decimal)voucher.MaxDiscountValue;

            response.TotalDiscount = Math.Floor(response.TotalDiscount);

            response.DiscountedPrice = response.OriginalPrice - response.TotalDiscount;
            if (response.DiscountedPrice < 50000)
            {
                response.DiscountedPrice = 50000M;
            }
        }

        private bool VoucherDecrement(string voucherCode)
        {
            try
            {
                return UpdateDb.VoucherDecrement(voucherCode);
            }
            catch (Exception)
            {
                return false;
            }
        }

        //private bool VoucherDecrement(string voucherCode)
        //{
        //    try
        //    {
        //        return UpdateDb.VoucherDecrement(voucherCode);
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
        private bool VoucherIncrement(string voucherCode)
        {
            try
            {
                return UpdateDb.VoucherIncrement(voucherCode);
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool UseHotelBudget(string voucherCode, string rsvNo)
        {
            try
            {
                return UpdateDb.UseHotelBudget(voucherCode, rsvNo);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
