using System.Collections.Generic;

namespace Dfc.ProviderPortal.Lars.Common.Files.Delimited
{
    public class DelimitedField : ValueObject<DelimitedField>, IDelimitedField
    {
        public int Number { get; }
        public string Value { get; }
        public bool IsDoubleQuoted { get; }

        public DelimitedField(int number, string value, bool isDoubleQuoted = false)
        {
            Throw.IfLessThan(1, number, nameof(number));

            Number = number;
            Value = value;// ?? throw new ArgumentNullException(nameof(value));
            IsDoubleQuoted = isDoubleQuoted;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Number;
            yield return Value;
            yield return IsDoubleQuoted;
        }
    }
}