using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SURVEY.Data.Repositories.Interfaces;
using SURVEY.Model.Common;
using SURVEY.Model.Common.Enum;
using SURVEY.Model.Common.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SURVEY.Data.Repositories.Implementations
{
    public class BaseRepository<T, TKey> : IBaseRepository<T, TKey> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly string _connectionString;
        private readonly ConnectionStringOptions _connectionStringOptions;
        protected readonly string _tableName;
        protected readonly IDbConnection _conn;
        private static IDbTransaction _transaction;
        private IEnumerable<PropertyInfo> GetProperties => typeof(T).GetProperties();

        public BaseRepository(DbContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration)
        {
            _context = context;
            _dbSet = context.Set<T>();
            _connectionStringOptions = options.Value;
            _connectionString = _connectionStringOptions.PCManagementConnection;
            //_connectionString = configuration.GetConnectionString("PCManagementConnection");
            _conn = new SqlConnection(_connectionStringOptions.PCManagementConnection);
            _tableName = GetTableName<T>();
        }
        #region Transaction
        public virtual void StartTransaction()
        {
            _conn.Open();
            _transaction = _conn.BeginTransaction();
        }

        public virtual void CommitTransaction()
        {
            _transaction.Commit();
        }

        public virtual void RollbackTransaction()
        {
            _transaction.Rollback();
        }
        #endregion

        public virtual string GetTableName<T>()
        {
            var type = typeof(T);
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            if (tableAttr != null)
            {
                return tableAttr.Name;
            }
            else
            {
                // Nếu không có attribute [Table], thì lấy tên của class
                return type.Name;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllWithDapperAsync()
        {
            var sql = $"SELECT * FROM {_tableName}";
            return await _conn.QueryAsync<T>(sql);
        }

        public virtual async Task<long> GetCountAllAsync()
        {
            var sql = $"SELECT COUNT(*) FROM {_tableName}";
            //if (typeof(IHasStatus).IsAssignableFrom(typeof(T)))
            //{
            //    sql += $" AND INT_STATUS_ID = {StatusEnum.Active}";
            //}
            return await _conn.QuerySingleAsync<long>(sql);
        }

        public virtual async Task<long> GetCountByConditionAsync(SearchOptions options)
        {
            using var connection = new SqlConnection(_connectionString);

            var sqlCount = new StringBuilder($"SELECT COUNT(*) FROM {_tableName} WHERE 1=1");
            var parameters = new DynamicParameters();

            // Search term
            if (!string.IsNullOrEmpty(options.SearchTerm) && options.SearchFields?.Any() == true)
            {
                var searchConditions = new List<string>();
                for (int i = 0; i < options.SearchFields.Count; i++)
                {
                    var field = options.SearchFields[i];
                    var paramName = $"@SearchTerm{i}";
                    searchConditions.Add($"{field} COLLATE Latin1_General_CI_AI LIKE {paramName}");
                    parameters.Add(paramName, $"%{options.SearchTerm}%");
                }

                var searchClause = " AND (" + string.Join(" OR ", searchConditions) + ")";
                sqlCount.Append(searchClause);
            }

            // Filters
            if (options.Filters?.Any() == true)
            {
                var filterClauses = new List<string>();

                for (int i = 0; i < options.Filters.Count; i++)
                {
                    var filter = options.Filters[i];
                    object value = filter.Value;

                    if (value is JsonElement jsonElement)
                    {
                        switch (jsonElement.ValueKind)
                        {
                            case JsonValueKind.String:
                                value = jsonElement.GetString();
                                break;
                            case JsonValueKind.Number:
                                if (jsonElement.TryGetInt32(out int intValue))
                                    value = intValue;
                                else if (jsonElement.TryGetDouble(out double doubleValue))
                                    value = doubleValue;
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                value = jsonElement.GetBoolean();
                                break;
                            case JsonValueKind.Null:
                                value = DBNull.Value;
                                break;
                            case JsonValueKind.Array:
                                var list = new List<object>();
                                foreach (var item in jsonElement.EnumerateArray())
                                {
                                    list.Add(item.ValueKind switch
                                    {
                                        JsonValueKind.String => item.GetString(),
                                        JsonValueKind.Number => item.TryGetInt32(out var intVal) ? intVal : item.GetDouble(),
                                        JsonValueKind.True => true,
                                        JsonValueKind.False => false,
                                        _ => item.ToString()
                                    });
                                }
                                value = list;
                                break;
                            default:
                                value = jsonElement.ToString();
                                break;
                        }
                    }

                    var logic = (i == 0) ? "" : $" {filter.LogicType?.ToUpper() ?? "AND"} ";
                    var op = filter.Operator?.ToUpper() ?? "=";
                    var field = filter.Field;
                    var paramName = $"@Filter_{i}";

                    switch (op)
                    {
                        case "IN":
                            if (value is IEnumerable<object> list)
                            {
                                var inParams = new List<string>();
                                int j = 0;
                                foreach (var item in list)
                                {
                                    var pName = $"{paramName}_{j}";
                                    inParams.Add(pName);
                                    parameters.Add(pName, item);
                                    j++;
                                }
                                filterClauses.Add($"{logic}{field} IN ({string.Join(",", inParams)})");
                            }
                            break;

                        case "BETWEEN":
                            if (value is IEnumerable<object> range && range.Count() == 2)
                            {
                                var fromParam = $"{paramName}_from";
                                var toParam = $"{paramName}_to";
                                var rangeList = range.ToList();
                                parameters.Add(fromParam, rangeList[0]);
                                parameters.Add(toParam, rangeList[1]);
                                filterClauses.Add($"{logic}{field} BETWEEN {fromParam} AND {toParam}");
                            }
                            break;

                        case "IS NULL":
                        case "IS NOT NULL":
                            filterClauses.Add($"{logic}{field} {op}");
                            break;

                        case "LIKE":
                            filterClauses.Add($"{logic}{field} COLLATE Latin1_General_CI_AI LIKE {paramName}");
                            parameters.Add(paramName, $"%{value}%");
                            break;

                        default:
                            filterClauses.Add($"{logic}{field} {op} {paramName}");
                            parameters.Add(paramName, value);
                            break;
                    }
                }

                var filterClause = string.Empty;

                filterClause = " AND (" + string.Join(" ", filterClauses) + ")";

                //if (typeof(IHasStatus).IsAssignableFrom(typeof(T)))
                //{
                //    filterClause = " AND (" + string.Join(" ", filterClauses) + $") AND INT_STATUS_ID = {StatusEnum.Active}";
                //}
                //else
                //{
                //    filterClause = " AND (" + string.Join(" ", filterClauses) + ")";
                //}

                sqlCount.Append(filterClause);
            }

            var count = await connection.ExecuteScalarAsync<long>(sqlCount.ToString(), parameters);
            return count;
        }


        public virtual async Task<PagedList<T>> SearchByConditionAsync(SearchOptions options)
        {
            using var connection = new SqlConnection(_connectionString);

            var sqlSelect = new StringBuilder($"SELECT * FROM {_tableName} WHERE 1=1");
            var sqlCount = new StringBuilder($"SELECT COUNT(*) FROM {_tableName} WHERE 1=1");
            var sqlFilteredCount = new StringBuilder($"SELECT COUNT(*) FROM {_tableName} WHERE 1=1");

            var parameters = new DynamicParameters();

            // Search term
            if (!string.IsNullOrEmpty(options.SearchTerm) && options.SearchFields?.Any() == true)
            {
                var searchConditions = new List<string>();
                for (int i = 0; i < options.SearchFields.Count; i++)
                {
                    var field = options.SearchFields[i];
                    var paramName = $"@SearchTerm{i}";
                    searchConditions.Add($"{field} COLLATE Latin1_General_CI_AI LIKE {paramName}");
                    parameters.Add(paramName, $"%{options.SearchTerm}%");
                }

                var searchClause = " AND (" + string.Join(" OR ", searchConditions) + ")";
                sqlSelect.Append(searchClause);
                sqlFilteredCount.Append(searchClause);
            }

            // Filters
            if (options.Filters?.Any() == true)
            {
                var filterClauses = new List<string>();

                for (int i = 0; i < options.Filters.Count; i++)
                {
                    var filter = options.Filters[i];
                    object value = filter.Value;

                    // Nếu value là JsonElement thì ép kiểu đúng
                    if (value is JsonElement jsonElement)
                    {
                        switch (jsonElement.ValueKind)
                        {
                            case JsonValueKind.String:
                                value = jsonElement.GetString();
                                break;
                            case JsonValueKind.Number:
                                if (jsonElement.TryGetInt32(out int intValue))
                                    value = intValue;
                                else if (jsonElement.TryGetDouble(out double doubleValue))
                                    value = doubleValue;
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                value = jsonElement.GetBoolean();
                                break;
                            case JsonValueKind.Null:
                                value = DBNull.Value;
                                break;
                            case JsonValueKind.Array:
                                var list = new List<object>();
                                foreach (var item in jsonElement.EnumerateArray())
                                {
                                    list.Add(item.ValueKind switch
                                    {
                                        JsonValueKind.String => item.GetString(),
                                        JsonValueKind.Number => item.TryGetInt32(out var intVal) ? intVal : item.GetDouble(),
                                        JsonValueKind.True => true,
                                        JsonValueKind.False => false,
                                        _ => item.ToString()
                                    });
                                }
                                value = list;
                                break;
                            default:
                                value = jsonElement.ToString();
                                break;
                        }
                    }

                    if (value is string stringValue)
                    {
                        // Chuẩn hóa khoảng cách trong chuỗi
                        stringValue = Regex.Replace(stringValue, @"\s+", " ").Trim();
                        value = stringValue;
                    }

                    var logic = (i == 0) ? "" : $" {filter.LogicType?.ToUpper() ?? "AND"} ";
                    var op = filter.Operator?.ToUpper() ?? "=";
                    var field = filter.Field;
                    var paramName = $"@Filter_{i}";

                    switch (op)
                    {
                        case "IN":
                            if (value is IEnumerable<object> list)
                            {
                                var inParams = new List<string>();
                                int j = 0;
                                foreach (var item in list)
                                {
                                    var pName = $"{paramName}_{j}";
                                    inParams.Add(pName);
                                    parameters.Add(pName, item);
                                    j++;
                                }
                                filterClauses.Add($"{logic}{field} IN ({string.Join(",", inParams)})");
                            }
                            break;

                        case "BETWEEN":
                            if (value is IEnumerable<object> range && range.Count() == 2)
                            {
                                var fromParam = $"{paramName}_from";
                                var toParam = $"{paramName}_to";
                                var rangeList = range.ToList();
                                parameters.Add(fromParam, rangeList[0]);
                                parameters.Add(toParam, rangeList[1]);
                                filterClauses.Add($"{logic}{field} BETWEEN {fromParam} AND {toParam}");
                            }
                            break;

                        case "IS NULL":
                        case "IS NOT NULL":
                            filterClauses.Add($"{logic}{field} {op}");
                            break;

                        case "LIKE":
                            filterClauses.Add($"{logic}{field} COLLATE Latin1_General_CI_AI LIKE {paramName}");
                            parameters.Add(paramName, $"%{value}%");
                            break;

                        default:
                            filterClauses.Add($"{logic}{field} {op} {paramName}");
                            parameters.Add(paramName, value);
                            break;
                    }
                }

                var filterClause = string.Empty;

                filterClause = " AND (" + string.Join(" ", filterClauses) + ")";
                //if (typeof(IHasStatus).IsAssignableFrom(typeof(T)))
                //{
                //    filterClause = " AND (" + string.Join(" ", filterClauses) + $") AND INT_STATUS_ID = {StatusEnum.Active}";
                //}
                //else
                //{
                //    filterClause = " AND (" + string.Join(" ", filterClauses) + ")";
                //}

                sqlSelect.Append(filterClause);
                sqlFilteredCount.Append(filterClause);
            }

            // Sort
            if (options.SortOptions?.Any() == true)
            {
                sqlSelect.Append(" ORDER BY ");
                for (int i = 0; i < options.SortOptions.Count; i++)
                {
                    var sort = options.SortOptions[i];
                    var direction = sort.SortDirection?.ToUpper() == "DESC" ? "DESC" : "ASC";
                    if (i > 0) sqlSelect.Append(", ");
                    sqlSelect.Append($"{sort.Field} {direction}");
                }
            }
            //else
            //{
            //    sqlSelect.Append($" ORDER BY {nameof(TM_USER.DTM_CREATE)} DESC");
            //}

            // Paging
            if (options.PageNumber != -1)
            {
                sqlSelect.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("@Offset", (options.PageNumber - 1) * options.PageSize);
                parameters.Add("@PageSize", options.PageSize);
            }

            // Combine queries
            var finalSql = $"{sqlSelect}; {sqlCount}; {sqlFilteredCount};";

            using var multi = await connection.QueryMultipleAsync(finalSql, parameters);
            var data = (await multi.ReadAsync<T>()).ToList();
            var totalCount = await multi.ReadFirstAsync<long>();
            var totalFiltered = await multi.ReadFirstAsync<int>();

            return new PagedList<T>
            {
                Data = data,
                PageIndex = options.PageNumber,
                PageSize = options.PageSize,
                TotalCount = totalCount,
                TotalFilter = totalFiltered,
                TotalPages = (int)Math.Ceiling((decimal)totalFiltered / options.PageSize)
            };
        }

        public virtual async Task<PagedList<T>> GetPagingAsync(PagingRequest request, bool onlyActive = true, CommandType commandType = CommandType.Text)
        {
            string text = GenerateWhereClause(request.Filters);
            string value = (string.IsNullOrEmpty(text) ? string.Empty : ("WHERE " + text));
            string text2 = GenerateOrderClause(request.Sorts);
            string value2 = (string.IsNullOrEmpty(text2) ? string.Empty : ("ORDER BY " + text2));
            string value3 = GenerateSelectClause(request.Fields);
            int num = (request.PageInfo.PageIndex - 1) * request.PageInfo.PageSize;
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("@PageSize", request.PageInfo.PageSize, DbType.Int32);
            dynamicParameters.Add("@Skip", num, DbType.Int32);
            string value4 = $"SELECT {value3} FROM {_tableName} {value} {value2} Limit @PageSize Offset @Skip;";
            string value5 = "SELECT COUNT(*) FROM " + _tableName + ";";
            string value6 = $"SELECT COUNT(*) FROM {_tableName} {value};";
            value4 = $"{value4} {value5} {value6}";
            IDbConnection conn = _conn;
            string sql = value4;
            CommandType? commandType2 = commandType;
            SqlMapper.GridReader obj = await conn.QueryMultipleAsync(sql, dynamicParameters, null, null, commandType2);
            IEnumerable<T> source = obj.Read<T>();
            long totalCount = obj.Read<long>().FirstOrDefault();
            int num2 = obj.Read<int>().FirstOrDefault();
            return new PagedList<T>
            {
                Data = source.ToList(),
                PageIndex = request.PageInfo.PageIndex,
                PageSize = request.PageInfo.PageSize,
                TotalCount = totalCount,
                TotalFilter = num2,
                TotalPages = (int)Math.Ceiling((decimal)num2 / (decimal)request.PageInfo.PageSize)
            };
        }

        public virtual async Task<T> GetByIdAsync(TKey id)
        {
            var parameterName = "@Id";

            var sql = $"SELECT * FROM {_tableName} WHERE ID = {parameterName}";

            //if (typeof(IHasStatus).IsAssignableFrom(typeof(T)))
            //{
            //    sql += $"AND INT_STATUS_ID = {StatusEnum.Active}";
            //}
            return await _conn.QuerySingleOrDefaultAsync<T>(sql, new { Id = id });
        }

        public virtual async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<TKey> ids)
        {
            var idsList = ids.ToList();
            var sql = $"SELECT * FROM {_tableName} WHERE ID IN @Ids";
            //if (typeof(IHasStatus).IsAssignableFrom(typeof(T)))
            //{
            //    sql += $"AND INT_STATUS_ID = {StatusEnum.Active}";
            //}
            return await _conn.QueryAsync<T>(sql, new { Ids = idsList });

        }

        public virtual async Task<TKey> AddAsync(T entity)
        {
            string sql = GenerateInsertQuery();
            return await _conn.QuerySingleOrDefaultAsync<TKey>(sql, entity);
        }

        public virtual async Task<T> AddAsyncV2(T entity)
        {
            string sql = GenerateInsertEntityQuery();
            entity = await _conn.QuerySingleOrDefaultAsync<T>(sql, entity);
            return entity;
        }

        public virtual async Task<int> AddMultiAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
            {
                return 0;
            }

            int num = 0;
            string sql = GenerateInsertQuery();
            int num2 = num;
            return num2 + await _conn.ExecuteAsync(sql, entities);
        }

        public virtual async Task<List<T>> AddMultiAsyncV2(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new List<T>();
            }

            List<T> lstEntities = entities.ToList();
            string query = GenerateInsertEntityQuery();
            for (int i = 0; i < lstEntities.Count; i++)
            {
                List<T> list = lstEntities;
                int index = i;
                list[index] = await _conn.QuerySingleOrDefaultAsync<T>(query, lstEntities[i]);
            }

            return lstEntities;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual async Task<bool> IsExistsAsync(string fieldName, object value)
        {
            var sql = $"SELECT COUNT(1) FROM {_tableName} WHERE {fieldName} = @Value";

            var count = await _conn.ExecuteScalarAsync<int>(sql, new { Value = value });
            return count > 0;
        }

        public virtual async Task<int> UpdateAsync(T entity)
        {
            string sql = GenerateUpdateQuery();
            return await _conn.ExecuteAsync(sql, entity);
        }

        public virtual async Task<int> UpdateMultiAsync(IEnumerable<T> entities)
        {
            int num = 0;
            string sql = GenerateUpdateQuery();
            int num2 = num;
            return num2 + await _conn.ExecuteAsync(sql, entities);
        }

        public virtual async Task<int> DeleteAsync(TKey id)
        {
            return await _conn.ExecuteAsync("DELETE FROM " + _tableName + " WHERE ID = @Id", new { Id = id });
        }

        public virtual async Task<int> DeleteLogicAsync(TKey id)
        {
            return await _conn.ExecuteAsync($"UPDATE {_tableName} SET INT_STATUS_ID='{(int)StatusEnum.Deleted}' WHERE ID=@Id", new
            {
                Id = id
            });
        }

        public virtual async Task DeleteLogicMultiAsync(IEnumerable<TKey> ids)
        {
            var sql = $"UPDATE {_tableName} SET INT_STATUS_ID='{(int)StatusEnum.Deleted}' WHERE ID IN @Ids";

            await _conn.ExecuteAsync(sql, new { Ids = ids });
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<TKey> ids)
        {
            var sql = $"DELETE FROM {_tableName} WHERE ID IN @Ids";

            await _conn.ExecuteAsync(sql, new { Ids = ids });
        }

        private string GenerateInsertQuery()
        {
            StringBuilder insertQuery = new StringBuilder($"INSERT INTO {_tableName} (");

            List<PropertyInfo> properties = GetProperties.ToList();
            List<string> propertyNames = GenerateListOfProperties(properties);

            // Bỏ qua trường "ID"
            var columns = propertyNames.Where(p => !p.Equals("ID", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var prop in columns)
            {
                insertQuery.Append($"[{prop}],");
            }

            insertQuery.Remove(insertQuery.Length - 1, 1).Append(") ");

            insertQuery.Append("OUTPUT Inserted.ID VALUES (");

            foreach (var prop in columns)
            {
                insertQuery.Append($"@{prop},");
            }

            insertQuery.Remove(insertQuery.Length - 1, 1).Append(")");

            return insertQuery.ToString();
        }


        private string GenerateInsertQuery_Old()
        {
            StringBuilder insertQuery = new StringBuilder("INSERT INTO " + _tableName + " ");
            insertQuery.Append("(");
            List<PropertyInfo> list = GetProperties.ToList();
            List<string> list2 = GenerateListOfProperties(list);
            bool hasKeyDatabaseGenerated = true;
            PropertyInfo propertyInfo = list.FirstOrDefault((PropertyInfo x) => x.Name == "ID");
            if (propertyInfo != null)
            {
                object[] customAttributes = propertyInfo.GetCustomAttributes(inherit: true);
                if (customAttributes.All((object x) => x.GetType().Name != "KeyAttribute") || customAttributes.All((object x) => x.GetType().Name != "DatabaseGeneratedAttribute"))
                {
                    hasKeyDatabaseGenerated = false;
                }
            }

            list2.ForEach(delegate (string prop)
            {
                if (!prop.Equals("ID"))
                {
                    StringBuilder stringBuilder4 = insertQuery;
                    StringBuilder stringBuilder5 = stringBuilder4;
                    StringBuilder.AppendInterpolatedStringHandler handler2 = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder4);
                    handler2.AppendLiteral("[");
                    handler2.AppendFormatted(prop);
                    handler2.AppendLiteral("],");
                    stringBuilder5.Append(ref handler2);
                }
                else
                {
                    if (prop.Equals("ID") && !hasKeyDatabaseGenerated)
                    {
                        StringBuilder stringBuilder4 = insertQuery;
                        StringBuilder stringBuilder7 = stringBuilder4;
                        StringBuilder.AppendInterpolatedStringHandler handler2 = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder4);
                        handler2.AppendLiteral("[");
                        handler2.AppendFormatted(prop);
                        handler2.AppendLiteral("],");
                        stringBuilder7.Append(ref handler2);
                    }
                }
            });

            insertQuery.Remove(insertQuery.Length - 1, 1).Append(") OUTPUT Inserted.ID VALUES (");

            list2.ForEach(delegate (string prop)
            {
                if (!prop.Equals("ID"))
                {
                    StringBuilder stringBuilder = insertQuery;
                    StringBuilder stringBuilder2 = stringBuilder;
                    StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder);
                    handler.AppendLiteral("@");
                    handler.AppendFormatted(prop);
                    handler.AppendLiteral(",");
                    stringBuilder2.Append(ref handler);
                }
            });

            insertQuery.Remove(insertQuery.Length - 1, 1).Append(")");

            return insertQuery.ToString();
        }

        private string GenerateInsertEntityQuery()
        {
            StringBuilder insertQuery = new StringBuilder($"INSERT INTO {_tableName} (");

            List<PropertyInfo> properties = GetProperties.ToList();
            List<string> propertyNames = GenerateListOfProperties(properties);

            // Bỏ qua trường "ID"
            var columns = propertyNames.Where(p => !p.Equals("ID", StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var prop in columns)
            {
                insertQuery.Append($"[{prop}],");
            }

            insertQuery.Remove(insertQuery.Length - 1, 1).Append(") ");

            insertQuery.Append("OUTPUT Inserted.* VALUES (");

            foreach (var prop in columns)
            {
                insertQuery.Append($"@{prop},");
            }

            insertQuery.Remove(insertQuery.Length - 1, 1).Append(")");

            return insertQuery.ToString();
        }


        private string GenerateInsertEntityQuery_old()
        {
            StringBuilder insertQuery = new StringBuilder("INSERT INTO " + _tableName + " ");
            insertQuery.Append("(");
            List<PropertyInfo> list = GetProperties.ToList();
            List<string> list2 = GenerateListOfProperties(list);
            bool hasKeyDatabaseGenerated = true;
            PropertyInfo propertyInfo = list.FirstOrDefault((PropertyInfo x) => x.Name == "ID");
            if (propertyInfo != null)
            {
                object[] customAttributes = propertyInfo.GetCustomAttributes(inherit: true);
                if (customAttributes.All((object x) => x.GetType().Name != "KeyAttribute") || customAttributes.All((object x) => x.GetType().Name != "DatabaseGeneratedAttribute"))
                {
                    hasKeyDatabaseGenerated = false;
                }
            }

            list2.ForEach(delegate (string prop)
            {
                if (!prop.Equals("ID"))
                {
                    StringBuilder stringBuilder4 = insertQuery;
                    StringBuilder stringBuilder5 = stringBuilder4;
                    StringBuilder.AppendInterpolatedStringHandler handler2 = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder4);
                    handler2.AppendLiteral("[");
                    handler2.AppendFormatted(prop);
                    handler2.AppendLiteral("],");
                    stringBuilder5.Append(ref handler2);
                }
                else
                {
                    if (prop.Equals("ID") && !hasKeyDatabaseGenerated)
                    {
                        StringBuilder stringBuilder4 = insertQuery;
                        StringBuilder stringBuilder7 = stringBuilder4;
                        StringBuilder.AppendInterpolatedStringHandler handler2 = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder4);
                        handler2.AppendLiteral("[");
                        handler2.AppendFormatted(prop);
                        handler2.AppendLiteral("],");
                        stringBuilder7.Append(ref handler2);
                    }
                }
            });
            insertQuery.Remove(insertQuery.Length - 1, 1).Append(") OUTPUT Inserted.* VALUES (");

            list2.ForEach(delegate (string prop)
            {
                if (!prop.Equals("ID"))
                {
                    StringBuilder stringBuilder = insertQuery;
                    StringBuilder stringBuilder2 = stringBuilder;
                    StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder);
                    handler.AppendLiteral("@");
                    handler.AppendFormatted(prop);
                    handler.AppendLiteral(",");
                    stringBuilder2.Append(ref handler);
                }
            });
            insertQuery.Remove(insertQuery.Length - 1, 1).Append(")");

            return insertQuery.ToString();
        }

        private string GenerateUpdateQuery()
        {
            StringBuilder updateQuery = new StringBuilder("UPDATE " + _tableName + " SET ");
            GenerateListOfProperties(GetProperties).ForEach(delegate (string property)
            {
                if (!property.Equals("ID"))
                {
                    StringBuilder stringBuilder = updateQuery;
                    StringBuilder stringBuilder2 = stringBuilder;
                    StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder);
                    handler.AppendFormatted(property);
                    handler.AppendLiteral("=@");
                    handler.AppendFormatted(property);
                    handler.AppendLiteral(",");
                    stringBuilder2.Append(ref handler);
                }
            });
            updateQuery.Remove(updateQuery.Length - 1, 1);
            updateQuery.Append(" WHERE ID=@Id");

            return updateQuery.ToString();
        }

        protected string GenerateWhereInClause(List<TKey> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder("(");
            int num = 0;
            foreach (TKey id in ids)
            {
                string value = ((num == 0) ? string.Empty : ", ");
                StringBuilder stringBuilder2 = stringBuilder;
                StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 2, stringBuilder2);
                handler.AppendFormatted(value);
                handler.AppendFormatted(id);
                stringBuilder2.Append(ref handler);
                num++;
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        protected string GenerateWhereClause_Old(List<Filter> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (Filter filter in filters)
            {
                if (!string.IsNullOrEmpty(filter.Field))
                {
                    string empty = string.Empty;
                    empty = ((filter.Operator == OperatorType.Contain) ? ("'%" + (filter.Value ?? string.Empty) + "%'") : ((filter.Operator == OperatorType.StartWith) ? ("'" + (filter.Value ?? string.Empty) + "%'") : ((filter.Operator == OperatorType.EndWith) ? ("'%" + (filter.Value ?? string.Empty) + "'") : ((filter.Operator != OperatorType.In && filter.Operator != OperatorType.NotIn) ? ("'" + (filter.Value ?? string.Empty) + "'") : ("(" + (filter.Value ?? string.Empty) + ")")))));
                    if (filter.Operator == OperatorType.IsNull || filter.Operator == OperatorType.IsNotNull)
                    {
                        empty = string.Empty;
                    }
                    else if (filter.Operator == OperatorType.Contain || filter.Operator == OperatorType.StartWith || filter.Operator == OperatorType.EndWith)
                    {
                        empty = "U&" + empty;
                    }

                    string value = filter.Field;
                    string value2 = (string.IsNullOrEmpty(empty) ? string.Empty : " ");
                    if (filter.Logic > LogicType.None)
                    {
                        StringBuilder stringBuilder2 = stringBuilder;
                        StringBuilder stringBuilder3 = stringBuilder2;
                        StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(5, 5, stringBuilder2);
                        handler.AppendLiteral(" ");
                        handler.AppendFormatted(filter.Logic);
                        handler.AppendLiteral(" (");
                        handler.AppendFormatted(value);
                        handler.AppendLiteral(" ");
                        handler.AppendFormatted(SqlOperatorHelper.GetSqlOperator(filter.Operator));
                        handler.AppendFormatted(value2);
                        handler.AppendFormatted(empty);
                        handler.AppendLiteral(")");
                        stringBuilder3.Append(ref handler);
                    }
                    else
                    {
                        StringBuilder stringBuilder2 = stringBuilder;
                        StringBuilder stringBuilder4 = stringBuilder2;
                        StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 4, stringBuilder2);
                        handler.AppendLiteral("(");
                        handler.AppendFormatted(value);
                        handler.AppendLiteral(" ");
                        handler.AppendFormatted(SqlOperatorHelper.GetSqlOperator(filter.Operator));
                        handler.AppendFormatted(value2);
                        handler.AppendFormatted(empty);
                        handler.AppendLiteral(") ");
                        stringBuilder4.Append(ref handler);
                    }
                }
                else if (filter.Filters != null && filter.Filters.Count > 0)
                {
                    StringBuilder stringBuilder2;
                    StringBuilder.AppendInterpolatedStringHandler handler;
                    if (filter.Logic > LogicType.None)
                    {
                        stringBuilder2 = stringBuilder;
                        StringBuilder stringBuilder5 = stringBuilder2;
                        handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
                        handler.AppendLiteral(" ");
                        handler.AppendFormatted(filter.Logic);
                        handler.AppendLiteral(" ");
                        stringBuilder5.Append(ref handler);
                    }

                    stringBuilder2 = stringBuilder;
                    StringBuilder stringBuilder6 = stringBuilder2;
                    handler = new StringBuilder.AppendInterpolatedStringHandler(2, 1, stringBuilder2);
                    handler.AppendLiteral("(");
                    handler.AppendFormatted(GenerateWhereClause_Old(filter.Filters));
                    handler.AppendLiteral(")");
                    stringBuilder6.Append(ref handler);
                }
            }

            return stringBuilder.ToString();
        }

        protected string GenerateWhereClause(List<Filter> filters)
        {
            if (filters == null || filters.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (var filter in filters)
            {
                if (!string.IsNullOrEmpty(filter.Field))
                {
                    string sqlOperator = SqlOperatorHelper.GetSqlOperator(filter.Operator);
                    string formattedValue = FormatSqlValueForSqlServerHelper.FormatSqlValue(filter);

                    if (filter.Logic > LogicType.None && sb.Length > 0)
                        sb.Append($" {filter.Logic} ");

                    sb.Append($"({filter.Field} {sqlOperator}");

                    if (!string.IsNullOrEmpty(formattedValue))
                        sb.Append($" {formattedValue}");

                    sb.Append(")");
                }
                else if (filter.Filters != null && filter.Filters.Count > 0)
                {
                    if (filter.Logic > LogicType.None && sb.Length > 0)
                        sb.Append($" {filter.Logic} ");

                    sb.Append("(");
                    sb.Append(GenerateWhereClause(filter.Filters));
                    sb.Append(")");
                }
            }

            return sb.ToString();
        }



        protected string GenerateOrderClause(List<Sort> sorts)
        {
            if (sorts == null || sorts.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (Sort sort in sorts)
            {
                string value = sort.Field;
                string value2 = ((sort == sorts.First()) ? string.Empty : ", ");
                StringBuilder stringBuilder2 = stringBuilder;
                StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 3, stringBuilder2);
                handler.AppendFormatted(value2);
                handler.AppendFormatted(value);
                handler.AppendLiteral(" ");
                handler.AppendFormatted(sort.OrderDirection);
                stringBuilder2.Append(ref handler);
            }

            return stringBuilder.ToString();
        }

        protected string GenerateSelectClause(string fields)
        {
            if (string.IsNullOrEmpty(fields))
            {
                return "*";
            }

            fields = Regex.Replace(fields, "\\s", "");

            return fields;
        }

        private static List<string> GenerateListOfProperties(IEnumerable<PropertyInfo> listOfProperties)
        {
            return (from prop in listOfProperties
                    let attributes = prop.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false)
                    where attributes.Length == 0 || (attributes[0] as DescriptionAttribute)?.Description != "ignore"
                    select prop.Name).ToList();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public Task<IQueryable<T>> GetQueryableAsync()
        {
            return Task.FromResult(_dbSet.AsQueryable());
        }
    }
}
