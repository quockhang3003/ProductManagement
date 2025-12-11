using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class AttachmentsRepository :IAttachmentsRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public AttachmentsRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }


        public async Task<IEnumerable<Attachments>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<Attachments>("SELECT * FROM Attachments ORDER BY Id");
        }


        public async Task<IEnumerable<Attachments>> GetByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<Attachments>(
                "SELECT * FROM Attachments WHERE UserId = @UserId ORDER BY Id",
                new { UserId = userId });
        }


        public async Task<Attachments?> GetByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Attachments>(
                "SELECT * FROM Attachments WHERE Id = @Id",
                new { Id = id });
        }


        public async Task<int> CreateAsync(Attachments attachment)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = @"
                INSERT INTO Attachments (UserID, AttachmentName, FileName, FileData, ContentType, UploadedAt)
                VALUES (@UserID, @AttachmentName, @FileName, @FileData, @ContentType, @UploadedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.QuerySingleAsync<int>(sql, attachment);
        }


        public async Task<bool> UpdateAsync(Attachments attachment)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = @"
                UPDATE Attachments 
                SET AttachmentName = @AttachmentName, FileName = @FileName, FileData = @FileData, ContentType = @ContentType
                WHERE Id = @Id";

            var affectedRows = await conn.ExecuteAsync(sql, attachment);
            return affectedRows > 0;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = "DELETE FROM Attachments WHERE Id = @Id";

            var affectedRows = await conn.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }

    }
}
