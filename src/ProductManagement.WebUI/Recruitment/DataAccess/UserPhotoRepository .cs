using Dapper;
using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
   public class UserPhotoRepository : IUserPhotoRepository
   {
       private readonly IDbConnectionFactory _dbFactory;
      
       public UserPhotoRepository(IDbConnectionFactory dbFactory)
       {
           _dbFactory = dbFactory;
       }


       public async Task<UserPhoto?> GetByUserIdAsync(int userId)
       {
           using var conn = _dbFactory.CreateConnection();
           return await conn.QueryFirstOrDefaultAsync<UserPhoto>(
               "SELECT * FROM UserPhoto WHERE UserId = @UserId AND IsActive = 1",
               new { UserId = userId });
       }


       public async Task<int> CreateAsync(UserPhoto photo)
       {
           using var conn = _dbFactory.CreateConnection();
           var sql = @"INSERT INTO UserPhoto (UserId, FileName, FilePath, ContentType, FileSize, UploadedAt, IsActive, FileData)
                      VALUES (@UserId, @FileName, @FilePath, @ContentType, @FileSize, @UploadedAt, @IsActive, @FileData);
                      SELECT CAST(SCOPE_IDENTITY() as int);";
           return await conn.QuerySingleAsync<int>(sql, photo);
       }


       public async Task<bool> UpdateAsync(UserPhoto photo)
       {
           using var conn = _dbFactory.CreateConnection();
           var sql = @"UPDATE UserPhoto
                      SET FileName = @FileName, FilePath = @FilePath, ContentType = @ContentType,
                          FileSize = @FileSize, UploadedAt = @UploadedAt
                      WHERE Id = @Id";
           var rowsAffected = await conn.ExecuteAsync(sql, photo);
           return rowsAffected > 0;
       }


       public async Task<bool> DeleteAsync(int id)
       {
           using var conn = _dbFactory.CreateConnection();
           var rowsAffected = await conn.ExecuteAsync("DELETE FROM UserPhoto WHERE Id = @Id", new { Id = id });
           return rowsAffected > 0;
       }


       public async Task<UserPhoto?> GetByUserEmailAsync(string userEmail)
       {
           using var conn = _dbFactory.CreateConnection();
           // Assuming you have a User table to map email to userId
           var sql = @"SELECT p.* FROM UserPhoto p
                      INNER JOIN Users u ON p.UserId = u.Id
                      WHERE u.Email = @UserEmail AND p.IsActive = 1";
           return await conn.QueryFirstOrDefaultAsync<UserPhoto>(sql, new { UserEmail = userEmail });
       }


       public async Task<bool> DeactivateOldPhotosByEmailAsync(string userEmail)
       {
           using var conn = _dbFactory.CreateConnection();
           var sql = @"UPDATE UserPhoto SET IsActive = 0
                      FROM UserPhoto p
                      INNER JOIN Users u ON p.UserId = u.Id
                      WHERE u.Email = @UserEmail";
           var rowsAffected = await conn.ExecuteAsync(sql, new { UserEmail = userEmail });
           return rowsAffected > 0;
       }

        public Task<bool> DeactivateOldPhotosAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}

