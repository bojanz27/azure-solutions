using System;

namespace AzureSolutions.Util.FluentValidation
{
    public static class ValidatorDouble
    {
        public static Validator<double> IsInRange(this Validator<double> validator, double min, double max)
        {
            validator.IsNotLowerThen(min);
            validator.IsNotGreaterThen(max);
            return validator;
        }

        public static Validator<double> IsNotGreaterThen(this Validator<double> validator, double max)
        {
            if (validator.Value > max)
                throw new ArgumentOutOfRangeException($"{validator.Name ?? "value"} cannot be greater then {max}.");
            return validator;
        }

        public static Validator<double> IsNotLowerThen(this Validator<double> validator, double min)
        {
            if (validator.Value < min)
                throw new ArgumentOutOfRangeException($"{validator.Name ?? "value"} cannot be lower then {min}.");
            return validator;
        }

        public static Validator<double> IsGreaterThen(this Validator<double> validator, double bound)
        {
            if (validator.Value <= bound)
                throw new ArgumentOutOfRangeException($"{validator.Name ?? "value"} must be greater then {bound}.");
            return validator;
        }
    }
}
