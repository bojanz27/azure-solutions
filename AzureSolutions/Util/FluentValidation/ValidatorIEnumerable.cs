using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureSolutions.Util.FluentValidation
{
    public static class ValidatorIEnumerable
    {
        public static Validator<IEnumerable<T>> IsNotEmpty<T>(this Validator<IEnumerable<T>> validator)
        {
            if (!validator.Value.Any())
                throw new ArgumentException($"{validator.Name ?? "collection"} cannot be empty.");
            return validator;
        }

        public static Validator<IEnumerable<T>> IsNotNullOrEmpty<T>(this Validator<IEnumerable<T>> validator)
        {
            validator.IsNotNull();
            validator.IsNotEmpty();
            return validator;
        }
    }
}
