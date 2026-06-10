using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text;
using SURVEY.Data.Repositories.Interfaces;
using SURVEY.Model.Common;
using SURVEY.Model.Models_SURVEY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURVEY.Data.Repositories.Implementations
{
    public class EmployeeEvaluationRepsitory : BaseRepository<employee_evaluation, int>, IEmployeeEvaluationRepsitory
    {
        private readonly SURVEYContext _context;

        public EmployeeEvaluationRepsitory(SURVEYContext context, IOptions<ConnectionStringOptions> options, IConfiguration configuration) : base(context, options, configuration)
        {
            _context = context;
        }
        // Get thông tin đánh giá của công nhân viên
        public async Task<List<employee_evaluation>> GetEvaluationsByEvaluatorIdAsync(string? employeeId, string? department, DateTime? dateFrom, DateTime? dateTo, int? pageIndex, int? pageSize)
        {
            var sql = new StringBuilder();
            sql.Append($"SELECT * FROM employee_evaluation WHERE 1=1");

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(employeeId))
            {
                sql.Append(" AND employee_code = @EmployeeCode");
                parameters.Add("EmployeeCode", employeeId, DbType.String);
            }

            if (!string.IsNullOrEmpty(department))
            {
                sql.Append(" AND department = @Department");
                parameters.Add("Department", department, DbType.String);
            }

            if (dateFrom.HasValue)
            {
                sql.Append(" AND created_at >= @DateFrom");
                parameters.Add("DateFrom", dateFrom.Value.Date, DbType.DateTime);
            }

            if (dateTo.HasValue)
            {
                // include the full day for dateTo
                var dateToEnd = dateTo.Value.Date.AddDays(1).AddTicks(-1);
                sql.Append(" AND created_at <= @DateTo");
                parameters.Add("DateTo", dateToEnd, DbType.DateTime);
            }

            // Default ordering
            sql.Append(" ORDER BY created_at DESC");

            if (pageIndex.HasValue && pageSize.HasValue && pageIndex.Value > 0 && pageSize.Value > 0)
            {
                int offset = (pageIndex.Value - 1) * pageSize.Value;
                sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
                parameters.Add("Offset", offset, DbType.Int32);
                parameters.Add("PageSize", pageSize.Value, DbType.Int32);
            }

            var result = await _conn.QueryAsync<employee_evaluation>(sql.ToString(), parameters);
            return result.ToList();
        }
        // Post thông tin đánh giá của công nhân viên
        public async Task<bool> AddEmployeeEvaluationAsync(employee_evaluation evaluation)
        {
            _context.employee_evaluations.Add(evaluation);
            return await Task.FromResult(_context.SaveChanges() > 0);
        }
    }
}
