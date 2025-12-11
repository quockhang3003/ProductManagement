using Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateSearchController : ControllerBase
    {
        private readonly CandidateSearchService _service;

        public CandidateSearchController(CandidateSearchService service)
        {
            _service = service;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] CandidateSearchFilter filter)
        {
            try
            {
                var result = await _service.SearchCandidatesAsync(filter);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = $"Search error: {e.Message}" });
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportToExcel([FromQuery] CandidateSearchFilter filter)
        {
            try
            {
                var result = await _service.SearchCandidatesAsync(filter);

                if (result.Candidates == null || result.Candidates.Count == 0)
                    return NotFound(new { Message = "No candidates found to export" });

                var grouped = result.Candidates
                    .GroupBy(c => c.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        First = g.First(),
                        SubmittedGroups = g
                            .GroupBy(x => x.SubmittedOn.Date)
                            .OrderByDescending(sg => sg.Key)
                            .Select(sg => new
                            {
                                SubmittedOn = sg.Key,
                                Items = sg.OrderByDescending(x => x.GPA).ToList()
                            }).ToList()
                    }).ToList();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Candidates");

                    var headers = new[]
                    {
                        "Name","DOB","Gender","Status","Office","Mobile","Email","IDCardNo",
                        "University","Major","GPA","Submitted On"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                    }

                    using (var range = worksheet.Cells[1, 1, 1, headers.Length])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Size = 12;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(166, 164, 255));
                        range.Style.Font.Color.SetColor(Color.Black);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    int row = 2;
                    foreach (var userGroup in grouped)
                    {

                        int totalRowsForUser = userGroup.SubmittedGroups.Sum(sg => sg.Items.Count);
                        int userStartRow = row;
                        int userEndRow = row + totalRowsForUser - 1;

                        bool personHeaderWritten = false;

                        foreach (var submittedGroup in userGroup.SubmittedGroups)
                        {
                            int submittedCount = submittedGroup.Items.Count;
                            int submittedStartRow = row;
                            int submittedEndRow = row + submittedCount - 1;

                            bool submittedHeaderWritten = false;

                            for (int i = 0; i < submittedGroup.Items.Count; i++)
                            {
                                var candidate = submittedGroup.Items[i];


                                worksheet.Cells[row, 9].Value = candidate.University ?? "";
                                worksheet.Cells[row, 10].Value = candidate.Major ?? "";

                                if (candidate.GPA != null && double.TryParse(candidate.GPA.ToString(), out double gpaVal))
                                    worksheet.Cells[row, 11].Value = gpaVal;
                                else
                                    worksheet.Cells[row, 11].Value = candidate.GPA?.ToString() ?? "";


                                if (!submittedHeaderWritten)
                                {
                                    worksheet.Cells[row, 12].Value = submittedGroup.SubmittedOn.ToString("dd/MM/yyyy");
                                    if (submittedCount > 1)
                                    {
                                        worksheet.Cells[submittedStartRow, 12, submittedEndRow, 12].Merge = true;
                                        worksheet.Cells[submittedStartRow, 12, submittedEndRow, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                        worksheet.Cells[submittedStartRow, 12, submittedEndRow, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    }
                                    submittedHeaderWritten = true;
                                }


                                if (!personHeaderWritten)
                                {
                                    worksheet.Cells[userStartRow, 1].Value = userGroup.First.FullName ?? "";
                                    worksheet.Cells[userStartRow, 2].Value = userGroup.First.DateOfBirth?.ToString("dd/MM/yyyy") ?? "";
                                    worksheet.Cells[userStartRow, 3].Value = userGroup.First.Gender ?? "";
                                    worksheet.Cells[userStartRow, 4].Value = userGroup.First.Status ?? "";
                                    worksheet.Cells[userStartRow, 5].Value = userGroup.First.Office ?? "";
                                    worksheet.Cells[userStartRow, 6].Value = userGroup.First.Mobile ?? "";
                                    worksheet.Cells[userStartRow, 7].Value = userGroup.First.Email ?? "";
                                    worksheet.Cells[userStartRow, 8].Value = userGroup.First.IDCardNo ?? "";

                                    if (totalRowsForUser > 1)
                                    {

                                        for (int col = 1; col <= 8; col++)
                                        {
                                            worksheet.Cells[userStartRow, col, userEndRow, col].Merge = true;
                                            worksheet.Cells[userStartRow, col, userEndRow, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                            worksheet.Cells[userStartRow, col, userEndRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                        }
                                    }

                                    personHeaderWritten = true;
                                }

                                using (var dataRange = worksheet.Cells[row, 1, row, 12])
                                {
                                    dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                }

                                row++;
                            }
                        }
                    }

                    worksheet.Cells.AutoFitColumns();

                    for (int col = 1; col <= 12; col++)
                    {
                        if (worksheet.Column(col).Width < 10)
                            worksheet.Column(col).Width = 10;
                        if (worksheet.Column(col).Width > 50)
                            worksheet.Column(col).Width = 50;
                    }

                    worksheet.View.FreezePanes(2, 1);

                    worksheet.Cells[row, 1].Value = "Total Candidates:";
                    worksheet.Cells[row, 2].Value = result.TotalCount;
                    using (var summaryRange = worksheet.Cells[row, 1, row, 2])
                    {
                        summaryRange.Style.Font.Bold = true;
                        summaryRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        summaryRange.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    }

                    row++;
                    worksheet.Cells[row, 1].Value = "Exported On:";
                    worksheet.Cells[row, 2].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    var content = package.GetAsByteArray();
                    var fileName = $"Candidates_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new { Message = $"Export error: {e.Message}", StackTrace = e.StackTrace });
            }
        }
        [HttpPatch("deactivate/{userId}")]
        public async Task<IActionResult> DeactivateCandidate(int userId)
        {
            try
            {
                var result = await _service.DeactivateCandidateAsync(userId);

                if (result)
                    return Ok(new { Message = "Candidate deactivated successfully" });
                else
                    return NotFound(new { Message = "Candidate not found" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { Message = $"Deactivate error: {e.Message}" });
            }
        }

        [HttpPatch("activate/{userId}")]
        public async Task<IActionResult> ActivateCandidate(int userId)
        {
            try
            {
                var result = await _service.ActivateCandidateAsync(userId);

                if (result)
                    return Ok(new { Message = "Candidate activated successfully" });
                else
                    return NotFound(new { Message = "Candidate not found" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { Message = $"Activate error: {e.Message}" });
            }
        }
    }
}
