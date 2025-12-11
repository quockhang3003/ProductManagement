using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITranscriptRepository
    {
        Task<IEnumerable<Transcript>> GetAllAsync();
        Task<IEnumerable<Transcript>> GetByUserIdAsync(int userId);
        Task<Transcript?> GetByIdAsync(int id);
        Task<int> CreateAsync(Transcript transcript);
        Task<bool> UpdateAsync(Transcript transcript);
        Task<bool> DeleteAsync(int id);
    }
}
