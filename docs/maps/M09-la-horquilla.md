# M09 — La Horquilla ★★★★★

> **1 tuyến tách 2 rồi nhập 1 · 11 ô sân + 1 ô thủ môn · boss kháng chậm 88%**
> Toạ độ đã kiểm bằng bản copy `tools/path_check.py` (mở rộng cho đường có nhánh) trong thư mục nháp.
> Mọi con số cân bằng là **ĐÍCH CẦN ĐO** bằng test 20-wave chạy engine thật.
> ⚠️ Không dùng `tools/balance_sim.py` để chốt số — nó báo game dễ hơn thực tế.

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m09 -->
**Nguồn: `config/maps/m09-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 2 — `A` 23.6u · `B` 23.5u |
| Ô sân / thủ môn | 11 / 2 |
| Σchord@1.4 mỗi ô | 3.66 |
| Tiền khởi đầu | 1 000 |
| Máu quái | ×0.67 × 1.07^(W−1) × milestone → W20 ×4.85 |
| Tổng quân 20 wave | 346 |

| W | ×máu | `A` | `B` | boss | quân | tổng máu |
|---|------|------|------|------|------|----------|
| 1 | ×0.67 | 4/0/0 | 4/0/0 | — | 8 | 536 |
| 2 | ×0.72 | 4/0/0 | 4/0/0 | — | 8 | 576 |
| 3 | ×0.77 | 4/2/0 | 4/2/0 | — | 12 | 1 292 |
| 4 | ×0.82 | 4/3/0 | 4/3/0 | — | 14 | 1 742 |
| **5** | ×1.05 | 0/5/0 | 0/5/1 | `o_capitao`@`B` | 12 | 3 900 |
| 6 | ×1.13 | 0/6/0 | 0/5/1 | — | 12 | 3 348 |
| 7 | ×1.21 | 0/6/1 | 0/5/2 | — | 14 | 4 907 |
| 8 | ×1.29 | 0/5/1 | 0/5/1 | — | 12 | 4 260 |
| 9 | ×1.38 | 0/6/1 | 0/5/1 | — | 13 | 4 864 |
| **10** | ×1.72 | 0/6/1 | 0/5/2 | `o_capitao`@`B` | 15 | 9 513 |
| 11 | ×1.85 | 0/7/1 | 0/6/2 | — | 16 | 8 323 |
| 12 | ×1.97 | 0/7/2 | 0/7/2 | — | 18 | 10 420 |
| 13 | ×2.11 | 0/9/2 | 0/8/2 | — | 21 | 12 553 |
| 14 | ×2.26 | 0/8/3 | 0/7/3 | — | 21 | 14 913 |
| **15** | ×2.94 | 0/8/2 | 0/7/2 | `o_capitao`@`A` + `o_capitao`@`B` | 21 | 21 750 |
| 16 | ×3.14 | 0/9/2 | 0/9/3 | — | 23 | 21 078 |
| 17 | ×3.36 | 0/10/2 | 0/9/3 | — | 24 | 23 305 |
| 18 | ×3.60 | 0/10/3 | 0/10/3 | — | 26 | 27 714 |
| 19 | ×3.85 | 0/10/3 | 1/9/4 | — | 27 | 31 297 |
| **20** | ×4.85 | 0/12/4 | 1/11/5 | `o_capitao`@`A` + `o_capitao`@`B` | 35 | 58 988 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m09 -->

---

## 1. Ý đồ thiết kế

*La Horquilla* (cái chạc) kiểm tra **khả năng phân bổ hoả lực theo thời gian, không theo không gian**. Quân ra từ **một cửa duy nhất**, đi chung 12.4 units, rồi đường **chẻ đôi**, chạy riêng 10–13 units, rồi **nhập lại** 5.4 units trước cầu môn.

Ba pha có ba luật khác nhau:
- **Trước tách** (f 0 → 0.44) — mọi con đều đi qua. Một ô ở đây "đáng giá gấp đôi" một ô ở nhánh, nhưng quân còn đầy máu và đông nhất.
- **Giữa** (f 0.44 → 0.81) — phải chia đôi hoả lực, hoặc chấp nhận bỏ một nhánh.
- **Sau nhập** (f 0.81 → 1.0) — dồn lại, nhưng chỉ còn **1 ô sân + ô thủ môn** ở đó và quân đã sát cầu môn.

11 ô **cố tình không đủ** để làm tốt cả ba pha. Người chơi phải chọn hy sinh pha nào — và câu trả lời đổi theo wave, vì tỉ lệ chia nhánh đổi theo loại quân.

---

## 2. Toạ độ waypoint

Đường gồm **4 đoạn** nối tại 2 nút. Mỗi đoạn spline Catmull-Rom **riêng** rồi ghép — không spline xuyên qua nút, nếu không điểm rẽ bị bo tròn và hai nhánh không chung gốc.

- **Nút TÁCH** `S = (0.00, 2.30)`
- **Nút NHẬP** `M = (0.00, -5.70)`

### Thân chung — "El Tronco" (12.40 u)

| # | x | y |
|---|---|---|
| 1 | 0.00 | 9.36 | (spawn — cửa duy nhất) |
| 2 | -2.90 | 8.70 |
| 3 | -4.30 | 6.70 |
| 4 | -3.00 | 4.60 |
| 5 | -0.50 | 3.90 |
| 6 | 0.00 | 2.30 | **(nút TÁCH)** |

### Nhánh A — "La Corta" (ngắn, 10.35 u)

| # | x | y |
|---|---|---|
| 1 | 0.00 | 2.30 | (nút TÁCH) |
| 2 | 2.10 | 1.40 |
| 3 | 2.70 | -1.00 |
| 4 | 1.70 | -3.40 |
| 5 | 0.40 | -4.90 |
| 6 | 0.00 | -5.70 | **(nút NHẬP)** |

### Nhánh B — "La Larga" (dài, 12.87 u)

| # | x | y |
|---|---|---|
| 1 | 0.00 | 2.30 | (nút TÁCH) |
| 2 | -2.30 | 1.50 |
| 3 | -4.10 | -0.20 |
| 4 | -4.30 | -2.60 |
| 5 | -2.90 | -4.30 |
| 6 | -1.10 | -5.10 |
| 7 | 0.00 | -5.70 | **(nút NHẬP)** |

### Đuôi chung — "El Cuello" (5.36 u)

| # | x | y |
|---|---|---|
| 1 | 0.00 | -5.70 | (nút NHẬP) |
| 2 | 1.90 | -6.60 |
| 3 | 0.70 | -8.30 |
| 4 | 0.00 | -9.13 | (goal) |

### Kết quả đo

| Tuyến hoàn chỉnh | Ghép | Độ dài | Hộp bao x | Hộp bao y | Tràn viewport |
|------------------|------|--------|-----------|-----------|----------------|
| **A** | Tronco + Corta + Cuello | **28.11 u** | [-4.30, 2.71] | [-9.13, 9.36] | ❌ không |
| **B** | Tronco + Larga + Cuello | **30.63 u** | [-4.41, 1.91] | [-9.13, 9.36] | ❌ không |

Mốc pha theo quãng đường:

| Pha | Trên tuyến A | Trên tuyến B |
|-----|--------------|--------------|
| Thân chung | f 0.00 → **0.44** | f 0.00 → **0.40** |
| Nhánh riêng | f 0.44 → **0.81** | f 0.40 → **0.82** |
| Đuôi chung | f 0.81 → 1.00 | f 0.82 → 1.00 |

---

## 3. Toạ độ ô

| id | type | x | y | d[A] | d[B] | f[A] | f[B] | Pha | Tướng với tới ở **Lv1** |
|----|------|------|------|------|------|------|------|-----|--------------------------|
| `f01` | field | -1.42 | 8.25 | **0.88** | **0.88** | 0.06 | 0.05 | thân | El Cinco · D10S · Árbitro · Pulga |
| `f02` | field | -2.97 | 6.74 | **1.30** | **1.30** | 0.18 | 0.16 | thân | **Pulga** |
| `f03` | field | -2.28 | 5.46 | **1.10** | **1.10** | 0.30 | 0.28 | thân | D10S · Árbitro · Pulga |
| `f04` | field | 1.16 | 4.10 | **1.50** | **1.50** | 0.40 | 0.36 | thân | — ⛰️ **ô tầm xa** (R ≥ 1.6) |
| `f05` | field | 1.31 | 0.61 | **1.10** | 2.14 | 0.53 | — | nhánh A | D10S · Árbitro · Pulga |
| `f06` | field | 0.87 | -2.24 | **1.30** | 3.44 | 0.68 | — | nhánh A | **Pulga** |
| `f07` | field | -2.42 | 0.36 | 3.10 | **0.88** | — | 0.51 | nhánh B | El Cinco · D10S · Árbitro · Pulga |
| `f08` | field | -2.89 | -2.05 | 4.35 | **1.50** | — | 0.64 | nhánh B | — ⛰️ **ô tầm xa** (R ≥ 1.6) |
| `f09` | field | -1.89 | -3.62 | 2.61 | **1.10** | — | 0.74 | nhánh B | D10S · Árbitro · Pulga |
| `f10` | field | -0.01 | 0.94 | **1.30** | **1.30** | 0.45 | 0.42 | 🔑 **bản lề** | **Pulga** — canh CẢ HAI nhánh |
| `f11` | field | 0.81 | -6.62 | **0.62** | **0.62** | 0.85 | 0.86 | đuôi | El Cinco · D10S · Árbitro · Pulga |
| `gk01` | goalkeeper | 0.14 | -7.89 | **0.69** | **0.69** | 0.96 | 0.96 | đuôi | Dibu (mọi cấp) |

### Phân bổ ô theo pha — chỗ hụt là cố ý

| Pha | Số ô | % quãng đường (tuyến A) | Nhận xét |
|-----|------|-------------------------|----------|
| Thân chung | **4** (`f01`–`f04`) | 44% | Mỗi ô đánh **100%** quân. Hiệu suất cao nhất, nhưng quân đầy máu. |
| Nhánh | **5 + 1 bản lề** (`f05`–`f10`) | 37% | 2 ô cho A, 3 ô cho B, `f10` canh cả hai. Nhánh A thưa hơn — **cố ý**, và boss đi nhánh A. |
| Đuôi chung | **1 + thủ môn** (`f11`, `gk01`) | 19% | Chỉ một ô sân. "Dồn lại" nhưng **không có chỗ đứng** — đây là bài học chính của map. |

### Thang tầm

| Tướng | Tầm Lv1 | Số ô với tới |
|-------|---------|--------------|
| El Cinco | 1.0 | **3/11** (`f01` `f07` `f11`) |
| D10S | 1.2 | **6/11** (+ `f03` `f05` `f09`) |
| El Árbitro | 1.2 | **6/11** |
| La Pulga | 1.4 | **9/11** (+ `f02` `f06` `f10`) |
| — không tướng Lv1 nào | | **2/11**: `f04` (1.50) · `f08` (1.50) |

### Ràng buộc vật lý (đã kiểm)

| Ràng buộc | Kết quả |
|-----------|---------|
| Ô chết không khai `farSlot` | 0 |
| Ranh giới dao cạo | 0 vi phạm |
| Cặp ô gần nhau nhất | `f05`–`f10` = 1.36 u = **50 pt** ≥ 48 pt ✅ |
| Vùng chạm tràn mép khung | 0 vi phạm |
| Đoạn hở tuyến A | **16%** (4.6 u) tại f 0.68 → 0.85 |
| Đoạn hở tuyến B | **14%** (4.3 u) tại f 0.86 → 1.00 ✅ |
| chord thật / 2R (tổng trên mọi tuyến) | R=1.4: tb **1.02** · R=1.8: tb **1.41** |

⚠️ `chord/2R` **> 1.0** ở đây là **đúng, không phải lỗi**: ô trên thân và đuôi được **đếm hai lần** vì hai tuyến đi trùng nhau ở đó. Đó chính là lý do ô thân đắt giá. Khi tính cân bằng, tổng máu phải chia theo tuyến, không nhân đôi — xem §8 mục 7.

---

## 4. Bảng phủ synergy

### 4a. Chồng lấn hình học (cho nguồn chậm CỤC BỘ: D10S Lv1, aura Árbitro)

| Ô làm chậm | Ô sát thương | Tuyến | Giao ở R 1.4 × 1.6 | Giao ở R 1.6 × 1.8 |
|------------|--------------|-------|--------------------|--------------------|
| `f03` | `f02` | A + B | 0.00 | **1.37 u** (0.68 A + 0.68 B) |
| `f02` | `f03` | A + B | 0.00 | 1.17 u (0.58 + 0.58) |
| `f05` | `f10` | A | **0.23 u** | 0.93 u |
| `f10` | `f05` | A | 0.00 | 0.88 u |
| *các cặp còn lại* | | | 0.00 | 0.00 |

🔴 Cặp `f02`×`f03` là **ổ khoá mạnh nhất trong ba map** vì nó nằm trên **thân chung**: một lần chồng lấn ăn cả hai tuyến. Nhưng nó chỉ mở ở tầm Lv3 (R 1.6 × 1.8 = Árbitro Lv3 + Pulga Lv3 = 816 + 1 020 = **1 836 Peso** cho hai ô). Ổ khoá rẻ hơn là `f05`×`f10` (Lv2, chỉ ăn nhánh A).

### 4b. Thẻ vĩnh viễn (Árbitro) — kênh synergy chính

Thẻ theo con quân tới hết đường (`docs/01` §8 FM-16), nên **không cần chồng lấn**: đặt Árbitro càng sớm càng nhiều ô hưởng lợi.

| Ô đặt Árbitro | Canh được (Lv3, R=1.6) | % quãng đường SAU ô — A / B | Giá trị |
|---------------|------------------------|------------------------------|---------|
| **`f01`** | A + B | **94% / 95%** | 🥇 dán thẻ sớm nhất, mọi ô sau đều hưởng |
| `f02` | A + B | 82% / 84% | |
| `f03` | A + B | 70% / 72% | |
| `f04` | A + B | 60% / 64% | ô tầm xa, Árbitro Lv3 mới với tới |
| `f10` | A + B | 55% / 58% | 🥈 bản lề — dán thẻ ngay sau khi tách |
| `f05` | chỉ A | 47% / — | |
| `f07` | chỉ B | — / 49% | |

**Bố trí khuyến nghị:** Árbitro ở `f01` (thân, sớm nhất) → thẻ dán từ f 0.06, mọi ô sát thương của cả hai nhánh và đuôi đều đánh vào mục tiêu "bị chậm". Đây là cách duy nhất để một tướng phục vụ **cả ba pha** cùng lúc.

---

## 5. Bảng 20 wave

`hp = baseHp × 0.67 × 1.09^(wave-1)` · spawn cách 0.7s · nghỉ 8s.

### Luật chia nhánh (KHÔNG dùng ngẫu nhiên)

Quân được gán nhánh **tại lúc spawn**, theo `spawnIndex` của từng loại, bằng bộ phân bổ **Bresenham** (xác định, tái lập được). Tỉ lệ theo loại:

| Loại | → Nhánh A (Corta) | → Nhánh B (Larga) | Lý do |
|------|-------------------|-------------------|-------|
| Adepto | **50%** | **50%** | nền, chia đều |
| Tifoso | **70%** | **30%** | nhanh + đường ngắn = mối đe doạ chính, dồn vào A |
| Tambor | **30%** | **70%** | chậm + đường dài = thời gian phơi sáng dài, ép người chơi đầu tư nhánh B |
| **`O Capitão`** | **100%** | 0% | boss LUÔN đi nhánh A — báo trước bằng cờ hiệu trên map |

Tỉ lệ gộp toàn map ≈ **55% A / 45% B**. Cột dưới ghi số quân đã quy ra (A/B).

| W | Adepto (A/B) | Tifoso (A/B) | Tambor (A/B) | Boss | Quân | Tổng máu | Thưởng hạ | Clear | Tiền wave | Ví cộng dồn |
|---|--------------|--------------|--------------|------|------|----------|-----------|-------|-----------|-------------|
| 1 | 8 (4/4) | — | — | — | 8 | 536 | 40 | 20 | 60 | 760 |
| 2 | 12 (6/6) | — | — | — | 12 | 876 | 60 | 25 | 85 | 845 |
| 3 | 10 (5/5) | 4 (3/1) | — | — | 14 | 1 500 | 94 | 30 | 124 | 969 |
| 4 | 10 (5/5) | 8 (6/2) | — | — | 18 | 2 398 | 138 | 35 | 173 | 1 142 |
| 5 | 10 (5/5) | 6 (4/2) | 1 (0/1) | — | 17 | 2 718 | 140 | 40 | 180 | 1 322 |
| 6 | 10 (5/5) | 9 (6/3) | 1 (0/1) | — | 20 | 3 640 | 173 | 45 | 218 | 1 540 |
| 7 | 10 (5/5) | 9 (6/3) | 3 (1/2) | — | 22 | 5 197 | 221 | 50 | 271 | 1 811 |
| 8 | 10 (5/5) | 7 (5/2) | 2 (1/1) | — | 19 | 4 451 | 252 | 55 | 307 | 2 118 |
| 9 | 12 (6/6) | 9 (6/3) | 2 (1/1) | — | 23 | 5 722 | 298 | 60 | 358 | 2 476 |
| **10** | 10 (5/5) | 9 (6/3) | 3 (1/2) | **1 → A** | 23 | **9 940** | 319 + **200** | 65 | 584 | 3 060 |
| 11 | 12 (6/6) | 11 (8/3) | 3 (1/2) | — | 26 | 8 363 | 365 | 70 | 435 | 3 495 |
| 12 | 12 (6/6) | 12 (8/4) | 4 (1/3) | — | 28 | 10 440 | 416 | 75 | 491 | 3 986 |
| 13 | 12 (6/6) | 15 (11/4) | 4 (1/3) | — | 31 | 12 625 | 464 | 80 | 544 | 4 530 |
| 14 | 15 (8/7) | 14 (10/4) | 5 (2/3) | — | 34 | 15 053 | 504 | 85 | 589 | 5 119 |
| 15 | 14 (7/7) | 12 (8/4) | 4 (1/3) | — | 30 | 13 976 | 580 | 90 | 670 | 5 789 |
| 16 | 14 (7/7) | 15 (11/4) | 5 (2/3) | — | 34 | 18 181 | 690 | 95 | 785 | 6 574 |
| 17 | 16 (8/8) | 15 (11/4) | 5 (2/3) | — | 36 | 20 346 | 710 | 100 | 810 | 7 384 |
| 18 | 16 (8/8) | 17 (12/5) | 6 (2/4) | — | 39 | 25 056 | 799 | 105 | 904 | 8 288 |
| 19 | 18 (9/9) | 18 (13/5) | 8 (2/6) | — | 44 | 32 102 | 934 | 110 | 1 044 | 9 332 |
| **20** | 20 (10/10) | 22 (15/7) | 10 (3/7) | **1 → A** | 53 | **55 506** | 1 132 + **500** | 150 | 1 782 | **11 114** |

**Tổng:** 531 quân · 248 626 máu.

### Boss `O Capitão` trên M09

| | W10 | W20 |
|---|-----|-----|
| Máu | **3 200** | **13 000** |
| Nhánh | **A (La Corta)** | **A (La Corta)** |
| Thưởng | 200 | 500 |
| **Kháng chậm** | **88%** | **88%** |
| Trừ máu khi lọt | 5 | 5 |

Kháng 88% → chậm thực tế tối đa `70% × 0.12` = **8.4%** → tốc 0.366 u/s (so 0.4). Kéo dài phơi sáng chỉ **9.2%**.

**Dẫn xuất máu:** tổng chord thật lên tuyến A ở R=1.8 = **26.53 u** ÷ 0.366 = **72.4 tower-giây**. Chuẩn map gốc 237.5 máu/tower-giây → 17 199; hạ xuống **13 000** để headroom đội hình chống-boss ≈ **1.14**.

**Nhịp:** W1–2 chỉ Adepto chia đều — dạy rằng tách nhánh có thật. W3 Tifoso vào và **dồn 70% về nhánh A**: người chơi mới sẽ xây cân đối rồi thấy A thủng. W5 Tambor vào **nhánh B** — buộc mở nhánh thứ hai đúng lúc ví còn mỏng. W13 và W16 là "Tifoso rush nhánh A" (11 con). W19–20 Tambor dồn nhánh B (6–7 con) trong khi boss đi nhánh A — **hai đầu ép cùng lúc**, đây là bài kiểm cuối của map.

---

## 6. Kinh tế đề xuất

| Tham số | Giá trị |
|---------|---------|
| `startingCash` | **700** |
| `goalHealth` | 20 |
| `bountyMultiplier` theo act | **1.00 / 1.45 / 1.95** (map gốc: 1.0 / 1.6 / 2.2) |
| Clear bonus | 15 + 5×W (W20 = 150) |
| `skipBonusPerSecond` | 3 · `sellRefundRatio` 0.6 |

### Trần chi tiêu

```
trần   = 11 × 1020 + 476 = 11 696
tiền cả trận (700 + thưởng 9 029 + clear 1 385) = 11 114
biên   = +582  (+5.0 %)
tiền cả trận + skip tối đa (456)                = 11 570  <  11 696  ✅
```

Biên +5.0% **hẹp hơn** map gốc (+5.7%) — cố ý, vì M09 là ★★★★★ và ô thân chung có hiệu suất gấp đôi, nên nếu cho dư tiền map sẽ dễ hơn nó trông.

⚠️ **Cần ĐO bằng engine thật**: headroom mỗi wave trong **[1.05, 1.65]**, đích [1.10, 1.50]. **Điểm nghi ngờ số một: W5–W7**, khi người chơi buộc phải mở nhánh thứ hai với ví ~1 300 Peso. Nếu vỡ sàn ở đó, đòn bẩy theo thứ tự: (1) dời Tambor đầu tiên từ W5 sang W6; (2) `startingCash` 700 → 780; (3) **không** đụng hình học.

---

## 7. Đường thắng dự kiến & build cố ý thua

### 7a. Bảng đối chứng boss W20 (13 000 máu, nhánh A, kháng chậm 88%)

Mọi ô canh được tuyến A (gồm ô thân và ô đuôi), cùng một tướng cùng một cấp:

| Đội hình | Tầm | Tổng chord tuyến A | Sát thương | **Headroom** |
|----------|-----|--------------------|------------|--------------|
| **Pulga Lv3** (204 dps) | 1.8 | 26.53 u | **14 773** | **1.14** ✅ |
| Pulga Lv2 (108 dps) | 1.6 | 21.49 u | 6 334 | 0.49 ❌ |
| Pulga Lv1 (54 dps) | 1.4 | 15.51 u | 2 286 | 0.18 ❌ |
| D10S Lv3 (60 dps) | 1.6 | 21.49 u | 3 519 | 0.27 ❌ |
| D10S Lv2 (38.4 dps) | 1.4 | 15.51 u | 1 626 | 0.13 ❌ |
| El Cinco Lv3 (27 dps) | 1.4 | 15.51 u | 1 143 | 0.09 ❌ |

*(Nếu boss đi nhánh B: Pulga Lv3 = 16 222 → headroom 1.25. Boss đi nhánh A vì A khó hơn.)*

### 7b. Build CỐ Ý THUA — "dồn hết vào thân chung"

**4 ô thân (`f01`–`f04`) + bản lề `f10`, tất cả Pulga Lv3** = 5 × 1 020 = 5 100 Peso, bỏ trắng hai nhánh và đuôi.
Chord của riêng 5 ô đó lên tuyến A ở R=1.8 ≈ 14.9 u → 40.7 tower-giây → **8 300** sát thương lên boss → **headroom 0.64** ❌.
Tệ hơn: ở W16–W19, 11 Tifoso nhánh A đi qua thân trong ~4 giây rồi **không gặp gì nữa** suốt 56% quãng đường còn lại. Đây là cái bẫy chính của map — "ô thân đáng giá gấp đôi" đúng về hiệu suất, **sai** về khả năng dọn hết máu.

Build thua thứ hai: **toàn D10S/El Cinco** (đám đông). Dọn quân thường xuất sắc nhờ lan 1.7 của D10S Lv2 trên hàng dọc, nhưng boss W20 chỉ ăn ≤ 3 519 → **headroom 0.27** ❌.

### 7c. Đường thắng — "thân giữ nhịp, nhánh A giữ boss, đuôi là lưới cuối"

| Wave | Hành động | Chi | Luỹ kế |
|------|-----------|-----|--------|
| W1 | El Cinco `f01` (120) · Pulga `f02` (300) — hai ô thân, ăn 100% quân | 420 | 420 |
| W3–4 | Pulga `f10` bản lề (300) — canh cả hai nhánh ngay sau nút tách | 300 | 720 |
| W5–7 | D10S `f09` (240) nhánh B chặn Tambor · El Cinco `f11` (120) đuôi | 360 | 1 080 |
| W8–10 | D10S `f05` (240) nhánh A · nâng `f02` → Lv2 (240) · Dibu `gk01` (140) | 620 | 1 700 |
| W11–13 | **Bán `f01` El Cinco (hoàn 72)** → **El Árbitro `f01` (240)**: thẻ dán từ f 0.06, 94% đường hưởng lợi · nâng `f10` → Lv2 (240) | 408 | 2 108 |
| W14–16 | Pulga `f06` Lv2 (540) nhánh A · Pulga `f04` **Lv2** (540) mở ô tầm xa trên thân | 1 080 | 3 188 |
| W17–18 | nâng `f02` → **Lv3** (480) · nâng `f10` → **Lv3** (480) · Árbitro `f01` → Lv2 (192) | 1 152 | 4 340 |
| W19 | nâng `f06` → Lv3 (480) · Pulga `f08` Lv2 (540) nhánh B | 1 020 | 5 360 |
| W20 | nâng `f04` → Lv3 (480) · nâng `f05` D10S → **Lv3** (576) · Dibu → Lv3 (336) | 1 392 | **6 752** |

Chi 6 752 / có 11 114 → dư 4 362 cho sai lầm và bán-xây-lại.
Ở W20, tuyến A được canh bởi `f01`(Árbitro) `f02`(Pulga Lv3) `f04`(Pulga Lv3) `f05`(D10S Lv3) `f06`(Pulga Lv3) `f10`(Pulga Lv3) `f11` + Dibu — đủ chord để vượt 13 000.

### 7d. Chứng minh vẫn có đường thắng dù kháng chậm 88%

1. **Không cần làm chậm:** §7a đo ở tốc boss 0.366 (đã trừ kháng 88%). Pulga Lv3 toàn tuyến A = **1.14**.
2. **Nếu buff "bị chậm" là nhị phân** (code hiện tại, `AbilityEngine.cs:130` kiểm `> 0`): thẻ Árbitro ở `f01` vẫn dán được lên boss (8.4% hiệu lực) → cờ bật → D10S Lv3 ở `f05` cộng **+20% cho MỌI tướng** → 14 773 × 1.2 = **17 728**, headroom **1.36**. Đây chính là lý do đường thắng §7c mua D10S Lv3.
3. **Nếu đổi sang ngưỡng** (buff cần chậm ≥ 15%): boss M09 không bao giờ kích buff, và đường thắng ở mục 1 **vẫn đứng** (1.14). Không map nào bất khả thi ở cả hai cách đọc.
4. **Van cuối:** Dibu `Cản Phá` ở `gk01` (d 0.69 tới cả hai nhánh) cản được boss.

---

## 8. Yêu cầu kỹ thuật

Ngoài toàn bộ hạng mục đa tuyến của M08 (§8 của `M08-el-mirador.md`, mục 1–10), map này thêm:

| # | Hạng mục | Cần làm |
|---|----------|---------|
| 1 | **Mô hình đường có nhánh** | Không cài "rẽ nhánh giữa đường". Cài **hai `EnemyPath` hoàn chỉnh** A và B **trùng nhau** trên đoạn thân và đoạn đuôi (lặp lại waypoint). Quân được gán `PathId` **tại lúc spawn** và **không bao giờ đổi**. Đây là cách rẻ nhất và tránh hoàn toàn lớp "chuyển nhánh" trong `PathFollower`. |
| 2 | Chia nhánh xác định | `WaveSpawner` cần bộ phân bổ **Bresenham theo loại quân** (`branchSplit: {adepto:[50,50], tifoso:[70,30], tambor:[30,70]}`). **Cấm dùng RNG** — test cân bằng phải tái lập được. |
| 3 | Boss theo nhánh | Boss khai `lane: "A"` cứng trong wave. UI phải **báo trước** bằng cờ hiệu ở nút tách, nếu không người chơi không có cơ sở để chuẩn bị. |
| 4 | Vẽ đường trùng | `Pitch.cs` vẽ **union** của hai polyline, không vẽ hai lần — nếu không đoạn thân/đuôi dày gấp đôi và trông như lỗi. Nút tách và nút nhập cần chỉ báo hình ảnh riêng (mũi tên chẻ / mũi tên gộp). |
| 5 | Vòng tầm ở ô thân | Khi hiện vòng tầm cho `f01`–`f04` `f11` `gk01`, phải làm rõ nó phủ **cả hai** tuyến; ô nhánh chỉ phủ một. Không có chỉ báo này thì giá trị gấp đôi của ô thân là kiến thức ẩn. |
| 6 | `path_check.py` | Thêm khái niệm **đoạn** (segment) để tính `f` theo pha, và cảnh báo riêng cho từng pha thay vì một con số toàn tuyến. |
| 7 | 🔴 Mô hình cân bằng | Ô thân/đuôi được đếm **hai lần** khi cộng chord theo tuyến (`chord/2R` tb = 1.41). Bất kỳ công thức nào cộng chord của mọi tuyến rồi so với **tổng** máu wave sẽ **đánh giá cao phe phòng ngự ~40%**. Đúng cách: tính headroom **theo từng tuyến**, mỗi tuyến chỉ nhận phần máu của quân đi tuyến đó. |
| 8 | `AbilityEngine.cs:130` | Như M08 §8 mục 6 — chốt luật "bị chậm" nhị phân hay theo ngưỡng. |

---

## 9. Rủi ro

| # | Rủi ro | Mức | Đánh giá / giảm thiểu |
|---|--------|-----|-----------------------|
| R1 | **Map có bất khả thi không?** | 🟢 KHÔNG | §7a: headroom 1.14 lên boss với 8 ô canh tuyến A ở Pulga Lv3 (8 × 1 020 = 8 160 < 11 114). Ba ô gần (`f01` `f07` `f11`) bảo đảm W1 không bế tắc với 700 Peso. |
| R2 | **Ô thân quá mạnh → map suy biến thành "chỉ xây thân"** | 🔴 CAO | Đây là rủi ro lớn nhất. Đã xử bằng: chỉ **4 ô thân** (36% ngân sách ô), thân chỉ chiếm 44% quãng đường, và §7b chứng minh build toàn-thân chỉ đạt 0.64 lên boss. **Phải đo lại**: nếu telemetry cho thấy > 70% người chơi bỏ trắng một nhánh mà vẫn thắng, giảm còn 3 ô thân và chuyển 1 ô sang đuôi. |
| R3 | Đuôi chung chỉ **1 ô sân** — có thể quá mỏng | 🟡 TRUNG | Cố ý (bài học "dồn lại nhưng không có chỗ đứng"). Nhưng nếu tỉ lệ lọt ở W18–20 > 3 con/wave thì thêm 1 ô đuôi và bỏ `f08` (ô tầm xa nhánh B ít được dùng nhất). |
| R4 | Chia nhánh 70/30 cho Tifoso làm nhánh B thành nội dung chết ở Act 1 | 🟡 TRUNG | Bù bằng Tambor 30/70 từ W5. Nếu đo thấy người chơi bỏ trắng B tới W10, đổi Tifoso thành 60/40. |
| R5 | Ổ khoá synergy rẻ chỉ có 1 cặp (`f05`×`f10`, chỉ nhánh A) | 🟡 TRUNG | Cặp mạnh (`f02`×`f03`) đòi 1 836 Peso. Kênh chính vẫn là **thẻ vĩnh viễn** của Árbitro ở `f01` (§4b) — không cần chồng lấn. Nếu Árbitro bị đo là quá yếu ở đây thì nâng `tauByArbitroLevel`, **không** dời ô. |
| R6 | Người chơi không hiểu boss luôn đi nhánh A | 🟡 TRUNG | Bắt buộc có cờ hiệu ở nút tách + dòng cảnh báo ở màn hình nghỉ trước W10 và W20. Nếu không, "boss đi nhánh nào" trở thành kiến thức phải chết mới học được. |
| R7 | Hai tuyến trùng nhau làm mô hình cân bằng sai 40% | 🔴 CAO | Xem §8 mục 7. Đây là lỗi **sẽ xảy ra** nếu ai đó tái dùng công thức của map một tuyến. Phải viết test bảo vệ: headroom tính theo tuyến, không theo tổng. |
