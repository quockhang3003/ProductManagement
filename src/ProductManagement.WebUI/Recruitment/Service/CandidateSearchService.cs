using Domain.DTO;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class CandidateSearchService
    {
        private readonly ICandidateSearchRepository _repository;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<CandidateSearchService> _logger;

        public CandidateSearchService(
            ICandidateSearchRepository repository,
            IEncryptionService encryptionService,
            ILogger<CandidateSearchService> logger)
        {
            _repository = repository;
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _logger = logger;
        }

        public async Task<CandidateSearchResponse> SearchCandidatesAsync(CandidateSearchFilter filter)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;
            if (filter.PageSize > 100) filter.PageSize = 100;

            if (!string.IsNullOrWhiteSpace(filter.Name))
                filter.Name = filter.Name.Trim();

            if (!string.IsNullOrWhiteSpace(filter.Email))
                filter.Email = filter.Email.Trim();

            if (!string.IsNullOrWhiteSpace(filter.University))
                filter.University = filter.University.Trim();

            if (!string.IsNullOrWhiteSpace(filter.Major))
                filter.Major = filter.Major.Trim();

            if (!string.IsNullOrWhiteSpace(filter.Status))
                filter.Status = filter.Status.Trim();

            _logger.LogInformation(
                "Search Filter - Status: '{Status}', Name: '{Name}', Page: {Page}",
                filter.Status ?? "NULL",
                filter.Name ?? "NULL",
                filter.Page);

            try
            {
                var response = await _repository.SearchCandidatesAsync(filter);

                _logger.LogInformation(
                    "Repository returned {Count} candidates (TotalCount: {Total})",
                    response.Candidates?.Count ?? 0,
                    response.TotalCount);

                if (response.Candidates != null && response.Candidates.Any())
                {
                    _logger.LogInformation(
                        "Decrypting {Count} IDCards for admin search",
                        response.Candidates.Count);

                    foreach (var candidate in response.Candidates)
                    {
                        var originalValue = candidate.IDCardNo;
                        candidate.IDCardNo = DecryptIDCard(candidate.IDCardNo, candidate.UserId);

                        _logger.LogDebug(
                            "UserId={UserId}: Encrypted='{Original}' → Decrypted='{Decrypted}'",
                            candidate.UserId,
                            string.IsNullOrEmpty(originalValue)
                                ? "NULL"
                                : (originalValue.Length > 20 ? originalValue.Substring(0, 20) + "..." : originalValue),
                            candidate.IDCardNo);
                    }

                    _logger.LogInformation(
                        "[AUDIT] Decrypted {Count} IDCards at {Time}",
                        response.Candidates.Count, DateTime.UtcNow);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchCandidatesAsync");
                throw;
            }
        }

        public async Task<bool> DeactivateCandidateAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Deactivating candidate with UserId: {UserId}", userId);

                var userUpdated = await _repository.UpdateCandidateStatusAsync(userId, -1);
                var heardAboutUpdated = await _repository.UpdateHeardAboutStatusAsync(userId, -1);

                _logger.LogInformation(
                    "Candidate deactivation result - UserId: {UserId}, UserUpdated: {UserUpdated}, HeardAboutUpdated: {HeardAboutUpdated}",
                    userId, userUpdated, heardAboutUpdated);

                return userUpdated && heardAboutUpdated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating candidate with UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ActivateCandidateAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Activating candidate with UserId: {UserId}", userId);

                var userUpdated = await _repository.UpdateCandidateStatusAsync(userId, 1);
                var heardAboutUpdated = await _repository.UpdateHeardAboutStatusAsync(userId, 1);

                _logger.LogInformation(
                    "Candidate activation result - UserId: {UserId}, UserUpdated: {UserUpdated}, HeardAboutUpdated: {HeardAboutUpdated}",
                    userId, userUpdated, heardAboutUpdated);

                return userUpdated && heardAboutUpdated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating candidate with UserId: {UserId}", userId);
                throw;
            }
        }

        private string DecryptIDCard(string encryptedIDCard, int userId)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedIDCard))
                {
                    _logger.LogWarning("IDCardNoEncrypted is empty for UserId {UserId}", userId);
                    return "N/A";
                }

                if (encryptedIDCard.StartsWith("$2"))
                {
                    _logger.LogWarning(
                        "User {UserId} has BCrypt hash in IDCardNoEncrypted (NOT MIGRATED)",
                        userId);
                    return "***NOT MIGRATED***";
                }

                _logger.LogDebug("Attempting to decrypt IDCard for UserId {UserId}", userId);

                var decrypted = _encryptionService.Decrypt(encryptedIDCard);

                if (string.IsNullOrEmpty(decrypted))
                {
                    _logger.LogWarning("Decrypt returned empty for UserId {UserId}", userId);
                    return "***DECRYPT FAILED***";
                }

                _logger.LogDebug("Successfully decrypted IDCard for UserId {UserId}", userId);
                return decrypted;
            }
            catch (FormatException fex)
            {
                _logger.LogError(fex,
                    "Base64 format error for UserId {UserId}. Possibly plaintext?",
                    userId);

                if (!encryptedIDCard.Contains("/") && !encryptedIDCard.Contains("+") &&
                    encryptedIDCard.Length < 30)
                {
                    return encryptedIDCard;
                }

                return "***FORMAT ERROR***";
            }
            catch (System.Security.Cryptography.CryptographicException cryptoEx)
            {
                _logger.LogError(cryptoEx,
                    "Crypto error decrypting IDCard for UserId {UserId}",
                    userId);
                return "***CRYPTO ERROR***";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error decrypting IDCard for UserId {UserId}",
                    userId);
                return "***ERROR***";
            }
        }
    }
}