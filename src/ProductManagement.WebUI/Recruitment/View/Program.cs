using DataAccess;
using Domain.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Service;
using System.Net;
using View.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(10);
    });

var sharedCookieContainer = new CookieContainer();
builder.Services.AddSingleton(sharedCookieContainer);

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "/api/";

builder.Services.AddHttpClient("LocalAPI", client =>
    client.BaseAddress = new Uri("https://localhost:7190/"))
  .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
  {
      UseCookies = true,
      CookieContainer = sp.GetRequiredService<CookieContainer>(),
      UseDefaultCredentials = false
  })
  .AddHttpMessageHandler<SessionResetHandler>()
  .AddHttpMessageHandler<CookieForwardingHandler>();


builder.Services.AddScoped<Domain.Interfaces.IDbConnectionFactory, DataAccess.SqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUniversityRepository, UniversityRepository>();
builder.Services.AddScoped<IEducationRepository, EducationRepository>();
builder.Services.AddScoped<IWorkExperienceRepository, WorkExperienceRepository>();
builder.Services.AddScoped<IRecruitmentProgramRepository, RecruitmentProgramRepository>();
builder.Services.AddScoped<IHeardAboutRepository, HeardAboutRepository>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();


builder.Services.AddScoped<HeardAboutService>();
builder.Services.AddScoped<RecruitmentProgramService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<UniversityService>();
builder.Services.AddScoped<EducationService>();
builder.Services.AddScoped<WorkExperienceService>();
builder.Services.AddScoped<SessionTimerService>();
builder.Services.AddScoped<APIClient>();
builder.Services.AddScoped<SessionResetHandler>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddServerSideBlazor()
   .AddCircuitOptions(options =>
   {
       options.DisconnectedCircuitMaxRetained = 10;

       options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);

       options.MaxBufferedUnacknowledgedRenderBatches = 10;

       if (builder.Environment.IsDevelopment())
       {
           options.DetailedErrors = true;
       }
   });
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(12);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 128 * 1024;
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CookieForwardingHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.Use(async (context, next) =>
{
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'nonce-RbwVv3QnnEuEoBuXLjA7Xg=='; " +
        "img-src 'self' data:; " +
        "object-src 'none'; " +
        "upgrade-insecure-requests; " +
        "block-all-mixed-content;";

    await next();
});
app.UseHttpsRedirection();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseStaticFiles(); 
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
