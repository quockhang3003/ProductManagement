using Azure.Core;
using Dapper;
using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Service;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserService service,
            IDbConnectionFactory dbFactory,
            IEncryptionService encryptionService,
            ILogger<UserController> logger)
        {
            _service = service;
            _dbFactory = dbFactory;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _service.GetAllAsync();
            return user == null ? NotFound() : Ok(user);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _service.GetUserByEmail(email);
            return user == null ? NotFound() : Ok(user);
        }
     
        [HttpGet("by-id/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetUserByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var user = await _service.LoginAsync(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            using var conn = _dbFactory.CreateConnection();

            if (string.IsNullOrEmpty(user.IDCardNoEncrypted))
            {
                try
                {
                    var encryptedIDCard = _encryptionService.Encrypt(dto.Password);

                    await conn.ExecuteAsync(
                        "UPDATE Users SET IDCardNoEncrypted = @Encrypted WHERE Id = @Id",
                        new { Encrypted = encryptedIDCard, user.Id });

                    _logger.LogInformation(
                        "Migration: IDCard encrypted for User {UserId} during login",
                        user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Migration failed for User {UserId}", user.Id);
                }
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, "User")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok(new
            {
                Message = "Login successful",
                User = user.Email,
                UserId = user.Id
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            Console.WriteLine("API received:");
            Console.WriteLine(JsonSerializer.Serialize(dto));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            using var conn = _dbFactory.CreateConnection();

            if (dto.ID > 0)
            {
                var existingUser = await conn.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Id = @Id", new { dto.ID });

                if (existingUser == null)
                    return NotFound("User not found");

                string encryptedIDCard;
                string hashedPassword;

                if (string.IsNullOrEmpty(existingUser.IDCardNoEncrypted))
                {
                    encryptedIDCard = _encryptionService.Encrypt(dto.IdCardNumber);
                    hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.IdCardNumber, 12);

                    _logger.LogInformation(
                        "Migration: IDCard encrypted for User {UserId} during update",
                        dto.ID);
                }
                else
                {
                    var currentIDCard = _encryptionService.Decrypt(existingUser.IDCardNoEncrypted);

                    if (currentIDCard != dto.IdCardNumber)
                    {
                        encryptedIDCard = _encryptionService.Encrypt(dto.IdCardNumber);
                        hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.IdCardNumber, 12);
                    }
                    else
                    {
                        encryptedIDCard = existingUser.IDCardNoEncrypted;
                        hashedPassword = existingUser.PasswordHash;
                    }
                }

                string updateSql = @"UPDATE Users SET
                                 LastName = @LastName,
                                 FirstName = @FirstName,
                                 VietnameseName = @VietnameseName,
                                 Gender = @Gender,
                                 Nationality = @Nationality,
                                 DateOfBirth = @DateOfBirth,
                                 PlaceOfBirth = @PlaceOfBirth,
                                 Email = @Email,
                                 IDCardNoEncrypted = @IDCardNoEncrypted,
                                 PasswordHash = @PasswordHash,
                                 DateOfIssue = @DateOfIssue,
                                 PlaceOfIssue = @PlaceOfIssue,
                                 Mobile = @Mobile,
                                 Street = @Street,
                                 Ward = @Ward,
                                 City = @City,
                                 CurrentAddress = @CurrentAddress,
                                 PreferableOfficeLocation = @PreferableOfficeLocation,
                                 FirstPreference = @FirstPreference,
                                 SecondPreference = @SecondPreference,
                                 UpdatedAt = GETDATE()
                                 WHERE Id = @ID";

                await conn.ExecuteAsync(updateSql, new
                {
                    dto.ID,
                    dto.LastName,
                    dto.FirstName,
                    dto.VietnameseName,
                    dto.Gender,
                    dto.Nationality,
                    dto.DateOfBirth,
                    dto.PlaceOfBirth,
                    dto.Email,
                    IDCardNoEncrypted = encryptedIDCard,
                    PasswordHash = hashedPassword,
                    dto.DateOfIssue,
                    dto.PlaceOfIssue,
                    dto.Mobile,
                    dto.Street,
                    dto.Ward,
                    dto.City,
                    dto.CurrentAddress,
                    dto.PreferableOfficeLocation,
                    dto.FirstPreference,
                    dto.SecondPreference
                });

                return Ok(new { message = "Update successful." });
            }

            var emailExistsNew = await conn.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) FROM Users WHERE Email = @Email", new { dto.Email });

            if (emailExistsNew)
                return BadRequest("Email already exists.");

            var idCardExistsNew = await _service.ExistsIDCardAsync(dto.IdCardNumber);
            if (idCardExistsNew)
                return BadRequest("IDCard already exists.");

            string encryptedIDCardNew = _encryptionService.Encrypt(dto.IdCardNumber);
            string hashedPasswordNew = BCrypt.Net.BCrypt.HashPassword(dto.IdCardNumber, workFactor: 12);

            string insertSql = @"INSERT INTO Users (
                LastName, FirstName, VietnameseName, Gender, Nationality, DateOfBirth,
                PlaceOfBirth, Email, 
                IDCardNoEncrypted, PasswordHash, 
                DateOfIssue, PlaceOfIssue, Mobile,
                Street, Ward, City, CurrentAddress, 
                PreferableOfficeLocation, FirstPreference, SecondPreference, 
                CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (
                @LastName, @FirstName, @VietnameseName, @Gender, @Nationality, @DateOfBirth,
                @PlaceOfBirth, @Email, 
                @IDCardNoEncrypted, @PasswordHash,
                @DateOfIssue, @PlaceOfIssue, @Mobile,
                @Street, @Ward, @City, @CurrentAddress,
                @PreferableOfficeLocation, @FirstPreference, @SecondPreference,
                GETDATE())";


            var newUserId = await conn.ExecuteScalarAsync<int>(insertSql, new
            {
                dto.LastName,
                dto.FirstName,
                dto.VietnameseName,
                dto.Gender,
                dto.Nationality,
                dto.DateOfBirth,
                dto.PlaceOfBirth,
                dto.Email,
                IDCardNoEncrypted = encryptedIDCardNew,
                PasswordHash = hashedPasswordNew,
                dto.DateOfIssue,
                dto.PlaceOfIssue,
                dto.Mobile,
                dto.Street,
                dto.Ward,
                dto.City,
                dto.CurrentAddress,
                dto.PreferableOfficeLocation,
                dto.FirstPreference,
                dto.SecondPreference
            });

            _logger.LogInformation("User registered: Email={Email}, UserId={UserId}", dto.Email, newUserId);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dto.Email),
                new Claim(ClaimTypes.NameIdentifier, newUserId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24),
                    AllowRefresh = true
                });
            var cookieOptions = HttpContext.Features.Get<IResponseCookiesFeature>();
            _logger.LogInformation("Cookies set in response: {Count}", Response.Cookies);
            _logger.LogInformation("Auto-login successful for new user: UserId={UserId}", newUserId);

            return Ok(new RegisterResponseDTO
            {
                Message = "Register successful.",
                UserId = newUserId,
                Email = dto.Email,
                AutoLogin = true
            });
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Response.Cookies.Delete("user_auth", new CookieOptions
            {
                Path = "/",
                Domain = HttpContext.Request.Host.Host,
                SameSite = SameSiteMode.None,
                Secure = true
            });

            Response.Cookies.Delete(".AspNetCore.Antiforgery.c8VslOfqnPk", new CookieOptions
            {
                Path = "/",
                SameSite = SameSiteMode.None,
                Secure = true
            });

            return NoContent();
        }

        [HttpGet("check-session")]
        [Authorize]
        public IActionResult CheckSession()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                return Unauthorized(new { IsAuthenticated = false });
            }

            return Ok(new
            {
                IsAuthenticated = true,
                UserEmail = User.Identity?.Name,
                SessionValid = true
            });
        }

        [HttpGet("idcard-masked/{userId:int}")]
        [Authorize]
        public async Task<IActionResult> GetMaskedIDCard(int userId)
        {
            try
            {
                var maskedIDCard = await _service.GetMaskedIDCardAsync(userId);
                return Ok(new { IDCard = maskedIDCard, IsMasked = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting masked IDCard for user {UserId}", userId);
                return StatusCode(500, "Error retrieving ID card");
            }
        }

        [HttpGet("idcard-full/{userId:int}")]
        [Authorize]
        public async Task<IActionResult> GetFullIDCard(int userId)
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != userId.ToString())
                {
                    _logger.LogWarning(
                        "SECURITY: User {CurrentUserId} attempted to access IDCard of User {TargetUserId}",
                        currentUserId, userId);
                    return Forbid();
                }

                var user = await _service.GetUserWithDecryptedIDCardAsync(userId);
                if (user == null)
                    return NotFound();

                return Ok(new
                {
                    IDCard = user.IDCardNoPlainText,
                    IsMasked = false,
                    Warning = "Sensitive data - handle with care"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting full IDCard for user {UserId}", userId);
                return StatusCode(500, "Error retrieving ID card");
            }
        }
    }
}
