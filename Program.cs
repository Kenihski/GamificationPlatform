using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using GamificationPlatform.Authentication;
using GamificationPlatform.DAL;
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
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] [{RequestId}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(logFileName, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{RequestId}] {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddControllersWithViews();

// Cookie authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        // Reject cookies for users removed after the cookie was issued.
        options.EventsType = typeof(ValidateUserCookieEvents);
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
builder.Services.AddScoped<ValidateUserCookieEvents>();

var app = builder.Build();

// Keep the same request reference on DAL, controller, and middleware logs.
app.Use(async (context, next) =>
{
    using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
    {
        await next();
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    DBInit.Seed(app);
}

else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms for user {UserId}";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
        diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");
    };
});

app.UseStatusCodePagesWithReExecute("/Home/ErrorStatus", "?code={0}");

app.MapStaticAssets(); // Enable static assets from wwwroot (images, JS, CSS)

app.UseRouting();

// Authentication must come before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
