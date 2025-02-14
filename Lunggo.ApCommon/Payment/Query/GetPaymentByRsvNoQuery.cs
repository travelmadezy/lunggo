﻿using Lunggo.Framework.Database;
using Lunggo.Repository.TableRecord;

namespace Lunggo.ApCommon.Payment.Query
{
    internal class GetPaymentByRsvNoQuery : DbQueryBase<GetPaymentByRsvNoQuery, PaymentTableRecord>
    {
        protected override string GetQuery(dynamic condition = null)
        {
            return "SELECT Id, MediumCd, MethodCd, StatusCd, Time, TimeLimit, " +
                   "Account, Url, ExternalId, DiscountCode, OriginalPrice, " +
                   "DiscountNominal, Surcharge, UniqueCode, FinalPrice, PaidAmount, " +
                   "LocalCurrencyCd, LocalRate, LocalFinalPrice, LocalPaidAmount," +
                   "InvoiceNo " +
                   "FROM Payment " +
                   "WHERE RsvNo = @RsvNo";
        }
    }
}
