# 05 — Kiến trúc kỹ thuật

> ⚠️ Cấu trúc dưới đây là **bản mô tả**, chưa được tạo. Phase hiện tại là define — không scaffold.
> Khi docs chín và chốt xong [`OPEN-QUESTIONS.md`](OPEN-QUESTIONS.md), M0 sẽ dựng đúng cây này.

---

## 1. Stack

| Lớp | Lựa chọn | Phiên bản |
|-----|----------|-----------|
| Engine | Unity | **6 LTS** (6000.0.x) |
| Render | Universal Render Pipeline — 2D Renderer | đi kèm Unity 6 |
| Ngôn ngữ | C# | 9.0 (Unity scripting runtime .NET Standard 2.1) |
| Input | Input System package | 1.8+ |
| Test | Unity Test Framework (NUnit) | 1.4+ |
| Build iOS | Xcode | 16+ |
| Build Android | Gradle qua Unity | Android API 24+ |

**Package đang dùng:**
`com.unity.render-pipelines.universal` · `com.unity.inputsystem` · `com.unity.test-framework` · `com.unity.2d.sprite` · `com.unity.2d.animation` · `com.google.ads.mobile` 11.3.0

**Package cố tình KHÔNG dùng:** DOTS/ECS, Addressables, Cinemachine và SDK IAP. AdMob chỉ phục vụ rewarded ad; không có banner/interstitial.

💡 *Why không DOTS: người ta hay reach cho ECS vì "tower defense = nhiều entity". 37 quân trên màn là con số MonoBehaviour thường xử lý không đổ mồ hôi. ECS ở đây chỉ mua thêm độ phức tạp và thời gian compile.*

---

## 2. Cây thư mục

```
LaMuralla/
├── Assets/
│   ├── _Project/                      ← mọi thứ của game nằm trong đây, không đổ ra Assets/ gốc
│   │   ├── Art/
│   │   │   ├── Sprites/
│   │   │   │   ├── Towers/            LaPulga, D10S, Batigol, Dibu (mỗi tướng 3 cấp)
│   │   │   │   ├── Enemies/           Adepto, Tifoso, Tambor
│   │   │   │   ├── Field/             sân, đường chạy, ô đặt, cầu môn
│   │   │   │   ├── UI/                icon, khung, nút
│   │   │   │   └── VFX/               hiệu ứng bắn, ngã, slow
│   │   │   ├── Fonts/
│   │   │   └── Materials/
│   │   ├── Audio/
│   │   │   ├── SFX/                   bắn, trúng, ngã, lọt lưới, cản phá, mua, nâng cấp
│   │   │   └── Music/                 1 track nền
│   │   ├── Prefabs/
│   │   │   ├── Towers/                1 prefab / tướng (cấp là state, không phải prefab riêng)
│   │   │   ├── Enemies/               1 prefab / loại
│   │   │   ├── Projectiles/
│   │   │   ├── UI/
│   │   │   └── VFX/
│   │   ├── Scenes/
│   │   │   ├── Boot.unity             load config → chuyển MainMenu
│   │   │   ├── MainMenu.unity
│   │   │   └── Match.unity            toàn bộ gameplay
│   │   ├── Resources/
│   │   │   └── Config/                ← copy/symlink từ /config. KHÔNG fork. Xem §4.
│   │   │       ├── towers.json
│   │   │       ├── enemies.json
│   │   │       ├── waves.json
│   │   │       ├── economy.json
│   │   │       └── path.json          ← đường chạy + 12 ô. Xem §4.2.
│   │   ├── ScriptableObjects/         sinh ra từ JSON lúc Boot, hoặc bake sẵn trong Editor
│   │   │   ├── Towers/
│   │   │   ├── Enemies/
│   │   │   └── Waves/
│   │   └── Scripts/
│   │       ├── Core/                  GameLoop, GameStateMachine, ServiceLocator, EventBus
│   │       ├── Config/                ConfigLoader, TowerDef, EnemyDef, WaveDef, EconomyDef
│   │       ├── Match/                 MatchController, WaveSpawner, EconomyService,
│   │       │                          GoalHealth, SlotManager, SpeedController
│   │       ├── Towers/                TowerBase, TargetingSystem, UpgradeService,
│   │       │                          Abilities/ (SoloRun, HandOfGod, Cross, Piercing, Save)
│   │       ├── Enemies/               EnemyBase, PathFollower, HealthComponent, BountyDropper
│   │       ├── Combat/                DamageSystem, Projectile, AoEResolver, StatusEffectStack
│   │       ├── UI/                    HUD, RadialBuyMenu, TowerInfoPanel, ResultScreen, WaveBanner
│   │       ├── Persistence/           SaveService (chỉ lưu số sao cao nhất)
│   │       └── Utils/                 ObjectPool, MathX, Gizmos
│   ├── Plugins/iOS/                   (rỗng ở MVP — chỗ dành sẵn)
│   └── Tests/
│       ├── EditMode/                  toán cân bằng, kinh tế, load config — chạy không cần scene
│       └── PlayMode/                  spawn wave, sát thương, lọt lưới, mua/bán
├── Packages/manifest.json
├── ProjectSettings/
├── .gitignore                         (Unity template chuẩn + /Library /Temp /Builds /obj)
└── README.md

── NGOÀI project Unity, ở gốc repo (ĐÃ TỒN TẠI): ──────────────────

config/                                ← 🔴 NGUỒN CHÂN LÝ SỐ LIỆU
├── towers.json                        6 đơn vị, 18 kỹ năng, tốc độ đạn
├── enemies.json                       3 loại + boss O Capitão
├── waves.json                         20 wave, hệ số act
├── economy.json                       tiền, balanceModel (η/σ/τ), roundingMode
└── path.json                          đường chạy + 12 ô + quy đổi đơn vị

tools/
└── gen_docs.py                        sinh bảng docs từ config. --check cho CI.
```

**Quy ước:** mọi asset của game nằm dưới `Assets/_Project/`. Package bên thứ ba nếu có thì ở `Assets/Plugins/`. Dấu `_` để `_Project` luôn nổi lên đầu Project window.

⚠️ **`config/` sống ngoài project Unity, và đó là chủ đích.** Nó là nguồn chân lý cho **ba** consumer: Unity (`ConfigLoader`), `tools/gen_docs.py` (sinh bảng docs), và bảng tính cân bằng M0. Nhét nó vào `Assets/` thì hai consumer sau phải với tay vào project Unity để đọc một file dữ liệu.

**M0 phải copy hoặc symlink `config/` → `Assets/_Project/Resources/Config/`, KHÔNG fork.** Fork = quay lại đúng lớp bug hai-nguồn-chân-lý mà cả cấu trúc này sinh ra để diệt.

---

## 3. Module & trách nhiệm

| Module | Trách nhiệm | KHÔNG được làm |
|--------|-------------|----------------|
| `Core/GameLoop` | Tick chính, quản lý hệ số tốc độ (1× / 2×) | Biết về tướng hay quân |
| `Core/GameStateMachine` | Boot → Menu → Kickoff → Wave → Rest → Result | Chạm UI trực tiếp |
| `Core/EventBus` | Pub/sub giữa module (`EnemyKilled`, `GoalDamaged`, `WaveCleared`) | Giữ state |
| `Config/ConfigLoader` | Đọc JSON → dựng ScriptableObject, validate schema | Chứa số liệu hardcode |
| `Match/MatchController` | Điều phối 1 trận, quyết định thắng/thua | Tính sát thương |
| `Match/WaveSpawner` | Đọc `waves.json`, spawn theo interval | Biết quân chết thế nào |
| `Match/EconomyService` | **Nguồn chân lý của ví.** Cộng/trừ tiền, xác thực đủ tiền | Biết tại sao trừ |
| `Match/GoalHealth` | Máu cầu môn, xử lý lọt lưới, phát `GoalDamaged` | Biết về Dibu |
| `Match/SlotManager` | 11 ô sân + 1 ô thủ môn; ô trống/đầy; ép luật "Dibu chỉ ở ô thủ môn" | Mua tướng |
| `Towers/TowerBase` | State tướng: cấp, chỉ số hiện tại, cooldown | Tự trừ tiền |
| `Towers/TargetingSystem` | Chọn mục tiêu `first-in-range` | Gây sát thương |
| `Towers/UpgradeService` | Áp công thức cấp (×1.6 / ×2.5), gọi EconomyService | Biết UI |
| `Combat/DamageSystem` | Áp sát thương, **quyết định ai gây đòn kết liễu** | Cộng tiền (phát event) |
| `Combat/StatusEffectStack` | Slow — **ép cap 70%, lấy max không cộng dồn** | Tự gây sát thương |
| `Enemies/PathFollower` | Đi theo spline cố định | Tìm đường |
| `UI/*` | Chỉ đọc state + phát intent người chơi | Thay đổi state trực tiếp |
| `Persistence/SaveService` | Lưu/đọc số sao cao nhất (PlayerPrefs) | Lưu state giữa trận |

### Ba luật bất khả xâm phạm

1. **`EconomyService` là nguồn chân lý duy nhất của ví.** Không module nào được `wallet += x`. Bug tiền là bug đắt nhất và khó tái hiện nhất trong thể loại này.
2. **`DamageSystem` là nơi duy nhất quyết định "ai gây đòn kết liễu".** Xem `01` §8 **FM-07** — hai tướng cùng bắn phát cuối → cộng tiền hai lần → kinh tế vỡ sau 10 wave. Một chỗ quyết, một chỗ phát `EnemyKilled`.
3. **Không số liệu nào hardcode trong C#.** Mọi chỉ số đến từ `Resources/Config/`. Test ở `Tests/EditMode/` phải fail nếu tìm thấy magic number trong `Towers/` hay `Enemies/`.

💡 *Why luật 1: trong TD, tiền được cộng từ ~5 chỗ (hạ quân, clear wave, skip, bán, hoàn nâng cấp lỗi). Nếu mỗi chỗ tự cộng, thì khi ví sai 12 Peso ở wave 14 bạn sẽ mất một buổi chiều để tìm ra chỗ nào. Một cổng vào duy nhất = một breakpoint duy nhất.*

---

## 4. Schema config JSON

**Nguồn chân lý số liệu — `config/*.json`.** Người cân bằng game sửa file này, **không** mở Unity, **không** sửa docs.

> Các file đã tồn tại thật ở `config/`. Docs sinh bảng từ chúng qua `tools/gen_docs.py`. Ở M0, `ConfigLoader` đọc thẳng từ `Assets/_Project/Resources/Config/` — copy hoặc symlink từ `config/`, **không** fork.

---

### 4.1 🔴 Đơn vị — phải chốt TRƯỚC dòng code đầu tiên

Toàn bộ mô hình cân bằng ở `04` sống trong "đơn vị sân". Trước bản này, **không doc nào định nghĩa nó là gì.**

```
1 đơn vị sân = 1 Unity world unit
```

| Thông số | Giá trị | Suy ra từ |
|----------|---------|-----------|
| Pixels Per Unit (PPU) | **100** | Chuẩn Unity 2D; sprite 64px → 0.64 unit |
| Camera | Orthographic, `size = 9.6` | Nửa chiều cao viewport |
| Viewport | **10.8 × 19.2** units | 1080×1920 ÷ 100 |
| Đường chạy | **40.0** units | `04` §3.2 dùng số này — đổi nó = tính lại toàn bộ |

Chốt ở `config/path.json → units`.

#### 🔴 PHÁT HIỆN: `chord = 2R` SAI trên đường cong — và sai theo hướng nguy hiểm

`04` §8 mục 5 ghi giả định này là *"rủi ro trung bình — ô ở góc phủ ít hơn"*. **Ngược lại.**

Đường chữ S dài 40 units nhét trong hộp ~7×17 → các đoạn đường **cách nhau chỉ 3–5 units**. Tầm El Fideo Lv3 = **7.0** → vòng tròn tầm của nó **cắt nhiều đoạn đường cùng lúc**.

```
Giả định:  chord = 2R = 14 units
Thực tế:   vòng tròn cắt 2–3 đoạn → chord có thể tới ~28 units
```

Nghĩa là mô hình đang **đánh giá THẤP phe phòng ngự tới ~2×** với tướng tầm xa — không phải đánh giá cao. Sai số này lớn hơn cả `η`, `σ`, `τ` cộng lại, và **chỉ giải được khi có toạ độ đường chạy thật** (Q1).

#### 🟡 Tỉ lệ tầm / đường có thể đã calibrate sai

| | Tầm | Đường | `2R / đường` |
|---|-----|-------|-------------|
| La Pulga | 4.5 | 40 | **22.5%** |
| Kingdom Rush (tham chiếu thể loại) | ~3 tiles | ~40 tiles | **~7.5%** |

Tướng ở đây phủ đường **rộng gấp 3 lần chuẩn thể loại**. Có thể là ý đồ (ít ô hơn, mỗi ô quan trọng hơn), có thể là lỗi calibrate. Bảng tính M0 nói được — đừng chỉnh vội.

---

### 4.2 `path.json` — file TRƯỚC ĐÂY KHÔNG TỒN TẠI

`05` §3 nói `PathFollower` "đi theo spline cố định". Spline đó **không có trong bất kỳ config nào** — không `waves.json`, không đâu cả. Giờ có:

```json
{
  "units": { "pixelsPerUnit": 100, "cameraOrthographicSize": 9.6 },
  "path": {
    "interpolation": "catmull-rom",
    "lengthUnits": 40.0,
    "spawnPoint": { "x": 0.0, "y": 8.5 },
    "goalPoint":  { "x": 0.0, "y": -8.0 },
    "waypoints": [ { "x": 0.0, "y": 8.5 }, { "x": -3.5, "y": 6.5 }, "..." ]
  },
  "slots": [
    { "id": "f01",  "type": "field",      "x": -1.2, "y": 7.2 },
    { "id": "gk01", "type": "goalkeeper", "x":  0.0, "y": -7.0 }
  ]
}
```

✅ **Toạ độ là THẬT từ vòng 5** (Q1 đóng) — đo bằng `tools/path_check.py`: spline 40.65 vs khai 40 (lệch 1.6%), hộp bao trong viewport, 0 ô chết. Chạy `python3 tools/path_check.py --check` trong CI: exit 1 nếu ai sửa toạ độ làm vỡ hình học.

**Validate bổ sung:** `lengthUnits` phải khớp độ dài spline tính từ `waypoints` (±2%). Lệch = mô hình `04` đang dùng số sai.

---

### 4.3 Tốc độ đạn — chưa từng tồn tại, và nó ảnh hưởng cân bằng

`06` §4 khai 4 loại đạn nhưng **không doc nào cho tốc độ**. Mô hình WDB giả định **trúng tức thì**.

<!-- GEN:projectiles -->
| Loại đạn | Tốc độ (đơn vị/giây) | Dùng bởi | Ghi chú |
|----------|---------------------|----------|---------|
| `ball_normal` | 12 | La Pulga | Bóng thường — La Pulga |
| `ball_heavy` | 8 | — | Bóng sút mạnh — Batigol. Chậm hơn: nặng. |
| `shockwave` | **hitscan** | D10S | D10S — hitscan, trúng tức thì. -1 = không có đạn bay. |
<!-- /GEN:projectiles -->

Thời gian bay ở tầm tối đa: `ball_normal` ~0.375s · `ball_heavy` ~0.375s. Quân tốc 1.0 đi được 0.375 unit trong lúc đó — nhỏ, nhưng **không bằng không**.

**Hệ quả lên cân bằng:** đạn đang bay tới một con vừa chết là **phí toàn bộ**. Đây là một phần của `η = 0.75` mà chưa ai tách ra đo. Batigol chịu nặng nhất (2.5s/phát, đạn chậm nhất) — mỗi phát phí là 2.5 giây mất trắng.

**Quy tắc:** đạn **bám mục tiêu** (homing), không bay theo toạ độ cố định. Trừ `sut_xuyen` của Batigol — nó bay theo **đường thẳng chốt lúc bắn**, và đó chính là cơ chế xuyên.

---

### 4.4 Nút ×2 — quyết định kiến trúc, không phải nút bấm

`01` §5 có nút ×2. `03` §5 nói **"thời gian nghỉ không đổi"**. Hai câu đó cùng nhau **loại trừ** giải pháp một dòng:

```csharp
Time.timeScale = 2.0f;   // ❌ SAI — tăng tốc CẢ đồng hồ nghỉ và animation UI
```

**Đúng:**

```csharp
// GameLoop giữ hệ số riêng, KHÔNG đụng Time.timeScale
public float MatchTimeScale { get; private set; } = 1f;   // 1 hoặc 2
public float MatchDeltaTime => Time.deltaTime * MatchTimeScale;
```

| Hệ thống | Dùng gì | Vì sao |
|----------|---------|--------|
| `WaveSpawner`, `PathFollower`, cooldown kỹ năng, `Projectile` | `GameLoop.MatchDeltaTime` | Đây là thứ ×2 phải tăng tốc |
| Animator của quân/tướng | `animator.speed = MatchTimeScale` | Hình phải khớp logic |
| **Đồng hồ nghỉ giữa wave** | `Time.deltaTime` **thuần** | `03` §5 — ×2 **không** rút ngắn 8 giây nghỉ |
| Animation UI, banner, popup | `Time.deltaTime` thuần | ×2 không được làm UI giật |

💡 *Chốt việc này sau khi viết `GameLoop` = viết lại `GameLoop`. `Time.timeScale` là cái bẫy hấp dẫn vì nó chạy được ngay ở prototype — rồi tới lúc thêm phần thưởng skip (`04` §1) mới phát hiện đồng hồ nghỉ cũng đang chạy đôi, và phần thưởng đó tính sai một nửa.*

---

### 4.5 Định nghĩa còn thiếu — dev sẽ phải tự đoán nếu không chốt

| # | Thứ | Chốt |
|---|-----|------|
| 1 | `first-in-range` **tie-break** | Con có **quãng đường đã đi dọc spline LỚN NHẤT** (gần cầu môn nhất). `03` §6 cho phép quân chồng nhau → phải có quy tắc tổng-thứ-tự, không được dựa vào thứ tự trong list. |
| 2 | AoE lan quanh **đâu** | Quanh **vị trí mục tiêu chính**, không phải điểm đạn chạm. Giả định "D10S chạm 3 mục tiêu" (`04` §8 mục 4) dựa trên điều này. |
| 3 | `sut_xuyen` — "đường thẳng" trên đường **cong** | Tia từ **tháp → mục tiêu tại thời điểm bắn**, dài bằng tầm, rộng `lineWidth: 0.5`. Trúng mọi quân giao với tia. Trên khúc cua có thể chỉ trúng 1 con — **đó là ý đồ**: Batigol thưởng cho việc đặt ở đoạn thẳng. |
| 4 | `SaveService` key + version | `lamuralla.save.v1` (JSON string trong PlayerPrefs). **Có trường `version`** ngay từ v1 — thêm sau khi đã có người chơi là không migrate được. |
| 5 | Failure mode ID | `01` §8 đánh số **theo vị trí** (1–21) và 7 test trỏ tới `mục 14/16/17/19/20/…`. Chèn một mục = lệch hết. **Đổi sang `FM-01`…`FM-21` cố định.** |

### `towers.json`

```json
{
  "$schema": "./schema/towers.schema.json",
  "towers": [
    {
      "id": "la_pulga",
      "displayName": "La Pulga",
      "tier": "S",
      "role": "single_target_dps",
      "slotType": "field",
      "levels": [
        { "level": 1, "cost": 300, "damage": 45,  "attackRate": 1.2, "range": 4.5 },
        { "level": 2, "cost": 240, "damage": 72,  "attackRate": 1.2, "range": 5.0 },
        { "level": 3, "cost": 480, "damage": 113, "attackRate": 1.2, "range": 5.5 }
      ],
      "abilities": [
        {
          "id": "solo_run",
          "unlockLevel": 1,
          "trigger": "cooldown",
          "cooldownSec": 12.0,
          "params": {
            "targetSelector": "highest_absolute_hp",
            "damageMultiplier": 2.0,
            "durationSec": 4.0
          }
        },
        {
          "id": "nhan_quan",
          "unlockLevel": 2,
          "trigger": "passive",
          "params": {
            "bonusDamagePercentVsSlowed": 25,
            "neverMissSlowed": true
          }
        },
        {
          "id": "so_10",
          "unlockLevel": 3,
          "trigger": "every_nth_attack",
          "params": { "n": 4, "pierceCount": 2 }
        }
      ]
    }
  ]
}
```

`slotType`: `"field"` (11 ô) | `"goalkeeper"` (1 ô, chỉ `dibu`). `El Árbitro` là `"field"`.

### Tướng 0 DPS — `dibu` và `el_arbitro`

```json
{
  "id": "el_arbitro",
  "displayName": "El Árbitro",
  "tier": null,
  "role": "control_multiplier",
  "slotType": "field",
  "levels": [
    { "level": 1, "cost": 240, "damage": 0, "attackRate": 0, "range": 3.5 },
    { "level": 2, "cost": 192, "damage": 0, "attackRate": 0, "range": 4.0 },
    { "level": 3, "cost": 384, "damage": 0, "attackRate": 0, "range": 4.5 }
  ],
  "abilities": [
    {
      "id": "coi_chi_tay", "unlockLevel": 1, "trigger": "aura",
      "params": { "slowPercent": 25, "scope": "in_range" }
    },
    {
      "id": "the_vang", "unlockLevel": 2, "trigger": "cooldown", "cooldownSec": 8.0,
      "params": {
        "targetSelector": "first_in_range_without_card",
        "slowPercent": 50, "scope": "permanent", "appliesCard": "yellow"
      }
    },
    {
      "id": "the_do", "unlockLevel": 3, "trigger": "cooldown", "cooldownSec": 8.0,
      "params": {
        "targetSelector": "first_in_range_without_card",
        "slowPercent": 70, "scope": "permanent", "appliesCard": "red"
      }
    }
  ]
}
```

🔴 **B-01 (user chốt 2026-07-17): kỹ năng KHÔNG tích luỹ.** Tướng cấp N chạy **đúng một**
kỹ năng — cái có `unlockLevel == N`. Kỹ năng cấp thấp không chạy.

Vì thế `the_do` dùng `first_in_range_without_card`, **không phải** `..._with_yellow_card`
như bản trước của tài liệu này: thẻ vàng do `the_vang` (Lv2) rút, mà Lv2 không còn chạy
ở Lv3 → không ai có vàng → `the_do` đứng im vĩnh viễn. Selector cũ đã bị **xoá khỏi code**;
luật 17 của `ConfigValidator` chặn mọi config gọi tên nó.

Đánh đổi cố ý: Árbitro Lv1 làm chậm 25% **mọi** con trong tầm, liên tục; Lv3 làm chậm
70% **một** con mỗi 8s. Đông quân thì Lv1 tổng lượng chậm cao hơn. Nâng cấp là chọn vai
trò, không phải leo thang.

⚠️ **`scope` là trường quan trọng nhất trong cả file này.**

| `scope` | Nghĩa | Ai dùng |
|---------|-------|---------|
| `"in_range"` | Hiệu ứng **mất khi ra khỏi tầm** | `coi_chi_tay`, `ap_dao` (Dibu Lv2) |
| `"permanent"` | Hiệu ứng **theo quân tới hết đường**, kể cả tướng bị bán | `the_vang`, `the_do` |

Lẫn hai giá trị này = biến tướng khắc chế tank thành cái đèn pin. Xem `01` §8 **FM-16**–17, 20.

**Công thức hệ số cấp (×1.6 / ×2.5) KHÔNG áp lên tướng 0 DPS** — nhân 0 vẫn là 0. Nhưng **công thức giá (0.8× / 1.6×) vẫn áp**, nên luật validate #3 vẫn phải xanh cho cả `dibu` và `el_arbitro`.

### ⚠️ `abilities[]` là mảng, KHÔNG phải object theo cấp

Đây là hệ quả trực tiếp của luật cộng dồn (`02` §3.2):

| Luật thiết kế | Hệ quả lên schema |
|---------------|-------------------|
| Mỗi cấp mở 1 kỹ năng **mới** | Mảng 3 phần tử, mỗi cái có `unlockLevel` |
| Kỹ năng **cộng dồn** | Runtime bật **mọi** ability có `unlockLevel <= currentLevel` |
| Kỹ năng cũ **không tự mạnh lên** | **Không có** `paramsByLevel`. Params là hằng số. |

💡 *Bản trước dùng `cooldownByLevel: [12, 10, 10]` và `damageMultiplierByLevel: [3, 3, 4]` — nghĩa là kỹ năng scale theo cấp. Schema đó giờ **cấm**: nó chính là cái mà `02` §3.2 luật 2 chặn. Nếu thấy hậu tố `ByLevel` trong `abilities[]`, đó là bug, không phải tính năng.*

### `enemies.json` — boss

```json
{
  "enemies": [
    { "id": "adepto", "displayName": "Adepto", "baseHp": 100, "speed": 1.0, "baseBounty": 8,  "leakDamage": 1 },
    { "id": "tifoso", "displayName": "Tifoso Kèn", "baseHp": 220, "speed": 1.4, "baseBounty": 14, "leakDamage": 1 },
    { "id": "tambor", "displayName": "Tambor Giáp", "baseHp": 550, "speed": 0.6, "baseBounty": 25, "leakDamage": 2 }
  ],
  "bosses": [
    {
      "id": "o_capitao",
      "displayName": "O Capitão",
      "speed": 0.4,
      "leakDamage": 5,
      "slowResistPercent": 75,
      "appearances": [
        { "wave": 10, "hp": 5500,  "bounty": 200 },
        { "wave": 20, "hp": 22000, "bounty": 500 }
      ]
    }
  ]
}
```

**Boss KHÔNG dùng `baseHp` + hệ số act.** Máu ghi thẳng cho từng lần xuất hiện — xem `03` §2. Đưa boss vào công thức nhân là sai về bản chất: boss là set-piece được dẫn xuất ngược từ mô hình hoả lực tập trung (`04` §3.3), không phải quân thường được scale.

### `waves.json`

```json
{
  "spawnIntervalSec": 0.7,
  "restBetweenWavesSec": 8.0,
  "hpScaling": {
    "formula": "hpMultiplier(wave) = base × pow(growthPerWave, wave - 1)",
    "base": 0.69, "growthPerWave": 1.09
  },
  "acts": [
    { "id": 1, "waves": [1, 7],   "bountyMultiplier": 1.0 },
    { "id": 2, "waves": [8, 14],  "bountyMultiplier": 1.6 },
    { "id": 3, "waves": [15, 20], "bountyMultiplier": 2.2 }
  ],
  "waves": [
    { "wave": 1,  "spawns": { "adepto": 6,  "tifoso": 0,  "tambor": 0 } },
    { "wave": 7,  "spawns": { "adepto": 6,  "tifoso": 7,  "tambor": 2 } },
    { "wave": 10, "spawns": { "adepto": 6,  "tifoso": 6,  "tambor": 2 }, "boss": "o_capitao" },
    { "wave": 20, "spawns": { "adepto": 14, "tifoso": 16, "tambor": 7 }, "boss": "o_capitao" }
  ]
}
```

**Máu và thưởng KHÔNG ghi trong `waves.json`** — chúng tính ra bằng `baseHp × hpMultiplier(wave)` và `baseBounty × bountyMultiplier` lúc chạy.

⚠️ **`hpMultiplier` là HÀM của wave, không phải hằng số theo act.** Engine tính `base × growth^(wave-1) × latestMilestoneMultiplier(wave)`. Map có thể override `base/growth` nhưng thừa kế milestone W5/W10/W15/W20 từ `waves.json`; milestone không cộng dồn.

⚠️ **`hpMultiplier` phải ĐƠN ĐIỆU TĂNG** — máu MỖI CON quái không bao giờ được giảm giữa hai wave. Kích thước wave thì ĐƯỢC reset ở đầu act (W8, W15): đó là nhịp xả hơi cố ý sau cao trào cuối act. Ghi cả hai chỗ là mời gọi hai chỗ lệch nhau. Boss là ngoại lệ: máu/thưởng nằm ở `enemies.json → bosses[].appearances[]`.

⚠️ **`hpMultiplier` và `bountyMultiplier` là hai trường TÁCH BIỆT — không gộp.** Gộp lại thành một `multiplier` (như bản đầu) làm độ khó thực tế phẳng: quân khoẻ gấp đôi thì người chơi cũng giàu gấp đôi. Xem `03` §3.

**Boss spawn cuối cùng**, sau khi toàn bộ `spawns` của wave đã ra hết. Đây là ràng buộc cân bằng, không phải chi tiết trình bày — mô hình hoả lực tập trung (`04` §3.3) giả định boss đi một mình.

### `economy.json`

```json
{
  "startingCash": 550,
  "goalHealth": 20,
  "waveClearBonus":  { "formula": "20 + 5 * (wave - 1)", "overrides": { "20": 150 } },
  "skipBonusPerSecond": 3,
  "sellRefundRatio": 0.6,
  "upgradeMultipliers": { "damage": [1.0, 1.6, 2.5], "cost": [0, 0.8, 1.6] },
  "goalRepair": { "baseCost": 200, "costGrowth": 1.5, "healAmount": 1 },
  "slowCapPercent": 70,
  "starThresholds": { "one": 1, "two": 10, "three": 20 },
  "balanceModel": {
    "etaWasteFactor": 0.75,
    "sigmaByTowerLevel": [1.10, 1.20, 1.35],
    "vBarAverageSpeed": 1.1,
    "headroomMin": 1.05,
    "headroomMax": 1.65
  }
}
```

`balanceModel` tồn tại để test `Balance_*` đọc được **cùng những con số** mà doc 04 dùng. Không hardcode chúng trong test.

### Validate lúc load (bắt buộc, `ConfigLoader` fail-fast)

| # | Luật | Vi phạm → |
|---|------|-----------|
| 1 | Mọi `id` tướng/quân/boss là duy nhất trên toàn bộ config | Throw, không vào được Menu |
| 2 | `levels` mỗi tướng đúng 3 phần tử, `level` = 1,2,3 | Throw |
| 3 | `cost[2] / cost[0] == 0.8` và `cost[3] / cost[0] == 1.6` (±0.01) | Throw — công thức cấp bị phá |
| 4 | Đúng 20 wave, `wave` chạy 1→20 liên tục | Throw |
| 5 | Mọi `id` trong `waves.spawns` tồn tại ở `enemies.json` | Throw |
| 6 | `slowCapPercent` ≤ 70 | Throw — xem `01` §8 **FM-08** |
| 7 | Đúng 1 tướng có `slotType: "goalkeeper"` | Throw |
| **8** | **Mỗi tướng đúng 3 `abilities`, `unlockLevel` = 1,2,3 mỗi cấp một cái** | Throw — luật cộng dồn `02` §3.2 |
| **9** | **Không key nào trong `abilities[].params` kết thúc bằng `ByLevel`** | Throw — kỹ năng cũ không được tự scale (`02` §3.2 luật 2) |
| **10** | **`hpMultiplier >= bountyMultiplier` ở mọi act** | Throw — nếu thưởng đuổi kịp máu, độ khó phẳng (`03` §3) |
| **11** | **Mọi `waves[].boss` tồn tại ở `bosses[]`, và có `appearances` khớp đúng số wave đó** | Throw |
| **12** | **`bosses[].slowResistPercent` trong [0, 100]** | Throw |
| **13** | **`bosses[].leakDamage < economy.goalHealth`** | Throw — một con boss lọt không được thắng ngay từ máu đầy |
| **14** | **Mọi `abilities[].params.slowPercent` ≤ `economy.slowCapPercent`** | Throw — `01` §8 **FM-08** |
| **15** | **Mọi ability có `slowPercent` phải khai báo `scope`** — `in_range` \| **`timed`** \| `permanent`. `timed` **bắt buộc** có `durationSec` > 0; scope khác **cấm** khai `durationSec`. | Throw — mặc định ngầm ở đây là bug im lặng |
| **16** | **Tướng có `damage: 0` ở mọi cấp thì `attackRate` cũng phải 0** | Throw — bắt lỗi copy-paste |

### 🔴 Luật 15 đã SAI — validator chạy lên config thật mới lộ (vòng 6)

Bản vòng 4 chỉ liệt kê `in_range | permanent`. Thực tế config có **ba** cơ chế chậm khác hẳn nhau về sức mạnh, mà nhìn `slowPercent: 50` thì giống hệt:

| `scope` | Nghĩa | Ai dùng |
|---------|-------|---------|
| `in_range` | Chậm khi còn trong tầm; ra khỏi tầm là hết | `dibu.ap_dao` · `el_arbitro.coi_chi_tay` |
| **`timed`** | Chậm trong `durationSec` rồi hết, **bất kể tầm** | `d10s.ban_tay_cua_chua` (50%, 3s) |
| `permanent` | Chậm vĩnh viễn | `el_arbitro.the_vang` · `the_do` |

**Docs thiếu, config đúng.** `ConfigValidator` viết đúng theo docs thì ném ngay lần chạy đầu lên config thật — đó là cách phát hiện.

Luật siết thêm theo đúng ý đồ gốc ("mặc định ngầm là bug im lặng"):
- `scope: timed` mà thiếu `durationSec` → thời lượng thành **mặc định ngầm** → throw
- `scope` khác `timed` mà vẫn khai `durationSec` → một trong hai trường đang **nói dối**, code sẽ âm thầm bỏ qua một cái → throw

### ⚠️ Luật 10 đã được VIẾT LẠI (vòng 5b)

Bản gốc: *"`hpMultiplier >= bountyMultiplier` ở mọi act"*. Nhưng `hpMultiplier` theo act **đã bị xoá** — nó tạo vách đứng ở W8/W15 (xem `03` §3). **Ý đồ giữ nguyên** (độ khó phải vượt sức mua, không thì độ khó thực tế phẳng), chỉ đổi cách đo:

> `hpScaling.MultiplierAt(act.LastWave) >= act.bountyMultiplier`

Đo ở **cuối mỗi act** — nơi sức mua của act đó đạt đỉnh. Hiện tại: Act 1 `1.16 ≥ 1.0` · Act 2 `2.12 ≥ 1.6` · Act 3 `3.55 ≥ 2.2` ✅

---

## 5. Test tối thiểu (M1 phải có)

| Tầng | Test | Chứng minh |
|------|------|-----------|
| EditMode | `ConfigLoader_RejectsBrokenUpgradeRatio` | Luật validate #3 |
| EditMode | `ConfigLoader_RejectsAbilityParamsThatScaleByLevel` | Luật #9 — `02` §3.2 luật 2 |
| EditMode | `ConfigLoader_RejectsBountyMultiplierExceedingHpMultiplier` | Luật #10 — `03` §3 |
| EditMode | `Economy_KillBountyPaidExactlyOnce` | `01` §8 **FM-07** |
| EditMode | `Economy_SellRefundsSixtyPercentOfTotalInvestment` | Bán 60% gồm cả nâng cấp |
| EditMode | `Balance_WdbMeetsHeadroomForAll20Waves` | **Chạy lại bảng `04` §4 bằng code.** Fail nếu wave nào ra ngoài `headroomMin..Max` |
| EditMode | `Balance_BossFocusFireMeetsHeadroom` | **`04` §3.3.** D10S phải tính DPS **đơn mục tiêu** — fail nếu ai đó nhân ×3 |
| EditMode | `Balance_MaxBuildCostExceedsLifetimeCash` | `04` §6 — tiền luôn khan hiếm. **Margin chỉ còn 2.2%** — test này sẽ đỏ sớm. |
| PlayMode | `Tower_Level3RunsAllThreeAbilitiesSimultaneously` | Luật cộng dồn `02` §3.2 luật 1 |
| PlayMode | `Tower_LowLevelAbilityParamsUnchangedAfterUpgrade` | `02` §3.2 luật 2 |
| PlayMode | `Slow_NeverExceedsSeventyPercent` | Nhiều D10S không cộng dồn slow |
| PlayMode | `Slow_BossCapsAtSeventeenPointFivePercent` | **`01` §8 **FM-14**** — cap 70% **trước**, kháng 75% **sau** |
| PlayMode | `Card_YellowSlowPersistsAfterLeavingArbitroRange` | **`01` §8 **FM-16**** — thẻ theo người, không theo chỗ |
| PlayMode | `Card_AuraDoesNotStackWithCard` | `01` §8 **FM-17** — lấy max, không cộng |
| PlayMode | `Card_YellowThenRedRequiresTwoSeparateApplications` | `01` §8 **FM-19** |
| PlayMode | `Card_SlowPersistsAfterArbitroIsSold` | **`01` §8 **FM-20**** — thẻ đã rút thì không rút lại |
| PlayMode | `Card_RedCardNeverRemovesEnemy` | `02` §4.6 — thẻ đỏ chỉ làm chậm, không xoá sổ ai |
| PlayMode | `Arbitro_DealsZeroDamageAtAllLevels` | `02` §2 |
| PlayMode | `Dibu_BlockedEnemyDealsNoDamageAndDropsNoCash` | `02` §4.5 |
| PlayMode | `Dibu_CanBlockBossAndSavesFiveGoalHealth` | `03` §2 |
| PlayMode | `Boss_SpawnsLastAfterAllRegularEnemies` | Giả định nền của `04` §3.3 |
| PlayMode | `Leak_TamborDeductsTwoGoalHealth` | `03` §1 |
| PlayMode | `Leak_BossDeductsFiveGoalHealth` | `03` §2 |

💡 *`Balance_WdbMeetsHeadroomForAll20Waves` và `Balance_BossFocusFireMeetsHeadroom` là hai test quan trọng nhất: chúng biến doc 04 từ tài liệu chết thành thứ **vỡ build khi ai đó chỉnh một con số**. Không có chúng, doc 04 sẽ lệch khỏi game trong đúng hai tuần.*

💡 *`Slow_BossCapsAtThirtyFivePercent` bắt đúng một lỗi thứ tự phép tính (`01` §8 **FM-14**) — cap trước rồi kháng, không phải ngược lại. Đây là loại bug không bao giờ crash, không bao giờ hiện trong log, chỉ âm thầm làm boss thành bao cát và không ai hiểu vì sao wave 20 dễ thế.*

---

## 6. Quy trình build iOS (test trước, theo yêu cầu)

### Chuẩn bị một lần

1. macOS + **Xcode 16+** (`xcode-select --install` cho command line tools)
2. **Unity Hub** → cài **Unity 6 LTS** kèm module **iOS Build Support**
3. Apple ID (free là đủ để chạy trên máy thật, provisioning 7 ngày)
4. Trong Xcode: Settings → Accounts → thêm Apple ID → Manage Certificates → tạo Apple Development cert

### Cấu hình Unity (Project Settings)

| Mục | Giá trị |
|-----|---------|
| Platform | iOS |
| Bundle Identifier | `com.{team}.lamuralla` |
| Target minimum iOS | **15.0** |
| Scripting Backend | IL2CPP (bắt buộc với iOS) |
| Architecture | ARM64 |
| Orientation | **Portrait only** (khoá xoay) |
| Target Device | iPhone + iPad |
| Camera/Mic/Location usage | Không dùng → **để trống, không thêm quyền nào** |

### Mỗi lần build

```
Unity: File → Build Settings → iOS → Build   →  chọn thư mục ./Builds/iOS/
Xcode: mở Builds/iOS/Unity-iPhone.xcodeproj
       → Signing & Capabilities → Team = Apple ID của bạn
       → chọn thiết bị thật (Simulator KHÔNG chạy được build IL2CPP mặc định)
       → ⌘R
```

### Vấp thường gặp

| Triệu chứng | Nguyên nhân | Xử lý |
|-------------|-------------|-------|
| `Signing for "Unity-iPhone" requires a development team` | Chưa chọn Team | Signing & Capabilities → chọn Team |
| Build lâu bất thường (10+ phút) | IL2CPP transpile toàn bộ | Bình thường ở build sạch đầu tiên. Build sau nhanh hơn nhiều. |
| App bật lên rồi tắt ngay trên máy thật | Provisioning free hết hạn (7 ngày) | Build lại từ Xcode |
| Sprite mờ trên iPhone Pro | Chưa set Reference Resolution | Canvas Scaler → Scale With Screen Size → 1080×1920 |

**Android để sau M2** — không cấu hình gì ở M0/M1. Một nền tảng một lúc.

---

## 7. Hiệu năng — ngân sách

| Chỉ số | Mục tiêu | Vì sao đạt được |
|--------|----------|-----------------|
| FPS | 60 ổn định trên iPhone 11 | Tối đa 37 quân + ~12 tướng + ~40 đạn trên màn |
| Draw call | < 50 | Sprite Atlas cho toàn bộ `Art/Sprites/` |
| Cấp phát/frame | **0 byte** trong vòng lặp chiến đấu | `ObjectPool` cho quân, đạn, VFX, số sát thương bay lên |
| Thời gian load | < 2s từ Boot đến Menu | Config JSON tổng < 50KB |
| Build size | < 80MB | Không dùng asset 3D, không video |

💡 *Ngân sách "0 byte cấp phát mỗi frame" là thứ dễ đạt lúc đầu và gần như không thể cứu vãn về sau. Đạn được `Instantiate`/`Destroy` mỗi phát bắn là nguyên nhân số một gây khựng GC trong TD — pool ngay từ ngày đầu, đừng đợi tới lúc profile.*
