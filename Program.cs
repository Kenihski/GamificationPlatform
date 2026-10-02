using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.DAL;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog to log application events to both the console
// and a separate log file for each application run.
var logFileName =
    $"Logs/log-{DateTime.Now:yyyyMMdd_HHmmss}.txt";

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File(logFileName);
});

builder.Services.AddControllersWithViews();

// Cookie authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
    });

builder.Services.AddDbContext<ChallengeDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:ChallengeDbContextConnection"]);
});

// Register repositories for dependency injection.
builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
builder.Services.AddScoped<IAttemptRepository, AttemptRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    DBInit.Seed(app);
}

app.MapStaticAssets(); // Enable static assets from wwwroot (images, JS, CSS)

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();