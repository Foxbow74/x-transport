using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using Community.CsharpSqlite.SQLiteClient;
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

		private static readonly Dictionary<Type, Func<object, IStorageValue>> m_tables2Type =
			new Dictionary<Type, Func<object, IStorageValue>>();

		private readonly AutoResetEvent m_autoResetEvent = new AutoResetEvent(true);

		private readonly SqliteConnection m_connection;

		private DbTransaction m_transaction;

		static SQLiteStorage()
		{
			m_type2Tables.Add(typeof (int), "ints");
			m_type2Tables.Add(typeof (Guid), "guids");
			m_type2Tables.Add(typeof (string), "strings");
			m_type2Tables.Add(typeof (DateTime), "dates");
			m_type2Tables.Add(typeof (double), "doubles");
			m_type2Tables.Add(typeof (decimal), "decimals");

			m_tables2Type.Add(typeof (int), _o => new StorageValue<int> {Val = (int) _o});
			m_tables2Type.Add(typeof (Guid), _o => new StorageValue<Guid> {Val = new Guid((string) _o)});
			m_tables2Type.Add(typeof (string), _o => new StorageValue<string> {Val = (string) _o});
			m_tables2Type.Add(typeof (DateTime), _o => new StorageValue<DateTime> {Val = (DateTime) _o});
			m_tables2Type.Add(typeof (double), _o => new StorageValue<double> {Val = (double) _o});
			m_tables2Type.Add(typeof (decimal), _o => new StorageValue<decimal> {Val = Decimal.Parse((string) _o)});
		}

		public SQLiteStorage(string _dbName)
		{
			var cs = string.Format("BinaryGUID=True, uri=file:{0}", _dbName);
			m_connection = new SqliteConnection(cs);
			m_connection.Open();
			CreateTablesIfNotExists();
		}

		private void CreateTablesIfNotExists()
		{
			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS main ( id INTEGER PRIMARY KEY AUTOINCREMENT, uid GUID, parent GUID, kind INTEGER, field INTEGER, vfrom DATETIME NOT NULL, vtill DATETIME)");

			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS ints ( id INTEGER NOT NULL, value INTEGER)");
			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS guids ( id INTEGER NOT NULL, value GUID)");
			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS dates ( id INTEGER NOT NULL, value DATETIME)");
			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS strings ( id INTEGER NOT NULL, value TEXT)");
			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS doubles ( id INTEGER NOT NULL, value REAL)");
			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS decimals ( id INTEGER NOT NULL, value TEXT)");
			ExecuteNonQuery("CREATE TABLE IF NOT EXISTS blobs ( id INTEGER NOT NULL, value BLOB)");
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
			return InsertValueInternal(_lastId, _uid, _field, m_type2Tables[typeof (T)], _value);
		}

		public void Delete(Guid _storedId, int _field, DateTime _now)
		{
			ExecuteInsertOrUpdate("UPDATE main SET vtill=@till WHERE uid=@uid"
			                      , new SqliteParameter("@uid", _storedId)
			                      , new SqliteParameter("@till", DateTime.Now));

			ExecuteNonQuery("UPDATE main SET vtill=@till WHERE parent=@uid"
			                , new SqliteParameter("@uid", _storedId)
			                , new SqliteParameter("@till", DateTime.Now));
		}

		public int InsertMain(Guid _uid, int _kind, DateTime _now, Guid _parent = default(Guid),
		                      int? _field = null)
		{
			if (_parent.Equals(default(Guid)))
			{
				ExecuteInsertOrUpdate(CreateCommand("INSERT INTO main (uid, kind, vfrom) VALUES (@uid, @kind, @from)",
				                                    new SqliteParameter("@uid", _uid),
				                                    new SqliteParameter("@kind", _kind),
				                                    new SqliteParameter("@from", _now)));
			}
			else
			{
				ExecuteInsertOrUpdate(
					CreateCommand("INSERT INTO main (uid, parent, kind, field, vfrom) VALUES (@uid, @parent, @kind, @field, @from)",
					              new SqliteParameter("@uid", _uid),
					              new SqliteParameter("@parent", _parent),
					              new SqliteParameter("@kind", _kind),
					              new SqliteParameter("@field", _field),
					              new SqliteParameter("@from", _now)));
			}
			var id = m_connection.LastInsertRowId;
			return id;
		}

		public IDisposable CreateTransaction()
		{
			return new Transaction(this);
		}

		public IEnumerable<StorageRootObject> LoadRoot()
		{
			var now = DateTime.Now;
			using (
				var rdr = CreateCommand("select * from main where parent IS NULL").ExecuteReader(CommandBehavior.CloseConnection))
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
					yield return new StorageRootObject {Kind = (int) kind, Uid = new Guid((string) uid)};
				}
			}
		}

		public DateTime LoadObjectParameters(Guid _uid, out int _kind)
		{
			using (
				var rdr =
					CreateCommand("select kind, vfrom from main where uid=@uid", new SqliteParameter("@uid", _uid)).ExecuteReader(
						CommandBehavior.CloseConnection))
			{
				while (rdr.Read())
				{
					_kind = (int) rdr[0];
					return (DateTime) rdr[1];
				}
			}
			throw new ApplicationException("Object not found UID=" + _uid);
		}

		public IEnumerable<IStorageRecord> LoadObject(Guid _uid, DateTime _now)
		{
			var vuids = new Dictionary<int, Guid>();
			var vfields = new Dictionary<int, int>();
			using (
				var rdr =
					CreateCommand("select * from main where parent=@parent AND vfrom<=@now AND (vtill IS NULL OR vtill>@now)",
					              new SqliteParameter("@parent", _uid), new SqliteParameter("@now", _now)).ExecuteReader(
					              	CommandBehavior.CloseConnection))
			{
				while (rdr.Read())
				{
					var parent = rdr[MF_PARENT];
					var kind = rdr[MF_KIND];
					var uid = rdr[MF_UID];
					var vform = rdr[MF_FROM];


					if (kind != null)
					{
						yield return new StorageChild
						             	{
						             		Kind = (int) kind,
						             		Uid = new Guid((string) uid),
						             		Parent = new Guid((string) parent),
						             		Field = (int) rdr[MF_FIELD],
						             		ValidFrom = (DateTime) vform,
						             	};
					}
					else
					{
						var id = (int) rdr[F_ID];
						vuids.Add(id, new Guid((string) parent));
						vfields.Add(id, (int) rdr[MF_FIELD]);
					}
				}
			}

			foreach (var table in m_type2Tables)
			{
				using (var rdr = CreateCommand("select * from " + table.Value).ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (rdr.Read())
					{
						var id = (int) rdr[F_ID];
						int field;
						if (vfields.TryGetValue(id, out field))
						{
							var val = m_tables2Type[table.Key](rdr[VF_VALUE]);
							val.Id = id;
							val.Field = field;
							val.Owner = vuids[id];
							val.OldId = (int) rdr[F_ID];
							yield return val;
						}
					}
				}
			}
		}

		public IEnumerable<IStorageRecord> LoadAll()
		{
			var vuids = new Dictionary<int, Guid>();
			var vfields = new Dictionary<int, int>();
			using (
				var rdr =
					CreateCommand("select * from main where vtill IS NULL OR vtill<@now", new SqliteParameter("@now", DateTime.Now)).
						ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (rdr.Read())
				{
					var parent = rdr[MF_PARENT];
					var kind = rdr[MF_KIND];
					var uid = rdr[MF_UID];
					var vform = rdr[MF_FROM];
					if (parent == null)
					{
						yield return new StorageObject {Kind = (int) kind, Uid = new Guid((string) uid), ValidFrom = (DateTime) vform};
					}
					else
					{
						if (kind != null)
						{
							yield return
								new StorageChild
									{
										Kind = (int) kind,
										Uid = new Guid((string) uid),
										Parent = new Guid((string) parent),
										Field = (int) rdr[MF_FIELD],
										ValidFrom = (DateTime) vform,
									};
						}
						else
						{
							var id = (int) rdr[F_ID];
							vuids.Add(id, new Guid((string) parent));
							vfields.Add(id, (int) rdr[MF_FIELD]);
						}
					}
				}
			}

			foreach (var table in m_type2Tables)
			{
				using (var rdr = CreateCommand("select * from " + table.Value).ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (rdr.Read())
					{
						var id = (int) rdr[F_ID];
						int field;
						if (vfields.TryGetValue(id, out field))
						{
							var val = m_tables2Type[table.Key](rdr[VF_VALUE]);
							val.Id = id;
							val.Field = field;
							val.Owner = vuids[id];
							val.OldId = (int) rdr[F_ID];
							yield return val;
						}
					}
				}
			}
		}

		#endregion

		private void RollBack()
		{
			m_transaction.Rollback();
			m_transaction.Dispose();
			m_transaction = null;
		}

		private int InsertValueInternal<T>(int? _lastId, Guid _uid, int _field, string _tbl, T _value)
		{
			var till = DateTime.Now;
			if (_lastId.HasValue)
			{
				ExecuteInsertOrUpdate("UPDATE main SET vtill=@till WHERE id=@lastId", new SqliteParameter("@till", till),
				                      new SqliteParameter("@lastId", _lastId.Value));
			}
			var id = ExecuteInsertOrUpdate(CreateCommand("INSERT INTO main (parent, field, vfrom) VALUES (@uid, @field, @from)",
			                                             new SqliteParameter("@uid", _uid), new SqliteParameter("@field", _field),
			                                             new SqliteParameter("@from", till)));
			using (var cmd = CreateCommand(string.Format("INSERT INTO {0} (id, value) VALUES ({1}, @value)", _tbl, id)))
			{
				cmd.Parameters.Add(new SqliteParameter("@value", _value));
				ExecuteInsertOrUpdate(cmd);
			}
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

		private int ExecuteInsertOrUpdate(SqliteCommand _cmd)
		{
			ExecuteNonQuery(_cmd);
			return _cmd.LastInsertRowID();
		}

		private void ExecuteNonQuery(SqliteCommand _cmd)
		{
			_cmd.ExecuteNonQuery();
			if (_cmd.GetLastErrorCode() != 0)
			{
				RollBack();
				throw new ApplicationException(string.Format("Can't insert:{0}", _cmd.GetLastError()));
			}
		}

		private void ExecuteInsertOrUpdate(string _text = null, params SqliteParameter[] _parameters)
		{
			using (var cmd = CreateCommand(_text, _parameters))
			{
				ExecuteInsertOrUpdate(cmd);
			}
		}

		public int InsertMain(UplodableObject _uplodableObject, Guid _parent = default(Guid), int? _field = null)
		{
			var now = DateTime.Now;

			if (_parent.Equals(default(Guid)))
			{
				ExecuteInsertOrUpdate(CreateCommand("INSERT INTO main (uid, kind, vfrom) VALUES (@uid, @kind, @from)",
				                                    new SqliteParameter("@uid", _uplodableObject.Uid),
				                                    new SqliteParameter("@kind", _uplodableObject.Kind),
				                                    new SqliteParameter("@from", now)));
			}
			else
			{
				ExecuteInsertOrUpdate(
					CreateCommand("INSERT INTO main (uid, parent, kind, field, vfrom) VALUES (@uid, @parent, @kind, @field, @from)",
					              new SqliteParameter("@uid", _uplodableObject.Uid),
					              new SqliteParameter("@parent", _parent),
					              new SqliteParameter("@kind", _uplodableObject.Kind),
					              new SqliteParameter("@field", _field),
					              new SqliteParameter("@from", now)));
			}
			var id = m_connection.LastInsertRowId;
			_uplodableObject.SaveFields(this, now);
			return id;
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