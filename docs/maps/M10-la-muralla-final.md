# M10 — La Muralla Final ★★★★★

> **3 tuyến · 12 ô sân + 1 ô thủ môn · boss kháng chậm 90% · HAI BOSS CÙNG LÚC ở W20**
> Toạ độ đã kiểm bằng bản copy `tools/path_check.py` (đa tuyến) trong thư mục nháp.
> Mọi con số cân bằng là **ĐÍCH CẦN ĐO** bằng test 20-wave chạy engine thật.
> ⚠️ Không dùng `tools/balance_sim.py` để chốt số.

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m10 -->
**Nguồn: `config/maps/m10-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 3 — `O` 21.0u · `C` 24.5u · `E` 21.0u |
| Ô sân / thủ môn | 17 / 2 |
| Σchord@1.4 mỗi ô | 2.88 |
| Tiền khởi đầu | 800 |
| Máu quái | ×0.67 × 1.07^(W−1) → W20 ×2.42 |
| Tổng quân 20 wave | 314 |

| W | ×máu | `O` | `C` | `E` | boss | quân | tổng máu |
|---|------|------|------|------|------|------|----------|
| 1 | ×0.67 | 8/0/0 | — | — | — | 8 | 536 |
| 2 | ×0.72 | — | 13/0/0 | — | — | 13 | 936 |
| 3 | ×0.77 | — | — | 10/4/0 | — | 14 | 1 446 |
| 4 | ×0.82 | 12/6/0 | — | — | — | 18 | 2 070 |
| 5 | ×0.88 | — | 0/10/1 | — | — | 11 | 2 413 |
| 6 | ×0.94 | — | — | 1/12/2 | — | 15 | 3 612 |
| 7 | ×1.01 | 0/6/1 | 1/6/1 | — | — | 15 | 3 859 |
| 8 | ×1.08 | — | 0/4/1 | 0/5/1 | — | 11 | 3 317 |
| 9 | ×1.15 | 0/6/1 | — | 1/6/1 | — | 15 | 4 417 |
| **10** | ×1.23 | 0/5/2 | 0/5/1 | — | `o_capitao`@`C` | 14 | 7 241 |
| 11 | ×1.32 | — | 0/5/2 | 1/4/3 | — | 15 | 6 367 |
| 12 | ×1.41 | 1/6/2 | — | 0/7/1 | — | 17 | 6 499 |
| 13 | ×1.51 | 0/8/2 | 0/8/2 | — | — | 20 | 8 632 |
| 14 | ×1.61 | — | 0/9/2 | 0/9/2 | — | 22 | 9 942 |
| 15 | ×1.73 | 1/4/1 | 0/5/1 | 0/4/1 | — | 17 | 7 963 |
| 16 | ×1.85 | 0/5/1 | 1/5/1 | 0/6/1 | — | 20 | 9 748 |
| 17 | ×1.98 | 0/6/1 | 0/6/2 | 1/6/1 | — | 23 | 12 380 |
| 18 | ×2.12 | 1/7/1 | 0/7/1 | 0/7/1 | — | 25 | 13 490 |
| 19 | ×2.26 | 0/5/2 | 0/5/2 | 0/5/3 | — | 22 | 16 192 |
| **20** | ×2.42 | — | — | — | `o_capitao`@`O` + `o_capitao`@`E` | 2 | 19 600 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m10 -->

---

## 1. Ý đồ thiết kế

Map cuối là **bài kiểm tổng hợp**: nó hỏi lại đúng ba thứ mà M01–M09 đã dạy, cùng lúc, trong một trận.

- **Ô ổ khoá / pocket** (từ M08): ba ô `f10` `f11` `f12` canh nhiều hơn một tuyến; `f12` canh **cả ba**. Nhưng chỉ **tuyến giữa** có cặp ô chồng lấn tầm — hai tuyến biên phải giải bằng sát thương thô.
- **Phân tuyến** (từ M08/M09): ba tuyến mở dần — W1–3 chỉ tuyến giữa, W4 mở tuyến Tây, W7 mở tuyến Đông. Ai xây lệch một bên vỡ ở W7, không phải W20.
- **Build chống boss** (từ mọi map trước): W20 thả **hai `O Capitão` trên hai tuyến khác nhau**, lệch 5 giây. Dibu chỉ cản được **một**. Không có đội hình nào phủ cả hai tuyến biên cùng lúc bằng một cụm — phải xây **hai cụm chống boss riêng**, và ngân sách chỉ vừa đủ nếu không tiêu phí.

Kháng chậm 90% là mức cao nhất trong game: chậm thực tế tối đa **7%**. Boss ở đây gần như miễn nhiễm cơ chế câu giờ.

---

## 2. Toạ độ waypoint

Ba tuyến, ba cửa vào, **cùng một** `goalPoint` `(0.00, -9.13)`. Catmull-Rom.

### Tuyến O — "Oeste" (22.51 u)

| # | x | y |  | # | x | y |
|---|---|---|---|---|---|---|
| 1 | -4.20 | 9.36 | (spawn O) | 6 | -3.20 | -1.40 |
| 2 | -4.70 | 7.20 | | 7 | -3.40 | -4.00 |
| 3 | -2.00 | 5.60 | | 8 | -2.20 | -5.40 |
| 4 | -3.90 | 3.40 | | 9 | -0.90 | -7.60 |
| 5 | -4.40 | 0.80 | | 10 | 0.00 | -9.13 | (goal) |

### Tuyến C — "Centro" (20.43 u — ngắn nhất, tuyến tốc hành)

| # | x | y |  | # | x | y |
|---|---|---|---|---|---|---|
| 1 | 0.00 | 9.36 | (spawn C) | 5 | -0.50 | 0.40 |
| 2 | 0.70 | 7.40 | | 6 | -0.60 | -2.20 |
| 3 | 0.40 | 5.00 | | 7 | 0.70 | -4.40 |
| 4 | 1.60 | 2.60 | | 8 | 0.10 | -6.40 |
| | | | | 9 | 0.00 | -9.13 | (goal) |

### Tuyến E — "Este" (21.17 u)

| # | x | y |  | # | x | y |
|---|---|---|---|---|---|---|
| 1 | 4.20 | 9.36 | (spawn E) | 6 | 2.10 | -2.00 |
| 2 | 4.70 | 7.20 | | 7 | 3.30 | -4.20 |
| 3 | 3.40 | 5.20 | | 8 | 2.20 | -5.40 |
| 4 | 4.30 | 2.60 | | 9 | 0.90 | -7.60 |
| 5 | 3.00 | 0.20 | | 10 | 0.00 | -9.13 | (goal) |

### Kết quả đo

| Tuyến | Độ dài | Hộp bao x | Hộp bao y | Tràn viewport |
|-------|--------|-----------|-----------|----------------|
| O (Oeste) | **22.51 u** | [-4.80, 0.00] | [-9.13, 9.36] | ❌ không |
| C (Centro) | **20.43 u** | [-0.76, 1.61] | [-9.13, 9.36] | ❌ không |
| E (Este) | **21.17 u** | [0.00, 4.72] | [-9.13, 9.36] | ❌ không |

Viewport `x ∈ [-5.40, 5.40]`. Lề ngang nhỏ nhất **0.60 u** (tuyến O tại x = −4.80).

**Điểm chụm có chủ đích** — ba chỗ hai tuyến áp sát nhau, mỗi chỗ nuôi đúng một ô pocket:

| Chụm | Vị trí | Khoảng cách 2 tuyến | Ô ăn theo |
|------|--------|---------------------|-----------|
| O × C | quanh y ≈ +4.8 | ~2.6 u | `f10` (d 1.30 / 1.29) |
| C × E | quanh y ≈ −2.1 | ~2.6 u | `f11` (d 1.30 / 1.30) |
| O × C × E | quanh y ≈ −8.8 (sát cầu môn) | ~2.5 u trải ba | `f12` + `gk01` |

---

## 3. Toạ độ ô

| id | type | x | y | d[O] | d[C] | d[E] | Tướng với tới ở **Lv1** | Vai trò |
|----|------|------|------|------|------|------|--------------------------|---------|
| `f01` | field | -3.64 | 7.51 | **0.88** | 4.07 | 7.39 | El Cinco · D10S · Árbitro · Pulga | mở màn O |
| `f02` | field | -3.09 | 2.52 | **1.09** | 3.32 | 6.46 | D10S · Árbitro · Pulga | giữa O |
| `f03` | field | -1.87 | -1.63 | **1.30** | **1.15** | 3.97 | D10S · Árbitro · Pulga | O + C |
| `f04` | field | -0.33 | 6.46 | 1.87 | **0.88** | 3.92 | El Cinco · D10S · Árbitro · Pulga | mở màn C |
| `f05` | field | 0.60 | 0.07 | 4.06 | **1.09** | 2.04 | D10S · Árbitro · Pulga | giữa C |
| `f06` | field | -0.89 | -4.71 | **1.47** | **1.53** | 2.99 | — | ⛰️ **ô tầm xa** (R ≥ 1.6), canh O + C |
| `f07` | field | 3.67 | 7.14 | 5.87 | 2.97 | **0.88** | El Cinco · D10S · Árbitro · Pulga | mở màn E |
| `f08` | field | 3.20 | 2.68 | 5.87 | 1.59 | **1.10** | D10S · Árbitro · Pulga | giữa E |
| `f09` | field | 1.06 | -4.76 | 3.11 | **0.36** | **1.30** | El Cinco · D10S · Árbitro · Pulga | C + E |
| `f10` | field | -0.87 | 4.75 | **1.30** | **1.29** | 4.28 | **Pulga** | 🔑 pocket **O × C** |
| `f11` | field | 0.81 | -2.14 | 3.99 | **1.30** | **1.30** | **Pulga** | 🔑 pocket **C × E** |
| `f12` | field | 1.25 | -8.80 | **1.29** | **1.24** | **0.91** | El Cinco · D10S · Árbitro · Pulga | 🔑🔑 **El Último Hombre** — canh **CẢ BA** tuyến |
| `gk01` | goalkeeper | -1.25 | -8.80 | **0.91** | **1.26** | **1.29** | Dibu (mọi cấp) | canh cả ba tuyến ngay Lv1 |

### Thang tầm

| Tướng | Tầm Lv1 | Số ô với tới |
|-------|---------|--------------|
| El Cinco | 1.0 | **5/12** (`f01` `f04` `f07` `f09` `f12`) |
| D10S | 1.2 | **9/12** (+ `f02` `f03` `f05` `f08`) |
| El Árbitro | 1.2 | **9/12** |
| La Pulga | 1.4 | **11/12** (+ `f10` `f11`) |
| — không tướng Lv1 nào | | **1/12**: `f06` (1.47/1.53) |

🔴 **Vì sao map cuối lại "dễ tầm" hơn M08?** Vì bài kiểm ở đây **không phải tầm** — mà là **phân bổ**. Cho hầu hết ô dùng được ngay Lv1 để bài toán thật (ba tuyến, hai boss, một ví) không bị che bởi bài toán phụ (ô câm). `f12` cố tình cho **mọi tướng Lv1 với tới cả ba tuyến** — nó là ô hấp dẫn nhất map và cũng là **cái bẫy**: nó chỉ canh 4% cuối cùng của đường, quân tới đó là đã sát lưới.

### Ràng buộc vật lý (đã kiểm)

| Ràng buộc | Kết quả |
|-----------|---------|
| Ô chết không khai `farSlot` | 0 (chỉ `f06` khai `farSlot: true`) |
| Ranh giới dao cạo | 0 vi phạm |
| Cặp ô gần nhau nhất | `f04`–`f10` = 1.79 u = **65 pt** ≥ 48 pt ✅ |
| Vùng chạm tràn mép khung | 0 vi phạm (`f12`/`gk01` tại y = −8.80: 8.80 + 0.66 = 9.46 < 9.60) |
| chord thật / 2R | R=1.4: tb **0.82** · R=1.8: tb **1.20** |

**Đoạn hở theo tuyến** (luật 0.15 không áp dụng — xem §8):

| Tuyến | Đoạn hở dài nhất | Vị trí |
|-------|------------------|--------|
| O | **22%** (4.9 u) | f 0.40 → 0.62 |
| C | **18%** (3.8 u) | f 0.81 → 0.98 |
| E | **24%** (5.1 u) | f 0.35 → 0.60 |

Với 12 ô chia cho 3 tuyến (≈ 4–6 ô canh mỗi tuyến), sàn toán học của đoạn hở là ~0.20. **Không có cách xếp nào đạt 0.15.**

---

## 4. Bảng phủ synergy

### 4a. Chồng lấn hình học

| Ô làm chậm | Ô sát thương | Tuyến | Giao R 1.4 × 1.6 | Giao R 1.6 × 1.8 |
|------------|--------------|-------|------------------|------------------|
| `f09` | `f06` | C | **1.19 u** | **2.90 u** |
| `f03` | `f11` | C | 0.57 u | 1.24 u |
| `f06` | `f09` | C | 0.00 | 1.19 u |
| `f11` | `f03` | C | 0.23 u | 1.07 u |
| `f04` | `f10` | C | 0.46 u | 0.95 u |
| `f10` | `f04` | C | 0.37 u | 0.88 u |
| `f11` | `f06` | C | 0.00 | 0.73 u |
| `f03` | `f05` | C | 0.00 | 0.60 u |
| `f09` | `f11` | C | 0.00 | 0.58 u |
| *mọi cặp trên tuyến **O** hoặc **E*** | | | **0.00** | **0.00** |

🔴 **Phát hiện:** **toàn bộ** chồng lấn của map nằm trên **tuyến C**. Hai tuyến biên có ô rải quá thưa (4–5 ô trên 21–22 u) nên vòng tầm không bao giờ chạm nhau. Hệ quả thiết kế — và nó tự nhất quán với chủ đề map:

> **Tuyến giữa là tuyến "kỹ thuật"** (đặt ổ khoá làm chậm × sát thương, ăn combo). **Hai tuyến biên là tuyến "sức mạnh"** (không combo, phải trả bằng Pulga Lv3). Và **boss W20 ra ở hai tuyến biên** — tức bài kiểm cuối cố ý **cấm** dùng combo.

Ổ khoá rẻ nhất: `f09` (làm chậm, El Cinco/D10S/Árbitro Lv1 đều với tới d 0.36) × `f06` (sát thương, cần R ≥ 1.6) — giao **2.90 u** ở tầm Lv3, dài nhất trong cả ba map.

### 4b. Thẻ vĩnh viễn (Árbitro)

| Ô đặt Árbitro | Canh được (Lv3, R=1.6) | % quãng đường SAU ô — O / C / E |
|---------------|------------------------|----------------------------------|
| `f04` | C | 76% / **84%** / 79% |
| `f07` | E | 76% / 89% / **86%** |
| `f01` | O | **86%** / 99% / 79% |
| `f10` | O + C | 74% / 78% / 77% |
| `f08` | C + E | 74% / 65% / 65% |
| `f03` | O + C | 38% / 40% / 41% |
| `f11` | C + E | 37% / 33% / 40% |
| `f12` | **O + C + E** | 0% / 2% / 4% ⚠️ vô dụng cho thẻ |

**Bố trí khuyến nghị:** một Árbitro ở `f10` (canh O + C, 74–78% đường nằm sau) là khoản đầu tư một-ô-hai-tuyến tốt nhất map. `f12` tuy canh cả ba tuyến nhưng nằm ở **cuối đường** — dán thẻ ở đó không còn ô nào phía sau để hưởng. Đây là cái bẫy đối xứng với §3.

---

## 5. Bảng 20 wave

`hp = baseHp × 0.67 × 1.09^(wave-1)` · spawn cách 0.7s · nghỉ 8s.
Cột ghi **(O/C/E)**. Tuyến mở dần: **W1–3 chỉ C · W4 mở O · W7 mở E**.

| W | Adepto (O/C/E) | Tifoso (O/C/E) | Tambor (O/C/E) | Boss | Quân | Tổng máu | Thưởng hạ | Clear | Tiền wave | Ví cộng dồn |
|---|----------------|----------------|----------------|------|------|----------|-----------|-------|-----------|-------------|
| 1 | 8 (0/8/0) | — | — | — | 8 | 536 | 40 | 20 | 60 | 760 |
| 2 | 12 (0/12/0) | — | — | — | 12 | 876 | 60 | 25 | 85 | 845 |
| 3 | 10 (0/10/0) | 4 (0/4/0) | — | — | 14 | 1 500 | 94 | 30 | 124 | 969 |
| 4 | 12 (6/6/0) | 6 (3/3/0) | — | — | 18 | 2 190 | 126 | 35 | 161 | 1 130 |
| 5 | 12 (6/6/0) | 8 (4/4/0) | 1 (1/0/0) | — | 21 | 3 324 | 172 | 40 | 212 | 1 342 |
| 6 | 12 (6/6/0) | 10 (5/5/0) | 2 (1/1/0) | — | 24 | 4 640 | 218 | 45 | 263 | 1 605 |
| 7 | 12 (4/4/4) | 10 (3/4/3) | 3 (1/1/1) | — | 25 | 5 668 | 242 | 50 | 292 | 1 897 |
| 8 | 12 (4/4/4) | 9 (3/3/3) | 2 (1/0/1) | — | 23 | 5 233 | 287 | 55 | 342 | 2 239 |
| 9 | 14 (5/5/4) | 11 (4/4/3) | 3 (1/1/1) | — | 28 | 7 312 | 365 | 60 | 425 | 2 664 |
| **10** | 12 (4/4/4) | 11 (4/3/4) | 3 (1/1/1) | **1 → C** | 27 | **11 072** | 351 + **200** | 65 | 616 | 3 280 |
| 11 | 15 (5/5/5) | 12 (4/4/4) | 3 (1/1/1) | — | 30 | 9 189 | 387 | 70 | 457 | 3 737 |
| 12 | 15 (5/5/5) | 14 (5/4/5) | 4 (1/2/1) | — | 33 | 11 719 | 451 | 75 | 526 | 4 263 |
| 13 | 15 (5/5/5) | 17 (6/5/6) | 4 (1/2/1) | — | 36 | 14 019 | 496 | 80 | 576 | 4 839 |
| 14 | 18 (6/6/6) | 16 (5/6/5) | 6 (2/2/2) | — | 40 | 17 702 | 570 | 85 | 655 | 5 494 |
| 15 | 15 (5/5/5) | 14 (5/4/5) | 5 (2/1/2) | — | 34 | 16 417 | 635 | 90 | 725 | 6 219 |
| 16 | 16 (5/6/5) | 17 (6/5/6) | 5 (2/1/2) | — | 38 | 19 743 | 704 | 95 | 799 | 7 018 |
| 17 | 18 (6/6/6) | 18 (6/6/6) | 6 (2/2/2) | — | 42 | 24 096 | 786 | 100 | 886 | 7 904 |
| 18 | 18 (6/6/6) | 20 (7/6/7) | 7 (2/3/2) | — | 45 | 29 145 | 870 | 105 | 975 | 8 879 |
| 19 | 21 (7/7/7) | 21 (7/7/7) | 9 (3/3/3) | — | 51 | 36 873 | 1 005 | 110 | 1 115 | 9 994 |
| **20** | 24 (8/8/8) | 24 (8/8/8) | 12 (4/4/4) | **2 → O + E** | 62 | **62 988** | 1 224 + **600** | 150 | 1 974 | **11 968** |

**Tổng:** 611 quân · 284 242 máu.

### Boss

| | W10 (một con) | W20 (**hai con**) |
|---|---------------|-------------------|
| Máu | **3 400** | **6 900 mỗi con** (13 800 tổng) |
| **Cùng máu?** | — | ✅ **CÓ — hai con máu bằng nhau.** |
| Tuyến | **C (Centro)** | **con 1 → O · con 2 → E** |
| **Ra cùng lúc?** | — | ❌ **Lệch 5 giây.** O ra trước, E ra sau 5 s. |
| Thưởng | 200 | 300 + 300 |
| Tốc độ | 0.4 | 0.4 |
| **Kháng chậm** | **90%** | **90%** |
| Trừ máu khi lọt | 5 | **5 mỗi con — lọt cả hai = 10 máu** |

**Vì sao máu bằng nhau và lệch 5 giây:**
- **Bằng nhau** → không có "con dễ" để dồn hết vào. Hai tuyến biên có tổng chord gần bằng nhau (14.28 u vs 14.20 u), nên hai cụm chống boss phải **cùng cỡ**. Nếu máu lệch, người chơi chỉ cần xây một cụm to và bỏ mặc con nhỏ.
- **Lệch 5 giây** (≈ 1.9 u đường boss) → đủ để ô chung `f12` và kỹ năng `Cản Phá` của Dibu **không** phục vụ được cả hai như nhau, nhưng chưa đủ để biến thành hai trận đánh nối tiếp. Nếu lệch 0 s thì `f12` chia mục tiêu ngẫu nhiên và kết quả phụ thuộc thứ tự nhắm mục tiêu — không đọc được. Nếu lệch > 15 s thì đây chỉ là hai boss lần lượt, mất hoàn toàn ý nghĩa "cùng lúc".

Kháng 90% → chậm thực tế tối đa `70% × 0.10` = **7%** → tốc 0.372 u/s. Câu giờ gần như không tồn tại.

**Dẫn xuất máu (không bịa):** tổng chord thật ở R=1.8 — tuyến O **14.28 u**, tuyến E **14.20 u** — chia cho 0.372 → **38.4 / 38.2 tower-giây**. Chuẩn map gốc 237.5 máu/tower-giây cho 9 118 / 9 065; hạ xuống **6 900** để đội hình chống-boss đạt headroom ≈ 1.13–1.14 (§7a). Boss W10 = 3 400 trên tuyến C (chord 23.50 u → 63.2 tower-giây) → headroom 1.45 với **Pulga Lv2**, mức mà ví ở W10 (3 280 Peso) vừa mua nổi.

**Nhịp dạy chơi:** W1–3 chỉ tuyến C — người chơi tưởng đây là map một tuyến. **W4 mở tuyến O** đúng lúc ví vừa đủ 1 130. **W7 mở tuyến E** cùng lúc Tambor lần đầu ra đủ ba tuyến. W10 boss ra tuyến **C** (tuyến có combo) — đây là boss "dễ", dạy rằng combo diệt boss được. **W20 boss ra hai tuyến biên** — nơi combo **không tồn tại** (§4a). Đó là cú lật của map cuối.

---

## 6. Kinh tế đề xuất

| Tham số | Giá trị |
|---------|---------|
| `startingCash` | **700** |
| `goalHealth` | 20 |
| `bountyMultiplier` theo act | **1.00 / 1.40 / 1.85** (map gốc: 1.0 / 1.6 / 2.2) |
| Clear bonus | 15 + 5×W (W20 = 150) |
| `skipBonusPerSecond` | 3 · `sellRefundRatio` 0.6 |

### Trần chi tiêu

```
trần   = 12 × 1020 + 476 = 12 716
tiền cả trận (700 + thưởng 9 883 + clear 1 385) = 11 968
biên   = +748  (+5.9 %)
tiền cả trận + skip tối đa (456)                = 12 424  <  12 716  ✅
```

🔴 **Điểm căng thật sự không phải trần, mà là ngân sách chống boss.** Để có headroom > 1.0 lên **cả hai** boss W20 cần Pulga Lv3 ở 5 ô tuyến O (`f01` `f02` `f03` `f06` `f10`) **và** 4 ô tuyến E (`f07` `f08` `f09` `f11`) **và** `f12` = **10 × 1 020 = 10 200 Peso**. Tiền cả trận là 11 968. Còn lại **1 768** cho: Dibu (476), toàn bộ phòng ngự tuyến C 19 wave đầu, và mọi sai lầm. **Không thể** làm được nếu không **bán và xây lại** ở Act 3 — đúng ý đồ của act 3 trong `docs/03`.

⚠️ **Cần ĐO bằng engine thật**: headroom mỗi wave trong **[1.05, 1.65]**. Điểm nghi ngờ: **W7** (mở tuyến thứ ba với ví 1 897) và **W20**. Nếu W7 vỡ sàn: dời việc mở tuyến E sang W8 và bù 2 Adepto vào tuyến C ở W7.

---

## 7. Đường thắng dự kiến & build cố ý thua

### 7a. Bảng đối chứng boss W20 (6 900 máu **mỗi con**, kháng chậm 90%)

Mọi ô canh được tuyến đó, cùng tướng cùng cấp:

| Đội hình | Tầm | Tuyến O: chord → dame → **headroom** | Tuyến E: chord → dame → **headroom** |
|----------|-----|---------------------------------------|---------------------------------------|
| **Pulga Lv3** (204 dps) | 1.8 | 14.28 u → 7 832 → **1.14** ✅ | 14.20 u → 7 786 → **1.13** ✅ |
| Pulga Lv2 (108 dps) | 1.6 | 11.09 u → 3 218 → 0.47 ❌ | 11.86 u → 3 442 → 0.50 ❌ |
| Pulga Lv1 (54 dps) | 1.4 | 7.14 u → 1 036 → 0.15 ❌ | 9.02 u → 1 309 → 0.19 ❌ |
| D10S Lv3 (60 dps) | 1.6 | 11.09 u → 1 788 → 0.26 ❌ | 11.86 u → 1 912 → 0.28 ❌ |
| D10S Lv2 (38.4 dps) | 1.4 | 7.14 u → 737 → 0.11 ❌ | 9.02 u → 931 → 0.13 ❌ |
| El Cinco Lv3 (27 dps) | 1.4 | 7.14 u → 518 → 0.08 ❌ | 9.02 u → 655 → 0.09 ❌ |

⚠️ **Hiệu chỉnh cho hai boss:** `f12` được tính trong **cả hai** cột, nhưng một tướng chỉ bắn **một** mục tiêu tại một thời điểm. Với độ lệch 5 giây, `f12` phục vụ boss O trước rồi boss E. Trừ phần đóng góp của `f12` khỏi một trong hai cột thì headroom con đó tụt xuống **≈ 1.02–1.05**. **Đây là biên thắng thật của map, và nó phải được ĐO, không suy luận.**

Boss W10 (3 400 máu, tuyến C): Pulga **Lv2** = 4 920 → headroom **1.45** ✅ — vừa tay ở W10.

### 7b. Build CỐ Ý THUA

**(1) "Đám đông"** — 10 × El Cinco Lv3 + 2 × D10S Lv2, ~4 500 Peso. Dọn 62 quân của W20 rất tốt (nổ lan 1.4 × 3 tuyến), nhưng lên boss chỉ đạt **0.08–0.09** mỗi con → **cả hai lọt = −10 máu**, thua ngay nếu đã mất ≥ 11 máu trước đó.

**(2) "Pháo đài Último Hombre"** — dồn tiền vào `f12` + `gk01` + `f11` (ba ô canh nhiều tuyến nhất), Pulga Lv3 hết. Nghe hợp lý vì chúng canh cả ba tuyến. Thực tế `f12` chỉ canh **4% cuối** đường (f 0.96–1.00): chord của riêng `f12` + `f11` lên tuyến O ở R=1.8 ≈ **1.6 u** → 4.3 tower-giây → **880** sát thương → headroom **0.13** ❌. Đây là cái bẫy trung tâm của map cuối: **ô canh nhiều tuyến nhất lại là ô canh ít đường nhất.**

**(3) "Chỉ một cụm boss"** — xây đủ Pulga Lv3 cho tuyến O, bỏ tuyến E. Boss O chết (1.14), boss E lọt → **−5 máu**. Sống sót **chỉ khi** máu cầu môn còn ≥ 6 lúc vào W20. Đây là build "gần thắng" cố ý — nó dạy rằng một cụm là không đủ, mà không punish quá tay.

### 7c. Đường thắng — "ba lớp, hai mũi khoan"

| Wave | Hành động | Chi | Luỹ kế |
|------|-----------|-----|--------|
| W1–3 | El Cinco `f04` (120) · Pulga `f05` (300) — chỉ tuyến C đang chạy | 420 | 420 |
| W4–6 | El Cinco `f01` (120) mở tuyến O · Pulga `f10` (300) pocket O×C | 420 | 840 |
| W7–9 | El Cinco `f07` (120) mở tuyến E · Pulga `f11` (300) pocket C×E · Dibu `gk01` (140) | 560 | 1 400 |
| W10 | nâng `f05` → Lv2 (240) · nâng `f11` → Lv2 (240) → đủ 1.45 headroom boss C | 480 | 1 880 |
| W11–13 | D10S `f09` (240) → **ổ khoá `f09` × `f06`** · Pulga `f06` **Lv2** (540) mở ô tầm xa | 780 | 2 660 |
| W14–16 | Árbitro `f10`? **không** — giữ `f10` làm sát thương. Árbitro `f03` (240) · Pulga `f02` (300) · Pulga `f08` (300) | 840 | 3 500 |
| W17 | nâng `f02` `f08` → Lv2 (240 × 2) · nâng `f10` → Lv2 (240) | 720 | 4 220 |
| W18 | **Bán `f01` + `f07` El Cinco (hoàn 72 × 2 = 144)** → Pulga `f01` (300) + Pulga `f07` (300) | 456 | 4 676 |
| W19 | nâng `f01` `f02` `f10` → **Lv3** (480 × 3 = 1 440) — hoàn tất cụm boss **tuyến O** | 1 440 | 6 116 |
| W20 (nghỉ) | nâng `f07` `f08` `f11` → **Lv3** (480 × 3 = 1 440) — cụm boss **tuyến E** · nâng `f12`? mua Pulga `f12` Lv3 (1 020) · Dibu → Lv3 (336) | 2 796 | **8 912** |

Chi 8 912 / có 11 968 → dư **3 056** cho sai lầm, sink sửa cầu môn và nâng `f06`/`f03`.
Ở W20: **tuyến O** có `f01` `f02` `f03` `f06` `f10` `f12`; **tuyến E** có `f07` `f08` `f09` `f11` `f12`; Dibu `Cản Phá` dành cho con nào tới trước.

### 7d. Chứng minh vẫn có đường thắng dù kháng chậm 90%

1. **Không cần làm chậm:** §7a đo ở tốc 0.372 (đã trừ kháng 90%). Pulga Lv3 đạt 1.14 / 1.13 — trước hiệu chỉnh `f12`.
2. **Hiệu chỉnh `f12`:** kể cả khi bỏ hẳn `f12` khỏi một tuyến, headroom vẫn ≈ 1.02–1.05 — **trên 1.0**. Đường thắng tồn tại nhưng **không có biên** → đúng định nghĩa map ★★★★★.
3. **Nếu buff "bị chậm" là nhị phân** (code hiện tại, `AbilityEngine.cs:130` kiểm `> 0`): Árbitro dán thẻ được lên boss (7% hiệu lực) → cờ bật → D10S Lv3 cộng **+20% cho mọi tướng** → 7 832 × 1.2 = **9 398**, headroom **1.36**. Đây là biên an toàn dành cho người chơi hiểu combo.
4. **Nếu đổi sang ngưỡng** (buff cần chậm ≥ 15%): boss M10 không bao giờ kích buff, và đường thắng ở mục 1–2 **vẫn đứng**.
5. **Van cuối:** Dibu `Cản Phá` cản đứt **một** boss (`docs/03` §6). Con còn lại phải chết bằng sát thương hoặc chấp nhận −5 máu. Cầu môn 20 máu ⇒ **được phép lọt tối đa 3 lần** cả trận (boss hoặc Tambor).

---

## 8. Yêu cầu kỹ thuật

Ngoài toàn bộ hạng mục đa tuyến của M08 (§8 mục 1–10) và mô hình cân bằng theo-tuyến của M09 (§8 mục 7), map cuối thêm:

| # | Hạng mục | Cần làm |
|---|----------|---------|
| 1 | **Nhiều boss trong một wave** | `WaveSpawner` hiện chỉ hiểu một boss/wave. Cần `bosses: [{ enemyId, hp, bounty, lane, delaySec }]`. `delaySec` tính **từ lúc quân thường cuối cùng đã spawn**, không phải từ đầu wave — nếu không, độ lệch 5 s bị trôi theo số quân. |
| 2 | HUD boss | Hai thanh máu boss cùng lúc, có nhãn tuyến (O / E). Một thanh cho hai boss = người chơi không biết đang bắn con nào. |
| 3 | `Cản Phá` của Dibu với nhiều boss | Chốt luật rõ: cản **con nào vào tầm trước**, cooldown **không** reset khi con thứ hai vào. Ghi vào `docs/01` §8 như một FM mới (FM-22). |
| 4 | Nhắm mục tiêu ở ô đa tuyến | `f12` và `gk01` thấy quân của cả ba tuyến. `TargetingSystem` hiện chọn theo "gần cầu môn nhất" — với ba tuyến, quy tắc này khiến `f12` liên tục đổi mục tiêu và **phí sát thương** (η). Cần chốt: giữ mục tiêu tới khi chết hoặc ra khỏi tầm (sticky targeting), và **đo lại η** cho map đa tuyến. |
| 5 | Mở tuyến dần | Tuyến chưa dùng ở W1–3 / W1–6 phải **vẽ mờ** chứ không ẩn — người chơi cần biết trước là sẽ có ba tuyến, để không tiêu hết tiền vào một bên. |
| 6 | `path_check.py` | Ngưỡng đoạn hở: `MAX_GAP = max(0.15, laneCount × 1.2 / fieldSlotCount)` → 3 × 1.2 / 12 = **0.30** cho map này. Thêm cảnh báo "ô canh N tuyến" để lộ ra ô như `f12` (canh 3 tuyến nhưng chỉ 4% đường). |
| 7 | Chỉ số mới cần tính | `path_check.py` nên in thêm **"% quãng đường mỗi tuyến mà ô này phủ"** bên cạnh `d`. Chỉ có con số đó mới phát hiện được bẫy `f12`; bảng `d` hiện tại làm nó trông như ô mạnh nhất map. |
| 8 | `AbilityEngine.cs:130` | Như M08 §8 mục 6 — chốt luật "bị chậm" nhị phân hay theo ngưỡng. Trên M10 nó đổi headroom boss **20%**, tức lật kết quả trận. |

---

## 9. Rủi ro

| # | Rủi ro | Mức | Đánh giá / giảm thiểu |
|---|--------|-----|-----------------------|
| R1 | **Map có bất khả thi không?** | 🟡 **KHÔNG, nhưng biên rất mỏng** | §7a: 1.14 / 1.13 trước hiệu chỉnh `f12`, **1.02–1.05 sau hiệu chỉnh**. Chi phí đường thắng 8 912 < 11 968 tiền cả trận. Bất khả thi **nếu** phép đo engine thật cho ra η thấp hơn giả định — **phải đo trước khi phát hành**. Nếu headroom đo được < 1.0, đòn bẩy đầu tiên là hạ boss 6 900 → 6 200 (giữ nguyên hình học). |
| R2 | Hai boss cùng lúc = mất kiểm soát, người chơi không hiểu vì sao thua | 🔴 CAO | Bắt buộc: cảnh báo ở màn hình nghỉ trước W20 ("**HAI** thủ lĩnh — tuyến Tây và tuyến Đông"), hai thanh máu riêng, và đánh dấu hai cửa vào trên map từ W19. Không có mấy thứ này thì W20 là một cú tát không đọc được. |
| R3 | Combo chỉ tồn tại trên tuyến C, boss lại ra hai tuyến biên | 🟡 TRUNG (**cố ý**) | Đây là ý đồ (§4a), không phải khiếm khuyết — nhưng nó **cấm** người chơi dùng thứ map vừa dạy ở W10. Phải kiểm bằng playtest rằng cảm giác là "cú lật" chứ không phải "bị lừa". Nếu phản hồi xấu, dời **một** boss W20 sang tuyến C. |
| R4 | `f12` là bẫy — trông mạnh nhất map nhưng chỉ canh 4% đường | 🟡 TRUNG | Bẫy có chủ đích (§7b-2). Giảm thiểu: vòng tầm khi chọn `f12` phải hiện rõ nó chỉ cắt đoạn cuối. Nếu > 60% người chơi mua `f12` trước W10 rồi thua, đổi thành: dời `f12` lên y ≈ −7.5 (mất phủ 1 tuyến, được phủ nhiều đường hơn). |
| R5 | Ngân sách chống boss (10 200) chiếm 85% tiền cả trận | 🔴 CAO | Ép bán-xây-lại ở Act 3 — đúng ý đồ `docs/03`, nhưng nếu `sellRefundRatio` 0.6 làm việc bán quá đắt thì đường thắng đóng lại. **Cần đo riêng**: chi phí bán-xây-lại của kịch bản §7c (W18 bán 2 El Cinco = mất 96 Peso ròng). Nếu đo thấy nghẹt, nâng `sellRefundRatio` cho **riêng map này** lên 0.7. |
| R6 | Đoạn hở 22–24% trên hai tuyến biên | 🟡 TRUNG | Sàn toán học với 12 ô / 3 tuyến là 0.20 — không sửa được bằng cách xếp lại. Nếu tỉ lệ lọt W15–19 quá cao, đòn bẩy đúng là **giảm số quân**, không phải thêm ô (thêm ô làm trần chi tiêu tăng và phá khan hiếm). |
| R7 | Ba tuyến trên màn dọc 10.8 u → chật, khó bấm | 🟡 TRUNG | Đã kiểm: cặp ô gần nhau nhất 65 pt, không ô nào tràn mép. Nhưng ba đường + 13 ô + 62 quân ở W20 là mật độ hình ảnh cao nhất game. Cần kiểm trên máy thật ở W20, không chỉ kiểm bằng số. |
| R8 | Mô hình cân bằng cộng chord của cả ba tuyến rồi so tổng máu | 🔴 CAO | `chord/2R` tb = 1.20 vì `f03` `f06` `f09` `f10` `f11` `f12` được đếm nhiều lần. Cộng gộp sẽ **đánh giá cao phe phòng ngự ~20%**. Bắt buộc tính headroom **theo từng tuyến** (như M09 §8 mục 7). |
