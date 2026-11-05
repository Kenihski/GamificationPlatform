using System.Security.Claims;
using GamificationPlatform.Controllers;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.Controllers;

public class UserImageControllerTests
{
    [Fact]
    public async Task Index_WhenSignedIn_ReturnsOwnImages()
    {
        var repository = Substitute.For<IUserImageRepository>();
        var images = new List<UserImage>
        {
            new UserImage
            {
                UserImageId = 12,
                UserId = 7,
                FileName = "example.png",
                ImageUrl = "/uploads/images/example.png"
            }
        };
        repository.GetImagesByUserIdAsync(7)
            .Returns(Task.FromResult(images));

        using var context = new ChallengeDbContext(
            new DbContextOptionsBuilder<ChallengeDbContext>().Options);
        var controller = CreateController(repository, context);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(images, view.Model);
        await repository.Received(1).GetImagesByUserIdAsync(7);
    }

    [Fact]
    public async Task Delete_WhenImageNotFound_Returns404WithoutDeleting()
    {
        var repository = Substitute.For<IUserImageRepository>();
        repository.GetImageByIdAsync(404)
            .Returns(Task.FromResult<UserImage?>(null));

        using var context = new ChallengeDbContext(
            new DbContextOptionsBuilder<ChallengeDbContext>().Options);
        var controller = CreateController(repository, context);

        var result = await controller.Delete(404, "/UserImage/Index");

        Assert.IsType<NotFoundResult>(result);
        await repository.DidNotReceive()
            .DeleteImageAsync(Arg.Any<UserImage>());
    }

    // Check both deletion routes, missing fallback files and protection of default image.
    [Theory]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public async Task Delete_ReplacesReferencesOrKeepsImageWhenFallbackIsUnavailable(
        bool defaultImage, bool fallbackAvailable, bool deleteFallback)
    {
        // Arrange: use an isolated in-memory database and temporary image files.
        using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=False");
        connection.Open();
        using var context = new ChallengeDbContext(
            new DbContextOptionsBuilder<ChallengeDbContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();

        var webRoot = Path.Combine(Path.GetTempPath(), "image-delete-test-" + Guid.NewGuid());
        Directory.CreateDirectory(Path.Combine(webRoot, "images"));
        Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "images"));
        try
        {
            const string fallbackUrl = "/images/Question_1.png";
            var fallbackPath = Path.Combine(webRoot, "images", "Question_1.png");
            if (fallbackAvailable) await File.WriteAllTextAsync(fallbackPath, "test image");
            var fallback = new DefaultImage { Name = "Question 1", ImageUrl = fallbackUrl };
            context.DefaultImages.Add(fallback);

            var deletedUrl = deleteFallback ? fallbackUrl :
                defaultImage ? "/images/example.png" : "/uploads/images/example.png";
            var deletedPath = Path.Combine(webRoot, deletedUrl.TrimStart('/'));
            if (!deleteFallback) await File.WriteAllTextAsync(deletedPath, "test image");
            var targetDefault = deleteFallback ? fallback :
                new DefaultImage { Name = "Example", ImageUrl = deletedUrl };
            if (defaultImage && !deleteFallback) context.DefaultImages.Add(targetDefault);

            var challenge = new Challenge { Title = "Affected", ImageUrl = deletedUrl };
            var question = new Question { Title = "Affected", ImageUrl = deletedUrl, Challenge = challenge };
            var unaffected = new Challenge { Title = "Unchanged", ImageUrl = "/images/other.png" };
            context.AddRange(challenge, question, unaffected);
            await context.SaveChangesAsync();

            var repository = Substitute.For<IUserImageRepository>();
            var userImage = new UserImage { UserImageId = 12, UserId = 7, ImageUrl = deletedUrl };
            repository.GetImageByIdAsync(12).Returns(Task.FromResult<UserImage?>(userImage));
            var environment = Substitute.For<IWebHostEnvironment>();
            environment.WebRootPath.Returns(webRoot);
            var controller = CreateController(repository, context, environment);
            controller.TempData = new TempDataDictionary(
                controller.HttpContext, Substitute.For<ITempDataProvider>());

            // Act: delete the selected image without touching the project's database.
            if (defaultImage) await controller.DeleteDefaultImage(targetDefault.DefaultImageId);
            else await controller.Delete(12, null);

            // Assert: persisted references change together, or deletion is safely refused.
            var shouldDelete = fallbackAvailable && !deleteFallback;
            await context.Entry(challenge).ReloadAsync();
            await context.Entry(question).ReloadAsync();
            Assert.Equal(shouldDelete ? fallbackUrl : deletedUrl, challenge.ImageUrl);
            Assert.Equal(shouldDelete ? fallbackUrl : deletedUrl, question.ImageUrl);
            Assert.Equal("/images/other.png", (await context.Challenges.FindAsync(unaffected.ChallengeId))!.ImageUrl);
            Assert.Equal(!shouldDelete, File.Exists(deletedPath));
            Assert.Equal(!shouldDelete, controller.TempData.ContainsKey("ErrorMessage"));
            if (defaultImage)
                Assert.Equal(!shouldDelete, await context.DefaultImages.AnyAsync(i => i.DefaultImageId == targetDefault.DefaultImageId));
            else
                await repository.Received(shouldDelete ? 1 : 0).DeleteImageAsync(userImage);
        }
        finally
        {
            // Remove only the temporary directory created for this test.
            Directory.Delete(webRoot, recursive: true);
        }
    }

    private static UserImageController CreateController(
        IUserImageRepository repository,
        ChallengeDbContext context,
        IWebHostEnvironment? environment = null)
    {
        var controller = new UserImageController(
            repository, context,
            environment ?? Substitute.For<IWebHostEnvironment>(),
            Substitute.For<ILogger<UserImageController>>());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "7") },
                    authenticationType: "Test"))
            }
        };
        return controller;
    }
}
