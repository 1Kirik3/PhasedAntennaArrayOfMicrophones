using System.Globalization;
using System.Windows.Controls;

namespace PAAOM_Common.ValidationRules
{
    public class DecimalRangeValidationRule : ValidationRule
    {
        public decimal Min { get; set; } = 0;
        public decimal Max { get; set; } = 1;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string stringValue)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                    return new ValidationResult(false, "Значение не может быть пустым");

                if (decimal.TryParse(stringValue.Replace(',', '.'), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out decimal result))
                {
                    if (result >= Min && result <= Max)
                        return ValidationResult.ValidResult;

                    return new ValidationResult(false, $"Значение должно быть между {Min} и {Max}");
                }

                return new ValidationResult(false, "Введите корректное число");
            }

            return new ValidationResult(false, "Некорректное значение");
        }
    }
}
