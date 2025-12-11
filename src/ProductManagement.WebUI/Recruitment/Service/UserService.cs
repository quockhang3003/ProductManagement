using BCrypt.Net;
using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Service
{
    public class UserService
    {
        private readonly IUserRepository _repo;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository repo,
            IEncryptionService encryptionService,
            ILogger<UserService> logger)
        {
            _repo = repo;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<User?> GetUserByEmail(string email) => await _repo.GetByEmailAsync(email);

        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var user = await _repo.GetByEmailAsync(email);
            return user?.Id;
        }

        public async Task<IEnumerable<User>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task RegisterUser(RegisterDTO dto)
        {
            if (await ExistsEmailAsync(dto.Email))
                throw new Exception("Email already exists.");

            var encryptedIDCard = _encryptionService.Encrypt(dto.IdCardNumber);
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.IdCardNumber, workFactor: 12);

            var user = new User
            {
                PreferableOfficeLocation = dto.PreferableOfficeLocation,
                FirstPreference = dto.FirstPreference,
                SecondPreference = dto.SecondPreference,
                LastName = dto.LastName,
                FirstName = dto.FirstName,
                VietnameseName = dto.VietnameseName,
                Gender = dto.Gender,
                Nationality = dto.Nationality,
                DateOfBirth = dto.DateOfBirth,
                PlaceOfBirth = dto.PlaceOfBirth,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                IDCardNoEncrypted = encryptedIDCard,
                DateOfIssue = dto.DateOfIssue,
                PlaceOfIssue = dto.PlaceOfIssue,
                Mobile = dto.Mobile,
                Street = dto.Street,
                Ward = dto.Ward,
                City = dto.City,
                CurrentAddress = dto.CurrentAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(user);

            _logger.LogInformation(
                "User registered successfully: Email={Email}, IDCard encrypted",
                dto.Email);
        }

        public async Task UpdateUser(User user)
        {
            // TODO: Implement update logic nếu cần
        }

        public async Task<bool> ExistsEmailAsync(string email) => await _repo.ExistsEmailAsync(email);

        public async Task<bool> ExistsIDCardAsync(string IDCard) => await _repo.ExistsIDCardAsync(IDCard);

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _repo.GetByEmailAsync(email);
            if (user == null) return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!isValid)
            {
                _logger.LogWarning("Login failed for email: {Email}", email);
                return null;
            }

            _logger.LogInformation("User logged in: {Email}, UserId={UserId}", email, user.Id);
            return isValid ? user : null;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id);
        }

        public async Task<User?> GetUserWithDecryptedIDCardAsync(int userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return null;

            user.IDCardNoPlainText = _encryptionService.Decrypt(user.IDCardNoEncrypted);

            _logger.LogWarning(
                "SENSITIVE DATA ACCESS: IDCard decrypted for UserId={UserId} at {Time}",
                userId, DateTime.UtcNow);

            return user;
        }

        public async Task<string> GetMaskedIDCardAsync(int userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return string.Empty;

            var plainIDCard = _encryptionService.Decrypt(user.IDCardNoEncrypted);
            return MaskIDCard(plainIDCard);
        }

        private string MaskIDCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard) || idCard.Length <= 6)
                return new string('*', idCard?.Length ?? 0);

            var firstPart = idCard.Substring(0, 4);
            var lastPart = idCard.Substring(idCard.Length - 2);
            var middleStars = new string('*', idCard.Length - 6);

            return $"{firstPart}{middleStars}{lastPart}";
        }

    }
}
