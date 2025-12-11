using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IHeardAboutRepository
    {
        Task<HeardAbout?> GetByUserIdAsync(int userId);
        Task<int> CreateAsync(HeardAbout heardAbout);
        Task<bool> UpdateAsync(HeardAbout heardAbout);
        Task<bool> DeleteAsync(int id);
    }

}
