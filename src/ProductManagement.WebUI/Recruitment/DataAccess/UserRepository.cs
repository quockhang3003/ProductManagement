using Dapper;
using System.Data;
using Domain.Interfaces;
using Domain.Entities;
using System.Reflection.Metadata.Ecma335;
using BCrypt.Net;

namespace DataAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public UserRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<User>("SELECT * FROM Users");
        }

        public async Task AddAsync(User user)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"INSERT INTO Users (
                            PreferableOfficeLocation, FirstPreference, SecondPreference, 
                            LastName, FirstName, VietnameseName, Gender, Nationality,
                            DateOfBirth, PlaceOfBirth, Email, 
                            PasswordHash, IDCardNoEncrypted,
                            DateOfIssue, PlaceOfIssue, Mobile, Street, Ward, City, CurrentAddress, CreatedAt) 
                        VALUES (
                            @PreferableOfficeLocation, @FirstPreference, @SecondPreference, 
                            @LastName, @FirstName, @VietnameseName, @Gender, @Nationality, @DateOfBirth,
                            @PlaceOfBirth, @Email, 
                            @PasswordHash, @IDCardNoEncrypted,
                            @DateOfIssue, @PlaceOfIssue, @Mobile, @Street, @Ward, @City, @CurrentAddress, @CreatedAt)";

            await conn.ExecuteAsync(sql, user);

        }

        public async Task<bool> ExistsEmailAsync(string email)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            return await conn.ExecuteScalarAsync<int>(sql, new { Email = email }) > 0;
        }

        public async Task<bool> ExistsIDCardAsync(string IDCard)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"SELECT PasswordHash 
                FROM Users 
                WHERE PasswordHash IS NOT NULL 
                AND PasswordHash LIKE '$2%'";

            var hashes = await conn.QueryAsync<string>(sql);

            foreach (var hash in hashes)
            {
                try
                {
                    if (BCrypt.Net.BCrypt.Verify(IDCard, hash))
                    {
                        return true;
                    }
                }
                catch (BCrypt.Net.SaltParseException ex)
                {
                    Console.WriteLine($"[ERROR] BCrypt verify failed for hash: {ex.Message}");
                    continue;
                }
            }

            return false;
        }


        public async Task UpdateAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE Users SET
                        PreferableOfficeLocation = @PreferableOfficeLocation,
                        FirstPreference = @FirstPreference,
                        SecondPreference = @SecondPreference,
                        LastName = @LastName,
                        FirstName = @FirstName,
                        VietnameseName = @VietnameseName,
                        Gender = @Gender,
                        Nationality = @Nationality,
                        DateOfBirth = @DateOfBirth,
                        PlaceOfBirth = @PlaceOfBirth,
                        Email = @Email,
                        DateOfIssue = @DateOfIssue,
                        PlaceOfIssue = @PlaceOfIssue,
                        Mobile = @Mobile,
                        Street = @Street,
                        Ward = @Ward,
                        City = @City,
                        CurrentAddress = @CurrentAddress,
                        UpdatedAt = @UpdatedAt
                        WHERE Id = @Id";

            await conn.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE Users SET DeletedAt = @DeletedAt WHERE Id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Id = @Id";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }
    }
}
