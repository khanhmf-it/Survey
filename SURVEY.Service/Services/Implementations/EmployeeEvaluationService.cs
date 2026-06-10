using AutoMapper;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
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

                string mailTo = (_configuration["ApiSettings:EmailSend"] ?? string.Empty).TrimEnd('/', '\\');
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
                var entity = _mapper.Map<employee_evaluation>(evaluation);
                await _repo.AddEmployeeEvaluationAsync(entity);

                attachmentPath = await CreateAttachmentFileAsync(templatePath, evaluation);

                var monthYear = ParseMonthYear(evaluation.evaluation_period, evaluation.created_at);
                var department = string.IsNullOrWhiteSpace(evaluation.department) ? "Unknown" : evaluation.department!.Trim();

                var emailForm = new EmailFormNetMailCustomSendMultiAttachFile
                {
                    title = $"Kết quả đánh giá 360-{monthYear.day}/{monthYear.month:D2}/{monthYear.year}",
                    mail_from = (_configuration["ApiSettings:EmailFrom"] ?? "noreply@brother.co.jp").Trim(),
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
            var outputName = $"Ket_qua_danh_gia_360_{safeEmployeeName}_{monthYear.day}_{monthYear.month:D2}_{monthYear.year}.xlsx";
            var outputPath = Path.Combine(Path.GetTempPath(), outputName);

            using var workbook = new XLWorkbook(templatePath);
            var ws = workbook.Worksheets.First();

            SetValueByLabel(ws, "Tên nhân viên / 氏名", evaluation.employee_name);
            SetValueByLabel(ws, "Nhóm / チーム", evaluation.department);
            SetValueByLabel(ws, "Thời gian / 評価期間", monthYear.displayText);

            FillGroupRow(ws, "1. Hợp tác & làm việc nhóm / チームワーク", evaluation.g1_good_point, evaluation.g1_good_score, evaluation.g1_example_good,evaluation.g1_improvement_proposal_good, evaluation.g1_improve_point, evaluation.g1_improve_score, evaluation.g1_example_improve, evaluation.g1_improvement_proposal_improve);
            FillGroupRow(ws, "2. Giao tiếp & chia sẻ thông tin / コミュニケーション", evaluation.g2_good_point, evaluation.g2_good_score, evaluation.g2_example_good, evaluation.g2_improvement_proposal_good, evaluation.g2_improve_point, evaluation.g2_improve_score, evaluation.g2_example_improve, evaluation.g2_improvement_proposal_improve);
            FillGroupRow(ws, "3. Đào tạo & hỗ trợ người khác / 指導・サポート", evaluation.g3_good_point, evaluation.g3_good_score, evaluation.g3_example_good, evaluation.g3_improvement_proposal_good, evaluation.g3_improve_point, evaluation.g3_improve_score, evaluation.g3_example_improve, evaluation.g3_improvement_proposal_improve);
            FillGroupRow(ws, "4. Trách nhiệm & tự chủ / 責任感・主体性", evaluation.g4_good_point, evaluation.g4_good_score, evaluation.g4_example_good, evaluation.g4_improvement_proposal_good, evaluation.g4_improve_point, evaluation.g4_improve_score, evaluation.g4_example_improve, evaluation.g4_improvement_proposal_improve);
            FillGroupRow(ws, "5. Kỷ luật & tuân thủ thời gian / 規律・時間遵守", evaluation.g5_good_point, evaluation.g5_good_score, evaluation.g5_example_good, evaluation.g5_improvement_proposal_good, evaluation.g5_improve_point, evaluation.g5_improve_score, evaluation.g5_example_improve, evaluation.g5_improvement_proposal_improve);

            SetValueBelowByLabel(ws, "Nhận xét chung / 総合コメント", evaluation.improvement_proposal);

            await Task.Run(() => workbook.SaveAs(outputPath));
            return outputPath;
        }

        private static void FillGroupRow(
            IXLWorksheet ws,
            string rowLabel,
            string? goodPoint,
            int? goodScore,
            string? goodExample,
            string? goodImproveProposal,
            string? improvePoint,
            int? improveScore,
            string? improveExample,
            string? improveProposal)
        {
            var labelCell = ws.CellsUsed(c => c.GetString().Trim() == rowLabel).FirstOrDefault();
            if (labelCell == null)
            {
                return;
            }

            var row = labelCell.Address.RowNumber;
            ws.Cell(row, 2).Value = goodPoint ?? string.Empty;
            ws.Cell(row, 3).Value = goodScore?.ToString() ?? string.Empty;
            ws.Cell(row, 4).Value = goodExample ?? string.Empty;
            ws.Cell(row, 5).Value = goodImproveProposal ?? string.Empty;
            ws.Cell(row, 6).Value = improvePoint ?? string.Empty;
            ws.Cell(row, 7).Value = improveScore?.ToString() ?? string.Empty;
            ws.Cell(row, 8).Value = improveExample ?? string.Empty;
            ws.Cell(row, 9).Value = improveProposal ?? string.Empty;
        }

        private static void SetValueByLabel(IXLWorksheet ws, string label, string? value)
        {
            var cell = ws.CellsUsed(c => c.GetString().Trim() == label).FirstOrDefault();
            if (cell == null)
            {
                return;
            }

            ws.Cell(cell.Address.RowNumber, cell.Address.ColumnNumber + 1).Value = value ?? string.Empty;
        }

        private static void SetValueBelowByLabel(IXLWorksheet ws, string label, string? value)
        {
            var cell = ws.CellsUsed(c => c.GetString().Trim() == label).FirstOrDefault();
            if (cell == null)
            {
                return;
            }

            ws.Cell(cell.Address.RowNumber + 1, cell.Address.ColumnNumber).Value = value ?? string.Empty;
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
                                    <h2 style=""margin: 8px 0 4px; color: #4361ee;"">Kết quả khảo sát đánh giá 360</h2>
                                    <p style=""margin: 0; color: #4361ee; font-size: 14px;"">360度評価調査結果</p>
                                </td>
                            </tr>
                            <td style=""padding: 24px 28px;"">
                                <p style=""margin: 0 0 16px; font-size: 15px; line-height: 1.5; color: #212529;"">Kính gửi <strong>Trưởng phòng</strong>,</p>
                                <p style=""margin: 0 0 12px; font-size: 14px; line-height: 1.5; color: #495057;"">Đính kèm là file Excel tổng hợp kết quả đánh giá của thành viên trong phòng.</p>
                                <p style=""margin: 0 0 20px; font-size: 14px; line-height: 1.5; color: #495057;"">File bao gồm: điểm chi tiết nhân viên, điểm trung bình theo nhóm năng lực, nhận xét và đề xuất cải thiện. Anh/chị vui lòng xem file đính kèm để biết chi tiết.</p>
                                <div style=""border-top: 1px dashed #dee2e6; margin: 20px 0;""></div>
                                <p style=""margin: 0 0 16px; font-size: 15px; line-height: 1.5; color: #212529;""><strong>部長様</strong></p>
                                <p style=""margin: 0 0 12px; font-size: 14px; line-height: 1.5; color: #495057;"">添付は、部署メンバーの評価結果をまとめたExcelファイルです。</p>
                                <p style=""margin: 0 0 20px; font-size: 14px; line-height: 1.5; color: #495057;"">ファイルの内容：個人別の詳細スコア、コンピテンシー別の平均点、コメント、改善提案などです。詳細は添付ファイルをご確認ください。</p>
                                <p style=""margin: 20px 0 0; font-size: 13px; color: #6c757d;"">Xin cảm ơn! Chúc anh chị ngày mới tốt lành 🎉<br>ありがとうございます！素敵な一日をお過ごしください！🎉</p>
                            </td>
                            <tr>
                                <td style=""padding: 16px 28px 24px; border-top: 1px solid #e9ecef;"">
                                    <p style=""margin: 0; font-size: 11px; color: #adb5bd;"">📧 Mọi thắc mắc vui lòng liên hệ 2072 (Vui lòng không phản hồi mail này)<br>📧ご質問は2072までご連絡ください（このメールには返信しないでください）</p>
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
