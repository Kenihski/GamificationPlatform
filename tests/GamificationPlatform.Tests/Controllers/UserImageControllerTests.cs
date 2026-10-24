using System.Security.Claims;
using GamificationPlatform.Controllers;
using GamificationPlatform.DAL;
using GamificationPlatform.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    private static UserImageController CreateController(
        IUserImageRepository repository,
        ChallengeDbContext context)
    {
        var controller = new UserImageController(
            repository, context,
            Substitute.For<IWebHostEnvironment>(),
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
