using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class SystemConfigurationService
    {
        private readonly ISystemConfigurationRepository _repo;

        public SystemConfigurationService(ISystemConfigurationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<SystemConfigurationGroupDTO>> GetAllGroupedAsync()
        {
            var allConfigs = await _repo.GetAllAsync();

            var grouped = allConfigs
                .GroupBy(c => c.Type)
                .Select(g => new SystemConfigurationGroupDTO
                {
                    Type = g.Key,
                    Items = g.Select(c => new SystemConfigurationDTO
                    {
                        Id = c.Id,
                        Type = c.Type,
                        Code = c.Code,
                        DisplayName = c.DisplayName,
                        DisplayOrder = c.DisplayOrder,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    }).ToList()
                })
                .OrderBy(g => g.Type)
                .ToList();

            return grouped;
        }

        public async Task<IEnumerable<SystemConfigurationDTO>> GetByTypeAsync(string type)
        {
            var configs = await _repo.GetByTypeAsync(type);
            return configs.Select(c => new SystemConfigurationDTO
            {
                Id = c.Id,
                Type = c.Type,
                Code = c.Code,
                DisplayName = c.DisplayName,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });
        }

        public async Task<IEnumerable<SystemConfigurationDTO>> GetActiveByTypeAsync(string type)
        {
            var configs = await _repo.GetActiveByTypeAsync(type);
            return configs.Select(c => new SystemConfigurationDTO
            {
                Id = c.Id,
                Type = c.Type,
                Code = c.Code,
                DisplayName = c.DisplayName,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive
            });
        }

        public async Task<(bool Success, string Message, int Id)> AddAsync(CreateSystemConfigurationDTO dto)
        {
            // Check if already exists
            var exists = await _repo.ExistsAsync(dto.Type, dto.Code);
            if (exists)
            {
                return (false, $"Configuration with Type '{dto.Type}' and Code '{dto.Code}' already exists", 0);
            }

            var config = new SystemConfiguration
            {
                Type = dto.Type.Trim(),
                Code = dto.Code.Trim(),
                DisplayName = dto.DisplayName.Trim(),
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var id = await _repo.AddAsync(config);
            return (true, "Configuration added successfully", id);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(UpdateSystemConfigurationDTO dto)
        {
            var existing = await _repo.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                return (false, "Configuration not found");
            }

            existing.DisplayName = dto.DisplayName.Trim();
            existing.DisplayOrder = dto.DisplayOrder;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.Now;

            var result = await _repo.UpdateAsync(existing);
            return result
                ? (true, "Configuration updated successfully")
                : (false, "Failed to update configuration");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var result = await _repo.DeleteAsync(id);
            return result
                ? (true, "Configuration deleted successfully")
                : (false, "Failed to delete configuration");
        }

        public async Task<IEnumerable<string>> GetAllTypesAsync()
        {
            return await _repo.GetAllTypesAsync();
        }
    }
}
