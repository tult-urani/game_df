# 08 — KIẾN TRÚC ĐA MAP

> **Trạng thái:** 🟢 **G1 + đợt 1 ĐÃ LÀM** (2026-08-21). Xem §11 để biết cái gì xong, cái gì chưa.
> Đợt 2 (7 map đa tuyến) vẫn là plan.
> **Phạm vi:** thêm 10 map (M01–M10) cạnh map gốc. Chi tiết từng map: `docs/maps/M*.md`.
> **Tài liệu này trả lời:** phải đổi gì trong code để 10 map kia tồn tại được.

---

## 0. Thang 11 map — số đang chạy

> Sinh từ `config/maps/*.json` bằng `tools/gen_docs.py`. Thứ tự cột `#` là thứ tự
> mở khoá. `MapPlayableTests` giữ đúng thang này bằng test trên engine thật —
> đổi số ở JSON mà không chạy lại test thì test đỏ.

<!-- GEN:map_ladder -->
| # | Map | Tuyến | Ô | Σchord/ô | Tiền đầu | growth | Quân | Sao |
|---|-----|-------|---|----------|----------|--------|------|-----|
| 0 | **La Muralla** `m00` | 1 | 11 | 1.74 | 700 | 1.09 | 465 | ★ |
| 1 | **El Potrero** `m01` | 1 | 11 | 1.79 | 700 | 1.12 | 429 | ★ |
| 2 | **La Bombonera** `m02` | 1 | 10 | 2.90 | 700 | 1.12 | 400 | ★★ |
| 3 | **Dos Rios** `m03` | 2 | 12 | 3.30 | 800 | 1.08 | 413 | ★★ |
| 4 | **El Cruce** `m04` | 2 | 13 | 2.86 | 800 | 1.075 | 465 | ★★★ |
| 5 | **La Confluencia** `m05` | 2 | 11 | 4.36 | 600 | 1.07 | 403 | ★★★ |
| 6 | **El Caracol** `m06` | 1 | 9 | 2.63 | 700 | 1.12 | 246 | ★★★★ |
| 7 | **Tres Puertas** `m07` | 3 | 13 | 3.79 | 700 | 1.07 | 301 | ★★★★ |
| 8 | **El Mirador** `m08` | 2 | 14 | 2.44 | 800 | 1.07 | 335 | ★★★★ |
| 9 | **La Horquilla** `m09` | 2 | 11 | 3.66 | 700 | 1.07 | 346 | ★★★★★ |
| 10 | **La Muralla Final** `m10` | 3 | 17 | 2.88 | 800 | 1.07 | 314 | ★★★★★ |
<!-- /GEN:map_ladder -->

---

## 1. Vấn đề: game đang cứng ở "đúng một map"

Không có khái niệm map/level/progression nào trong codebase. Không phải "có nhưng chưa
dùng" — là **không tồn tại**. `grep -rn "MapId\|LevelId\|currentMap\|mapIndex"` trên toàn
`Assets/_Project/` trả về 0 kết quả.

Toàn bộ hình học sống trong **một** `config/path.json`, và nó được nối cứng vào 11 chỗ:

| # | Chỗ | Giả định một-map | Mức độ |
|---|-----|------------------|--------|
| 1 | `Core/Config/ConfigModel.cs:219` | `GameConfig.Path` là MỘT `PathDef`, không phải danh sách | 🔴 nền |
| 2 | `Core/Config/ConfigModel.cs:184` | `PathDef.Waypoints` là MỘT danh sách điểm → một tuyến | 🔴 nền |
| 3 | `Core/Config/ConfigMapper.cs:15` | `Map(towersJson, enemiesJson, wavesJson, economyJson, pathJson)` — 5 chuỗi cố định | 🔴 nền |
| 4 | `Core/Match/MatchController.cs:30` | `private readonly EnemyPath _path;` — MỘT đường | 🔴 nền |
| 5 | `Core/Match/MatchController.cs:111` | `_path = new EnemyPath(cfg.Path.Waypoints)` | 🔴 nền |
| 6 | `Core/Match/MatchController.cs:243,259,264` | spawn / xét lọt lưới / cập nhật vị trí đều bám `_path` | 🔴 nền |
| 7 | `Core/Match/Enemy.cs` | **không có trường lane** | 🔴 nền |
| 8 | `Core/Match/WaveSpawner.cs:9` | `ScheduledSpawn` **không có trường lane** | 🔴 nền |
| 9 | `Core/Match/SlotManager.cs:101` | ép `economy.fieldSlots` khớp số ô trong `path.json` | 🟡 chặn số ô đổi theo map |
| 10 | `Unity/BakedConfig.cs:27-31` | đúng 5 trường chuỗi JSON | 🟡 đóng gói |
| 11 | `Unity/Pitch.cs:98` | `Road(EnemyPath path, ...)` vẽ MỘT con đường | 🟡 hiển thị |

Ngoài code còn hai công cụ cũng giả định một map:
`tools/path_check.py` và `tools/balance_sim.py` đều mở thẳng `config/path.json`.

**Kết luận:** đây KHÔNG phải việc "thêm 10 file JSON". Nền dữ liệu phải đổi trước, và
đa tuyến (mục 3) là thay đổi sâu nhất vì nó chạm vòng lặp mô phỏng.

---

## 2. Schema đề xuất

### 2.1 Tách "map" thành đơn vị độc lập

```
config/
  towers.json          ← DÙNG CHUNG, không đổi theo map
  enemies.json         ← DÙNG CHUNG (định nghĩa quân + boss gốc)
  maps/
    m00-la-muralla.json   ← map gốc hiện tại, chuyển nguyên trạng
    m01-el-potrero.json
    ...
    m10-la-muralla-final.json
```

Mỗi file map tự chứa **hình học + wave + kinh tế** của riêng nó:

```jsonc
{
  "id": "m01",
  "displayName": "El Potrero",
  "order": 1,                    // thứ tự mở khoá
  "difficulty": 1,               // 1..5, chỉ để hiển thị

  "units": { /* giữ nguyên khối `units` của path.json hiện tại */ },
  "ui":    { /* giữ nguyên khối `ui` */ },

  "lanes": [                     // 🔴 MỚI: luôn là MẢNG, kể cả map 1 tuyến
    {
      "id": "L1",
      "interpolation": "catmull-rom",
      "lengthUnits": 42.28,
      "spawnPoint": { "x": 0.0, "y": 9.36 },
      "goalPoint":  { "x": 0.0, "y": -9.1 },
      "waypoints": [ /* ... */ ]
    }
  ],

  "slots": [ { "id": "f01", "type": "field", "x": -2.2, "y": 8.3 } ],

  "economy": {                   // 🔴 GHI ĐÈ economy.json dùng chung
    "startingCash": 700,
    "goalHealth": 20
  },

  "waves": [                     // như waves.json hiện tại, thêm `lane`
    { "wave": 1, "spawns": [ { "enemy": "adepto", "count": 8, "lane": "L1" } ] }
  ],

  "hpScaling":   { "base": 0.67, "growthPerWave": 1.09 },
  "bossOverrides": { "o_capitao": { "slowResistPercent": 78 } }
}
```

### 2.2 Ba quyết định schema và lý do

**(a) `lanes` LUÔN là mảng, kể cả map một tuyến.**
Nếu để `path` (một) cho map cũ và `lanes` (nhiều) cho map mới thì mọi chỗ đọc hình học
phải rẽ nhánh, và cái rẽ nhánh đó sẽ sinh bug im lặng đúng kiểu repo này đã dính nhiều
lần (xem `waves.json → _whyNotPerAct`, `towers.json → _redesign`). Một dạng dữ liệu duy
nhất; map một tuyến là mảng một phần tử.

**(b) `spawns` đổi từ object sang MẢNG.**
Hiện tại là `"spawns": { "adepto": 8, "tifoso": 0 }` — không chỗ nào nhét `lane` vào
được, và cũng không diễn tả được "8 adepto tuyến trái + 6 adepto tuyến phải" vì key trùng.
Đổi sang mảng `[{enemy, count, lane, delaySec?}]` giải cả hai. Đây là **breaking change**
với `ConfigMapper`, `WaveSpawner`, `ConfigValidator` luật #5, và ~4 test.

**(c) `fieldSlots` BỎ khỏi `economy.json`, suy từ `slots` của map.**
`SlotManager.cs:101` đang ép hai nguồn phải khớp. Khi số ô đổi theo map thì con số dùng
chung ở `economy.json` thành vô nghĩa. Suy ra từ map là **một nguồn chân lý**, và
`maxBuildCost` cũng phải tính theo map (xem mục 5).

---

## 3. Đa tuyến — phần khó nhất

### 3.1 Cái gì phải đổi

| Thứ | Từ | Thành |
|-----|----|----|
| `MatchController._path` | `EnemyPath` | `IReadOnlyDictionary<string, EnemyPath>` theo `laneId` |
| `Enemy` | không có lane | thêm `string LaneId { get; }`, gán lúc spawn, bất biến |
| `ScheduledSpawn` | `(enemyId, timeSec, hp, isBoss)` | thêm `laneId` |
| `MatchController` cập nhật vị trí | `_path.PositionAt(d)` | `_lanes[e.LaneId].PositionAt(d)` |
| `MatchController` xét lọt lưới | `d >= _path.Length` | `d >= _lanes[e.LaneId].Length` |
| `Pitch.Road` | vẽ 1 đường | vẽ N đường, mỗi tuyến một màu/độ mờ khác nhau |

### 3.2 Ba kiểu ra quân, và cái nào cần code thêm

| Kiểu | Map dùng | Cần gì |
|------|----------|--------|
| **Luân phiên** (wave lẻ tuyến A, chẵn tuyến B) | M03 | Chỉ cần `lane` trong wave — **không cần code lịch mới** |
| **Đồng thời** (hai tuyến cùng lúc) | M04, M07, M10 | Cũng chỉ cần `lane`; `WaveSpawner` đã sinh lịch theo thời gian, chỉ việc sinh hai chuỗi rồi trộn theo `timeSec` |
| **Lệch nhịp** (tuyến B bắt đầu sau tuyến A 6 giây) | M07 | Cần thêm `delaySec` cho mỗi nhóm spawn |

👉 Điểm đáng mừng: **cả ba kiểu đều KHÔNG cần cơ chế lập lịch mới**, vì `ScheduledSpawn`
vốn đã là danh sách `(thời điểm, quân)`. Chỉ cần sinh nhiều chuỗi rồi trộn theo `TimeSec`.

### 3.3 Tách/nhập tuyến (M05 hợp lưu, M09 tách rồi nhập)

⚠️ **Đây là ca KHÓ NHẤT và tôi đề xuất KHÔNG làm nó bằng đồ thị đường.**

Cách rẻ và đủ dùng: **tuyến vẫn là đường độc lập từ đầu tới cuối, chỉ là hình học của
chúng TRÙNG NHAU ở đoạn hợp lưu.** Hai tuyến M05 chạy riêng nửa đầu rồi có waypoint nửa
sau giống hệt nhau. Người chơi thấy "hai dòng nhập một"; engine vẫn chỉ có hai đường
độc lập, không cần đồ thị, không cần chia quân theo tỉ lệ ở nút rẽ.

**Đánh đổi phải nói rõ:** quân trên hai tuyến trùng nhau sẽ đi CHỒNG LÊN NHAU về mặt hình
ảnh ở đoạn chung (hai con có thể đứng đúng một chỗ). Cách chữa rẻ: lệch hai tuyến ~0.25
unit ở đoạn chung — nhìn ra là hai làn sát nhau, và AoE vẫn quét được cả hai vì bán kính
lan nhỏ nhất là 0.8.

Nếu sau này thật sự cần rẽ nhánh động thì mới làm đồ thị. Đừng làm trước.

---

## 3B. 🔴 `slowResistPercent` KHÔNG phải núm xoay — nó là VÁCH ở 100

**Phát hiện lúc lập plan (2026-08-21), đã kiểm bằng code. Nó lật một phần khung độ khó
ban đầu, nên phải sửa TRƯỚC khi code map.**

Ý tưởng "map càng cao thì boss kháng làm chậm càng mạnh" giả định kháng chậm làm yếu dần
synergy slow+dame. **Nó không làm thế.** Hai buff phụ thuộc "đang bị chậm" đều kiểm
**nhị phân**:

| Chỗ | Điều kiện |
|-----|-----------|
| `Core/Match/AbilityEngine.cs:61` — `nhan_quan` (La Pulga Lv2, +25%) | `if (slowOnTarget > 0)` |
| `Core/Match/AbilityEngine.cs:130` — `ban_thang_the_ky` (D10S Lv3, +20% cho MỌI tướng) | `if (... && ctx.SlowPercentOn(target.Id) > 0)` |

`SlowPercentOn` trả về giá trị **đã trừ kháng** (`MatchController.cs:583` →
`SlowStack.EffectiveSlowPercent` = `min(nguồn, cap 70) × (1 − kháng/100)`). Nhưng vì so
sánh là `> 0`, mọi mức kháng dưới 100 đều cho **cùng một kết quả**:

| kháng | slow hiệu dụng | buff +25% | buff +20% | tổng nhân sát thương |
|------:|---------------:|:---------:|:---------:|---------------------:|
| 0% | 70.0% | BẬT | BẬT | **1.50×** |
| 75% | 17.5% | BẬT | BẬT | **1.50×** |
| 85% | 10.5% | BẬT | BẬT | **1.50×** |
| 90% | 7.0% | BẬT | BẬT | **1.50×** |
| 99% | 0.7% | BẬT | BẬT | **1.50×** |
| 100% | 0.0% | tắt | tắt | 1.00× |

→ Nâng kháng 75 → 90 qua 10 map chỉ đổi **tốc độ đi** của boss (chậm 17.5% → chậm 7%),
còn **sát thương thì y hệt**. Đây là một núm xoay giả.

### Hai cách sửa

**(A) Ngưỡng — `slowedBuffThresholdPercent` (mặc định 15).** Buff bật khi
`slowHiệuDụng > ngưỡng`.
- kháng 0 → 70% → BẬT · kháng 75 → 17.5% → BẬT (giữ nguyên hành vi hiện tại)
- kháng 80 → 14% → **TẮT** · kháng 85/90 → TẮT
- Ưu: thay đổi nhỏ nhất, cân bằng hiện tại KHÔNG đổi (boss đang ở kháng 75).
- Nhược: chỉ có **2 trạng thái** bật/tắt, không phải dải mượt cho 10 map.

**(B) Buff TỈ LỆ với mức chậm thật — `buff = bonus × (slowHiệuDụng / cap)`.** ⭐ đề xuất
- kháng 0 → ×1.00 → +25%/+20% đầy đủ (**quân thường không đổi gì**)
- kháng 75 → ×0.25 → +6.25%/+5%
- kháng 90 → ×0.10 → +2.5%/+2%
- Ưu: núm xoay **thật và mượt**, đúng trực giác "kháng càng cao synergy càng vô dụng".
  Quân thường (kháng 0) hoàn toàn không đổi → không đụng cân bằng wave.
- Nhược: boss YẾU ĐI đáng kể trong tay build synergy → **máu boss phải đo lại**. Ở map
  gốc boss đang kháng 75, đổi sang (B) là boss mất buff 1.50× → 1.31×, tức phe thủ giảm
  ~13% hoả lực lên boss. Phải chạy lại quét máu boss.

### Ảnh hưởng tới bảng độ khó 10 map

Bảng kháng chậm 75/78/80/82/85/88/90 tôi chốt ban đầu **chỉ có nghĩa nếu chọn (B)**.
Nếu chọn (A) thì nên đơn giản hoá còn hai mức: **75** (synergy còn tác dụng) và **85**
(synergy tắt), và dùng nó như một CÔNG TẮC thiết kế chứ không phải thang điểm.

⚠️ Dù chọn cách nào, đây là **thay đổi code lõi có test bọc** → phải làm ở **G1**, trước
khi cân bất kỳ map nào. Cân map trên luật cũ rồi đổi luật sau = cân lại từ đầu.

---

## 3C. Luật `MAX_GAP` chặn mọi map đa tuyến

`tools/path_check.py:235` đặt `MAX_GAP = 0.15` — đoạn đường dài nhất không ô nào canh
không được quá 15% chiều dài đường. Luật này viết cho map **một tuyến, 11 ô**.

Với map đa tuyến, cùng ngân sách ~10–13 ô phải chia cho 2–3 tuyến, nên mật độ ô trên mỗi
tuyến tụt xuống và đoạn hở tăng theo. Sàn của nó xấp xỉ `số_tuyến × 1.2 / số_ô`, tức
~0.24 với map 2 tuyến 10 ô và ~0.30 với map 3 tuyến 12 ô — **đều vượt 0.15**.

→ `MAX_GAP` phải thành **tham số theo map** (khai trong file map), không phải hằng số
toàn cục. Nếu không thì `path_check` sẽ báo đỏ cho mọi map đa tuyến khả thi, và cách
"chữa" duy nhất là nhồi thêm ô — tức phá luôn ý đồ khan hiếm ô.

---

## 4. Progression — mở khoá map

Hiện chưa có gì để lưu tiến độ. Đề xuất tối thiểu:

- `PlayerPrefs` khoá `lamuralla.progress.v1` → JSON `{ "m01": 3, "m02": 2 }` (map → số sao).
- Map `order = N` mở khi map `order = N-1` đạt **≥1 sao**. Không ép 3 sao, nếu không người
  chơi kẹt cứng.
- Màn chọn map: lưới thẻ, hiện tên + số sao + độ khó. Vẽ bằng IMGUI như HUD hiện tại để
  không phải dựng hệ UI mới.

⚠️ Đây là **scope mới hoàn toàn** (màn hình thứ hai — game hiện chỉ có một scene `Match`).
Xem mục 7 để biết vì sao tôi đề xuất tách nó thành giai đoạn riêng.

---

## 5. Kinh tế theo map — ràng buộc dễ vỡ nhất

Trần chi tiêu tính theo map:

```
trần(map) = số_ô_sân(map) × cost(la_pulga full = 1020) + cost(dibu full = 476)
tiền_cả_trận(map) PHẢI < trần(map)
```

Với số ô thay đổi 9→13, trần dao động **9656 → 13736**. Map ít ô (M06 chỉ 9 ô) là chỗ
ràng buộc CHẶT nhất: chỉ được kiếm dưới 9656 cả trận.

🔴 **Bài học vòng 22, đừng lặp lại:** tăng số quái làm tiền cả trận vọt lên và **vỡ trần
16%** mà không luật validator nào chặn — nó chỉ sống trong ghi chú `_marginNote`. Với 11
map thì phải có **luật validator thật**, không phải ghi chú.

→ Đề xuất **luật #21**: `tiền_cả_trận_lý_thuyết < trần(map)`, chạy cho MỌI map, chặn build.

---

## 6. Kiểm chứng — phần quan trọng nhất của plan này

### 6.1 Một map không có test winnability là một map chưa biết có chơi được không

Hiện có ĐÚNG MỘT test kiểu đó: `MatchControllerTests.Nguoi_choi_biet_tieu_tien_thi_thang_duoc_20_wave`.
Nó chạy engine thật 20 wave với người chơi tham lam. Ở vòng 22 chính nó bắt được việc
map không thắng nổi, trong khi `balance_sim.py` vẫn báo "quá dễ".

🔴 **`balance_sim.py` KHÔNG dùng được để chốt số.** Đo vòng 22: sim nói headroom trung
bình **2.09** (quá dễ, 18/20 wave ngoài dải) trong khi engine thật thắng sát nút còn
**11/20 máu**. Ba hệ số η/σ/τ của sim là ước lượng chưa đo, và `economy.json` tự cảnh báo
chúng nhân nhau thì lệch tới 73%. Engine mới là thước đo.

→ **Mỗi map PHẢI có một test winnability.** 11 map = 11 test. Đây là phần khối lượng lớn
nhất và cũng là phần không được cắt.

### 6.2 Ba mức test cho mỗi map

| Mức | Test | Chặn cái gì |
|-----|------|-------------|
| Hình học | `path_check` chạy được trên map → độ dài khớp khai báo ±2%, hộp bao trong viewport, không ô chết, không đoạn đường quá dài không ai canh | map vẽ sai, ô đặt ngoài màn |
| Cấu hình | 20 luật validator hiện có + luật #21 (trần chi tiêu) chạy cho MỌI map | số vô lý lọt vào build |
| **Chơi được** | 20 wave engine thật, người chơi tham lam → **PHẢI Won** | map bất khả thi |

### 6.3 Test winnability phải ĐO chứ không đoán

`startingCash = 700` của map gốc là số **đo được**, không phải gõ: 650 thua W10 · 670 tới
W20 rồi chết 0/20 máu · **700 thắng còn 11/20** · 730 và 760 y hệt 700 (bão hoà). Vách
chuyển rất sắc.

→ Mỗi map cần chạy quét tương tự để tìm `startingCash`. Nên viết **một tiện ích quét**
(`tools/` hoặc một test `[Theory]`) thay vì dò tay 11 lần.

⚠️ **Cảnh báo về "người chơi tham lam":** fixture hiện tại có DANH SÁCH Ô GÕ CỨNG
(`crowd` = f01,f11,f07,f08,f05,f10,f02 · `reach` = f06,f03,f04,f09). Với 11 map có bố cục
ô khác nhau, danh sách này vô nghĩa. **Phải viết lại fixture thành chọn ô theo `chord`
(đoạn đường nằm trong tầm) tính từ hình học map**, nếu không mỗi map lại phải sửa tay
một fixture và ta quay về đúng cái nợ vừa trả.

---

## 7. Thứ tự triển khai đề xuất

Chia 4 giai đoạn. **Mỗi giai đoạn kết thúc bằng một game chạy được**, không phải bằng
"code xong chờ ráp".

| GĐ | Nội dung | Xong thì có gì | Rủi ro |
|----|----------|----------------|--------|
| **G1** | Schema map + `lanes` mảng + chuyển map gốc thành `m00.json`. Chưa thêm map mới, chưa đa tuyến thật (mọi map 1 tuyến). Sửa `path_check`/`balance_sim` nhận tham số map. Fixture test chọn ô theo chord. | Game y hệt hôm nay, nhưng nền đã đa map. **210 test phải vẫn xanh.** | Thấp — thuần refactor, có test bọc |
| **G2** | Map 1 tuyến: **M01, M02, M06, M08** (không cần lane). Mỗi map một test winnability. | 5 map chơi được | Trung bình — chủ yếu là cân số |
| **G3** | Đa tuyến: `LaneId` vào `Enemy`/`ScheduledSpawn`, `Pitch` vẽ N đường. Map **M03, M04, M05, M07, M09, M10**. | Đủ 11 map | 🔴 Cao — chạm vòng lặp mô phỏng |
| **G4** | Màn chọn map + lưu tiến độ + mở khoá. | Sản phẩm hoàn chỉnh | Trung bình — scope UI mới |

**Vì sao G1 tách riêng và không thêm map nào:** đổi schema mà đồng thời thêm 10 map thì
lúc test đỏ sẽ không biết do schema hay do số của map. Tách ra thì G1 có một tiêu chí
đúng/sai duy nhất và rất mạnh: *210 test cũ vẫn xanh, game chạy y hệt*.

**Vì sao G4 sau cùng:** 10 map không có màn chọn thì vẫn test được (đổi map bằng config).
Màn chọn không có map thì chẳng để làm gì. Nên map trước, vỏ sau.

---

## 8. Công cụ phải sửa

| Công cụ | Sửa gì |
|---------|--------|
| `tools/path_check.py` | nhận `--map <id>`; kiểm TỪNG tuyến; thêm kiểm "hai tuyến có chồng hình không" |
| `tools/balance_sim.py` | nhận `--map <id>`; `Geometry` tính chord trên NHIỀU tuyến; **và phải ghi rõ ở đầu output rằng số của nó chưa được hiệu chuẩn** |
| `tools/gen_docs.py` | sinh bảng wave cho từng map vào `docs/maps/M*.md` |
| MỚI: quét `startingCash` | dò nhị phân trên engine thật cho từng map |

---

## 9. Rủi ro

| # | Rủi ro | Vì sao đáng lo | Giảm thiểu |
|---|--------|----------------|------------|
| R1 | **11 map × 20 wave = 220 wave cần cân.** Vòng 22 cân MỘT map đã tốn cả buổi và phát hiện mô hình sai | Đây là rủi ro lớn nhất của cả plan, lớn hơn mọi vấn đề code | Tự động hoá quét `startingCash`; chấp nhận "chơi được" thay vì "hoàn hảo" ở lần đầu |
| R2 | `balance_sim.py` sai mà vẫn được dùng để chốt số | Đã xảy ra ở vòng 22 | In cảnh báo ngay trong output của sim; mọi số chốt phải qua test engine |
| R3 | Fixture "người chơi tham lam" gõ cứng danh sách ô | 11 map = 11 fixture sửa tay = nợ mới | Bắt buộc làm ở G1, không hoãn |
| R4 | Hiệu năng map 3 tuyến (M07, M10) | ✅ **ĐÃ ĐO** — xem 9.1 | Đặt trần **40 quân sống cùng lúc**; map đa tuyến phải đo lại |
| R5 | Đoạn đường trùng nhau ở M05/M09 làm quân chồng hình | Nhìn ra ngay, trông như bug | Lệch hai làn 0.25 unit (mục 3.3) |
| R6 | Scope phình: màn chọn map kéo theo lưu tiến độ, sao, mở khoá, có thể cả nút reset | Game hiện chỉ có MỘT scene | G4 tách riêng, có thể cắt nếu cần ship sớm |

---

### 9.1 Số đo hiệu năng (đo 2026-08-21, map gốc, engine thật)

**Đỉnh quân sống cùng lúc cả trận: 27 con** (ở W20).

Con số này KHÁC hẳn "W20 có 47 con" — 47 là tổng SINH RA trong wave, còn quân chết dần
trên đường nên chưa bao giờ cùng sống. Diễn tiến đỉnh: W4 = 11 · W7 = 15 · W10 = 19 ·
W11 = 20 · W19 = 24 · W20 = 27.

Ý nghĩa cho thiết kế map:
- Rủi ro FPS **nhỏ hơn dự đoán ban đầu**. Mỗi quân hiện là ~4 GameObject (cha + art +
  Animator + thanh máu) → đỉnh hiện tại ≈ 108 object, không đáng lo.
- **Trần đề xuất: 40 quân đồng thời.** Map 3 tuyến (M07, M10) dễ vượt vì ba dòng cùng
  chảy. Phải đo lại đỉnh cho từng map đa tuyến, KHÔNG suy từ tổng số quân.
- Cách đo: chạy engine 20 wave, lấy `max(m.Enemies.Count)` mỗi tick. Nên gắn thẳng vào
  test winnability của mỗi map để khỏi đo tay.

---

## 10. Câu hỏi cần user quyết

1. **Map gốc có nằm trong 11 map không**, hay 10 map mới thay thế nó? (Tài liệu này giả định map gốc thành `m00` và được giữ.)
2. **Có làm màn chọn map (G4) trong lần này không**, hay tạm thời đổi map bằng config để chơi thử?
3. **Ưu tiên nếu phải cắt:** ít map hơn nhưng cân kỹ, hay đủ 10 map nhưng cân thô rồi chỉnh sau?


---

## 11. Đã làm — G1 + đợt 1 (2026-08-21)

| Việc | Trạng thái | Bằng chứng |
|------|:---:|------------|
| **L1** — buff phụ thuộc-chậm nhị phân → TỈ LỆ (phương án B) | ✅ | `AbilityEngine.SlowRatio`; test `Khang_cham_lam_YEU_buff_chu_khong_phai_bat_tat` — kháng 0/75/90% giờ cho ba giá trị khác nhau |
| `path` (một) → `lanes` (MẢNG) | ✅ | `LaneDef`, `PathDef.Lanes`; `config/maps/*.json` |
| `config/path.json` → `config/maps/m00-la-muralla.json` | ✅ | m00 KHÔNG khai override nào → cân bằng chứng minh được là không đổi |
| File map ghi đè `waves`/`acts`/`hpScaling`/`economy`/`bossOverrides` | ✅ | `ConfigMapper.MapBosses`, `MapEconomy(o, map)` |
| `MAX_GAP` thành tham số theo map | ✅ | `maxGapFraction` trong file map; `path_check.py` đọc từ đó |
| `path_check.py` / `balance_sim.py` nhận `--map` | ✅ | `load_map()`; báo lỗi kèm danh sách map có sẵn |
| **R3** — bỏ danh sách ô gõ cứng trong fixture | ✅ | `GreedyPlayer` chọn ô theo chord đo từ hình học. Đối chiếu: luật suy-từ-chord tái tạo ĐÚNG danh sách gõ cứng cũ của m00 và ra đúng kết quả cũ (11/20 máu, kiếm 11245) |
| **Luật #21** — trần chi tiêu theo map | ✅ | `MapPlayableTests.Tien_ca_tran_khong_vuot_tran_chi_tieu`, chạy cho MỌI map |
| Bài kiểm "map có chơi được không" cho MỖI map | ✅ | `MapPlayableTests`, 4 map |
| Trần 40 quân đồng thời | ✅ | kiểm trong chính bài kiểm đó |
| Bake MỌI map + chọn map lúc chạy | ✅ | `ConfigBaker` validate từng map; `MatchView.MapId` |
| 3 map đợt 1: M01, M02, M06 | ✅ | `config/maps/` |

### Số đo cuối (engine thật, `GreedyPlayer`)

| Map | Ô | startingCash | Kết cục | Đỉnh quân | Tiền/trần | Biên |
|-----|--:|--:|---|--:|---|--:|
| m00 La Muralla | 11 | 700 | Won · 11/20 máu | 27 | 11245 / 11696 | 3.9% |
| m01 El Potrero | 11 | 700 | Won · 10/20 | 22 | 10041 / 11696 | 14.2% |
| m02 La Bombonera | 10 | 700 | Won · 10/20 | 25 | 9571 / 10676 | 10.4% |
| m06 El Caracol | 9 | 600 | Won · 8/20 | 32 | 8668 / 9656 | 10.2% |

`dotnet test`: **219/219**. `path_check --map`: 4/4 đạt. Unity assembly: 0 lỗi.

### CHƯA làm — phải nói rõ

1. **Máu boss chưa đo lại sau L1.** Cơ chế được chứng minh ở mức unit test, nhưng
   `GreedyPlayer` dựng build D10S Lv2 + La Pulga Lv3 — theo luật B-01 build đó KHÔNG có
   nguồn làm chậm nào, nên nó không hề chạm tới buff synergy. **Cần thêm một fixture build
   synergy** (El Árbitro + D10S Lv3) thì mới đo được ảnh hưởng thật của kháng chậm lên boss.
2. **`spawns` vẫn là object, chưa thành mảng.** Lý do đổi là để nhét `lane`, mà cả ba map
   đợt 1 đều một tuyến. Đẩy sang đợt 2 để migrate một lần thay vì hai.
3. **Đa tuyến (`LaneId`) chưa động tới** — đó là đợt 2.
4. **Màn chọn map chưa có** — đổi map bằng `MatchView.MapId` trong Inspector.
5. **`balance_sim.py` vẫn báo số không tin được** (18–22 wave "ngoài dải" ở cả 4 map)
   trong khi engine nói cả 4 đều thắng. Nó chạy được `--map` nhưng ĐỪNG dùng để chốt số.
