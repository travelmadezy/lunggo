using System;
using System.Collections.Generic;
using System.Linq;
using Lunggo.Framework.Database;

namespace Lunggo.Repository.TableRecord
{
    public class UserClaimTableRecord : Lunggo.Framework.Database.TableRecord
    {
		private static List<ColumnMetadata> _recordMetadata;
        private static List<ColumnMetadata> _primaryKeys;
        private static String _tableName;

		public int? Id
		{
		    get { return _Id; }
		    set
		    {
		        _Id = value;
		        IncrementLog("Id");
		    }
		}
		public String UserId
		{
		    get { return _UserId; }
		    set
		    {
		        _UserId = value;
		        IncrementLog("UserId");
		    }
		}
		public String ClaimType
		{
		    get { return _ClaimType; }
		    set
		    {
		        _ClaimType = value;
		        IncrementLog("ClaimType");
		    }
		}
		public String ClaimValue
		{
		    get { return _ClaimValue; }
		    set
		    {
		        _ClaimValue = value;
		        IncrementLog("ClaimValue");
		    }
		}

		
		private int? _Id;
		private String _UserId;
		private String _ClaimType;
		private String _ClaimValue;


		public static UserClaimTableRecord CreateNewInstance()
        {
            var record = new UserClaimTableRecord();
            var iRecord = record.AsInterface();
            iRecord.ManuallyCreated = true;
            return record;
        }

		public UserClaimTableRecord()
        {
            ;
        }

        static UserClaimTableRecord()
        {
            InitTableName();
            InitRecordMetadata();
            InitPrimaryKeysMetadata();
        }

        private static void InitTableName()
        {
            _tableName = "UserClaim";
        }

        private static void InitRecordMetadata()
        {
            _recordMetadata = new List<ColumnMetadata>
            {
				new ColumnMetadata("Id", true),
				new ColumnMetadata("UserId", false),
				new ColumnMetadata("ClaimType", false),
				new ColumnMetadata("ClaimValue", false),

            };
        }

        private static void InitPrimaryKeysMetadata()
        {
            _primaryKeys = _recordMetadata.Where(p => p.IsPrimaryKey).ToList();
        }

		public override List<ColumnMetadata> GetMetadata()
        {
            return _recordMetadata;
        }

        public override string GetTableName()
        {
            return _tableName;
        }

        public override List<ColumnMetadata> GetPrimaryKeys()
        {
            return _primaryKeys;
        }



    }
}
