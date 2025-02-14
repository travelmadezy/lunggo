using System;
using System.Collections.Generic;
using Lunggo.Framework.Database;
using Lunggo.Repository.TableRecord;
using System.Data;

namespace Lunggo.Repository.TableRepository
{
	public class VoucherRecipientsTableRepo : TableDao<VoucherRecipientsTableRecord>, IDbTableRepository<VoucherRecipientsTableRecord> 
    {
		private static readonly VoucherRecipientsTableRepo Instance = new VoucherRecipientsTableRepo("VoucherRecipients");
        
        private VoucherRecipientsTableRepo(String tableName) : base(tableName)
        {
            ;
        }

		public static VoucherRecipientsTableRepo GetInstance()
        {
            return Instance;
        }

        public int Insert(IDbConnection connection, VoucherRecipientsTableRecord record)
        {
            return Insert(connection, record, CommandDefinition.GetDefaultDefinition());
        }

        public int Delete(IDbConnection connection, VoucherRecipientsTableRecord record)
        {
            return Delete(connection, record, CommandDefinition.GetDefaultDefinition());
        }

		public int Update(IDbConnection connection, VoucherRecipientsTableRecord record)
        {
            return Update(connection, record, CommandDefinition.GetDefaultDefinition());
        }

		public VoucherRecipientsTableRecord Find1(IDbConnection connection, VoucherRecipientsTableRecord record)
        {
            return Find1(connection, record, CommandDefinition.GetDefaultDefinition());
        }

		public IEnumerable<VoucherRecipientsTableRecord> Find(IDbConnection connection, VoucherRecipientsTableRecord record)
        {
            return Find(connection, record, CommandDefinition.GetDefaultDefinition());
        }

        public IEnumerable<VoucherRecipientsTableRecord> FindAll(IDbConnection connection)
        {
            return FindAll(connection, CommandDefinition.GetDefaultDefinition());
        }

        public int DeleteAll(IDbConnection connection)
        {
            return DeleteAll(connection, CommandDefinition.GetDefaultDefinition());
        }

        public int Insert(IDbConnection connection, VoucherRecipientsTableRecord record, CommandDefinition definition)
        {
            return InsertInternal(connection, record, definition);
        }

        public int Delete(IDbConnection connection, VoucherRecipientsTableRecord record, CommandDefinition definition)
        {
            return DeleteInternal(connection, record, definition);
        }

        public int Update(IDbConnection connection, VoucherRecipientsTableRecord record, CommandDefinition definition)
        {
            return UpdateInternal(connection, record, definition);
        }

		public VoucherRecipientsTableRecord Find1(IDbConnection connection, VoucherRecipientsTableRecord record, CommandDefinition definition)
        {
			return Find1Internal(connection, record, definition);
        }

		public IEnumerable<VoucherRecipientsTableRecord> Find(IDbConnection connection, VoucherRecipientsTableRecord record, CommandDefinition definition)
        {
			return FindInternal(connection, record, definition);
        }

        public int DeleteAll(IDbConnection connection, CommandDefinition definition)
        {
            return DeleteAllInternal(connection, definition);
        }

        public IEnumerable<VoucherRecipientsTableRecord> FindAll(IDbConnection connection, CommandDefinition definition)
        {
            return FindAllInternal(connection, definition);
        }
	}	
}