using System.Collections.Generic;
using System.Linq;

namespace Dfc.ProviderPortal.Lars.Common.Files.Delimited
{
    public class DelimitedLine : ValueObject<DelimitedLine>, IDelimitedLine
    {
        public int Number { get; }
        public IReadOnlyList<IDelimitedField> Fields { get; }

        public DelimitedLine(
            int number,
            IEnumerable<IDelimitedField> fields)
        {
            Throw.IfLessThan(1, number, nameof(number));
            Throw.IfNull(fields, nameof(fields));

            Number = number;
            Fields = fields.ToList().AsReadOnly();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Number;
            yield return Fields;
        }
    }
}