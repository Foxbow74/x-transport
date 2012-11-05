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
			CreateCommand("CREATE TABLE IF NOT EXISTS main ( id INTEGER PRIMARY KEY AUTOINCREMENT, uid GUID, parent GUID, kind INTEGER, field INTEGER, vfrom DATETIME NOT NULL, vtill DATETIME)").ExecuteNonQuery();
			CreateCommand("CREATE TABLE IF NOT EXISTS ints ( id INTEGER NOT NULL, value INTEGER)").ExecuteNonQuery();
			CreateCommand("CREATE TABLE IF NOT EXISTS guids ( id INTEGER NOT NULL, value GUID)").ExecuteNonQuery();
			CreateCommand("CREATE TABLE IF NOT EXISTS dates ( id INTEGER NOT NULL, value DATETIME)").ExecuteNonQuery();
			CreateCommand("CREATE TABLE IF NOT EXISTS strings ( id INTEGER NOT NULL, value TEXT)").ExecuteNonQuery();
			CreateCommand("CREATE TABLE IF NOT EXISTS doubles ( id INTEGER NOT NULL, value REAL)").ExecuteNonQuery();
			CreateCommand("CREATE TABLE IF NOT EXISTS decimals ( id INTEGER NOT NULL, value TEXT)").ExecuteNonQuery();
			CreateCommand("CREATE TABLE IF NOT EXISTS blobs ( id INTEGER NOT NULL, value BLOB)").ExecuteNonQuery();

			CreateCommand("CREATE INDEX IF NOT EXISTS  main_idx1 ON main (id)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  main_idx2 ON main (uid)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  main_idx3 ON main (parent)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  ints_idx ON ints (id)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  strings_idx ON strings (id)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  guids_idx ON guids (id)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  dates_idx ON dates (id)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  doubles_idx ON doubles (id)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  decimals_idx ON decimals (id)").ExecuteNonQuery();
			CreateCommand("CREATE INDEX IF NOT EXISTS  blobs_idx ON blobs (id)").ExecuteNonQuery();
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

		public void Delete(Guid _uid, int _field, DateTime _now)
		{
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

		public AbstractXServer.ObjectDescriptor LoadObjectCharacteristics(Guid _uid, DateTime _now = default(DateTime))
		{
			if(_now==default(DateTime))
			{
				using (var rdr = CreateCommand("select kind, vfrom, vtill from main where uid=@uid", new SqliteParameter("@uid", _uid)).ExecuteReader(CommandBehavior.CloseConnection))
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

		public IEnumerable<IStorageRecord> LoadObject(Guid _uid, DateTime _now)
		{
			var vuids = new Dictionary<int, Guid>();
			var vfields = new Dictionary<int, int>();
			using (var rdr = CreateCommand("select * from main where parent=@parent AND vfrom<=@now AND (vtill IS NULL OR vtill>@now)", new SqliteParameter("@parent", _uid), new SqliteParameter("@now", _now)).ExecuteReader(
					              	CommandBehavior.CloseConnection))
			{
				while (rdr.Read())
				{
					var parent = rdr[MF_PARENT];
					var kind = rdr[MF_KIND];
					var uid = rdr[MF_UID];
					var vform = rdr[MF_FROM];
					var vtill = rdr[MF_TILL];


					if (kind != null)
					{
						yield return new StorageChild
						             	{
						             		Kind = (int) kind,
						             		Uid = new Guid((string) uid),
						             		Parent = new Guid((string) parent),
						             		Field = (int) rdr[MF_FIELD],
											ValidFrom = (DateTime)vform,
											ValidTill = (DateTime?)vtill,
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

			foreach (var vfield in vfields)
			{
				foreach (var table in m_type2Tables)
				{
					var obj = CreateCommand("select value from " + table.Value + " where id=" + vfield.Key).ExecuteScalar();
					if (obj != null)
					{
						var id = vfield.Key;
						var val = m_tables2Type[table.Key](obj);
						val.Id = vfield.Key;
						val.Field = vfield.Value;
						val.Owner = vuids[id];
						val.OldId = id;
						yield return val;
						break;
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
			var id = ExecuteInsertOrUpdate("INSERT INTO main (parent, field, vfrom) VALUES (@uid, @field, @from)",
			                                             new SqliteParameter("@uid", _uid), new SqliteParameter("@field", _field),
			                                             new SqliteParameter("@from", till));

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
			//Debug.WriteLine(affected + "\t" + _cmd.CommandText);
			//foreach (SqliteParameter parameter in _cmd.Parameters)
			//{
			//    Debug.WriteLine("\t" + parameter.ParameterName + "\t" + parameter.Value);
			//}
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