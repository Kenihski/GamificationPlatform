using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ChallengeDbContext>(options =>{
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

app.MapDefaultControllerRoute();

app.Run();

