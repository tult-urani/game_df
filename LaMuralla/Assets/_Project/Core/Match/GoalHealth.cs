using System;

namespace LaMuralla.Core.Match
{
    /// <summary>Xếp hạng cuối trận. `01` §3.</summary>
    public enum StarRating
    {
        None = 0,
        /// <summary>Thắng, máu còn 1–9.</summary>
        One = 1,
        /// <summary>Thắng, máu còn 10–19.</summary>
        Two = 2,
        /// <summary>Thắng, máu còn ĐÚNG 20 — clean sheet, không thủng lưới lần nào.</summary>
        Three = 3,
    }

    public readonly struct GoalDamagedInfo
    {
        public int Amount { get; }
        public int HealthAfter { get; }
        public string EnemyId { get; }
        public GoalDamagedInfo(int amount, int healthAfter, string enemyId)
        {
            Amount = amount; HealthAfter = healthAfter; EnemyId = enemyId;
        }
    }

    /// <summary>
    /// Máu cầu môn = số bàn thua chịu được. `01` §3, `05` §3.
    ///
    /// Cột "không làm" ở docs/05 bảng §3: **"Biết về Dibu"**. Lớp này không biết
    /// thủ môn tồn tại. Dibu `can_pha` chặn quân TRƯỚC khi tới đây (qua
    /// DamageSystem.RemoveWithoutKill), còn `nguoi_hung_luan_luu` hồi máu thì gọi
    /// <see cref="Heal"/> từ ngoài. Nếu lớp này biết về Dibu thì luật "cản" và luật
    /// "máu" dính vào nhau, và sau này đổi một cái là hỏng cái kia.
    /// </summary>
    public sealed class GoalHealth
    {
        private readonly int _max;

        public int Current { get; private set; }
        public int Max => _max;
        public bool IsLost => Current <= 0;

        /// <summary>Chưa thủng lưới lần nào. Điều kiện 3 sao — và nó KHÔNG bằng
        /// `Current == Max`: mua "Sửa cầu môn" hoặc Dibu Lv3 hồi máu có thể đưa máu
        /// về 20 sau khi đã thủng. Clean sheet là "chưa bao giờ thủng", không phải
        /// "hiện đang đầy máu".</summary>
        public bool CleanSheet { get; private set; } = true;

        public event Action<GoalDamagedInfo>? GoalDamaged;
        public event Action? GoalLost;

        public GoalHealth(int maxHealth)
        {
            if (maxHealth <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "máu cầu môn phải > 0");
            _max = maxHealth;
            Current = maxHealth;
        }

        /// <summary>
        /// Quân lọt lưới. Trả về true nếu cú này làm thua.
        ///
        /// 🔴 KHÔNG có luật "trừ tối đa còn 1" — `01` §8 **FM-09**: boss `O Capitão`
        /// trừ 5, lọt lưới khi máu ≤ 5 là THUA. Cài "để lại 1 máu cho người chơi
        /// một cơ hội nữa" nghe nhân đạo nhưng phá điều kiện thua, và làm boss
        /// thành vô hại ở đúng lúc nó đáng sợ nhất.
        /// </summary>
        public bool Leak(int damage, string enemyId)
        {
            if (damage <= 0)
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "sát thương lọt lưới phải > 0");
            if (IsLost) return true;   // đã thua rồi, không trừ tiếp

            CleanSheet = false;
            Current = Math.Max(0, Current - damage);
            GoalDamaged?.Invoke(new GoalDamagedInfo(damage, Current, enemyId));

            if (Current > 0) return false;
            GoalLost?.Invoke();
            return true;
        }

        /// <summary>Hồi máu — sink "Sửa cầu môn" (`04` §1) hoặc Dibu Lv3
        /// `nguoi_hung_luan_luu`. Không vượt quá máu tối đa, và KHÔNG xoá
        /// <see cref="CleanSheet"/> đã mất.</summary>
        public void Heal(int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "hồi máu phải > 0");
            if (IsLost)
                throw new InvalidOperationException(
                    "cầu môn đã vỡ — hồi máu sau khi thua là hồi sinh, không phải sửa chữa");
            Current = Math.Min(_max, Current + amount);
        }

        /// <summary>Xếp hạng khi THẮNG. Gọi khi đã clear đủ 20 wave.</summary>
        public StarRating RatingOnWin()
        {
            if (IsLost) return StarRating.None;
            if (CleanSheet) return StarRating.Three;
            return Current >= 10 ? StarRating.Two : StarRating.One;
        }
    }
}
