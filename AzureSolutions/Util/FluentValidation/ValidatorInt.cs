using System;

namespace AzureSolutions.Util.FluentValidation
{
    public static class ValidatorInt
    {
        public static Validator<int> IsInRange(this Validator<int> validator, int min, int max)
        {
            validator.IsNotLowerThen(min);
            validator.IsNotGreaterThen(max);
            return validator;
        }

        public static Validator<int> IsNotGreaterThen(this Validator<int> validator, int max)
        {
            if (validator.Value > max)
                throw new ArgumentOutOfRangeException($"{validator.Name ?? "value"} cannot be greater then {max}.");
            return validator;
        }

        public static Validator<int> IsNotLowerThen(this Validator<int> validator, int min)
        {
            if (validator.Value < min)
                throw new ArgumentOutOfRangeException($"{validator.Name ?? "value"} cannot be lower then {min}.");
            return validator;
        }

        public static Validator<int> IsGreaterThen(this Validator<int> validator, int bound)
        {
            if (validator.Value <= bound)
                throw new ArgumentOutOfRangeException($"{validator.Name ?? "value"} must be greater then {bound}.");
            return validator;
        }
    }
}
