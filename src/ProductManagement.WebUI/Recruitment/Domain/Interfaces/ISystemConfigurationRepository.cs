using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ISystemConfigurationRepository
    {
        Task<IEnumerable<SystemConfiguration>> GetAllAsync();
        Task<IEnumerable<SystemConfiguration>> GetByTypeAsync(string type);
        Task<IEnumerable<SystemConfiguration>> GetActiveByTypeAsync(string type);
        Task<SystemConfiguration> GetByIdAsync(int id);
        Task<int> AddAsync(SystemConfiguration config);
        Task<bool> UpdateAsync(SystemConfiguration config);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string type, string code, int? excludeId = null);
        Task<IEnumerable<string>> GetAllTypesAsync();
    }
}
