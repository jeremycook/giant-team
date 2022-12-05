using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;

namespace GiantTeam.Startup.Json
{
    internal sealed class JsonConfigurationFileParser
    {
        private JsonConfigurationFileParser() { }

        private readonly Dictionary<string, string?> _data = new(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _paths = new();

        public static IDictionary<string, string?> ParseObject(Stream input)
            => new JsonConfigurationFileParser().ParseStream(input, requireObjectRoot: true);

        public static IDictionary<string, string?> Parse(Stream input)
            => new JsonConfigurationFileParser().ParseStream(input, requireObjectRoot: false);

        private IDictionary<string, string?> ParseStream(Stream input, bool requireObjectRoot)
        {
            var jsonDocumentOptions = new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            using (var reader = new StreamReader(input))
            using (JsonDocument doc = JsonDocument.Parse(reader.ReadToEnd(), jsonDocumentOptions))
            {
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    VisitObjectElement(doc.RootElement);
                }
                else if (requireObjectRoot)
                {
                    throw new FormatException(string.Format("Root element must be an Object but JsonValueKind was {0}.", doc.RootElement.ValueKind));
                }
                else
                {
                    EnterContext(string.Empty);
                    VisitValue(doc.RootElement);
                    ExitContext();
                }
            }

            return _data;
        }

        private void VisitObjectElement(JsonElement element)
        {
            var isEmpty = true;

            foreach (JsonProperty property in element.EnumerateObject())
            {
                isEmpty = false;
                EnterContext(property.Name);
                VisitValue(property.Value);
                ExitContext();
            }

            SetNullIfElementIsEmpty(isEmpty);
        }

        private void VisitArrayElement(JsonElement element)
        {
            int index = 0;

            foreach (JsonElement arrayElement in element.EnumerateArray())
            {
                EnterContext(index.ToString());
                VisitValue(arrayElement);
                ExitContext();
                index++;
            }

            SetNullIfElementIsEmpty(isEmpty: index == 0);
        }

        private void SetNullIfElementIsEmpty(bool isEmpty)
        {
            if (isEmpty && _paths.Count > 0)
            {
                _data[_paths.Peek()] = null;
            }
        }

        private void VisitValue(JsonElement value)
        {
            Debug.Assert(_paths.Count > 0);

            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    VisitObjectElement(value);
                    break;

                case JsonValueKind.Array:
                    VisitArrayElement(value);
                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    string key = _paths.Peek();
                    if (_data.ContainsKey(key))
                    {
                        throw new FormatException(string.Format("Duplicate key: {0}.", key));
                    }
                    _data[key] = value.ToString();
                    break;

                default:
                    throw new FormatException(string.Format("Unsupported JsonValueKind: {0}.", value.ValueKind));
            }
        }

        private void EnterContext(string context) =>
            _paths.Push(_paths.Count > 0 ?
                _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
                context);

        private void ExitContext() => _paths.Pop();
    }
}