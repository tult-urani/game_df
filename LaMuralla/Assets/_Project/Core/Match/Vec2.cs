using System;

namespace LaMuralla.Core.Match
{
    /// <summary>
    /// Vector 2D. Tồn tại vì core KHÔNG được tham chiếu UnityEngine — đó là điều
    /// duy nhất cho phép chạy `dotnet test` mà không cần cài Unity. Lớp vỏ Unity
    /// đổi qua lại với UnityEngine.Vector2 ở biên.
    ///
    /// `double` chứ không `float`: hình học đường chạy được đối chiếu với
    /// tools/path_check.py (Python, double). Dùng float thì hai bên lệch nhau và
    /// không còn biết ai đúng.
    /// </summary>
    public readonly struct Vec2 : IEquatable<Vec2>
    {
        public double X { get; }
        public double Y { get; }

        public Vec2(double x, double y) { X = x; Y = y; }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, double k) => new Vec2(a.X * k, a.Y * k);
        public static Vec2 operator *(double k, Vec2 a) => a * k;

        public double Length => Math.Sqrt(X * X + Y * Y);
        public double SqrLength => X * X + Y * Y;

        public static double Distance(Vec2 a, Vec2 b) => (a - b).Length;

        /// <summary>Khoảng cách bình phương. Dùng khi chỉ cần SO SÁNH với tầm —
        /// tránh Sqrt trong vòng lặp nhắm mục tiêu chạy mỗi frame cho mỗi tướng.</summary>
        public static double SqrDistance(Vec2 a, Vec2 b) => (a - b).SqrLength;

        public bool Equals(Vec2 o) => X.Equals(o.X) && Y.Equals(o.Y);
        public override bool Equals(object? o) => o is Vec2 v && Equals(v);
        public override int GetHashCode() => (X, Y).GetHashCode();
        public override string ToString() => $"({X:0.###}, {Y:0.###})";
    }
}
