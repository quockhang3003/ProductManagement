using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class TranscriptService
    {
        private readonly ITranscriptRepository _repo;

        public TranscriptService(ITranscriptRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Transcript>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<IEnumerable<Transcript>> GetByUserIdAsync(int userId) => await _repo.GetByUserIdAsync(userId);
        public async Task<Transcript?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);
        public async Task<int> CreateAsync(Transcript transcript) => await _repo.CreateAsync(transcript);
        public async Task<bool> UpdateAsync(Transcript transcript) => await _repo.UpdateAsync(transcript);
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}
