using System;

namespace AzureSolutions.Util.FluentValidation
{
    public static class ValidatorString
    {
        public static Validator<string> IsNotNullOrEmpty(this Validator<string> validator)
        {
            if (string.IsNullOrEmpty(validator.Value))
                throw new ArgumentException($"{validator.Name ?? "value"} cannot be null or empty.");

            return validator;
        }

        public static Validator<string> IsNotEmpty(this Validator<string> validator)
        {
            if (validator.Value.Length == 0)
                throw new ArgumentException($"{validator.Name ?? "value"} cannot be empty.");

            return validator;
        }

        // todo: implement StartsWith, EndsWith, Contains
    }
}
