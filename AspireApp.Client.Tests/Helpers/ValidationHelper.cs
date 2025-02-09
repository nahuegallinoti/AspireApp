using System.ComponentModel.DataAnnotations;

namespace AspireApp.Tests.Client.Helpers
{
    public static class ValidationHelper
    {
        public static string GetErrorMessage(object instance, string propertyName)
        {
            var propertyInfo = instance.GetType().GetProperty(propertyName);

            var validationAttributes = propertyInfo?.GetCustomAttributes(typeof(ValidationAttribute), false)
                                                     .Cast<ValidationAttribute>()
                                                     .ToList();

            if (validationAttributes is null || validationAttributes.Count is 0)
                return "No validation attributes found.";

            var validationAttribute = validationAttributes.FirstOrDefault();

            var errorMessage = validationAttribute?.ErrorMessage ?? "No error message found.";

            return string.Format(errorMessage, propertyName);
        }
    }
}