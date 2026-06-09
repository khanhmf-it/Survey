using AutoMapper;
using SURVEY.Data.Repositories.Implementations;
using SURVEY.Data.Repositories.Interfaces;
using SURVEY.Model.Common;
using SURVEY.Model.DTOs;
using SURVEY.Model.Models_SURVEY;
using SURVEY.Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SURVEY.Service.Services.Implementations
{
    public class EmployeeEvaluationService : BaseService<employee_evaluation, int, employee_evaluationDTO> , IEmployeeEvaluationService
    {
        private readonly IEmployeeEvaluationRepsitory _repo;
        private readonly IMapper _mapper;
        public EmployeeEvaluationService(IEmployeeEvaluationRepsitory repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        // Get thông tin đánh giá của công nhân viên
        public async Task<GenericResponse<List<employee_evaluationDTO>>> GetEvaluationsByEvaluatorIdAsync(string? employeeId, string? department, DateTime? dateFrom, DateTime? dateTo, int? pageIndex, int? pageSize)
        {
            var result = new GenericResponse<List<employee_evaluationDTO>>();
            try
            {
                var q = await _repo.GetEvaluationsByEvaluatorIdAsync(employeeId, department, dateFrom, dateTo, pageIndex, pageSize);
                result.Data = _mapper.Map<List<employee_evaluationDTO>>(q);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }

            return result;
        }
        // Post thông tin đánh giá của công nhân viên
        public async Task<GenericResponse<bool>> AddEmployeeEvaluationAsync(employee_evaluationDTO evaluation)
        {
            var result = new GenericResponse<bool>();
            try
            {
                var entity = _mapper.Map<employee_evaluation>(evaluation);
                await _repo.AddEmployeeEvaluationAsync(entity);
                result.Data = true;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
        // Get All
        public async Task<GenericResponse<List<employee_evaluationDTO>>> GetAllEmployeeEvaluationsAsync()
        {
            var result = new GenericResponse<List<employee_evaluationDTO>>();
            try
            {
                var q = await _repo.GetAllAsync();
                result.Data = _mapper.Map<List<employee_evaluationDTO>>(q);
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }
    }
}
