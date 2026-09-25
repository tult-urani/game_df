# M05 — La Confluencia (Hợp Lưu)

> **2 tuyến nhập làm 1 · 11 ô sân + 1 ô thủ môn · boss kháng chậm 80% · ★★★**
> Trạng thái: **THIẾT KẾ** — toạ độ đã kiểm bằng bản sao `tools/path_check.py` (đa tuyến).
> Số kinh tế ở §6 là **ĐÍCH CẦN ĐO** bằng engine thật, không phải kết luận.

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m05 -->
**Nguồn: `config/maps/m05-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 2 — `norte` 28.2u · `sur` 28.3u |
| Ô sân / thủ môn | 11 / 1 |
| Σchord@1.4 mỗi ô | 4.36 |
| Tiền khởi đầu | 600 |
| Máu quái | ×0.67 × 1.07^(W−1) → W20 ×2.42 |
| Tổng quân 20 wave | 403 |

| W | ×máu | `norte` | `sur` | boss | quân | tổng máu |
|---|------|------|------|------|------|----------|
| 1 | ×0.67 | 4/0/0 | 4/0/0 | — | 8 | 536 |
| 2 | ×0.72 | 5/0/0 | 5/0/0 | — | 10 | 720 |
| 3 | ×0.77 | 4/2/0 | 4/2/0 | — | 12 | 1 292 |
| 4 | ×0.82 | 4/4/0 | 4/4/0 | — | 16 | 2 104 |
| 5 | ×0.88 | 4/3/0 | 4/3/1 | — | 15 | 2 345 |
| 6 | ×0.94 | 4/4/0 | 4/4/1 | — | 17 | 2 925 |
| 7 | ×1.01 | 4/4/1 | 4/4/2 | — | 19 | 4 235 |
| 8 | ×1.08 | 4/3/0 | 4/3/1 | — | 15 | 2 878 |
| 9 | ×1.15 | 4/4/0 | 4/4/1 | — | 17 | 3 577 |
| **10** | ×1.23 | 4/4/1 | 4/4/2 | `o_capitao`@`norte` | 20 | 7 683 |
| 11 | ×1.32 | 4/4/1 | 4/4/2 | — | 19 | 5 551 |
| 12 | ×1.41 | 4/5/2 | 4/4/2 | — | 21 | 7 022 |
| 13 | ×1.51 | 4/6/2 | 4/4/2 | — | 22 | 7 848 |
| 14 | ×1.61 | 5/6/2 | 5/4/3 | — | 25 | 9 600 |
| 15 | ×1.73 | 4/5/2 | 4/4/2 | — | 21 | 8 604 |
| 16 | ×1.85 | 4/6/2 | 4/4/2 | — | 22 | 9 618 |
| 17 | ×1.98 | 5/6/2 | 5/4/3 | — | 25 | 11 770 |
| 18 | ×2.12 | 5/7/2 | 5/5/4 | — | 28 | 14 696 |
| 19 | ×2.26 | 6/7/3 | 6/6/4 | — | 32 | 17 908 |
| **20** | ×2.42 | 7/9/4 | 8/8/4 | `o_capitao`@`sur` | 41 | 33 155 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m05 -->

---

## 1. Ý đồ thiết kế

Hai nhánh riêng biệt chảy xuống, gặp nhau ở `(0.0, −1.40)` rồi **nhập làm một thân chung** dài 12.1 u đổ về cầu môn. Nhánh chiếm 57 % quãng đường, thân chung 43 %.

Điểm hợp lưu là chỗ **mật độ quân gấp đôi** — hai dòng spawn cùng giây 0, hai nhánh gần bằng nhau (28.31 vs 28.40 u) nên quân từ hai bên tới hợp lưu gần như cùng lúc. Đó là mảnh đất vàng của AoE: `d10s` Lv2 bán kính lan 1.7 ở `t01`/`t02` chạm được số mục tiêu cao nhất trong cả bốn map.

**Nhưng dồn hết vào hợp lưu là KHÔNG ĐỦ** — và lý do không phải "quân lọt trước điểm nhập" (về hình học thì mọi con quân đều bắt buộc đi qua hợp lưu). Lý do thật có ba tầng:

1. **Bão hoà thông lượng.** Nhịp bắn hữu hạn. Ở W19 có 37 con dồn vào 12.1 u thân chung; một cụm 3 tướng ở hợp lưu có tổng chord ~13 u nhưng mỗi phát chỉ giết được một (hoặc một cụm nhỏ). Sát thương thừa đổ vào con đã chết là sát thương mất trắng (FM-07).
2. **Thời gian phơi nhiễm ngắn.** Thân chung chỉ 12.1 u = 12.1 s với `adepto`, **8.6 s** với `tifoso`. Một tướng bắn 1.2 s/phát chỉ kịp 7 phát vào một `tifoso` đi qua toàn bộ thân chung — nếu nó chưa bị bào ở nhánh thì nó về tới lưới.
3. **Máu vào thân chung phải đã bị bào.** Nhánh chiếm 57 % đường; bỏ trống nhánh nghĩa là quân tới hợp lưu với 100 % máu, và HP wave 20 là 37 308 — không cụm nào ở thân chung xử lý nổi trong 12 s.

Điều map này DẠY: **AoE khuếch đại, nhưng không thay thế được sát thương nền.** Người chơi phải trả tiền cho cả hai nhánh trước, rồi mới được hưởng phần thưởng của hợp lưu.

---

## 2. Toạ độ waypoint

Nội suy `catmull-rom`. Hai tuyến **dùng chung đuôi bit-đối-bit** kể từ waypoint hợp lưu `(0.0, −1.40)` (xem §8.1 — đây là yêu cầu schema, không phải trùng hợp toạ độ).

### Thân chung `TRUNK` — 6 waypoint · **12.1 u**

| # | x | y |
|---|---|---|
| T1 | 0.00 | -1.40 | ← **điểm hợp lưu** |
| T2 | 2.20 | -3.20 |
| T3 | 0.40 | -5.60 |
| T4 | -2.20 | -7.20 |
| T5 | -0.60 | -8.70 |
| T6 | 0.00 | -9.13 | ← cầu môn |

### Tuyến `norte` — 7 waypoint nhánh + TRUNK · **28.31 u**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 1 | -3.40 | 9.36 | | 5 | -2.80 | 2.00 |
| 2 | -4.80 | 7.40 | | 6 | -3.60 | 0.20 |
| 3 | -3.00 | 5.60 | | 7 | -1.60 | -0.80 |
| 4 | -4.60 | 3.60 | | → | *TRUNK* | |

### Tuyến `sur` — 7 waypoint nhánh + TRUNK · **28.40 u**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 1 | 3.40 | 9.36 | | 5 | 2.80 | 2.00 |
| 2 | 4.80 | 7.40 | | 6 | 3.60 | 0.20 |
| 3 | 3.00 | 5.60 | | 7 | 1.60 | -0.80 |
| 4 | 4.60 | 3.60 | | → | *TRUNK* | |

### Kết quả kiểm hình học

| Chỉ số | norte | sur |
|---|---|---|
| Độ dài spline | **28.31 u** | **28.40 u** |
| Hộp bao X | [-4.80, 2.20] | [-2.21, 4.80] |
| Hộp bao Y | [-9.13, 9.36] | [-9.13, 9.36] |
| Tràn viewport | KHÔNG | KHÔNG |
| Điểm hợp lưu tại f | **0.573** | **0.574** |
| Khe hở dài nhất không ô nào canh | 7 % (2.1 u) tại f=0.13→0.20 | 7 % (2.0 u) tại f=0.24→0.31 |
| Độ phủ (tầm Lv1 1.4) | 72 % | 73 % |

**Quan hệ hai tuyến:** đoạn chồng nhau (< 0.35 u) = **12.48 u** ≈ đúng chiều dài thân chung → xác nhận hợp lưu là **trùng khít**, không phải hai đường chạy song song. Trước hợp lưu, khoảng cách hai nhánh ≥ 2.6 u ở mọi độ cao ngoại trừ đoạn tiếp cận (`c01` là ô duy nhất với tới cả hai nhánh trước điểm nhập).

**Thời gian:** `adepto` đi hết nhánh trong **16.2 s**, thân chung thêm **12.1 s** (tổng 28.3 s). `tifoso` 11.6 s + 8.6 s. `tambor` 27.0 s + 20.2 s.

---

## 3. Toạ độ ô đặt tướng

11 ô sân + 1 ô thủ môn. `chord` cộng cả hai tuyến — với ô ở **thân chung**, con số này bị đếm hai lần **một cách đúng đắn**, vì quân của cả hai tuyến đều đi qua đó.

| ô | x | y | d.norte | d.sur | chord@1.0 | chord@1.2 | chord@1.4 | tướng Lv1 với tới | tuyến canh |
|---|---|---|---|---|---|---|---|---|---|
| `n01` | -3.66 | 7.94 | 0.78 | 7.04 | 1.34 | 2.01 | 3.56 | Batigol, D10S, Árbitro, Pulga | norte |
| `n02` | -3.52 | 3.67 | 0.87 | 6.06 | 2.14 | 3.50 | 4.02 | Batigol, D10S, Árbitro, Pulga | norte |
| `n03` | -1.88 | 0.69 | 1.30 | 2.72 | 0.00 | 0.00 | 1.89 | Pulga | norte |
| `s01` | 3.97 | 7.66 | 7.26 | 0.71 | 2.49 | 3.14 | 3.67 | Batigol, D10S, Árbitro, Pulga | sur |
| `s02` | 2.81 | 3.73 | 5.85 | 1.29 | 0.00 | 0.00 | 2.00 | Pulga | sur |
| `s03` | 2.24 | 0.37 | 2.66 | 0.88 | 0.99 | 1.96 | 4.02 | Batigol, D10S, Árbitro, Pulga | sur |
| `c01` | -0.20 | -0.10 | 1.09 | 1.11 | 0.00 | 1.45 | 2.75 | D10S, Árbitro, Pulga | **norte+sur** (trước hợp lưu) |
| `t01` | 2.05 | -1.86 | 0.70 | 0.70 | 2.59 | 4.17 | **5.92** | Batigol, D10S, Árbitro, Pulga | **thân chung** |
| `t02` | 1.07 | -3.10 | 0.88 | 0.88 | 2.16 | 5.11 | **7.04** | Batigol, D10S, Árbitro, Pulga | **thân chung** |
| `t03` | 0.63 | -6.80 | 1.10 | 1.10 | 0.00 | 1.66 | 3.05 | D10S, Árbitro, Pulga | **thân chung** |
| `t04` | -1.00 | -7.37 | 0.87 | 0.87 | 4.06 | 7.34 | **8.87** | Batigol, D10S, Árbitro, Pulga | **thân chung** |
| `gk01` | -1.60 | -8.80 | 0.64 | 0.64 | — | — | 4.89 | Dibu | thân chung |

**Thang tầm Lv1:** Batigol 7/11 · D10S 9/11 · Árbitro 9/11 · La Pulga 11/11.
(`c01`, `t03` không dùng được Batigol Lv1 · `n03`, `s02` chỉ La Pulga.)

**Ràng buộc đã kiểm — tất cả ĐẠT:** không ô chết · không ranh giới dao cạo trên bất kỳ tuyến nào · hai ô gần nhất `t01`–`t02` = 1.58 u = **58 pt** > 48 pt · vùng chạm nằm trọn trong khung · Batigol không với tới mọi ô.

**Ô thủ môn `gk01`** cách thân chung 0.64, chord 4.89 (Lv1) → 6.31 (Lv3) — Dibu ở đây chặn được cả hai luồng vì chúng đã nhập một.

**Đọc bảng:** bốn ô thân chung (`t01`–`t04`) có chord 5.92 / 7.04 / 3.05 / 8.87 — cao gấp đôi ô nhánh. Đó chính là "chỗ vàng" mà brief nói tới, và nó **đúng là chỗ vàng**. Cái bẫy là chúng chỉ phủ 43 % cuối quãng đường.

---

## 4. Bảng phủ synergy

| cặp ô | đoạn chồng | ở tuyến | cặp tướng được thưởng |
|---|---|---|---|
| `t01` + `t02` | **4.42 u** (norte 2.21 + sur 2.21) | thân chung | **Cụm hợp lưu.** Árbitro `t01` × D10S Lv2 `t02` (lan 1.7) · hoặc Árbitro `t01` × La Pulga Lv2 `t02` (+25 %) |
| `s03` + `t01` | 0.94 u | sur, ngay trước hợp lưu | Nguồn chậm ở `s03` "giao ca" cho sát thương ở `t01` |
| `t03` + `t04` | 0.59 u | thân chung | Cụm cuối, ngay trước `gk01` |
| `t04` + `gk01` | tiếp giáp | thân chung | Dibu chặn × mọi thứ ở `t04` (chord 8.87 — cao nhất bốn map) |

**Cụm `t01`+`t02` là ô trục của map.** Hai ô cách nhau 1.58 u, cùng ôm khúc cua ngay sau hợp lưu, mỗi ô nhìn thấy **cả hai luồng quân**. Đặt `el_arbitro` một ô và `d10s` Lv2 ô kia = mọi con quân trong game đều đi qua vùng đó ở trạng thái bị chậm.

⚠️ **Nhưng phải trả giá trước.** Nếu `n01`/`n02`/`s01`/`s03` để trống, quân tới `t01` với 100 % máu; ở W16 trở đi HP wave > 14 000 và cụm hai ô không xử lý kịp trong 8.6 s (thời gian `tifoso` qua thân chung).

⚠️ **Trục thượng nguồn.** Thẻ vàng của Árbitro là **vĩnh viễn** (FM-16). Một Árbitro ở `n02` hoặc `s03` (nhánh) rút thẻ, rồi La Pulga Lv2 ở `t01`/`t02`/`t04` (thân chung) ăn +25 % — **không cần chồng vùng phủ**. Đây là cách chơi rẻ hơn cụm hợp lưu và là lối chơi "đúng" của map: đầu tư ở nhánh, thu hoạch ở thân.

⚠️ **D10S Lv3 ở thân chung là bẫy chết người.** Nó bỏ khả năng tự làm chậm. Nếu D10S ở `t02` là nguồn chậm duy nhất và bị nâng lên Lv3, buff +20 % của chính nó không bao giờ kích hoạt — người chơi mất tiền để yếu đi.

---

## 5. Bảng 20 wave

**Luật nhịp:** hai nhánh spawn **cùng giây 0**, nhịp 0.7 s **mỗi nhánh**. Vì hai nhánh dài gần bằng nhau, hai luồng chạm hợp lưu lệch nhau < 0.1 s → mật độ ở thân chung đúng bằng **2×**. Đó là thiết kế, không phải trùng hợp.

| W | norte (A/T/Tb) | sur (A/T/Tb) | tổng | HP mỗi `adepto` | HP wave | boss |
|---|---|---|---|---|---|---|
| 1 | 4/0/0 | 4/0/0 | 8 | 67 | 536 | — |
| 2 | 6/0/0 | 6/0/0 | 12 | 73 | 876 | — |
| 3 | 5/2/0 | 5/2/0 | 14 | 80 | 1 497 | — |
| 4 | 5/4/0 | 5/4/0 | 18 | 87 | 2 395 | — |
| 5 | 4/3/0 | 4/3/1 | 15 | 95 | 2 525 | — |
| 6 | 5/4/0 | 5/4/1 | 19 | 103 | 3 412 | — |
| 7 | 4/4/1 | 4/4/2 | 19 | 112 | 4 731 | — |
| 8 | 4/3/0 | 4/3/1 | 15 | 122 | 3 270 | — |
| 9 | 5/4/0 | 5/4/1 | 19 | 134 | 4 419 | — |
| 10 | 4/4/1 | 4/4/2 | 19 | 146 | 6 126 | **`norte`** |
| 11 | 5/5/1 | 5/4/2 | 22 | 159 | 7 344 | — |
| 12 | 5/6/2 | 5/4/2 | 24 | 173 | 9 336 | — |
| 13 | 5/7/2 | 5/5/2 | 26 | 188 | 11 005 | — |
| 14 | 6/7/2 | 6/5/3 | 29 | 205 | 13 536 | — |
| 15 | 5/6/2 | 5/4/2 | 24 | 224 | 12 090 | — |
| 16 | 5/7/2 | 5/5/2 | 26 | 244 | 14 252 | — |
| 17 | 6/7/2 | 6/5/3 | 29 | 266 | 17 530 | — |
| 18 | 6/8/2 | 6/6/4 | 32 | 290 | 21 978 | — |
| 19 | 7/8/3 | 7/7/5 | 37 | 316 | 28 760 | — |
| 20 | 8/10/4 | 9/9/5 | 45 | 344 | 37 308 | **`sur`** |

**Quy tắc chia nhánh:** `norte` nhận 55 % `tifoso` (quân nhanh), `sur` nhận 60 % `tambor` (quân tank) — hai nhánh **không đối xứng về loại quân** dù đối xứng về hình học. Người chơi không được sao chép đội hình từ nhánh này sang nhánh kia.

**Boss `o_capitao` — kháng chậm 80 %.** Cap chậm thực tế = 70 % × 20 % = **14 %**.

| | wave | tuyến | HP đề xuất | thưởng | lý do |
|---|---|---|---|---|---|
| Boss 1 | 10 | `norte` | **1 700** | 200 | Đi hết nhánh + thân chung: phơi nhiễm dài nhất → boss dạy |
| Boss 2 | 20 | `sur` | **6 600** | 500 | Người chơi đã dồn hết vào thân chung; boss cũng đi thân chung nhưng đi MỘT MÌNH nên AoE mất hệ số ×n mục tiêu |

> HP suy ra từ tỉ lệ độ dài (28.31 u so với 42.28 u của map gốc). **Phải đo lại bằng test 20 wave chạy engine thật.**

💡 *Boss ở map này là bài kiểm tra đúng của cơ chế hợp lưu: cụm AoE ở `t01`/`t02` mạnh nhất game khi có 20 con đi cùng lúc, và yếu nhất game khi chỉ có một khối 6 600 máu.*

---

## 6. Kinh tế đề xuất

| Mục | Giá trị |
|---|---|
| `startingCash` | **680** |
| `goalHealth` | 20 |
| Số ô sân | 11 |
| **Trần chi tiêu** = 11 × 1 020 + 476 | **11 696** |
| Tiền cả đời một trận | **11 274** |
| Biên | **+3.6 %** ✅ |

Phân rã: 680 khởi đầu + 7 929 thưởng hạ quân + 1 385 clear + 480 skip + 700 boss.

`startingCash` **680** (thấp hơn map gốc 20 Peso) là cố ý: ở 680 người chơi mua được đúng **hai** tướng Lv1 rẻ (Batigol 120 × 2 + Árbitro 240 = 480, dư 200) — vừa đủ đặt một chốt mỗi nhánh, **không** đủ để mở màn bằng cụm hợp lưu. Quyết định mở màn bị ép về đúng bài học của map.

⚠️ **Không dùng `tools/balance_sim.py` để chốt.** Công cụ báo sai trên map một tuyến (headroom trung bình 2.09 trong khi engine thật thắng còn 11/20 máu) và không có khái niệm hợp lưu — nó sẽ đếm thân chung một lần trong khi thực tế mật độ ở đó gấp đôi. Số headroom phải lấy từ **test 20 wave chạy `MatchController`**.

**Đích cần đo:**
1. Headroom mỗi wave trong [1.05, 1.65].
2. **Đo riêng cho hai kiểu build:** "toàn thân chung" (4 ô `t*` max) và "cân nhánh–thân". Build toàn-thân-chung phải THUA từ W16 trở đi — nếu nó thắng thì thân chung quá dài, rút `TRUNK` xuống ~10 u.
3. Số quân còn sống khi chạm hợp lưu ở W19: mục tiêu 55–70 % (nếu > 80 % nghĩa là nhánh vô dụng, nếu < 40 % nghĩa là hợp lưu vô dụng).
4. Dư tiền tối đa < 400 Peso.

---

## 7. Đường thắng dự kiến & build cố ý thua

### Đường thắng

| Giai đoạn | Nước đi | Lý do |
|---|---|---|
| Kickoff (680) | Batigol `n02` (120) + Batigol `s03` (120) + Batigol `t02` (120) | Ba chord tốt nhất mua được bằng tiền khởi đầu: 4.02 / 4.02 / 7.04 |
| W3–W6 | Nâng `t02` → Lv2 ; thêm Batigol `t04` (120) | `t04` chord 8.87 — ô đắt giá nhất map |
| W7–W10 | Bán Batigol `t02`, đặt **D10S** `t02` (240) ; **El Árbitro** `t01` (240) | Cụm hợp lưu: Árbitro chậm → D10S Lv2 lan 1.7 quét cả hai luồng |
| W11–W14 | Nâng D10S `t02` → Lv2 (192) ; thêm La Pulga `n02` (300) và `s01` (300) | Bào máu ở nhánh để thân chung không bão hoà |
| W15–W18 | **Bán `n03` / `s02` nếu đã lỡ mua** (chord 1.89 / 2.00, chỉ Pulga dùng được) ; La Pulga Lv3 `t04` | Ô nhánh xa là khoản đầu tư kém nhất khi tiền cạn |
| W19–W20 | Dibu Lv2 `gk01` ; giữ ≥ 2 nguồn đơn mục tiêu ở thân chung | Boss W20 đi một mình — AoE mất giá trị |

### Build CỐ Ý thua

**"Pháo đài hợp lưu"** — toàn bộ tiền vào `t01`, `t02`, `t03`, `t04` (4 ô max = 4 080 Peso) + Dibu Lv3, không đặt gì ở nhánh.

Vì sao thua, theo thứ tự:
1. **W1–W12 nó THẮNG dễ dàng** — đây là điều làm nó nguy hiểm. Chord cộng dồn 24.88 u ở thân chung dư sức xử lý HP wave < 10 000.
2. **W15 trở đi hỏng.** Quân tới hợp lưu với 100 % máu. HP wave 12 090 phải bị xoá hết trong 12.1 u × (thời gian phơi nhiễm 8.6 s với `tifoso`). Bốn tướng không đủ nhịp bắn — sát thương thừa đổ vào con đã chết bị vứt (FM-07).
3. **`tambor` xuyên thủng.** W20 có 9 `tambor` (550 × 3.44 = 1 892 máu mỗi con, lọt trừ 2 máu). Không có Árbitro ở nhánh nên không con nào mang thẻ khi tới thân chung.
4. **Boss W20 kết liễu.** 6 600 máu, kháng chậm 80 %, đi một mình trên `sur` rồi thân chung. Cụm AoE mất hết hệ số ×n mục tiêu; D10S Lv2 đánh một con là D10S tệ nhất game.

---

## 8. Yêu cầu kỹ thuật

### 8.1 Hợp lưu phải là ĐUÔI CHUNG khai báo tường minh — không phải trùng toạ độ

Nếu chỉ chép cùng bộ waypoint vào cuối hai tuyến, Catmull-Rom sẽ sinh **hai spline hơi lệch nhau** ở đoạn ngay sau điểm nhập (tiếp tuyến tại waypoint phụ thuộc điểm liền trước, mà điểm đó khác nhau giữa hai nhánh). Lệch nhỏ nhưng đủ để hai luồng quân đi song song lệch nhau ~0.2 u — người chơi thấy "hai đường gần trùng" thay vì "một đường".

```jsonc
"paths": [
  { "id": "norte", "lengthUnits": 28.31, "waypoints": [ /* 7 nhánh + 6 TRUNK */ ] },
  { "id": "sur",   "lengthUnits": 28.40,
    "waypoints": [ /* 7 nhánh của sur */ ],
    "mergeInto": { "pathId": "norte", "atPoint": {"x": 0.0, "y": -1.40} } }
]
```

`mergeInto` nghĩa là: dựng spline riêng của `sur` tới `atPoint`, rồi **nối nguyên xi** phần đuôi đã lấy mẫu của `norte` từ điểm đó. Đảm bảo đuôi bit-đối-bit.

### 8.2 Schema `waves.json`

```jsonc
{ "wave": 20,
  "spawns": { "norte": {"adepto": 8, "tifoso": 10, "tambor": 4},
              "sur":   {"adepto": 9, "tifoso": 9,  "tambor": 5} },
  "laneOffsetsSec": { "norte": 0.0, "sur": 0.0 },
  "boss": { "id": "o_capitao", "lane": "sur" } }
```

### 8.3 Code phải đổi

Tất cả các mục ở M04 §8.3 (đa tuyến), **cộng thêm**:

| File | Đổi gì |
|---|---|
| `EnemyPath.cs` | Thêm constructor nhận đường cong đã lấy mẫu (không phải waypoint) để dựng đuôi chung; hoặc thêm `EnemyPath.WithSharedTail(EnemyPath src, Vec2 at)` |
| `ConfigModel.cs` | `PathDef.MergeInto` (nullable) |
| `ConfigValidator.cs` | Luật mới: (a) `mergeInto.pathId` phải tồn tại và **không được tạo vòng**; (b) `atPoint` phải là một waypoint có thật của cả hai tuyến; (c) hai tuyến đã nhập phải có **cùng** `goalPoint` |
| `MatchController` | Không đổi thêm — sau khi nhập, hai `EnemyPath` khác nhau nhưng trả cùng toạ độ; quân vẫn tra theo `LaneId` của mình |
| `Pitch.cs:98` | Vẽ đuôi chung **một lần** (nếu vẽ hai lần thì đường dày gấp đôi ở thân chung — người chơi đọc nhầm là đường khác) |
| `tools/path_check.py` | Thêm phép đo "độ dài đoạn hai tuyến chồng nhau" để xác nhận hợp lưu trùng khít (M05 kỳ vọng **12.48 u**) |

### 8.4 KHÔNG cần đổi
`TargetingSystem` / `CombatResolver` / `DamageSystem` / `SlowStack` — chúng chỉ nhìn toạ độ. Ở thân chung, quân hai tuyến đứng cạnh nhau nên AoE quét cả hai **mà không cần một dòng code nào biết đến "tuyến"**. Đây là lý do M05 rẻ hơn M04 về mặt kỹ thuật dù nghe có vẻ phức tạp hơn.

---

## 9. Rủi ro

| # | Rủi ro | Mức | Giảm thiểu |
|---|---|---|---|
| R1 | **Brief nói "quân lọt trước khi tới điểm nhập" — điều đó KHÔNG THỂ xảy ra** về hình học: mọi con đều phải đi qua hợp lưu để tới lưới. Nếu ai đó cài luật "rò rỉ ở nhánh" thì đó là cơ chế mới, không phải hệ quả của hợp lưu | CAO | Đã thiết kế lại lập luận theo bão-hoà-thông-lượng (§1). Nếu muốn rò rỉ thật ở nhánh thì phải có **hai cầu môn phụ** — đó là map khác, không phải M05 |
| R2 | Thân chung 43 % có thể vẫn quá dài → "pháo đài hợp lưu" thắng được | CAO | Đo build-thua ở §7. Nếu nó thắng W20 → rút `TRUNK` từ 12.1 u xuống ~10 u bằng cách dời điểm hợp lưu xuống `(0.0, −2.6)` |
| R3 | Hai nhánh đối xứng gương → người chơi sao chép đội hình, mất một nửa quyết định | TRUNG BÌNH | Đã chống bằng **chia loại quân bất đối xứng** (§5) và ô bất đối xứng (`n03` d 1.30 vs `s02` d 1.29 nhưng chord khác nhau). Nếu playtest vẫn thấy sao chép → đổi hình nhánh `sur` cho khác hẳn |
| R4 | Lệch spline ở đoạn sau hợp lưu nếu quên `mergeInto` | TRUNG BÌNH | Luật validator ở §8.3; và `path_check` phải khẳng định đoạn chồng ≈ 12.48 u |
| R5 | Mật độ 2× ở thân chung → số quân trên sân cùng lúc cao, `CombatResolver.SplashInto` quét danh sách quân nhiều lần mỗi frame | TRUNG BÌNH | Đo thời gian frame ở W19–W20; nếu vượt ngân sách thì thêm lưới không gian, **đừng** giảm số quân (sẽ phá cân bằng) |
| R6 | `n03` (chord 1.89) và `s02` (chord 2.00) chỉ La Pulga với tới → dễ thành ô chết về mặt kinh tế | THẤP | Cố ý — chúng là ô "chỉ mua khi giàu". Nếu telemetry cho thấy 0 % người chơi mua thì kéo d về 1.10 |
