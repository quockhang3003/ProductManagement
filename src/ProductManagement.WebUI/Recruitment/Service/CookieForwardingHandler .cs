using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class CookieForwardingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CookieForwardingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx != null && ctx.Request.Headers.TryGetValue("Cookie", out var cookieHeader))
                {
                    // Forward raw Cookie header (contains auth cookie(s))
                    if (!request.Headers.Contains("Cookie"))
                        request.Headers.Add("Cookie", (string)cookieHeader);
                }
            }
            catch
            {
                // swallow - don't fail the whole request if forwarding cookie fails
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

}
