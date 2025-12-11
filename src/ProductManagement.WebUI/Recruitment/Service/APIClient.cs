using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class APIClient
    {
        private readonly HttpClient _http;
        private readonly SessionTimerService _sessionTimer;

        public APIClient(IHttpClientFactory factory, SessionTimerService timer)
        {
            _http = factory.CreateClient("LocalAPI");
            _sessionTimer = timer;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var r = await _http.GetAsync(url);
            _sessionTimer.ResetSession();
            return r;
        }

        public async Task<T?> GetFromJsonAsync<T>(string url)
        {
            var result = await _http.GetFromJsonAsync<T>(url);
            _sessionTimer.ResetSession();
            return result;
        }
        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T data)
        {
            var r = await _http.PostAsJsonAsync(url, data);
            _sessionTimer.ResetSession();
            return r;
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T data)
        {
            var r = await _http.PutAsJsonAsync(url, data);
            _sessionTimer.ResetSession();
            return r;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            var r = await _http.DeleteAsync(url);
            _sessionTimer.ResetSession();
            return r;
        }

        public async Task<HttpResponseMessage> PatchAsJsonAsync<T>(string url, T data)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = JsonContent.Create(data)
            };
            var r = await _http.SendAsync(request);
            _sessionTimer.ResetSession();
            return r;
        }

    }
}
