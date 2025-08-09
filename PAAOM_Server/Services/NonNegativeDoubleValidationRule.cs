using System.Globalization;
using System.Windows.Controls;

namespace PAAOM_Server.Services
{
	public class NonNegativeDoubleValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
				return new ValidationResult(false, "Value cannot be empty");

			if (!double.TryParse(value.ToString(), NumberStyles.Any, cultureInfo, out double result))
				return new ValidationResult(false, "Value must be a number");

			if (result < 0)
				return new ValidationResult(false, "Value cannot be negative");

			return ValidationResult.ValidResult;
		}
	}
}
