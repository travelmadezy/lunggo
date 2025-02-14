﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lunggo.ApCommon.Payment.Model;
using Lunggo.Framework.Database;
using Lunggo.Repository.TableRecord;

namespace Lunggo.ApCommon.Payment.Query
{
    internal class GetUncheckedTransferConfirmationReportsQuery : DbQueryBase<GetUncheckedTransferConfirmationReportsQuery, TransferConfirmationReportTableRecord>
    {
        protected override string GetQuery(dynamic condition = null)
        {
            return "SELECT * FROM TransferConfirmationReport WHERE StatusCd = 'UNC'";
        }
    }
}
