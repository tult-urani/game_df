using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LaMuralla.Core.Json
{
    /// <summary>
    /// Parser JSON đệ quy xuống, đủ cho config/*.json và không hơn.
    ///
    /// Cố tình KHÔNG hỗ trợ: comment, dấu phẩy thừa, khoá không ngoặc kép.
    /// Đó không phải thiếu sót — config là dữ liệu máy sinh/máy đọc, nhận thêm
    /// cú pháp lỏng chỉ tạo chỗ cho hai công cụ hiểu khác nhau về cùng một file.
    /// </summary>
    public static class JsonParser
    {
        public static JsonValue Parse(string text)
        {
            var p = new Cursor(text);
            p.SkipWhitespace();
            JsonValue v = p.ParseValue("$");
            p.SkipWhitespace();
            if (!p.AtEnd)
                throw new JsonException($"Thừa ký tự sau giá trị JSON ở vị trí {p.Pos}");
            return v;
        }

        private sealed class Cursor
        {
            private readonly string _s;
            private int _i;

            internal Cursor(string s) => _s = s;

            internal bool AtEnd => _i >= _s.Length;
            internal int Pos => _i;

            private char Cur => _i < _s.Length
                ? _s[_i]
                : throw new JsonException("JSON kết thúc đột ngột");

            internal void SkipWhitespace()
            {
                while (_i < _s.Length && (_s[_i] == ' ' || _s[_i] == '\t' || _s[_i] == '\n' || _s[_i] == '\r'))
                    _i++;
            }

            internal JsonValue ParseValue(string path)
            {
                SkipWhitespace();
                JsonValue v = Cur switch
                {
                    '{' => ParseObject(path),
                    '[' => ParseArray(path),
                    '"' => new JsonString(ParseString()),
                    't' => ParseLiteral("true", new JsonBool(true)),
                    'f' => ParseLiteral("false", new JsonBool(false)),
                    'n' => ParseLiteral("null", new JsonNull()),
                    _ => ParseNumber(),
                };
                v.Path = path;
                return v;
            }

            private JsonValue ParseLiteral(string word, JsonValue value)
            {
                if (_i + word.Length > _s.Length || string.CompareOrdinal(_s, _i, word, 0, word.Length) != 0)
                    throw new JsonException($"Token không hợp lệ ở vị trí {_i}, cần `{word}`");
                _i += word.Length;
                return value;
            }

            private JsonObject ParseObject(string path)
            {
                _i++; // {
                var items = new Dictionary<string, JsonValue>();
                SkipWhitespace();
                if (Cur == '}') { _i++; return new JsonObject(items); }

                while (true)
                {
                    SkipWhitespace();
                    string key = ParseString();
                    // Khoá trùng = im lặng mất dữ liệu. Đã xảy ra thật ở vòng 3
                    // (`slowResistPercent` khai hai lần trong enemies.json).
                    if (items.ContainsKey(key))
                        throw new JsonException($"{path}: khoá trùng `{key}`");
                    SkipWhitespace();
                    if (Cur != ':') throw new JsonException($"{path}: cần `:` sau khoá `{key}`");
                    _i++;
                    items[key] = ParseValue(path == "$" ? key : $"{path}.{key}");
                    SkipWhitespace();
                    if (Cur == ',') { _i++; continue; }
                    if (Cur == '}') { _i++; return new JsonObject(items); }
                    throw new JsonException($"{path}: cần `,` hoặc `}}` ở vị trí {_i}");
                }
            }

            private JsonArray ParseArray(string path)
            {
                _i++; // [
                var items = new List<JsonValue>();
                SkipWhitespace();
                if (Cur == ']') { _i++; return new JsonArray(items); }

                while (true)
                {
                    items.Add(ParseValue($"{path}[{items.Count}]"));
                    SkipWhitespace();
                    if (Cur == ',') { _i++; continue; }
                    if (Cur == ']') { _i++; return new JsonArray(items); }
                    throw new JsonException($"{path}: cần `,` hoặc `]` ở vị trí {_i}");
                }
            }

            private string ParseString()
            {
                if (Cur != '"') throw new JsonException($"Cần chuỗi ở vị trí {_i}");
                _i++;
                var sb = new StringBuilder();
                while (true)
                {
                    char c = Cur;
                    _i++;
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }

                    char esc = Cur;
                    _i++;
                    switch (esc)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (_i + 4 > _s.Length)
                                throw new JsonException($"Escape \\u thiếu ký tự ở vị trí {_i}");
                            sb.Append((char)Convert.ToInt32(_s.Substring(_i, 4), 16));
                            _i += 4;
                            break;
                        default:
                            throw new JsonException($"Escape không hợp lệ `\\{esc}` ở vị trí {_i - 1}");
                    }
                }
            }

            private JsonNumber ParseNumber()
            {
                int start = _i;
                if (Cur == '-') _i++;
                while (!AtEnd && (char.IsDigit(_s[_i]) || _s[_i] == '.' ||
                                  _s[_i] == 'e' || _s[_i] == 'E' || _s[_i] == '+' || _s[_i] == '-'))
                    _i++;
                string slice = _s.Substring(start, _i - start);
                // InvariantCulture bắt buộc: máy đặt locale vi-VN đọc "0.69" thành 69.
                if (!double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    throw new JsonException($"Số không hợp lệ `{slice}` ở vị trí {start}");
                return new JsonNumber(d);
            }
        }
    }
}
