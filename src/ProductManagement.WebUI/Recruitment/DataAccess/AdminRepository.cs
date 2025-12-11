using Dapper;
using Domain.Entities;
using Domain.Interfaces;
namespace DataAccess 
{ 
    public class AdminRepository : IAdminRepository 
    { 
        private readonly IDbConnectionFactory _dbFactory;
        public AdminRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task<Admin?> GetByUsernameAsync(string username) 
        { 
            using var conn = _dbFactory.CreateConnection(); 
            var sql = "SELECT * FROM Admin WHERE Username = @Username AND IsActive = 1";
            return await conn.QueryFirstOrDefaultAsync<Admin>(sql, new { Username = username }); 
        } 
        public async Task<Admin?> GetByEmailAsync(string email) 
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM Admin WHERE Email = @Email AND IsActive = 1";
            return await conn.QueryFirstOrDefaultAsync<Admin>(sql, new { Email = email }); 
        } 
        public async Task<IEnumerable<Admin>> GetAllAsync() 
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<Admin>("SELECT * FROM Admin WHERE IsActive = 1"); 
        } 
        public async Task AddAsync(Admin admin)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"INSERT INTO Admin (Username, PasswordHash, Email, FullName, CreatedAt, IsActive) VALUES (@Username, @PasswordHash, @Email, @FullName, @CreatedAt, @IsActive)"; 
            await conn.ExecuteAsync(sql, admin);
        }
    }
}