using SURVEY.Model.Models_SURVEY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURVEY.Data.Repositories.Interfaces
{
    internal interface IEmployeeEvaluationRepsitory: IBaseRepository<employee_evaluation, int>
    {
        // Get thông tin đánh giá của công nhân viên
        Task<List<employee_evaluation>> GetEvaluationsByEvaluatorIdAsync(string? employeeId, string? department, int? pageIndex, int? pageSize);
        // Post thông tin đánh giá của công nhân viên
        Task<bool> AddEmployeeEvaluationAsync(employee_evaluation evaluation);
    }
}
