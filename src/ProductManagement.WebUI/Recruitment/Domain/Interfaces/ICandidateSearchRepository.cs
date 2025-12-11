using Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ICandidateSearchRepository
    {
        Task<CandidateSearchResponse> SearchCandidatesAsync(CandidateSearchFilter filter);
        Task<bool> UpdateCandidateStatusAsync(int userId, int statusId);
        Task<bool> UpdateHeardAboutStatusAsync(int userId, int statusId);
    }
}
