﻿using System.Text;
using Lunggo.ApCommon.Identity.Query.Record;
using Lunggo.Framework.Database;
using Lunggo.Repository.TableRecord;

namespace Lunggo.ApCommon.Identity.Query
{
    public class GetRefreshTokenByIdQuery : DbQueryBase<GetRefreshTokenByIdQuery, RefreshTokenTableRecord>
    {
        protected override string GetQuery(dynamic condition = null)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT * FROM [RefreshToken] WHERE Id = @Id");
            return queryBuilder.ToString();
        }
    }
				
}
