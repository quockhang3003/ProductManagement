using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {

        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        Task<bool> ExistsEmailAsync(string email);
        Task<bool> ExistsIDCardAsync(string IDCard);
        Task UpdateAsync(int id);
        Task DeleteAsync(int id);
        Task<User?> GetByIdAsync(int id);
    }
}
