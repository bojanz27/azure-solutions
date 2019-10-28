using System;

namespace AzureSolutions.Util.FluentValidation
{
    public static class Requires
    {
        public static Validator<T> That<T>(T value, string withName = null)
        {
            return new Validator<T>(value, withName);
        }

        public static Validator<TMem> That<T, TMem>(T value, Func<T, TMem> memberSelector, string withName = null)
        {
            if (memberSelector == null)
                throw new ArgumentNullException(nameof(memberSelector));
            return new Validator<TMem>(memberSelector(value), withName);
        }
    }
}
