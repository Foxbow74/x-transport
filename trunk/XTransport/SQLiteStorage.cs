using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using Community.CsharpSqlite.SQLiteClient;
using XTransport.Server;
using XTransport.Server.Storage;

namespace XTransport
{
	public class SQLiteStorage : IStorage
	{
		private const int F_ID = 0;
		private const int VF_VALUE = 1;
		private const int MF_UID = 1;
		private const int MF_PARENT = 2;
		private const int MF_KIND = 3;
		private const int MF_FIELD = 4;
		private const int MF_FROM = 5;
		private const int MF_TILL = 6;

		private static readonly Dictionary<Type, string> m_type2Tables = new Dictionary<Type, string>();
		private static readonly Dictionary<int, Type> m_typeIndex2Type = new Dictionary<int, Type>();
		private static readonly Dictionary<Type, int> m_type2TypeIndex = new Dictionary<Type, int>();
		private static readonly Dictionary<Type, Func<object, IStorageValue>> m_tables2Type = new Dictionary<Type, Func<object, IStorageValue>>();

		readonly AutoResetEvent m_autoResetEvent = new AutoResetEvent(true);

		private readonly SqliteConnection m_connection;

		private DbTransaction m_transaction;

		private void RegisterTypes()
		{
            RegisterType(_o => 0!=(int)_o, SQL_TYPE_INTEGER, "bools", -1);
            RegisterType(_o => (long)_o, SQL_TYPE_INTEGER, "longs", -2);
            RegisterType(_o => (int)_o, SQL_TYPE_INTEGER, "ints", -3);
            RegisterType(_o => (byte)(int)_o, SQL_TYPE_INTEGER, "bytes", -4);
            RegisterType(_o => (short)_o, SQL_TYPE_INTEGER, "shorts", -5);
            RegisterType(_o => new Guid((string)_o), SQL_TYPE_GUID, "guids", -6);
            RegisterType(_o => (string) _o, SQL_TYPE_TEXT, "strings", -7);
            RegisterType(_o => (DateTime)_o, SQL_TYPE_DATETIME, "dates",-8);
            RegisterType(_o => (_o is int)?(int)_o:(double) _o, SQL_TYPE_REAL, "doubles", -9);
            RegisterType(_o => (float)(double)_o, SQL_TYPE_REAL, "floats", -10);
			RegisterType(_o => (decimal)_o, SQL_TYPE_REAL, "decimals", -11);
			RegisterType(_o => (byte[])_o, SQL_TYPE_BLOB, "blobs", -12);
		}

		private const string SQL_TYPE_INTEGER = "INTEGER";
		private const string SQL_TYPE_GUID = "GUID";
		private const string SQL_TYPE_DATETIME = "DATETIME";
		private const string SQL_TYPE_TEXT = "TEXT";
		private const string SQL_TYPE_REAL = "REAL";
		private const string SQL_TYPE_BLOB = "BLOB";

		private void RegisterType<T>(Func<object, T> _func, string _sqliteType, string _name, int _typeIndex)
		{
			m_type2Tables[typeof(T)] = _name;
			m_type2TypeIndex[typeof(T)] = _typeIndex;
			m_typeIndex2Type[_typeIndex] = typeof(T);
			m_tables2Type[typeof(T)] = _o => new StorageValue<T> { Val = _func(_o) };
			CreateCommand("CREATE TABLE IF NOT EXISTS " + _name + " ( id " + _sqliteType + " NOT NULL, value INTEGER)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  " + _name + "_idx ON " + _name + " (id)").ExecuteNonQuery();
		}

		static readonly List<string> m_initialized = new List<string>();

		public SQLiteStorage(string _dbName)
		{
			var cs = string.Format("BinaryGUID=True, uri=file:{0}", _dbName);
			m_connection = new SqliteConnection(cs);
			m_connection.Open();

			//if (!m_initialized.Contains(_dbName))
			{
				CreateCommand("CREATE TABLE IF NOT EXISTS main ( id INTEGER PRIMARY KEY AUTOINCREMENT, uid GUID, parent GUID, kind INTEGER, field INTEGER, vfrom DATETIME NOT NULL, vtill DATETIME)").ExecuteNonQuery();

				CreateCommand("CREATE INDEX IF NOT EXISTS  main_idx1 ON main (id)").ExecuteNonQuery();
				CreateCommand("CREATE INDEX IF NOT EXISTS  main_idx2 ON main (uid)").ExecuteNonQuery();
				CreateCommand("CREATE INDEX IF NOT EXISTS  main_idx3 ON main (parent)").ExecuteNonQuery();

				RegisterTypes();

				m_initialized.Add(_dbName);
			}
		}

        public void DeleteAll()
        {
            ExecuteNonQuery("DELETE FROM main");
            foreach (var table in m_type2Tables.Values)
            {
                ExecuteNonQuery("DELETE FROM " + table);
            }
        }

	    public void Shrink()
	    {
            ExecuteNonQuery("VACUUM");
	    }

	    #region IStorage Members

		public void Dispose()
		{
			if (m_connection.State != ConnectionState.Closed)
			{
                m_connection.Close();
                m_connection.Dispose();
			}
		}

		int IStorage.InsertValue<T>(Guid _uid, int _field, T _value, int? _lastId, DateTime _now)
		{
			return InsertValueInternal(_lastId, _uid, _field, m_type2Tables[typeof (T)], _value, m_type2TypeIndex[typeof(T)]);
		}

		public void Delete(Guid _uid, int _field, DateTime _now)
		{
            Debug.WriteLine("DELETED: " + _uid);
			ExecuteNonQuery("UPDATE main SET vtill=@till WHERE uid=@uid"
			                , new SqliteParameter("@uid", _uid)
							, new SqliteParameter("@till", _now));
		}

		public int InsertMain(Guid _uid, int _kind, DateTime _now, Guid _parent = default(Guid),
		                      int? _field = null)
		{
			if (_parent.Equals(default(Guid)))
			{
				return ExecuteInsertOrUpdate("INSERT INTO main (uid, kind, vfrom) VALUES (@uid, @kind, @from)",
				                                    new SqliteParameter("@uid", _uid),
				                                    new SqliteParameter("@kind", _kind),
				                                    new SqliteParameter("@from", _now));
			}
			
			return ExecuteInsertOrUpdate("INSERT INTO main (uid, parent, kind, field, vfrom) VALUES (@uid, @parent, @kind, @field, @from)",
			                             new SqliteParameter("@uid", _uid),
			                             new SqliteParameter("@parent", _parent),
			                             new SqliteParameter("@kind", _kind),
			                             new SqliteParameter("@field", _field),
			                             new SqliteParameter("@from", _now));
		}

		public IDisposable CreateTransaction()
		{
			return new Transaction(this);
		}

		public Guid GetCollectionOwnerUid(Guid _uid)
		{
			var scalar = (string)CreateCommand("SELECT parent FROM main WHERE uid=@uid", new SqliteParameter("@uid", _uid)).ExecuteScalar();
			return new Guid(scalar);
		}

	    public IEnumerable<StorageRootObject> LoadRoot()
		{
			var lst = new List<StorageRootObject>();
			var now = DateTime.Now;
			using (var rdr = CreateCommand("select * from main where parent IS NULL").ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (rdr.Read())
				{
					var till = rdr[MF_TILL];
					if (till != null)
					{
						if (rdr.GetDateTime(MF_TILL) < now) continue;
					}
					var kind = rdr[MF_KIND];
					var uid = rdr[MF_UID];
					lst.Add(new StorageRootObject {Kind = (int) kind, Uid = new Guid((string) uid)});
				}
			}
	    	return lst;
		}

		public AbstractXServer.ObjectDescriptor LoadObjectCharacteristics(Guid _uid, DateTime _now = default(DateTime))
		{
			if(_now==default(DateTime))
			{
				using (var rdr = CreateCommand("select kind, vfrom, vtill from main where uid=@uid and vtill IS NULL", new SqliteParameter("@uid", _uid)).ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (rdr.Read())
					{
						var result = new AbstractXServer.ObjectDescriptor((int)rdr[0], (DateTime)rdr[1], (DateTime?)rdr[2]);
						return result;
					}
				}
			}
			else
			{
				using (var rdr = CreateCommand("select kind, vfrom, vtill from main where uid=@uid and @now>=vfrom and @now<vtill", new SqliteParameter("@uid", _uid), new SqliteParameter("@now", _now)).ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (rdr.Read())
					{
						var result = new AbstractXServer.ObjectDescriptor((int)rdr[0], (DateTime)rdr[1], (DateTime?)rdr[2]);
						return result;
					}
				}				
			}
			throw new ApplicationException("Object not found UID=" + _uid);
		}

		public IEnumerable<IStorageRecord> LoadObject(Guid _uid, DateTime _now=default(DateTime))
		{
			var vuids = new Dictionary<int, Guid>();
			var vfields = new Dictionary<int, int>();
			var vfieldType = new Dictionary<int, int>();
			SqliteCommand cmd;
			if(_now==default(DateTime))
			{
				cmd = CreateCommand("select * from main where parent=@parent AND vtill IS NULL", new SqliteParameter("@parent", _uid), new SqliteParameter("@now", _now));
			}
			else
			{
				cmd = CreateCommand("select * from main where parent=@parent AND vfrom<=@now AND (vtill IS NULL OR vtill<@now)", new SqliteParameter("@parent", _uid), new SqliteParameter("@now", _now));
			}
			
			using (var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (rdr.Read())
				{
					var parent = rdr[MF_PARENT];
					var preKind = rdr[MF_KIND];
					var kind = preKind==null?0:(int)preKind;
					var uid = rdr[MF_UID];
					var vform = rdr[MF_FROM];
					var vtill = rdr[MF_TILL];


					if (kind>0)
					{
						yield return new StorageChild
							             {
								             Kind = kind,
								             Uid = new Guid((string) uid),
								             Parent = new Guid((string) parent),
								             Field = (int) rdr[MF_FIELD],
								             ValidFrom = (DateTime) vform,
								             ValidTill = (DateTime?) vtill,
							             };
					}
					else
					{
						var id = (int) rdr[F_ID];
						vuids.Add(id, new Guid((string) parent));
						vfields.Add(id, (int) rdr[MF_FIELD]);
						vfieldType.Add(id, kind);
					}
				}
			}

			foreach (var vfield in vfields)
			{
				var type = m_typeIndex2Type[vfieldType[vfield.Key]];
				var obj = CreateCommand(string.Format("select value from {0} where id={1} limit 1", m_type2Tables[type], vfield.Key)).ExecuteScalar();
				var id = vfield.Key;
				var val = m_tables2Type[type](obj);
				val.Id = vfield.Key;
				val.Field = vfield.Value;
				val.Owner = vuids[id];
				val.OldId = id;
				yield return val;
			}
		}

		#endregion

		private void RollBack()
		{
			m_transaction.Rollback();
			m_transaction.Dispose();
			m_transaction = null;
		}

		private int InsertValueInternal<T>(int? _lastId, Guid _uid, int _field, string _tbl, T _value, int _typeIndex)
		{
			var till = DateTime.Now;
			if (_lastId.HasValue)
			{
				ExecuteInsertOrUpdate("UPDATE main SET vtill=@till WHERE id=@lastId", new SqliteParameter("@till", till),
				                      new SqliteParameter("@lastId", _lastId.Value));
			}
			var id = ExecuteInsertOrUpdate("INSERT INTO main (parent, field, vfrom, kind) VALUES (@uid, @field, @from, @kind)",
			                                             new SqliteParameter("@uid", _uid), 
														 new SqliteParameter("@field", _field),
			                                             new SqliteParameter("@from", till),
														 new SqliteParameter("@kind", _typeIndex));

			ExecuteInsertOrUpdate(string.Format("INSERT INTO {0} (id, value) VALUES ({1}, @value)", _tbl, id), new SqliteParameter("@value", _value));
			return id;
		}

		private SqliteCommand CreateCommand(string _text = null, params SqliteParameter[] _parameters)
		{
			var command = new SqliteCommand {Connection = m_connection, Transaction = m_transaction, CommandText = _text};
			command.Parameters.AddRange(_parameters);
			return command;
		}

		private void ExecuteNonQuery(string _text = null, params SqliteParameter[] _parameters)
		{
			using (var cmd = CreateCommand(_text, _parameters))
			{
				ExecuteNonQuery(cmd);
			}
		}

		private int ExecuteInsertOrUpdate(string _text = null, params SqliteParameter[] _parameters)
		{
			using (var cmd = CreateCommand(_text, _parameters))
			{
				ExecuteNonQuery(cmd);
				return cmd.LastInsertRowID();
			}
		}

		private int ExecuteNonQuery(SqliteCommand _cmd)
		{
			var affected = _cmd.ExecuteNonQuery();
			if (_cmd.GetLastErrorCode() != 0)
			{
				RollBack();
				throw new ApplicationException(string.Format("Can't insert:{0}", _cmd.GetLastError()));
			}
			return affected;
		}

	    public int InsertMain(UplodableObject _uplodableObject, Guid _parent = default(Guid), int? _field = null)
		{
			var now = DateTime.Now;
			int result;
			if (_parent.Equals(default(Guid)))
			{
				result = ExecuteInsertOrUpdate("INSERT INTO main (uid, kind, vfrom) VALUES (@uid, @kind, @from)",
				                                    new SqliteParameter("@uid", _uplodableObject.Uid),
				                                    new SqliteParameter("@kind", _uplodableObject.Kind),
				                                    new SqliteParameter("@from", now));
			}
			else
			{
				result = ExecuteInsertOrUpdate("INSERT INTO main (uid, parent, kind, field, vfrom) VALUES (@uid, @parent, @kind, @field, @from)",
					              new SqliteParameter("@uid", _uplodableObject.Uid),
					              new SqliteParameter("@parent", _parent),
					              new SqliteParameter("@kind", _uplodableObject.Kind),
					              new SqliteParameter("@field", _field),
					              new SqliteParameter("@from", now));
			}
			_uplodableObject.SaveFields(this, now);
			return result;
		}

		#region Nested type: Transaction

		private class Transaction : IDisposable
		{
			private readonly SQLiteStorage m_storage;

			public Transaction(SQLiteStorage _storage)
			{
				m_storage = _storage;
                _storage.m_autoResetEvent.WaitOne();
                _storage.m_transaction = _storage.m_connection.BeginTransaction();
			}

			#region IDisposable Members

			public void Dispose()
			{
				if (m_storage.m_transaction != null)
				{
					m_storage.m_transaction.Commit();
					m_storage.m_transaction.Dispose();
                    m_storage.m_transaction = null;
				}
                m_storage.m_autoResetEvent.Set();
			}

			#endregion
		}

		#endregion
	}
}