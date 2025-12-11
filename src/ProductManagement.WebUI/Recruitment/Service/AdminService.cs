using BCrypt.Net;
using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
namespace Service 
{
    public class AdminService 
    { 
        private readonly IAdminRepository _adminRepo; 
        public AdminService(IAdminRepository adminRepo)
        { 
            _adminRepo = adminRepo;
        } 
        public async Task<Admin?> LoginAsync(string email, string password)
        { 
            var admin = await _adminRepo.GetByEmailAsync(email); 
            if (admin == null) return null; 
            bool isValid = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash); 
            return isValid ? admin : null; 
        } 
        public async Task<Admin?> GetByUsernameAsync(string username) 
        {
            return await _adminRepo.GetByUsernameAsync(username);
        } 
        public async Task<IEnumerable<Admin>> GetAllAsync() 
        {
            return await _adminRepo.GetAllAsync(); 
        }
    } 
}