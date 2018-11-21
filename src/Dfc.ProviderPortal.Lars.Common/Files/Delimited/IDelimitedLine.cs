using System.Collections.Generic;

namespace Dfc.ProviderPortal.Lars.Common.Files.Delimited
{
    public interface IDelimitedLine
    {
        int Number { get; }
        IReadOnlyList<IDelimitedField> Fields { get; }
    }
}