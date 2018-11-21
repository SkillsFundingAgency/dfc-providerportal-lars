using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dfc.ProviderPortal.Lars.Common.Files.Delimited
{
    public static class DelimitedFileReader
    {
        public static IEnumerable<IDelimitedLine> ReadLines(TextReader reader, IDelimitedFileSettings settings)
        {
            Throw.IfNull(reader, nameof(reader));
            Throw.IfNull(settings, nameof(settings));

            try
            {
                var number = settings.IsFirstRowHeaders ? -1 : 0;
                while (reader.Peek() > -1) yield return ReadLine(number++, reader.ReadLine(), settings);
            }
            finally
            {
                if (reader != null) reader.Close();
            }
        }

        public static IEnumerable<IDelimitedLine> ReadLines(string path, IDelimitedFileSettings settings)
        {
            Throw.IfNullOrWhiteSpace(path, nameof(path));
            Throw.IfNull(settings, nameof(settings));

            if (!File.Exists(path)) throw new FileNotFoundException(nameof(path));

            using (var streamReader = new StreamReader(path))
            {
                return ReadLines(streamReader, settings);
            }
        }

        internal static IDelimitedLine ReadLine(int number, string line, IDelimitedFileSettings settings)
        {
            Throw.IfLessThan(0, number, nameof(number));
            Throw.IfNull(settings, nameof(settings));

            var fields = new List<IDelimitedField>();
            var fieldCount = 0;
            var chars = line == null ? new char[] { } : line.ToCharArray();
            var buffer = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < chars.Length; i++)
            {
                if (inQuotes)
                {
                    var nextChar = (i + 1) == chars.Length ? chars[i] : chars[i + 1];

                    if (chars[i] == '"' && nextChar == settings.DelimitingCharacter
                        || chars[i] == '"' && nextChar == chars[i] && (i + 1) == chars.Length
                        || chars[i] == '"' && Environment.NewLine.Contains(nextChar))
                    {
                        fieldCount++;
                        fields.Add(new DelimitedField(fieldCount, buffer.ToString(), inQuotes));
                        buffer = new StringBuilder();
                        inQuotes = false;
                    }
                    else
                    {
                        buffer.Append(chars[i]);
                    }
                }
                else
                {
                    if (chars[i] == settings.DelimitingCharacter)
                    {
                        var prevChar = i == 0 ? chars[i] : chars[i - 1];

                        if (buffer.Length > 0 || prevChar == settings.DelimitingCharacter)
                        {
                            fieldCount++;
                            fields.Add(new DelimitedField(fieldCount, buffer.ToString()));
                            buffer = new StringBuilder();
                        }

                        continue;
                    }
                    else if (Environment.NewLine.Contains(chars[i]))
                    {
                        break;
                    }
                    else if (chars[i] == '"' && buffer.Length == 0)
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        buffer.Append(chars[i]);
                    }
                }
            }

            var lastChar = chars.Length == 0 ? default(char) : chars[chars.Length - 1];

            if (buffer.Length > 0 || lastChar == settings.DelimitingCharacter)
            {
                fieldCount++;
                fields.Add(new DelimitedField(fieldCount, buffer.ToString()));
            }

            return new DelimitedLine(number, fields);
        }
    }
}