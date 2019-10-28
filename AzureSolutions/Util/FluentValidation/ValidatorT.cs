using System;

namespace AzureSolutions.Util.FluentValidation
{
    public static class ValidatorT
    {
        public static Validator<T> IsNotDefault<T>(this Validator<T> validator)
        {
            var def = default(T);

            if (validator.Value.Equals(def))
                throw new ArgumentException($"{validator.Name ?? "value"} cannot have default value.");

            return validator;
        }

        public static Validator<T> IsNotNull<T>(this Validator<T> validator)
        {
            if (validator.Value == null)
                throw new ArgumentException($"{validator.Name ?? "value"} cannot be null.");

            return validator;
        }

        public static Validator<T> IsNotNullOrDefault<T>(this Validator<T> validator)
        {
            validator.IsNotNull();
            validator.IsNotDefault();
            return validator;
        }

        public static Validator<T> IsTypeOf<T, TOther>(this Validator<T> validator, TOther other)
        {
            if (validator.Value.GetType() != other.GetType())
                throw new ArgumentException($"{validator.Name ?? "value"} is not of type '{other.GetType().ToString()}'.");
            return validator;
        }
    }
}
