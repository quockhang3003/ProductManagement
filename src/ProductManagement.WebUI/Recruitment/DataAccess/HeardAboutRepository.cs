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
    public class HeardAboutRepository : IHeardAboutRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public HeardAboutRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<HeardAbout?> GetByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM HeardAbout WHERE UserId = @UserId";
            return await conn.QueryFirstOrDefaultAsync<HeardAbout>(sql, new { UserId = userId });
        }

        public async Task<int> CreateAsync(HeardAbout heardAbout)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"INSERT INTO HeardAbout (
                           UserId,
                           KpmgWebsite, UniversityClubs, Others, FacebookGroup,
                           UniversityWebsite, KpmgPresentation, KpmgSocialMedia,
                           Tiktok, CareerTalk, Professor, CampusAmbassadors,
                           AgreeToTerms, CreatedAt
                       ) VALUES (
                           @UserId,
                           @KpmgWebsite, @UniversityClubs, @Others, @FacebookGroup,
                           @UniversityWebsite, @KpmgPresentation, @KpmgSocialMedia,
                           @Tiktok, @CareerTalk, @Professor, @CampusAmbassadors,
                           @AgreeToTerms, @CreatedAt
                       );
                       SELECT CAST(SCOPE_IDENTITY() as int)";
            return await conn.ExecuteScalarAsync<int>(sql, heardAbout);
        }

        public async Task<bool> UpdateAsync(HeardAbout heardAbout)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE HeardAbout SET
                           KpmgWebsite = @KpmgWebsite,
                           UniversityClubs = @UniversityClubs,
                           Others = @Others,
                           FacebookGroup = @FacebookGroup,
                           UniversityWebsite = @UniversityWebsite,
                           KpmgPresentation = @KpmgPresentation,
                           KpmgSocialMedia = @KpmgSocialMedia,
                           Tiktok = @Tiktok,
                           CareerTalk = @CareerTalk,
                           Professor = @Professor,
                           CampusAmbassadors = @CampusAmbassadors,
                           AgreeToTerms = @AgreeToTerms
                       WHERE Id = @Id";
            var result = await conn.ExecuteAsync(sql, heardAbout);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "DELETE FROM HeardAbout WHERE Id = @Id";
            var result = await conn.ExecuteAsync(sql, new { Id = id });
            return result > 0;
        }

    }
}
