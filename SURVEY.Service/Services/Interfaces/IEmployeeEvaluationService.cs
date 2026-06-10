using SURVEY.Model.Common;
using SURVEY.Model.DTOs;
using SURVEY.Model.Models_SURVEY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURVEY.Service.Services.Interfaces
{
    internal interface IEmployeeEvaluationService: IBaseService<employee_evaluation, int, employee_evaluationDTO>
    {
        // Get thông tin đánh giá của công nhân viên
        Task<GenericResponse<List<employee_evaluationDTO>>> GetEvaluationsByEvaluatorIdAsync(string? employeeId, string? department, DateTime? dateFrom, DateTime? dateTo, int? pageIndex, int? pageSize);
        // Post thông tin đánh giá của công nhân viên
        Task<GenericResponse<bool>> AddEmployeeEvaluationAsync(employee_evaluationDTO evaluation);
        // Get All
        Task<GenericResponse<List<employee_evaluationDTO>>> GetAllEmployeeEvaluationsAsync();
        // Send mail to Section manager 
        Task<GenericResponse<bool>> SendEmailToSectionManagerAsync(employee_evaluationDTO evaluation);
    }
}
    