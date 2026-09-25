# 06 — Art & UX

---

## 1. Hướng nghệ thuật

**Một câu:** sân bóng nhìn từ trên xuống như bàn cờ, nhân vật chibi 2D vẽ tay, tông vui — không phải trận chiến, mà là một trận đấu.

| Trục | Chọn | Không chọn |
|------|------|-----------|
| Góc nhìn | Top-down thẳng đứng (bird's eye) | Isometric, side-view |
| Tỉ lệ nhân vật | Chibi ~3 đầu, dễ đọc ở kích thước 64px | Tỉ lệ thực |
| Nét | Vẽ tay, viền đậm, màu phẳng + 1 lớp shading | Pixel art, 3D, vector phẳng không viền |
| Tông | Vui, hài hước, đầy màu | Nghiêm túc, tối, "chiến tranh" |
| Bạo lực | **Không.** Cổ động viên bị "đẩy ngã ra khỏi sân", không chảy máu, không chết | Máu, thương tích |

💡 *Quyết định "không bạo lực" không phải để làm màu — nó giữ App Store rating ở 4+, mở cửa cho lượng người chơi lớn nhất, và hợp với fantasy: đây là cổ động viên tràn sân, không phải kẻ thù.*

---

## 2. Bảng màu

| Vai trò | Màu | Hex | Dùng ở đâu |
|---------|-----|-----|-----------|
| Phe nhà (Argentina) | Xanh nhạt | `#75AADB` | Tướng, HUD, nút xác nhận |
| Phe nhà — phụ | Trắng | `#FFFFFF` | Sọc áo, viền |
| Phe địch (Bồ Đào Nha) | Đỏ thẫm | `#C8102E` | Cổ động viên, thanh máu địch |
| Phe địch — phụ | Xanh lá đậm | `#006847` | Chi tiết, kèn, trống |
| Sân | Xanh cỏ | `#4C9A2A` | Nền |
| Đường chạy | Cỏ nhạt hơn | `#6FBF44` | Đường quân đi (khác nền đủ để đọc, không chói) |
| Ô đặt trống | Trắng 30% | `#FFFFFF4D` | Vòng tròn đứt nét |
| Tiền | Vàng | `#F2C14E` | Peso, số tiền bay lên |
| Trọng tài | Đen | `#1A1A1A` | Áo trọng tài — **màu duy nhất không thuộc phe nào**, đọc ngay là "người thứ ba" |
| Thẻ vàng | Vàng thẻ | `#FFD32C` | Khác vàng-tiền `#F2C14E` đủ để không nhầm |
| Thẻ đỏ | Đỏ thẻ | `#E4003A` | Khác đỏ-địch `#C8102E` và đỏ-nguy-hiểm `#E03131` |
| Cảnh báo | Cam | `#F2762E` | Máu cầu môn < 30% |
| Nguy hiểm | Đỏ | `#E03131` | Máu cầu môn < 10%, nút bán |

**Kiểm tra bắt buộc:** đỏ thẫm `#C8102E` trên xanh cỏ `#4C9A2A` là cặp màu **mù màu đỏ-lục đọc rất kém** (~8% nam giới). Bù bằng **hình dáng, không chỉ màu**: cổ động viên có silhouette riêng biệt (kèn / trống / tay không) và viền trắng dày 2px quanh mọi nhân vật địch.

### ⚠️ Bảng màu giờ có 3 sắc đỏ và 2 sắc vàng — đây là nợ, không phải thiết kế

| Đỏ | Dùng cho | | Vàng | Dùng cho |
|----|----------|---|------|----------|
| `#C8102E` | Cổ động viên (phe địch) | | `#F2C14E` | Peso / tiền |
| `#E03131` | Máu cầu môn nguy kịch | | `#FFD32C` | Thẻ vàng |
| `#E4003A` | Thẻ đỏ | | | |

Ba sắc đỏ này **gần nhau tới mức người mù màu đọc như một**, và người mắt thường ở 64px cũng khó tách. Tương tự hai sắc vàng.

**Cách giải bắt buộc — không phải chỉnh hex, mà là tách trục:**

| Yếu tố | Trục phân biệt (không dùng màu) |
|--------|--------------------------------|
| Thẻ vàng/đỏ | **Hình chữ nhật bo góc, bay lên, xoay** — không thứ gì khác trong game có hình đó |
| Tiền | **Số bay lên kèm ký hiệu ⚽** |
| Máu cầu môn nguy kịch | **Nhấp nháy** — chuyển động, không phải màu |
| Cổ động viên | **Silhouette** (kèn / trống / tay không) |

💡 *Đây là hệ quả trực tiếp của việc thêm tướng thứ 6 — không ai dự đoán được lúc chốt "thêm trọng tài". Cách sai là đi tinh chỉnh hex cho ba sắc đỏ xa nhau ra; ở 64px trên nền cỏ thì vẫn hỏng. Cách đúng là chấp nhận **màu đã hết chỗ** và chuyển sang trục hình dạng + chuyển động.*

💡 *Nếu chỉ dùng màu để phân biệt phe trên nền cỏ, một phần đáng kể người chơi sẽ không phân biệt được gì. Silhouette làm việc đó tốt hơn màu — và nó cũng đọc được ở 64px, thứ mà màu không làm được.*

---

## 3. Layout HUD (dọc, 1080×1920)

```
┌─────────────────────────────┐
│ ⏸        Wave 7/20      ⏩2× │  ← thanh trên, 120px
├─────────────────────────────┤
│                             │
│      [ CẦU MÔN ▓▓▓▓░ 16/20 ]│  ← máu cầu môn ngay trên xà ngang
│                             │
│                             │
│         S Â N                │  ← vùng chơi, ~1400px
│      (đường chữ S)          │     an toàn cho ngón cái:
│       ○   ●   ○             │     mọi ô đặt nằm trong
│         ●   ○               │     nửa dưới màn hình
│       ○     ●               │
│                             │
│      ▲ ▲ ▲  (spawn)         │
├─────────────────────────────┤
│ ⚽ 1 042        [Sẵn sàng 5s]│  ← thanh dưới, 160px
└─────────────────────────────┘
```

| Quy tắc | Lý do |
|---------|-------|
| Ví tiền góc **dưới trái** | Ngón cái phải che góc dưới phải; tiền là thứ phải đọc liên tục |
| Nút "Sẵn sàng" góc **dưới phải** | Nút bấm nhiều nhất → chỗ ngón cái tự nhiên nhất |
| Máu cầu môn **trên xà ngang**, không ở thanh trên | Đọc được ngay tại chỗ nó có ý nghĩa; người chơi đang nhìn xuống cầu môn khi lo lắng |
| Vùng chạm tối thiểu **48×48pt** | HIG của Apple |
| ~~Mọi ô đặt nằm ở **nửa dưới** màn hình~~ | 🔴 **LUẬT NÀY ĐÃ CHẾT — xem dưới** |

#### 🔴 "Mọi ô ở nửa dưới" bất khả thi — phát hiện vòng 5

Toạ độ thật (`config/path.json`) cho thấy luật này mâu thuẫn với chính thiết kế đường: 11 ô rải theo **quãng đường** từ f=0.04 đến f=0.91, nên chúng trải y từ **+8.57 xuống −6.12** — 6/11 ô nằm ở nửa TRÊN.

Không phải lỗi đặt ô. Ép mọi ô xuống nửa dưới thì chỉ nửa cuối đường được phòng thủ, nửa đầu đi tự do → wave 1 đã thủng. Đường đi qua cả màn hình thì ô phải theo nó.

**Luật đúng phải là:** mọi ô **chạm được** (≥48×48pt, không bị HUD che), không phải mọi ô nằm trong tầm ngón cái ở một tư thế cầm. Đây là game **đặt trước / xem sau**, không phải game bấm liên tục — thao tác đặt xảy ra lúc nghỉ giữa wave, người chơi có thời gian đổi tay hoặc dùng ngón trỏ. Nút bấm gấp duy nhất là "Sẵn sàng", và nó vẫn ở góc dưới phải.

⚠️ Đánh đổi còn treo: ô f01 ở y=+8.57 nằm sát mép trên — cần kiểm tra không bị notch/Dynamic Island che ở M1.

### Radial buy menu

Tap ô trống → 5 tướng xoè thành hình bán nguyệt **hướng lên trên** (không bị ngón tay che):

```
          La Pulga
     D10S              El Árbitro
  Batigol
          ⊙ (ô được tap)
```

- **5 đơn vị**, nhưng ô sân chỉ hiện **4** — Dibu không đặt ra sân được nên **không hiện ở đây**
- Ô thủ môn → chỉ hiện Dibu (1 nút duy nhất)
- Mỗi nút hiện icon + giá
- Không đủ tiền → xám 40% + không bấm được (không rung, không popup — im lặng là đủ)
- Tap ra ngoài → đóng

💡 *Radial menu chỉ hiện thứ **đặt được ở ô này**, không hiện toàn bộ 6 đơn vị rồi xám 1 cái. Xám nghĩa là "thiếu tiền"; nếu Dibu cũng xám vì "sai ô" thì hai lý do khác hẳn nhau lại trông giống nhau. Menu đúng vẫn là 5 nút — thêm tướng thứ 6 không làm nó chật hơn.*

---

## 4. Danh sách asset (MVP)

| Nhóm | Số lượng | Ghi chú |
|------|----------|---------|
| Tướng | **6** × 3 cấp = **18** sprite | Cấp thể hiện bằng phụ kiện (băng đội trưởng, áo khoác, hào quang), không đổi tư thế |
| Anim tướng | 6 × 2 (idle, attack/rút thẻ) = **12** | Mỗi anim 4 frame **@ 24fps = 0.17s** — xem §4b |
| **Icon kỹ năng** | **18** | Mỗi cấp mỗi đơn vị 1 kỹ năng (`02` §6). Hiện ở panel thông tin tướng. |
| **VFX thẻ** | **3** | Thẻ vàng bay lên, thẻ đỏ bay lên, dấu thẻ nhỏ bám trên đầu quân đã bị phạt |
| Cổ động viên | **3** sprite + 3 anim đi + 3 anim ngã | Chibi, silhouette phân biệt rõ |
| **`O Capitão`** (boss) | **1** sprite lớn + anim đi + anim ngã **1.2s** | To gấp ~1.8× quân thường. Silhouette phải đọc được ngay từ lúc spawn. Xem guardrail §5. |
| Đạn | **3** | Bóng thường, bóng sút mạnh (Batigol), sóng AoE (D10S) |
| VFX | **6** | Trúng đòn, ngã, slow (vòng xoáy), Solo Run (hào quang), Cản Phá (chớp sáng), tiền bay lên |
| Sân | **1** | Nền + đường + 12 ô + cầu môn |
| UI | ~**20** | Icon, khung, nút, thanh máu, sao |
| Font | **1** | Bo tròn, dễ đọc, hỗ trợ tiếng Việt có dấu ⚠️ |
| SFX | ~**12** | Bắn ×4, trúng, ngã, lọt lưới, cản phá, mua, nâng cấp, clear wave, thua |
| Nhạc | **1** track | Loop, tông sân vận động |

⚠️ **Font phải có dấu tiếng Việt đầy đủ.** Rất nhiều font game miễn phí thiếu `ế ộ ữ ẳ` — kiểm tra trước khi chốt, không phải sau khi vẽ xong UI.

---

## 4b. Hợp đồng kỹ thuật của asset — thứ code cần, art không cần biết

> Code **không cần art**. Code cần **hợp đồng**: một hình chữ nhật xám đúng kích thước là đủ để build cả game. Mục này là hợp đồng đó. Thiếu nó, art vẽ xong mới phát hiện sai — lúc đó vẽ lại.

### Thang vật lý — nguồn: `config/path.json → ui`

| | Giá trị | Suy ra |
|---|---|---|
| Thiết kế | 1080 × 1920 px | 10.8 × 19.2 units (PPU 100) |
| Bề ngang trên iPhone | ~393 pt | **1 unit = 100px = 36.4 pt** |
| Vùng chạm tối thiểu (HIG) | 48 pt | = **1.32 units** |
| Bán kính ngoài radial menu | 87 pt | = **2.39 units** — 5 nút 48pt xoè bán nguyệt (bán kính vòng ~63pt + nửa nút) |

**Đây là chỗ hình học biến thành cảm giác cầm máy.** Một layout đúng hoàn hảo trong "unit" vẫn có thể không bấm được. Vòng 5 kiểm thử điều này và **bắt được lỗi thật**: ô `f01` ở y=8.57 khiến radial menu xoè lên tới y=10.96 — **vượt mép trên 9.6, nút bị cắt khỏi màn**. Đã dời f01 xuống y=7.00.

`tools/path_check.py --check` **thi hành** mục này: exit 1 nếu hai ô gần nhau hơn vùng chạm, hoặc menu của ô nào tràn mép trên. Đừng chép các con số này đi chỗ khác — sửa `config/path.json → ui`.

### Kích thước sprite

| Đối tượng | Cao (units) | = px @ PPU 100 | Ràng buộc |
|---|---|---|---|
| Cổ động viên | **0.7** | 70 px | Phải NHỎ hơn tầm nhỏ nhất (Batigol 1.1) — nếu quân to bằng vòng tầm thì "trong tầm/ngoài tầm" không đọc được |
| `O Capitão` | **1.26** | 126 px | ×1.8 quân thường (§4) |
| Tướng | **0.9** | 90 px | Đứng trong ô, không tràn sang ô cạnh (ô gần nhau nhất cách 2.21 units) |
| Vòng tầm khi tap | `2 × range` | Batigol Lv1 = 200px · La Pulga Lv3 = 360px | Vẽ bằng đường tròn, không phải sprite co giãn |

⚠️ **Thu tầm vòng 5 làm mục này bắt buộc.** Tầm cũ 3.0–7.0 thì vòng tầm to hơn màn, quân to nhỏ không quan trọng. Tầm mới 1.1–2.6 → vòng tầm Batigol chỉ 220px = 20% bề ngang màn, và một con quân 70px nằm gọn trong đó. Tỉ lệ quân/tầm giờ **đọc được**, nên nó phải **đúng**.

### Bố cục sprite sheet cổ động viên — KHÁC sheet tướng

⚠️ **Đừng nhầm hai khuôn.** Tướng có 3 cấp nên hàng = cấp. Cổ động viên **không có cấp** (`config/enemies.json` — máu lên theo công thức trơn `0.67 × 1.09^(wave−1)`, tạo hình giữ nguyên từ W1 đến W20), nên nó chỉ cần **đi** và **ngã**.

| | Sheet tướng | Sheet cổ động viên |
|---|---|---|
| Canvas | 1152 × 928 | **1408 × 768** |
| Lưới | 5 × 4 = 20 frame | **4 × 2 = 8 frame** |
| Ô | 230.4 × 232 | **352 × 384** |
| PPU nhập | 232 | **384** |
| Hướng mặt | sang **PHẢI** | sang **PHẢI** (giống sheet tướng) |
| Hàng 1 | idle | **chu kỳ đi** (frame 0–3, @6fps) |
| Hàng 2 | đòn Lv1 | ngã / dự phòng (frame 4–7) |

**Chỉ trộn frame trong CÙNG một hàng.** Đo trên `adepto_enemy.png`: bbox dọc của f0–f3 giống hệt nhau (y 48→374) nên lặp không nhấp nhô; hàng 2 lệch lên 26px — ghép hai hàng vào một clip là quân giật lên xuống.

**Cỡ trên màn** (`MatchView`): `EnemyArtScale = 0.88` → quân cao **0.75 unit**; boss ×1.8 → 1.35 unit. Chọn 0.75 vì phải **nhỏ hơn tầm ngắn nhất** (El Cinco Lv1 = 1.0) để "trong tầm / ngoài tầm" còn đọc được, và **nhỏ hơn bề rộng đường** (0.95) để quân nhìn như đang đi trên đường. `EnemyFootLift = 0.42` đưa bàn chân chạm đúng điểm trên đường.

**Lật mặt** lấy từ **tiếp tuyến của đường**, không phải hiệu vị trí hai khung: khúc đường gần thẳng đứng làm hiệu vị trí dao động quanh 0 và quân rung lật liên tục. Có thêm vùng chết 0.02. Sheet vẽ mặt sang phải ⇒ `flipX` khi đi sang **trái**.

**Nhịp chân tỉ lệ với tốc độ.** Lấy adepto 6fps @ `speed 1.0` làm gốc, mọi con khác nhân theo `speed` của nó trong `config/enemies.json` — giữ như vậy thì **sải chân gần như không đổi** giữa các con, con nhanh khua chân nhanh hơn chứ không trượt băng.

| Quái | `speed` | Lưới sheet | Ô | PPU | fps clip |
|---|---|---|---|---|---|
| Adepto | 1.0 | 4×2 | 352×384 | 384 | **6** |
| Tifoso Kèn | 1.4 | 4×2 | 352×384 | 384 | **8** |
| Tambor Giáp | 0.6 | 4×2 | 352×384 | 384 | **4** |
| O Capitão (boss) | 0.4 | **4×1** | **352×768** | **768** | **3** ⚠️ |

⚠️ **Boss là ngoại lệ có tên của luật fps.** Đúng luật thì 0.4 × 6 = 2.4fps, nhưng dưới ~3fps thì 4 frame nhìn ra trình chiếu slide chứ không ra bước đi. Boss là set-piece (`docs/03 §2`) — đọc được quan trọng hơn sải chân đúng vật lý, nên ép sàn 3fps.

**Sheet boss cao gấp đôi** (ô 352×768, lưới 4×1) nên PPU của nó cũng gấp đôi — quy tắc chung là **PPU = chiều cao ô**, nhờ vậy mọi frame đều cao đúng 1 unit bất kể ô to nhỏ. Nó vẫn có kẻ ngang của lưới 4×2 vắt ngang ngực; xoá bằng `--hline 384`.

**Vị trí chân và thanh máu suy từ SỐ ĐO THẬT của từng sheet**, không phải hằng số chung. Đơn vị = chiều cao ô:

| | chân so với tâm ô | đỉnh đầu so với tâm ô | cao trên màn | nâng chân | thanh máu |
|---|---|---|---|---|---|
| Quái thường (ô 384) | 0.474 | 0.43 | **0.80** u | 0.42 u | 0.90 u |
| Boss (ô 768) | 0.441 | 0.361 | **1.27** u | 0.70 u | 1.37 u |

Hai sheet lệch nhau vì nhân vật chiếm tỉ lệ khác nhau trong ô của nó — dùng chung một con số thì **boss lơ lửng trên mặt đường 0.06 unit**. Boss trên màn to gấp **1.59×** quân thường (không phải đúng 1.8 như §4 ước, vì boss chiếm ít phần ô hơn).

⚠️ Thanh máu từng để cứng ở 0.62 — di sản thời quân còn là hình khối cao 0.42. Với art thật cao 0.80 thì nó **chui vào trong người**. Con nào chưa có art thì vẫn dùng nửa chiều cao hình khối.

**Bỏ nền checker: LUÔN dùng `--checker-auto`, đừng dò tay.**

```bash
python tools/declak.py images/monster_lv1.png \
       LaMuralla/Assets/_Project/Art/Characters/adepto_enemy.png \
       --cols 4 --rows 2 --checker-auto --min-blob 500
```

`--checker-auto` lấy **hai đỉnh lớn nhất** trong histogram độ sáng của px bão hoà thấp — nền checker chiếm 50–70% ảnh nên hai đỉnh đó luôn vượt xa mọi thứ khác — rồi tự đặt luôn `--protect-dark` xuống dưới dải nền.

⚠️ **Vì sao bắt buộc:** tông checker **đổi giữa các lần xuất file**, và lệch 5 đơn vị là đủ làm hỏng kết quả. Đo trên 4 sheet quái, đỉnh lần lượt là `122/172 · 118/168 · 112/162 · 108/158`. Có lần dò tay `--checker 105 185` cho sheet boss xuất lại: dải nền thật bắt đầu ở 100 nên 5 đơn vị lọt ra ngoài → **83 742px bị `--min-blob` gom mất**, bbox nhân vật vọt từ `107..723` lên `9..749`. Dùng auto: còn 334px vụn.

Sheet boss còn có kẻ ngang lẻ ở `y=384` (lưới vẽ 4×2 nhưng nhân vật cao cả 768) → thêm `--hline 384`. Nó chỉ dọn trong **cột sát mép nền** (nở 14 cột vào trong), nên mưu kẻ thò ra sát thân thì bị bắt còn phù hiệu giữa ngực thì không với tới. *Đã thử lọc theo độ sáng thay vì cách này: cửa sổ lum ăn mất phần trắng của phù hiệu — đừng quay lại cách đó.*

Sheet nhiều chi tiết rời (bụi, khói) hay sót vụn checker — thêm `--min-blob 500` để bỏ mọi mảnh liên thông nhỏ hơn 500px. **Ngưỡng phải nằm dưới chi tiết thật nhỏ nhất**: `monster_lv2` có 8 nhân vật ~52 000px, 7 cụm bụi 1 064–1 732px, rồi tụt thẳng xuống ≤134px — chọn 500 là cắt đúng khe đó (bỏ 854px vụn, giữ nguyên bụi).

### Nền, đường, bệ, âm thanh — nguồn và lý do (2026-08-20)

| Phần | Nguồn | Vì sao |
|---|---|---|
| Mặt sân, con đường, bệ đứng | **Sinh bằng code** — `Assets/_Project/Unity/Pitch.cs` | xem bên dưới |
| Âm thanh | **Kenney, CC0** — `Resources/Audio/`, bảng map ở `CREDITS.txt` cạnh đó | public domain, dùng thương mại được, không bắt buộc ghi công |

**Đã thử và LOẠI sprite pack tower-defense tải sẵn.** Lý do đo được, không phải cảm tính:

1. Đường đi là **spline Catmull-Rom tự do** qua 14 waypoint. Góc rẽ thật lấy từ `config/path.json`: `70° · 77° · 9.6° · -48° · -86° · -26° · 37° · 98° · 26° · -42° · -99° · 8.5°`. Mọi pack tile TD (Kenney và tương đương) đều là ô **vuông trên lưới 90°** — không khớp một khúc nào.
2. Chỉnh waypoint thì hình tự vẽ lại. Ảnh cắt sẵn thì mỗi lần chỉnh phải cắt lại.
3. §5 ở dưới: asset ngoài luôn phải soi bản quyền. Sinh bằng code thì rủi ro đó bằng 0.

**Ràng buộc hình học của con đường — đừng gỡ.** Bề rộng `PathLineWidth = 0.95` unit, nửa bề rộng 0.475. Bán kính cong nhỏ nhất của spline hiện tại là **0.383 unit** (quanh waypoint `(4.0, -6.55)`, khúc rẽ -99°) — **nhỏ hơn** nửa bề rộng, nên dải lưới sẽ tự cắt thành nút hình nơ nếu offset đều. `Pitch.Road` kẹp bề rộng theo bán kính cong **nhỏ nhất trong cửa sổ ±3 mẫu**; kẹp tại chỗ thôi thì KHÔNG đủ (đỉnh cong chỉ rơi vào một mẫu, hai mẫu kề vẫn phình ra và vẫn gấp). Sau khi kẹp: 0 đoạn gấp ngược, 7/260 mẫu bị bóp, chỗ hẹp nhất còn 0.76 unit — vẫn rộng hơn cổ động viên (0.7).

⚠️ **Ai nới `PathLineWidth` hoặc kéo waypoint gần nhau hơn thì phải đo lại bán kính cong.** Rộng hơn 2 × 0.383 = 0.766 mà không có bước kẹp là đường thắt nút.

**Thứ tự vẽ** (`MatchView`): sân `-20` → đường `-10` → bệ `-5` → vòng chạm/vòng ô `1,2` → quân `6` → thanh máu `7` → đạn `8` → tướng `12`.

**Âm thanh** móc vào 8 event có sẵn của `MatchController` + 3 thao tác HUD; core không phải sửa gì. Có nút tắt tiếng cạnh nút ×2, nhớ qua `PlayerPrefs`. `AudioService` giữ pool 8 nguồn, chặn phát lại cùng một tiếng dày hơn 0.05s, và lệch cao độ ±6% cho đỡ máy móc.

### Bố cục sprite sheet hero — hợp đồng 4 hàng

> **Hàng = CẤP.** Đây là hợp đồng ràng buộc art với `SpriteAnimBaker`. Vẽ đúng khuôn này thì đổi art là **thay đúng 1 file PNG**, không sửa một dòng code nào (đã chứng minh ở vòng D10S 2026-07-31).

| | Giá trị |
|---|---|
| Canvas | **1152 × 928 px** |
| Lưới | **5 cột × 4 hàng = 20 frame**, ô 230.4 × 232 |
| Đánh số | trái→phải, trên→dưới: `0..4` hàng 1, `5..9` hàng 2, `10..14` hàng 3, `15..19` hàng 4 |
| Nền | **alpha = 0 thật sự.** Không checker giả, không đường kẻ lưới. Nếu file gốc có → chạy `python tools/declak.py <src> <dst>` |
| Hướng mặt | **sang PHẢI.** Code tự `flipX` khi mục tiêu ở bên trái |
| PPU | 232 (mỗi ô cao đúng 1 unit), `MatchView` nhân `SpriteArtScale = 1.8` |

| Hàng | Frame | Nội dung |
|---|---|---|
| 1 | 0–4 | Đứng thủ. **Frame 0 = clip `idle`** (đứng yên, 1 frame) |
| 2 | 5–8 | **Đòn của Cấp 1** (frame 9 dự phòng, không dùng) |
| 3 | 10–13 | **Đòn của Cấp 2** |
| 4 | 15–18 | **Đòn của Cấp 3** |

Mỗi đòn = 4 frame, 12fps → 0.33s. Ràng buộc độ dài xem mục "Nhịp animation" ngay dưới.

**Đòn của cấp nào phải NHÌN RA được cấp đó.** Ba hàng đòn giống nhau = nâng cấp không có phản hồi hình ảnh, người chơi không biết mình vừa tiêu 240 vàng để làm gì.

| Tướng | Hàng 2 = Lv1 | Hàng 3 = Lv2 | Hàng 4 = Lv3 |
|---|---|---|---|
| `batigol` (El Cinco) | **Đấm** — nắm đấm phóng thẳng | **Đá** — vung chân, mũi giày tới trước | **Đạp** — đạp thẳng gót/lòng bàn chân |
| `la_pulga` | **Xút thường** | **Xút xoáy** — má ngoài, thân nghiêng | **Vô-lê** — bật người, chân ngang hông |
| `d10s` | Ném bóng | Ném chai | Bắn cối |
| `dibu` | Ném bóng | Ném găng | Nâng cúp |
| `el_arbitro` | (đứng yên) | Thẻ vàng | Thẻ đỏ |

⚠️ **Nợ art đang mở (2026-08-20).** Sheet `batigol_hero.png` và `la_pulga_hero.png` hiện là sheet **chạy** chung: hàng 3 và hàng 4 là cùng một chu kỳ chạy lệch nửa pha, không phải hai đòn khác nhau. Hệ quả: El Cinco Lv2/Lv3 "chạy tại chỗ" thay vì đá/đạp và trông giống hệt nhau. Code, baker và controller đã đúng — chỉ chờ art vẽ lại theo bảng trên. `d10s_hero.png` là mẫu đúng để đối chiếu.

**Quy trình thay art:** thả PNG mới vào `images/` → `python tools/declak.py images/<x>.png LaMuralla/Assets/_Project/Art/Characters/<id>_hero.png` → trong Unity chạy menu **La Muralla → Bake All Hero Anim**.

### Nhịp animation — ràng buộc từ `attackRate`, không phải từ art

`05` §4.4 chốt `animator.speed = MatchTimeScale`, nên ở chế độ **×2** mọi anim chạy gấp đôi. Ràng buộc:

> **Anim `attack` phải NGẮN HƠN khoảng cách giữa hai đòn, ở tốc độ ×2.**

| Tướng | `attackRate` | Giãn cách | Ở ×2 | Anim 4f @24fps = 0.17s |
|---|---|---|---|---|
| Batigol | 0.4 | 2.50s | 1.25s | ✅ |
| D10S | 0.8 | 1.25s | 0.62s | ✅ |
| La Pulga | 1.2 | 0.83s | 0.42s | ✅ |

🟡 **Ràng buộc chặt nhất đã biến mất cùng El Fideo (gỡ 2026-08-20).** Nó bắn 2.0/s
→ giãn cách ở ×2 chỉ 0.25s, sát nút anim 4f@24fps = 0.17s (biên 0.08s), và **đó là
lý do duy nhất chốt 24fps làm sàn**. Người nhanh nhất còn lại là La Pulga 1.2/s →
0.42s ở ×2, biên 0.25s — kể cả 12fps (0.33s) cũng vừa. **24fps giờ là lựa chọn
thẩm mỹ, không còn là ràng buộc kỹ thuật.** Quyết định giữ hay hạ đang bỏ ngỏ.

**Luật cho code:** nếu `1/attackRate/2 < anim_length` → **cắt anim**, không hoãn phát bắn. Logic bắn là chân lý; hình phải theo. Ngược lại = tướng bắn chậm hơn chỉ số → mọi số ở `04` sai.

⚠️ **Ai tăng `attackRate` của tướng nào lên > 2.9 thì phải xem lại mục này** (0.17s × 2 = giãn cách tối thiểu 0.34s → rate tối đa 2.9).

---

## 5. Guardrail bản quyền (BẮT BUỘC)

Đây là ràng buộc pháp lý, không phải gợi ý thẩm mỹ.

| ❌ Không bao giờ | ✅ Thay bằng |
|-----------------|-------------|
| Mặt cầu thủ thật, ảnh chụp, tranh vẽ giống mặt | Chibi cách điệu — nhận ra qua **kiểu tóc + dáng người + phụ kiện**, không qua khuôn mặt |
| Áo đấu chính thức (logo AFA, nhà tài trợ, hoạ tiết chính xác) | Áo sọc xanh-trắng **chung chung**, không logo, không tên nhà tài trợ |
| Số áo thật (10, 11, 9, 23) | Không hiện số áo. Hoặc dùng ký hiệu (⚡, ★) |
| Tên thật ở bất kỳ đâu | Biệt danh — xem [`02-TOWERS.md`](02-TOWERS.md) §1 |
| Logo FIFA / World Cup / tên giải thật | "World Cup" chỉ dùng nội bộ. Store listing: **"giải đấu bóng đá"** chung chung |
| **"CR7"** — ở bất kỳ đâu, kể cả tên file, tên biến, comment | **Nhãn hiệu đã đăng ký.** Ronaldo kinh doanh thật dưới thương hiệu này (quần áo, khách sạn, nước hoa) — bảo hộ mạnh hơn cả tên riêng. Dùng **`O Capitão`**. |
| **Số áo 7** trên boss | Cùng luật "không số áo thật" ở trên. Boss không mang số. |
| Cờ quốc gia dùng làm định danh đội | Màu sắc là đủ. Không cắm cờ. |
| Giọng nói / chữ ký / dáng ăn mừng đặc trưng | Ăn mừng chung chung |

**Kiểm tra trước mọi lần đưa asset vào repo:** *"Nếu luật sư của cầu thủ này thấy sprite này, họ có gửi thư được không?"* — nếu do dự, sửa.

### `El Árbitro` — trò đùa "trọng tài bị mua"

**Chốt 2026-07-16 (user).** Trọng tài **công khai thiên vị** Argentina. Trò đùa nằm ở chỗ ông ấy giả vờ công tâm mà không giả vờ nổi.

| Chi tiết tạo hình | Ý đồ |
|-------------------|------|
| Áo đen kẻ sọc — **đúng chuẩn trọng tài thật** | Tạo kỳ vọng "người thứ ba công tâm" |
| **Tờ Peso thò ra túi quần sau** | Phá kỳ vọng đó. Toàn bộ trò đùa nằm ở một chi tiết 6px này. |
| Nháy mắt về phía cầu môn Argentina khi rút thẻ | Xác nhận, không cần chữ |
| Rút thẻ với vẻ mặt **hoàn toàn nghiêm túc** | Càng nghiêm túc càng buồn cười |
| **Không** logo giải đấu trên áo | `06` §5 |

**Bản quyền:** trọng tài là **nhân vật hư cấu hoàn toàn**, không nguyên mẫu, không có bảng mapping ở `02` §1. Không rủi ro IP — đây là đơn vị duy nhất trong game không cần guardrail.

#### ⚠️ Đánh đổi đã được cân nhắc và chấp nhận

Tôi có nêu rằng framing này hàm ý **"Argentina thắng nhờ trọng tài thiên vị"** — một lời chê Argentina, trong một game vốn tôn vinh Argentina. Phương án thay thế (trọng tài trung lập, đang thực thi luật vì **fan tràn sân là phạm luật thật**) tránh được điều đó.

**User chọn framing thiên vị.** Quyết định đã chốt, ghi lại ở đây để không ai mở lại nhầm — không phải để tranh luận tiếp.

Nếu về sau beta cho thấy người chơi Argentina/Nam Mỹ phản ứng tiêu cực, phương án "thực thi luật" đổi được mà **không tốn một dòng code nào** — chỉ đổi art (bỏ tờ Peso, bỏ cái nháy mắt) và flavor text. Cơ chế giữ nguyên 100%.

### `O Capitão` — nhận diện đến từ ngữ cảnh, không từ ngoại hình

Boss phải khiến người chơi **tự nghĩ ra** đó là ai, mà không có chi tiết nào chỉ thẳng vào một người thật:

| ✅ Được | ❌ Không |
|---------|---------|
| Băng đội trưởng, dáng đứng vênh váo, tóc vuốt keo | Mặt thật, đường nét khuôn mặt nhận ra được |
| Áo đỏ-lục **không logo**, không số | Áo tuyển Bồ chính thức, số 7 |
| To hơn quân thường 1.8×, hào quang riêng | Dáng ăn mừng đặc trưng (nhảy + xoay + "Siu") |
| Tên hiển thị **`O Capitão`** | "CR7", "Cristiano", "Ronaldo", "R7", "Bicho" |

💡 *Nhận diện đến từ **vị trí trong game** — thủ lĩnh phe Bồ, chỉ xuất hiện ở wave trùm, to gấp đôi mọi thứ khác. Không cần một pixel nào giống người thật. Đó vừa là cách an toàn nhất, vừa thường là cách vui hơn.*

**Store listing cũng chịu ràng buộc này.** Không được đưa tên thật cầu thủ vào mô tả app kiểu "Play as {tên thật}!". Đó là chỗ dễ quên nhất và cũng là chỗ dễ bị soi nhất.

### Lint CI — phạm vi chính xác

Ràng buộc này grep được, **nhưng chỉ trên bề mặt sản phẩm**:

```bash
# QUÉT: mọi thứ được ship
grep -rniE "{danh sách cấm}" Assets/ ProjectSettings/ store-listing/
# → phải trả về RỖNG. Khác rỗng = fail build.

# KHÔNG quét: docs/ và .dev-agent/
```

| Phạm vi | Quét? | Vì sao |
|---------|-------|--------|
| `Assets/`, `ProjectSettings/` | ✅ | Được ship. Tên thật ở đây là rủi ro thật. |
| Store listing | ✅ | Chỗ dễ bị soi nhất |
| `docs/` | ❌ | **Docs phải gọi tên thứ nó cấm.** Bảng mapping `02` §1 và các bảng guardrail (§5 này, `03` §2) buộc phải viết ra "CR7", "Ronaldo" — nếu không thì không ai biết đang cấm cái gì. |
| `.dev-agent/` | ❌ | Ghi chú nội bộ, không ship |

💡 *Bản đầu của doc này định nghĩa lint là "grep toàn repo, trừ 02-TOWERS.md". Sai — vì rồi chính doc này và `03` §2 phải viết "CR7" ra để cấm nó, và lint tự đỏ vào mặt mình. Ranh giới đúng không phải **file nào**, mà là **cái gì được ship**. Một luật mà chính tài liệu định nghĩa nó cũng vi phạm được thì là luật viết sai, không phải tài liệu viết sai.*

---

## 6. Phản hồi & cảm giác (game feel)

| Sự kiện | Phản hồi |
|---------|----------|
| Tướng bắn | Giật nhẹ 2px ngược hướng bắn + SFX |
| Trúng đòn | Sprite địch nháy trắng 0.05s |
| Hạ cổ động viên | Ngã lăn ra khỏi sân 0.4s + số tiền vàng bay lên ví |
| **Lọt lưới** | Rung màn hình 0.2s + lưới cầu môn rung + SFX trầm + máu cầu môn nháy đỏ |
| Cản Phá (Dibu) | Chớp sáng trắng + SFX "bốp" + cổ động viên văng ngược |
| Mua tướng | Tướng "rơi" từ trên xuống, nảy 1 nhịp |
| Nâng cấp | Hào quang toả ra 1 nhịp + vòng tầm mới hiện 1s |
| Không đủ tiền | Số tiền ở ví nháy cam 1 nhịp. **Không** popup, **không** âm thanh lỗi |
| Clear wave | Banner trượt ngang + số tiền thưởng bay lên |
| **`O Capitão` spawn** | Banner đỏ trượt ngang + nhạc đổi 1 nhịp + camera **không** đổi (vẫn tĩnh) |
| **Hạ `O Capitão`** | Anim ngã **1.2s** (dài gấp 3 quân thường) + rung màn hình + tiền vàng bay thành chùm |
| **`O Capitão` lọt lưới** | Rung màn hình **0.5s** (mạnh nhất game) + máu cầu môn tụt 5 nấc một |
| **Rút thẻ vàng** | Thẻ bay lên xoay 1 vòng + tiếng còi + dấu thẻ nhỏ **bám lên đầu** con bị phạt |
| **Rút thẻ đỏ** | Như trên nhưng chậm hơn 1 nhịp + con bị phạt **đổi tông xám nhẹ** (đánh dấu vĩnh viễn) |
| Trọng tài rút thẻ | Nháy mắt về phía cầu môn Argentina (`06` §5) |
| Thua | Sân xám dần trong 1s, tất cả dừng, kết quả trượt lên |

💡 *Lọt lưới là sự kiện duy nhất được rung màn hình — và boss lọt rung mạnh nhất. Nếu mọi thứ đều rung, không có gì đáng kể cả; mà lọt lưới đúng là thứ duy nhất người chơi phải cảm thấy tệ khi nó xảy ra.*

⚠️ **Boss spawn KHÔNG được zoom camera, KHÔNG cutscene, KHÔNG khoá input.** Người chơi đang bận đặt tướng cho chính con boss đó. Cướp quyền điều khiển đúng khoảnh khắc căng nhất là cách chắc chắn để biến một cao trào thành một phiền toái.
