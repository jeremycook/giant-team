using System.Text;

namespace GiantTeam.Services
{
    public static class CsvHelper
    {
        public static class Defaults
        {
            public const char Separator = ',';
            public const char StringDelimiter = '"';
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="separator"></param>
        /// <param name="stringDelimiter"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Unexpected character.</exception>
        public static async Task<IReadOnlyList<string>> ParseRecordAsync(StreamReader reader, char separator = Defaults.Separator, char stringDelimiter = Defaults.StringDelimiter)
        {
            string? line = await reader.ReadLineAsync();

            List<string> chunks = new();

            if (line is null)
            {
                return chunks;
            }

            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];

                if (ch == separator)
                {
                    i++;
                    if (i < line.Length)
                    {
                        ch = line[i];
                        if (ch == stringDelimiter)
                        {
                            StringBuilder delimitedString = new();
                            while (true)
                            {
                                var (text, end, finished) = TryParseDelimitedString(line, i, stringDelimiter);

                                delimitedString.Append(text);
                                i = end;

                                if (finished)
                                {
                                    chunks.Add(delimitedString.ToString());
                                    break;
                                }
                                else if (reader.EndOfStream)
                                {
                                    throw new InvalidOperationException($"Unexpectedly found the end of stream. The {stringDelimiter} string delimiter character was expected.");
                                }
                                else
                                {
                                    line += "\n" + await reader.ReadLineAsync();
                                }
                            }
                        }
                        else
                        {
                            var (text, end) = ReadUndelimitedString(line, i, separator);
                            chunks.Add(text);
                            i = end;
                        }
                    }
                    else // end of line
                    {
                        chunks.Add(string.Empty);
                    }
                }
                else if (i == 0) // beginning of line
                {
                    if (ch == stringDelimiter)
                    {
                        var (text, end, readToEnd) = TryParseDelimitedString(line, i, stringDelimiter);
                        chunks.Add(text);
                        i = end;
                    }
                    else
                    {
                        var (text, end) = ReadUndelimitedString(line, i, separator);
                        chunks.Add(text);
                        i = end;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected character at {i}. The {separator} separator character was expected.");
                }
            }

            return chunks;
        }

        static (string text, int end) ReadUndelimitedString(string line, int start, char separator)
        {
            int end = line.IndexOf(separator, start);
            if (end < 0)
            {
                end = line.Length;
            }
            string text = line[start..end];

            return (text, end - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="start">The index in <paramref name="line"/> where the delimited string starts or continues.</param>
        /// <param name="stringDelimiter"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Unexpected character.</exception>
        static (string text, int end, bool finished) TryParseDelimitedString(string line, int start, char stringDelimiter)
        {
            bool sawStringDelimiter = false;

            StringBuilder sb = new();

            int i;
            for (i = start + 1; i < line.Length; i++)
            {
                char ch = line[i];

                if (ch == stringDelimiter)
                {
                    if (!sawStringDelimiter)
                    {
                        // The string delimiter character is an escape character
                        // if it precedes another string delimiter, otherwise it
                        // terminates the string, but we won't know until we read
                        // the next character.

                        // Skip appending it and flag that we saw a string delimiter.
                        sawStringDelimiter = true;
                    }
                    else // saw string delimiter
                    {
                        // This character was escaped.
                        // Append it and reset the flag.
                        sb.Append(ch);
                        sawStringDelimiter = false;
                    }
                }
                else
                {
                    if (sawStringDelimiter)
                    {
                        // Reached the end of the delimited string.
                        break;
                    }
                    else
                    {
                        // Keep consuming
                        sb.Append(ch);
                    }
                }
            }

            if (sawStringDelimiter)
            {
                // Reached the end of the delimited string.
                string token = sb.ToString();
                return (token, i - 1, finished: true);
            }
            else
            {
                // We reached the end of the line but didn't find the end of the string.
                string token = sb.ToString();
                return (token, i - 1, finished: false);
            }
        }
    }
}
