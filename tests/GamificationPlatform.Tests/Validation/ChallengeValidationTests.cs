using System.ComponentModel.DataAnnotations;
using GamificationPlatform.Models;
using Xunit;

namespace GamificationPlatform.Tests.Validation;

public class ChallengeValidationTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void TitleValidation_WhenLengthChanges_ReturnsExpectedResult(
        int titleLength,
        bool expectedValid)
    {
        // Arrange: prepare a title and its validation context.
        var challenge = new Challenge
        {
            Title = new string('A', titleLength)
        };

        var context = new ValidationContext(challenge)
        {
            MemberName = nameof(Challenge.Title)
        };

        var errors = new List<ValidationResult>();

        // Act: run the actual validation attributes on Title.
        bool isValid = Validator.TryValidateProperty(
            challenge.Title,
            context,
            errors);

        // Assert: check whether the title is accepted.
        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
[InlineData(0, false)]
[InlineData(1, true)]
[InlineData(500, true)]
[InlineData(501, false)]
public void DescriptionValidation_WhenLengthChanges_ReturnsExpectedResult(
    int descriptionLength,
    bool expectedValid)
{
    // Arrange
    var challenge = new Challenge
    {
        Description = new string('A', descriptionLength)
    };

    var context = new ValidationContext(challenge)
    {
        MemberName = nameof(Challenge.Description)
    };

    var errors = new List<ValidationResult>();

    // Act
    bool isValid = Validator.TryValidateProperty(
        challenge.Description,
        context,
        errors);

    // Assert
    Assert.Equal(expectedValid, isValid);
}

[Theory]
[InlineData(null, true)]
[InlineData(10, true)]
[InlineData(20, true)]
[InlineData(30, true)]
[InlineData(60, true)]
[InlineData(90, true)]
[InlineData(120, true)]
[InlineData(-1, false)]
[InlineData(0, false)]
[InlineData(15, false)]
[InlineData(121, false)]
public void TimeLimitValidation_WhenValueChanges_ReturnsExpectedResult(
    int? minutes,
    bool expectedValid)
{
    // Arrange
    var challenge = new Challenge
    {
        TimeLimitMinutes = minutes
    };

    var context = new ValidationContext(challenge)
    {
        MemberName = nameof(Challenge.TimeLimitMinutes)
    };

    var errors = new List<ValidationResult>();

    // Act
    bool isValid = Validator.TryValidateProperty(
        challenge.TimeLimitMinutes,
        context,
        errors);

    // Assert
    Assert.Equal(expectedValid, isValid);
}
}