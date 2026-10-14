
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using GamificationPlatform.Services;
using Serilog;
using Serilog.Context;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog to log application events to both the console
// and a separate log file for each application run.
var logFileName =
    $"Logs/log-{DateTime.Now:yyyyMMdd_HHmmss}.txt";

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] [{RequestId}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            logFileName,
            outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{RequestId}] {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ChallengeDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration[
            "ConnectionStrings:ChallengeDbContextConnection"]);
});

// Configure ASP.NET Core Identity.
builder.Services
    .AddIdentity<User, IdentityRole<int>>(options =>
    {
        // Password requirements.
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // User settings.
        options.User.RequireUniqueEmail = true;

        // Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ChallengeDbContext>()
    .AddDefaultTokenProviders();

// Configure Identity authentication cookies.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register repositories for dependency injection.
builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
builder.Services.AddScoped<IAttemptRepository, AttemptRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IUserImageRepository, UserImageRepository>();

// Register services for business logic.
builder.Services.AddScoped<IAchievementService, AchievementService>();

var app = builder.Build();

// Keep the same request reference on DAL, controller, and middleware logs.
app.Use(async (context, next) =>
{
    using (LogContext.PushProperty(
        "RequestId",
        context.TraceIdentifier))
    {
        await next();
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    await DBInit.SeedAsync(app);
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// Register default images from wwwroot/images at application startup.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<ChallengeDbContext>();

    string imagesPath = Path.Combine(
        app.Environment.WebRootPath,
        "images");

    if (Directory.Exists(imagesPath))
    {
        // Get all PNG images in the default images folder.
        var imageFiles = Directory.GetFiles(
            imagesPath,
            "*.png");

        // Get images already registered in the database.
        var existingFileNames = await context.DefaultImages
            .Select(image => image.FileName)
            .ToListAsync();

        var defaultImages = new List<DefaultImage>();

        foreach (string imagePath in imageFiles)
        {
            string fileName = Path.GetFileName(imagePath);

            // Skip images already registered.
            if (existingFileNames.Contains(
                fileName,
                StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            string imageName =
                Path.GetFileNameWithoutExtension(fileName);

            // Display bundled question images with readable names.
            if (imageName.StartsWith(
                "Question_",
                StringComparison.OrdinalIgnoreCase))
            {
                imageName = imageName.Replace("_", " ");
            }

            defaultImages.Add(new DefaultImage
            {
                Name = imageName,
                FileName = fileName,
                ImageUrl = "/images/" + fileName
            });
        }

        if (defaultImages.Count > 0)
        {
            context.DefaultImages.AddRange(defaultImages);
            await context.SaveChangesAsync();
        }
    }
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} " +
        "in {Elapsed:0.0000} ms for user {UserId}";

    options.EnrichDiagnosticContext =
        (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set(
                "RequestId",
                httpContext.TraceIdentifier);

            diagnosticContext.Set(
                "UserId",
                httpContext.User.FindFirstValue(
                    ClaimTypes.NameIdentifier) ?? "anonymous");
        };
});

app.UseStatusCodePagesWithReExecute(
    "/Home/ErrorStatus",
    "?code={0}");

app.MapStaticAssets();
app.UseStaticFiles();

app.UseRouting();

// Authentication must come before authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
