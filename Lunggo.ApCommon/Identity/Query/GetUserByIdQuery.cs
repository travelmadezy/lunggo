﻿using System.Text;
using Lunggo.ApCommon.Identity.Query.Record;
using Lunggo.Framework.Database;

namespace Lunggo.ApCommon.Identity.Query
{
    public class GetUserByIdQuery : DbQueryBase<GetUserByIdQuery, GetUserByAnyQueryRecord>
    {
        private GetUserByIdQuery()
        {
            
        }

        protected override string GetQuery(dynamic condition = null)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT * FROM [User] WHERE Id = @Id");
            return queryBuilder.ToString();
        }
    }
				
}
