using DataAccess;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Service;
using System.Data.Entity.Infrastructure;
using OfficeOpenXml;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<Domain.Interfaces.IDbConnectionFactory, DataAccess.SqlConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUniversityRepository, UniversityRepository>();
builder.Services.AddScoped<IEducationRepository, EducationRepository>();
builder.Services.AddScoped<ILanguageAbilitiesRepository, LanguageAbilitiesRepository>();
builder.Services.AddScoped<ILanguageProficiencyRepository, LanguageProficiencyRepository>();
builder.Services.AddScoped<IProfessionalQualificationRepository, ProfesstionalQualificationRepository>();
builder.Services.AddScoped<IWorkExperienceRepository, WorkExperienceRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IUserQuestionAnswerRepository, UserQuestionAnswerRepository>();
builder.Services.AddScoped<IUserPhotoRepository, UserPhotoRepository>();
builder.Services.AddScoped<ITranscriptRepository, TranscriptRepository>();
builder.Services.AddScoped<IAttachmentsRepository, AttachmentsRepository>();
builder.Services.AddScoped<IHeardAboutRepository, HeardAboutRepository>();
builder.Services.AddScoped<IRecruitmentProgramRepository, RecruitmentProgramRepository>();
builder.Services.AddScoped<ICandidateSearchRepository, CandidateSearchRepository>();
builder.Services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();

builder.Services.AddScoped<SystemConfigurationService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<CandidateSearchService>();
builder.Services.AddScoped<SessionTimerService>();
builder.Services.AddScoped<RecruitmentProgramService>();
builder.Services.AddScoped<UniversityService>();
builder.Services.AddScoped<HeardAboutService>();
builder.Services.AddScoped<TranscriptService>();
builder.Services.AddScoped<AttachmentsService>();
builder.Services.AddScoped<UserPhotoService>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<UserQuestionAnswerService>();
builder.Services.AddScoped<WorkExperienceService>();
builder.Services.AddScoped<LanguageAbilitiesService>();
builder.Services.AddScoped<LanguageProficiencyService>();
builder.Services.AddScoped<ProfessionalQualificationService>();
builder.Services.AddScoped<EducationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "user_auth";
        options.LoginPath = "/api/User/Login";
        options.LogoutPath = "/api/User/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
    })
    .AddCookie("AdminAuth", options =>
    {
        options.Cookie.Name = "admin_auth";
        options.LoginPath = "/api/Admin/Login";
        options.LogoutPath = "/api/Admin/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
    });

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor",
        builder => builder
            .WithOrigins("https://localhost:7159")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() 
            .SetIsOriginAllowed(origin => true));
});


builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOrAdmin", policy =>
    {
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.AuthenticationSchemes.Add("AdminAuth");
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("AdminOnly", policy =>
    {
        policy.AuthenticationSchemes.Add("AdminAuth");
        policy.RequireRole("Admin");
    });
});

builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UsePathBase("/api");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowBlazor");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapControllers();

app.Run();
