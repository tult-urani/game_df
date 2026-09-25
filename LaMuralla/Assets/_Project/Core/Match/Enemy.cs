using System;

namespace LaMuralla.Core.Match
{
    /// <summary>
    /// Một cổ động viên trên sân.
    ///
    /// 🔴 KHÔNG GIỮ MÁU. `Hp` hỏi `DamageSystem` — đó là luật bất khả xâm phạm #2:
    /// nếu quân giữ máu thì ai cũng `enemy.Hp -= dmg` được, và ai cũng tự kết luận
    /// "tôi giết nó" → FM-07, cộng tiền hai lần, kinh tế vỡ sau ~10 wave.
    ///
    /// Nhìn hơi ngược đời (một "con quân" không biết máu mình), nhưng đó chính là
    /// điểm: nó KHÔNG ĐƯỢC biết, vì biết thì sẽ có người sửa.
    /// </summary>
    public sealed class Enemy : ITarget
    {
        private readonly DamageSystem _damage;

        public int Id { get; }
        public string DefId { get; }
        public bool IsBoss { get; }

        /// <summary>Máu tối đa Ở WAVE NÀY — đã nhân `hpScaling`. Cần cho
        /// `batigol.ban_nang_sat_thu` ("+30% khi máu > 80%" là % của máu tối đa).</summary>
        public double MaxHp { get; }

        /// <summary>Tốc độ gốc, chưa trừ chậm. `enemies.json → speed`.</summary>
        public double BaseSpeed { get; }

        /// <summary>`enemies.json → slowResistPercent`. Boss = 75.</summary>
        public double SlowResistPercent { get; }

        /// <summary>Trừ bao nhiêu máu cầu môn khi lọt lưới.</summary>
        public int LeakDamage { get; }

        public double DistanceTravelled { get; internal set; }
        public Vec2 Position { get; internal set; }
        public CardState Card { get; internal set; }

        /// <summary>Mọi nguồn làm chậm đang tác động lên con này.</summary>
        public SlowStack Slows { get; }

        /// <summary>Tuyến con này chạy. BẤT BIẾN — đặt lúc spawn, không đổi giữa đường.
        /// Cho phép đổi tuyến giữa chừng là mở cửa cho cả một lớp bug vị trí.</summary>
        public string LaneId { get; }

        public Enemy(int id, string defId, double maxHp, double baseSpeed, double slowResistPercent,
                     int leakDamage, bool isBoss, double slowCapPercent, DamageSystem damage,
                     string laneId = "L1")
        {
            if (maxHp <= 0) throw new ArgumentOutOfRangeException(nameof(maxHp));
            LaneId = laneId;
            Id = id; DefId = defId; MaxHp = maxHp; BaseSpeed = baseSpeed;
            SlowResistPercent = slowResistPercent; LeakDamage = leakDamage; IsBoss = isBoss;
            Slows = new SlowStack(slowCapPercent);
            _damage = damage ?? throw new ArgumentNullException(nameof(damage));
        }

        /// <summary>Máu hiện tại — HỎI DamageSystem, không tự giữ.</summary>
        public double Hp => _damage.HpOf(Id);

        public bool IsAlive => _damage.IsAlive(Id);

        /// <summary>Tốc độ thực sau khi trừ chậm (đã qua cap 70% rồi kháng — thứ tự
        /// đó là luật, xem SlowStack).</summary>
        public double CurrentSpeed => BaseSpeed * Slows.SpeedMultiplier(SlowResistPercent);

        public override string ToString() =>
            $"#{Id} {DefId}{(IsBoss ? " BOSS" : "")} {Hp:0}/{MaxHp:0} @{DistanceTravelled:0.0}";
    }
}
