using System.Globalization;
using System.Windows.Controls;

namespace PAAOM_Common.ValidationRules
{
    public class NumericValidationRule : ValidationRule
    {
        public double Min { get; set; } = double.MinValue;
        public double Max { get; set; } = double.MaxValue;
        public bool AllowEmpty { get; set; } = false;
        public string ErrorMessage { get; set; } = "Некорректное значение";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string stringValue)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return AllowEmpty
                        ? ValidationResult.ValidResult
                        : new ValidationResult(false, "Значение обязательно для заполнения");
                }

                if (double.TryParse(stringValue.Replace(',', '.'), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double result))
                {
                    if (result >= Min && result <= Max)
                        return ValidationResult.ValidResult;

                    return new ValidationResult(false,
                        string.IsNullOrEmpty(ErrorMessage)
                            ? $"Значение должно быть между {Min} и {Max}"
                            : ErrorMessage);
                }

                return new ValidationResult(false, "Введите корректное число");
            }

            return new ValidationResult(false, "Некорректное значение");
        }
    }
}