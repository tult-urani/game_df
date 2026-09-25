using LaMuralla.Core.Config;

namespace LaMuralla.Core.Match
{
    /// <summary>Đòn đánh chạm theo hình gì.</summary>
    public enum AttackShape
    {
        /// <summary>Chỉ mục tiêu chính.</summary>
        Single,
        /// <summary>Lan quanh VỊ TRÍ MỤC TIÊU (`05` §4.5 mục 2).</summary>
        Splash,
        /// <summary>Tia thẳng chốt lúc bắn — `batigol.sut_xuyen`, `la_pulga.so_10`.</summary>
        Line,
    }

    /// <summary>
    /// Một phát bắn cụ thể: bắn ai, bao nhiêu sát thương, chạm theo hình gì.
    ///
    /// Là DỮ LIỆU, không phải hành động. `AbilityEngine` sinh ra nó; người gọi
    /// đem cho `CombatResolver` hỏi trúng ai, rồi cho `DamageSystem` trừ máu.
    /// Tách ba bước để test được từng bước — "tính sát thương đúng chưa" trả lời
    /// được mà không cần dựng cả trận đấu.
    /// </summary>
    public readonly struct AttackPlan
    {
        public int TargetId { get; }
        public Vec2 TargetPos { get; }

        /// <summary>Sát thương lên MỘT con, đã nhân mọi hệ số của kỹ năng.</summary>
        public double Damage { get; }

        public AttackShape Shape { get; }

        /// <summary>Chỉ có nghĩa với <see cref="AttackShape.Splash"/>. Lấy từ
        /// `levels[].splashRadius` — 🔴 kỹ năng KHÔNG cộng thêm (luật 19).</summary>
        public double SplashRadius { get; }

        /// <summary>Chỉ có nghĩa với <see cref="AttackShape.Line"/>.</summary>
        public double LineLength { get; }
        public double LineWidth { get; }

        /// <summary>Tối đa bao nhiêu con phụ ngoài mục tiêu chính. `0` = không giới
        /// hạn (`sut_xuyen` xuyên cả hàng). `tat_canh` = 2, `so_10` = 2.</summary>
        public int MaxExtraTargets { get; }

        /// <summary>Kỹ năng đang tạo ra phát bắn này — để log và đo ở M1.</summary>
        public string? AbilityId { get; }

        public AttackPlan(int targetId, Vec2 targetPos, double damage, AttackShape shape,
                          double splashRadius = 0, double lineLength = 0, double lineWidth = 0,
                          int maxExtraTargets = 0, string? abilityId = null)
        {
            TargetId = targetId; TargetPos = targetPos; Damage = damage; Shape = shape;
            SplashRadius = splashRadius; LineLength = lineLength; LineWidth = lineWidth;
            MaxExtraTargets = maxExtraTargets; AbilityId = abilityId;
        }

        public override string ToString() =>
            $"→#{TargetId} {Damage:0.#} dmg {Shape}" + (AbilityId != null ? $" ({AbilityId})" : "");
    }

    /// <summary>Thứ AbilityEngine cần biết về thế giới để tính một phát bắn — không hơn.</summary>
    public interface IAbilityContext
    {
        /// <summary>% chậm hiệu dụng đang chịu trên một con. 0 = không bị chậm.
        /// Dùng cho `nhan_quan` (+25% vs bị chậm) và `ban_thang_the_ky` (+20% toàn đội).</summary>
        double SlowPercentOn(int enemyId);

        /// <summary>Có tướng D10S Lv3 nào đang trên sân không — `ban_thang_the_ky`
        /// buff MỌI tướng, không riêng chủ nhân nó.</summary>
        bool AnyGlobalSlowedDamageBuff { get; }

        /// <summary>Hệ số buff toàn đội (%). Lấy từ `ban_thang_the_ky.params`.</summary>
        double GlobalSlowedDamageBonusPercent { get; }

        /// <summary>
        /// Trần làm chậm (`economy.json` → `slowCapPercent`, hiện 70).
        ///
        /// Có mặt ở đây vì hai buff "lên mục tiêu bị chậm" được tính TỈ LỆ với mức
        /// chậm thật: `hệ_số = bonus × (slowHiệuDụng / cap)`. Không có cap thì không
        /// biết "chậm nhiều" là bao nhiêu, và ta lại quay về so sánh nhị phân `> 0`.
        /// </summary>
        double SlowCapPercent { get; }
    }
}
