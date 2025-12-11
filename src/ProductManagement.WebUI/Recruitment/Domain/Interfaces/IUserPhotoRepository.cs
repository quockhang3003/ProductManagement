using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserPhotoRepository
    {
        Task<UserPhoto?> GetByUserIdAsync(int userId);
        Task<UserPhoto?> GetByUserEmailAsync(string userEmail); 
        Task<int> CreateAsync(UserPhoto photo);
        Task<bool> UpdateAsync(UserPhoto photo);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeactivateOldPhotosAsync(int userId);
        Task<bool> DeactivateOldPhotosByEmailAsync(string userEmail);
    }

}
