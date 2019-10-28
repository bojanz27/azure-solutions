namespace AzureSolutions.Util.FluentValidation
{
    public class Validator<T>
    {
        public Validator(T value, string name = null)
        {
            Value = value;
            Name = name;
        }

        /// <summary>
        /// For internal use only - do not use.
        /// </summary>
        internal T Value { get; }

        /// <summary>
        /// Object name used in exeption message.
        /// </summary>
        internal string Name { get; }
    }
}
