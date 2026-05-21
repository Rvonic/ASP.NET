using Xunit;
using PrviLabos.Model;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PrviLabos.Tests;

public class EntityValidationTests
{
	[Fact]
	public void Model_types_compile_and_basic_validation_works()
	{
		var c = new Customer { FirstName = "A", LastName = "B", Email = "a@b.com", PhoneNumber = "123-456-7890", DriverLicenseNumber = "DL" };
		var results = new List<ValidationResult>();
		var ok = Validator.TryValidateObject(c, new ValidationContext(c), results, true);
		Assert.True(ok);
	}
}
