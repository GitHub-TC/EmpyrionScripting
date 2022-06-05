using EmpyrionNetAPIDefinitions;
using EmpyrionScripting.Interface;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EmpyrionScripting
{
    public class SqlDynamicModel : DynamicObject
    {
        private readonly Dictionary<string, object> properties;

        public SqlDynamicModel(Dictionary<string, object> properties)
        {
            this.properties = properties;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (properties.ContainsKey(binder.Name))
            {
                result = properties[binder.Name];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames() => properties.Keys;
    }

    public class SqlDbAccess
    {
        public Action<string, LogLevel> Log { get; set; }
        public string SaveGamePath {
            set {
                ConnectionString = new SqliteConnectionStringBuilder()
                {
                    Mode       = SqliteOpenMode.ReadOnly,
                    Cache      = SqliteCacheMode.Shared,
                    DataSource = Path.Combine(value, "..", "global.db")
                }.ToString();

                Log($"DB ConnectionString:{ConnectionString}", LogLevel.Message);
            }
        }

        public string ConnectionString { get; set; }
        public Dictionary<string, DBQuery> PlayerQueries { get; set; }
        public Dictionary<string, DBQuery> ElevatedQueries { get; set; }
        ConcurrentDictionary<string, CacheResult> CacheQuery { get; } = new ConcurrentDictionary<string, CacheResult>();
        public TimeSpan OverallQueryTime { get; set; } = TimeSpan.Zero;
        public int QueryCounter = 0;

        class CacheResult {
            public DateTime Expired { get; set; }
            public List<Dictionary<string, object>> Data { get; set; }
        }

        public List<Dictionary<string,object>> ReadData(string queryName, bool isElevatedAllowed, int top, string orderBy, string additionalWhereAnd, Dictionary<string, object> parameters)
        {
            var cacheKey = $"{queryName}{top}{isElevatedAllowed}{additionalWhereAnd}{orderBy}{parameters.Aggregate(string.Empty, (l, p) => $"{l}{p.Key}{p.Value}")}";

            if (CacheQuery.TryGetValue(cacheKey, out var result) && result.Expired >= DateTime.Now) return result.Data;

            DBQuery query = null;
            if ((!isElevatedAllowed || ElevatedQueries?.TryGetValue(queryName, out query) == false) && PlayerQueries?.TryGetValue(queryName, out query) == false) return null;

            var valuesRows = new List<Dictionary<string, object>>();
            var stopwatch = Stopwatch.StartNew();

            using (var DbConnection = new SQLiteConnection(ConnectionString))
            {
                DbConnection.Open();

                using (var cmd = new SQLiteCommand(DbConnection))
                {
                    var andCustomWhere = string.Empty;
                    if (!string.IsNullOrEmpty(additionalWhereAnd) && !additionalWhereAnd.Contains("/*") && !additionalWhereAnd.Contains("--"))
                    {
                        andCustomWhere = query.Query.IndexOf("WHERE", StringComparison.InvariantCultureIgnoreCase) > 0
                            ? $" AND ({additionalWhereAnd})"
                            : $" WHERE {additionalWhereAnd}";
                    }

                    var sqlQuery = new StringBuilder(query.Query.Replace("{additionalWhereAnd}", andCustomWhere));

                    if (!string.IsNullOrEmpty(orderBy))
                    {
                        var checkOrderBy = orderBy.Split(' ');
                        sqlQuery.AppendFormat("\nORDER BY {0}{1}", checkOrderBy.First(), checkOrderBy.Skip(1).FirstOrDefault()?.ToLower() == "desc" ? " desc" : " asc");
                    }

                    cmd.CommandText = sqlQuery.ToString();
                    parameters?.ForEach(p => cmd.Parameters.AddWithValue(p.Key, p.Value));

                    Log($"SQL[{queryName}]: {cmd.CommandText}{parameters?.Aggregate("", (l, p) => $"{l}\n{p.Key} -> {p.Value}")}", LogLevel.Debug);

                    using (var reader = cmd.ExecuteReader())
                    {
                        for (int i = top - 1; i >= 0 && reader.Read(); i--)
                        {
                            var values = new Dictionary<string, object>();
                            for (int fieldIndex = reader.FieldCount - 1; fieldIndex >= 0; fieldIndex--)
                            {
                                var fieldName = reader.GetOriginalName(fieldIndex);
                                var tableName = reader.GetTableName(fieldIndex);

                                if (values.TryGetValue(tableName, out var subTable)) ((Dictionary<string, object>)subTable)                .Add(fieldName, reader.GetValue(fieldIndex));
                                else                                                 values.Add(tableName, new Dictionary<string, object>() { { fieldName, reader.GetValue(fieldIndex) } });
                            }
                            valuesRows.Add(values);
                        }
                    }
                }

                DbConnection.Close();
            }

            CacheQuery.AddOrUpdate(cacheKey, 
                k       => new CacheResult { Data = valuesRows, Expired = DateTime.Now + TimeSpan.FromSeconds(query.CacheQueryForSeconds)}, 
                (k, d)  => new CacheResult { Data = valuesRows, Expired = DateTime.Now + TimeSpan.FromSeconds(query.CacheQueryForSeconds) });

            stopwatch.Stop();
            Log($"SQL:[{queryName}] takes {stopwatch.Elapsed}", LogLevel.Debug);

            OverallQueryTime += stopwatch.Elapsed;
            Interlocked.Increment(ref QueryCounter);

            return valuesRows;

        }
    }
}
