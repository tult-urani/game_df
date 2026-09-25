# M07 — Tres Puertas (Ba Cánh Cửa)

> **3 tuyến LỆCH NHỊP · 13 ô sân + 1 ô thủ môn · boss kháng chậm 85% · ★★★★**
> Trạng thái: **THIẾT KẾ** — toạ độ đã kiểm bằng bản sao `tools/path_check.py` (đa tuyến).
> Số kinh tế ở §6 là **ĐÍCH CẦN ĐO** bằng engine thật.

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m07 -->
**Nguồn: `config/maps/m07-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 3 — `oeste` 22.5u · `central` 25.4u · `este` 22.9u |
| Ô sân / thủ môn | 13 / 1 |
| Σchord@1.4 mỗi ô | 4.19 |
| Tiền khởi đầu | 900 |
| Máu quái | ×0.67 × 1.07^(W−1) × milestone → W20 ×4.85 |
| Tổng quân 20 wave | 301 |

| W | ×máu | `oeste` | `central` | `este` | boss | quân | tổng máu |
|---|------|------|------|------|------|------|----------|
| 1 | ×0.67 | 2/0/0 | 2/0/0 | 2/0/0 | — | 6 | 402 |
| 2 | ×0.72 | 4/0/0 | 3/0/0 | 4/0/0 | — | 11 | 792 |
| 3 | ×0.77 | 3/1/0 | 3/2/0 | 2/1/0 | — | 12 | 1 292 |
| 4 | ×0.82 | 2/2/0 | 3/2/0 | 3/2/0 | — | 14 | 1 742 |
| **5** | ×1.05 | 0/3/0 | 0/2/0 | 1/2/0 | `o_capitao`@`oeste` | 9 | 2 729 |
| 6 | ×1.13 | 0/3/0 | 0/4/0 | 0/3/1 | — | 11 | 3 100 |
| 7 | ×1.21 | 0/3/1 | 0/3/1 | 0/3/1 | — | 12 | 4 377 |
| 8 | ×1.29 | 0/3/0 | 0/2/0 | 1/2/0 | — | 8 | 2 117 |
| 9 | ×1.38 | 0/3/0 | 0/4/0 | 0/3/1 | — | 11 | 3 800 |
| **10** | ×1.72 | 0/3/1 | 0/3/1 | 0/3/1 | `o_capitao`@`central` | 13 | 8 755 |
| 11 | ×1.85 | 0/3/1 | 0/2/2 | 1/2/2 | — | 13 | 8 102 |
| 12 | ×1.97 | 0/4/1 | 0/4/1 | 0/4/2 | — | 16 | 9 552 |
| 13 | ×2.11 | 0/4/2 | 0/4/2 | 0/4/1 | — | 17 | 11 390 |
| 14 | ×2.26 | 0/5/1 | 0/5/2 | 0/4/2 | — | 19 | 13 173 |
| **15** | ×2.94 | 0/4/1 | 0/4/1 | 0/4/2 | `o_capitao`@`este` + `o_capitao`@`oeste` | 18 | 19 812 |
| 16 | ×3.14 | 0/4/2 | 0/4/2 | 0/4/1 | — | 17 | 16 932 |
| 17 | ×3.36 | 0/5/1 | 0/5/2 | 0/4/2 | — | 19 | 19 605 |
| 18 | ×3.60 | 0/5/2 | 0/6/2 | 0/6/2 | — | 23 | 25 338 |
| 19 | ×3.85 | 0/5/3 | 1/5/3 | 0/5/2 | — | 24 | 30 026 |
| **20** | ×4.85 | 0/8/2 | 0/8/3 | 1/7/3 | `o_capitao`@`este` + `o_capitao`@`central` | 34 | 56 323 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m07 -->

---

## 1. Ý đồ thiết kế

Ba cổng vào sân — Oeste, Central, Este — mỗi cổng một tuyến riêng, ba tuyến chạy song song xuống dưới rồi **chỉ gộp ở 3.0 u cuối** (12 % quãng đường) thành một tiền sảnh trước cầu môn.

Điểm khác biệt duy nhất so với M04 và M03: **lệch nhịp**. Không đồng thời (M04), không luân phiên hẳn (M03), mà **so le theo giây**. Tuyến Oeste ra trước, Central ra sau 5.6 s, Este ra sau 11.2 s. Ba đợt gối lên nhau một phần, nên tại bất kỳ khoảnh khắc nào cũng có **1.5–2 tuyến đang nóng, không bao giờ đủ ba**.

Hệ quả chiến thuật: người chơi **có thể** dùng chung tướng cho hai mặt trận — nhưng chỉ nếu tướng đó đứng ở ô canh-hai-tuyến, và chỉ nếu độ lệch nhịp đủ để nó bắn xong đợt này rồi kịp quay sang đợt kia. Đó là lý do bảng lệch nhịp **siết dần theo act** (5.6 s → 4.2 s → 2.8 s): cái mẹo "một tướng gánh hai cửa" đang sống ở act 1 sẽ chết ở act 3.

Map có **nhiều ô nhất game (13)** nhưng phải chia cho **ba** mặt trận — trung bình 4.3 ô/tuyến, ít hơn map một tuyến (11 ô/tuyến). Nhiều ô hơn nhưng nghèo hơn: đó là câu đùa của map này.

---

## 2. Toạ độ waypoint

Nội suy `catmull-rom`. Ba tuyến **dùng chung tiền sảnh** kể từ `(0.0, −7.30)` (khai bằng `mergeInto`, xem §8.1).

### Tiền sảnh chung `VESTIBULO` — 3 waypoint · **3.0 u**

| # | x | y |
|---|---|---|
| V1 | 0.00 | -7.30 | ← ba tuyến gộp |
| V2 | -0.90 | -8.40 |
| V3 | 0.00 | -9.13 | ← cầu môn |

### `oeste` — 9 waypoint nhánh + VESTIBULO · **24.40 u**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 1 | -4.20 | 9.36 | | 6 | -4.40 | -2.40 |
| 2 | -4.60 | 6.80 | | 7 | -2.60 | -4.20 |
| 3 | -2.60 | 5.00 | | 8 | -3.00 | -6.20 |
| 4 | -4.40 | 2.60 | | 9 | -1.50 | -6.90 |
| 5 | -3.60 | 0.20 | | → | *VESTIBULO* | |

### `central` — 8 waypoint nhánh + VESTIBULO · **22.12 u**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 1 | 0.00 | 9.36 | | 5 | 0.20 | 0.20 |
| 2 | -0.60 | 7.00 | | 6 | 1.20 | -2.20 |
| 3 | -1.00 | 4.60 | | 7 | -0.60 | -4.00 |
| 4 | 1.40 | 2.40 | | 8 | -0.20 | -5.90 |
| | | | → | *VESTIBULO* | |

### `este` — 9 waypoint nhánh + VESTIBULO · **23.52 u**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 1 | 4.20 | 9.36 | | 6 | 3.60 | -2.40 |
| 2 | 4.60 | 6.80 | | 7 | 2.20 | -4.60 |
| 3 | 2.80 | 5.20 | | 8 | 3.00 | -6.20 |
| 4 | 3.60 | 2.60 | | 9 | 1.50 | -6.90 |
| 5 | 4.40 | 0.20 | | → | *VESTIBULO* | |

### Kết quả kiểm hình học

| Chỉ số | oeste | central | este |
|---|---|---|---|
| Độ dài spline | **24.40 u** | **22.12 u** | **23.52 u** |
| Hộp bao X | [-4.67, 0.01] | [-1.10, 1.42] | [-0.90, 4.66] |
| Hộp bao Y | [-9.13, 9.36] | [-9.13, 9.36] | [-9.13, 9.36] |
| Tràn viewport | KHÔNG | KHÔNG | KHÔNG |
| Tiền sảnh bắt đầu tại f | 0.891 | 0.880 | 0.887 |
| Khe hở dài nhất | 14 % (3.4 u) f=0.42→0.56 | 12 % (2.6 u) f=0.48→0.60 | 11 % (2.5 u) f=0.48→0.59 |
| Độ phủ (tầm Lv1 1.4) | 46 % | 51 % | 49 % |

**Quan hệ ba tuyến:** mỗi cặp chồng nhau đúng **3.0 u** = chiều dài tiền sảnh. Trước tiền sảnh ba tuyến **không bao giờ trùng**, nhưng có **bốn chỗ thắt** (pinch) nơi hai tuyến lại gần nhau ≤ 2.6 u, cho phép một ô canh hai tuyến:

| chỗ thắt | y | cặp tuyến | ô đặt |
|---|---|---|---|
| Thắt 1 | ≈ +5.2 | oeste ↔ central | `f02` |
| Thắt 2 | ≈ +3.0 | central ↔ este | `f06` |
| Thắt 3 | ≈ −3.2 | central ↔ este | `f08` |
| Thắt 4 | ≈ −6.0 | oeste ↔ central | `f12` |

**Thời gian đi hết tuyến** (`adepto` 1.0): oeste **24.4 s** · central **22.1 s** · este **23.5 s**.

---

## 3. Toạ độ ô đặt tướng

13 ô sân + 1 ô thủ môn. `chord` cộng cả ba tuyến.

| ô | x | y | d.oeste | d.central | d.este | chord@1.0 | chord@1.2 | chord@1.4 | tướng Lv1 với tới | tuyến canh |
|---|---|---|---|---|---|---|---|---|---|---|
| `f01` | -3.41 | 6.85 | 0.88 | 2.63 | 6.43 | 1.03 | 1.88 | 2.81 | Batigol, D10S, Árbitro, Pulga | oeste |
| `f02` | -1.90 | 5.20 | 0.72 | 0.80 | 4.68 | 1.93 | 2.85 | **3.70** | Batigol, D10S, Árbitro, Pulga | **oeste+central** |
| `f03` | -2.90 | 2.01 | 1.31 | 3.11 | 6.24 | 0.00 | 0.00 | 1.04 | Pulga | oeste |
| `f04` | -3.10 | -2.78 | 0.70 | 2.78 | 5.48 | 1.44 | 1.99 | 2.58 | Batigol, D10S, Árbitro, Pulga | oeste |
| `f05` | 0.57 | 7.18 | 3.78 | 1.10 | 2.92 | 0.00 | 0.97 | 1.73 | D10S, Árbitro, Pulga | central |
| `f06` | 2.30 | 3.00 | 5.25 | 1.06 | 1.05 | 0.00 | 1.87 | **3.10** | D10S, Árbitro, Pulga | **central+este** |
| `f07` | 0.34 | 1.76 | 4.16 | 0.71 | 3.35 | 1.45 | 2.09 | 3.30 | Batigol, D10S, Árbitro, Pulga | central |
| `f08` | 1.90 | -3.20 | 4.47 | 1.09 | 0.93 | 0.67 | 2.14 | **3.24** | Batigol, D10S, Árbitro, Pulga | **central+este** |
| `f09` | 3.77 | 7.26 | 6.74 | 4.18 | 0.88 | 1.47 | 2.55 | 3.19 | Batigol, D10S, Árbitro, Pulga | este |
| `f10` | 3.10 | 0.10 | 6.70 | 2.48 | 1.30 | 0.00 | 0.00 | 1.47 | Pulga | este |
| `f11` | 4.00 | -6.66 | 4.05 | 3.98 | 1.10 | 0.00 | 0.47 | 0.94 | D10S, Árbitro, Pulga | este |
| `f12` | -1.30 | -6.00 | 0.92 | 1.09 | 1.84 | 0.81 | 2.75 | **4.08** | Batigol, D10S, Árbitro, Pulga | **oeste+central** |
| `f13` | 0.30 | -6.40 | 0.90 | 0.37 | 0.74 | 3.52 | 4.93 | **6.31** | Batigol, D10S, Árbitro, Pulga | **cả BA tuyến** |
| `gk01` | 0.60 | -8.40 | 0.94 | 0.94 | 0.94 | — | — | 7.70 | Dibu | cả BA tuyến |

**Thang tầm Lv1:** Batigol 8/13 · D10S 11/13 · Árbitro 11/13 · La Pulga 13/13.
(`f05`, `f06`, `f11` không dùng được Batigol Lv1 · `f03`, `f10` chỉ La Pulga.)

**Ràng buộc đã kiểm — tất cả ĐẠT:** không ô chết · không ranh giới dao cạo trên **bất kỳ** tuyến nào · hai ô gần nhất `f12`–`f13` = 1.65 u = **60 pt** > 48 pt · vùng chạm nằm trọn trong khung · Batigol không với tới mọi ô.

### Đọc bảng

- **`f13` là ô đắt giá nhất trong cả bốn map**: chord 6.31 u vì nó canh cả ba tuyến ở ngay cửa tiền sảnh. Nhưng nó ở f ≈ 0.87 của mọi tuyến — quân đã đi 87 % đường mới tới đó. **Nó là lưới an toàn, không phải hàng phòng ngự.**
- Ba ô hai-tuyến (`f02`, `f06`, `f08`, `f12`) là chỗ duy nhất mà "một tướng gánh hai cửa" khả thi — và đây là nơi bảng lệch nhịp ở §5 tấn công trực tiếp.
- `f03` (chord 1.04) và `f11` (chord 0.94) là hai ô yếu nhất map: xa đường, một tuyến, chỉ La Pulga (hoặc D10S) với tới. Chúng tồn tại để **bịt khe hở**, không để gây sát thương.

**Ô thủ môn `gk01`** cách cả ba tuyến 0.94 (tiền sảnh chung) — chord 7.70 ở Lv1, **10.86 ở Lv3**. Dibu ở M07 chặn 100 % quân của cả ba cửa. Đây là ô thủ môn giá trị nhất trong lô này, và là lý do M07 vẫn thắng được dù chia ba mặt trận.

---

## 4. Bảng phủ synergy

| cặp ô | đoạn chồng | ở tuyến | cặp tướng được thưởng |
|---|---|---|---|
| `f12` + `f13` | **1.97 u** (oeste 0.52 + central 1.45) | oeste, central | **Cụm tiền sảnh.** Árbitro `f12` × La Pulga Lv2 `f13` (+25 %) · hoặc D10S Lv2 `f13` (lan 1.7) ăn cả ba luồng |
| `f06` + `f07` | 1.25 u | central | Nguồn chậm `f07` × sát thương `f06` — cụm duy nhất phủ được **cả central lẫn este** |
| `f01` + `f02` | 0.31 u | oeste | Cụm đầu tuyến Oeste — tuyến ra sớm nhất mỗi wave |
| `f13` + `gk01` | tiếp giáp | cả ba | Dibu chặn × mọi thứ ở `f13` |

**Ô trục: `f13` + `f12`.** Đây là điểm duy nhất trên map mà một Árbitro làm chậm quân của **cả ba** tuyến. Nhưng nó nằm ở f ≈ 0.87: mọi thứ xảy ra ở đó đều là phút chót.

⚠️ **Cái bẫy trung tâm của M07.** Cụm `f12`+`f13`+`gk01` mạnh đến mức nó **thắng được act 1 và phần lớn act 2 một mình**. Người chơi tối ưu sẽ dồn tiền vào đó. Rồi act 3 tới với lệch nhịp siết còn 2.8 s: ba đợt quân gần như chập lại, tất cả đổ vào tiền sảnh 3.0 u trong cùng 5 giây, và **thời gian phơi nhiễm ở tiền sảnh chỉ 3.0 s với `adepto`, 2.1 s với `tifoso`**. Không cụm nào xử lý nổi 47 con trong 2 giây. Cấu trúc y hệt cái bẫy của M05, nhưng ở M07 nó ngặt hơn gấp bốn (tiền sảnh 12 % đường, thân chung của M05 là 43 %).

⚠️ **Trục thượng nguồn — cách chơi đúng.** Thẻ vàng của Árbitro là **vĩnh viễn** (FM-16). Ba Árbitro Lv1 ở `f02` (oeste+central), `f06` (central+este) và `f04` (oeste) phủ **cả ba** tuyến ở nửa trên bản đồ. Quân mang thẻ đi tiếp 60 % quãng đường còn lại trong trạng thái chậm 50 % → mọi La Pulga Lv2 ở hạ nguồn ăn +25 % mà không cần chồng vùng phủ. Đây là lối chơi mà bảng ô của M07 thưởng.

⚠️ **D10S Lv3 trên map ba tuyến.** Buff +20 % của nó áp cho **mọi tướng** trên **mọi tuyến** miễn mục tiêu đang bị chậm — nhưng chính nó mất khả năng làm chậm. Với ba mặt trận, một D10S Lv3 mà chỉ có một Árbitro là công thức để hai tuyến kia không bao giờ hưởng buff.

---

## 5. Bảng 20 wave + lịch lệch nhịp

### 5.1 Lịch lệch nhịp (theo giây, tính từ đầu wave)

| act | wave | `oeste` | `central` | `este` | giãn cách quân đầu tiên tới cầu môn |
|---|---|---|---|---|---|
| **1** | W1–W7 | **+0.0 s** | **+5.6 s** | **+11.2 s** | 24.4 s / 27.7 s / 34.7 s — giãn **10.3 s** |
| **2** | W8–W14 | **+0.0 s** | **+4.2 s** | **+8.4 s** | 24.4 s / 26.3 s / 31.9 s — giãn **7.5 s** |
| **3** | W15–W20 | **+0.0 s** | **+2.8 s** | **+5.6 s** | 24.4 s / 24.9 s / 29.1 s — giãn **4.7 s** |

Nhịp spawn trong một tuyến: **0.7 s** (giống toàn game). Offset là bội số nguyên của 0.7 s (8 / 6 / 4 tick) để lịch tất định, dễ test.

**Vì sao siết theo act:** ở act 1, 10.3 giây đủ để một tướng ở ô hai-tuyến bắn xong đợt Oeste rồi quay sang đợt Central. Ở act 3, 4.7 giây thì không. Cùng một bảng ô, cùng một đội hình, **giá trị của ô hai-tuyến giảm dần theo trận** — người chơi phải xây lại, đúng ý đồ act 3 của `waves.json` ("ô đặt hết, phải bán và xây lại").

### 5.2 Bảng quân

| W | oeste (A/T/Tb) | central (A/T/Tb) | este (A/T/Tb) | tổng | HP mỗi `adepto` | HP wave | boss |
|---|---|---|---|---|---|---|---|
| 1 | 4/0/0 | 4/0/0 | 2/0/0 | 10 | 67 | 670 | — |
| 2 | 6/0/0 | 6/0/0 | 4/0/0 | 16 | 73 | 1 168 | — |
| 3 | 4/1/0 | 4/2/0 | 4/2/0 | 17 | 80 | 1 831 | — |
| 4 | 4/2/0 | 4/4/0 | 4/4/0 | 22 | 87 | 2 950 | — |
| 5 | 4/2/0 | 4/3/0 | 2/2/1 | 18 | 95 | 2 922 | — |
| 6 | 4/2/0 | 4/4/0 | 4/4/1 | 23 | 103 | 4 072 | — |
| 7 | 4/2/2 | 4/4/1 | 2/4/1 | 24 | 112 | 6 068 | — |
| 8 | 4/2/0 | 4/3/0 | 2/2/1 | 18 | 122 | 3 785 | — |
| 9 | 4/2/0 | 4/4/0 | 4/4/1 | 23 | 134 | 5 273 | — |
| 10 | 4/2/2 | 4/4/1 | 2/4/1 | 24 | 146 | 7 858 | **`este`** |
| 11 | 4/3/2 | 4/5/1 | 4/3/1 | 27 | 159 | 9 231 | — |
| 12 | 4/3/2 | 4/5/1 | 4/4/2 | 29 | 173 | 11 393 | — |
| 13 | 4/4/2 | 4/7/1 | 4/5/2 | 33 | 188 | 14 077 | — |
| 14 | 6/4/3 | 6/7/1 | 4/5/2 | 38 | 205 | 17 295 | — |
| 15 | 4/3/2 | 4/5/1 | 4/4/2 | 29 | 224 | 14 755 | — |
| 16 | 4/4/2 | 4/7/1 | 4/5/2 | 33 | 244 | 18 230 | — |
| 17 | 6/4/3 | 6/7/1 | 4/5/2 | 38 | 266 | 22 398 | — |
| 18 | 6/4/3 | 6/8/1 | 4/6/3 | 41 | 290 | 27 284 | — |
| 19 | 6/5/4 | 6/9/2 | 6/5/4 | 47 | 316 | 36 282 | — |
| 20 | 8/6/5 | 8/11/2 | 6/7/4 | 57 | 344 | 46 610 | **`central`** |

**Tính cách từng cửa (cố định cả 20 wave):**
- **`oeste`** (24.40 u — dài nhất, ra **trước** nhất): 45 % `tambor`. Quân chậm + tuyến dài + ra sớm = mặt trận "chậm mà chắc", thưởng cho sát thương dồn.
- **`central`** (22.12 u — ngắn nhất): 45 % `tifoso`. Nhanh nhất, ngắn nhất, ra thứ hai — cửa nguy hiểm nhất.
- **`este`** (23.52 u — ra **sau** cùng): hỗn hợp. Ra muộn nên người chơi có thể "mượn" tướng từ hai cửa kia ở act 1 — và không mượn được nữa ở act 3.

**Boss `o_capitao` — kháng chậm 85 %** (cao nhất trong bốn map). Cap chậm thực tế = 70 % × 15 % = **10.5 %**. Ba Árbitro phủ ba tuyến gần như không làm gì được nó.

| | wave | tuyến | HP đề xuất | thưởng | lý do |
|---|---|---|---|---|---|
| Boss 1 | 10 | `este` (23.52 u) | **1 400** | 200 | Cửa ra muộn nhất — người chơi có 8.4 s để chuẩn bị sau khi thấy hai cửa kia. Boss dạy |
| Boss 2 | 20 | `central` (22.12 u) | **5 100** | 500 | Tuyến **ngắn nhất**, phơi nhiễm ít nhất, và là cửa mà 45 % `tifoso` đã bắt người chơi tiêu tiền suốt trận |

> HP suy từ tỉ lệ độ dài tuyến so với 42.28 u của map gốc. **Ngoại suy bậc nhất — phải đo lại bằng test 20 wave chạy engine thật.**

---

## 6. Kinh tế đề xuất

| Mục | Giá trị |
|---|---|
| `startingCash` | **730** |
| `goalHealth` | 20 |
| Số ô sân | **13** |
| **Trần chi tiêu** = 13 × 1 020 + 476 | **13 736** |
| Tiền cả đời một trận | **13 345** |
| Biên | **+2.8 %** ✅ |

Phân rã: 730 khởi đầu + 10 050 thưởng hạ quân + 1 385 clear + 480 skip + 700 boss.

`startingCash` **730** cho phép mua đúng **ba** Batigol Lv1 (360) + một D10S (240) = 600, dư 130 — tức **một chốt mỗi cửa cộng một tướng mạnh ở đâu đó**. Ở 700 thì không đủ (600 + nâng 96 = 696, sát nút một cách khó chịu); ở 780 thì mua được ba Batigol + Árbitro + còn tiền, tức quyết định mở màn hết là quyết định. **730 là con số ép người chơi bỏ trống một cửa ở W1** — và cửa `este` ra muộn 11.2 s chính là cửa hợp lý để bỏ trống. Kinh tế và lệch nhịp dạy cùng một điều.

⚠️ **`tools/balance_sim.py` không dùng được.** Nó đã báo sai trên map một tuyến (headroom trung bình 2.09 trong khi engine thật thắng còn 11/20 máu) và không có khái niệm tuyến hay offset thời gian. Mô hình hoả lực tập trung ở `docs/04 §3.3` giả định **một** con đường; với ba tuyến lệch nhịp nó sai theo hướng không đoán được.

**Đích cần đo:**
1. Headroom mỗi wave trong [1.05, 1.65].
2. **Đo riêng ba build:** "pháo đài tiền sảnh" (`f12`+`f13`+`gk01` max), "ba mặt trận đều", "hai mặt trận + bỏ `este`". Build pháo-đài phải THẮNG act 1–2 và **THUA từ W16** — nếu nó thắng cả trận thì tiền sảnh quá dài, rút `VESTIBULO` từ 3.0 u xuống ~2.2 u.
3. Đo hiệu quả của lệch nhịp: số quân trên sân cùng lúc ở W7 (act 1, giãn 10.3 s) so với W17 (act 3, giãn 4.7 s). Kỳ vọng đỉnh W17 cao hơn W7 ≥ 40 % dù tổng quân chỉ hơn 58 %.
4. Dư tiền tối đa < 400 Peso.

---

## 7. Đường thắng dự kiến & build cố ý thua

### Đường thắng

| Giai đoạn | Nước đi | Lý do |
|---|---|---|
| Kickoff (730) | Batigol `f02` (120) + Batigol `f08` (120) + D10S `f13` (240) — dư 250 | `f02` và `f08` mỗi ô canh **hai** tuyến; `f13` canh cả ba. Ba tướng, sáu "suất canh cửa" |
| W3–W6 | Nâng `f13` → Lv2 (192, lan 1.7) ; Batigol `f09` (120) cho `este` khi nó bắt đầu ra | Act 1 giãn 10.3 s — ba tướng vẫn kịp gánh cả ba cửa |
| W7–W10 | **El Árbitro `f12`** (240) ; nâng `f02` → Lv2 | `f12`+`f13` chồng 1.97 u → cụm tiền sảnh thành thật. Boss W10 ở `este`: `f08`/`f09` gánh |
| W11–W14 | La Pulga `f07` (300) ; Batigol `f04` (120) cho `oeste` | Act 2 siết còn 7.5 s — bắt đầu phải có tướng riêng cho từng cửa |
| W15–W18 | **Bán `f03` / `f10` / `f11` nếu đã lỡ mua** (chord 1.04 / 1.47 / 0.94) ; thêm Árbitro thứ hai ở `f06` | Act 3 giãn còn 4.7 s: cụm tiền sảnh một mình không kịp. Phải đánh chặn ở thượng nguồn bằng thẻ vàng |
| W19–W20 | Dibu Lv2/Lv3 `gk01` (chord 10.86 ở Lv3) ; La Pulga Lv3 ở `f07` hoặc `f13` | Boss W20 đi `central` **một mình**: cần đơn mục tiêu, không cần AoE |

### Build CỐ Ý thua

**"Pháo đài tiền sảnh"** — dồn toàn bộ vào `f12`, `f13`, `gk01`, cộng `f11` và `f10` cho đẹp; không đặt gì ở nửa trên bản đồ.

Vì sao thua, theo thứ tự:
1. **W1–W13 nó thắng thoải mái.** `f13` chord 6.31 + `f12` chord 4.08 + Dibu chord 7.70 = quá đủ cho HP wave < 15 000. Đây là điều làm nó nguy hiểm: người chơi được **thưởng** vì chọn sai suốt 13 wave.
2. **Act 3 siết lệch nhịp.** Từ W15, giãn cách còn 4.7 s. Ba đợt quân chập lại ở tiền sảnh 3.0 u. Thời gian phơi nhiễm: 3.0 s (`adepto`), **2.1 s** (`tifoso`). Với 45 % `tifoso` ở `central`, phần lớn quân qua tiền sảnh trong hai giây.
3. **Không có nguồn chậm ở thượng nguồn** → quân tới `f12` với 100 % máu. `f11` (chord 0.94) và `f10` (chord 1.47) không bào được gì.
4. **`tambor` xuyên thủng.** W20 có 11 `tambor` (1 892 máu mỗi con, lọt trừ **2** máu). Sáu con lọt = mất 12/20 máu cầu môn.
5. **Boss W20 kết liễu.** 5 100 máu, kháng chậm 85 % (Árbitro chỉ kéo dài 10.5 %), đi `central` một mình. AoE của `f13` Lv2 đánh một mục tiêu là AoE tệ nhất game; Dibu không gây sát thương.

---

## 8. Yêu cầu kỹ thuật

M07 là map **tốn kém nhất** về kỹ thuật trong bốn map: nó cần đa tuyến (như M04), đuôi chung (như M05), **và** thêm lệch nhịp theo tuyến theo act — cái mà không map nào khác cần.

### 8.1 Schema `config/path.json`

```jsonc
"paths": [
  { "id": "oeste",  "lengthUnits": 24.40, "spawnPoint": {"x": -4.2, "y": 9.36},
    "goalPoint": {"x": 0.0, "y": -9.13}, "waypoints": [ /* 9 nhánh + 3 VESTIBULO */ ] },
  { "id": "central","lengthUnits": 22.12, "spawnPoint": {"x": 0.0, "y": 9.36},
    "goalPoint": {"x": 0.0, "y": -9.13}, "waypoints": [ /* 8 nhánh */ ],
    "mergeInto": { "pathId": "oeste", "atPoint": {"x": 0.0, "y": -7.30} } },
  { "id": "este",   "lengthUnits": 23.52, "spawnPoint": {"x": 4.2, "y": 9.36},
    "goalPoint": {"x": 0.0, "y": -9.13}, "waypoints": [ /* 9 nhánh */ ],
    "mergeInto": { "pathId": "oeste", "atPoint": {"x": 0.0, "y": -7.30} } }
]
```

### 8.2 Schema `config/waves.json` — LỆCH NHỊP

```jsonc
{ "wave": 17,
  "spawns": { "oeste":   {"adepto": 6, "tifoso": 4, "tambor": 3},
              "central": {"adepto": 6, "tifoso": 7, "tambor": 1},
              "este":    {"adepto": 4, "tifoso": 5, "tambor": 2} },
  "laneOffsetsSec": { "oeste": 0.0, "central": 2.8, "este": 5.6 },
  "boss": null }
```

**Luật bổ sung cần ghi vào `waves.json`:**
- `laneOffsetsSec` phải là **bội số của `spawnIntervalSec` (0.7)** — nếu không, lịch spawn hết tất định và test phải mô phỏng thay vì so mốc.
- Ít nhất một tuyến phải có offset `0.0` (mốc gốc của wave).
- Boss ra **sau khi toàn bộ quân thường của MỌI tuyến đã ra** — giữ nguyên giả định nền của `docs/04 §3.3`; công thức thời điểm boss = `max(offset_l + (n_l − 1) × 0.7)` trên mọi tuyến, cộng một khoảng `0.7`.

### 8.3 Code phải đổi

Toàn bộ danh sách ở **M04 §8.3** (đa tuyến) + **M05 §8.3** (`mergeInto`), **cộng thêm**:

| File | Đổi gì |
|---|---|
| `WaveSpawner.Schedule()` | Sinh lịch từng tuyến, **cộng `laneOffsetsSec[lane]` vào `TimeSec`**, rồi trộn ba danh sách theo `TimeSec` tăng dần. `InterleaveOrder` chạy trong phạm vi một tuyến (không được trộn ba tuyến rồi mới xen kẽ — làm thế thì tỉ lệ loại quân mỗi tuyến sai) |
| `WaveSpawner` — boss | Thời điểm boss phải tính theo **tuyến muộn nhất**, không phải theo `t` tích luỹ của một tuyến |
| `ConfigModel.WaveDef` | Thêm `IReadOnlyDictionary<string,double> LaneOffsetsSec` |
| `ConfigValidator.cs` | Luật mới: (a) `laneOffsetsSec` là bội của `spawnIntervalSec`; (b) có ít nhất một offset = 0; (c) mọi `laneId` trong `spawns`/`laneOffsetsSec`/`boss.lane` tồn tại trong `paths` |
| `Pitch.cs:98` | Ba tuyến, ba màu; **vẽ tiền sảnh một lần** (vẽ ba lần thì dày gấp ba). Ba cổng vào ở mép trên phải có biển tên đọc được |
| `Hud.cs` | Cần chỉ báo "cửa nào đang ra quân" — không có nó, lệch nhịp là cơ chế vô hình |
| `core/.../WaveSpawnerTests.cs` | Test hiện so mốc thời gian tuyệt đối. Phải thêm bộ test cho offset: `Schedule(17)` phải có phần tử đầu của `este` tại `t = 5.6` |
| `tools/path_check.py` | Thêm: (a) d tới **từng** tuyến; (b) ranh-giới-dao-cạo kiểm trên **mọi** tuyến; (c) khe hở tính riêng mỗi tuyến; (d) đếm ô canh ≥ 2 tuyến (M07 kỳ vọng **5**, gồm cả `gk01`) |

### 8.4 KHÔNG cần đổi
`TargetingSystem`, `CombatResolver`, `DamageSystem`, `SlowStack`, `AbilityEngine`, `EconomyService`, `GoalHealth` — không một dòng. Ba tuyến chung một cầu môn nên máu cầu môn vẫn là một biến.

---

## 9. Rủi ro

| # | Rủi ro | Mức | Giảm thiểu |
|---|---|---|---|
| R1 | **Lệch nhịp là cơ chế vô hình.** Người chơi không có cách nào biết `este` sẽ ra sau 11.2 s. Không có chỉ báo thì M07 chỉ giống "M04 nhưng ba tuyến" | **CAO** | Bắt buộc có UI: ba biểu tượng cổng ở mép trên, đếm ngược tới lượt ra quân của từng cửa. Đây là **điều kiện tiên quyết**, không phải tính năng phụ |
| R2 | **Không đo được bằng `balance_sim.py`** — nó không biết tuyến, càng không biết offset | CAO | Chỉ chấp nhận số từ test 20 wave chạy `MatchController` thật, mỗi build một lần |
| R3 | Tiền sảnh 3.0 u (12 %) có thể **vẫn đủ** để pháo-đài-tiền-sảnh thắng cả trận | CAO | Đo build-thua ở §7. Nếu nó thắng W20 → rút `VESTIBULO` xuống ~2.2 u bằng cách dời điểm gộp từ `(0, −7.30)` xuống `(0, −8.00)`. **Đừng** nâng HP boss để bù — làm thế chỉ trừng phạt build đúng |
| R4 | 13 ô chia ba tuyến → độ phủ mỗi tuyến chỉ 46–51 % (map gốc 68 %). Khe hở `oeste` 14 % sát trần 15 % | TRUNG BÌNH | Đã đo và trong ngưỡng. Nếu playtest thấy rò quá → giảm `tambor` ở `oeste` trước, đừng thêm ô (thêm ô sẽ đẩy trần chi tiêu lên và phá §6) |
| R5 | Ba tuyến ba màu + quân ba màu trên màn hình dọc 393 pt → **quá tải thị giác** ở W19–W20 (47 con) | TRUNG BÌNH | Test đọc-hiểu trên máy thật ở W19. Nếu rối, giảm độ tương phản đường và giữ màu cho quân |
| R6 | Số quân trên sân cùng lúc ở act 3 cao nhất trong bốn map | TRUNG BÌNH | Đo `Enemies.Count` đỉnh ở W19–W20 trước khi chốt; nếu vượt ngân sách frame thì **nới lệch nhịp act 3 từ 2.8 s lên 3.5 s**, đó là cần chỉnh rẻ nhất |
| R7 | `f03` (chord 1.04), `f10` (1.47), `f11` (0.94) là ba ô rất yếu — nguy cơ thành nội dung chết | TRUNG BÌNH | Cố ý làm ô "bịt khe hở, không gây sát thương". Nếu telemetry cho thấy < 5 % người chơi từng mua → kéo `f11` từ d 1.10 về 0.88 và `f03` từ 1.31 về 1.10 |
| R8 | Kháng chậm 85 % làm El Árbitro gần như vô dụng trước boss (chỉ 10.5 %) trong khi map lại **ép** mua nhiều Árbitro cho ba tuyến | TRUNG BÌNH | Đó là đánh đổi có chủ ý: Árbitro giỏi ở đám đông, kém trước boss. Nhưng phải đo — nếu người chơi buộc phải bán hết Árbitro trước W20 thì 85 % quá cao, hạ về 82 % |
