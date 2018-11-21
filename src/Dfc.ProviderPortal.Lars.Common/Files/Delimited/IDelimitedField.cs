namespace Dfc.ProviderPortal.Lars.Common.Files.Delimited
{
    public interface IDelimitedField
    {
        int Number { get; }
        string Value { get; }
        bool IsDoubleQuoted { get; }
    }
}