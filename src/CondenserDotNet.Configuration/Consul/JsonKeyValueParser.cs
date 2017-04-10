using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CondenserDotNet.Configuration.Consul
{
    public class JsonKeyValueParser : IKeyParser
    {
        private readonly Stack<string> _context = new Stack<string>();
        private readonly SortedDictionary<string, string> _data =
            new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string _currentPath;
        private JsonTextReader _reader;

        public IEnumerable<KeyValue> Parse(KeyValue key)
        {
            if (key.Value == null) yield break;

            _data.Clear();

            _reader = new JsonTextReader(new StringReader(key.ValueFromBase64()))
            {
                DateParseHandling = DateParseHandling.None
            };

            var jsonConfig = JToken.Load(_reader);

            VisitToken(jsonConfig);

            foreach (var kv in _data)
            {
                yield return new KeyValue
                {
                    Key = key.Key + ConfigurationPath.KeyDelimiter + kv.Key,
                    Value = kv.Value,
                    IsDerivedKey = true
                };
            }
        }

        private void VisitJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                EnterContext(property.Name);
                VisitProperty(property);
                ExitContext();
            }
        }

        private void VisitProperty(JProperty property) => VisitToken(property.Value);

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token);
                    break;
                default:
                    throw new FormatException(
                        $"Unsupported JSON token '{_reader.TokenType}' was found. Path '{_reader.Path}', line {_reader.LineNumber} position {_reader.LinePosition}.");
            }
        }

        private void VisitArray(JArray array)
        {
            for (var index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        private void VisitPrimitive(JToken data)
        {
            var key = _currentPath;
            if (_data.ContainsKey(key))
            {
                throw new FormatException($"A duplicate key '{key}' was found.");
            }
            _data[key] = data.ToString();
        }

        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }
    }
}