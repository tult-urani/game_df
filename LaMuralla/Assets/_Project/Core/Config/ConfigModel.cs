using System;
using System.Collections.Generic;
using LaMuralla.Core.Json;

// LƯU Ý cho người sửa file này: dùng `{ get; set; }`, KHÔNG dùng `{ get; init; }`.
// `init` cần System.Runtime.CompilerServices.IsExternalInit, thứ KHÔNG có trong
// netstandard2.1 (target bắt buộc của Unity 6). Thêm shim IsExternalInit thì có
// nguy cơ đụng độ định nghĩa của chính Unity — và điều đó không kiểm chứng được
// nếu máy không cài Unity. Bất biến kém hơn một chút, đổi lấy chắc chắn biên dịch.

namespace LaMuralla.Core.Config
{
    /// <summary>Config hỏng. Ném lúc BAKE (Editor) → vỡ build, không vỡ app người chơi.</summary>
    public sealed class ConfigException : Exception
    {
        public ConfigException(string message) : base(message) { }
    }

    public sealed class TowerLevel
    {
        public int Level { get; set; }
        public int Cost { get; set; }
        public double Damage { get; set; }
        public double AttackRate { get; set; }
        public double Range { get; set; }

        /// <summary>
        /// Bán kính lan. Chỉ D10S khai (1.2 / 1.7 / 1.2); tướng khác = 0 = không lan.
        ///
        /// 🔴 ĐÂY LÀ GIÁ TRỊ CUỐI CÙNG, đã gồm mọi bonus. Kỹ năng KHÔNG cộng thêm —
        /// luật 19. `cu_cham_thien_tai` từng khai `splashRadiusBonus: 0.5` trong khi
        /// Lv2 đã là 1.7 = 1.2 + 0.5 → đọc cả hai ra 2.2, mạnh hơn thiết kế 29%,
        /// không crash, không log.
        /// </summary>
        public double SplashRadius { get; set; }

        /// <summary>DPS thô. Không nhân số mục tiêu của D10S — chỗ đó là quyết định
        /// của mô hình chiến đấu, không phải của dữ liệu. Xem docs/04 §2.</summary>
        public double Dps => Damage * AttackRate;
    }

    public sealed class AbilityDef
    {
        public string Id { get; set; } = "";
        public int UnlockLevel { get; set; }
        public string Trigger { get; set; } = "";
        public double CooldownSec { get; set; }
        public IReadOnlyDictionary<string, JsonValue> Params { get; set; } =
            new Dictionary<string, JsonValue>();
    }

    public sealed class TowerDef
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string SlotType { get; set; } = "";
        public IReadOnlyList<TowerLevel> Levels { get; set; } = Array.Empty<TowerLevel>();
        public IReadOnlyList<AbilityDef> Abilities { get; set; } = Array.Empty<AbilityDef>();

        /// <summary>Tối đa bao nhiêu con loại này trên sân. `0` = không giới hạn.
        /// Chỉ Dibu khai `maxInstances: 1` (docs/02 §55: "Dibu là ngoại lệ duy nhất:
        /// tối đa 1 con, chỉ ở ô thủ môn").</summary>
        public int MaxInstances { get; set; }

        public bool IsGoalkeeper => SlotType == "goalkeeper";

        /// <summary>
        /// Kỹ năng chạy ở cấp này — theo B-01 là ĐÚNG MỘT cái, cái có
        /// `unlockLevel == level`. Kỹ năng cấp thấp KHÔNG chạy.
        ///
        /// Trả `null` nếu cấp đó không khai kỹ năng nào. Luật 18 của validator ép
        /// mỗi unlockLevel có đúng một kỹ năng, nên `null` ở đây nghĩa là config đã
        /// lọt qua validator — tức có lỗ hổng ở luật, không phải ở đây.
        /// </summary>
        public AbilityDef? AbilityAt(int level)
        {
            foreach (AbilityDef a in Abilities)
                if (a.UnlockLevel == level) return a;
            return null;
        }
    }

    public sealed class EnemyDef
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int BaseHp { get; set; }
        public double Speed { get; set; }
        public int BaseBounty { get; set; }
        public int LeakDamage { get; set; }
        public int SlowResistPercent { get; set; }
    }

    public sealed class BossAppearance
    {
        public int Wave { get; set; }
        public int Hp { get; set; }
        public int Bounty { get; set; }
    }

    public sealed class BossDef
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public double Speed { get; set; }
        public int LeakDamage { get; set; }
        public int SlowResistPercent { get; set; }
        public IReadOnlyList<BossAppearance> Appearances { get; set; } = Array.Empty<BossAppearance>();
    }

    /// <summary>
    /// Một nhóm quân trong wave: bao nhiêu con, loại gì, ra ở TUYẾN nào.
    ///
    /// Vì sao `spawns` phải là MẢNG chứ không phải object `{adepto: 8, tifoso: 6}`:
    /// object không nhét được `lane`, và cũng không diễn tả nổi "8 adepto tuyến trái
    /// + 6 adepto tuyến phải" vì khoá trùng nhau. Xem docs/08 §2.2(b).
    /// </summary>
    public sealed class SpawnGroup
    {
        public string Enemy { get; set; } = "";
        public int Count { get; set; }

        /// <summary>Id tuyến (`lanes[].id`). Map một tuyến để mặc định `L1`.</summary>
        public string Lane { get; set; } = "L1";

        /// <summary>
        /// Giây trễ trước khi TUYẾN này bắt đầu nhả quân — cơ chế "ba tuyến lệch nhịp".
        ///
        /// Đây là độ trễ của TUYẾN, không phải của riêng nhóm: trong một tuyến các
        /// nhóm vẫn ra XEN KẼ ĐỀU (mô hình cân bằng coi wave là một dòng đều), nên
        /// mọi nhóm cùng tuyến phải khai cùng một `delaySec`. Luật 22 chặn nếu lệch.
        /// </summary>
        public double DelaySec { get; set; }
    }

    /// <summary>Một con boss trong wave: id và tuyến nó đi ra.</summary>
    public sealed class BossSpawn
    {
        public string Id { get; set; } = "";
        public string Lane { get; set; } = "L1";
    }

    public sealed class WaveDef
    {
        public int Wave { get; set; }
        public IReadOnlyList<SpawnGroup> Spawns { get; set; } = Array.Empty<SpawnGroup>();
        /// <summary>
        /// Boss của wave — DANH SÁCH, vì map cuối cho HAI con ra cùng lúc ở hai tuyến.
        /// Rỗng = wave thường. Một dạng dữ liệu duy nhất, không có nhánh "một boss" và
        /// nhánh "nhiều boss" — cùng lập luận với `lanes` và `spawns`.
        /// </summary>
        public IReadOnlyList<BossSpawn> Bosses { get; set; } = Array.Empty<BossSpawn>();

        /// <summary>Boss đầu tiên, hoặc `null`. Lối tắt cho code chỉ cần biết "wave này có boss không".</summary>
        public string? Boss => Bosses.Count > 0 ? Bosses[0].Id : null;

        /// <summary>Tổng số con của MỘT loại trong wave, cộng qua mọi tuyến.</summary>
        public int CountOf(string enemy)
        {
            int n = 0;
            foreach (SpawnGroup g in Spawns)
                if (g.Enemy == enemy) n += g.Count;
            return n;
        }

        public int TotalEnemies
        {
            get
            {
                int n = 0;
                foreach (SpawnGroup g in Spawns) n += g.Count;
                return n;
            }
        }
    }

    public sealed class ActDef
    {
        public int Id { get; set; }
        public int FirstWave { get; set; }
        public int LastWave { get; set; }
        public double BountyMultiplier { get; set; }

        public bool Contains(int wave) => wave >= FirstWave && wave <= LastWave;
    }

    public sealed class HpMilestone
    {
        public int Wave { get; set; }
        public double Multiplier { get; set; }
    }

    /// <summary>
    /// Máu quái theo wave = đường cong trơn × hệ số Hard của mốc gần nhất.
    /// Mốc không cộng dồn: W10 dùng trực tiếp hệ số W10, không nhân thêm W5.
    /// </summary>
    public sealed class HpScaling
    {
        public double Base { get; set; }
        public double GrowthPerWave { get; set; }
        public IReadOnlyList<HpMilestone> Milestones { get; set; } = Array.Empty<HpMilestone>();

        public double MilestoneMultiplierAt(int wave)
        {
            double multiplier = 1;
            foreach (HpMilestone milestone in Milestones)
            {
                if (milestone.Wave > wave) break;
                multiplier = milestone.Multiplier;
            }
            return multiplier;
        }

        public double MultiplierAt(int wave) =>
            Base * Math.Pow(GrowthPerWave, wave - 1) * MilestoneMultiplierAt(wave);
    }

    public sealed class EconomyDef
    {
        public int StartingCash { get; set; }
        public int GoalHealth { get; set; }
        public int SlowCapPercent { get; set; }
        public int SkipBonusPerSecond { get; set; }
        public double SellRefundRatio { get; set; }
        public int FieldSlots { get; set; }
        public int GoalkeeperSlots { get; set; }
        public string RoundingMode { get; set; } = "";
        public int RewardedHealAmount { get; set; }
        public int RewardedHealUsesPerMatch { get; set; }
        public int RewardedContinueHealth { get; set; }
        public int RewardedContinueUsesPerMatch { get; set; }
    }

    /// <summary>Một ô đặt tướng. `Type` là "field" hoặc "goalkeeper" — thủ môn có
    /// thang tầm riêng nên không được lẫn vào ô sân khi đo hình học.</summary>
    public sealed class SlotDef
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public Match.Vec2 Position { get; set; }

        public bool IsField => Type == "field";
    }

    /// <summary>
    /// Hình học từ path.json: đường chạy, ô đặt tướng, và tỉ lệ quy đổi.
    ///
    /// `PixelsPerUnit` + `DesignWidthPt` là thứ nối hình học "unit" với ngón tay
    /// thật: 1 unit = 100px, và 1080px render ra ~393pt → 1 unit ≈ 36.4pt. Vùng
    /// chạm tối thiểu 48pt của Apple = 1.32 unit. Hình học đúng trong unit vẫn có
    /// thể không bấm trúng trên máy — nên hai con số này phải đi cùng nhau.
    /// </summary>
    /// <summary>
    /// MỘT tuyến chạy. Map một tuyến = danh sách một phần tử.
    ///
    /// Vì sao luôn là danh sách kể cả khi chỉ có một: giữ song song hai dạng ("path"
    /// cho map cũ, "lanes" cho map mới) buộc mọi chỗ đọc hình học phải rẽ nhánh, và
    /// nhánh đó là nơi bug im lặng sinh ra. Xem docs/08-MAPS-ARCHITECTURE.md §2.2(a).
    /// </summary>
    public sealed class LaneDef
    {
        public string Id { get; set; } = "L1";
        public IReadOnlyList<Match.Vec2> Waypoints { get; set; } = Array.Empty<Match.Vec2>();

        /// <summary>Độ dài KHAI BÁO. Mô hình cân bằng dùng con số này; spline thật
        /// phải khớp trong ±2% — luật mà tools/path_check.py và PathOracleTests ép.</summary>
        public double DeclaredLengthUnits { get; set; }

        public Match.Vec2 SpawnPoint { get; set; }
        public Match.Vec2 GoalPoint { get; set; }
    }

    public sealed class PathDef
    {
        /// <summary>Các tuyến chạy. LUÔN có ít nhất một.</summary>
        public IReadOnlyList<LaneDef> Lanes { get; set; } = Array.Empty<LaneDef>();

        public IReadOnlyList<SlotDef> Slots { get; set; } = Array.Empty<SlotDef>();

        /// <summary>Tuyến đầu tiên — lối tắt cho map một tuyến và cho code chưa
        /// chuyển sang đa tuyến. KHÔNG dùng ở chỗ phải xử lý mọi tuyến.</summary>
        public LaneDef MainLane => Lanes[0];

        /// <summary>Waypoint của tuyến đầu. Giữ để code/test một tuyến không phải sửa.</summary>
        public IReadOnlyList<Match.Vec2> Waypoints => MainLane.Waypoints;

        /// <summary>Độ dài khai báo của tuyến đầu.</summary>
        public double DeclaredLengthUnits => MainLane.DeclaredLengthUnits;

        public double PixelsPerUnit { get; set; }
        public double CameraOrthographicSize { get; set; }
        public double ViewportWidthUnits { get; set; }
        public double ViewportHeightUnits { get; set; }

        public double DesignWidthPt { get; set; }
        public double MinTouchTargetPt { get; set; }
        public double RadialMenuOuterRadiusPt { get; set; }

        /// <summary>Số pt trên một unit sân. Cách duy nhất để hỏi "ô này có bấm
        /// trúng không" mà không phải đoán.</summary>
        public double PtPerUnit => DesignWidthPt / ViewportWidthUnits;

        /// <summary>Vùng chạm tối thiểu quy ra unit. Ô đặt tướng nhỏ hơn cái này
        /// là ngón tay không bấm nổi, dù hình học có đẹp thế nào.</summary>
        public double MinTouchTargetUnits => MinTouchTargetPt / PtPerUnit;
    }

    public sealed class GameConfig
    {
        /// <summary>Id map đang chạy (`m00`, `m01`, ...). Từ `config/maps/*.json`.</summary>
        public string MapId { get; set; } = "m00";

        /// <summary>Tên hiển thị của map. Rỗng = chưa khai.</summary>
        public string MapDisplayName { get; set; } = "";

        public IReadOnlyList<TowerDef> Towers { get; set; } = Array.Empty<TowerDef>();
        public IReadOnlyList<EnemyDef> Enemies { get; set; } = Array.Empty<EnemyDef>();
        public IReadOnlyList<BossDef> Bosses { get; set; } = Array.Empty<BossDef>();
        public IReadOnlyList<WaveDef> Waves { get; set; } = Array.Empty<WaveDef>();
        public IReadOnlyList<ActDef> Acts { get; set; } = Array.Empty<ActDef>();
        public HpScaling HpScaling { get; set; } = new();
        public EconomyDef Economy { get; set; } = new();
        public PathDef Path { get; set; } = new();
        public double SpawnIntervalSec { get; set; }
        public double RestBetweenWavesSec { get; set; }

        /// <summary>
        /// 🔴 B-01 — phải là `false`. towers.json → `_upgradeRules.abilitiesAreCumulative`.
        ///
        /// `false` = tướng cấp N chạy ĐÚNG MỘT kỹ năng (cái có unlockLevel == N).
        /// Kỹ năng cấp thấp KHÔNG chạy. Nâng cấp là đánh đổi, không phải cộng dồn.
        ///
        /// Nằm trong model chứ không chỉ là chú thích ở JSON, vì luật 18 phải đọc
        /// được nó — cờ mà code không đọc thì chỉ là lời hứa.
        /// </summary>
        public bool AbilitiesAreCumulative { get; set; }

        public ActDef ActOf(int wave)
        {
            foreach (ActDef a in Acts)
                if (a.Contains(wave)) return a;
            throw new ConfigException($"wave {wave} không thuộc act nào");
        }

        /// <summary>Máu một con quái ở wave cụ thể. half_up theo economy.json →
        /// roundingMode. KHÔNG dùng Math.Round mặc định: nó là banker's rounding
        /// (62.5 → 62), lệch với tools/gen_docs.py. Xem docs/04 §8.</summary>
        public int HpOf(EnemyDef e, int wave) => Round.HalfUp(e.BaseHp * HpScaling.MultiplierAt(wave));

        public int BountyOf(EnemyDef e, int wave) => Round.HalfUp(e.BaseBounty * ActOf(wave).BountyMultiplier);
    }

    public static class Round
    {
        /// <summary>
        /// Làm tròn ra xa số 0 — 62.5 → 63.
        ///
        /// KHÔNG dùng Math.Round(x): mặc định của nó là MidpointRounding.ToEven
        /// (banker's rounding) → 62.5 thành 62. Python round() cũng vậy. Lỗi này
        /// ĐÃ xảy ra thật ở vòng 4: thưởng Tambor 25 × 2.5 = 62.5 lệch 28 Peso
        /// trên cả Act 3, giữa generator và game. Xem economy.json → _roundingWarning.
        /// </summary>
        public static int HalfUp(double x) => (int)Math.Round(x, MidpointRounding.AwayFromZero);
    }
}
