using System.Collections.Generic;

namespace Dfc.ProviderPortal.Lars.Common.Files.Delimited
{
    public class DelimitedFileSettings : ValueObject<DelimitedFileSettings>, IDelimitedFileSettings
    {
        private const char DELIMITED_CHARACTER = ',';
        private const bool IS_FIRST_ROW_HEADERS = false;

        public char DelimitingCharacter { get; }
        public bool IsFirstRowHeaders { get; }

        public DelimitedFileSettings()
            : this(DELIMITED_CHARACTER, IS_FIRST_ROW_HEADERS) { }

        public DelimitedFileSettings(bool isFirstRowHeaders)
            : this(DELIMITED_CHARACTER, isFirstRowHeaders) { }

        public DelimitedFileSettings(char delimitingCharacter)
            : this(delimitingCharacter, IS_FIRST_ROW_HEADERS) { }

        public DelimitedFileSettings(
            char delimitingCharacter,
            bool isFirstRowHeaders)
        {
            DelimitingCharacter = delimitingCharacter;
            IsFirstRowHeaders = isFirstRowHeaders;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DelimitingCharacter;
            yield return IsFirstRowHeaders;
        }
    }
}