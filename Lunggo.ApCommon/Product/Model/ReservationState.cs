﻿using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Lunggo.ApCommon.Identity.Auth;
using Lunggo.ApCommon.Payment.Model;
using Lunggo.ApCommon.Product.Constant;
using Lunggo.Framework.Context;
using Lunggo.Framework.Database;
using Lunggo.Repository.TableRecord;
using Lunggo.Repository.TableRepository;

namespace Lunggo.ApCommon.Product.Model
{
    public class ReservationState
    {
        public PlatformType Platform { get; set; }
        public string DeviceId { get; set; }
        public string Language { get; set; }
        public Currency Currency { get; set; }

        internal void GenerateNew()
        {
            var identity = HttpContext.Current.User.Identity as ClaimsIdentity ?? new ClaimsIdentity();
            var clientId = identity.Claims.Single(claim => claim.Type == "Client ID").Value;
            var platform = Client.GetPlatformType(clientId);
            var deviceId = identity.Claims.Single(claim => claim.Type == "Device ID").Value;
            Platform = platform;
            DeviceId = deviceId;
            Language = "id"; //OnlineContext.GetActiveLanguageCode();
            Currency = new Currency("IDR"); //OnlineContext.GetActiveCurrencyCode());
        }

        internal void InsertToDb(string rsvNo)
        {
            using (var conn = DbService.GetInstance().GetOpenConnection())
            {
                ReservationStateTableRepo.GetInstance().Insert(conn, new ReservationStateTableRecord
                {
                    RsvNo = rsvNo,
                    PlatformCd = PlatformTypeCd.Mnemonic(Platform),
                    DeviceId = DeviceId,
                    LanguageCd = Language,
                    CurrencyCd = Currency,
                    InsertBy = "LunggoSystem",
                    InsertDate = DateTime.UtcNow,
                    InsertPgId = "0"
                });
            }
        }

        internal static ReservationState GetFromDb(string rsvNo)
        {
            using (var conn = DbService.GetInstance().GetOpenConnection())
            {
                var record = GetReservationStateQuery.GetInstance().Execute(conn, new { RsvNo = rsvNo }).SingleOrDefault();

                if (record == null)
                    return null;

                return new ReservationState
                {
                    Platform = PlatformTypeCd.Mnemonic(record.PlatformCd),
                    DeviceId = record.DeviceId,
                    Language = record.LanguageCd,
                    Currency = new Currency(record.CurrencyCd)
                };
            }
        }

        private class GetReservationStateQuery : DbQueryBase<GetReservationStateQuery, ReservationStateTableRecord>
        {
            protected override string GetQuery(dynamic condition = null)
            {
                return "SELECT PlatformCd, DeviceId, LanguageCd, CurrencyCd " +
                       "FROM ReservationState " +
                       "WHERE RsvNo = @RsvNo";
            }
        }
    }
}
