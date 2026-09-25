using System;
using System.Collections.Generic;
using System.Linq;
using LaMuralla.Core.Json;

namespace LaMuralla.Core.Config
{
    /// <summary>
    /// Các luật validate config của docs/05 §5.
    ///
    /// Chạy lúc BAKE trong Editor → config hỏng làm vỡ BUILD, không vỡ app người
    /// chơi. Đây là chỗ docs/05 nói "Throw, không vào được Menu" — bake mạnh hơn:
    /// lỗi không bao giờ ra khỏi máy build.
    ///
    /// Gom TẤT CẢ lỗi rồi mới ném một lần. Ném ở lỗi đầu tiên nghĩa là sửa xong
    /// một cái lại phải chạy lại để thấy cái tiếp theo.
    /// </summary>
    public static class ConfigValidator
    {
        public static void Validate(GameConfig c)
        {
            var errs = new List<string>();

            Rule01_IdsUnique(c, errs);
            Rule02_ThreeLevels(c, errs);
            Rule03_CostFormula(c, errs);
            Rule04_TwentyWaves(c, errs);
            Rule05_SpawnIdsExist(c, errs);
            Rule06_SlowCap(c, errs);
            Rule07_OneGoalkeeper(c, errs);
            Rule08_ThreeAbilities(c, errs);
            Rule09_NoByLevelParams(c, errs);
            Rule10_HpOutpacesBounty(c, errs);
            Rule11_BossAppearancesMatch(c, errs);
            Rule12_BossSlowResistRange(c, errs);
            Rule13_BossLeakNotInstantLoss(c, errs);
            Rule14_SlowPercentUnderCap(c, errs);
            Rule15_SlowNeedsScope(c, errs);
            Rule16_ZeroDamageMeansZeroRate(c, errs);
            Rule17_TargetSelectorKnown(c, errs);
            Rule18_AbilitiesNotCumulative(c, errs);
            Rule19_NoAbilityShadowsLevelStat(c, errs);
            Rule20_KillChanceSane(c, errs);
            Rule22_LaneDelayConsistent(c, errs);
            Rule23_HpMilestonesSane(c, errs);
            Rule24_RewardedRecoverySane(c, errs);

            if (errs.Count > 0)
                throw new ConfigException(
                    $"Config không hợp lệ ({errs.Count} lỗi):\n  - " + string.Join("\n  - ", errs));
        }

        // 1 — id trùng giữa tướng/quân/boss = ScriptableObject ghi đè nhau im lặng
        private static void Rule01_IdsUnique(GameConfig c, List<string> e)
        {
            var seen = new Dictionary<string, string>();
            void Check(string id, string kind)
            {
                if (seen.TryGetValue(id, out string? prev))
                    e.Add($"[1] id `{id}` trùng: {prev} và {kind}");
                else seen[id] = kind;
            }
            foreach (TowerDef t in c.Towers) Check(t.Id, "tower");
            foreach (EnemyDef x in c.Enemies) Check(x.Id, "enemy");
            foreach (BossDef b in c.Bosses) Check(b.Id, "boss");
        }

        // 2
        private static void Rule02_ThreeLevels(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
            {
                if (t.Levels.Count != 3)
                {
                    e.Add($"[2] tướng `{t.Id}` có {t.Levels.Count} cấp, cần đúng 3");
                    continue;
                }
                for (int i = 0; i < 3; i++)
                    if (t.Levels[i].Level != i + 1)
                        e.Add($"[2] tướng `{t.Id}` cấp thứ {i} khai level={t.Levels[i].Level}, cần {i + 1}");
            }
        }

        // 3 — công thức giá 0.8× / 1.6×. Áp cho CẢ tướng 0 DPS (docs/05 §4)
        private static void Rule03_CostFormula(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
            {
                if (t.Levels.Count != 3) continue;   // luật 2 đã báo
                double b = t.Levels[0].Cost;
                if (b <= 0) { e.Add($"[3] tướng `{t.Id}` giá mua {b} phải > 0"); continue; }
                CheckRatio(t.Id, "Lv2", t.Levels[1].Cost / b, 0.8, e);
                CheckRatio(t.Id, "Lv3", t.Levels[2].Cost / b, 1.6, e);
            }
        }

        private static void CheckRatio(string id, string lvl, double got, double want, List<string> e)
        {
            if (Math.Abs(got - want) > 0.01)
                e.Add($"[3] tướng `{id}` {lvl}: giá/giá_mua = {got:0.###}, cần {want} (±0.01)");
        }

        // 4
        private static void Rule04_TwentyWaves(GameConfig c, List<string> e)
        {
            if (c.Waves.Count != 20)
                e.Add($"[4] có {c.Waves.Count} wave, cần đúng 20");
            for (int i = 0; i < c.Waves.Count; i++)
                if (c.Waves[i].Wave != i + 1)
                    e.Add($"[4] wave thứ {i} khai wave={c.Waves[i].Wave}, cần {i + 1} (phải liên tục 1→20)");
        }

        // 5 — id quân VÀ id tuyến phải có thật
        private static void Rule05_SpawnIdsExist(GameConfig c, List<string> e)
        {
            var ids = new HashSet<string>(c.Enemies.Select(x => x.Id));
            var lanes = new HashSet<string>(c.Path.Lanes.Select(x => x.Id));

            foreach (WaveDef w in c.Waves)
            {
                foreach (SpawnGroup g in w.Spawns)
                {
                    if (!ids.Contains(g.Enemy))
                        e.Add($"[5] wave {w.Wave} spawn `{g.Enemy}` không có trong enemies.json");
                    // Tuyến sai chính tả = quân ra ở một con đường không tồn tại. Không
                    // có luật này thì nó nổ lúc CHẠY, giữa trận, trên máy người chơi.
                    if (!lanes.Contains(g.Lane))
                        e.Add($"[5] wave {w.Wave}: tuyến `{g.Lane}` không có trong lanes[] "
                              + $"(có: {string.Join(", ", lanes)})");
                }
                foreach (BossSpawn bs in w.Bosses)
                    if (!lanes.Contains(bs.Lane))
                        e.Add($"[5] wave {w.Wave}: boss `{bs.Id}` ra tuyến `{bs.Lane}` không có trong lanes[]");
            }
        }

        /// <summary>
        /// 22 — mọi nhóm CÙNG MỘT TUYẾN trong một wave phải khai cùng `delaySec`.
        ///
        /// `delaySec` là độ trễ của TUYẾN, không phải của nhóm: trong một tuyến quân
        /// vẫn ra XEN KẼ ĐỀU (mô hình cân bằng coi wave là một dòng đều), nên hai nhóm
        /// cùng tuyến khai hai độ trễ khác nhau là mâu thuẫn — `WaveSpawner` lấy của
        /// nhóm ĐẦU và bỏ qua nhóm kia, IM LẶNG. Đúng kiểu bug repo này đã dính nhiều
        /// lần: config nói một đằng, engine làm một nẻo.
        /// </summary>
        private static void Rule22_LaneDelayConsistent(GameConfig c, List<string> e)
        {
            foreach (WaveDef w in c.Waves)
            {
                var seen = new Dictionary<string, double>();
                foreach (SpawnGroup g in w.Spawns)
                {
                    if (g.Count <= 0) continue;
                    if (!seen.TryGetValue(g.Lane, out double d)) { seen[g.Lane] = g.DelaySec; continue; }
                    if (Math.Abs(d - g.DelaySec) > 1e-9)
                        e.Add($"[22] wave {w.Wave} tuyến `{g.Lane}`: hai nhóm khai delaySec khác nhau "
                              + $"({d} và {g.DelaySec}) — delaySec là của TUYẾN, phải giống nhau");
                }
            }
        }

        // 6 — xem docs/01 §8 FM-08
        private static void Rule06_SlowCap(GameConfig c, List<string> e)
        {
            if (c.Economy.SlowCapPercent > 70)
                e.Add($"[6] slowCapPercent = {c.Economy.SlowCapPercent}, phải ≤ 70");
        }

        // 7
        private static void Rule07_OneGoalkeeper(GameConfig c, List<string> e)
        {
            int n = c.Towers.Count(t => t.IsGoalkeeper);
            if (n != 1) e.Add($"[7] có {n} tướng slotType=goalkeeper, cần đúng 1");
        }

        // 8 — luật cộng dồn docs/02 §3.2: mỗi cấp mở đúng 1 kỹ năng mới
        private static void Rule08_ThreeAbilities(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
            {
                if (t.Abilities.Count != 3)
                {
                    e.Add($"[8] tướng `{t.Id}` có {t.Abilities.Count} kỹ năng, cần đúng 3");
                    continue;
                }
                var levels = t.Abilities.Select(a => a.UnlockLevel).OrderBy(x => x).ToList();
                if (!levels.SequenceEqual(new[] { 1, 2, 3 }))
                    e.Add($"[8] tướng `{t.Id}` unlockLevel = [{string.Join(",", levels)}], cần mỗi cấp một cái (1,2,3)");
            }
        }

        // 9 — kỹ năng cũ KHÔNG được tự mạnh lên theo cấp (docs/02 §3.2 luật 2).
        // Nếu vừa cộng dồn kỹ năng vừa scale kỹ năng cũ thì Lv3 mạnh gấp 5–6× Lv1
        // trong khi chỉ tốn 3.4× tiền → không ai xây gì khác ngoài Lv3.
        private static void Rule09_NoByLevelParams(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
                foreach (AbilityDef a in t.Abilities)
                    foreach (string k in a.Params.Keys)
                        if (k.EndsWith("ByLevel", StringComparison.Ordinal))
                            e.Add($"[9] `{t.Id}.{a.Id}.params.{k}`: hậu tố `ByLevel` bị cấm — kỹ năng cũ không được scale theo cấp");
        }

        // 10 — VIẾT LẠI Ở VÒNG 5b. Luật gốc là "hpMultiplier >= bountyMultiplier ở
        // mọi act", nhưng hpMultiplier theo act ĐÃ BỊ XOÁ (nó tạo vách đứng ở W8/W15
        // — xem docs/03 §3). Ý ĐỒ giữ nguyên: độ khó phải vượt sức mua, nếu không
        // thì độ khó thực tế phẳng. Đo ở CUỐI mỗi act, nơi sức mua của act đó đạt đỉnh.
        private static void Rule10_HpOutpacesBounty(GameConfig c, List<string> e)
        {
            foreach (ActDef a in c.Acts)
            {
                double hp = c.HpScaling.MultiplierAt(a.LastWave);
                if (hp < a.BountyMultiplier)
                    e.Add($"[10] act {a.Id}: cuối act (W{a.LastWave}) máu ×{hp:0.##} < thưởng ×{a.BountyMultiplier} " +
                          $"→ sức mua đuổi kịp độ khó, độ khó thực tế phẳng");
            }
        }

        // 11
        private static void Rule11_BossAppearancesMatch(GameConfig c, List<string> e)
        {
            var byId = c.Bosses.ToDictionary(b => b.Id);
            foreach (WaveDef w in c.Waves)
            {
                foreach (BossSpawn spawn in w.Bosses)
                {
                    if (!byId.TryGetValue(spawn.Id, out BossDef? b))
                    {
                        e.Add($"[11] wave {w.Wave} gọi boss `{spawn.Id}` không có trong bosses[]");
                        continue;
                    }
                    if (b.Appearances.All(x => x.Wave != w.Wave))
                        e.Add($"[11] boss `{b.Id}` không có appearance cho wave {w.Wave}");
                }
            }
            foreach (BossDef b in c.Bosses)
                foreach (BossAppearance ap in b.Appearances)
                    if (c.Waves.All(w => w.Wave != ap.Wave || w.Bosses.All(x => x.Id != b.Id)))
                        e.Add($"[11] boss `{b.Id}` khai appearance ở wave {ap.Wave} nhưng wave đó không gọi nó");
        }

        private static void Rule23_HpMilestonesSane(GameConfig c, List<string> e)
        {
            if (c.HpScaling.Base <= 0 || c.HpScaling.GrowthPerWave <= 0)
                e.Add("[23] hpScaling base và growthPerWave phải > 0");
            if (c.HpScaling.Milestones.Count == 0)
                e.Add("[23] hpScaling.milestones không được rỗng");

            int previousWave = 0;
            double previousMultiplier = 1;
            foreach (HpMilestone milestone in c.HpScaling.Milestones)
            {
                if (milestone.Wave <= previousWave || milestone.Wave > c.Waves.Count)
                    e.Add($"[23] mốc HP wave {milestone.Wave} phải tăng dần và nằm trong trận");
                if (milestone.Multiplier < previousMultiplier)
                    e.Add($"[23] hệ số HP W{milestone.Wave} = {milestone.Multiplier} làm độ khó giảm");
                previousWave = milestone.Wave;
                previousMultiplier = milestone.Multiplier;
            }
        }

        private static void Rule24_RewardedRecoverySane(GameConfig c, List<string> e)
        {
            EconomyDef x = c.Economy;
            if (x.RewardedHealAmount <= 0 || x.RewardedHealUsesPerMatch <= 0)
                e.Add("[24] rewarded heal amount/uses phải > 0");
            if (x.RewardedContinueHealth <= 0 || x.RewardedContinueHealth > x.GoalHealth ||
                x.RewardedContinueUsesPerMatch <= 0)
                e.Add("[24] rewarded continue health/uses không hợp lệ");
        }

        // 12
        private static void Rule12_BossSlowResistRange(GameConfig c, List<string> e)
        {
            foreach (BossDef b in c.Bosses)
                if (b.SlowResistPercent < 0 || b.SlowResistPercent > 100)
                    e.Add($"[12] boss `{b.Id}` slowResistPercent = {b.SlowResistPercent}, phải trong [0, 100]");
        }

        // 13 — một con boss lọt không được thắng ngay từ máu đầy
        private static void Rule13_BossLeakNotInstantLoss(GameConfig c, List<string> e)
        {
            foreach (BossDef b in c.Bosses)
                if (b.LeakDamage >= c.Economy.GoalHealth)
                    e.Add($"[13] boss `{b.Id}` leakDamage {b.LeakDamage} >= goalHealth {c.Economy.GoalHealth} " +
                          $"→ một con lọt là thua ngay dù máu đầy");
        }

        // 14 — docs/01 §8 FM-08
        private static void Rule14_SlowPercentUnderCap(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
                foreach (AbilityDef a in t.Abilities)
                    if (a.Params.TryGetValue("slowPercent", out JsonValue? v) &&
                        v.AsNumber() > c.Economy.SlowCapPercent)
                        e.Add($"[14] `{t.Id}.{a.Id}` slowPercent = {v.AsNumber()} > cap {c.Economy.SlowCapPercent}");
        }

        // 15 — mặc định ngầm ở đây là bug im lặng: "chậm 50% khi trong tầm", "chậm
        // 50% trong 3 giây" và "chậm 50% vĩnh viễn" là ba cơ chế khác hẳn nhau về
        // sức mạnh, mà nhìn param `slowPercent: 50` thì giống hệt.
        //
        // docs/05 §5 (viết vòng 4) chỉ liệt kê `in_range | permanent`. Validator
        // chạy lên config THẬT ở vòng 6 phát hiện scope thứ ba `timed`
        // (d10s.ban_tay_cua_chua: 50% trong 3s). Docs thiếu, config đúng.
        private static readonly string[] SlowScopes = { "in_range", "timed", "permanent" };

        private static void Rule15_SlowNeedsScope(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
                foreach (AbilityDef a in t.Abilities)
                {
                    if (!a.Params.ContainsKey("slowPercent")) continue;
                    if (!a.Params.TryGetValue("scope", out JsonValue? s))
                    {
                        e.Add($"[15] `{t.Id}.{a.Id}` có slowPercent nhưng thiếu `scope`");
                        continue;
                    }
                    string scope = s.AsString();
                    if (Array.IndexOf(SlowScopes, scope) < 0)
                    {
                        e.Add($"[15] `{t.Id}.{a.Id}` scope = `{scope}`, chỉ nhận: {string.Join(" | ", SlowScopes)}");
                        continue;
                    }

                    // `timed` mà không khai thời lượng thì thời lượng thành mặc định
                    // ngầm — đúng cái bug im lặng mà luật này sinh ra để chặn.
                    bool hasDuration = a.Params.TryGetValue("durationSec", out JsonValue? d);
                    if (scope == "timed" && (!hasDuration || d!.AsNumber() <= 0))
                        e.Add($"[15] `{t.Id}.{a.Id}` scope=timed nhưng thiếu `durationSec` > 0");

                    // Ngược lại: khai durationSec cho scope không dùng nó thì một
                    // trong hai trường đang nói dối, và code sẽ âm thầm bỏ qua một cái.
                    if (scope != "timed" && hasDuration)
                        e.Add($"[15] `{t.Id}.{a.Id}` scope={scope} nhưng vẫn khai `durationSec` — " +
                              $"trường này chỉ có nghĩa với scope=timed, để lại là gây hiểu nhầm");
                }
        }

        // 16 — bắt lỗi copy-paste: tướng 0 sát thương mà vẫn khai tốc đánh.
        //
        // NGOẠI LỆ (vòng 18): tướng giết bằng XÁC SUẤT, không bằng sát thương. Dibu
        // khai `damage: 0` + `attackRate: 0.5` + `killChancePercent` — đó là hợp lệ:
        // nó vẫn phải có nhịp để biết bao lâu tung xúc xắc một lần.
        //
        // Nới cho ĐÚNG ca đó, không tắt luật: El Árbitro (damage 0, attackRate 0,
        // không có killChance) vẫn bị luật này canh. Tắt hẳn thì lỗi copy-paste thật
        // — thứ luật này sinh ra để bắt — sẽ trôi qua.
        private static void Rule16_ZeroDamageMeansZeroRate(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
            {
                if (t.Levels.Count == 0 || t.Levels.Any(l => l.Damage != 0)) continue;
                bool killsByChance = t.Abilities.Any(a => a.Params.ContainsKey("killChancePercent"));
                if (killsByChance) continue;

                foreach (TowerLevel l in t.Levels)
                    if (l.AttackRate != 0)
                        e.Add($"[16] tướng `{t.Id}` Lv{l.Level}: damage = 0 nhưng attackRate = {l.AttackRate}, " +
                              $"phải 0 — trừ khi tướng giết bằng `killChancePercent`");
            }
        }

        /// <summary>Tướng giết bằng xác suất PHẢI có nhịp đánh, và % phải trong
        /// (0, 100]. `killChancePercent: 0` là tướng không bao giờ làm gì; `> 100`
        /// là người viết config nhầm đơn vị (0.2 vs 20).</summary>
        private static void Rule20_KillChanceSane(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
                foreach (AbilityDef a in t.Abilities)
                {
                    if (!a.Params.TryGetValue("killChancePercent", out JsonValue? v)) continue;
                    double pct = v.AsNumber();
                    if (pct <= 0 || pct > 100)
                        e.Add($"[20] `{t.Id}.{a.Id}` killChancePercent = {pct}, phải trong (0, 100]");

                    TowerLevel lv = t.Levels[a.UnlockLevel - 1];
                    if (lv.AttackRate <= 0)
                        e.Add($"[20] `{t.Id}` Lv{a.UnlockLevel} có killChancePercent nhưng attackRate = 0 " +
                              $"→ không bao giờ tung xúc xắc, tướng đứng im vĩnh viễn");
                }
        }

        /// <summary>Mọi `targetSelector` mà TargetingSystem cài đặt được. Gõ sai
        /// một chữ ở config thì tướng im lặng không bắn ai — không crash, không log,
        /// chỉ là một tướng vô dụng mà không ai biết vì sao.</summary>
        private static readonly string[] TargetSelectors =
        {
            "first_in_range", "highest_absolute_hp", "first_in_range_without_card",
        };

        // 17 — targetSelector phải là thứ code hiểu được.
        //
        // Luật này sinh ra từ B-01: `the_do` từng dùng `first_in_range_with_yellow_card`,
        // giá trị giờ không còn tồn tại. Không có luật này thì đổi config kiểu đó
        // trôi qua 16 luật cũ mà không ai chặn — đúng như nó ĐÃ trôi qua.
        private static void Rule17_TargetSelectorKnown(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
                foreach (AbilityDef a in t.Abilities)
                {
                    if (!a.Params.TryGetValue("targetSelector", out JsonValue? v)) continue;
                    string sel = v.AsString();
                    if (Array.IndexOf(TargetSelectors, sel) < 0)
                        e.Add($"[17] `{t.Id}.{a.Id}` targetSelector = `{sel}`, chỉ nhận: " +
                              string.Join(" | ", TargetSelectors));
                }
        }

        /// <summary>Tham số nào cho thấy một kỹ năng đang TRỎ tới kỹ năng cấp khác
        /// thay vì tự đứng được. Xem luật 18.</summary>
        private static readonly string[] CrossLevelParams =
        {
            "overridesTatCanhN",        // El Fideo Lv3 từng sửa tat_canh (Lv1)
            "onlyWhileOwnSlowActive",   // D10S Lv3 từng cần slow của ban_tay_cua_chua (Lv1)
        };

        // 18 — 🔴 B-01: KỸ NĂNG KHÔNG TÍCH LUỸ (user chốt 2026-07-17)
        //
        // Tướng cấp N chạy ĐÚNG MỘT kỹ năng: cái có unlockLevel == N. Nên mỗi kỹ
        // năng phải TỰ ĐỨNG ĐƯỢC — không được trỏ tới hiệu ứng của cấp khác.
        //
        // Trước B-01, 4/6 tướng có Lv3 phụ thuộc cấp dưới. Chúng trôi qua 16 luật
        // cũ vì không luật nào hỏi câu này, và hậu quả chỉ lộ ra khi chơi thật:
        // El Árbitro Lv3 đứng im vĩnh viễn vì không ai có thẻ vàng để nó rút thẻ đỏ.
        private static void Rule18_AbilitiesNotCumulative(GameConfig c, List<string> e)
        {
            if (c.AbilitiesAreCumulative)
                e.Add("[18] `_upgradeRules.abilitiesAreCumulative` = true — B-01 chốt là false. " +
                      "Nếu muốn quay lại tích luỹ thì phải mở lại B-01 với user, " +
                      "không phải lật cờ ở đây.");

            foreach (TowerDef t in c.Towers)
            {
                foreach (AbilityDef a in t.Abilities)
                    foreach (string bad in CrossLevelParams)
                        if (a.Params.ContainsKey(bad))
                            e.Add($"[18] `{t.Id}.{a.Id}` có `{bad}` — tham số này trỏ tới kỹ năng " +
                                  $"cấp khác. Theo B-01 kỹ năng cấp thấp KHÔNG chạy, nên điều kiện " +
                                  $"này không bao giờ đúng → kỹ năng chết. Cho nó tự chứa thông số.");

                // Thủ môn là ngoại lệ CÓ TÊN: cản bóng là định nghĩa của vị trí,
                // không phải kỹ năng chọn thêm. Xem `_b01Exception` ở dibu.
                // Ngoại lệ này KHÔNG mở cho tướng sân.
                if (t.SlotType == "goalkeeper") continue;

                var seen = new HashSet<int>();
                foreach (AbilityDef a in t.Abilities)
                    if (!seen.Add(a.UnlockLevel))
                        e.Add($"[18] tướng `{t.Id}` có 2 kỹ năng cùng unlockLevel {a.UnlockLevel} — " +
                              $"cấp N phải có ĐÚNG MỘT kỹ năng, nếu không thì 'chạy kỹ năng cao nhất' " +
                              $"là câu mơ hồ.");
            }
        }

        /// <summary>Tham số kỹ năng ↔ trường ở `levels` mà nó có thể trùng vai.
        /// Xem luật 19.</summary>
        private static readonly (string Param, string LevelField)[] ShadowedStats =
        {
            ("splashRadiusBonus", "splashRadius"),
            ("rangeBonus", "range"),
            ("damageBonus", "damage"),
            ("attackRateBonus", "attackRate"),
        };

        // 19 — kỹ năng KHÔNG được mang tham số trùng vai với trường ở `levels`.
        //
        // Sinh ra từ một quả mìn tôi tự đặt ở vòng 10: `d10s.cu_cham_thien_tai` khai
        // `splashRadiusBonus: 0.5`, trong khi `levels[1].splashRadius` đã là 1.7 —
        // tức 1.2 + 0.5, bonus ĐÃ cộng sẵn. Code đọc cả hai ra 2.2. Không crash,
        // không log: D10S Lv2 chỉ lặng lẽ mạnh hơn thiết kế 29%, và bảng cân bằng
        // 20/20 wave nói về một trận đấu không tồn tại.
        //
        // `levels[]` là NGUỒN CHÂN LÝ của chỉ số. Kỹ năng đặt TÊN và giải thích cho
        // chênh lệch giữa các cấp; nó không được cộng thêm lần nữa.
        private static void Rule19_NoAbilityShadowsLevelStat(GameConfig c, List<string> e)
        {
            foreach (TowerDef t in c.Towers)
                foreach (AbilityDef a in t.Abilities)
                    foreach ((string param, string field) in ShadowedStats)
                        if (a.Params.ContainsKey(param))
                            e.Add($"[19] `{t.Id}.{a.Id}` có `{param}` — trùng vai với `levels[].{field}`, " +
                                  $"thứ đã ghi giá trị CUỐI CÙNG cho từng cấp. Đọc cả hai = cộng hai lần. " +
                                  $"Gỡ tham số này; để `levels[]` nói, kỹ năng chỉ đặt tên.");
        }
    }
}
