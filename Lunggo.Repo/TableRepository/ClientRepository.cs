using System;
using System.Collections.Generic;
using Lunggo.Framework.Database;
using Lunggo.Repository.TableRecord;
using System.Data;

namespace Lunggo.Repository.TableRepository
{
	public class ClientTableRepo : TableDao<ClientTableRecord>, IDbTableRepository<ClientTableRecord> 
    {
		private static readonly ClientTableRepo Instance = new ClientTableRepo("Client");
        
        private ClientTableRepo(String tableName) : base(tableName)
        {
            ;
        }

		public static ClientTableRepo GetInstance()
        {
            return Instance;
        }

        public int Insert(IDbConnection connection, ClientTableRecord record)
        {
            return Insert(connection, record, CommandDefinition.GetDefaultDefinition());
        }

        public int Delete(IDbConnection connection, ClientTableRecord record)
        {
            return Delete(connection, record, CommandDefinition.GetDefaultDefinition());
        }

		public int Update(IDbConnection connection, ClientTableRecord record)
        {
            return Update(connection, record, CommandDefinition.GetDefaultDefinition());
        }

		public ClientTableRecord Find1(IDbConnection connection, ClientTableRecord record)
        {
            return Find1(connection, record, CommandDefinition.GetDefaultDefinition());
        }

		public IEnumerable<ClientTableRecord> Find(IDbConnection connection, ClientTableRecord record)
        {
            return Find(connection, record, CommandDefinition.GetDefaultDefinition());
        }

        public IEnumerable<ClientTableRecord> FindAll(IDbConnection connection)
        {
            return FindAll(connection, CommandDefinition.GetDefaultDefinition());
        }

        public int DeleteAll(IDbConnection connection)
        {
            return DeleteAll(connection, CommandDefinition.GetDefaultDefinition());
        }

        public int Insert(IDbConnection connection, ClientTableRecord record, CommandDefinition definition)
        {
            return InsertInternal(connection, record, definition);
        }

        public int Delete(IDbConnection connection, ClientTableRecord record, CommandDefinition definition)
        {
            return DeleteInternal(connection, record, definition);
        }

        public int Update(IDbConnection connection, ClientTableRecord record, CommandDefinition definition)
        {
            return UpdateInternal(connection, record, definition);
        }

		public ClientTableRecord Find1(IDbConnection connection, ClientTableRecord record, CommandDefinition definition)
        {
			return Find1Internal(connection, record, definition);
        }

		public IEnumerable<ClientTableRecord> Find(IDbConnection connection, ClientTableRecord record, CommandDefinition definition)
        {
			return FindInternal(connection, record, definition);
        }

        public int DeleteAll(IDbConnection connection, CommandDefinition definition)
        {
            return DeleteAllInternal(connection, definition);
        }

        public IEnumerable<ClientTableRecord> FindAll(IDbConnection connection, CommandDefinition definition)
        {
            return FindAllInternal(connection, definition);
        }
	}	
}