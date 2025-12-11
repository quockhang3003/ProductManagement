using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class TranscriptRepository : ITranscriptRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public TranscriptRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<Transcript>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = @"
                SELECT 
                    t.Id, 
                    t.UserId, 
                    t.UniversityCode,
                    t.FileName, 
                    t.FileData, 
                    t.ContentType, 
                    t.UploadedAt,
                    sc.Code,
                    sc.DisplayName as UniversityName
                FROM Transcript t
                LEFT JOIN SystemConfiguration sc 
                    ON t.UniversityCode = sc.Code 
                    AND sc.Type = 'University'
                ORDER BY t.Id";

            return await conn.QueryAsync<Transcript, SystemConfiguration, Transcript>(
                sql,
                (transcript, sysConfig) =>
                {
                    if (sysConfig != null)
                    {
                        transcript.University = new University
                        {
                            UniversityName = sysConfig.DisplayName
                        };
                    }
                    return transcript;
                },
                splitOn: "Code");
        }

        public async Task<IEnumerable<Transcript>> GetByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();

            const string sql = @"
        SELECT 
            t.Id, 
            t.UserId, 
            t.UniversityCode,
            t.FileName, 
            t.ContentType, 
            t.UploadedAt
        FROM Transcript t
        WHERE t.UserId = @UserId
        ORDER BY t.UploadedAt DESC";

            var transcripts = (await conn.QueryAsync<Transcript>(sql, new { UserId = userId })).ToList();

            if (!transcripts.Any())
                return transcripts;

            // Get university names separately
            var universityCodes = transcripts.Select(t => t.UniversityCode).Distinct().ToList();

            const string sqlNames = @"
        SELECT Code, DisplayName
        FROM SystemConfiguration
        WHERE Type = 'University' 
          AND IsActive = 1
          AND Code IN @Codes";

            var universityNames = await conn.QueryAsync<(string Code, string DisplayName)>(
                sqlNames,
                new { Codes = universityCodes }
            );

            var nameDict = universityNames.ToDictionary(x => x.Code, x => x.DisplayName, StringComparer.OrdinalIgnoreCase);

            // Assign names
            foreach (var transcript in transcripts)
            {
                if (nameDict.TryGetValue(transcript.UniversityCode, out var displayName))
                {
                    transcript.UniversityName = displayName;
                    Console.WriteLine($"[TranscriptRepo] Found name for {transcript.UniversityCode}: {displayName}");
                }
                else
                {
                    transcript.UniversityName = transcript.UniversityCode;
                    Console.WriteLine($"[TranscriptRepo] No name found for {transcript.UniversityCode}, using code as fallback");
                }
            }

            return transcripts;
        }





        public async Task<Transcript?> GetByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = @"
                SELECT 
                    t.Id, 
                    t.UserId, 
                    t.UniversityCode,
                    t.FileName, 
                    t.FileData, 
                    t.ContentType, 
                    t.UploadedAt
                FROM Transcript t
                WHERE t.Id = @Id";

            return await conn.QueryFirstOrDefaultAsync<Transcript>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(Transcript transcript)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = @"
                INSERT INTO Transcript (UserId, UniversityCode, FileName, FileData, ContentType, UploadedAt)
                VALUES (@UserId, @UniversityCode, @FileName, @FileData, @ContentType, @UploadedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.QuerySingleAsync<int>(sql, transcript);
        }

        public async Task<bool> UpdateAsync(Transcript transcript)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = @"
                UPDATE Transcript 
                SET FileName = @FileName, 
                    FileData = @FileData, 
                    ContentType = @ContentType
                WHERE Id = @Id";

            var affectedRows = await conn.ExecuteAsync(sql, transcript);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = "DELETE FROM Transcript WHERE Id = @Id";

            var affectedRows = await conn.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
        private class SystemConfigData
        {
            public int SystemConfigId { get; set; }
            public string SystemConfigCode { get; set; } = string.Empty;
            public string SystemConfigDisplayName { get; set; } = string.Empty;
        }
    }
}

