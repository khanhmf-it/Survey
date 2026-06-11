using AutoMapper;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using SURVEY.Data.Repositories.Implementations;
using SURVEY.Data.Repositories.Interfaces;
using SURVEY.Model.Common;
using SURVEY.Model.DTOs;
using SURVEY.Model.Models_SURVEY;
using SURVEY.Service.Configs;
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
        private readonly IConfiguration _configuration;
        public EmployeeEvaluationService(IEmployeeEvaluationRepsitory repo, IMapper mapper, IConfiguration configuration) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _configuration = configuration;
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
        // Send mail to Section manager 
        public async Task<GenericResponse<bool>> SendEmailToSectionManagerAsync(employee_evaluationDTO evaluation)
        {
            var result = new GenericResponse<bool>();
            string? attachmentPath = null;
            try
            {
                if (evaluation == null)
                {
                    result.Success = false;
                    result.Message = "Không nhận được dữ liệu đánh giá.";
                    return result;
                }

                string mailTo = _configuration.GetSecureValue("ApiSettings:EmailSend").TrimEnd('/', '\\');
                if (string.IsNullOrWhiteSpace(mailTo))
                {
                    result.Success = false;
                    result.Message = "Thiếu cấu hình ApiSettings:EmailSend.";
                    return result;
                }

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "Formatted_Employee_Evaluation.xlsx");
                if (!File.Exists(templatePath))
                {
                    result.Success = false;
                    result.Message = "Không tìm thấy file mẫu Formatted_Employee_Evaluation.xlsx";
                    return result;
                }

                evaluation.created_at ??= DateTime.Now;
                //var entity = _mapper.Map<employee_evaluation>(evaluation);
                //await _repo.AddEmployeeEvaluationAsync(entity);

                attachmentPath = await CreateAttachmentFileAsync(templatePath, evaluation);

                var monthYear = ParseMonthYear(evaluation.evaluation_period, evaluation.created_at);
                var department = string.IsNullOrWhiteSpace(evaluation.department) ? "Unknown" : evaluation.department!.Trim();
                var mailFrom = _configuration.GetSecureValue("ApiSettings:EmailFrom").Trim();
                if (string.IsNullOrWhiteSpace(mailFrom))
                {
                    mailFrom = "360Survey@brothergroup.net";
                }

                var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                {
                    title = $"360 Survey",//-{monthYear.day}/{monthYear.month:D2}/{monthYear.year}
                    mail_from = mailFrom,
                    mail_to = mailTo,
                    mail_cc = string.Empty,
                    mail_bcc = string.Empty,
                    body = BuildMailBody(department, monthYear.month, monthYear.year),
                    attachmentPaths = new List<string> { attachmentPath }
                };

                var sendResult = await EmailSender.SendEmailNotifyCustomSendMultiAttachFileAsync(emailForm);
                result.Success = sendResult.Success;
                result.Data = sendResult.Success;
                result.Message = sendResult.Message;
                if (sendResult.Success)
                {
                    result.Message = "Gửi mail thành công.";
                }


            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
                result.Success = false;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(attachmentPath) && File.Exists(attachmentPath))
                {
                    File.Delete(attachmentPath);
                }
            }
            return result;
        }

        private async Task<string> CreateAttachmentFileAsync(string templatePath, employee_evaluationDTO evaluation)
        {
            var monthYear = ParseMonthYear(evaluation.evaluation_period, evaluation.created_at);
            var safeEmployeeName = MakeSafeFileName(evaluation.employee_name, "NhanVien");
            var outputName = $"Survey_360_{safeEmployeeName}_{monthYear.day}_{monthYear.month:D2}_{monthYear.year}.xlsx";
            var outputPath = Path.Combine(Path.GetTempPath(), outputName);

            using var workbook = new XLWorkbook(templatePath);
            var ws = workbook.Worksheets.First();

            ws.Cell(2, 2).SetValue(evaluation.employee_name ?? string.Empty);
            ws.Cell(3, 2).SetValue(evaluation.department ?? string.Empty);
            ws.Cell(4, 2).SetValue(monthYear.displayText);

            //thông tin bảng 
            // điểm tốt
            ws.Cell(7, 2).SetValue(evaluation.g1_good_point ?? string.Empty);
            ws.Cell(8, 2).SetValue(evaluation.g2_good_point ?? string.Empty);
            ws.Cell(9, 2).SetValue(evaluation.g3_good_point ?? string.Empty);
            ws.Cell(10, 2).SetValue(evaluation.g4_good_point ?? string.Empty);
            ws.Cell(11, 2).SetValue(evaluation.g5_good_point ?? string.Empty);

            // điểm điểm tốt
            ws.Cell(7, 3).SetValue(evaluation.g1_good_score ?? null);
            ws.Cell(8, 3).SetValue(evaluation.g2_good_score ?? null);
            ws.Cell(9, 3).SetValue(evaluation.g3_good_score ?? null);
            ws.Cell(10, 3).SetValue(evaluation.g4_good_score ?? null);
            ws.Cell(11, 3).SetValue(evaluation.g5_good_score ?? null);
            // tình huống điểm tốt
            ws.Cell(7, 4).SetValue(evaluation.g1_example_good ?? string.Empty);
            ws.Cell(8, 4).SetValue(evaluation.g2_example_good ?? string.Empty);
            ws.Cell(9, 4).SetValue(evaluation.g3_example_good ?? string.Empty);
            ws.Cell(10, 4).SetValue(evaluation.g4_example_good ?? string.Empty);
            ws.Cell(11, 4).SetValue(evaluation.g5_example_good ?? string.Empty);

            // đề xuất cải thiện điểm tốt
            ws.Cell(7, 5).SetValue(evaluation.g1_improvement_proposal_good ?? string.Empty);
            ws.Cell(8, 5).SetValue(evaluation.g2_improvement_proposal_good ?? string.Empty);
            ws.Cell(9, 5).SetValue(evaluation.g3_improvement_proposal_good ?? string.Empty);
            ws.Cell(10, 5).SetValue(evaluation.g4_improvement_proposal_good ?? string.Empty);
            ws.Cell(11, 5).SetValue(evaluation.g5_improvement_proposal_good ?? string.Empty);

            // điểm cần cải thiện
            ws.Cell(7, 6).SetValue(evaluation.g1_improve_point ?? string.Empty);
            ws.Cell(8, 6).SetValue(evaluation.g2_improve_point ?? string.Empty);
            ws.Cell(9, 6).SetValue(evaluation.g3_improve_point ?? string.Empty);
            ws.Cell(10, 6).SetValue(evaluation.g4_improve_point ?? string.Empty);
            ws.Cell(11, 6).SetValue(evaluation.g5_improve_point ?? string.Empty);

            // điểm điểm cần cải thiện
            ws.Cell(7, 7).SetValue(evaluation.g1_improve_score ?? null);
            ws.Cell(8, 7).SetValue(evaluation.g2_improve_score ?? null);
            ws.Cell(9, 7).SetValue(evaluation.g3_improve_score ?? null);
            ws.Cell(10, 7).SetValue(evaluation.g4_improve_score ?? null);
            ws.Cell(11, 7).SetValue(evaluation.g5_improve_score ?? null);
            // tình huống điểm cần cải thiện
            ws.Cell(7, 8).SetValue(evaluation.g1_example_improve ?? string.Empty);
            ws.Cell(8, 8).SetValue(evaluation.g2_example_improve ?? string.Empty);
            ws.Cell(9, 8).SetValue(evaluation.g3_example_improve ?? string.Empty);
            ws.Cell(10, 8).SetValue(evaluation.g4_example_improve ?? string.Empty);
            ws.Cell(11, 8).SetValue(evaluation.g5_example_improve ?? string.Empty);

            // đề xuất cải thiện điểm cần cải thiện
            ws.Cell(7, 9).SetValue(evaluation.g1_improvement_proposal_improve ?? string.Empty);
            ws.Cell(8, 9).SetValue(evaluation.g2_improvement_proposal_improve ?? string.Empty);
            ws.Cell(9, 9).SetValue(evaluation.g3_improvement_proposal_improve ?? string.Empty);
            ws.Cell(10, 9).SetValue(evaluation.g4_improvement_proposal_improve ?? string.Empty);
            ws.Cell(11, 9).SetValue(evaluation.g5_improvement_proposal_improve ?? string.Empty);

            ws.Cell(14, 1).SetValue(evaluation.g5_improvement_proposal_good ?? string.Empty);

            await Task.Run(() => workbook.SaveAs(outputPath));
            return outputPath;
        }

        private static (int month, int year, int day, string displayText) ParseMonthYear(string? evaluationPeriod, DateTime? createdAt)
        {
            if (!string.IsNullOrWhiteSpace(evaluationPeriod))
            {
                var normalized = evaluationPeriod.Trim();
                if (DateTime.TryParse(normalized + "-01", out var monthDate) || DateTime.TryParse(normalized, out monthDate))
                {
                    return (monthDate.Month, monthDate.Year, monthDate.Day, $"{monthDate:dd/MM/yyyy}");
                }
            }

            var date = createdAt ?? DateTime.Now;
            return (date.Month, date.Year, date.Day, $"{date:dd/MM/yyyy}");
        }

        private static string MakeSafeFileName(string? value, string fallback)
        {
            var text = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                text = text.Replace(invalid, '_');
            }

            return text.Replace(' ', '_');
        }

        private static string BuildMailBody(string department, int month, int year)
        {
            return $@"<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Arial, 'Noto Sans JP', sans-serif; background-color: #f5f7fb;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f5f7fb;"">
                    <tr>
                        <td align=""center"" style=""padding: 40px 20px;"">
                            <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 16px; box-shadow: 0 2px 8px rgba(0,0,0,0.05);"">
                    
                                <tr>
                                    <td style=""padding: 24px 28px 16px; border-bottom: 2px solid #e9ecef;"">
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 24px 28px;"">
                            
                                        <p style=""margin: 0 0 16px; font-size: 15px; line-height: 1.5; color: #212529;"">
                                            Dear <strong>Manager</strong>,
                                        </p>
                                        <p style=""margin: 0 0 12px; font-size: 14px; line-height: 1.5; color: #495057;"">
                                            This is an automated notification to provide the results of the 360 survey.
                                        </p>
                                        <p style=""margin: 0 0 20px; font-size: 14px; line-height: 1.5; color: #495057;"">
                                            The detailed results and analysis are attached for your review. Please refer to the attached file for more information.
                                        </p>

                                        <p style=""margin: 20px 0 0; font-size: 13px; color: #6c757d;"">
                                            Best regards,<br>
                                            360 Survey
                                        </p>

                                    </td>
                                </tr>

                                <tr>
                                    <td style=""padding: 16px 28px 24px; border-top: 1px solid #e9ecef;"">
                                        <p style=""margin: 0; font-size: 11px; color: #adb5bd;"">
                                            📧 For any inquiries, please contact 2072 (Please do not reply to this email)<br>
                                        </p>
                                    </td>
                                </tr>

                            </table>
                        </td>
                    </tr>
                </table>
            </body>";
        }
    }
}
