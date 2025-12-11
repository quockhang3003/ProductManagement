using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Add debug logging
            Console.WriteLine("CustomAuthStateProvider: Getting authentication state...");

            var userEmail = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "userEmail");
            var userId = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "userId");
            var adminUsername = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "adminUsername");
            var adminId = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "adminId");

            // Debug logging
            Console.WriteLine($"Retrieved from sessionStorage - UserEmail: '{userEmail}', UserId: '{userId}', AdminUsername: '{adminUsername}', AdminId: '{adminId}'");

            var claims = new List<Claim>();

            // Check for user authentication
            if (!string.IsNullOrEmpty(userEmail))
            {
                claims.Add(new Claim(ClaimTypes.Name, userEmail));
                claims.Add(new Claim(ClaimTypes.Role, "User"));

                if (!string.IsNullOrEmpty(userId))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                }

                Console.WriteLine($"User claims added: {claims.Count} claims");
            }
            // Check for admin authentication
            else if (!string.IsNullOrEmpty(adminUsername))
            {
                claims.Add(new Claim(ClaimTypes.Name, adminUsername));
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

                if (!string.IsNullOrEmpty(adminId))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, adminId));
                }

                Console.WriteLine($"Admin claims added: {claims.Count} claims");
            }

            if (claims.Any())
            {
                var identity = new ClaimsIdentity(claims, "sessionauth"); // Changed authentication type
                var principal = new ClaimsPrincipal(identity);

                Console.WriteLine($"Authentication successful - Identity.IsAuthenticated: {identity.IsAuthenticated}");
                Console.WriteLine($"Claims: {string.Join(", ", claims.Select(c => $"{c.Type}:{c.Value}"))}");

                return new AuthenticationState(principal);
            }
            else
            {
                Console.WriteLine("No valid authentication found, returning anonymous");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAuthenticationStateAsync: {ex.Message}");
        }

        return new AuthenticationState(_anonymous);
    }

    public async Task NotifyLoginAsync(string identifier, string role, string? userId = null)
    {
        if (role == "User")
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userEmail", identifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "userId", userId);
            }
        }
        else if (role == "Admin")
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "adminUsername", identifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "adminId", userId);
            }
        }

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task NotifyLogoutAsync(string role)
    {
        Console.WriteLine($"NotifyLogoutAsync called - Role: {role}");

        if (role == "User")
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "userEmail");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "userId");
        }
        else if (role == "Admin")
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "adminUsername");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "adminId");
        }

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task RefreshStateAsync()
    {
        Console.WriteLine("[AuthStateProvider] RefreshStateAsync called");
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }
}