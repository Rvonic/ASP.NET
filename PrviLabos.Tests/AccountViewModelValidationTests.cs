using System.ComponentModel.DataAnnotations;
using PrviLabos.Models;
using Xunit;

namespace PrviLabos.Tests;

public sealed class AccountViewModelValidationTests
{
    [Fact]
    public void RegisterViewModel_ShouldRequireValidOibAndJmbg()
    {
        var model = new RegisterViewModel
        {
            Email = "user@example.test",
            Password = "Password1",
            ConfirmPassword = "Password1",
            FirstName = "Test",
            LastName = "User",
            OIB = "123",
            JMBG = "abc"
        };

        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.OIB)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterViewModel.JMBG)));
    }

    [Fact]
    public void ExternalLoginConfirmationViewModel_ShouldAcceptValidOibAndJmbg()
    {
        var model = new ExternalLoginConfirmationViewModel
        {
            Email = "external@example.test",
            FirstName = "External",
            LastName = "User",
            OIB = "12345678901",
            JMBG = "1234567890123"
        };

        var results = Validate(model);

        Assert.Empty(results);
    }

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
