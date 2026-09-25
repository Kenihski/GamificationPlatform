using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;

var builder = WebApplication.CreateBuilder(args);

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