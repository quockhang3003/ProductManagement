using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class UniversityService
    {
        private readonly IUniversityRepository _repo;
        public UniversityService(IUniversityRepository repo )
        {
            _repo = repo;
        }
        public async Task<IEnumerable<University>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<IEnumerable<University>> GetByUserIdAsync(int userId) => await _repo.GetByUserIdAsync(userId);
    }
}

