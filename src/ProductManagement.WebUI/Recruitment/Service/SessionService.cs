using Domain.DTO;
using Microsoft.JSInterop;

namespace Service
{
    public class SessionService
    {
        private readonly IJSRuntime _js;

        public SessionService(IJSRuntime js) => _js = js;

        public Task SetUserSessionAsync(int userId, string email)
            => Task.WhenAll(
                _js.InvokeVoidAsync("sessionStorage.setItem", "userId", userId.ToString()).AsTask(),
                _js.InvokeVoidAsync("sessionStorage.setItem", "userEmail", email).AsTask()
            );

        public Task LogoutUserAsync()
            => Task.WhenAll(
                _js.InvokeVoidAsync("sessionStorage.removeItem", "userId").AsTask(),
                _js.InvokeVoidAsync("sessionStorage.removeItem", "userEmail").AsTask()
            );

        public Task SetAdminSessionAsync(int adminId, string username)
            => Task.WhenAll(
                _js.InvokeVoidAsync("sessionStorage.setItem", "adminId", adminId.ToString()).AsTask(),
                _js.InvokeVoidAsync("sessionStorage.setItem", "adminUsername", username).AsTask()
            );

        public Task LogoutAdminAsync()
            => Task.WhenAll(
                _js.InvokeVoidAsync("sessionStorage.removeItem", "adminId").AsTask(),
                _js.InvokeVoidAsync("sessionStorage.removeItem", "adminUsername").AsTask()
            );

        public async Task<SessionInfoDTO> GetSessionInfoAsync()
        {
            var userEmail = await _js.InvokeAsync<string>("sessionStorage.getItem", "userEmail");
            var adminUsername = await _js.InvokeAsync<string>("sessionStorage.getItem", "adminUsername");
            var userIdStr = await _js.InvokeAsync<string>("sessionStorage.getItem", "userId");
            var adminIdStr = await _js.InvokeAsync<string>("sessionStorage.getItem", "adminId");

            int.TryParse(userIdStr, out var userId);
            int.TryParse(adminIdStr, out var adminId);

            var isUser = !string.IsNullOrEmpty(userEmail);
            var isAdmin = !string.IsNullOrEmpty(adminUsername);

            return new SessionInfoDTO
            {
                UserId = userId == 0 ? null : userId,
                AdminId = adminId == 0 ? null : adminId,
                UserEmail = userEmail,
                AdminUsername = adminUsername,
                IsUser = isUser,
                IsAdmin = isAdmin,
                IsAuthenticated = isUser || isAdmin
            };
        }

        public async Task LogoutAllAsync()
        {
            await LogoutUserAsync();
            await LogoutAdminAsync();
        }
    }
}
