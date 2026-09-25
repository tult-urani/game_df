# M06 — El Caracol (Con Ốc)

> **1 tuyến xoáy ốc · 9 ô sân + 1 ô thủ môn · boss kháng chậm 82% · ★★★★**
> Trạng thái: **THIẾT KẾ** — toạ độ đã kiểm bằng bản sao `tools/path_check.py`.
> Số kinh tế ở §6 là **ĐÍCH CẦN ĐO** bằng engine thật.

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m06 -->
**Nguồn: `config/maps/m06-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 1 — `L1` 64.6u |
| Ô sân / thủ môn | 9 / 1 |
| Σchord@1.4 mỗi ô | 2.63 |
| Tiền khởi đầu | 700 |
| Máu quái | ×0.67 × 1.12^(W−1) → W20 ×5.77 |
| Tổng quân 20 wave | 246 |

| W | ×máu | `L1` | boss | quân | tổng máu |
|---|------|------|------|------|----------|
| 1 | ×0.67 | 4/0/0 | — | 4 | 268 |
| 2 | ×0.75 | 7/0/0 | — | 7 | 525 |
| 3 | ×0.84 | 5/2/0 | — | 7 | 790 |
| 4 | ×0.94 | 5/4/0 | — | 9 | 1 298 |
| 5 | ×1.05 | 4/3/1 | — | 8 | 1 696 |
| 6 | ×1.18 | 5/4/1 | — | 10 | 2 279 |
| 7 | ×1.32 | 4/4/2 | — | 10 | 3 146 |
| 8 | ×1.48 | 4/3/1 | — | 8 | 2 385 |
| 9 | ×1.66 | 5/4/1 | — | 10 | 3 202 |
| **10** | ×1.86 | 4/4/2 | `o_capitao`@`L1` | 11 | 6 924 |
| 11 | ×2.08 | 5/5/2 | — | 12 | 5 620 |
| 12 | ×2.33 | 5/5/2 | — | 12 | 6 294 |
| 13 | ×2.61 | 5/7/2 | — | 14 | 8 195 |
| 14 | ×2.92 | 7/7/3 | — | 17 | 11 369 |
| 15 | ×3.27 | 5/5/2 | — | 12 | 8 837 |
| 16 | ×3.67 | 5/7/2 | — | 14 | 11 518 |
| 17 | ×4.11 | 7/7/3 | — | 17 | 15 982 |
| 18 | ×4.60 | 7/8/3 | — | 18 | 18 906 |
| 19 | ×5.15 | 8/9/4 | — | 21 | 25 662 |
| **20** | ×5.77 | 10/11/5 | `o_capitao`@`L1` | 27 | 45 410 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m06 -->

---

## 1. Ý đồ thiết kế

Một đường duy nhất, cuộn vào trong 1.5 vòng. Vòng ngoài và vòng trong cách nhau **2.2–2.5 u** — vừa đủ để một ô đặt ở giữa **với tới cả hai lần đường đi qua**.

Đây là map **ít ô nhất game (9)** nhưng **đường dài nhất game (52.18 u)** — dài hơn map gốc 23 %. Hai con số đó không mâu thuẫn: chúng là hai mặt của cùng một quyết định. Ít ô nghĩa là mỗi ô phải làm việc nhiều hơn; đường dài đi qua chính nó nghĩa là **có ô làm được gấp đôi**.

Con số nói hết: `f01` (d = 0.92, nằm giữa hai vòng ở đỉnh) có chord **4.03 u**; `f02` (d = 0.60, dí sát đường nhưng chỉ chạm một vòng) chỉ **2.56 u**. Ô xa hơn mà phủ nhiều hơn 57 %. Trực giác "đặt càng gần đường càng tốt" — đúng ở M01–M05 — **sai ở đây**.

Và có một tầng thứ hai: ở tầm Lv1, `f01` chỉ phủ **1.25 u** (Batigol tầm 1.0 chỉ với tới một vòng). Nâng Batigol lên Lv3 (tầm 1.4) → **4.03 u**, gấp **3.2 lần**. Trên map này **nâng cấp tại chỗ đánh bại mua thêm ô** — và đó là bài học duy nhất map này dạy mà năm map kia không dạy được.

> Một ô tốt hơn ba ô tồi.

---

## 2. Toạ độ waypoint

Nội suy `catmull-rom`. Xoáy ốc elip tâm `(0.0, −0.20)`, bán trục co tuyến tính 2.2 u (ngang) và 2.3 u (dọc) mỗi vòng, chạy 1.5 vòng.

### Tuyến `caracol` — 19 waypoint · **52.18 u**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 1 | 0.00 | 9.36 | ← spawn | 11 | -2.05 | 3.62 |
| 2 | 2.01 | 7.91 | | 12 | -1.09 | 6.25 |
| 3 | 3.32 | 4.39 | | 13 | 0.00 | 7.06 | ← đỉnh vòng **trong** |
| 4 | 3.65 | -0.20 | ← phải vòng **ngoài** | 14 | 0.91 | 5.92 |
| 5 | 3.00 | -4.60 | | 15 | 1.41 | 3.24 |
| 6 | 1.64 | -7.65 | | 16 | 1.45 | -0.20 | ← phải vòng **trong** |
| 7 | 0.00 | -8.61 | ← đáy vòng **ngoài** | 17 | 1.10 | -3.45 |
| 8 | -1.46 | -7.32 | | 18 | 0.54 | -5.66 |
| 9 | -2.37 | -4.21 | | 19 | 0.00 | -6.31 | ← cầu môn |
| 10 | -2.55 | -0.20 | ← trái vòng ngoài (đi **một lần**) | | | |

### Kết quả kiểm hình học

| Chỉ số | giá trị |
|---|---|
| Độ dài spline | **52.18 u** |
| Hộp bao X | [-2.57, 3.66] — trong ±5.4 ✅ |
| Hộp bao Y | [-8.62, 9.36] — trong ±9.6 ✅ |
| Khe hở dài nhất không ô nào canh | **8 %** (4.0 u) tại f = 0.51 → 0.58 |
| Độ phủ (tầm Lv1 1.4) | 48 % |
| Khoảng cách vòng ngoài ↔ vòng trong | đỉnh 2.30 u · phải 2.20 u · đáy 2.55 u |

**Các cặp đi-qua-hai-lần** (cùng một cung, hai bán kính):

| cung | lần 1 (vòng ngoài) | lần 2 (vòng trong) | cách nhau |
|---|---|---|---|
| Đỉnh | f ≈ 0.07 | f ≈ 0.76 | 2.30 u |
| Phải | f ≈ 0.18 | f ≈ 0.85 | 2.20 u |
| Đáy | f ≈ 0.39 | f ≈ 1.00 | 2.55 u |
| **Trái** | f ≈ 0.60 | **không có** | — |

Nửa trái chỉ được đi **một lần** — đó là cửa rẻ của map (§9 R3).

**Thời gian đi hết đường:** `adepto` 52.2 s · `tifoso` 37.3 s · `tambor` **87.0 s**. Wave 20 kéo dài ~110 s — dài nhất trong bốn map.

⚠️ **Cầu môn nằm ở TÂM xoáy `(0.0, −6.31)`, không ở mép dưới màn hình.** Vòng ngoài đi **vòng qua phía sau cầu môn** ở y = −8.61. Art phải vẽ khung thành nhìn từ trên xuống, không che đường chạy, và thanh máu cầu môn phải neo theo `goalPoint` chứ không neo cứng đáy màn (xem §9 R4).

---

## 3. Toạ độ ô đặt tướng

9 ô sân + 1 ô thủ môn. Cột **lần** = số đoạn đường rời rạc mà vòng tròn tầm 1.4 cắt qua.

| ô | x | y | d | chord@1.0 | chord@1.2 | chord@1.4 | chord@1.8 | lần | đoạn f phủ | tướng Lv1 với tới |
|---|---|---|---|---|---|---|---|---|---|---|
| `f01` | 1.80 | 6.30 | 0.92 | 1.25 | 2.88 | **4.03** | 5.98 | **2** | 0.05–0.10 · 0.74–0.78 | Batigol, D10S, Árbitro, Pulga |
| `f02` | 2.90 | 3.20 | 0.60 | 1.62 | 2.10 | 2.56 | 5.39 | 1 | 0.12–0.17 | Batigol, D10S, Árbitro, Pulga |
| `f03` | 2.55 | 1.30 | 1.07 | 0.00 | 2.00 | **3.53** | 5.74 | **2** | 0.16–0.20 · 0.83–0.87 | D10S, Árbitro, Pulga |
| `f04` | 2.20 | -3.40 | 1.05 | 0.00 | 2.19 | **3.60** | 5.78 | **2** | 0.26–0.29 · 0.92–0.95 | D10S, Árbitro, Pulga |
| `f05` | 0.20 | -7.40 | 1.11 | 0.00 | 0.93 | **3.70** | 5.75 | **2** | 0.36–0.42 · 0.99–1.00 | D10S, Árbitro, Pulga |
| `f06` | -1.10 | -6.60 | 0.63 | 1.64 | 2.21 | 2.92 | 4.33 | **2** | 0.42–0.47 · 0.99–1.00 | Batigol, D10S, Árbitro, Pulga |
| `f07` | -3.70 | -4.40 | 1.34 | 0.00 | 0.00 | 0.74 | 2.29 | 1 | 0.49–0.51 | Pulga |
| `f08` | -3.50 | 1.20 | 1.05 | 0.00 | 1.13 | 1.79 | 2.83 | 1 | 0.58–0.62 | D10S, Árbitro, Pulga |
| `f09` | -0.90 | 5.20 | 0.63 | 1.61 | 2.12 | 2.60 | 3.58 | 1 | 0.66–0.71 | Batigol, D10S, Árbitro, Pulga |
| `gk01` | 1.50 | -5.80 | 0.95 | — | — | 3.82 | 5.62 | 2 | tiếp cận cầu môn | Dibu |

**Thang tầm Lv1:** Batigol **4/9** · D10S 8/9 · Árbitro 8/9 · La Pulga 9/9.

Batigol 4/9 là thấp nhất trong bốn map — **có chủ ý**. Batigol là tướng rẻ của đầu game; ở đây nó chỉ vào được các ô một-lần. Muốn Batigol chiếm ô đi-qua-hai-lần thì phải **nâng lên Lv2 (tầm 1.2)**, và Lv3 (tầm 1.4) mới ăn trọn cả hai vòng. Đó là bài học của map, viết thẳng vào bảng tầm.

**Ràng buộc đã kiểm — tất cả ĐẠT:** không ô chết · không ranh giới dao cạo · hai ô gần nhất `f05`–`f06` = 1.53 u = **56 pt** > 48 pt · vùng chạm nằm trọn trong khung · Batigol chỉ với tới 4/9 ô → tầm là đánh đổi mạnh nhất trong cả bốn map.

**Bảng lợi tức nâng cấp tại chỗ** (chord tăng bao nhiêu khi tăng tầm):

| ô | Lv1→Lv2 | Lv2→Lv3 | tổng Lv1→Lv3 |
|---|---|---|---|
| `f01` (2 lần) | +130 % | +40 % | **+222 %** |
| `f05` (2 lần) | — (Lv1 gần như 0) | +298 % | **rất lớn** |
| `f02` (1 lần) | +30 % | +22 % | +58 % |
| `f09` (1 lần) | +32 % | +23 % | +61 % |

> Nâng cấp một ô-hai-lần lãi gấp **3–4 lần** nâng cấp một ô-một-lần. Đó là toàn bộ nội dung chiến thuật của M06.

---

## 4. Bảng phủ synergy

Map một tuyến nên "chồng vùng phủ" hiếm hơn — 9 ô rải trên 52.18 u thì hầu như không đụng nhau. Nhưng xoáy ốc tạo ra **một dạng synergy mà map thẳng không có**: hai ô canh **hai vòng khác nhau của cùng một cung** vẫn tác dụng liên tiếp lên cùng một con quân, cách nhau đúng một vòng.

| cặp ô | đoạn chồng | kiểu | cặp tướng được thưởng |
|---|---|---|---|
| `f05` + `f06` | **0.49 u** (đoạn f 0.42–0.47 và 0.99–1.00) | chồng thật | Árbitro `f06` × La Pulga Lv2 `f05` (+25 %) — và cả hai đều đánh **hai lần** |
| `f02` + `f03` | 0.24 u | chồng thật | D10S Lv1 `f02` (chậm 3 s) × Pulga `f03` |
| `f01` ↔ chính nó | — | **hai-lần** | Một Árbitro ở `f01` rút thẻ ở vòng ngoài (f 0.05–0.10); thẻ **vĩnh viễn** (FM-16) nên con quân còn bị chậm khi quay lại f 0.74–0.78 — nơi chính `f01` bắn nó lần hai |
| `f03` ↔ `f04` ↔ `f05` | — | **dây chuyền** | Ba ô hai-lần liên tiếp trên cạnh phải + đáy: quân đi ngang chúng ở vòng ngoài rồi lại ở vòng trong |

**Đòn bẩy đặc trưng của M06 — "thẻ vòng ngoài, thu vòng trong".**
Đặt `el_arbitro` ở một ô hai-lần (`f01`, `f03`, `f04` hoặc `f05`). Nó rút thẻ vàng cho quân ở **vòng ngoài**; thẻ theo người và không mất đi. Khi quân quay lại đúng cung đó ở **vòng trong**, chúng đã bị chậm 50 % sẵn → mọi `la_pulga` Lv2 ăn +25 % và mọi tướng ăn +20 % nếu có một `d10s` Lv3 trên sân. Không map nào khác cho một Árbitro duy nhất phủ hai lần lên cùng đàn quân.

⚠️ **Cảnh báo `d10s` Lv3.** Với 9 ô, người chơi rất dễ chỉ có **một** nguồn chậm. Nâng nó lên `d10s` Lv3 = mất nguồn chậm duy nhất (Lv3 không tự làm chậm được) → buff +20 % của chính nó thành số 0. Trên M06 sai lầm này đắt hơn ở map khác vì không có ô dự phòng để đặt Árbitro bù.

---

## 5. Bảng 20 wave

**Luật nhịp:** một tuyến, nhịp spawn 0.7 s, nghỉ 8 s. Boss ra sau cùng.
Số quân **thấp hơn map gốc 25 %** — bù lại đường dài 23 % và mỗi ô làm việc hai lần.

| W | caracol (A/T/Tb) | tổng | HP mỗi `adepto` | HP wave | boss |
|---|---|---|---|---|---|
| 1 | 6/0/0 | 6 | 67 | 402 | — |
| 2 | 10/0/0 | 10 | 73 | 730 | — |
| 3 | 8/3/0 | 11 | 80 | 1 162 | — |
| 4 | 8/6/0 | 14 | 87 | 1 839 | — |
| 5 | 6/4/1 | 11 | 95 | 1 920 | — |
| 6 | 8/6/1 | 15 | 103 | 2 752 | — |
| 7 | 6/6/2 | 14 | 112 | 3 393 | — |
| 8 | 6/4/1 | 11 | 122 | 2 486 | — |
| 9 | 8/6/1 | 15 | 134 | 3 564 | — |
| 10 | 6/6/2 | 14 | 146 | 4 395 | **có** |
| 11 | 8/7/2 | 17 | 159 | 5 456 | — |
| 12 | 8/8/3 | 19 | 173 | 7 279 | — |
| 13 | 8/10/3 | 21 | 188 | 8 763 | — |
| 14 | 10/10/4 | 24 | 205 | 11 092 | — |
| 15 | 8/8/3 | 19 | 224 | 9 426 | — |
| 16 | 8/10/3 | 21 | 244 | 11 348 | — |
| 17 | 10/10/4 | 24 | 266 | 14 365 | — |
| 18 | 10/11/4 | 25 | 290 | 16 295 | — |
| 19 | 11/12/6 | 29 | 316 | 22 250 | — |
| 20 | 14/15/7 | 36 | 344 | 29 454 | **có** |

**Boss `o_capitao` — kháng chậm 82 %** (cao nhất trong ba map ★★★). Cap chậm thực tế = 70 % × 18 % = **12.6 %**.

| | wave | HP đề xuất | thưởng | lý do |
|---|---|---|---|---|
| Boss 1 | 10 | **3 100** | 200 | Đường dài 52.18 u → phơi nhiễm 130 s ở tốc 0.4. Boss dạy: nó đi qua ô hai-lần **hai lần**, người chơi thấy tận mắt vì sao ô đó đắt giá |
| Boss 2 | 20 | **12 100** | 500 | Cao nhất trong bốn map, vì đường dài nhất. Kháng chậm 82 % → Árbitro chỉ kéo dài thêm ~13 % thời gian trên sân |

> HP suy từ tỉ lệ 52.18 / 42.28 = 1.234 nhân số của map gốc. **Ngoại suy bậc nhất — phải đo lại bằng test 20 wave chạy engine thật.**

💡 *Tại sao kháng chậm 82 % chứ không 80 %: trên đường dài, mỗi % thời gian ở lại sân đáng giá hơn — cùng một Árbitro Lv3 ở M06 kéo dài phơi nhiễm nhiều gấp 1.23 lần M04 tính theo unit tuyệt đối. Nâng 2 điểm kháng để giữ Árbitro ở vai "giúp", không thành "giải hộ".*

---

## 6. Kinh tế đề xuất

| Mục | Giá trị |
|---|---|
| `startingCash` | **600** |
| `goalHealth` | 20 |
| Số ô sân | **9** |
| **Trần chi tiêu** = 9 × 1 020 + 476 | **9 656** |
| Tiền cả đời một trận | **9 434** |
| Biên | **+2.3 %** ✅ |

Phân rã: 600 khởi đầu + 6 269 thưởng hạ quân + 1 385 clear + 480 skip + 700 boss.

**Ràng buộc trần chi tiêu là cái buộc M06 phải nghèo.** Chỉ 9 ô → trần chỉ 9 656, thấp hơn map gốc 17 %. Nếu giữ nguyên bảng wave của map gốc thì tiền cả đời (11 507) **vượt trần** → người chơi max được toàn sân từ W17 và act 3 mất sạch sức căng. Vì thế bảng wave §5 nhỏ hơn 25 %.

Nghịch lý cần nói thẳng: **ít ô hơn KHÔNG có nghĩa là khó hơn.** Ít ô → ít tiền cần → wave nhỏ hơn → có thể dễ hơn. Cái làm M06 khó (★★★★) là **chọn ô**, không phải khối lượng quân. Đây là điểm PHẢI đo:

**Đích cần đo:**
1. Headroom mỗi wave trong [1.05, 1.65].
2. **So sánh hai build:** "9 ô Batigol Lv1" (rải mỏng) và "4 ô hai-lần nâng Lv3" (dồn). Build dồn phải thắng **rõ rệt** — nếu build rải thắng thì bán kính co của xoáy ốc quá lớn, phải rút từ 2.2/2.3 u xuống ~2.0 u để ô hai-lần dễ đạt hơn.
3. `startingCash` 600: đủ mua đúng 2 Batigol + 1 lần nâng (120 + 120 + 96 = 336) hoặc 1 D10S + 1 Batigol (360). Kiểm bằng tay: **không** được đủ mua La Pulga + D10S (540) và còn tiền nâng.
4. Thời lượng W20 (~110 s với `tambor` tốc 0.6 trên 52.18 u) — nếu quá dài, giảm `tambor` chứ **đừng** rút ngắn đường.

---

## 7. Đường thắng dự kiến & build cố ý thua

### Đường thắng

| Giai đoạn | Nước đi | Lý do |
|---|---|---|
| Kickoff (600) | Batigol `f02` (120) + Batigol `f09` (120) + Batigol `f06` (120) | Ba ô Batigol Lv1 với tới. `f06` đã là ô **hai-lần** (chord 2.92 ngay ở Lv1 1.0 → 1.64) |
| W3–W6 | Nâng `f06` → Lv2 rồi Lv3 (96 + 192) | Ô hai-lần: Lv3 tầm 1.4 → chord 2.92, và nó ôm cả đoạn f 0.99–1.00 sát cầu môn |
| W7–W10 | Bán `f02` (hoàn 60 % = 72), đặt **D10S** `f04` (240) | `f04` là ô hai-lần, D10S Lv1 tầm 1.2 đã phủ 2.19 u ở **hai** cung |
| W11–W14 | **El Árbitro `f01`** (240) — ô hai-lần ở đỉnh | Thẻ vàng rút ở vòng ngoài (f 0.05–0.10) còn hiệu lực khi quân quay lại f 0.74–0.78 |
| W15–W18 | La Pulga `f05` (300) → Lv2 (240) | `f05` chord 3.70 ở tầm 1.4; Lv2 (+25 % lên mục tiêu bị chậm) ăn trọn thẻ của `f01` |
| W19–W20 | Nâng `f04` → Lv2 (lan 1.7) ; Dibu Lv2 `gk01` | Boss W20: `gk01` chord 3.82 ở Lv1 — cao nhất trong bốn map |

Tổng chi ≈ 8 900 / trần 9 656. Người chơi **không** max được 9 ô — đúng thiết kế.

### Build CỐ Ý thua

**"Rải đều chín ô"** — mua Batigol Lv1 ở tất cả ô mua nổi, không nâng cấp ô nào quá Lv2.

Vì sao thua, theo thứ tự:
1. **5/9 ô Batigol Lv1 không với tới đường** (`f03`, `f04`, `f05`, `f07`, `f08` — d từ 1.05 đến 1.34). Người chơi tiêu 600 Peso vào tướng không bắn được gì. Đây là lỗi mà người chơi thật đã báo ba lần trên map gốc; ở M06 nó **là bài kiểm tra**, không phải bug.
2. Bốn ô còn dùng được (`f01`, `f02`, `f06`, `f09`) đều chỉ phủ ở tầm 1.0 → tổng chord Lv1 = 1.25 + 1.62 + 1.64 + 1.61 = **6.12 u trên 52.18 u = 12 %**.
3. Không nâng cấp → không ô nào đạt tầm 1.4 → **không ô nào từng bắn hai lần**. Toàn bộ cơ chế của map bị bỏ qua.
4. `f07` (d 1.34) chỉ La Pulga với tới, chord 0.74 — ô tệ nhất map. Người chơi rải đều sẽ mua nó và không hiểu vì sao nó im lặng.
5. Boss W20: 12 100 máu, kháng chậm 82 %, gặp một hàng Batigol Lv1/Lv2 tổng DPS thấp. Trừ 5 máu, và W20 còn 36 quân thường.

---

## 8. Yêu cầu kỹ thuật

M06 là map **rẻ nhất** trong bốn map về mặt code: **một tuyến duy nhất, không cần đa tuyến.** Nhưng nó phá ba giả định khác.

### 8.1 Schema

Giữ nguyên shape `path` đơn của `config/path.json` hiện tại. Chỉ thay `waypoints`, `lengthUnits` = 52.18, `spawnPoint` = `(0, 9.36)`, `goalPoint` = **`(0, −6.31)`**.

Nếu dự án đã chuyển sang `paths: [...]` cho M04/M05/M07 thì M06 khai đúng **một** phần tử.

### 8.2 Cái PHẢI đổi

| Vấn đề | File | Đổi gì |
|---|---|---|
| **Cầu môn không ở đáy màn** | `Pitch.cs`, `Hud.cs` | Khung thành + thanh máu phải neo theo `path.goalPoint`, không neo cứng `y = −9.1`. Hiện `Pitch.cs:11` ghi giả định "spline tự do 14 waypoint" — số waypoint cũng phải thôi cứng |
| **Đường tự cắt / tự áp sát** | `Pitch.cs:98` `Road(EnemyPath, ...)` | Dải lưới bám spline: ở M06 hai vòng cách nhau 2.2 u, với `width` > 2.0 hai vòng sẽ dính vào nhau thành một mảng. Chốt `width ≤ 1.2` cho map này, hoặc vẽ viền |
| **Đường dài 52.18 u** | `EnemyPath.cs` | Kiểm số mẫu nội suy: 4 000 mẫu / 52.18 u = 77 mẫu/unit, vẫn dư. Nhưng `PositionAt` tra tuyến tính theo `DistanceTravelled` — đo lại chi phí khi có 36 quân |
| **Bất biến test chốt cứng** | `core/.../PathConfigTests.cs:17-23` | `Assert.Equal(14, p.Waypoints.Count)` và `12, p.Slots.Count` → M06 là 19 và 10. Test phải nhận số theo map |
| | `core/.../SlotAndUpgradeTests.cs:35-36` | Chốt cứng 12/11 → phải đọc từ `economy.fieldSlots` |
| **`economy.json`** | | `fieldSlots: 9`, `startingCash: 600`, `maxBuildCost.expected: 9656` |
| **Luật validator mới** | `ConfigValidator.cs` | Luật "ô đi-qua-hai-lần": cảnh báo (không phải lỗi) nếu **không có ô nào** mà vòng tròn tầm 1.4 cắt ≥ 2 đoạn đường rời rạc — vì khi đó xoáy ốc không còn là xoáy ốc |
| **`tools/path_check.py`** | | Đã in `runs` (số đoạn bị cắt) nhưng chỉ dùng để **báo cáo**. Với M06 phải nâng thành **ràng buộc**: ≥ 4 ô có `runs ≥ 2` ở tầm 1.4 (M06 hiện có 5) |

### 8.3 KHÔNG cần đổi
`MatchController`, `Enemy`, `ScheduledSpawn`, `WaveSpawner`, `TargetingSystem`, `DamageSystem`, `SlowStack` — không một dòng nào. M06 chạy được trên kiến trúc hôm nay nếu chỉ thay dữ liệu và sửa lớp vẽ.

> 💡 *Điều này đáng chú ý về mặt lộ trình: M06 là map ★★★★ duy nhất **không** cần thay đổi kiến trúc. Nếu cần một map khó sớm mà chưa muốn làm đa tuyến, đây là map để làm trước.*

---

## 9. Rủi ro

| # | Rủi ro | Mức | Giảm thiểu |
|---|---|---|---|
| R1 | **Cầu môn ở giữa màn hình** (`0, −6.31`) — trái mọi quy ước bố cục hiện có; vùng y < −8.6 gần như trống, và vòng ngoài đi *sau lưng* khung thành | CAO | Art phải xử lý từ đầu (§8.2). Phương án dự phòng: kéo cả xoáy ốc xuống và chấp nhận vòng ngoài chạm y = −9.4 → **vi phạm viewport**, nên không dùng. Phương án 2: chấp nhận và vẽ khán đài ở dải trống |
| R2 | Người chơi **không nhận ra** ô nào đi-qua-hai-lần → cả cơ chế vô hình, map chỉ còn là "9 ô ít ỏi" | CAO | UI phải hiện vòng tròn tầm khi chạm ô trống (đã có ở `docs/01 §5`), và **tô sáng đoạn đường nằm trong tầm**. Không có phản hồi này thì M06 không dạy được gì |
| R3 | **Nửa trái chỉ đi một lần** → khe hở f 0.51–0.58 (4.0 u) là cửa rẻ; người chơi tối ưu sẽ bỏ luôn `f07`/`f08` | TRUNG BÌNH | Cố ý: đó là chi phí của một xoáy ốc thật (số vòng lẻ luôn để hở nửa cung). Nếu đo thấy rò rỉ quá nhiều → tăng lên 2.0 vòng, nhưng khi đó bán trục co còn 1.65 u → ô hai-lần dí quá sát nhau, **phải kiểm lại 48 pt** |
| R4 | Thời lượng W20 ~110 s — dài hơn trần "30–120 s" của `docs/03 §5` | TRUNG BÌNH | Giảm `tambor` W19/W20 (từ 6/7 xuống 5/6) trước khi động vào độ dài đường |
| R5 | Chỉ 9 ô → người chơi rất dễ **chỉ có một nguồn chậm**; nâng nó lên D10S Lv3 làm mất luôn nguồn chậm | TRUNG BÌNH | Cảnh báo trong UI khi nâng D10S lên Lv3 mà trên sân không còn nguồn chậm nào khác. Đây là luật B-01 lộ ra ở chỗ đau nhất |
| R6 | HP boss 12 100 là cao nhất game — nếu ngoại suy theo độ dài sai, boss W20 thành bất khả | TRUNG BÌNH | Đo trước khi khoá. Thứ tự chỉnh: **HP boss trước, kháng chậm sau, đừng động bảng wave** |
| R7 | Chord ở Lv1 của `f03`/`f04`/`f05` = 0.00 (Batigol không với tới) — người chơi mới sẽ tưởng ô hỏng | THẤP | Vòng tròn tầm + làm xám nút tướng không với tới (mở rộng FM-02 sang cả trường hợp "ngoài tầm", không chỉ "không đủ tiền") |
