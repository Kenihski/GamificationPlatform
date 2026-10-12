using System.ComponentModel.DataAnnotations;
using GamificationPlatform.ViewModels;
using Xunit;

namespace GamificationPlatform.Tests.Validation;

public class AuthenticationValidationTests
{
    [Theory]
    [InlineData("", "example@example.com", "secret123", "secret123", false)]
    [InlineData("ab", "example@example.com", "secret123", "secret123", false)]
    [InlineData("   ", "example@example.com", "secret123", "secret123", false)]
    [InlineData("newuser", "not-an-email", "secret123", "secret123", false)]
    [InlineData("newuser", "example@example.com", "short", "short", false)]
    [InlineData("newuser", "example@example.com", "secret123", "different", false)]
    [InlineData("newuser", "example@example.com", "newuser", "newuser", false)]
    [InlineData("newuser", "example@example.com", "secret123", "secret123", true)]
    public void Register_WhenFieldsChange_ValidatesExpectedResult(
        string username, string email, string password,
        string confirmation, bool expectedValid)
    {
        // Arrange: evaluate the model, including its IValidatableObject rules.
        var model = new RegisterViewModel
        {
            Username = username,
            Email = email,
            Password = password,
            ConfirmPassword = confirmation
        };
        var errors = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(
            model, new ValidationContext(model), errors,
            validateAllProperties: true);

        // Assert
        Assert.Equal(expectedValid, isValid);
        if (!expectedValid)
        {
            Assert.NotEmpty(errors);
        }
    }

    [Theory]
    [InlineData("", "password", false)]
    [InlineData("newuser", "", false)]
    [InlineData("newuser", "password", true)]
    public void Login_WhenRequiredFieldsChange_ValidatesExpectedResult(
        string username, string password, bool expectedValid)
    {
        // Arrange
        var model = new LoginViewModel
        {
            Username = username,
            Password = password
        };
        var errors = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(
            model, new ValidationContext(model), errors,
            validateAllProperties: true);

        // Assert
        Assert.Equal(expectedValid, isValid);
    }
}
