using System;
using LaMuralla.Core.Config;
using LaMuralla.Core.Json;

namespace LaMuralla.Core.Match
{
    /// <summary>
    /// Biến "tướng cấp N bắn con X" thành một <see cref="AttackPlan"/> cụ thể.
    ///
    /// 🔴 B-01: tướng cấp N chạy ĐÚNG MỘT kỹ năng — cái có `unlockLevel == N`.
    /// Kỹ năng cấp thấp KHÔNG chạy. Nên ở đây không có vòng lặp "duyệt mọi kỹ năng":
    /// chỉ có `def.AbilityAt(level)`, một cái, hoặc `null`.
    ///
    /// 🔴 LUẬT 19: `levels[].damage` / `range` / `attackRate` / `splashRadius` là
    /// chỉ số CUỐI CÙNG. Kỹ năng chỉ NHÂN hệ số hoặc đổi HÌNH DẠNG đòn đánh — nó
    /// không cộng thêm vào chỉ số. Cộng thêm là quả mìn ở vòng 10 (D10S Lv2 lan
    /// 1.7 + 0.5 = 2.2, mạnh hơn thiết kế 29%, không crash, không log).
    /// </summary>
    public static class AbilityEngine
    {
        /// <summary>
        /// Phát bắn của tướng ở ô, lên mục tiêu đã chọn.
        /// </summary>
        /// <param name="tower">State chạy — cấp, đếm đòn (cho `every_nth_attack`).</param>
        /// <param name="def">Định nghĩa tướng từ towers.json.</param>
        /// <param name="towerPos">Vị trí ô.</param>
        /// <param name="target">Mục tiêu do TargetingSystem chọn.</param>
        /// <param name="ctx">Trạng thái thế giới cần cho hệ số (ai đang bị chậm...).</param>
        public static AttackPlan Plan(TowerInstance tower, TowerDef def, Vec2 towerPos,
                                      ITarget target, IAbilityContext ctx)
        {
            if (tower == null) throw new ArgumentNullException(nameof(tower));
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            TowerLevel lv = def.Levels[tower.Level - 1];
            AbilityDef? ab = def.AbilityAt(tower.Level);   // B-01: đúng một, hoặc null

            double dmg = lv.Damage;
            AttackShape shape = AttackShape.Single;
            double splashR = 0, lineLen = 0, lineW = 0;
            int extra = 0;

            // ── Hệ số của kỹ năng ĐANG chạy ──────────────────────────────────
            if (ab != null)
            {
                double slowOnTarget = ctx.SlowPercentOn(target.Id);

                switch (ab.Id)
                {
                    // La Pulga Lv1 — ×2 lên mục tiêu máu cao nhất, 4s / mỗi 12s.
                    // Hệ số chỉ áp KHI kỹ năng đang bật (TowerInstance.SoloRunRemaining).
                    case "solo_run":
                        if (tower.AbilityActiveRemaining > 0)
                            dmg *= P(ab, "damageMultiplier", 1.0);
                        break;

                    // La Pulga Lv2 — +25% lên mục tiêu ĐANG bị chậm.
                    case "nhan_quan":
                        dmg *= 1 + P(ab, "bonusDamagePercentVsSlowed", 0) / 100.0
                                   * SlowRatio(slowOnTarget, ctx);
                        break;

                    // La Pulga Lv3 — mỗi đòn thứ n xuyên qua `pierceCount` con phía sau.
                    case "so_10":
                        if (IsNthAttack(tower, ab))
                        {
                            shape = AttackShape.Line;
                            lineLen = lv.Range;
                            lineW = 0.5;   // như sut_xuyen — bóng đi xuyên qua thân người
                            extra = (int)P(ab, "pierceCount", 0);
                        }
                        break;

                    // D10S Lv1 — bản thân đòn đánh vẫn lan theo levels[].splashRadius.
                    // Phần làm chậm do MatchController áp theo cooldown, không phải ở đây.
                    case "ban_tay_cua_chua":
                        shape = AttackShape.Splash;
                        splashR = lv.SplashRadius;
                        break;

                    // D10S Lv2 — 🔴 KHÔNG cộng bán kính ở đây. levels[1].splashRadius
                    // đã là 1.7 (= 1.2 + 0.5). Kỹ năng này giờ chỉ là cái TÊN giải
                    // thích vì sao Lv2 rộng hơn Lv1. Xem luật 19.
                    case "cu_cham_thien_tai":
                        shape = AttackShape.Splash;
                        splashR = lv.SplashRadius;
                        break;

                    // D10S Lv3 — buff TOÀN ĐỘI, không buff riêng mình. Đòn đánh của
                    // chính nó vẫn hưởng buff (nó cũng là "mọi tướng"), nhưng buff
                    // được cộng ở khối chung bên dưới, không ở đây.
                    case "ban_thang_the_ky":
                        shape = AttackShape.Splash;
                        splashR = lv.SplashRadius;
                        break;

                    // El Cinco (batigol) — mỗi cấp một đòn NỔ (đấm/đá/đạp): splash
                    // tại vị trí mục tiêu, bán kính = levels[].splashRadius (cấp cao
                    // nổ to hơn). Dame lấy từ levels[] (60/96/150). Mọi con trong
                    // vùng nổ ăn cùng dame — ApplyPlan lo phần quét vùng.
                    case "cu_dam":
                    case "cu_da":
                    case "cu_dap":
                        shape = AttackShape.Splash;
                        splashR = lv.SplashRadius;
                        break;

                    // El Árbitro & Dibu: 0 sát thương, không bắn. Kỹ năng của chúng
                    // là chậm/cản, do MatchController áp theo cooldown/aura.
                    case "coi_chi_tay":
                    case "the_vang":
                    case "the_do":
                    case "can_pha":
                    case "ap_dao":
                    case "nguoi_hung_luan_luu":
                        break;

                    default:
                        throw new ConfigException(
                            $"kỹ năng `{ab.Id}` chưa được cài trong AbilityEngine. " +
                            "Thêm config mà quên thêm code = tướng im lặng không làm gì.");
                }
            }

            // ── Buff toàn đội của D10S Lv3 ───────────────────────────────────
            // `ban_thang_the_ky` buff MỌI tướng, kể cả chính nó — nên cộng ở đây,
            // sau switch, chứ không trong nhánh của nó.
            if (ctx.AnyGlobalSlowedDamageBuff)
                dmg *= 1 + ctx.GlobalSlowedDamageBonusPercent / 100.0
                           * SlowRatio(ctx.SlowPercentOn(target.Id), ctx);

            return new AttackPlan(target.Id, target.Position, dmg, shape,
                                  splashR, lineLen, lineW, extra, ab?.Id);
        }

        /// <summary>
        /// Mức "bị chậm" quy về [0, 1] để nhân vào buff — 1.0 khi chậm kịch trần.
        ///
        /// 🔴 ĐỔI 2026-08-21. Trước đây hai buff phụ thuộc-chậm (`nhan_quan` +25% và
        /// `ban_thang_the_ky` +20%) kiểm NHỊ PHÂN `slowOnTarget > 0`, nên mọi mức
        /// kháng chậm dưới 100 đều cho cùng một kết quả:
        ///
        ///     kháng  0% → chậm 70.0% → buff ĐẦY ĐỦ
        ///     kháng 75% → chậm 17.5% → buff ĐẦY ĐỦ   ← giống hệt
        ///     kháng 90% → chậm  7.0% → buff ĐẦY ĐỦ   ← giống hệt
        ///     kháng 99% → chậm  0.7% → buff ĐẦY ĐỦ   ← giống hệt
        ///
        /// Tức `slowResistPercent` là một cái VÁCH ở 100, không phải núm xoay —
        /// nâng kháng 75→90 chỉ đổi tốc độ đi của boss, không đổi một điểm sát
        /// thương nào. Điều đó làm hỏng cả cơ chế "boss cuối kháng chậm mạnh" lẫn
        /// việc dùng kháng chậm làm thang độ khó giữa các map.
        ///
        /// Nay buff TỈ LỆ với mức chậm thật. Quân thường (`slowResistPercent = 0`)
        /// bị chậm kịch trần nên tỉ lệ = 1.0 → **hành vi không đổi, cân bằng wave
        /// nguyên vẹn**. Chỉ quân có kháng mới yếu buff đi, đúng bằng mức nó kháng.
        ///
        /// Xem `docs/08-MAPS-ARCHITECTURE.md` §3B.
        /// </summary>
        private static double SlowRatio(double slowOnTarget, IAbilityContext ctx)
        {
            double cap = ctx.SlowCapPercent;
            if (cap <= 0) return 0;                       // cấu hình vô nghĩa → không buff
            double r = slowOnTarget / cap;
            return r < 0 ? 0 : r > 1 ? 1 : r;             // kẹp [0,1]
        }

        /// <summary>
        /// Nhịp bắn hiện tại. Từ khi El Fideo bị gỡ (2026-08-20) KHÔNG kỹ năng nào
        /// còn đổi nhịp bắn — `suc_ben` là cái duy nhất từng làm thế. Hàm vẫn giữ
        /// vì mọi chỗ tính cooldown đều đi qua đây, và vì `AttackRateDebugMultiplier`
        /// phải được áp ở đúng một chỗ.
        /// </summary>
        /// <summary>Hệ số nhân nhịp đánh TOÀN CỤC — CHỈ để DEBUG/xem hoạt ảnh. Mặc
        /// định 1.0 (không đổi gì); core &amp; 224 test luôn chạy ở 1.0. Lớp view
        /// (`MatchView`) đặt >1 lúc chạy để mọi tướng đánh nhanh hơn cho dễ nhìn.
        /// KHÔNG phải cân bằng — không lưu vào config, không rơi vào build thật trừ
        /// khi view chủ động bật.</summary>
        public static double AttackRateDebugMultiplier = 1.0;

        public static double AttackRateOf(TowerInstance tower, TowerDef def)
        {
            TowerLevel lv = def.Levels[tower.Level - 1];
            return lv.AttackRate * AttackRateDebugMultiplier;
        }

        /// <summary>Đòn này có phải đòn thứ n không. `AttacksFired` đếm từ 0, nên
        /// đòn sắp bắn là con thứ `AttacksFired + 1`.</summary>
        private static bool IsNthAttack(TowerInstance tower, AbilityDef ab)
        {
            int n = (int)P(ab, "n", 0);
            return n > 0 && (tower.AttacksFired + 1) % n == 0;
        }

        /// <summary>Đọc tham số số học. Ném nếu thiếu thay vì trả mặc định im lặng —
        /// trừ khi người gọi chủ động khai mặc định.</summary>
        private static double P(AbilityDef ab, string key, double fallback)
        {
            if (!ab.Params.TryGetValue(key, out JsonValue? v)) return fallback;
            return v.AsNumber();
        }
    }
}
