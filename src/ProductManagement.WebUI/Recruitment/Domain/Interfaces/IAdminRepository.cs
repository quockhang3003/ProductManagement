using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAdminRepository 
    { 
        Task<Admin?> GetByUsernameAsync(string username);
        Task<Admin?> GetByEmailAsync(string email); 
        Task<IEnumerable<Admin>> GetAllAsync(); 
        Task AddAsync(Admin admin); 
    }
}
