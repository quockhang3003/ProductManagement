using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class UserPhotoService 
    {
        private readonly IUserPhotoRepository _repo;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserRepository _userRepo; 


        public UserPhotoService(IUserPhotoRepository repo, IWebHostEnvironment environment, IUserRepository userRepo)
        {
            _repo = repo;
            _environment = environment;
            _userRepo = userRepo;
        }


        public async Task<UserPhoto?> GetUserPhotoAsync(int userId)
        {
            return await _repo.GetByUserIdAsync(userId);
        }


        public async Task<UserPhoto?> GetUserPhotoByEmailAsync(string userEmail)
        {
            return await _repo.GetByUserEmailAsync(userEmail);
        }


        public async Task<PhotoUploadResponse> UploadPhotoAsync(PhotoUploadRequest request)
        {
            Console.WriteLine($"[DEBUG] WebRootPath: {_environment.WebRootPath}");

            try
            {
                if (request.Photo == null || request.Photo.Length == 0)
                    return new PhotoUploadResponse { Success = false, Message = "No file selected" };


                if (request.Photo.Length > 1024 * 1024) // 1MB
                    return new PhotoUploadResponse { Success = false, Message = "File size must be less than 1MB" };


                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
                if (!allowedTypes.Contains(request.Photo.ContentType.ToLower()))
                    return new PhotoUploadResponse { Success = false, Message = "Only JPG and PNG files are allowed" };


                var userId = await GetUserIdByEmailAsync(request.UserEmail);
                if (userId == 0)
                    return new PhotoUploadResponse { Success = false, Message = "User not found" };


                await _repo.DeactivateOldPhotosByEmailAsync(request.UserEmail);

                var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(request.Photo.FileName)}";
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await request.Photo.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var photo = new UserPhoto
                {
                    UserID = userId,
                    FileName = request.Photo.FileName,
                    FilePath = $"/uploads/photos/{fileName}",
                    ContentType = request.Photo.ContentType,
                    FileSize = request.Photo.Length,
                    UploadedAt = DateTime.Now,
                    IsActive = true,
                    FileData = fileBytes
                };


                var photoId = await _repo.CreateAsync(photo);


                return new PhotoUploadResponse
                {
                    Success = true,
                    Message = "Photo uploaded successfully",
                    PhotoUrl = photo.FilePath,
                    PhotoId = photoId
                };
            }
            catch (Exception ex)
            {
                return new PhotoUploadResponse
                {
                    Success = false,
                    Message = $"Upload failed: {ex.Message}"
                };
            }
        }


        private async Task<int> GetUserIdByEmailAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            return user?.Id ?? 0;
        }


        public async Task<bool> DeletePhotoAsync(int userId)
        {
            var existingPhoto = await _repo.GetByUserIdAsync(userId);
            if (existingPhoto != null)
            {
                var fullPath = Path.Combine(_environment.WebRootPath, existingPhoto.FilePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                return await _repo.DeleteAsync(existingPhoto.Id);
            }
            return false;
        }
    }

}
