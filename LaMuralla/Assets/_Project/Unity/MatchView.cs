using System.Collections.Generic;
using Action = System.Action;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Lớp vỏ Unity của một trận. Nó KHÔNG chứa luật chơi nào — mọi thứ nằm ở
    /// `MatchController` (pure C#, 218 test, chạy được không cần Unity).
    ///
    /// Việc của lớp này đúng ba thứ:
    ///   1. đẩy thời gian cho MatchController
    ///   2. vẽ state ra màn hình
    ///   3. biến ngón tay thành lệnh (mua/nâng/bán)
    ///
    /// Nếu thấy mình sắp viết `if (enemy.Hp <= 0)` ở đây thì dừng lại — chỗ đó
    /// thuộc về core.
    /// </summary>
    public sealed class MatchView : MonoBehaviour
    {
        private MatchController _match = null!;
        private PathDef _def = null!;
        private Hud _hud = null!;
        private AdMobRewardedAds? _adGateway;
        private RewardedAdCoordinator? _rewardedAds;
        private bool _resumeAfterRewardedAd;

        private readonly Dictionary<int, GameObject> _enemyGo = new();
        private readonly Dictionary<int, Transform> _hpBar = new();

        /// <summary>SpriteRenderer của thanh máu, cache sẵn. Bản cũ gọi
        /// `GetComponent<SpriteRenderer>()` cho MỖI quân MỖI frame — ở W20 với 27 quân
        /// trên sân là 27 lần tra component mỗi frame, chỉ để đổi một màu.</summary>
        private readonly Dictionary<int, SpriteRenderer> _hpBarSr = new();
        // SpriteRenderer của thân quân, giữ sẵn để lật mặt mỗi khung mà không phải
        // GetComponentInChildren — vòng lặp này từng gây giật hình vì cấp phát.
        private readonly Dictionary<int, SpriteRenderer> _enemyArt = new();
        // (bề rộng, độ cao treo) của thanh máu từng con — chốt lúc tạo vì nó phụ
        // thuộc con đó đang là sprite hay hình khối, hai loại cao khác hẳn nhau.
        private readonly Dictionary<int, Vector2> _barGeom = new();
        private readonly Dictionary<string, GameObject> _towerGo = new();
        private readonly Dictionary<string, LineRenderer> _slotRing = new();

        // Tướng có art hoạt ảnh (sprite sheet đã bake) — trigger cú xút khi bắn.
        // Tướng chưa có art vẫn dùng hình khối `Draw`; hai kiểu sống chung.
        private readonly Dictionary<string, Animator> _towerAnim = new();
        // Chỉ El Cinco có idle khác nhau theo cấp. Cache cấp đã đẩy sang Animator
        // để không SetInteger lặp lại mỗi frame.
        private readonly Dictionary<string, int> _towerVisualLevel = new();
        // Mỗi tướng chỉ có những param của riêng nó; `FireTrigger` lọc theo bộ param
        // CÓ THẬT nên kích nhầm tên không làm gì cả (xem `_animTriggers`).
        private static readonly int KickHash = Animator.StringToHash("Kick");
        // Trọng tài (aura, vòng 21): giơ thẻ vàng (Lv2) / đỏ (Lv3) định kỳ cho sinh
        // động — thuần animation, không gắn cơ chế.
        private static readonly int YellowHash = Animator.StringToHash("Yellow");
        private static readonly int RedHash = Animator.StringToHash("Red");
        // Dibu (thủ môn): mỗi cấp một đòn ném — Lv1 bóng, Lv2 găng, Lv3 cúp.
        private static readonly int BallHash = Animator.StringToHash("Ball");
        private static readonly int GloveHash = Animator.StringToHash("Glove");
        private static readonly int CupHash = Animator.StringToHash("Cup");
        // El Cinco (batigol): đấm (Lv1) / đá (Lv2 = Kick, dùng lại KickHash) /
        // húc cuồng nộ (Lv3; trigger Stomp giữ nguyên để tương thích controller).
        private static readonly int PunchHash = Animator.StringToHash("Punch");
        private static readonly int StompHash = Animator.StringToHash("Stomp");
        // D10S (Maradona): mỗi cấp một đòn ném — Lv1 bóng (dùng lại BallHash),
        // Lv2 chai (Bottle), Lv3 cối (Mortar).
        private static readonly int BottleHash = Animator.StringToHash("Bottle");
        private static readonly int MortarHash = Animator.StringToHash("Mortar");
        // La Pulga: mỗi cấp một cú xút — Lv1 thường (dùng lại KickHash), Lv2 xoáy
        // (Curl), Lv3 vô-lê (Volley).
        private static readonly int CurlHash = Animator.StringToHash("Curl");
        private static readonly int VolleyHash = Animator.StringToHash("Volley");
        // Trigger CÓ THẬT trong controller của từng ô, đọc một lần lúc tạo Animator.
        // Không có bộ này thì mọi SetTrigger sai tên vừa spam warning vừa để hero
        // đứng hình — đúng thứ xảy ra trong khoảng giữa "đổi code" và "bake lại".
        private readonly Dictionary<string, HashSet<int>> _animTriggers = new();
        private const float SpriteArtScale = 1.8f;   // nhân vật to hơn hình khối placeholder
        // Frame lao Lv3 của El Cinco có canvas cao 724px và silhouette thấp hơn
        // sprite Lv2 (426/724 so với 213/232 đơn vị). Bù riêng art, không scale
        // parent để pip/vòng ô vẫn cùng cỡ các tướng khác.
        private const float BatigolBerserkArtScale = 2.8f;
        private const float ArtStandOffset = 0.5f;    // nâng nhân vật lên (chân ~ tâm ô), đẹp hơn
        private static Sprite? _ballSprite;           // cầu lửa (nạp 1 lần, dùng lại)
        private static Sprite? _pulgaNormalBallSprite; // Pulga Lv1: bóng thường
        private static Sprite? _pulgaGoldBallSprite;   // Pulga Lv2: bóng vàng
        private static Sprite? _gloveSprite;          // găng Dibu Lv2 (nạp 1 lần)
        private static Sprite? _cupSprite;            // cúp Dibu Lv3 (nạp 1 lần)
        private static Sprite? _bottleSprite;         // chai rượu D10S Lv2 (nạp 1 lần)

        // Buffer dùng lại — xem chú thích ở SyncEnemies. Cấp phát trong vòng vẽ
        // là nguồn giật hình, và nó chỉ lộ ra trên máy thật.
        private readonly HashSet<int> _live = new();
        private readonly List<int> _dead = new();
        private readonly List<string> _goneTowers = new();


        private LineRenderer? _rangeRing;
        private Camera _cam = null!;

        // 🔴 HÌNH HỌC DÙNG CHUNG giữa cái được VẼ và cái được ĐO để đóng khung.
        // Trước đây mấy số này là const cục bộ trong DrawGoal/DrawStatic — và đó
        // đúng là cách khung thành lọt ra ngoài màn: camera khớp theo hộp thiết kế
        // 10.8 × 19.2 của path.json, không chỗ nào biết net bị hạ thêm 1.2 unit
        // xuống dưới waypoint cuối. Số nằm một chỗ thì không lệch được nữa.
        private const float GoalDropUnits   = 1.2f;    // net hạ dưới vạch cầu môn
        private const float GoalWidthUnits  = 2.6f;
        private const float GoalHeightUnits = 1.05f;
        private const float GoalLineWidth   = 0.10f;
        // Bề rộng con đường. 0.5 là số của thời `LineRenderer` vẽ VỆT; giờ nó là
        // mặt đường thật, phải rộng hơn cổ động viên (0.7 unit) mới ra dáng lối đi.
        private const float PathLineWidth   = 0.95f;
        private const float FrameMarginUnits = 0.3f;   // đừng để thứ gì hôn mép màn

        // Thứ tự vẽ, từ dưới lên. Gom một chỗ vì bug "cái này đè cái kia" luôn bắt
        // đầu bằng việc phải đi tìm các con số này rải rác khắp file.
        // Sheet quái nhập ở PPU 384 = chiều cao ô → mỗi frame cao đúng 1 unit.
        // Nhân vật chiếm 0.852 chiều cao ô (bbox dọc y 48→374 trên adepto_enemy.png,
        // đo cả 4 frame đều bằng nhau). Muốn quân cao 0.75 unit thì scale 0.75/0.852.
        //
        // Vì sao 0.75: phải NHỎ HƠN tầm ngắn nhất (El Cinco Lv1 = 1.0) thì "trong
        // tầm / ngoài tầm" mới đọc được — đúng ràng buộc `docs/06 §4b`. Và phải nhỏ
        // hơn bề rộng đường (0.95) để quân nhìn như đang đi TRÊN đường.
        private const float EnemyArtScale = 0.88f;
        // Boss to gấp 1.8× quân thường (`docs/06 §4`). Nhân cả cỡ lẫn độ nâng chân,
        // nếu không thì boss to ra mà chân vẫn lún xuống dưới mặt đường.
        private const float BossArtMultiplier = 1.8f;

        private const int PitchOrder    = -20;   // mặt sân
        private const int RoadOrder     = -10;   // con đường
        private const int PedestalOrder = -5;    // bệ đứng của tướng

        /// <summary>Hộp bao mọi thứ vẽ trong world. Camera đóng khung theo cái này.</summary>
        private Bounds _content;

        /// <summary>Hệ số ×2. `05` §4.4: chỉ thời gian TRẬN nhanh lên, thời gian
        /// NGHỈ không đổi — nên nó nhân vào Tick, không vào Time.timeScale.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>
        /// Hiện bảng đo hiệu năng trên máy (avg/worst ms, KB rác mỗi giây, số lần GC).
        ///
        /// Bật để TRẢ LỜI câu "giật kiểu gì" bằng số thay vì bằng cảm giác:
        /// worst vọt so với avg + GC nhảy = khựng do rác; avg cao mà worst phẳng =
        /// quá tải vẽ. ĐẶT VỀ false TRƯỚC KHI PHÁT HÀNH.
        ///
        /// 🔵 ĐÃ TẮT: bảng này đã làm xong việc của nó. Nó là thứ chứng minh "giật"
        /// là trần 30 FPS của iOS chứ không phải rác (`avg 33.4 · worst 33.6`), và
        /// sau khi gỡ trần thì nó chỉ còn là một khối chữ vàng che mất sân. Bật lại
        /// khi có câu hỏi hiệu năng CẦN số để trả lời, đừng để bật thường trực.
        /// </summary>
        public bool ShowPerfHud = false;

        /// <summary>
        /// LỐI TẮT CHO NGƯỜI PHÁT TRIỂN: vào thẳng một map, BỎ QUA màn chọn map.
        /// Bỏ trống (mặc định) = hiện màn chọn map như người chơi thật thấy.
        /// </summary>
        public string MapId = "";

        /// <summary>
        /// Mở sẵn MỌI map ở màn chọn, bỏ qua luật "thắng map trước mới mở map sau".
        ///
        /// ⚠️ TẮT trong bản phát hành. Chỉ bật tạm thời khi cần kiểm tra nhanh các
        /// map bằng tay trong Unity Editor.
        /// </summary>
        public bool UnlockAllMaps = false;

        /// <summary>Tiền cộng thêm lúc khởi đầu, CHỈ để thử tướng. 0 = tắt.
        /// Xem chú thích ở Start(). Đặt về 0 trước khi phát hành.</summary>
        public int TestExtraCash = 0;     // 🔵 vòng 22: TẮT để build iPhone chạy đúng 700 Peso thiết kế

        /// <summary>
        /// 🔴 GỠ KHOÁ 30 FPS CỦA iOS.
        ///
        /// Unity mặc định `Application.targetFrameRate = 30` trên iOS/tvOS, và trong
        /// project này KHÔNG CHỖ NÀO đặt lại — nên game đã chạy 30 FPS từ đầu tới giờ.
        ///
        /// Đây là thứ người chơi gọi là "giật nhẹ", và nó KHÔNG phải rác. Số đo trên
        /// iPhone 12 lúc bị: `avg 33.4ms · worst 33.6ms · alloc 4 KB/s`. `worst` chỉ
        /// hơn `avg` 0.2ms — nếu là thu gom rác thì `worst` phải vọt gấp 2–3 lần. Độ
        /// lệch gần bằng không là chữ ký của một cái TRẦN CỨNG, và 33.3ms đúng bằng
        /// 1/30 giây.
        ///
        /// Bài học đắt: tôi đã sửa BẢY chỗ cấp phát mỗi frame qua hai vòng trước khi
        /// đo. Cả bảy đều là defect thật, nhưng KHÔNG cái nào là nguyên nhân. Một con
        /// số đo được thay thế ba vòng suy luận.
        ///
        /// `RuntimeInitializeOnLoadMethod` để nó áp cho CẢ màn chọn map, không riêng
        /// lúc vào trận. `vSyncCount = 0` vì trên iOS vSync bị bỏ qua — `targetFrameRate`
        /// mới là thứ quyết định, nhưng để 0 cho khỏi hiểu nhầm khi đọc lại.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void UnlockFrameRate()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Debug.Log($"[MatchView] targetFrameRate = {Application.targetFrameRate} "
                      + $"(mặc định iOS của Unity là 30 — đã gỡ)");
        }

        private void Start()
        {
            BakedConfig baked = Resources.Load<BakedConfig>(BakedConfig.ResourcePath);
            if (baked == null)
            {
                Debug.LogError("[MatchView] Không nạp được BakedConfig. Chạy `La Muralla → Bake Config`.");
                enabled = false;
                return;
            }

            // 🔵 MÀN CHỌN MAP LÀ MÀN ĐẦU TIÊN.
            //
            // Chưa chọn map (và không có lối tắt `MapId` trong Inspector) → dựng menu
            // rồi DỪNG Ở ĐÂY: không tạo trận, không dựng sân. Người chơi chạm một thẻ
            // thì `MapSession.PickedMapId` được đặt và scene NẠP LẠI — lần này rẽ
            // xuống nhánh dưới. Xem `MapSession` về lý do đi qua nạp lại scene.
            string wanted = !string.IsNullOrWhiteSpace(MapId)
                ? MapId.Trim()
                : MapSession.PickedMapId ?? "";

            if (string.IsNullOrEmpty(wanted))
            {
                // Âm thanh cũng dựng ở menu — chạm không có tiếng thì cảm giác chết.
                gameObject.AddComponent<AudioService>();
                MapSelect sel = gameObject.AddComponent<MapSelect>();
                sel.Baked = baked;
                sel.UnlockAll = UnlockAllMaps;
                return;
            }

            string? mapJson = baked.MapJson(wanted);
            if (mapJson == null)
            {
                var ids = new System.Text.StringBuilder();
                foreach (BakedConfig.MapEntry m in baked.maps) ids.Append(m.id).Append(' ');
                Debug.LogError($"[MatchView] không có map `{wanted}`. Đã bake: {ids}");
                MapSession.PickedMapId = null;
                enabled = false;
                return;
            }

            GameConfig cfg = ConfigMapper.Map(baked.towersJson, baked.enemiesJson,
                                              baked.wavesJson, baked.economyJson, mapJson);
            Debug.Log($"[MatchView] map `{cfg.MapId}` — {cfg.MapDisplayName} · "
                      + $"{cfg.Path.Slots.Count} ô · khởi đầu {cfg.Economy.StartingCash} Peso");

            if (ShowPerfHud)
            {
                var perf = gameObject.AddComponent<PerfHud>();
                perf.LiveEnemies = () => _match?.Enemies.Count ?? 0;
            }
            _def = cfg.Path;

            // 🔴 TIỀN TEST — KHÔNG phải số thiết kế.
            //
            // 🔵 VÒNG 22 — comment cũ ở đây đã SAI và được viết lại. Nó nói
            // "economy.json giữ startingCash: 550, thiếu đúng 30 Peso để mua La Pulga
            // (300) + D10S (280)". Cả ba con số đều đã đổi: startingCash = 700 (đo từ
            // engine, xem economy.json → _startingCashNote) và D10S = 240.
            //
            // Bất biến CÒN SỐNG là: không ai lên thẳng CẤP MẠNH NHẤT ngay từ đầu —
            // La Pulga full = 300 + 240 + 480 = 1020 > 700. Test
            // `Khong_mua_duoc_cap_manh_nhat_ngay_tu_dau_la_CO_Y` canh đúng bất biến đó.
            //
            // Vì sao tiền test sống Ở ĐÂY chứ không nhét vào config: nhét vào config
            // sẽ (a) phá bất biến trên, (b) làm balance_sim đọc nhầm → bảng W1-W20 mô
            // tả một trận người chơi giàu hơn nhiều lần thực tế. Ở đây thì nhìn thấy
            // được, và mô hình cân bằng không bao giờ biết tới nó.
            //
            // ⚠️ ĐANG BẬT: 950 → Play chạy với 700 + 950 = 1650 Peso. Muốn cảm nhận
            // ĐÚNG cân bằng đã chỉnh thì đặt về 0 (Inspector hoặc dòng khai báo trên).
            //
            // ĐẶT VỀ 0 TRƯỚC KHI PHÁT HÀNH.
            if (TestExtraCash > 0)
            {
                cfg.Economy.StartingCash += TestExtraCash;
                Debug.LogWarning($"[MatchView] TIỀN TEST +{TestExtraCash} → " +
                                 $"{cfg.Economy.StartingCash}. Cân bằng KHÔNG còn đúng.");
            }

            _mapId = cfg.MapId;

            // Map kế tiếp = phần tử ngay sau map này khi xếp theo `order`.
            var ordered = new System.Collections.Generic.List<BakedConfig.MapEntry>(baked.maps);
            ordered.Sort((a, b) => a.order.CompareTo(b.order));
            for (int i = 0; i < ordered.Count - 1; i++)
                if (ordered[i].id == _mapId) { _nextMapId = ordered[i + 1].id; break; }

            // Nhãn map cho HUD. SỐ lấy từ ĐÚNG chỗ màn chọn map lấy — vị trí trong
            // danh sách đã xếp theo `order`, không phải cắt chữ số từ `_mapId`. Hai
            // nguồn khác nhau là hai nguồn sẽ lệch nhau: `ConfigBaker.cs:66` gán
            // `order = entries.Count` (thứ tự TÊN FILE) và hôm nay nó trùng với số
            // trong tên file chỉ vì may mắn đặt tên m00…m10.
            for (int i = 0; i < ordered.Count; i++)
                if (ordered[i].id == _mapId)
                {
                    _mapLabel = $"MAP {i + 1:00}  ·  {cfg.MapDisplayName.ToUpperInvariant()}";
                    break;
                }

            _match = new MatchController(cfg);

            // DEBUG xem hoạt ảnh: mọi tướng đánh nhanh hơn ×AttackRateDebug. Đặt về
            // 1f là tắt hẳn (trả nhịp gốc). KHÔNG phải cân bằng — chỉ để quan sát.
            AbilityEngine.AttackRateDebugMultiplier = AttackRateDebug;

            SetupCamera();
            DrawStatic(cfg);

            // Âm thanh dựng TRƯỚC khi nối event: handler bên dưới gọi thẳng
            // `AudioService.Play`, có sẵn Instance thì tiếng của wave 1 không mất.
            gameObject.AddComponent<AudioService>();

            _match.ShotFired += OnShotFired;
            _match.Damage.EnemyKilled += OnKilled;
            _match.Threw += OnThrew;
            _match.Saved += OnSaved;
            _match.AbilityFired += OnAbilityFired;
            _match.CardShown += OnCardShown;
            _match.WaveStarted += _ => AudioService.Play("wave_start", 0.7f);
            _match.WaveCleared += _ => AudioService.Play("wave_clear", 0.8f);
            _match.Goal.GoalDamaged += _ => AudioService.Play("goal_hit", 0.9f);

            _adGateway = AdMobRewardedAds.Instance;
            if (_adGateway != null)
            {
                _rewardedAds = new RewardedAdCoordinator(_adGateway);
                _adGateway.PresentationClosed += OnRewardedAdClosed;
            }

            _hud = gameObject.AddComponent<Hud>();
            _hud.Bind(_match, _def, this);

            Debug.Log($"[MatchView] sẵn sàng · {_def.Slots.Count} ô · {cfg.Waves.Count} wave");
        }

        /// <summary>
        /// 🔴 CHỨA TRỌN SÂN — khớp theo chiều CHẬT hơn, không cố định một chiều.
        ///
        /// path.json thiết kế 10.8 × 19.2 unit = 9:16. Hỏng được theo hai hướng
        /// đối xứng nhau, và ta đã dính cả hai:
        ///
        ///   • Khớp cứng chiều CAO (`cameraOrthographicSize: 9.6` đặt thẳng):
        ///     iPhone đời mới 19.5:9 bóp chiều ngang còn ~8.9 → ô rìa (f02, f05)
        ///     văng ra ngoài màn. Đã đo trên iPhone 12 thật ở vòng 8.
        ///   • Khớp cứng chiều NGANG (cách sửa vòng 8): màn RỘNG hơn 9:16 —
        ///     Game view ngang trong Editor, iPad 3:4 — cắt cụt trên/dưới. Ở
        ///     Game view 16:9 chỉ còn 6.1/19.2 unit = 32% sân, thấy mỗi dải giữa.
        ///
        /// Lấy max của hai yêu cầu = trọn 10.8 × 19.2 luôn nằm trong khung, chiều
        /// nào dư thì tràn ra thành viền cỏ. Không tỉ lệ nào cắt mất sân nữa.
        /// </summary>
        private void SetupCamera()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                _cam = go.AddComponent<Camera>();
            }
            _content = ComputeContentBounds();
            _cam.orthographic = true;
            _cam.transform.position = new Vector3(_content.center.x, _content.center.y, -10f);
            _cam.backgroundColor = Pitch.Stands;
            FitCamera();
        }

        /// <summary>
        /// Hộp bao MỌI thứ được vẽ ra, KHÔNG phải hộp thiết kế khai báo.
        ///
        /// path.json khai 10.8 × 19.2, nhưng khung thành nằm tới y = -10.9
        /// (waypoint cuối -9.13, hạ thêm 1.2, cộng nửa chiều cao net + nét vẽ).
        /// Khớp camera theo con số khai báo = cắt cụt cầu môn — đúng lỗi vòng này.
        ///
        /// Đo cái thật sự vẽ ra thì thêm ô, dời net hay đổi path đều tự lọt khung,
        /// không ai phải nhớ đi chỉnh camera. Hộp thiết kế vẫn được gộp vào làm SÀN:
        /// không bao giờ đóng khung chật hơn vùng chơi đã khai báo.
        /// </summary>
        private Bounds ComputeContentBounds()
        {
            var b = new Bounds(Vector3.zero,
                new Vector3((float)_def.ViewportWidthUnits,
                            (float)_def.ViewportHeightUnits, 0f));

            foreach (LaneDef lane in _def.Lanes)
                foreach (Vec2 w in lane.Waypoints)
                    b.Encapsulate(new Bounds(new Vector3((float)w.X, (float)w.Y, 0f),
                                             new Vector3(PathLineWidth, PathLineWidth, 0f)));

            float touch = (float)_def.MinTouchTargetUnits;
            foreach (SlotDef s in _def.Slots)
            {
                var c = new Vector3((float)s.Position.X, (float)s.Position.Y, 0f);
                b.Encapsulate(new Bounds(c, new Vector3(touch, touch, 0f)));
            }

            foreach (LaneDef lane in _def.Lanes)
            {
                Vec2 g = lane.GoalPoint;
                var net = new Vector3((float)g.X, (float)g.Y - GoalDropUnits, 0f);
                b.Encapsulate(new Bounds(net, new Vector3(GoalWidthUnits + GoalLineWidth,
                                                          GoalHeightUnits + GoalLineWidth, 0f)));
            }

            b.Expand(new Vector3(FrameMarginUnits * 2f, FrameMarginUnits * 2f, 0f));
            return b;
        }

        // Kích thước màn ở lần khớp gần nhất. Kéo cửa sổ Game view hay xoay máy
        // đổi tỉ lệ giữa chừng; khớp một lần ở Start là lại cụt sân.
        private int _fitW, _fitH;

        /// <summary>Đặt orthographicSize sao cho CẢ 10.8 lẫn 19.2 unit đều lọt khung.</summary>
        private void FitCamera()
        {
            _fitW = Screen.width;
            _fitH = Screen.height;
            float aspect = _fitW / (float)_fitH;
            _cam.orthographicSize = Mathf.Max(_content.extents.y, _content.extents.x / aspect);
        }

        private EnemyPath _pathGeo = null!;

        /// <summary>Ô này cách đường chạy bao xa. Tướng có tầm nhỏ hơn con số này
        /// thì KHÔNG BAO GIỜ bắn được ai từ ô đó.</summary>
        /// <summary>
        /// Khoảng cách từ ô tới TUYẾN GẦN NHẤT.
        ///
        /// 🔴 PHẢI gộp mọi tuyến. `Hud` dùng hàm này để KHOÁ nút mua khi tướng không
        /// với tới đường ("không với tới = không bấm được" — sửa tận gốc ba lần người
        /// chơi báo 'tướng không gây dame'). Nếu chỉ đo tuyến đầu thì trên map hai
        /// tuyến, ô phục vụ tuyến B bị coi là ngoài tầm và người chơi KHÔNG MUA ĐƯỢC
        /// GÌ ở đó — cùng lớp lỗi "ô chết giả" mà `path_check` vừa phải sửa.
        /// </summary>
        internal double DistanceToPath(Vec2 slot)
        {
            if (_laneGeo.Count == 0) return _pathGeo.DistanceToPath(slot);
            double best = double.MaxValue;
            foreach (EnemyPath ln in _laneGeo)
            {
                double d = ln.DistanceToPath(slot);
                if (d < best) best = d;
            }
            return best;
        }

        /// <summary>Hình học MỌI tuyến — để vẽ đường và để đo khoảng cách ô→đường.</summary>
        private readonly List<EnemyPath> _laneGeo = new();

        private void DrawStatic(GameConfig cfg)
        {
            // Tuyến ĐẦU giữ làm `_pathGeo` — nó là thứ `DistanceToPath` dùng để trả
            // lời "ô này có với tới đường không". Với map đa tuyến, câu trả lời đúng
            // là khoảng cách tới tuyến GẦN NHẤT; xem `DistanceToPath`.
            var path = new EnemyPath(new List<Vec2>(cfg.Path.Lanes[0].Waypoints));
            _pathGeo = path;

            _laneGeo.Clear();
            foreach (LaneDef ln in cfg.Path.Lanes)
                _laneGeo.Add(new EnemyPath(new List<Vec2>(ln.Waypoints)));

            // Sân cỏ nằm sau tất cả; đường là dải lưới bám spline (xem `Pitch`).
            // Cả hai sinh bằng code nên đổi `path.json` là hình tự khớp lại.
            // Sân vẽ theo HỘP THIẾT KẾ (10.8 × 19.2 quanh gốc), KHÔNG theo `_content`.
            // `_content` bị khung thành kéo dài xuống dưới y = -10.9; lấy nó thì vòng
            // cấm và vạch giữa sân lệch hẳn so với vùng chơi. Vẽ theo hộp thiết kế
            // thì vạch giữa đúng giữa, và khung thành nằm ngoài vạch biên — đúng như
            // bóng đá thật. Phần dư hai bên là khán đài tối.
            Pitch.Field(Vector3.zero, (float)_def.ViewportWidthUnits,
                        (float)_def.ViewportHeightUnits, PitchOrder);
            // Vẽ MỌI tuyến. Map một tuyến = một lời gọi, y như trước.
            foreach (EnemyPath ln in _laneGeo)
                Pitch.Road(ln, PathLineWidth, RoadOrder);

            DrawGoals(cfg);

            foreach (SlotDef s in _def.Slots)
            {
                var pos = new Vector3((float)s.Position.X, (float)s.Position.Y, 0);
                // Vòng ngoài = vùng chạm 48pt THẬT. Nhìn nó chồng nhau trên máy =
                // hai ô quá sát, dù hình học "đúng".
                Draw.Make($"Base_{s.Id}", Pitch.Pedestal(), Color.white, 0.98f, PedestalOrder)
                    .transform.position = pos;
                Draw.Ring($"Touch_{s.Id}", pos, (float)_def.MinTouchTargetUnits / 2f,
                          Draw.Touch, 0.04f, 1);
                _slotRing[s.Id] = Draw.Ring($"Slot_{s.Id}", pos, 0.42f,
                                            s.IsField ? Draw.SlotFree : Draw.SlotKeeper, 0.12f, 2);
            }
        }

        /// <summary>
        /// Vệt đạn. Trước đây KHÔNG vẽ gì cả — tướng bắn vô hình, quân biến mất.
        ///
        /// Batigol Lv1 gây 90 dame vào adepto 69 máu ở W1: đầy máu → một phát →
        /// bốc hơi. Không có khoảnh khắc nào để thấy "tụt máu". Người chơi báo
        /// "không thấy tụt máu, đôi khi quái biến mất luôn" — logic đúng, phản hồi
        /// hình ảnh thì không có.
        /// </summary>
        private void OnShotFired(Vec2 from, Vec2 to, AttackPlan plan)
        {
            // `from` = vị trí tướng bắn (MatchController.cs:422). Khớp vị trí để
            // trigger cú xút đúng tướng — không cần Core gửi kèm SlotId.
            TowerInstance? firedTower = null;
            Animator? firedAnim = null;
            foreach (KeyValuePair<string, Animator> kv in _towerAnim)
            {
                // CHA (go) = điểm ô; art con bị nâng ArtStandOffset nên KHÔNG khớp
                // bằng vị trí art được — phải so vị trí cha với `from`/`at`.
                Vector3 tp = kv.Value.transform.parent.position;
                if (Mathf.Abs(tp.x - (float)from.X) > 0.05f ||
                    Mathf.Abs(tp.y - (float)from.Y) > 0.05f) continue;
                // El Cinco đấm/đá/húc cuồng nộ theo cấp; D10S bóng/chai/cối theo cấp;
                // tướng khác dùng cú xút (Kick).
                firedTower = _match.Slots.At(kv.Key);
                firedAnim = kv.Value;
                int trig = KickHash;
                if (firedTower != null && firedTower.TowerId == "batigol")
                    trig = firedTower.Level >= 3 ? StompHash : firedTower.Level == 2 ? KickHash : PunchHash;
                else if (firedTower != null && firedTower.TowerId == "d10s")
                    trig = firedTower.Level >= 3 ? MortarHash : firedTower.Level == 2 ? BottleHash : BallHash;
                else if (firedTower != null && firedTower.TowerId == "la_pulga")
                    trig = firedTower.Level >= 3 ? VolleyHash : firedTower.Level == 2 ? CurlHash : KickHash;
                FireTrigger(kv.Key, kv.Value, trig, KickHash);
                PlayShotSfx(firedTower);
                // Sheet vẽ mặt sang PHẢI; mục tiêu bên TRÁI thì lật lại.
                if (kv.Value.TryGetComponent(out SpriteRenderer sr)) sr.flipX = to.X < from.X;
                break;
            }

            // Projectile của Pulga là nhận diện cấp, không phụ thuộc AttackShape:
            // Lv3 vẫn dùng cầu lửa cả ở đòn xuyên, thay vì đổi thành vệt Line.
            if (firedTower != null && firedTower.TowerId == "la_pulga")
            {
                SpawnPulgaProjectile(from, to, firedTower.Level);
                return;
            }

            // El Cinco (Splash) — NỔ tại vị trí quái, bán kính = plan.SplashRadius.
            // D10S cũng Splash nhưng NÉM trước (bóng/chai/cối bay from→to), nổ chỉ
            // xảy ra khi đạn TỚI (xem SpawnD10Thrown/TickShots). Đòn xuyên hàng
            // (`so_10`) là TIA thẳng. Còn lại: cầu lửa BAY tới quân.
            if (plan.Shape == AttackShape.Splash)
            {
                if (firedTower != null && firedTower.TowerId == "d10s")
                    SpawnD10Thrown(from, to, firedTower.Level, (float)plan.SplashRadius);
                else if (firedTower != null && firedTower.TowerId == "batigol" &&
                         firedTower.Level >= 3 && firedAnim != null)
                    StartBerserkCharge(firedAnim.transform, to, (float)plan.SplashRadius);
                else
                {
                    SpawnExplosion(to, (float)plan.SplashRadius);
                    AudioService.Play("explode", 0.8f);
                }
            }
            else if (plan.Shape == AttackShape.Line)
            {
                var go = new GameObject("shot");
                var lr = go.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPosition(0, new Vector3((float)from.X, (float)from.Y, -2));
                lr.SetPosition(1, new Vector3((float)to.X, (float)to.Y, -2));
                lr.widthMultiplier = 0.22f;
                lr.sharedMaterial = Draw.UnlitMaterial();
                lr.startColor = lr.endColor = new Color(1f, 0.95f, 0.6f, 0.9f);
                lr.sortingOrder = 8;
                _fx.Add((go, 0.12f, 0.12f));   // vệt sáng ngắn — đủ thấy, không rối mắt
            }
            else
            {
                SpawnFireball(from, to);
            }
        }

        // ── Vụ nổ El Cinco (đấm/đá/đạp) — hoạt ảnh 4 frame từ Art/fire ──────
        private static Sprite[]? _fireFrames;      // 4 frame nổ (trái→phải), nạp 1 lần
        private const float ExplodeLife = 0.50f;   // thời gian chạy hết 4 frame (chậm hơn 1 xíu)
        private const float ExplodeScale = 1f;   // cỡ nổ = radius × cái này (×3 so với 0.4)
        private static readonly Color ExplodeCore = new(1f, 0.86f, 0.42f);   // dự phòng: lõi loé
        private static readonly Color ExplodeRing = new(1f, 0.45f, 0.15f);   // dự phòng: sóng nổ

        // (nổ, tuổi, đời) — chạy 4 sprite lửa theo thời gian.
        private readonly List<(GameObject Go, float Age, float Life)> _explosions = new();

        /// <summary>Nổ tại `at`, cỡ theo `radius` (= splashRadius) → TO DẦN theo cấp
        /// (0.8/1.1/1.4). Thuần hình ảnh — sát thương vùng do core (ApplyPlan Splash) lo.</summary>
        private void SpawnExplosion(Vec2 at, float radius)
        {
            if (_fireFrames == null)
            {
                _fireFrames = Resources.LoadAll<Sprite>("Art/fire");
                System.Array.Sort(_fireFrames, (x, y) => x.rect.x.CompareTo(y.rect.x));
            }

            Vector3 c = new((float)at.X, (float)at.Y, -3f);
            if (_fireFrames.Length >= 1)
            {
                GameObject go = Draw.Make("boom", _fireFrames[0], Color.white, radius * ExplodeScale, 10);
                go.transform.position = c;
                _explosions.Add((go, 0f, ExplodeLife));
            }
            else
            {
                // Dự phòng nếu chưa slice fire.png: nổ vẽ bằng code.
                GameObject flash = Draw.Make("boom", Draw.Circle(), ExplodeCore, radius * 1.3f, 10);
                flash.transform.position = c;
                _fx.Add((flash, 0.18f, 0.18f));
                GameObject ring = Draw.UnitRing("boomRing", ExplodeRing, 0.09f, 9);
                ring.transform.position = c;
                _waves.Add((ring, 0f, 0.45f, radius));
            }
        }

        /// <summary>Chạy hoạt ảnh 4 frame của các vụ nổ đang sống, xong thì huỷ.</summary>
        private void TickExplosions()
        {
            if (_fireFrames == null || _fireFrames.Length == 0) return;
            for (int i = _explosions.Count - 1; i >= 0; i--)
            {
                (GameObject go, float age, float life) = _explosions[i];
                if (go == null) { _explosions.RemoveAt(i); continue; }
                age += Time.deltaTime * SpeedMultiplier;
                if (age >= life) { Destroy(go); _explosions.RemoveAt(i); continue; }
                int idx = Mathf.Clamp((int)(age / life * _fireFrames.Length), 0, _fireFrames.Length - 1);
                if (go.TryGetComponent(out SpriteRenderer sr)) sr.sprite = _fireFrames[idx];
                _explosions[i] = (go, age, life);
            }
        }

        // ── El Cinco Lv3: art rời ô, húc mục tiêu rồi trở về ô gốc ───────────
        // Logic tower VẪN đứng yên ở slot; chỉ transform `art` (con của slot) được
        // di chuyển. Không được đưa parent đi, nếu không lần bắn kế tiếp sẽ không
        // còn khớp được `from` với tower đã bắn trong OnShotFired.
        private const float BerserkChargeTravel = 3f / 12f; // frame 4 chạm mục tiêu, @12fps
        private const float BerserkChargeLife = 4f / 12f;   // hết frame húc → về ô
        private readonly List<(Transform Art, Vector3 Home, Vector3 Target, float Age,
                               float SplashRadius, bool Exploded)> _berserkCharges = new();

        private void StartBerserkCharge(Transform art, Vec2 target, float splashRadius)
        {
            Vector3 home = art.position;
            // Giữ cùng offset đứng của sprite tại slot, để chân ở mục tiêu thay vì
            // tâm sprite chui xuống mặt sân.
            Vector3 at = new((float)target.X, (float)target.Y + ArtStandOffset, home.z);
            _berserkCharges.Add((art, home, at, 0f, splashRadius, false));
        }

        private void TickBerserkCharges()
        {
            float dt = Time.deltaTime * SpeedMultiplier;
            for (int i = _berserkCharges.Count - 1; i >= 0; i--)
            {
                (Transform art, Vector3 home, Vector3 target, float age, float splashRadius, bool exploded)
                    = _berserkCharges[i];
                if (art == null) { _berserkCharges.RemoveAt(i); continue; }

                age += dt;
                float progress = Mathf.Clamp01(age / BerserkChargeTravel);
                art.position = Vector3.LerpUnclamped(home, target, progress * progress * (3f - 2f * progress));

                // Frame húc cuối cùng chạm mục tiêu; FX/sound runtime xuất hiện
                // cùng khoảnh khắc để không cần một frame sprite vụ nổ riêng.
                if (!exploded && age >= BerserkChargeTravel)
                {
                    SpawnExplosion(new Vec2(target.x, target.y - ArtStandOffset), splashRadius);
                    AudioService.Play("explode", 0.8f);
                    exploded = true;
                }

                if (age >= BerserkChargeLife)
                {
                    art.position = home;
                    _berserkCharges.RemoveAt(i);
                    continue;
                }
                _berserkCharges[i] = (art, home, target, age, splashRadius, exploded);
            }
        }

        // ── Cầu lửa bay ─────────────────────────────────────────────────────
        // Viên đạn có CHUYỂN ĐỘNG, khác `_fx` (chỉ fade tại chỗ) → cần danh sách
        // riêng để nội suy vị trí mỗi frame. Tốc độ cố định (không phải thời gian
        // cố định) để phát gần và phát xa trông cùng một "lực xút".
        // DEBUG: nhân nhịp đánh mọi tướng để xem hoạt ảnh rõ hơn. 1f = tắt (nhịp gốc).
        private const float AttackRateDebug = 2.5f;
        private const float FireballSpeed = 6f;   // đơn vị/giây — hạ để nhìn rõ đạn/anim

        // Cỡ sprite cầu lửa — nhân với cỡ import (PPU). Đổi 1 số là đổi cỡ đạn.
        private const float FireballScale = 1.0f;

        // ── Bảng màu lửa (dùng cho chớp va chạm vẽ bằng code) ───────────────
        private static readonly Color FireWhite = new(1.00f, 0.97f, 0.85f);
        private static readonly Color FireOrange = new(1.00f, 0.50f, 0.10f);

        // ── Aura trọng tài (vòng 22) ────────────────────────────────────────
        // Vùng XANH bán kính = tầm (aura nền 25%, cả 3 cấp) + SÓNG lan từ trong ra.
        // Thẻ bay (Lv2/Lv3) mang màu theo cấp: vàng / đỏ.
        private static readonly Color AuraBlue = new(0.35f, 0.65f, 1.00f);
        private const float WaveInterval = 1.1f;    // mỗi 1.1s phát một vòng sóng
        private const float WaveLife = 1.3f;        // sóng lan tới rìa tầm rồi tắt trong 1.3s
        private readonly Dictionary<string, GameObject> _auraGlow = new();

        /// <summary>SpriteRenderer của nền aura — cùng lý do với `_hpBarSr`.</summary>
        private readonly Dictionary<string, SpriteRenderer> _auraGlowSr = new();

        private static readonly Color BarNormal = new(0.2f, 1f, 0.3f);
        private static readonly Color BarSlowed = new(0.4f, 0.7f, 1f);
        private readonly Dictionary<string, float> _waveTimer = new();

        // Sóng đang lan: (ring, tuổi, đời, bán kính đích, màu).
        private readonly List<(GameObject Go, float Age, float Life, float MaxR)> _waves = new();

        // Đạn-thẻ: vàng (Lv2) / đỏ (Lv3).
        private const float CardSpeed = 6f;   // hạ để nhìn rõ thẻ bay
        private static readonly Color CardYellow = new(1.00f, 0.82f, 0.10f);
        private static readonly Color CardRed = new(0.90f, 0.15f, 0.13f);

        private static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }

        private struct Shot
        {
            public GameObject Go;
            public Vector3 From, To;
            public float T, Dur;
            public bool IsCard;    // thẻ trọng tài: xoay nhẹ khi bay, chớp màu thẻ khi tới
            public Color Tint;
            public float SplashRadius; // >0: D10S — nổ (SpawnExplosion) khi tới thay vì chớp thường
        }
        private readonly List<Shot> _shots = new();

        private const float PulgaBallScale = 1f;

        /// <summary>Đạn riêng của Pulga: Lv1 bóng thường, Lv2 bóng vàng, Lv3 cầu lửa.</summary>
        private void SpawnPulgaProjectile(Vec2 from, Vec2 to, int level)
        {
            if (level >= 3)
            {
                SpawnFireball(from, to);
                return;
            }

            Sprite? sprite;
            if (level == 2)
            {
                if (_pulgaGoldBallSprite == null)
                    _pulgaGoldBallSprite = Resources.Load<Sprite>("Art/pulga_ball_gold");
                sprite = _pulgaGoldBallSprite;
            }
            else
            {
                if (_pulgaNormalBallSprite == null)
                    _pulgaNormalBallSprite = Resources.Load<Sprite>("Art/pulga_ball_normal");
                sprite = _pulgaNormalBallSprite;
            }

            if (sprite == null)
            {
                SpawnFireball(from, to);
                return;
            }

            Vector3 a = new((float)from.X, (float)from.Y, -2f);
            Vector3 b = new((float)to.X, (float)to.Y, -2f);
            GameObject ball = Draw.Make(level == 2 ? "goldBall" : "football",
                                        sprite, Color.white, PulgaBallScale, 8);
            ball.transform.position = a;
            if (level == 2)
            {
                float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
                ball.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            float dur = Mathf.Max(0.08f, Vector3.Distance(a, b) / FireballSpeed);
            _shots.Add(new Shot { Go = ball, From = a, To = b, T = 0f, Dur = dur });
        }

        private void SpawnFireball(Vec2 from, Vec2 to)
        {
            if (_ballSprite == null) _ballSprite = Resources.Load<Sprite>("Art/la_pulga_ball");

            Vector3 a = new((float)from.X, (float)from.Y, -2f);
            Vector3 b = new((float)to.X, (float)to.Y, -2f);

            GameObject fb;
            if (_ballSprite != null)
            {
                // Sprite cầu lửa THẬT (đuôi lửa đã vẽ trong ảnh). Pivot đặt ở LÕI khi
                // bake → vị trí GO = tâm lõi; xoay để lõi dẫn hướng bay, đuôi kéo sau.
                fb = Draw.Make("fireball", _ballSprite, Color.white, FireballScale, 8);
                float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
                fb.transform.rotation = Quaternion.Euler(0f, 0f, ang);
            }
            else
            {
                // Đường lui: chưa bake sprite → lõi trắng + quầng cam vẽ bằng code.
                fb = new GameObject("fireball");
                Draw.Make("glow", Draw.Circle(), WithAlpha(FireOrange, 0.5f), 0.60f, 7, fb.transform);
                Draw.Make("core", Draw.Circle(), FireWhite, 0.30f, 8, fb.transform);
            }
            fb.transform.position = a;

            float dur = Mathf.Max(0.08f, Vector3.Distance(a, b) / FireballSpeed);
            _shots.Add(new Shot { Go = fb, From = a, To = b, T = 0f, Dur = dur });
        }

        // Cỡ găng/cúp bay — nhân với cỡ import (ProjPpu). Đổi 1 số là đổi cỡ đạn.
        private const float ProjScale = 1.25f;

        // Hướng "forward" đã VẼ SẴN trong ảnh (độ, hệ toạ độ thế giới): vệt tốc độ
        // của găng/cúp chỉ về phía TRÊN-PHẢI. Ta xoay sprite đi (góc bay − góc này)
        // để forward khớp hướng đạn thật → vệt luôn nằm SAU. Đo tay ~ chỉnh nếu lệch.
        private const float GloveBakedDeg = 25f;
        private const float CupBakedDeg = 58f;

        /// <summary>Ném găng (Lv2) / cúp (Lv3) của Dibu: bay thẳng from→to, XOAY sprite
        /// để hướng vệt (bakedDeg) khớp hướng bay. Thiếu art thì rơi về cầu lửa.</summary>
        private void SpawnThrown(Vec2 from, Vec2 to, Sprite? sprite, float bakedDeg)
        {
            if (sprite == null) { SpawnFireball(from, to); return; }
            Vector3 a = new((float)from.X, (float)from.Y, -2f);
            Vector3 b = new((float)to.X, (float)to.Y, -2f);
            GameObject go = Draw.Make("throw", sprite, Color.white, ProjScale, 8);
            go.transform.position = a;
            float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0f, 0f, ang - bakedDeg);
            float dur = Mathf.Max(0.08f, Vector3.Distance(a, b) / FireballSpeed);
            _shots.Add(new Shot { Go = go, From = a, To = b, T = 0f, Dur = dur });
        }

        // ── D10S ném theo cấp (Lv1 bóng · Lv2 chai · Lv3 cối) ───────────────
        // Bay from→to như cầu lửa/Dibu, nhưng khi TỚI thì NỔ (SpawnExplosion,
        // qua Shot.SplashRadius trong TickShots) thay vì chớp va chạm thường —
        // đòn đánh D10S vốn đã lan theo splashRadius, đây chỉ là lớp ném phủ
        // lên trên, KHÔNG đổi damage/radius/balance. Cối CHƯA có sprite viên đạn
        // riêng trong sheet (frame bắn chỉ có chớp lửa nòng, không có khung đạn
        // bay tách rời) → vẽ tạm viên đạn bằng code, tô màu quân sự.
        private const float BottleBakedDeg = 25f;    // góc trục chai đã vẽ sẵn trong ảnh (đo tay, chỉnh nếu lệch)
        private static readonly Color MortarShellColor = new(0.30f, 0.34f, 0.20f);

        private void SpawnD10Thrown(Vec2 from, Vec2 to, int level, float splashRadius)
        {
            Vector3 a = new((float)from.X, (float)from.Y, -2f);
            Vector3 b = new((float)to.X, (float)to.Y, -2f);
            float ang = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;

            GameObject go;
            if (level >= 3)
            {
                go = Draw.Make("mortarShell", Draw.Circle(), MortarShellColor, 0.22f, 8);
            }
            else if (level == 2)
            {
                if (_bottleSprite == null) _bottleSprite = Resources.Load<Sprite>("Art/d10s_bottle");
                if (_bottleSprite == null) { SpawnExplosion(to, splashRadius); return; }
                go = Draw.Make("d10Bottle", _bottleSprite, Color.white, ProjScale, 8);
                go.transform.rotation = Quaternion.Euler(0f, 0f, ang - BottleBakedDeg);
            }
            else
            {
                if (_ballSprite == null) _ballSprite = Resources.Load<Sprite>("Art/la_pulga_ball");
                if (_ballSprite == null) { SpawnExplosion(to, splashRadius); return; }
                go = Draw.Make("d10Ball", _ballSprite, Color.white, FireballScale, 8);
                go.transform.rotation = Quaternion.Euler(0f, 0f, ang);
            }

            go.transform.position = a;
            float dur = Mathf.Max(0.08f, Vector3.Distance(a, b) / FireballSpeed);
            _shots.Add(new Shot { Go = go, From = a, To = b, T = 0f, Dur = dur, SplashRadius = splashRadius });
        }

        private void TickShots()
        {
            for (int i = _shots.Count - 1; i >= 0; i--)
            {
                Shot s = _shots[i];
                if (s.Go == null) { _shots.RemoveAt(i); continue; }

                s.T += Time.deltaTime * SpeedMultiplier;   // theo tốc độ trận (×2)
                if (s.T >= s.Dur)
                {
                    if (s.SplashRadius > 0f) SpawnExplosion(new Vec2(s.To.x, s.To.y), s.SplashRadius);
                    else if (s.IsCard) SpawnCardImpact(s.To, s.Tint);
                    else SpawnImpact(s.To);
                    Destroy(s.Go);
                    _shots.RemoveAt(i);
                    continue;
                }

                s.Go.transform.position = Vector3.Lerp(s.From, s.To, s.T / s.Dur);
                if (s.IsCard)   // thẻ xoay nhẹ khi bay cho có sức sống
                    s.Go.transform.Rotate(0f, 0f, 220f * Time.deltaTime * SpeedMultiplier);
                _shots[i] = s;
            }
        }

        /// <summary>Chớp lửa ở điểm trúng — người chơi thấy cú xút CHẠM quân, không
        /// phải viên đạn tự biến mất giữa trời. Lõi trắng nóng loé rồi phai, vòng
        /// cam giãn ra: đọc ra "va chạm nóng", không phải chấm tĩnh.</summary>
        private void SpawnImpact(Vector3 at)
        {
            // Lõi loé — sprite tròn trắng, phóng to nhanh rồi phai (nhờ TickFx).
            var flash = Draw.Make("hitFlash", Draw.Circle(), FireWhite, 0.45f, 11);
            flash.transform.position = at;
            _fx.Add((flash, 0.14f, 0.14f));

            // Vòng cam giãn ra — Ring dùng useWorldSpace, TickFx chỉ fade nên giữ
            // cỡ ban đầu; đặt hẹp để thành quầng nổ mảnh, không phình quá to.
            LineRenderer ring = Draw.Ring("hit", at, 0.14f,
                WithAlpha(FireOrange, 0.9f), 0.10f, 9);
            _fx.Add((ring.gameObject, 0.22f, 0.22f));
        }

        /// <summary>Một tấm thẻ (viền trắng + mặt màu) bay từ trọng tài tới con bị
        /// phạt. Dùng lại cơ chế `_shots` như cầu lửa, chỉ khác cờ IsCard.</summary>
        private void SpawnCard(Vec2 from, Vec2 to, Color face)
        {
            Vector3 a = new((float)from.X, (float)from.Y, -2f);
            Vector3 b = new((float)to.X, (float)to.Y, -2f);

            var card = new GameObject("card");
            GameObject edge = Draw.Make("edge", Draw.Square(), Color.white, 1f, 9, card.transform);
            edge.transform.localScale = new Vector3(0.40f, 0.56f, 1f);
            GameObject faceGo = Draw.Make("face", Draw.Square(), face, 1f, 10, card.transform);
            faceGo.transform.localScale = new Vector3(0.30f, 0.46f, 1f);
            card.transform.position = a;

            float dur = Mathf.Max(0.08f, Vector3.Distance(a, b) / CardSpeed);
            _shots.Add(new Shot { Go = card, From = a, To = b, T = 0f, Dur = dur,
                                  IsCard = true, Tint = face });
        }

        /// <summary>Chớp màu thẻ ở con bị phạt — đọc ra "vừa lãnh thẻ", không lẫn với
        /// chớp lửa cam của cú sút.</summary>
        private void SpawnCardImpact(Vector3 at, Color tint)
        {
            var flash = Draw.Make("cardHit", Draw.Circle(), WithAlpha(tint, 0.95f), 0.5f, 11);
            flash.transform.position = at;
            _fx.Add((flash, 0.25f, 0.25f));
        }

        /// <summary>
        /// Xác mờ dần. `03` §6: "Anim ngã 0.4s rồi huỷ", boss "1.2s + rung màn hình".
        /// Trước đây `Destroy()` tức thì → quân biến mất không dấu vết.
        /// </summary>
        private void OnKilled(KillInfo k)
        {
            AudioService.Play("enemy_die", 0.55f);
            if (!_enemyGo.TryGetValue(k.EnemyId, out GameObject? go)) return;

            // Tiền rơi: chỉ khi có tướng ghi công (KillerSlotId != null → đã cộng
            // bounty). Số tiền đọc từ entry MỚI NHẤT của sổ cái — core cộng tiền
            // ngay trước handler này (đăng ký trước), nên entry cuối chính là cú này.
            if (k.KillerSlotId != null)
            {
                IReadOnlyList<CashEntry> led = _match.Economy.Ledger;
                CashEntry last = led[led.Count - 1];
                if ((last.Reason == CashReason.Kill || last.Reason == CashReason.BossKill) && last.Delta > 0)
                    SpawnMoneyPop(go.transform.position, last.Delta);
            }

            _enemyGo.Remove(k.EnemyId);
            // Thanh máu là CON của GO quái + có SpriteRenderer riêng → _fx chỉ mờ
            // sprite quái, KHÔNG mờ thanh máu → trơ thanh máu trên xác. Huỷ nó ngay.
            if (_hpBar.TryGetValue(k.EnemyId, out Transform? bar) && bar != null) Destroy(bar.gameObject);
            _hpBar.Remove(k.EnemyId);
            go.name = "dead";
            float dur = go.transform.localScale.x > 0.7f ? 1.2f : 0.4f;   // boss to hơn
            _fx.Add((go, dur, dur));
        }

        /// <summary>
        /// Dibu cản thành công — vòng sáng bung ra ở khung thành. Đây là khoảnh khắc
        /// quan trọng nhất trong trận (thứ đứng giữa người chơi và thua cuộc), và nó
        /// là XÁC SUẤT — người chơi phải THẤY nó xảy ra, không chỉ thấy quân biến mất.
        /// </summary>
        /// <summary>Dibu NÉM (mỗi nhịp, bất kể trúng/trượt): tung anim ném theo cấp
        /// + phóng đạn Dibu→quân. Con đó CÓ CHẾT hay không do <see cref="OnSaved"/>
        /// xử lý riêng — trượt thì đạn vẫn bay, quân vẫn sống.</summary>
        private void OnThrew(int enemyId, Vec2 at)
        {
            AudioService.Play("throw", 0.7f);

            // Vị trí quân trên màn để đạn bay tới đúng chỗ.
            Vec2 to = at;
            if (_enemyGo.TryGetValue(enemyId, out GameObject? go))
            {
                Vector3 p = go.transform.position;
                to = new Vec2(p.x, p.y);
            }

            // Cấp Dibu quyết định đòn ném: Lv1 bóng · Lv2 găng · Lv3 cúp.
            int level = DibuLevel();

            // Kích anim ném đúng tướng Dibu (khớp vị trí ô) + lật mặt về phía quân.
            foreach (KeyValuePair<string, Animator> kv in _towerAnim)
            {
                // CHA (go) = điểm ô; art con bị nâng ArtStandOffset nên KHÔNG khớp
                // bằng vị trí art được — phải so vị trí cha với `from`/`at`.
                Vector3 tp = kv.Value.transform.parent.position;
                if (Mathf.Abs(tp.x - (float)at.X) > 0.05f ||
                    Mathf.Abs(tp.y - (float)at.Y) > 0.05f) continue;
                FireTrigger(kv.Key, kv.Value,
                            level >= 3 ? CupHash : level == 2 ? GloveHash : BallHash, BallHash);
                // Sheet mặt sang PHẢI; quái bên TRÁI thì lật.
                if (kv.Value.TryGetComponent(out SpriteRenderer sr)) sr.flipX = to.X < at.X;
                break;
            }

            // Đạn bay từ Dibu tới quân — projectile theo cấp.
            if (level >= 3)
            {
                if (_cupSprite == null) _cupSprite = Resources.Load<Sprite>("Art/dibu_cup");
                SpawnThrown(at, to, _cupSprite, CupBakedDeg);
            }
            else if (level == 2)
            {
                if (_gloveSprite == null) _gloveSprite = Resources.Load<Sprite>("Art/dibu_glove");
                SpawnThrown(at, to, _gloveSprite, GloveBakedDeg);
            }
            else SpawnFireball(at, to);
        }

        /// <summary>Dibu CẢN THÀNH CÔNG (đạn trúng + trúng xác suất chết): xoá quân +
        /// vòng "CẢN!" ở chỗ quân biến mất. Anim/đạn đã tung ở <see cref="OnThrew"/>.</summary>
        private void OnSaved(int enemyId, Vec2 at)
        {
            AudioService.Play("save", 0.85f);

            Vector3 ringPos = new((float)at.X, (float)at.Y, -2f);
            if (_enemyGo.TryGetValue(enemyId, out GameObject? go))
            {
                ringPos = go.transform.position; ringPos.z = -2f;
                _enemyGo.Remove(enemyId);
                // Huỷ thanh máu (con) ngay, kẻo _fx mờ xác mà thanh máu còn trơ lại.
                if (_hpBar.TryGetValue(enemyId, out Transform? bar) && bar != null) Destroy(bar.gameObject);
                _hpBar.Remove(enemyId);
            _hpBarSr.Remove(enemyId);
                _fx.Add((go, 0.35f, 0.35f));   // mờ dần trong khi đạn bay tới
            }

            // Vòng sáng "CẢN!" ngay chỗ quân biến mất — khoảnh khắc mấu chốt phải THẤY.
            LineRenderer ring = Draw.Ring("save", ringPos, 0.5f, new Color(1f, 0.9f, 0.2f), 0.16f, 9);
            _fx.Add((ring.gameObject, 0.5f, 0.5f));
        }

        /// <summary>Cấp Dibu hiện tại (chỉ 1 thực thể). Không thấy → coi như Lv1.</summary>
        private int DibuLevel()
        {
            foreach (TowerInstance t in _match.Slots.Towers)
                if (t.TowerId == "dibu") return t.Level;
            return 1;
        }

        /// <summary>Kỹ năng nổ — vòng sóng lan ra bằng đúng TẦM của tướng. Người chơi
        /// thấy chính xác vùng bị ảnh hưởng, thay vì đoán.</summary>
        private void OnAbilityFired(string abilityId, Vec2 at, double range)
        {
            // Thẻ trọng tài có hình ảnh riêng (aura xanh + sóng + thẻ bay ở
            // OnCardShown) → không vẽ thêm vòng tầm kẻo rối.
            if (abilityId == "the_vang" || abilityId == "the_do") return;

            Color c = abilityId switch
            {
                "ban_tay_cua_chua" => new Color(0.60f, 0.45f, 0.95f),   // D10S — tím
                "solo_run" => new Color(0.45f, 0.80f, 1.00f),           // La Pulga
                _ => Color.white,
            };
            LineRenderer ring = Draw.Ring($"fx_{abilityId}", new Vector3((float)at.X, (float)at.Y, -2),
                                          (float)range, c, 0.12f, 9);
            _fx.Add((ring.gameObject, 0.45f, 0.45f));
        }

        /// <summary>Trọng tài ném thẻ vào 1 con (Lv2/Lv3): giơ thẻ (anim) + phóng thẻ
        /// bay tới đúng con đó, màu theo cấp (vàng/đỏ). Con trúng chậm 45/65 (core lo);
        /// đây chỉ là hình ảnh. Aura nền 25% + sóng vẽ ở SyncArbitro.</summary>
        private void OnCardShown(string color, Vec2 from, Vec2 to)
        {
            bool red = color == "red";
            AudioService.Play("card", 0.8f);

            foreach (KeyValuePair<string, Animator> kv in _towerAnim)
            {
                // CHA (go) = điểm ô; art con bị nâng ArtStandOffset nên KHÔNG khớp
                // bằng vị trí art được — phải so vị trí cha với `from`/`at`.
                Vector3 tp = kv.Value.transform.parent.position;
                if (Mathf.Abs(tp.x - (float)from.X) > 0.05f ||
                    Mathf.Abs(tp.y - (float)from.Y) > 0.05f) continue;
                FireTrigger(kv.Key, kv.Value, red ? RedHash : YellowHash, YellowHash);
                // Sheet mặt sang PHẢI; mục tiêu bên TRÁI thì lật.
                if (kv.Value.TryGetComponent(out SpriteRenderer sr)) sr.flipX = to.X < from.X;
                break;
            }

            SpawnCard(from, to, red ? CardRed : CardYellow);
        }

        // ── Tiền rơi khi quái chết (view-only) ──────────────────────────────
        // Cụm xu vàng bung lên rồi rơi theo trọng lực, mờ dần ở cuối. Càng nhiều
        // tiền càng nhiều xu (giới hạn). Đọc bounty từ sổ cái Economy — không đụng core.
        private static readonly Color MoneyGold = new(1f, 0.84f, 0.25f);
        private const float CoinGravity = 9f;        // đơn vị/giây² — kéo xu rơi xuống

        // (xu, vận tốc, tuổi, đời).
        private readonly List<(GameObject Go, Vector2 Vel, float Age, float Life)> _coins = new();

        /// <summary>Bung cụm xu vàng ở chỗ quái chết. Thuần hình ảnh.</summary>
        private void SpawnMoneyPop(Vector3 at, int amount)
        {
            at.z = -3f;
            int n = Mathf.Clamp(2 + amount / 8, 2, 7);
            for (int i = 0; i < n; i++)
            {
                GameObject coin = Draw.Make("coin", Draw.Circle(), MoneyGold, 0.22f, 11);
                coin.transform.position = at;
                Vector2 vel = new(Random.Range(-1.6f, 1.6f), Random.Range(2.6f, 3.8f));
                _coins.Add((coin, vel, 0f, Random.Range(0.6f, 0.85f)));
            }
        }

        private void TickCoins()
        {
            float dt = Time.deltaTime * SpeedMultiplier;
            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                (GameObject go, Vector2 vel, float age, float life) = _coins[i];
                if (go == null) { _coins.RemoveAt(i); continue; }
                age += dt;
                if (age >= life) { Destroy(go); _coins.RemoveAt(i); continue; }
                vel.y -= CoinGravity * dt;
                go.transform.position += new Vector3(vel.x, vel.y, 0f) * dt;
                float k = age / life;                       // mờ dần ở 40% cuối
                float a = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
                if (go.TryGetComponent(out SpriteRenderer sr)) sr.color = WithAlpha(MoneyGold, a);
                _coins[i] = (go, vel, age, life);
            }
        }

        /// <summary>Hiệu ứng đang mờ dần: (object, còn lại, tổng).</summary>
        private readonly List<(GameObject Go, float Left, float Total)> _fx = new();

        private void TickFx()
        {
            for (int i = _fx.Count - 1; i >= 0; i--)
            {
                (GameObject go, float left, float total) = _fx[i];
                left -= Time.deltaTime;
                if (left <= 0 || go == null) { if (go != null) Destroy(go); _fx.RemoveAt(i); continue; }

                float a = left / total;
                if (go.TryGetComponent(out SpriteRenderer sr))
                {
                    Color c = sr.color; c.a = a; sr.color = c;
                    go.transform.localScale *= 1f - Time.deltaTime * 1.2f;   // ngã: co lại
                }
                else if (go.TryGetComponent(out LineRenderer lr))
                {
                    Color c = lr.startColor; c.a = a; lr.startColor = lr.endColor = c;
                }
                _fx[i] = (go, left, total);
            }
        }

        /// <summary>
        /// Khung thành ở cuối đường. Trước đây KHÔNG vẽ gì — đường chạy chỉ… hết.
        /// Người chơi không thấy quân đi vào đâu, cũng không thấy Dibu (gk01 ở
        /// y=-6.9) đang chắn cái gì. Máu cầu môn là điều kiện THUA của cả trận mà
        /// nó vô hình trên sân.
        /// </summary>
        private void DrawGoals(GameConfig cfg)
        {
            var unique = new List<Vec2>();
            foreach (LaneDef lane in cfg.Path.Lanes)
            {
                bool seen = false;
                foreach (Vec2 goal in unique)
                    if (Vec2.Distance(goal, lane.GoalPoint) < 0.05) { seen = true; break; }
                if (!seen) unique.Add(lane.GoalPoint);
            }

            Vector3 labelCentre = Vector3.zero;
            for (int i = 0; i < unique.Count; i++)
                labelCentre += DrawGoalAt(unique[i], i);
            labelCentre /= unique.Count;
            _goalWorld = labelCentre + new Vector3(0, GoalHeightUnits / 2 + 0.35f, 0);
        }

        private static Vector3 DrawGoalAt(Vec2 g, int index)
        {
            // Hạ net xuống dưới vạch cầu môn (offset 1.2 unit) cho tách khỏi cụm ô đáy
            // và đỡ chật. Thuần hình ảnh — KHÔNG đụng toạ độ đường hay cân bằng. Điểm
            // quân kết thúc vẫn ở waypoint cuối (-7.8); net chỉ là hình vẽ khung thành.
            var c = new Vector3((float)g.X, (float)g.Y - GoalDropUnits, 0);

            const float w = GoalWidthUnits, h = GoalHeightUnits;
            var net = new GameObject($"Goal_{index + 1}");
            var lr = net.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 4;
            lr.SetPosition(0, c + new Vector3(-w / 2, +h / 2, 0));
            lr.SetPosition(1, c + new Vector3(+w / 2, +h / 2, 0));
            lr.SetPosition(2, c + new Vector3(+w / 2, -h / 2, 0));
            lr.SetPosition(3, c + new Vector3(-w / 2, -h / 2, 0));
            lr.widthMultiplier = GoalLineWidth;
            lr.sharedMaterial = Draw.UnlitMaterial();
            lr.startColor = lr.endColor = Color.white;
            lr.sortingOrder = 2;

            // Lưới: các sợi dọc. Đủ để đọc ra "đây là khung thành", không cần art.
            for (int i = 1; i < 6; i++)
            {
                float x = -w / 2 + w * i / 6f;
                var n = new GameObject($"Goal_{index + 1}_net{i}");
                var nl = n.AddComponent<LineRenderer>();
                nl.useWorldSpace = true;
                nl.positionCount = 2;
                nl.SetPosition(0, c + new Vector3(x, +h / 2, 0));
                nl.SetPosition(1, c + new Vector3(x, -h / 2, 0));
                nl.widthMultiplier = 0.03f;
                nl.sharedMaterial = Draw.UnlitMaterial();
                nl.startColor = nl.endColor = new Color(1f, 1f, 1f, 0.4f);
                nl.sortingOrder = 1;
            }
            return c;
        }

        /// <summary>Chỗ Hud vẽ số máu cầu môn — ngay trên khung thành.</summary>
        internal Vector3 GoalLabelWorld => _goalWorld;
        private Vector3 _goalWorld;

        private void Update()
        {
            if (_match == null) return;
            // Xoay máy vẫn phải đóng khung lại kể cả lúc dừng — nếu không, mở lại
            // menu ở chiều ngang là thấy sân bị cắt.
            if (Screen.width != _fitW || Screen.height != _fitH) FitCamera();

            // 🔴 TẠM DỪNG = TRẢ VỀ SỚM, KHÔNG PHẢI `Time.timeScale = 0`.
            //
            // Tốc độ ×2 nhân vào tham số của `Tick` chứ không vào timeScale (xem
            // `SpeedMultiplier`). Nếu pause đi qua timeScale thì hai cơ chế cùng
            // điều khiển một đồng hồ, và bug kinh điển là thoát pause lúc đang ×2
            // thì timeScale bị trả về 1 trong khi HUD vẫn sáng chip ×2.
            //
            // Trả về ở đây đóng băng ĐỦ BỐN thứ, không chỉ luật chơi: `_match.Tick`
            // (trận), `TickFx/TickShots/TickExplosions` (hoạt ảnh + đạn đang bay),
            // `TickWaves/TickCoins` (nhãn nổi), và `HandleTouch` — cái cuối là lý do
            // chạm vào sân sau lưng overlay không chọn nhầm ô.
            if (Paused) return;

            // Trận vừa kết thúc → ghi sao MỘT LẦN. Đặt ở đây chứ không ở lớp vẽ:
            // màn kết thúc được vẽ lại mỗi frame, ghi trong đó là ghi hàng trăm lần.
            // Không khoá cờ ở Lost: quảng cáo continue có thể đưa trận về Fighting,
            // và chiến thắng sau đó vẫn phải được ghi sao/mở map.
            if (!_resultSaved && _match.Phase == MatchPhase.Won)
            {
                _resultSaved = true;
                MapProgress.Record(_mapId, (int)_match.Rating);
            }

            TickFx();
            TickShots();
            TickWaves();
            TickCoins();
            TickExplosions();
            TickBerserkCharges();

            _match.Tick(Time.deltaTime * SpeedMultiplier);
            SyncEnemies();
            SyncTowers();
            HandleTouch();
        }

        // ── Vẽ ──────────────────────────────────────────────────────────────

        private void SyncEnemies()
        {
            foreach (Enemy e in _match.Enemies)
            {
                if (!_enemyGo.TryGetValue(e.Id, out GameObject? go))
                {
                    go = MakeEnemyVisual(e);
                    _enemyGo[e.Id] = go;
                }

                go.transform.position = new Vector3((float)e.Position.X, (float)e.Position.Y, -1);
                FaceTravel(e);

                // Thanh máu co theo % — và ĐỔI MÀU khi bị chậm, vì `docs/06` cảnh báo
                // bảng màu đã hết chỗ nên trạng thái phải đọc được qua thứ khác.
                float f = Mathf.Clamp01((float)(e.Hp / e.MaxHp));
                Vector2 g = _barGeom[e.Id];
                Transform b = _hpBar[e.Id];
                b.localScale = new Vector3(g.x * f, BarHeight, 1f);
                b.localPosition = new Vector3(-g.x * (1 - f) / 2f, g.y, 0);
                if (_hpBarSr.TryGetValue(e.Id, out SpriteRenderer? barSr) && barSr != null)
                    barSr.color = e.CurrentSpeed < e.BaseSpeed ? BarSlowed : BarNormal;
            }

            // Quân đã chết/lọt lưới → core đã gỡ khỏi list, vỏ phải theo.
            //
            // Dùng buffer dùng lại thay vì LINQ: bản đầu cấp phát một HashSet + hai
            // List MỖI KHI có con chết. D10S đánh lan giết nhiều con cùng lúc, nên
            // nó ép chỗ này chạy liên tục → GC gom rác giữa trận → GIẬT HÌNH. Người
            // chơi báo đúng hiện tượng đó, và chỉ với màu tím (D10S là tướng lan duy nhất).
            if (_enemyGo.Count == _match.Enemies.Count) return;

            _live.Clear();
            for (int i = 0; i < _match.Enemies.Count; i++) _live.Add(_match.Enemies[i].Id);

            _dead.Clear();
            foreach (int id in _enemyGo.Keys) if (!_live.Contains(id)) _dead.Add(id);

            for (int i = 0; i < _dead.Count; i++)
            {
                Destroy(_enemyGo[_dead[i]]);
                _enemyGo.Remove(_dead[i]);
                _hpBar.Remove(_dead[i]);
                _hpBarSr.Remove(_dead[i]);
                _enemyArt.Remove(_dead[i]);
                _barGeom.Remove(_dead[i]);
            }
        }

        // Thanh máu tính bằng đơn vị THẾ GIỚI vì cha luôn scale 1 (trước đây cha
        // mang scale = cỡ quân nên số đo thanh máu bị nhân theo, đổi cỡ quân là
        // thanh máu đổi theo mà không ai chủ ý).
        private const float BarHeight = 0.09f;
        private static float BarWidth(Enemy e) => e.IsBoss ? 0.95f : 0.52f;

        // Vị trí BÀN CHÂN và ĐỈNH ĐẦU so với tâm ô sheet, tính bằng chiều cao ô
        // (= 1 unit sau khi nhập ở PPU = chiều cao ô). Đo trên chính sheet đã bóc nền:
        //   quái thường, ô 384: chân y≈374 ⇒ (374−192)/384 = 0.474
        //                       đỉnh lấy con CAO NHẤT (tifoso vác cờ, y≈28) ⇒ 0.43
        //   boss, ô 768:        chân y≈723 ⇒ (723−384)/768 = 0.441
        //                       đỉnh y≈107 ⇒ (384−107)/768 = 0.361
        // Hai sheet lệch nhau vì nhân vật chiếm tỉ lệ khác nhau trong ô của nó —
        // dùng chung một con số thì boss lơ lửng trên mặt đường 0.06 unit.
        private static float FootFromCentre(Enemy e) => e.IsBoss ? 0.441f : 0.474f;
        private static float TopFromCentre(Enemy e) => e.IsBoss ? 0.361f : 0.43f;
        private const float BarClearance = 0.10f;   // hở giữa đỉnh đầu và thanh máu

        /// <summary>
        /// Vỏ của một con quân: sprite + Animator nếu đã bake được
        /// `Resources/Anim/&lt;defId&gt;.controller`, chưa có art thì rơi về hình khối
        /// `Draw` — cùng lối rẽ với tướng ở <see cref="MakeTowerVisual"/>, nhờ vậy
        /// thay art từng con một mà không đụng con còn lại.
        ///
        /// Cha LUÔN scale 1 và đứng đúng điểm trên đường; mọi co giãn nằm ở con
        /// "art". Thanh máu treo ở cha nên không bị cỡ nhân vật kéo méo.
        /// </summary>
        private readonly Dictionary<string, RuntimeAnimatorController?> _ctrlCache = new();

        /// <summary>
        /// `Resources/Anim/&lt;id&gt;.controller`, NẠP MỘT LẦN cho mỗi id.
        ///
        /// Bản cũ gọi thẳng `Resources.Load` ở đường spawn — tức MỖI CON quái một
        /// lần nạp (W20 có 47 con) và mỗi lần dựng tướng một lần nữa. `Resources.Load`
        /// là lời gọi ĐỒNG BỘ: lần đầu của mỗi loại phải giải nén asset ngay trong
        /// frame đó, và nó rơi đúng vào lúc một loại quái mới lần đầu ra sân — chỗ
        /// người chơi dễ thấy khựng nhất.
        ///
        /// Cache nhớ CẢ giá trị null: id chưa có art thì cũng chỉ hỏi ổ đĩa một lần.
        /// </summary>
        private RuntimeAnimatorController? AnimCtrl(string id)
        {
            if (_ctrlCache.TryGetValue(id, out RuntimeAnimatorController? c)) return c;
            c = Resources.Load<RuntimeAnimatorController>($"Anim/{id}");
            _ctrlCache[id] = c;
            return c;
        }

        private GameObject MakeEnemyVisual(Enemy e)
        {
            var go = new GameObject($"E{e.Id}");

            var art = new GameObject("art");
            art.transform.SetParent(go.transform, false);
            var sr = art.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 6;

            var ctrl = AnimCtrl(e.DefId);
            if (ctrl != null)
            {
                float big = e.IsBoss ? BossArtMultiplier : 1f;
                art.transform.localScale = Vector3.one * (EnemyArtScale * big);
                // Nâng đúng bằng khoảng cách tâm-ô → bàn chân, để chân chạm điểm trên đường.
                art.transform.localPosition =
                    new Vector3(0f, FootFromCentre(e) * EnemyArtScale * big, 0f);
                art.AddComponent<Animator>().runtimeAnimatorController = ctrl;
                _enemyArt[e.Id] = sr;
            }
            else
            {
                // Hình khối tạm: giữ nguyên cỡ cũ để bàn cờ không đổi cảm giác giữa
                // chừng khi mới có art của một vài con.
                sr.sprite = Draw.ShapeOfEnemy(e.DefId, e.IsBoss);
                sr.color = Draw.OfEnemy(e.DefId, e.IsBoss);
                art.transform.localScale = Vector3.one * (e.IsBoss ? 0.9f : 0.42f);
            }

            // Thanh máu treo ngay TRÊN đỉnh đầu. Số cũ (0.62) tính từ hồi quân còn
            // là hình khối cao 0.42; art thật cao 0.75-0.80 nên thanh máu chui vào
            // trong người. Nay suy thẳng từ hình học của chính con đang dựng.
            float lift = _enemyArt.ContainsKey(e.Id)
                // chân→đỉnh đầu = (chân + đỉnh) so với tâm ô, nhân cỡ hiển thị
                ? (FootFromCentre(e) + TopFromCentre(e)) * EnemyArtScale
                  * (e.IsBoss ? BossArtMultiplier : 1f) + BarClearance
                : (e.IsBoss ? 0.45f : 0.21f) + BarClearance;   // nửa chiều cao hình khối
            _barGeom[e.Id] = new Vector2(BarWidth(e), lift);

            var bar = Draw.Make("hp", Draw.Square(), new Color(0.2f, 1f, 0.3f), 1f, 7, go.transform);
            bar.transform.localScale = new Vector3(BarWidth(e), BarHeight, 1f);
            bar.transform.localPosition = new Vector3(0f, lift, 0f);
            _hpBar[e.Id] = bar.transform;
            if (bar.TryGetComponent(out SpriteRenderer barSr)) _hpBarSr[e.Id] = barSr;

            return go;
        }

        /// <summary>
        /// Lật mặt quân theo hướng đang đi. Sheet quái vẽ mặt sang **PHẢI** — cùng
        /// quy ước với sheet tướng (kiểm lại trên frame 0: tai bên trái, mũi/mắt
        /// hướng phải) → đi sang TRÁI thì mới lật.
        ///
        /// Hướng lấy từ tiếp tuyến của đường tại quãng đã đi, không phải từ hiệu vị
        /// trí hai khung: ở khúc đường gần thẳng đứng, hiệu vị trí dao động quanh 0
        /// và quân sẽ rung lật liên tục. Vùng chết 0.02 chặn nốt phần còn lại.
        /// </summary>
        private void FaceTravel(Enemy e)
        {
            if (!_enemyArt.TryGetValue(e.Id, out SpriteRenderer? sr) || sr == null) return;
            double d = e.DistanceTravelled, len = _pathGeo.Length;
            Vec2 a = _pathGeo.PositionAt(System.Math.Max(0, d - 0.05));
            Vec2 b = _pathGeo.PositionAt(System.Math.Min(len, d + 0.05));
            float dx = (float)(b.X - a.X);
            if (Mathf.Abs(dx) > 0.02f) sr.flipX = dx < 0f;
        }

        private void SyncTowers()
        {
            foreach (TowerInstance t in _match.Slots.Towers)
            {
                if (!_towerGo.TryGetValue(t.SlotId, out GameObject? go))
                {
                    Vec2 p = _match.Slots.SlotOf(t.SlotId).Position;
                    go = MakeTowerVisual(t);
                    go.transform.position = new Vector3((float)p.X, (float)p.Y, -0.5f);
                    _towerGo[t.SlotId] = go;
                    // Ô đã có hero → ẩn vòng ô mặc định. Vòng chỉ để DẪN đặt tướng ở
                    // ô trống; đứng dưới chân hero thì rối, không đẹp.
                    if (_slotRing.TryGetValue(t.SlotId, out LineRenderer? ring)) ring.enabled = false;
                }
                // Cấp đọc bằng KÍCH THƯỚC + số chấm. Kích thước một mình thì phải có
                // hai tướng cạnh nhau mới so được; chấm thì đếm được ngay.
                // El Cinco Lv3 giữ đúng cỡ Lv2: khác biệt của cấp này nằm ở cú húc
                // cuồng nộ, không phải một silhouette phình to che mất animation.
                float scale = t.TowerId == "batigol" && t.Level >= 3 ? 0.8f : 0.6f + 0.1f * t.Level;
                go.transform.localScale = Vector3.one * scale;
                bool hasLevelIdle = t.TowerId == "batigol" || t.TowerId == "la_pulga";
                if (hasLevelIdle && _towerAnim.TryGetValue(t.SlotId, out Animator? anim) &&
                    (!_towerVisualLevel.TryGetValue(t.SlotId, out int shown) || shown != t.Level))
                {
                    anim.SetInteger("Level", t.Level);
                    _towerVisualLevel[t.SlotId] = t.Level;
                }
                if (t.TowerId == "batigol" && _towerAnim.TryGetValue(t.SlotId, out Animator? batigolArt))
                    batigolArt.transform.localScale = Vector3.one *
                        (t.Level >= 3 ? BatigolBerserkArtScale : SpriteArtScale);
                SyncLevelPips(t, go.transform);
                // CHỈ trọng tài có aura vùng THẬT (làm chậm) → giữ. Các hero khác
                // không có kỹ năng tác động cả vùng nên KHÔNG vẽ hào quang quanh thân;
                // tầm đánh chỉ hiện khi người chơi chọn hero (ShowRangePreview).
                SyncArbitro(t, go);
            }

            _goneTowers.Clear();
            foreach (string id in _towerGo.Keys) if (_match.Slots.At(id) == null) _goneTowers.Add(id);
            for (int i = 0; i < _goneTowers.Count; i++)
            {
                string id = _goneTowers[i];
                Destroy(_towerGo[id]);
                _towerGo.Remove(id);
                _towerAnim.Remove(id);
                _towerVisualLevel.Remove(id);
                _animTriggers.Remove(id);
                // Bán tướng → ô trống lại → hiện lại vòng dẫn đặt.
                if (_slotRing.TryGetValue(id, out LineRenderer? ring)) ring.enabled = true;
                if (_auraGlow.TryGetValue(id, out GameObject? glow)) { Destroy(glow); _auraGlow.Remove(id); }
                _waveTimer.Remove(id);
            }
        }

        /// <summary>
        /// El Árbitro (vòng 22): vùng aura XANH bán kính = tầm (nền chậm 25%, CẢ 3
        /// cấp) + SÓNG lan từ trong ra. Cơ chế chậm ở core (`TickAuras` cho nền, thẻ
        /// cho con trúng) — đây chỉ VẼ. Thẻ bay + giơ thẻ do `OnCardShown` lo (Lv2/Lv3).
        /// </summary>
        private void SyncArbitro(TowerInstance t, GameObject go)
        {
            if (t.TowerId != "el_arbitro") return;

            double range = _match.Upgrades.TowerById(t.TowerId).Levels[t.Level - 1].Range;
            Vector3 center = new(go.transform.position.x, go.transform.position.y, 0f);

            // Nền xanh mờ phủ đúng vùng tầm (thở nhẹ).
            if (!_auraGlow.TryGetValue(t.SlotId, out GameObject? glow))
            {
                glow = Draw.Make($"aura_{t.SlotId}", Draw.Circle(), AuraBlue, 1f, 1);
                _auraGlow[t.SlotId] = glow;
                if (glow.TryGetComponent(out SpriteRenderer gsr)) _auraGlowSr[t.SlotId] = gsr;
            }
            glow.transform.position = center;
            glow.transform.localScale = Vector3.one * (float)(range * 2.0);
            float pulse = 0.12f + 0.04f * Mathf.Sin(Time.time * 3f);
            if (_auraGlowSr.TryGetValue(t.SlotId, out SpriteRenderer? glowSr) && glowSr != null)
                glowSr.color = WithAlpha(AuraBlue, pulse);

            // Sóng: cứ WaveInterval phát một vòng nở từ tâm ra rìa tầm rồi tắt.
            if (!_waveTimer.TryGetValue(t.SlotId, out float timer)) timer = 0f;
            timer -= Time.deltaTime * SpeedMultiplier;
            if (timer <= 0f)
            {
                GameObject ring = Draw.UnitRing($"wave_{t.SlotId}", AuraBlue, 0.06f, 2);
                ring.transform.position = center;
                _waves.Add((ring, 0f, WaveLife, (float)range));
                timer = WaveInterval;
            }
            _waveTimer[t.SlotId] = timer;
        }

        /// <summary>Sóng aura đang lan: nở từ tâm ra rìa tầm, mờ dần rồi tắt.</summary>
        private void TickWaves()
        {
            for (int i = _waves.Count - 1; i >= 0; i--)
            {
                (GameObject go, float age, float life, float maxR) = _waves[i];
                if (go == null) { _waves.RemoveAt(i); continue; }
                age += Time.deltaTime * SpeedMultiplier;
                float k = age / life;
                if (k >= 1f) { Destroy(go); _waves.RemoveAt(i); continue; }
                go.transform.localScale = Vector3.one * Mathf.Lerp(0.15f, maxR, k);
                var lr = go.GetComponent<LineRenderer>();
                Color c = lr.startColor; c.a = 0.5f * (1f - k); lr.startColor = lr.endColor = c;
                _waves[i] = (go, age, life, maxR);
            }
        }

        /// <summary>
        /// Chọn cách vẽ tướng: nếu đã bake được Animator Controller
        /// (`Resources/Anim/&lt;towerId&gt;.controller`, xem `SpriteAnimBaker`) thì dùng
        /// sprite + Animator; chưa có art thì rơi về hình khối `Draw`. Nhờ vậy thay
        /// art từng tướng một, không cần đụng gameplay hay các tướng còn lại.
        /// </summary>
        /// <summary>
        /// Kích trigger cho hero ở ô <paramref name="slotId"/>. Controller CHƯA có
        /// param đó (art mới, chưa chạy `La Muralla/Bake All Hero Anim`) thì rơi về
        /// <paramref name="fallback"/>; không có cả hai thì im lặng bỏ qua.
        ///
        /// Trước đây gọi thẳng `SetTrigger` với ghi chú "no-op an toàn" — không
        /// đúng: Unity ghi warning mỗi phát bắn, và hero đứng nguyên ở Idle.
        /// </summary>
        private void FireTrigger(string slotId, Animator anim, int hash, int fallback)
        {
            if (!_animTriggers.TryGetValue(slotId, out HashSet<int>? have))
            {
                have = new HashSet<int>();
                foreach (AnimatorControllerParameter p in anim.parameters)
                    if (p.type == AnimatorControllerParameterType.Trigger) have.Add(p.nameHash);
                // `Animator.parameters` trả RỖNG nếu controller chưa init xong. Nhớ
                // bộ rỗng đó = câm vĩnh viễn ô này → chỉ cache khi đã đọc được thật.
                if (have.Count == 0) { anim.SetTrigger(hash); return; }
                _animTriggers[slotId] = have;
            }
            if (have.Contains(hash)) anim.SetTrigger(hash);
            else if (have.Contains(fallback)) anim.SetTrigger(fallback);
        }

        /// <summary>
        /// Tiếng lúc bắn, chọn theo TƯỚNG và CẤP để khớp với animation vừa tung —
        /// El Cinco đấm ở Lv1 nghe nhẹ hơn đá/đạp ở Lv2/Lv3. Tướng ném có tiếng
        /// riêng ở <see cref="OnThrew"/>; ở đây chỉ lo đòn đánh thường.
        /// </summary>
        private static void PlayShotSfx(TowerInstance? t)
        {
            if (t == null) { AudioService.Play("shot_ball", 0.5f); return; }
            if (t.TowerId == "batigol")
                AudioService.Play(t.Level >= 2 ? "punch_heavy" : "punch", 0.7f);
            else
                AudioService.Play("shot_ball", 0.5f);
        }

        private GameObject MakeTowerVisual(TowerInstance t)
        {
            var ctrl = AnimCtrl(t.TowerId);
            if (ctrl == null)
                return Draw.Make($"T_{t.SlotId}", Draw.ShapeOf(t.TowerId),
                                 Draw.OfTower(t.TowerId), 0.72f, 12);   // trên cùng như hero có art

            // Sprite nằm trên con "art" có scale riêng → nhân vật to hơn hình khối
            // cũ mà không xô lệch chấm cấp (pip gắn ở go, không bị scale theo).
            var go = new GameObject($"T_{t.SlotId}");
            var art = new GameObject("art");
            art.transform.SetParent(go.transform, false);
            // Nâng nhân vật lên để nó ĐỨNG TRÊN điểm ô (chân gần tâm ô) thay vì nằm
            // giữa tâm — nhìn "đứng gác" tự nhiên hơn. Pip/nhãn vẫn ở điểm ô.
            art.transform.localPosition = new Vector3(0f, ArtStandOffset, 0f);
            art.transform.localScale = Vector3.one * SpriteArtScale;
            var sr = art.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 12;   // TRÊN CÙNG: quái (6) + thanh máu (7) + đạn (8) không đè lên hero
            var anim = art.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl;
            _towerAnim[t.SlotId] = anim;
            return go;
        }

        private readonly Dictionary<string, List<GameObject>> _pips = new();

        /// <summary>Chấm cấp dưới chân tướng: ●, ●●, ●●●.</summary>
        private void SyncLevelPips(TowerInstance t, Transform parent)
        {
            if (!_pips.TryGetValue(t.SlotId, out List<GameObject>? list))
                _pips[t.SlotId] = list = new List<GameObject>();
            if (list.Count == t.Level) return;

            foreach (GameObject g in list) Destroy(g);
            list.Clear();
            for (int i = 0; i < t.Level; i++)
            {
                GameObject p = Draw.Make($"pip{i}", Draw.Circle(), Color.white, 0.16f, 5, parent);
                p.transform.localPosition = new Vector3((i - (t.Level - 1) / 2f) * 0.24f, -0.62f, 0);
                list.Add(p);
            }
        }

        // ── Chạm tay ────────────────────────────────────────────────────────

        private void HandleTouch()
        {
            Vector2? tap = TapPosition();
            if (tap == null) return;

            // Chạm trúng bất kỳ nút nào của HUD → để Hud xử lý, đừng đụng tới ô.
            //
            // Trước đây chỗ này chỉ né dải thanh chọn tướng, nên một cú bấm START
            // vừa bấm nút VỪA chọn ô nằm dưới nút — và chính việc chọn ô đó làm nút
            // không bấm được. Xem `Hud.BlocksTouch` để biết vì sao hai việc lại
            // triệt tiêu nhau.
            if (_hud != null && _hud.BlocksTouch(tap.Value)) return;

            Vector3 w = _cam.ScreenToWorldPoint(new Vector3(tap.Value.x, tap.Value.y, 10));
            SlotDef? hit = NearestSlotWithinTouch(new Vec2(w.x, w.y));

            Select(hit?.Id);   // chạm chỗ trống = bỏ chọn
        }

        /// <summary>Ô đang chọn. `null` = không chọn gì. Hud đọc để vẽ thanh dưới.</summary>
        internal string? SelectedSlot { get; private set; }

        internal void Select(string? slotId)
        {
            if (SelectedSlot == slotId) return;
            if (SelectedSlot != null) HighlightSlot(SelectedSlot, false);
            SelectedSlot = slotId;
            if (slotId != null) HighlightSlot(slotId, true);
            ShowRangePreview(slotId);
        }

        /// <summary>
        /// 🔴 VÒNG TẦM — sửa cái lỗi đắt nhất buổi thử máy.
        ///
        /// Người chơi báo "đỏ và xanh nước biển không gây dame". Không phải bug:
        /// Batigol tầm 1.0 chỉ với tới 5/11 ô, La Pulga tầm 1.4 với tới 8/11 — họ
        /// đặt vào ô tướng KHÔNG VỚI TỚI, nên nó đứng im.
        ///
        /// Thang tầm là cơ chế cốt lõi (vòng 5 dựng nó để giết nội dung chết), nhưng
        /// nó VÔ HÌNH. Cơ chế hay mà không nhìn thấy được thì người chơi đọc ra
        /// thành game hỏng — và họ đúng khi báo lỗi.
        ///
        /// Giờ chọn ô là thấy ngay tầm tướng sẽ có, TRƯỚC khi trả tiền.
        /// </summary>
        private void ShowRangePreview(string? slotId)
        {
            if (_rangeRing != null) { Destroy(_rangeRing.gameObject); _rangeRing = null; }
            if (slotId == null) return;

            TowerInstance? t = _match.Slots.At(slotId);
            double range = t != null
                ? _match.Upgrades.TowerById(t.TowerId).Levels[t.Level - 1].Range
                : PreviewRange(slotId);
            if (range <= 0) return;

            Vec2 p = _match.Slots.SlotOf(slotId).Position;
            _rangeRing = Draw.Ring("RangePreview", new Vector3((float)p.X, (float)p.Y, 0),
                                   (float)range, new Color(1f, 1f, 1f, 0.35f), 0.06f, 3);
        }

        /// <summary>Tầm của tướng Hud đang rê chọn — ô trống thì xem trước tầm của
        /// tướng nào người chơi đang cân nhắc.</summary>
        internal string? PreviewTowerId { get; set; }

        private double PreviewRange(string slotId) =>
            PreviewTowerId == null ? 0 : _match.Upgrades.TowerById(PreviewTowerId).Levels[0].Range;

        internal void RefreshRange() => ShowRangePreview(SelectedSlot);

        /// <summary>
        /// Ô gần nhất trong VÙNG CHẠM 48pt. Không phải "ô gần nhất" — ngón tay chạm
        /// giữa hai ô mà cách cả hai 3 unit thì không được chọn ô nào.
        /// </summary>
        private SlotDef? NearestSlotWithinTouch(Vec2 p)
        {
            double r = _def.MinTouchTargetUnits / 2.0;
            SlotDef? best = null;
            double bestD = double.MaxValue;
            foreach (SlotDef s in _def.Slots)
            {
                double d = Vec2.SqrDistance(s.Position, p);
                if (d <= r * r && d < bestD) { bestD = d; best = s; }
            }
            return best;
        }

        /// <summary>
        /// Input System mới — `activeInputHandler: 1` nên `Input.GetMouseButtonDown`
        /// KHÔNG chạy (nó là API legacy, đã tắt). Hỗ trợ cả chạm (máy thật) lẫn chuột
        /// (Editor) để thử được mà không cần build.
        /// </summary>
        private static Vector2? TapPosition()
        {
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                return Touchscreen.current.primaryTouch.position.ReadValue();
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return Mouse.current.position.ReadValue();
            return null;
        }

        // ── Tạm dừng / chơi lại / thoát ─────────────────────────────────────

        /// <summary>Trận đang đóng băng. Hud đọc để vẽ overlay và để ẩn thanh
        /// mua tướng — một trận đã dừng không được phép tiêu tiền.</summary>
        internal bool Paused { get; private set; }

        internal bool RewardedAdReady => _adGateway?.IsReady == true;
        internal bool PrivacyOptionsRequired => _adGateway?.PrivacyOptionsRequired == true;

        internal bool TryShowRewarded(Action reward)
        {
            if (_rewardedAds == null) return false;

            bool shouldResume = (_match.Phase is MatchPhase.Preparing or MatchPhase.Fighting) && !Paused;
            _resumeAfterRewardedAd = shouldResume;
            if (shouldResume) SetPaused(true);

            if (_rewardedAds.TryShow(reward)) return true;

            if (shouldResume) SetPaused(false);
            _resumeAfterRewardedAd = false;
            return false;
        }

        internal bool ShowPrivacyOptions() => _adGateway?.ShowPrivacyOptions() == true;

        private void OnRewardedAdClosed()
        {
            if (_resumeAfterRewardedAd) SetPaused(false);
            _resumeAfterRewardedAd = false;
        }

        /// <summary>
        /// Bật/tắt tạm dừng. Chỉ có nghĩa khi trận còn đang chạy: đã Won/Lost thì
        /// không còn gì để dừng, và cho dừng ở đó chỉ tạo ra một overlay che mất
        /// màn hình kết quả.
        /// </summary>
        internal void SetPaused(bool on)
        {
            if (_match == null) return;
            if (on && _match.Phase is MatchPhase.Won or MatchPhase.Lost) return;
            if (Paused == on) return;
            Paused = on;
            // Đóng thanh chọn tướng khi dừng: nó nằm ở đáy màn, overlay phủ lên,
            // và mở lại game với một ô còn "đang chọn" mà không thấy thanh là trạng
            // thái ma.
            if (on) CloseMenu();
        }

        /// <summary>
        /// 🔴 CHƠI LẠI BẰNG CÁCH NẠP LẠI SCENE, không reset tay từng vùng state.
        ///
        /// Lớp này giữ 8 dictionary (`_enemyGo`, `_hpBar`, `_enemyArt`, `_barGeom`,
        /// `_towerGo`, `_slotRing`, `_towerAnim`, `_animTriggers`), cộng danh sách
        /// FX/đạn/coin, cộng `MatchController` với ví-máu-wave của nó, cộng camera
        /// và HUD. Bản reset tay phải nhớ đủ cả chuỗi đó; bỏ sót một cái là quân
        /// của trận cũ còn đứng trên sân — và đó là lỗi chỉ lộ ra ở lần chơi lại
        /// thứ hai, tức là sau khi đã tin là nó chạy.
        ///
        /// `Start()` đã biết dựng mọi thứ từ số không. Dùng lại nó.
        /// </summary>
        private bool _resultSaved;
        private string _mapId = "";
        private string _nextMapId = "";
        private string _mapLabel = "";

        /// <summary>Map kế tiếp theo thứ tự. Rỗng = đang ở map cuối.</summary>
        internal string NextMapId => _nextMapId;

        /// <summary>
        /// "MAP 08 · TRES PUERTAS" — Hud vẽ ở đỉnh màn.
        ///
        /// Người chơi vào trận qua màn chọn map rồi scene NẠP LẠI, nên cái tên vừa
        /// bấm biến mất khỏi màn hình. Chơi lại hoặc đi tiếp map sau thì lại càng
        /// không biết mình đang ở đâu trong 11 map. Số này là số HIỂN THỊ ở màn chọn,
        /// không phải chỉ số mảng — hai thứ lệch nhau một đơn vị.
        /// </summary>
        internal string MapLabel => _mapLabel;

        /// <summary>Vào thẳng một map. Nạp lại scene để dọn sạch trận cũ.</summary>
        internal void GoToMap(string id)
        {
            MapSession.PickedMapId = id;
            MapSelect.Reload();
        }

        /// <summary>Về màn chọn map. Nạp lại scene để dọn sạch trận — xem `MapSession`.</summary>
        internal void BackToMenu()
        {
            MapSession.PickedMapId = null;
            MapSelect.Reload();
        }

        internal void Restart()
        {
            Scene s = SceneManager.GetActiveScene();
            // `buildIndex` = -1 khi scene không nằm trong Build Settings (mở thẳng
            // file .unity trong Editor). Rơi về tên để việc chơi lại không chết
            // trong đúng cái môi trường ta hay thử nhất.
            if (s.buildIndex >= 0) SceneManager.LoadScene(s.buildIndex);
            else SceneManager.LoadScene(s.name);
        }

        /// <summary>
        /// Thoát. `Application.Quit()` KHÔNG làm gì trong Editor (Unity ghi rõ), nên
        /// nhánh Editor phải tự tắt play mode — không có nó thì nút QUIT nhìn như
        /// hỏng suốt quá trình thử, và ta sẽ đi tìm bug không tồn tại.
        ///
        /// Trên iOS Apple khuyến nghị đừng tự thoát app (HIG); nút này giữ vì đây là
        /// yêu cầu của người chơi, và nó chỉ nằm trong menu chứ không ở HUD chính.
        /// </summary>
        internal void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        /// <summary>
        /// App xuống nền (nhận cuộc gọi, chuyển app) → tự dừng. Không có cái này thì
        /// người chơi quay lại thấy cầu môn đã thủng: Unity vẫn chạy `Update` vài
        /// khung trước khi hệ điều hành treo tiến trình, và quan trọng hơn là lúc
        /// quay lại `Time.deltaTime` bơm một cục thời gian dồn vào `Tick`.
        /// </summary>
        private void OnApplicationPause(bool paused)
        {
            if (paused) SetPaused(true);
        }

        private void OnDestroy()
        {
            if (_adGateway != null)
                _adGateway.PresentationClosed -= OnRewardedAdClosed;
        }

        internal void CloseMenu() => Select(null);

        /// <summary>Tô vòng ô đang chọn — phản hồi tức thì cho ngón tay.</summary>
        private void HighlightSlot(string id, bool on)
        {
            if (!_slotRing.TryGetValue(id, out LineRenderer? lr)) return;
            SlotDef s = _match.Slots.SlotOf(id);
            Color b = s.IsField ? Draw.SlotFree : Draw.SlotKeeper;
            lr.startColor = lr.endColor = on ? Color.white : b;
        }
    }
}
