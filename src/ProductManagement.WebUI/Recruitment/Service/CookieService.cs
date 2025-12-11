using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Service 
{ 
    public class CookieService 
    {
        public readonly IJSRuntime iJSRuntime; 
        public CookieService(IJSRuntime iJSRuntime)
        {
            this.iJSRuntime = iJSRuntime; 
        }
        public async Task<string> GetCookieAsync(string cookieName) 
        { 
            return await iJSRuntime.InvokeAsync<string>("getCookie", cookieName); 
        } 
        public async Task<string> GetUserEmailFromCookieAsync() 
        { 
            return await GetCookieAsync("userEmail");
        } 
        public async Task ClearCookieAsync(string cookieName) 
        {
            await iJSRuntime.InvokeVoidAsync("deleteCookie", cookieName);
        } 
        public async Task ClearUserEmailCookieAsync()
        {
            await ClearCookieAsync("userEmail"); 
        }
        public async Task SaveUserEmailToCookie(string email)
        { 
            await iJSRuntime.InvokeVoidAsync("setCookie", "userEmail", email, 2);
        } 
    } 
}