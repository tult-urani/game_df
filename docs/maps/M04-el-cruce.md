# M04 — El Cruce (Ngã Tư)

> **2 tuyến chạy ĐỒNG THỜI · 12 ô sân + 1 ô thủ môn · boss kháng chậm 80% · ★★★**
> Trạng thái: **THIẾT KẾ** — toạ độ đã kiểm bằng bản sao `tools/path_check.py` (đa tuyến).
> Chưa có số cân bằng đo bằng engine. Mọi con số kinh tế ở §6 là **ĐÍCH CẦN ĐO**, không phải kết luận.

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m04 -->
**Nguồn: `config/maps/m04-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 2 — `corta` 23.1u · `larga` 23.1u |
| Ô sân / thủ môn | 13 / 2 |
| Σchord@1.4 mỗi ô | 3.07 |
| Tiền khởi đầu | 800 |
| Máu quái | ×0.67 × 1.075^(W−1) × milestone → W20 ×5.29 |
| Tổng quân 20 wave | 465 |

| W | ×máu | `corta` | `larga` | boss | quân | tổng máu |
|---|------|------|------|------|------|----------|
| 1 | ×0.67 | 4/0/0 | 4/0/0 | — | 8 | 536 |
| 2 | ×0.72 | 6/0/0 | 7/0/0 | — | 13 | 936 |
| 3 | ×0.77 | 5/2/0 | 5/2/0 | — | 14 | 1 450 |
| 4 | ×0.83 | 5/4/0 | 5/4/0 | — | 18 | 2 294 |
| **5** | ×1.07 | 4/3/1 | 4/3/0 | `o_capitao`@`corta` | 16 | 3 863 |
| 6 | ×1.15 | 5/4/1 | 4/4/1 | — | 19 | 4 337 |
| 7 | ×1.24 | 4/4/2 | 4/4/1 | — | 19 | 5 222 |
| 8 | ×1.33 | 4/3/1 | 4/3/0 | — | 15 | 3 556 |
| 9 | ×1.43 | 5/4/1 | 4/4/1 | — | 19 | 5 385 |
| **10** | ×1.80 | 4/4/2 | 4/4/1 | `o_capitao`@`larga` | 20 | 10 075 |
| 11 | ×1.93 | 5/5/1 | 5/5/1 | — | 22 | 8 306 |
| 12 | ×2.08 | 5/5/2 | 5/5/2 | — | 24 | 11 222 |
| 13 | ×2.23 | 5/7/2 | 5/6/2 | — | 27 | 13 542 |
| 14 | ×2.40 | 7/7/2 | 6/6/3 | — | 31 | 16 589 |
| **15** | ×3.14 | 5/5/2 | 5/5/2 | `o_capitao`@`corta` + `o_capitao`@`larga` | 26 | 22 536 |
| 16 | ×3.37 | 5/7/2 | 5/6/2 | — | 27 | 20 419 |
| 17 | ×3.62 | 7/7/2 | 6/6/3 | — | 31 | 25 032 |
| 18 | ×3.89 | 6/7/4 | 6/7/4 | — | 34 | 33 802 |
| 19 | ×4.19 | 7/8/5 | 7/8/4 | — | 39 | 41 329 |
| **20** | ×5.29 | 9/10/5 | 9/10/4 | `o_capitao`@`corta` + `o_capitao`@`larga` | 49 | 69 030 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m04 -->

---

## 1. Ý đồ thiết kế

M04 là map đầu tiên bắt người chơi **chia tiền cho hai mặt trận cùng một lúc**. M03 luân phiên hai tuyến nên vẫn có thể dồn hết hoả lực rồi xoay; ở đây hai tuyến spawn cùng giây 0, nên mỗi Peso bỏ vào tuyến này là một Peso không có ở tuyến kia.

Hai tuyến **cố ý không cân**: `corta` dài 23.29 u, `larga` dài 32.47 u — tỉ lệ 0.72. Cùng số quân thì tuyến ngắn nguy hiểm hơn hẳn (quân chạm cầu môn sớm hơn 9.2 giây với `adepto`), nhưng tuyến dài lại chiếm nhiều ô hơn. Bài kiểm tra: **phân bổ KHÔNG ĐỀU là đúng, chia đôi là sai.**

Hai tuyến **cắt nhau hai lần** (khoảng cách nhỏ nhất 0.01 u). Bốn ô nằm ngay chỗ cắt (`f03`, `f05`, `f07`, `f11`) canh được cả hai tuyến — đó là toàn bộ giá trị chiến thuật của map. Ô canh-hai-tuyến có chord gấp đôi ô thường (`f03` 5.23 u vs `f01` 2.81 u ở tầm 1.4).

Điều map này DẠY mà map khác không: **một ô canh hai tuyến đáng giá hơn hai ô canh một tuyến** — vì tiền không nhân đôi được nhưng vị trí thì có.

---

## 2. Toạ độ waypoint

Nội suy `catmull-rom`, giống map hiện tại. Cả hai tuyến kết thúc ở cùng cầu môn `(0.0, -9.13)`.

### Tuyến `corta` — 10 waypoint · **23.29 u**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 1 | -3.20 | 9.36 | | 6 | -0.60 | -1.60 |
| 2 | -4.50 | 7.00 | | 7 | -2.40 | -3.60 |
| 3 | -3.40 | 4.60 | | 8 | -2.80 | -6.20 |
| 4 | -1.00 | 3.40 | | 9 | -1.20 | -8.20 |
| 5 | 0.40 | 1.00 | | 10 | 0.00 | -9.13 |

### Tuyến `larga` — 13 waypoint · **32.47 u**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 1 | 3.20 | 9.36 | | 8 | 0.00 | -1.60 |
| 2 | 4.90 | 7.60 | | 9 | 3.00 | -2.40 |
| 3 | 4.20 | 5.20 | | 10 | 4.70 | -4.40 |
| 4 | 1.60 | 4.40 | | 11 | 3.60 | -6.40 |
| 5 | -1.60 | 3.20 | | 12 | 1.60 | -7.90 |
| 6 | -3.80 | 1.60 | | 13 | 0.00 | -9.13 |
| 7 | -3.00 | -0.80 | | | | |

### Kết quả kiểm hình học

| Chỉ số | corta | larga |
|---|---|---|
| Độ dài spline | **23.29 u** | **32.47 u** |
| Hộp bao X | [-4.50, 0.41] | [-3.87, 4.94] |
| Hộp bao Y | [-9.13, 9.36] | [-9.13, 9.36] |
| Tràn viewport (x ±5.4 · y ±9.6) | KHÔNG | KHÔNG |
| Khe hở dài nhất không ô nào canh | 10 % (2.3 u) tại f=0.81→0.91 | 12 % (3.9 u) tại f=0.84→0.95 |
| Độ phủ (tầm Lv1 1.4) | 68 % | 55 % |

**Quan hệ hai tuyến:** khoảng cách nhỏ nhất **0.01 u** (cắt nhau), tổng đoạn chồng nhau (< 0.35 u) chỉ **1.90 u** — nằm ở đoạn tiếp cận cầu môn chung. Nghĩa là hai tuyến **giao nhau chứ không trùng nhau**: đọc được bằng mắt, và không có đoạn nào bị đếm hai lần khi tính phủ.

**Thời gian đi hết tuyến** (tốc gốc, chưa tính chậm):

| Loại | corta | larga | lệch |
|---|---|---|---|
| `adepto` (1.0) | 23.3 s | 32.5 s | 9.2 s |
| `tifoso` (1.4) | 16.6 s | 23.2 s | 6.6 s |
| `tambor` (0.6) | 38.8 s | 54.1 s | 15.3 s |

---

## 3. Toạ độ ô đặt tướng

12 ô sân + 1 ô thủ môn. `chord` = tổng quãng đường (cộng cả hai tuyến) nằm trong tầm.

| ô | x | y | d.corta | d.larga | chord@1.0 | chord@1.2 | chord@1.4 | tướng Lv1 với tới | tuyến canh |
|---|---|---|---|---|---|---|---|---|---|
| `f01` | -3.61 | 7.48 | 0.70 | 4.73 | 1.59 | 2.23 | 2.81 | Batigol, D10S, Árbitro, Pulga | corta |
| `f02` | -2.76 | 5.53 | 1.10 | 2.60 | 0.00 | 1.20 | 2.23 | D10S, Árbitro, Pulga | corta |
| `f03` | -1.25 | 2.65 | 0.70 | 0.65 | 3.10 | 4.21 | **5.23** | Batigol, D10S, Árbitro, Pulga | **corta+larga** |
| `f04` | 1.16 | -0.03 | 0.89 | 1.78 | 0.79 | 1.41 | 1.91 | Batigol, D10S, Árbitro, Pulga | corta |
| `f05` | -0.89 | -0.71 | 0.70 | 0.70 | 2.86 | 3.95 | **4.95** | Batigol, D10S, Árbitro, Pulga | **corta+larga** |
| `f06` | -1.92 | -4.91 | 0.88 | 3.61 | 1.05 | 1.81 | 2.44 | Batigol, D10S, Árbitro, Pulga | corta |
| `f07` | -0.20 | -7.90 | 0.89 | 1.10 | 0.98 | 2.70 | 3.54 | Batigol, D10S, Árbitro, Pulga | **corta+larga** |
| `f08` | 3.66 | 7.22 | 6.00 | 1.28 | 0.00 | 0.00 | 2.22 | Pulga | larga |
| `f09` | 3.04 | 3.86 | 3.49 | 0.85 | 1.03 | 1.65 | 2.17 | Batigol, D10S, Árbitro, Pulga | larga |
| `f10` | -2.76 | 1.23 | 2.78 | 1.10 | 0.00 | 2.04 | 3.02 | D10S, Árbitro, Pulga | larga |
| `f11` | 0.31 | -2.58 | 1.31 | 0.88 | 0.90 | 1.57 | 2.97 | Batigol, D10S, Árbitro, Pulga | **corta+larga** |
| `f12` | 3.42 | -4.57 | 4.99 | 1.26 | 0.00 | 0.00 | 2.69 | Pulga | larga |
| `gk01` | 1.20 | -8.60 | 1.31 | 0.31 | 1.89 | 2.31 | 2.80 | Dibu | corta+larga |

**Thang tầm Lv1:** Batigol 8/12 · D10S 8/12 · Árbitro 8/12 · La Pulga 12/12.
(`f02`, `f10` chỉ D10S/Árbitro/Pulga · `f08`, `f12` chỉ La Pulga.)

**Ràng buộc đã kiểm — tất cả ĐẠT:**
- Không ô chết (mọi ô sân d ≤ 1.4 = tầm Lv1 lớn nhất).
- Không ô nào rơi vào ranh giới dao cạo (|d − tầm| ≥ 0.05 với **mọi** tuyến, không chỉ tuyến gần nhất).
- Hai ô gần nhau nhất `f07`–`gk01` = 1.57 u = **57 pt** > 48 pt.
- Vùng chạm 48 pt của mọi ô nằm trọn trong khung.
- Batigol không với tới mọi ô → tầm vẫn là đánh đổi.

**Ô thủ môn:** `gk01` cách `larga` 0.31 và `corta` 1.31 — Dibu Lv1 (tầm 1.4) chặn được **cả hai** tuyến ở đoạn tiếp cận. Chord 2.80 (Lv1) → 3.78 (Lv3). Đây là ô thủ môn hiệu quả nhất trong bốn map của lô này.

---

## 4. Bảng phủ synergy

Hai ô "chồng phủ" khi vùng tầm của chúng cùng ôm một đoạn đường — điều kiện để nguồn chậm và nguồn sát thương tác động lên **cùng một con quân trong cùng khoảnh khắc**.

| cặp ô | đoạn chồng | ở tuyến | cặp tướng được thưởng |
|---|---|---|---|
| `f05` + `f11` | **1.57 u** | corta 0.83 + larga 0.73 | Árbitro (`f05`) × La Pulga Lv2 (`f11`) — +25 % · hoặc D10S Lv1 (`f05`) × mọi tướng khi D10S kia lên Lv3 |
| `f04` + `f05` | 0.85 u | corta | Árbitro (`f05`) × Batigol/Pulga (`f04`) |
| `f07` + `gk01` | tiếp giáp | corta+larga | Dibu chặn × Pulga (`f07`) dồn đòn vào quân bị kẹt |

**Ô trục: `f05`.** Đây là ô duy nhất canh cả hai tuyến VÀ chồng phủ với hai ô khác. Đặt `el_arbitro` ở `f05` là nước đi mạnh nhất map: aura chậm phủ cả hai tuyến, và thẻ vàng rút ở đây theo quân **suốt phần đường còn lại** (FM-16 — thẻ theo người, aura theo chỗ).

⚠️ **Trục synergy thứ hai — thượng nguồn/hạ nguồn.** Vì thẻ của Árbitro là vĩnh viễn, một Árbitro đặt ở `f03` (thượng nguồn, f≈0.35 của cả hai tuyến) làm mọi La Pulga Lv2 ở `f05`/`f07`/`f11` được +25 % **mà không cần chồng vùng phủ**. Ngược lại, nguồn chậm của D10S Lv1 chỉ 3 s → BẮT BUỘC phải chồng. Hai cách chơi khác nhau, cùng một bảng ô.

⚠️ **Bẫy D10S Lv3.** D10S Lv3 cho MỌI tướng +20 % lên mục tiêu đang bị chậm nhưng **tự nó không làm chậm được nữa**. Trên map này, nâng D10S ở `f05` lên Lv3 mà không có Árbitro ở đâu cả = mất luôn nguồn chậm duy nhất phủ hai tuyến → buff 20 % không bao giờ kích hoạt.

---

## 5. Bảng 20 wave

**Luật nhịp:** cả hai tuyến spawn **cùng giây 0** của wave. Nhịp spawn 0.7 s **mỗi tuyến** (tức 2 con / 0.7 s trên toàn sân). Nghỉ giữa wave 8 s. Boss ra sau cùng, đi **một mình trên tuyến được chỉ định**.

| W | corta (A/T/Tb) | larga (A/T/Tb) | tổng | HP mỗi `adepto` | HP wave | boss |
|---|---|---|---|---|---|---|
| 1 | 4/0/0 | 5/0/0 | 9 | 67 | 603 | — |
| 2 | 6/0/0 | 8/0/0 | 14 | 73 | 1 022 | — |
| 3 | 5/3/0 | 6/1/0 | 15 | 80 | 1 576 | — |
| 4 | 5/6/0 | 6/3/0 | 20 | 87 | 2 672 | — |
| 5 | 4/5/0 | 5/2/1 | 17 | 95 | 2 828 | — |
| 6 | 5/6/0 | 6/3/1 | 21 | 103 | 3 742 | — |
| 7 | 4/6/1 | 5/3/2 | 21 | 112 | 5 090 | — |
| 8 | 4/5/0 | 5/2/1 | 17 | 122 | 3 662 | — |
| 9 | 5/6/0 | 6/3/1 | 21 | 134 | 4 846 | — |
| 10 | 4/6/1 | 5/3/2 | 21 | 146 | 6 592 | **`larga`** |
| 11 | 5/6/1 | 6/4/2 | 24 | 159 | 7 851 | — |
| 12 | 5/7/1 | 6/4/3 | 26 | 173 | 9 889 | — |
| 13 | 5/9/1 | 6/5/3 | 29 | 188 | 12 023 | — |
| 14 | 6/9/1 | 8/5/5 | 34 | 205 | 15 981 | — |
| 15 | 5/7/1 | 6/4/3 | 26 | 224 | 12 807 | — |
| 16 | 5/9/1 | 6/5/3 | 29 | 244 | 15 570 | — |
| 17 | 6/9/1 | 8/5/5 | 34 | 266 | 20 696 | — |
| 18 | 6/10/1 | 8/6/6 | 37 | 290 | 25 429 | — |
| 19 | 7/12/2 | 9/6/7 | 43 | 316 | 33 217 | — |
| 20 | 9/14/2 | 11/8/8 | 52 | 344 | 42 510 | **`corta`** |

A = `adepto` · T = `tifoso` · Tb = `tambor`. Máu theo công thức chung `baseHp × 0.67 × 1.09^(w−1)`.

**Quy tắc chia tuyến (cố định cả 20 wave):**
- `corta` nhận **65 % `tifoso`** — tuyến ngắn + quân nhanh = cửa tử. Batigol (2.5 s/phát) chết ở đây.
- `larga` nhận **80 % `tambor`** — tuyến dài + quân chậm = cần sát thương dồn, thưởng cho AoE và cho Árbitro.
- `adepto` chia 45/55 nghiêng về `larga`.

**Boss `o_capitao` — kháng chậm 80 %** (cao hơn 75 % của map gốc). Cap chậm thực tế = 70 % × 20 % = **14 %**.

| | wave | tuyến | HP đề xuất | thưởng | lý do |
|---|---|---|---|---|---|
| Boss 1 | 10 | `larga` (32.47 u) | **1 900** | 200 | Tuyến dài, phơi nhiễm lâu → boss dạy, không giết |
| Boss 2 | 20 | `corta` (23.29 u) | **5 400** | 500 | Tuyến NGẮN. Ai bỏ bê `corta` suốt act 3 thua ở đây |

> HP boss suy ra từ tỉ lệ độ dài tuyến so với map gốc (42.28 u → 2 500 / 9 800). **Đây là ước lượng bậc nhất, PHẢI đo lại bằng test 20 wave chạy engine thật.**

---

## 6. Kinh tế đề xuất

| Mục | Giá trị |
|---|---|
| `startingCash` | **700** |
| `goalHealth` | 20 |
| Số ô sân | 12 |
| **Trần chi tiêu** = 12 × 1 020 + 476 | **12 716** |
| Tiền cả đời một trận | **12 285** |
| Biên | **+3.4 %** ✅ |

Phân rã tiền cả đời: `startingCash` 700 + thưởng hạ quân 9 020 + thưởng clear wave 1 385 + thưởng skip (24 × 20) 480 + thưởng boss 700.

Giá tướng giữ nguyên `towers.json` (Lv2 = 0.8 × giá mua, Lv3 = 1.6 × — luật validator #3). Đội hình đắt nhất vẫn là 12 × La Pulga Lv3 (1 020) + Dibu Lv3 (476).

⚠️ **Headroom [1.05, 1.65] chưa đo.** `tools/balance_sim.py` **không dùng được** cho map này: nó đã báo sai trên map một tuyến (nói headroom trung bình 2.09 trong khi engine thật thắng còn 11/20 máu), và nó **không có khái niệm nhiều tuyến** — mô hình hoả lực tập trung của `docs/04 §3.3` giả định một con đường duy nhất. Số headroom cho M04 phải lấy từ **test 20 wave chạy `MatchController` thật**, mỗi build một lần.

**Các đích cần đo (không phải kết luận):**
1. Headroom mỗi wave trong [1.05, 1.65], không wave nào thủng sàn.
2. Máu cầu môn cuối trận của người chơi giỏi: 10–19 (⭐⭐).
3. Dư tiền tối đa tại bất kỳ thời điểm nào < 400 Peso (tiền phải luôn khan).
4. Boss W20 trên `corta`: build đám-đông thua (< 1.0), build có-tính-boss thắng (1.2–1.5).

---

## 7. Đường thắng dự kiến & build cố ý thua

### Đường thắng (bám sát ngân sách)

| Giai đoạn | Nước đi | Lý do |
|---|---|---|
| Kickoff (700) | Batigol `f01` (120) + Batigol `f09` (120) + Batigol `f05` (120) | Ba tuyến-vào rẻ nhất; `f05` canh cả hai tuyến ngay từ đầu |
| W3–W6 | Nâng `f05` → Lv2 ; thêm D10S `f03` (240) | `f03` chord 5.23 — ô AoE tốt nhất map |
| W7–W10 | El Árbitro `f05` (bán Batigol, hoàn 60 %) ; La Pulga `f11` (300) | `f05`+`f11` chồng phủ 1.57 u → Pulga Lv2 ăn +25 % |
| W11–W14 | Nâng `f11` → Lv2, `f03` → Lv2 (bán kính lan 1.7) ; thêm Batigol `f06`, `f07` | Bịt khe hở f=0.81→0.91 của `corta` |
| W15–W18 | **Bán `f08` và `f12`** (chord chỉ 2.2/2.7, chỉ Pulga dùng được) → dồn vào La Pulga Lv3 ở `f03` và `f07` | Ô xa là ô đầu tư kém nhất khi tiền cạn |
| W19–W20 | Dibu Lv2/Lv3 `gk01` ; giữ ≥ 2 nguồn đơn mục tiêu trên `corta` | Boss W20 đi `corta` một mình |

### Build CỐ Ý thua

**"Tất tay AoE trên tuyến dài"** — 5 × D10S ở `f08`, `f09`, `f10`, `f11`, `f12` + Batigol lẻ trên `corta`.

Vì sao thua, theo thứ tự:
1. `f08` và `f12` chỉ La Pulga Lv1 với tới (d 1.28 / 1.26) — **D10S đặt ở đó không bắn được gì cho tới khi lên Lv2**. Người chơi mất 2 lượt wave mới nhận ra.
2. `corta` chỉ có 4 ô được canh → 65 % `tifoso` của map đi qua đây gần như tự do từ W13.
3. Không có nguồn chậm nào phủ `corta` → La Pulga Lv2 (+25 % lên mục tiêu bị chậm) không bao giờ kích hoạt.
4. Boss W20 spawn trên `corta`, kháng chậm 80 %, 5 400 máu, đi một mình: AoE của D10S ở tuyến bên kia **không chạm tới**. Trừ 5 máu cầu môn, và W20 còn 51 con quân thường phía sau.

---

## 8. Yêu cầu kỹ thuật

M04 **không chạy được trên kiến trúc hiện tại**. `MatchController` chỉ giữ MỘT `EnemyPath` (`MatchController.cs:30`, khởi tạo ở `:111`), `Enemy` không có trường tuyến, `ScheduledSpawn` cũng không (`WaveSpawner.cs:9-23`).

### 8.1 Schema `config/path.json`

```jsonc
{
  "paths": [                                  // THAY cho "path" đơn
    { "id": "corta", "interpolation": "catmull-rom", "lengthUnits": 23.29,
      "spawnPoint": {"x": -3.2, "y": 9.36}, "goalPoint": {"x": 0.0, "y": -9.13},
      "waypoints": [ /* 10 điểm ở §2 */ ] },
    { "id": "larga", "interpolation": "catmull-rom", "lengthUnits": 32.47,
      "spawnPoint": {"x": 3.2, "y": 9.36},  "goalPoint": {"x": 0.0, "y": -9.13},
      "waypoints": [ /* 13 điểm ở §2 */ ] }
  ],
  "slots": [ /* như §3, KHÔNG đổi shape */ ]
}
```

### 8.2 Schema `config/waves.json`

```jsonc
{ "wave": 10,
  "spawns": { "corta": {"adepto": 4, "tifoso": 6, "tambor": 1},
              "larga": {"adepto": 5, "tifoso": 3, "tambor": 2} },
  "laneOffsetsSec": { "corta": 0.0, "larga": 0.0 },   // M04: LUÔN 0 cho cả hai
  "boss": { "id": "o_capitao", "lane": "larga" } }
```

### 8.3 Code phải đổi

| File:dòng | Đổi gì |
|---|---|
| `ConfigModel.cs:184` | `PathConfig.Waypoints` → danh sách `PathDef` có `Id` |
| `ConfigModel.cs:111-122` | `WaveDef.Spawns` từ `IReadOnlyDictionary<string,int>` → `IReadOnlyDictionary<string, IReadOnlyDictionary<string,int>>` (tuyến → loại → số); `TotalCount` cộng lồng hai tầng |
| `WaveSpawner.cs:9-23` | `ScheduledSpawn` thêm `string LaneId` |
| `WaveSpawner.Schedule()` | Sinh lịch **mỗi tuyến**, cộng `laneOffsetsSec`, rồi trộn theo `TimeSec`. `InterleaveOrder` chạy trong phạm vi một tuyến |
| `Enemy.cs` (constructor) | Thêm `string LaneId` chỉ-đọc |
| `MatchController.cs:30,111` | `_path` → `Dictionary<string, EnemyPath> _paths` |
| `MatchController.cs:243` | `Position = _paths[s.LaneId].Spawn` |
| `MatchController.cs:259,264` | `_paths[e.LaneId].Length` / `.PositionAt(...)` |
| `ConfigValidator.cs` | Luật mới: (a) mọi `laneId` trong `waves` phải tồn tại trong `paths`; (b) `boss.lane` phải tồn tại; (c) mọi ô sân phải cách **ít nhất một** tuyến ≤ tầm Lv1 lớn nhất |
| `Pitch.cs:98` `Road(EnemyPath, ...)` | Gọi N lần, mỗi tuyến một màu/hoạ tiết khác để người chơi phân biệt |
| `core/.../PathConfigTests.cs:17-23` | Bất biến `14 waypoint / 12 ô` chốt cứng — phải chuyển thành kiểm theo từng map |
| `tools/path_check.py` | Chưa biết đa tuyến. Cần: d tới **từng** tuyến, ranh-giới-dao-cạo kiểm trên mọi tuyến (không chỉ tuyến gần nhất), khe hở tính riêng mỗi tuyến |

### 8.4 Điều KHÔNG cần đổi
- `TargetingSystem`, `CombatResolver`, `DamageSystem`, `SlowStack`, `AbilityEngine` — chúng làm việc trên toạ độ, không quan tâm tuyến.
- `SlotManager` — chỉ cần `economy.fieldSlots` = 12.
- Cầu môn: hai tuyến chung một `goalPoint`, nên `GoalHealth` không đổi.

---

## 9. Rủi ro

| # | Rủi ro | Mức | Giảm thiểu |
|---|---|---|---|
| R1 | **Nhịp spawn thực gấp đôi** (2 con / 0.7 s). Số quân trên sân cùng lúc cao hơn map gốc ~1.6× → có thể thủng ngân sách frame trên máy yếu | CAO | Đo `Enemies.Count` đỉnh ở W19–W20 trên máy thật trước khi chốt bảng wave |
| R2 | **Không đo được bằng `balance_sim.py`.** Công cụ giả định một đường. Chốt số bằng nó = chốt sai | CAO | Chỉ chấp nhận số từ test 20 wave chạy `MatchController` |
| R3 | Độ phủ `larga` chỉ 55 % (map gốc 68 %). Map đa tuyến **về cấu trúc là rò rỉ hơn** với cùng số ô | TRUNG BÌNH | Chấp nhận có chủ đích; bù bằng `goalHealth` 20 và bảng wave nhỏ hơn. Nếu đo ra quá dễ rò → cắt `tifoso` ở `corta` trước, đừng thêm ô |
| R4 | Hai tuyến cắt nhau → người chơi khó đọc con nào đi đâu, tưởng game lỗi | TRUNG BÌNH | Hai màu đường khác nhau + màu viền quân theo tuyến (`Pitch.cs`) |
| R5 | HP boss 1 900 / 5 400 mới là ngoại suy theo độ dài tuyến, chưa phải suy ra từ mô hình hoả lực tập trung | TRUNG BÌNH | Đo trước khi khoá; nếu build đám-đông vẫn thắng W20 thì nâng HP, **đừng** nâng kháng chậm quá 80 % |
| R6 | `f08` và `f12` chỉ La Pulga Lv1 với tới, chord thấp (2.2 / 2.7) → dễ thành nội dung chết | THẤP | Cố ý — đó là ô "bán lại ở act 3". Nếu đo thấy chưa ai từng mua thì kéo d về 1.10 |
