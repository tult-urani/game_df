using System;
using System.Collections.Generic;

namespace LaMuralla.Core.Match
{
    /// <summary>Ba cơ chế chậm ở docs/05 §5 luật 15. Nhìn `slowPercent: 50` thì
    /// giống hệt nhau, nhưng sức mạnh khác hẳn.</summary>
    public enum SlowScope
    {
        /// <summary>Chậm khi còn trong tầm; ra khỏi tầm là hết. `dibu.ap_dao`.</summary>
        InRange,
        /// <summary>Chậm trong durationSec rồi hết, bất kể tầm. `d10s.ban_tay_cua_chua`.</summary>
        Timed,
        /// <summary>Chậm vĩnh viễn. Thẻ vàng / thẻ đỏ của `el_arbitro`.</summary>
        Permanent,
    }

    /// <summary>
    /// Gom mọi nguồn làm chậm trên MỘT con quân và tính ra hệ số tốc độ.
    ///
    /// 🔴 THỨ TỰ PHÉP TÍNH LÀ LUẬT, KHÔNG PHẢI SỞ THÍCH — docs/01 §8 FM-14:
    ///
    ///     MAX các nguồn  →  cap 70%  →  RỒI mới nhân (1 − kháng/100)
    ///
    /// Làm ngược (kháng trước, cap sau) thì kháng chậm thành VÔ NGHĨA: boss kháng
    /// 75% ăn thẻ đỏ 70% → 70 × 0.25 = 17.5% → dưới cap → cap không cắt gì → boss
    /// vẫn bị chậm 17.5%. Đúng thứ tự: cap(70) = 70 → 70 × 0.25 = 17.5%. Trùng
    /// nhau ở ví dụ này, nhưng KHÁC khi tổng nguồn vượt cap: hai nguồn 50% + 60%
    /// → MAX = 60 → cap → 60 → ×0.25 = 15%. Nếu cộng dồn trước: 110 → ×0.25 =
    /// 27.5 → dưới cap → 27.5%. Gần gấp đôi.
    ///
    /// 🔴 LẤY MAX, KHÔNG CỘNG DỒN. Cộng dồn thì 3 Árbitro = 150% = quân đi lùi.
    /// </summary>
    public sealed class SlowStack
    {
        private readonly struct Source
        {
            internal readonly double Percent;
            internal readonly SlowScope Scope;
            internal readonly double Remaining;   // chỉ có nghĩa với Timed

            internal Source(double percent, SlowScope scope, double remaining)
            {
                Percent = percent; Scope = scope; Remaining = remaining;
            }
        }

        private readonly Dictionary<string, Source> _sources = new Dictionary<string, Source>();
        private readonly double _capPercent;

        /// <param name="capPercent">economy.json → slowCapPercent. Validator luật 6 ép ≤ 70.</param>
        public SlowStack(double capPercent)
        {
            if (capPercent < 0 || capPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(capPercent), capPercent, "cap phải trong [0, 100]");
            _capPercent = capPercent;
        }

        public int SourceCount => _sources.Count;

        /// <summary>Áp/làm mới một nguồn chậm. Cùng `sourceId` thì GHI ĐÈ, không
        /// thêm chồng — hai tướng cùng loại không được cộng dồn thành hai nguồn.</summary>
        public void Apply(string sourceId, double percent, SlowScope scope, double durationSec = 0)
        {
            if (scope == SlowScope.Timed && durationSec <= 0)
                throw new ArgumentException("scope=Timed cần durationSec > 0", nameof(durationSec));
            _sources[sourceId] = new Source(percent, scope, durationSec);
        }

        /// <summary>Quân ra khỏi tầm → gỡ nguồn InRange. Không đụng Timed/Permanent.</summary>
        public void Remove(string sourceId) => _sources.Remove(sourceId);

        /// <summary>Đếm ngược các nguồn Timed. Gọi bằng MatchDeltaTime (đã nhân hệ
        /// số ×2), KHÔNG phải Time.deltaTime thuần — xem docs/05 §4.4.</summary>
        public void Tick(double deltaSec)
        {
            if (deltaSec <= 0) return;
            List<string>? expired = null;
            foreach (KeyValuePair<string, Source> kv in _sources)
            {
                if (kv.Value.Scope != SlowScope.Timed) continue;
                double left = kv.Value.Remaining - deltaSec;
                if (left <= 0) (expired ??= new List<string>()).Add(kv.Key);
                else _sources[kv.Key] = new Source(kv.Value.Percent, SlowScope.Timed, left);
            }
            if (expired == null) return;
            foreach (string k in expired) _sources.Remove(k);
        }

        /// <summary>
        /// % chậm cuối cùng sau cap và kháng. Xem chú thích đầu lớp về thứ tự.
        /// </summary>
        /// <param name="resistPercent">enemies.json → slowResistPercent. Boss = 75.</param>
        public double EffectiveSlowPercent(double resistPercent)
        {
            if (resistPercent < 0 || resistPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(resistPercent), resistPercent, "kháng phải trong [0, 100]");

            double max = 0;
            foreach (Source s in _sources.Values)
                if (s.Percent > max) max = s.Percent;      // ① MAX, không cộng dồn

            double capped = Math.Min(max, _capPercent);     // ② cap TRƯỚC
            return capped * (1 - resistPercent / 100.0);    // ③ kháng SAU
        }

        /// <summary>Hệ số nhân tốc độ. 1.0 = đi bình thường, 0.3 = còn 30%.</summary>
        public double SpeedMultiplier(double resistPercent) =>
            1 - EffectiveSlowPercent(resistPercent) / 100.0;
    }
}
