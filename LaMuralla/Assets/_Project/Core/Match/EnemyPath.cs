using System;
using System.Collections.Generic;

namespace LaMuralla.Core.Match
{
    /// <summary>
    /// Đường chạy: spline Catmull-Rom qua waypoint, lấy mẫu sẵn thành polyline +
    /// bảng độ dài tích luỹ.
    ///
    /// ⚠️ Cách lấy mẫu ở đây PHẢI khớp từng bit với tools/path_check.py, vì:
    ///   · path_check.py ép luật "độ dài spline khớp lengthUnits ±2%"
    ///   · balance_sim.py tính chord từ chính polyline đó → ra toàn bộ bảng wave
    /// Lệch cách lấy mẫu = game và bảng cân bằng nói về hai đường khác nhau.
    /// Test PathOracleTests khoá điều này lại bằng con số Python thật.
    /// </summary>
    public sealed class EnemyPath
    {
        /// <summary>Khớp SAMPLES trong tools/path_check.py. Đổi một bên mà không
        /// đổi bên kia thì độ dài đường lệch — test oracle sẽ đỏ.</summary>
        public const int Samples = 4000;

        private readonly Vec2[] _points;
        private readonly double[] _cumulative;   // _cumulative[i] = quãng đường tới _points[i]

        public double Length => _cumulative[_cumulative.Length - 1];
        public Vec2 Spawn => _points[0];
        public Vec2 Goal => _points[_points.Length - 1];
        public int SampleCount => _points.Length;

        public EnemyPath(IReadOnlyList<Vec2> waypoints)
        {
            if (waypoints == null || waypoints.Count < 2)
                throw new ArgumentException("đường chạy cần ít nhất 2 waypoint", nameof(waypoints));

            _points = Sample(waypoints);
            _cumulative = new double[_points.Length];
            double total = 0;
            for (int i = 1; i < _points.Length; i++)
            {
                total += Vec2.Distance(_points[i - 1], _points[i]);
                _cumulative[i] = total;
            }
        }

        /// <summary>
        /// Nhân đôi điểm đầu/cuối để spline CHẠM waypoint đầu và cuối, thay vì chỉ
        /// đi ngang qua chúng — spawn và cầu môn phải nằm đúng chỗ đã khai.
        /// </summary>
        private static Vec2[] Sample(IReadOnlyList<Vec2> wp)
        {
            var p = new List<Vec2>(wp.Count + 2) { wp[0] };
            p.AddRange(wp);
            p.Add(wp[wp.Count - 1]);

            int segments = p.Count - 3;
            int per = Math.Max(2, Samples / segments);
            var outPts = new List<Vec2>(segments * per + 1);

            for (int i = 0; i < segments; i++)
            {
                Vec2 p0 = p[i], p1 = p[i + 1], p2 = p[i + 2], p3 = p[i + 3];
                for (int j = 0; j < per; j++)
                {
                    double t = (double)j / per;
                    outPts.Add(CatmullRom(p0, p1, p2, p3, t));
                }
            }
            outPts.Add(wp[wp.Count - 1]);
            return outPts.ToArray();
        }

        private static Vec2 CatmullRom(Vec2 p0, Vec2 p1, Vec2 p2, Vec2 p3, double t)
        {
            double t2 = t * t, t3 = t2 * t;
            return new Vec2(
                0.5 * (2 * p1.X
                       + (-p0.X + p2.X) * t
                       + (2 * p0.X - 5 * p1.X + 4 * p2.X - p3.X) * t2
                       + (-p0.X + 3 * p1.X - 3 * p2.X + p3.X) * t3),
                0.5 * (2 * p1.Y
                       + (-p0.Y + p2.Y) * t
                       + (2 * p0.Y - 5 * p1.Y + 4 * p2.Y - p3.Y) * t2
                       + (-p0.Y + 3 * p1.Y - 3 * p2.Y + p3.Y) * t3));
        }

        /// <summary>
        /// Vị trí sau khi đi được <paramref name="distance"/> đơn vị dọc đường.
        /// Kẹp ở hai đầu: quân chưa spawn đứng ở spawn, quân đã tới thì đứng ở cầu môn.
        /// </summary>
        public Vec2 PositionAt(double distance)
        {
            if (distance <= 0) return _points[0];
            if (distance >= Length) return _points[_points.Length - 1];

            int i = UpperBound(distance);
            double segStart = _cumulative[i - 1];
            double segLen = _cumulative[i] - segStart;
            // Đoạn dài 0 xảy ra khi hai mẫu trùng nhau (waypoint nhân đôi ở đầu).
            // Chia cho 0 ở đây cho ra NaN và quân biến mất khỏi màn hình.
            double f = segLen > 0 ? (distance - segStart) / segLen : 0;
            return _points[i - 1] + (_points[i] - _points[i - 1]) * f;
        }

        /// <summary>Chỉ số mẫu đầu tiên có quãng đường tích luỹ ≥ distance.
        /// Nhị phân, không quét tuyến tính: gọi mỗi frame cho mỗi con quân.</summary>
        private int UpperBound(double distance)
        {
            int lo = 1, hi = _cumulative.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (_cumulative[mid] < distance) lo = mid + 1;
                else hi = mid;
            }
            return lo;
        }

        /// <summary>Quãng đường tích luỹ tại mẫu gần <paramref name="p"/> nhất.
        /// Dùng để đo, không dùng trong vòng lặp mỗi frame — nó quét toàn bộ mẫu.</summary>
        public double DistanceAlongNearest(Vec2 p)
        {
            int best = 0;
            double bestSqr = double.MaxValue;
            for (int i = 0; i < _points.Length; i++)
            {
                double d = Vec2.SqrDistance(_points[i], p);
                if (d < bestSqr) { bestSqr = d; best = i; }
            }
            return _cumulative[best];
        }

        /// <summary>Khoảng cách ngắn nhất từ <paramref name="p"/> tới đường.</summary>
        public double DistanceToPath(Vec2 p)
        {
            double bestSqr = double.MaxValue;
            foreach (Vec2 s in _points)
            {
                double d = Vec2.SqrDistance(s, p);
                if (d < bestSqr) bestSqr = d;
            }
            return Math.Sqrt(bestSqr);
        }
    }
}
