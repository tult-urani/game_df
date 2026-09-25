using System;
using System.Collections.Generic;
using System.Globalization;

namespace LaMuralla.Core.Json
{
    /// <summary>
    /// Cây JSON tối giản. Tồn tại vì netstandard2.1 KHÔNG có System.Text.Json,
    /// còn JsonUtility của Unity không deserialize được Dictionary (mà
    /// `waves[].spawns` chính là Dictionary). Xem docs/05 §1.
    ///
    /// Chỉ chạy trong Editor lúc bake JSON → ScriptableObject. KHÔNG ship vào app.
    /// </summary>
    public abstract class JsonValue
    {
        /// <summary>Đường dẫn tới node này, ví dụ `towers[3].levels[0].cost`.
        /// Có mặt để thông báo lỗi chỉ đúng chỗ thay vì "JSON không hợp lệ".</summary>
        public string Path { get; internal set; } = "$";

        public virtual JsonObject AsObject() => throw TypeError("object");
        public virtual JsonArray AsArray() => throw TypeError("array");
        public virtual double AsNumber() => throw TypeError("number");
        public virtual string AsString() => throw TypeError("string");
        public virtual bool AsBool() => throw TypeError("bool");

        public int AsInt()
        {
            double d = AsNumber();
            if (Math.Abs(d % 1) > double.Epsilon)
                throw new JsonException($"{Path}: cần số nguyên, gặp {d.ToString(CultureInfo.InvariantCulture)}");
            return (int)d;
        }

        private JsonException TypeError(string want) =>
            new JsonException($"{Path}: cần {want}, gặp {TypeName}");

        public abstract string TypeName { get; }
    }

    public sealed class JsonObject : JsonValue
    {
        private readonly Dictionary<string, JsonValue> _items;

        public JsonObject(Dictionary<string, JsonValue> items) => _items = items;

        public override string TypeName => "object";
        public override JsonObject AsObject() => this;

        public IReadOnlyDictionary<string, JsonValue> Items => _items;
        public IEnumerable<string> Keys => _items.Keys;
        public bool Has(string key) => _items.ContainsKey(key);

        /// <summary>Bắt buộc có. Thiếu = ném, vì config thiếu trường là lỗi build,
        /// không phải chuyện dùng giá trị mặc định cho qua.</summary>
        public JsonValue this[string key] =>
            _items.TryGetValue(key, out JsonValue? v)
                ? v
                : throw new JsonException($"{Path}: thiếu trường bắt buộc `{key}`");

        /// <summary>Tuỳ chọn. Trả null nếu không có.</summary>
        public JsonValue? Opt(string key) => _items.TryGetValue(key, out JsonValue? v) ? v : null;
    }

    public sealed class JsonArray : JsonValue
    {
        private readonly List<JsonValue> _items;

        public JsonArray(List<JsonValue> items) => _items = items;

        public override string TypeName => "array";
        public override JsonArray AsArray() => this;

        public int Count => _items.Count;
        public JsonValue this[int i] => _items[i];
        public IReadOnlyList<JsonValue> Items => _items;
    }

    public sealed class JsonNumber : JsonValue
    {
        private readonly double _value;
        public JsonNumber(double value) => _value = value;
        public override string TypeName => "number";
        public override double AsNumber() => _value;
    }

    public sealed class JsonString : JsonValue
    {
        private readonly string _value;
        public JsonString(string value) => _value = value;
        public override string TypeName => "string";
        public override string AsString() => _value;
    }

    public sealed class JsonBool : JsonValue
    {
        private readonly bool _value;
        public JsonBool(bool value) => _value = value;
        public override string TypeName => "bool";
        public override bool AsBool() => _value;
    }

    public sealed class JsonNull : JsonValue
    {
        public override string TypeName => "null";
    }

    public sealed class JsonException : Exception
    {
        public JsonException(string message) : base(message) { }
    }
}
