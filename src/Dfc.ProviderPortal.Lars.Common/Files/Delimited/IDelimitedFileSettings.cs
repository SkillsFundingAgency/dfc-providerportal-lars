namespace Dfc.ProviderPortal.Lars.Common.Files.Delimited
{
    public interface IDelimitedFileSettings
    {
        char DelimitingCharacter { get; }
        bool IsFirstRowHeaders { get; }
    }
}