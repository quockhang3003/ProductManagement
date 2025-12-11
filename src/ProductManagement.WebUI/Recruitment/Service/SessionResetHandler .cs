using Microsoft.JSInterop;

namespace Service;
public class SessionResetHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime; 
    public SessionResetHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[SessionResetHandler] Successful API call to {request.RequestUri} - no JS reset here");
        }
        else
        {
            Console.WriteLine($"[SessionResetHandler] Failed API call: {response.StatusCode} - {request.RequestUri}");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("[SessionResetHandler] 401 detected - session likely expired");
            }
        }
        return response;
    }
}



