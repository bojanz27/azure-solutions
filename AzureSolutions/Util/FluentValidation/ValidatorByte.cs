using System;

namespace AzureSolutions.Util.FluentValidation
{
    public static class ValidatorByte
    {
        public static Validator<byte> IsInRange(this Validator<byte> validator, byte min, byte max)
        {
            validator.IsNotLowerThen(min);
            validator.IsNotGreaterThen(max);
            return validator;
        }

        public static Validator<byte> IsNotGreaterThen(this Validator<byte> validator, byte max)
        {
            if (validator.Value > max)
                throw new ArgumentOutOfRangeException($"{validator.Name ?? "value"} cannot be greater then {max}.");
            return validator;
        }

        public static Validator<byte> IsNotLowerThen(this Validator<byte> validator, byte min)
        {
            if (validator.Value < min)
                throw new ArgumentOutOfRangeException($"{validator.Name ?? "value"} cannot be lower then {min}.");
            return validator;
        }

        public static Validator<byte> IsGreaterThen(this Validator<byte> validator, byte bound)
        {
            if (validator.Value <= bound)
                throw new ArgumentOutOfRangeException($"{validator.Name ?? "value"} must be greater then {bound}.");
            return validator;
        }

    }
}
