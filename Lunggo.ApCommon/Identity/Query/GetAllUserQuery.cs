﻿using System.Text;
using Lunggo.ApCommon.Identity.Query.Record;
using Lunggo.Framework.Database;

namespace Lunggo.ApCommon.Identity.Query
{

    public class GetAllUserQuery : DbQueryBase<GetAllUserQuery,GetUserByAnyQueryRecord>
    {
       private GetAllUserQuery()
        {

        }

        protected override string GetQuery(dynamic condition = null)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT * FROM [User]");
            return queryBuilder.ToString();
        }
    }
				
}
