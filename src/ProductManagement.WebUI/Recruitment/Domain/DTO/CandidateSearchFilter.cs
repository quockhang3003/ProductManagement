using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class CandidateSearchFilter
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? University { get; set; }
        public string? Major { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Office { get; set; }
        public string? Status { get; set; }
        public string? Gender { get; set; }
        public string? FirstPreference { get; set; }
        public string? SecondPreference { get; set; }
        public int? ProgramId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class CandidateSearchResult
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Status { get; set; }
        public string Office { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string IDCardNo { get; set; }
        public string University { get; set; }
        public string Major { get; set; }
        public string GPA { get; set; }
        public DateTime SubmittedOn { get; set; }
    }

    public class CandidateSearchResponse
    {
        public List<CandidateSearchResult> Candidates { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
