using GamificationPlatform.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace GamificationPlatform.Tests.Controllers;

public class AccountErrorControllerTests
{
    [Fact]
    public void AccessDenied_ReturnsForbiddenStatusAndFriendlyView()
    {
        // Identity services are not used by the access-denied action.
        var controller = new AccountController(
            null!,
            null!,
            Substitute.For<ILogger<AccountController>>());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = controller.AccessDenied();

        Assert.IsType<ViewResult>(result);
        Assert.Equal(
            StatusCodes.Status403Forbidden,
            controller.Response.StatusCode);
    }
}
