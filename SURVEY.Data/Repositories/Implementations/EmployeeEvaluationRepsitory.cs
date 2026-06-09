using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
            var query = _context.employee_evaluations.AsQueryable();
            if (!string.IsNullOrEmpty(employeeId))
            {
                query = query.Where(e => e.employee_code == employeeId);
            }
            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(e => e.department == department);
            }
            if (pageIndex.HasValue && pageSize.HasValue)
            {
                int skip = (pageIndex.Value - 1) * pageSize.Value;
                query = query.Skip(skip).Take(pageSize.Value);
            }
            return await Task.FromResult(query.ToList());
        }
        // Post thông tin đánh giá của công nhân viên
        public async Task<bool> AddEmployeeEvaluationAsync(employee_evaluation evaluation)
        {
            _context.employee_evaluations.Add(evaluation);
            return await Task.FromResult(_context.SaveChanges() > 0);
        }
    }
}
