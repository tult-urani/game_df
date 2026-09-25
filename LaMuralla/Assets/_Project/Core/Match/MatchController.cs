using System;
using System.Collections.Generic;
using LaMuralla.Core.Config;
using LaMuralla.Core.Json;

namespace LaMuralla.Core.Match
{
    public enum MatchPhase
    {
        /// <summary>Trước wave 1, hoặc đang nghỉ giữa wave. Người chơi xây/nâng/bán.</summary>
        Preparing,
        /// <summary>Quân đang ra sân hoặc còn trên sân.</summary>
        Fighting,
        Won,
        Lost,
    }

    /// <summary>
    /// Điều phối một trận. Cột "không làm" ở docs/05 bảng §3: **"Tính sát thương"**
    /// — nó gọi AbilityEngine/CombatResolver/DamageSystem, không tự tính.
    ///
    /// 🔴 ĐÂY LÀ CHỖ LUẬT #1 GẶP LUẬT #2. `DamageSystem` phát `EnemyKilled` (nó
    /// KHÔNG được cộng tiền); lớp này nghe và gọi `EconomyService.EarnKill` (cổng
    /// duy nhất của ví). Một chỗ quyết ai giết, một chỗ giữ tiền, một sợi dây nối
    /// giữa — và sợi dây đó nằm ở đây, nhìn thấy được.
    /// </summary>
    /// <summary>Một tuyến sẽ ra quân ở wave sắp tới, và ra bao nhiêu con.</summary>
    public readonly struct LaneLoad
    {
        public LaneLoad(string lane, int count) { Lane = lane; Count = count; }
        public string Lane { get; }
        public int Count { get; }
    }

    public sealed class MatchController
    {
        private readonly GameConfig _cfg;
        /// <summary>
        /// Đường chạy theo TUYẾN. Map một tuyến = từ điển một phần tử.
        ///
        /// Không giữ một `EnemyPath` duy nhất nữa: quân biết mình thuộc tuyến nào
        /// (`Enemy.LaneId`) và mọi phép tính vị trí/lọt lưới đều tra qua đây. Xem
        /// docs/08 §3.1.
        /// </summary>
        private readonly Dictionary<string, EnemyPath> _lanes = new(StringComparer.Ordinal);

        /// <summary>Tuyến đầu — dùng khi cần một mốc chung (ví dụ độ dài tham chiếu).</summary>
        private readonly EnemyPath _path;
        private readonly WaveSpawner _spawner;
        private readonly List<Enemy> _enemies = new List<Enemy>();
        /// <summary>Chỉ mục id → quân. `Find()` từng quét tuyến tính cả list, và nó
        /// được gọi từ `SlowPercentOn` — tức MỖI phát bắn của MỖI tướng. Với 37 con
        /// ở W20 và 12 tướng, đó là hàng nghìn phép so sánh mỗi frame.</summary>
        private readonly Dictionary<int, Enemy> _byId = new Dictionary<int, Enemy>();
        /// <summary>
        /// `"f07:aura"` — chuỗi định danh nguồn aura, dựng MỘT LẦN cho mỗi ô.
        ///
        /// Trước đây `TickAuras()` làm `$"{t.SlotId}:aura"` mỗi frame cho mỗi tướng:
        /// 12 tướng × 60fps = 720 chuỗi cấp phát MỖI GIÂY, cộng băm chuỗi trong
        /// Dictionary của SlowStack. Đó là nguồn giật hình mà lần sửa trước bỏ sót —
        /// người chơi báo "vẫn còn giật" và họ đúng.
        ///
        /// (Field này từng được khai rồi KHÔNG dùng — code chết, đúng thứ lẽ ra
        /// để tránh chính lỗi này.)
        /// </summary>
        private readonly Dictionary<string, string> _auraSourceId = new Dictionary<string, string>();

        private string AuraSourceOf(string slotId)
        {
            if (_auraSourceId.TryGetValue(slotId, out string? id)) return id;
            id = slotId + ":aura";
            _auraSourceId[slotId] = id;
            return id;
        }

        private IReadOnlyList<ScheduledSpawn> _schedule = Array.Empty<ScheduledSpawn>();
        private int _spawnCursor;
        private double _waveClock;
        private double _restRemaining;
        private int _nextEnemyId = 1;

        /// <summary>Cấp phát MỘT LẦN. Trước đây `new Ctx(this)` chạy mỗi phát bắn
        /// của mỗi tướng — với D10S (tướng duy nhất đánh lan) nó giết nhiều con cùng
        /// lúc, ép cả đường ống chạy ở tần suất cao nhất và GC gom rác giữa trận →
        /// GIẬT HÌNH. Người chơi báo đúng hiện tượng này ở buổi thử máy.
        /// Ctx không giữ state riêng, nên một cái dùng chung là đủ.</summary>
        private readonly Ctx _ctx;

        /// <summary>
        /// Nguồn ngẫu nhiên của trận — CÓ HẠT GIỐNG.
        ///
        /// Dibu (vòng 18) là cơ chế RNG đầu tiên trong game. Dùng `Random` không hạt
        /// giống thì cùng một trận không bao giờ tái hiện được: người chơi báo "thua
        /// oan ở W14" và không ai dựng lại được tình huống. Hạt giống cố định ở test,
        /// ngẫu nhiên ở máy thật.
        /// </summary>
        private readonly Random _rng;

        public DamageSystem Damage { get; }
        public EconomyService Economy { get; }
        public GoalHealth Goal { get; }
        public SlotManager Slots { get; }
        public UpgradeService Upgrades { get; }
        public RewardedRecovery RewardedRecovery { get; }

        public MatchPhase Phase { get; private set; } = MatchPhase.Preparing;
        public int Wave { get; private set; }
        public IReadOnlyList<Enemy> Enemies => _enemies;
        public double RestRemaining => _restRemaining;

        /// <summary>Số tuyến của map. 1 = map một tuyến; HUD dùng để biết có cần
        /// báo trước tuyến hay không.</summary>
        public int LaneCount => _cfg.Path.Lanes.Count;

        /// <summary>
        /// Tuyến nào sẽ ra quân ở `wave`, kèm số con — theo thứ tự khai trong map.
        ///
        /// 🔴 Đây là YÊU CẦU THIẾT KẾ, không phải trang trí. `docs/maps/M03` §3:
        /// map luân phiên (wave lẻ ra `L1`, wave chẵn ra `L2`) mà HUD không báo
        /// trước trong lúc nghỉ thì người chơi phải ĐOÁN nửa số wave — đó là trò
        /// tung đồng xu, không phải bài toán bố trí. Rỗng = wave không tồn tại.
        /// </summary>
        public IReadOnlyList<LaneLoad> LanesOfWave(int wave)
        {
            var outp = new List<LaneLoad>();
            if (wave < 1 || wave > _cfg.Waves.Count) return outp;
            WaveDef w = _cfg.Waves[wave - 1];
            foreach (LaneDef ln in _cfg.Path.Lanes)
            {
                int n = 0;
                foreach (SpawnGroup g in w.Spawns) if (g.Lane == ln.Id) n += g.Count;
                foreach (BossSpawn b in w.Bosses) if (b.Lane == ln.Id) n++;
                if (n > 0) outp.Add(new LaneLoad(ln.Id, n));
            }
            return outp;
        }

        /// <summary>Tổng sát thương phí do đánh thừa. Để đo `etaWasteFactor = 0.75`
        /// ở M1 — một trong ba hệ số bịa của mô hình cân bằng.</summary>
        public double TotalOverkill { get; private set; }

        public event Action<int>? WaveStarted;
        public event Action<int>? WaveCleared;
        public event Action<MatchPhase>? PhaseChanged;

        /// <summary>Vừa bắn: (vị trí tháp, vị trí mục tiêu, kế hoạch). Lớp vỏ vẽ
        /// đường đạn. Core KHÔNG biết gì về hình ảnh — nó chỉ báo "có phát bắn".</summary>
        public event Action<Vec2, Vec2, AttackPlan>? ShotFired;

        public MatchController(GameConfig cfg) : this(cfg, Environment.TickCount) { }

        /// <param name="seed">Hạt giống RNG. Test truyền số cố định để tái hiện.</param>
        public MatchController(GameConfig cfg, int seed)
        {
            _rng = new Random(seed);
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            foreach (LaneDef ln in cfg.Path.Lanes)
                _lanes[ln.Id] = new EnemyPath(new List<Vec2>(ln.Waypoints));
            _path = _lanes[cfg.Path.Lanes[0].Id];
            _spawner = new WaveSpawner(cfg);

            Damage = new DamageSystem();
            Economy = new EconomyService(cfg.Economy);
            Goal = new GoalHealth(cfg.Economy.GoalHealth);
            Slots = new SlotManager(cfg);
            Upgrades = new UpgradeService(cfg, Slots, Economy);
            RewardedRecovery = new RewardedRecovery(this, cfg.Economy);

            // 🔴 Sợi dây nối luật #2 → luật #1. DamageSystem quyết ai giết;
            // EconomyService là cổng duy nhất của ví. Đây là chỗ DUY NHẤT hai luật
            // chạm nhau, và nó nhìn thấy được trong 6 dòng.
            _ctx = new Ctx(this);
            _byDistanceToCentre = (a, b) => Vec2.SqrDistance(_sortCentre, a.Position)
                                               .CompareTo(Vec2.SqrDistance(_sortCentre, b.Position));
            Damage.EnemyKilled += OnEnemyKilled;
            Goal.GoalLost += () => SetPhase(MatchPhase.Lost);
        }

        private void OnEnemyKilled(KillInfo k)
        {
            TotalOverkill += k.Overkill;

            // KillerSlotId == null → không tướng nào ghi công (quân lọt lưới tới cầu
            // môn, hoặc tự huỷ). Không ai ghi công = không tiền. (Dibu giờ ghi công
            // bằng ô của nó → KHÔNG rơi vào nhánh này nữa.)
            if (k.KillerSlotId == null) { RemoveEnemy(k.EnemyId); return; }

            Enemy? e = Find(k.EnemyId);
            if (e != null)
            {
                if (e.IsBoss) Economy.EarnBossKill(BossDefOf(e.DefId), Wave);
                else Economy.EarnKill(EnemyDefOf(e.DefId), Wave, _cfg);
            }
            RemoveEnemy(k.EnemyId);
        }

        // ── Vòng đời wave ───────────────────────────────────────────────────

        /// <summary>Bắt đầu wave kế tiếp. Người chơi bấm, hoặc hết giờ nghỉ.</summary>
        public void StartNextWave()
        {
            if (Phase == MatchPhase.Fighting)
                throw new InvalidOperationException("wave đang chạy");
            if (Phase is MatchPhase.Won or MatchPhase.Lost)
                throw new InvalidOperationException("trận đã kết thúc");

            Wave++;
            _schedule = _spawner.Schedule(Wave);
            _spawnCursor = 0;
            _waveClock = 0;
            _restRemaining = 0;
            SetPhase(MatchPhase.Fighting);
            WaveStarted?.Invoke(Wave);
        }

        /// <summary>
        /// Bỏ qua thời gian nghỉ để lấy thưởng: `giây_còn_lại × skipBonusPerSecond`.
        ///
        /// Đây là một trong ~5 nguồn tiền — và là lý do luật #1 tồn tại. Nó KHÔNG
        /// tự cộng vào ví; nó gọi `EconomyService`.
        /// </summary>
        public void SkipRest()
        {
            if (Phase != MatchPhase.Preparing || _restRemaining <= 0)
                throw new InvalidOperationException("không có thời gian nghỉ nào để bỏ qua");

            Economy.EarnSkipRest(_restRemaining);
            _restRemaining = 0;
            StartNextWave();
        }

        // ── Tick ────────────────────────────────────────────────────────────

        /// <summary>
        /// Đẩy thời gian đi `dt` giây.
        ///
        /// ⚠️ `dt` phải là MatchDeltaTime (đã nhân hệ số ×2 nếu người chơi bật),
        /// KHÔNG phải Time.deltaTime thuần — docs/05 §4.4: thời gian NGHỈ không đổi
        /// khi ×2, chỉ thời gian TRẬN mới nhanh lên.
        /// </summary>
        public void Tick(double dt)
        {
            if (dt <= 0) return;
            if (Phase is MatchPhase.Won or MatchPhase.Lost) return;

            if (Phase == MatchPhase.Preparing)
            {
                if (_restRemaining > 0)
                {
                    _restRemaining -= dt;
                    if (_restRemaining <= 0) StartNextWave();
                }
                return;
            }

            _waveClock += dt;
            SpawnDue();
            TickAuras();
            MoveEnemies(dt);
            TickTowers(dt);
            CheckWaveCleared();
        }

        private void SpawnDue()
        {
            while (_spawnCursor < _schedule.Count && _schedule[_spawnCursor].TimeSec <= _waveClock)
            {
                ScheduledSpawn s = _schedule[_spawnCursor++];
                Spawn(s);
            }
        }

        private void Spawn(ScheduledSpawn s)
        {
            double speed, resist;
            int leak;
            if (s.IsBoss)
            {
                BossDef b = BossDefOf(s.EnemyId);
                speed = b.Speed; resist = b.SlowResistPercent; leak = b.LeakDamage;
            }
            else
            {
                EnemyDef d = EnemyDefOf(s.EnemyId);
                speed = d.Speed; resist = d.SlowResistPercent; leak = d.LeakDamage;
            }

            int id = _nextEnemyId++;
            var e = new Enemy(id, s.EnemyId, s.Hp, speed, resist, leak, s.IsBoss,
                              _cfg.Economy.SlowCapPercent, Damage, s.LaneId)
            {
                Position = LaneOf(s.LaneId).Spawn,
            };
            Damage.Register(id, s.Hp);
            _enemies.Add(e);
            _byId[id] = e;
        }

        /// <summary>Đường của một tuyến. Id sai là lỗi CẤU HÌNH (luật 5 chặn từ lúc
        /// bake), nên ném thay vì lặng lẽ rơi về tuyến đầu — rơi về thì quân chạy sai
        /// đường mà không ai biết.</summary>
        private EnemyPath LaneOf(string laneId)
        {
            if (_lanes.TryGetValue(laneId, out EnemyPath? p)) return p;
            throw new ConfigException($"tuyến `{laneId}` không có trong lanes[] — "
                                      + $"có: {string.Join(", ", _lanes.Keys)}");
        }

        private void MoveEnemies(double dt)
        {
            // Duyệt ngược: quân lọt lưới bị gỡ khỏi list ngay trong vòng lặp.
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                Enemy e = _enemies[i];
                e.Slows.Tick(dt);
                e.DistanceTravelled += e.CurrentSpeed * dt;

                EnemyPath lane = LaneOf(e.LaneId);
                if (e.DistanceTravelled >= lane.Length)
                {
                    Leak(e);
                    continue;
                }
                e.Position = lane.PositionAt(e.DistanceTravelled);
            }
        }

        /// <summary>
        /// Quân tới cầu môn. `03` §6: "Trừ máu, tự huỷ, KHÔNG rơi tiền".
        ///
        /// Dùng `RemoveWithoutKill`, KHÔNG phải `Apply(∞)` — Apply sẽ phát KillInfo
        /// → OnEnemyKilled → cộng tiền → người chơi được thưởng vì để quân lọt lưới.
        /// </summary>
        private void Leak(Enemy e)
        {
            Damage.RemoveWithoutKill(e.Id);
            _enemies.Remove(e);
            _byId.Remove(e.Id);
            Goal.Leak(e.LeakDamage, e.DefId);
        }

        private void CheckWaveCleared()
        {
            if (Phase != MatchPhase.Fighting) return;
            if (_spawnCursor < _schedule.Count || _enemies.Count > 0) return;

            Economy.EarnWaveClear(Wave);
            WaveCleared?.Invoke(Wave);

            if (Wave >= _cfg.Waves.Count) { SetPhase(MatchPhase.Won); return; }

            _restRemaining = _cfg.RestBetweenWavesSec;
            SetPhase(MatchPhase.Preparing);
        }

        private void SetPhase(MatchPhase p)
        {
            if (Phase == p) return;
            Phase = p;
            PhaseChanged?.Invoke(p);
        }

        internal void ResumeAfterRewardedContinue()
        {
            if (Phase != MatchPhase.Lost)
                throw new InvalidOperationException("chỉ được tiếp tục sau khi thua");
            SetPhase(MatchPhase.Fighting);
        }

        public StarRating Rating => Phase == MatchPhase.Won ? Goal.RatingOnWin() : StarRating.None;

        // ── Tướng ───────────────────────────────────────────────────────────

        private void TickTowers(double dt)
        {
            foreach (TowerInstance t in Slots.Towers)
            {
                TowerDef def = Upgrades.TowerById(t.TowerId);
                AbilityDef? ab = def.AbilityAt(t.Level);

                if (t.CooldownRemaining > 0) t.CooldownRemaining -= dt;
                if (t.AbilityActiveRemaining > 0) t.AbilityActiveRemaining -= dt;
                if (t.AttackCooldownRemaining > 0) t.AttackCooldownRemaining -= dt;

                if (ab != null && ab.CooldownSec > 0 && t.CooldownRemaining <= 0)
                    FireCooldownAbility(t, def, ab);

                TickAttack(t, def, dt);
            }
        }

        /// <summary>Kỹ năng `trigger: cooldown` — nổ khi đếm ngược về 0.</summary>
        private void FireCooldownAbility(TowerInstance t, TowerDef def, AbilityDef ab)
        {
            Vec2 pos = Slots.SlotOf(t.SlotId).Position;
            double range = def.Levels[t.Level - 1].Range;
            bool fired = false;

            switch (ab.Id)
            {
                // La Pulga Lv1 — bật ×2 trong `durationSec`, không nổ tức thì.
                case "solo_run":
                    t.AbilityActiveRemaining = Num(ab, "durationSec", 0);
                    fired = true;
                    break;

                // D10S Lv1 — chậm MỌI con trong tầm, `scope: timed`.
                case "ban_tay_cua_chua":
                    foreach (Enemy e in InRange(pos, range))
                        e.Slows.Apply($"{t.SlotId}:{ab.Id}", Num(ab, "slowPercent", 0),
                                      SlowScope.Timed, Num(ab, "durationSec", 0));
                    fired = true;
                    break;

                // El Árbitro Lv2/Lv3 (vòng 22) — ném thẻ cho 1 con chậm VĨNH VIỄN.
                // Aura nền 25% (auraSlowPercent) áp riêng ở TickAuras, không ở đây.
                case "the_vang":
                case "the_do":
                    fired = DrawCard(t, ab, pos, range);
                    break;

                // Dibu KHÔNG còn kỹ năng cooldown nào (vòng 18): nó giết bằng xác
                // suất mỗi lần đánh, xử lý ở TickKillChance. Ba case cũ
                // (can_pha/ap_dao/nguoi_hung_luan_luu) đã xoá cùng BlockAtGoal —
                // để lại thì chúng là code chết mà người sau tưởng còn chạy.
            }

            // Chỉ reset cooldown khi kỹ năng THỰC SỰ nổ. Thẻ vàng không tìm được ai
            // → không tiêu cooldown, thử lại frame sau. Nếu reset vô điều kiện thì
            // Árbitro "phí" 8 giây vì không có mục tiêu, và người chơi thấy nó đứng
            // im mà không hiểu vì sao.
            if (!fired) return;
            t.CooldownRemaining = ab.CooldownSec;
            AbilityFired?.Invoke(ab.Id, pos, range);
        }

        private bool DrawCard(TowerInstance t, AbilityDef ab, Vec2 pos, double range)
        {
            Enemy? target = TargetingSystem.Select(_enemies, pos, range,
                                                   TargetSelector.FirstInRangeWithoutCard);
            if (target == null) return false;

            string card = Str(ab, "appliesCard", "yellow");
            target.Card = card == "red" ? CardState.Red : CardState.Yellow;

            // `scope: permanent` — theo quân ra khỏi tầm, không gỡ. Nguồn id gắn với
            // THẺ chứ không gắn với ô: bán Árbitro đi thì thẻ vẫn còn trên quân.
            // SlowStack lấy MAX nên thẻ (45/65) đè aura nền (25) trên con trúng.
            target.Slows.Apply($"card:{card}", Num(ab, "slowPercent", 0), SlowScope.Permanent);
            CardShown?.Invoke(card, pos, target.Position);
            return true;
        }

        private void TickAttack(TowerInstance t, TowerDef def, double dt)
        {
            TowerLevel lv = def.Levels[t.Level - 1];
            if (lv.AttackRate <= 0) return;                     // El Árbitro

            AbilityDef? killAb = def.AbilityAt(t.Level);
            if (killAb != null && killAb.Params.ContainsKey("killChancePercent"))
            {
                TickKillChance(t, def, lv, killAb, dt);
                return;
            }

            if (lv.Damage <= 0) return;

            Vec2 pos = Slots.SlotOf(t.SlotId).Position;
            AbilityDef? ab = def.AbilityAt(t.Level);

            TargetSelector sel = ab?.Id == "solo_run" && t.AbilityActiveRemaining > 0
                ? TargetSelector.HighestAbsoluteHp
                : TargetSelector.FirstInRange;

            Enemy? target = TargetingSystem.Select(_enemies, pos, lv.Range, sel);
            if (target == null) return;
            if (t.AttackCooldownRemaining > 0) return;

            AttackPlan plan = AbilityEngine.Plan(t, def, pos, target, _ctx);
            ShotFired?.Invoke(pos, target.Position, plan);
            ApplyPlan(plan, pos, t.SlotId);

            t.AttacksFired++;
            t.AbilityCharged = false;   // cú vô lê tiêu ở đây, đúng một phát
            t.AttackCooldownRemaining = 1.0 / AbilityEngine.AttackRateOf(t, def);
        }

        /// <summary>
        /// Dibu — giết bằng XÁC SUẤT, không bằng sát thương (vòng 18, user chốt:
        /// "chỉ có tỉ lệ đánh chết quái bất kỳ là 20/30/40, không có kỹ năng khác").
        ///
        /// Giết CÓ ghi công (user chốt 2026-07-22): đi qua `Apply` với ô Dibu →
        /// phát KillInfo → `OnEnemyKilled` cộng tiền như mọi cú giết thường (đảo
        /// lại thiết kế `dropsBounty: false` cũ). Dùng đúng máu còn lại, KHÔNG `∞`,
        /// để overkill = 0 (Apply(∞) sẽ nhét vô cực vào `TotalOverkill`).
        ///
        /// `worksOnBoss: true` — docs/03 §6 chốt boss "cản được, và đó là điểm mấu
        /// chốt". Không có ngoại lệ nào cho boss ở đây.
        /// </summary>
        private void TickKillChance(TowerInstance t, TowerDef def, TowerLevel lv,
                                    AbilityDef ab, double dt)
        {
            if (t.AttackCooldownRemaining > 0) return;

            Vec2 pos = Slots.SlotOf(t.SlotId).Position;
            Enemy? target = TargetingSystem.Select(_enemies, pos, lv.Range, TargetSelector.FirstInRange);
            if (target == null) return;

            // Qua AttackRateOf (không phải lv.AttackRate thẳng) để hệ số debug nhịp
            // đánh áp cho CẢ Dibu; ở 1.0 giá trị y hệt lv.AttackRate.
            t.AttackCooldownRemaining = 1.0 / AbilityEngine.AttackRateOf(t, def);
            t.AttacksFired++;

            // Dibu NÉM mỗi nhịp như tướng thường (bắt buộc có mục tiêu) — hình ảnh
            // ném không còn gắn với việc trúng. Xác suất chỉ quyết định con đó CÓ
            // CHẾT hay không, không quyết định có ném hay không.
            Threw?.Invoke(target.Id, pos);

            if (_rng.NextDouble() * 100.0 >= Num(ab, "killChancePercent", 0)) return;

            // Giết CÓ ghi công: Apply với ô Dibu → OnEnemyKilled cộng tiền + gỡ quân
            // (RemoveEnemy). Máu đúng bằng máu còn lại → overkill 0.
            Damage.Apply(target.Id, Damage.HpOf(target.Id), t.SlotId);
            t.BlocksMade++;
            SavedCount++;
            Saved?.Invoke(target.Id, pos);
        }

        /// <summary>Tổng số quái Dibu đã cản (loại thẳng, không tính điểm) trong trận.
        /// HUD hiện con số này — cản là XÁC SUẤT, người chơi muốn biết Dibu "ăn" được
        /// bao nhiêu để đánh giá có đáng tiền không.</summary>
        public int SavedCount { get; private set; }

        /// <summary>Dibu vừa NÉM vào 1 con (mỗi nhịp tấn công, bất kể trúng/trượt):
        /// (id quân bị nhắm, vị trí Dibu). Lớp vỏ tung anim ném + đạn bay. Con có
        /// chết hay không do <see cref="Saved"/> quyết định — trượt thì chỉ ném.</summary>
        public event Action<int, Vec2>? Threw;

        /// <summary>Dibu vừa cản thành công (đạn trúng + trúng xác suất chết): (id
        /// quân, vị trí Dibu). Lớp vỏ vẽ vòng "CẢN!" + xoá quân. KHÔNG còn tung anim
        /// ném ở đây — anim/đạn đã ở <see cref="Threw"/>.</summary>
        public event Action<int, Vec2>? Saved;

        /// <summary>Kỹ năng vừa nổ: (id kỹ năng, vị trí tháp, tầm). Lớp vỏ vẽ hiệu
        /// ứng. Core KHÔNG biết gì về hình ảnh — nó chỉ báo "vừa có chuyện xảy ra".</summary>
        public event Action<string, Vec2, double>? AbilityFired;

        /// <summary>Trọng tài vừa ném thẻ vào 1 con: (màu thẻ "yellow"/"red", vị trí
        /// trọng tài, vị trí con bị phạt). Lớp vỏ vẽ thẻ bay tới đúng mục tiêu —
        /// `AbilityFired` chỉ cho vị trí tháp, không đủ để biết bay về đâu.</summary>
        public event Action<string, Vec2, Vec2>? CardShown;

        private void ApplyPlan(AttackPlan plan, Vec2 towerPos, string slotId)
        {
            // Buffer dùng lại — mỗi phát bắn từng cấp phát một List. Đo hồi còn
            // El Fideo (đã gỡ): 2 phát/giây × 6 tướng = 12 rác mỗi giây → GC gom
            // giữa trận → giật hình. La Pulga 1.2 phát/giây vẫn đủ để tái diễn.
            List<Enemy> hit = _hitBuf;
            switch (plan.Shape)
            {
                case AttackShape.Splash:
                    CombatResolver.SplashInto(_enemies, plan.TargetPos, plan.SplashRadius, hit);
                    break;
                case AttackShape.Line:
                    CombatResolver.LineInto(_enemies, towerPos, plan.TargetPos,
                                            plan.LineLength, plan.LineWidth, hit);
                    break;
                default:
                    hit.Clear();
                    Enemy? only = Find(plan.TargetId);
                    if (only != null) hit.Add(only);
                    break;
            }

            // `MaxExtraTargets` = 0 nghĩa là KHÔNG GIỚI HẠN với sut_xuyen (xuyên cả
            // hàng), nhưng `tat_canh` khai `splashTargets: 2` → mục tiêu chính + 2.
            if (plan.MaxExtraTargets > 0 && hit.Count > plan.MaxExtraTargets + 1)
                TrimToNearest(hit, plan.TargetPos, plan.MaxExtraTargets + 1);

            foreach (Enemy e in hit)
                Damage.Apply(e.Id, plan.Damage, slotId);
        }

        /// <summary>
        /// Giữ `n` con gần tâm nhất. `tat_canh` lan sang "2 quân GẦN NHẤT" (docs/02)
        /// — không phải 2 con bất kỳ trong bán kính.
        ///
        /// Cắt TẠI CHỖ, không `GetRange` (cấp phát List mới). Và comparer là field,
        /// không phải lambda: `(a,b) => ...centre...` BẮT biến `centre` nên nó cấp
        /// phát một closure mỗi phát bắn.
        /// </summary>
        private void TrimToNearest(List<Enemy> hit, Vec2 centre, int n)
        {
            _sortCentre = centre;
            hit.Sort(_byDistanceToCentre);
            if (hit.Count > n) hit.RemoveRange(n, hit.Count - n);
        }

        private Vec2 _sortCentre;
        private readonly Comparison<Enemy> _byDistanceToCentre;
        /// <summary>
        /// ⚠️ DÙNG CHUNG. `ApplyPlan` ghi vào đây rồi duyệt nó. An toàn HIỆN NAY vì
        /// không gì trong vòng lặp gọi ngược lại `ApplyPlan`. Nếu sau này thêm hiệu
        /// ứng dây chuyền (một cú chết kích hoạt một phát bắn khác) thì buffer bị
        /// ghi đè GIỮA CHỪNG và sát thương rơi vào sai người — không crash, không log.
        /// Lúc đó phải đổi sang stack buffer hoặc cấp phát riêng cho nhánh đệ quy.
        /// </summary>
        private readonly List<Enemy> _hitBuf = new List<Enemy>();

        // ── Aura ────────────────────────────────────────────────────────────

        /// <summary>
        /// Kỹ năng `trigger: aura` và phần aura của Dibu. Chạy MỖI TICK vì
        /// `scope: in_range` — quân ra khỏi tầm là hết chậm ngay.
        /// </summary>
        private void TickAuras()
        {
            foreach (TowerInstance t in Slots.Towers)
            {
                TowerDef def = Upgrades.TowerById(t.TowerId);
                AbilityDef? ab = def.AbilityAt(t.Level);
                if (ab == null) continue;

                double pct = ab.Trigger == "aura"
                    ? Num(ab, "slowPercent", 0)
                    : Num(ab, "auraSlowPercent", 0);   // Dibu ap_dao / nguoi_hung
                if (pct <= 0) continue;

                Vec2 pos = Slots.SlotOf(t.SlotId).Position;
                double range = def.Levels[t.Level - 1].Range;
                string src = AuraSourceOf(t.SlotId);
                double r2 = range * range;

                foreach (Enemy e in _enemies)
                {
                    bool inside = Vec2.SqrDistance(pos, e.Position) <= r2;
                    if (inside) e.Slows.Apply(src, pct, SlowScope.InRange);
                    else e.Slows.Remove(src);   // `in_range`: ra khỏi tầm là hết
                }
            }
        }

        // ── Ngữ cảnh cho AbilityEngine ──────────────────────────────────────

        private sealed class Ctx : IAbilityContext
        {
            private readonly MatchController _m;
            public Ctx(MatchController m) => _m = m;

            public double SlowPercentOn(int enemyId)
            {
                Enemy? e = _m.Find(enemyId);
                return e == null ? 0 : e.Slows.EffectiveSlowPercent(e.SlowResistPercent);
            }

            /// <summary>Có D10S Lv3 nào trên sân không. `ban_thang_the_ky` buff MỌI
            /// tướng — kể cả tướng của người khác, kể cả chính nó.</summary>
            public bool AnyGlobalSlowedDamageBuff
            {
                get
                {
                    foreach (TowerInstance t in _m.Slots.Towers)
                        if (_m.Upgrades.TowerById(t.TowerId).AbilityAt(t.Level)?.Id == "ban_thang_the_ky")
                            return true;
                    return false;
                }
            }

            public double GlobalSlowedDamageBonusPercent
            {
                get
                {
                    foreach (TowerInstance t in _m.Slots.Towers)
                    {
                        AbilityDef? ab = _m.Upgrades.TowerById(t.TowerId).AbilityAt(t.Level);
                        if (ab?.Id == "ban_thang_the_ky")
                            return Num(ab, "allTowersBonusDamagePercentVsSlowed", 0);
                    }
                    return 0;
                }
            }

            /// <summary>Trần làm chậm từ `economy.json` — mẫu số của buff tỉ lệ.</summary>
            public double SlowCapPercent => _m._cfg.Economy.SlowCapPercent;
        }

        // ── Tiện ích ────────────────────────────────────────────────────────

        /// <summary>Quân trong tầm. Ghi vào buffer dùng lại thay vì `yield return`
        /// — iterator cấp phát mỗi lần gọi, và hàm này chạy trong đường nóng.
        /// KHÔNG lồng hai lần gọi vào nhau: cả hai dùng chung một buffer.</summary>
        private readonly List<Enemy> _inRangeBuf = new List<Enemy>();

        private List<Enemy> InRange(Vec2 pos, double range)
        {
            _inRangeBuf.Clear();
            double r2 = range * range;
            for (int i = 0; i < _enemies.Count; i++)
                if (Vec2.SqrDistance(pos, _enemies[i].Position) <= r2) _inRangeBuf.Add(_enemies[i]);
            return _inRangeBuf;
        }

        private Enemy? Find(int id) => _byId.TryGetValue(id, out Enemy? e) ? e : null;

        private void RemoveEnemy(int id)
        {
            if (!_byId.TryGetValue(id, out Enemy? e)) return;
            _enemies.Remove(e);
            _byId.Remove(id);
        }

        private EnemyDef EnemyDefOf(string id)
        {
            foreach (EnemyDef e in _cfg.Enemies)
                if (e.Id == id) return e;
            throw new ConfigException($"không có quân `{id}`");
        }

        private BossDef BossDefOf(string id)
        {
            foreach (BossDef b in _cfg.Bosses)
                if (b.Id == id) return b;
            throw new ConfigException($"không có boss `{id}`");
        }

        private static double Num(AbilityDef ab, string key, double fallback) =>
            ab.Params.TryGetValue(key, out JsonValue? v) ? v.AsNumber() : fallback;

        private static string Str(AbilityDef ab, string key, string fallback) =>
            ab.Params.TryGetValue(key, out JsonValue? v) ? v.AsString() : fallback;
    }
}
