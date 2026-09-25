# M02 — La Bombonera

> **Trạng thái:** 🟡 Plan. Hình học ĐÃ ĐO bằng bản copy `tools/path_check.py`; số cân bằng là **ĐÍCH CẦN ĐO**.
> **Schema:** theo `docs/08-MAPS-ARCHITECTURE.md` §2 — `lanes` là MẢNG (1 phần tử), `spawns` là MẢNG `[{enemy,count,lane}]`, `fieldSlots` suy từ `slots`.
> 1 tuyến · **10 ô sân** + 1 ô thủ môn · boss kháng chậm 75% · độ khó ★★

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m02 -->
**Nguồn: `config/maps/m02-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 1 — `L1` 44.5u |
| Ô sân / thủ môn | 10 / 1 |
| Σchord@1.4 mỗi ô | 2.90 |
| Tiền khởi đầu | 700 |
| Máu quái | ×0.67 × 1.12^(W−1) → W20 ×5.77 |
| Tổng quân 20 wave | 400 |

| W | ×máu | `L1` | boss | quân | tổng máu |
|---|------|------|------|------|----------|
| 1 | ×0.67 | 7/0/0 | — | 7 | 469 |
| 2 | ×0.75 | 11/0/0 | — | 11 | 825 |
| 3 | ×0.84 | 9/3/0 | — | 12 | 1 311 |
| 4 | ×0.94 | 9/7/0 | — | 16 | 2 295 |
| 5 | ×1.05 | 7/5/1 | — | 13 | 2 475 |
| 6 | ×1.18 | 9/7/1 | — | 17 | 3 531 |
| 7 | ×1.32 | 7/7/3 | — | 17 | 5 142 |
| 8 | ×1.48 | 7/5/1 | — | 13 | 3 481 |
| 9 | ×1.66 | 9/7/1 | — | 17 | 4 961 |
| **10** | ×1.86 | 7/7/3 | `o_capitao`@`L1` | 18 | 9 731 |
| 11 | ×2.08 | 9/8/3 | — | 20 | 8 971 |
| 12 | ×2.33 | 9/9/3 | — | 21 | 10 560 |
| 13 | ×2.61 | 9/11/4 | — | 24 | 14 407 |
| 14 | ×2.92 | 11/11/4 | — | 26 | 16 717 |
| 15 | ×3.27 | 9/9/4 | — | 22 | 16 627 |
| 16 | ×3.67 | 9/10/4 | — | 23 | 19 441 |
| 17 | ×4.11 | 11/10/4 | — | 25 | 22 597 |
| 18 | ×4.60 | 11/12/5 | — | 28 | 29 854 |
| 19 | ×5.15 | 13/13/6 | — | 32 | 38 441 |
| **20** | ×5.77 | 15/17/7 | `o_capitao`@`L1` | 40 | 62 263 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m02 -->

---

## 1. Ý đồ thiết kế

M01 dạy "cua nào đáng đặt". M02 dạy thứ ngược lại và khó nuốt hơn: **đặt đủ mọi nơi là thua**.

Map có **một cái túi** — khúc đường gấp bốn lần liên tiếp ở nửa phải sân, và **4 ô nằm trong túi đó
(f04–f07) gánh 77% toàn bộ hoả lực khả dĩ của map** (Σ chord@1.2 = 16.9 / 22.0). Sáu ô còn lại cộng lại
chỉ bằng **một phần ba** một ô trong túi. Người chơi rải đều 10 ô sẽ có một hàng thủ trông đầy đặn trên màn
hình và thủng ở W12–W14.

Ba thứ M02 kiểm tra mà M01 không:
- **Chồng vùng phủ là tài nguyên.** Bốn ô trong túi phủ nối tiếp nhau **không hở** từ f=0.405 tới f=0.764 —
  một hành lang giết dài **15.9 unit** (36% đường). Chỗ chồng lấn giữa các cặp là nơi `el_arbitro`/`d10s`Lv1
  làm chậm và `d10s`Lv3/`la_pulga`Lv2 ăn thưởng trên **cùng một con quái**.
- **Nâng cấp lãi hơn mua thêm.** Trong túi, tăng tầm 1.2→1.6 làm vùng chồng lấn giữa hai ô kề nhau tăng từ
  0.58 u lên **1.49 u (+157%)**. Ngoài túi, cũng khoản tiền ấy chỉ mua thêm ~1.5 u chord.
- **Ít ô hơn ép chọn.** 10 ô (M01 có 11) và trần chi tiêu tụt từ 11 696 xuống **10 676** — biên chỉ còn 3.8%.

---

## 2. Đường chạy (lane `L1`)

| # | x | y | ghi chú |
|---|---|---|---|
| 0 | 0.00 | 9.30 | `spawnPoint` |
| 1 | -3.50 | 8.40 | |
| 2 | -4.35 | 5.90 | cua trái trên |
| 3 | -1.30 | 4.70 | |
| 4 | 2.90 | 4.10 | |
| 5 | 4.05 | 2.40 | **T1 — vào túi** |
| 6 | 0.70 | 0.70 | T2 |
| 7 | 3.95 | -1.00 | T3 |
| 8 | 0.70 | -2.70 | T4 |
| 9 | 3.80 | -4.40 | **T5 — ra túi** |
| 10 | 0.30 | -5.40 | |
| 11 | -3.30 | -6.00 | |
| 12 | -3.50 | -7.80 | |
| 13 | -1.20 | -8.60 | |
| 14 | 0.00 | -9.10 | `goalPoint` |

**Kết quả kiểm (bản copy `tools/path_check.py`):**

| Mục | Giá trị | Kết |
|---|---|---|
| Độ dài spline thật | **44.49 units** (khai 44.49, lệch 0.0%) | ✅ |
| Hộp bao | x [-4.5, 4.2] · y [-9.1, 9.3] | ✅ trong viewport |
| Ô chết | 0 | ✅ |
| Đoạn không ô nào canh | f=0.60→0.74 = 14% (6.4 u) | ✅ |
| Hai ô gần nhau nhất | f06–f07 = 1.73 u = **63 pt** | ✅ |
| chord/2R trung bình ở R=1.8 | **1.16** (map gốc 0.77) | ⚠️ mô hình docs/04 đánh giá THẤP phe thủ ~16% |
| Vòng tầm cắt tối đa mấy đoạn | **2** | ✅ cơ chế túi |

Túi chiếm f = **0.367 → 0.705** (33.8% quãng đường, 15.0 unit). Bốn khúc gấp cách nhau ~1.7 unit theo
phương dọc, tức ~1.56 unit theo phương vuông góc — vừa đủ để một ô đứng giữa hai nhánh ở d≈0.65 mà vẫn
**không nằm đè lên đường**.

---

## 3. Ô đặt tướng — 10 ô sân + 1 thủ môn

| ô | x | y | d | f | chord@1.0 | chord@1.2 | chord@1.4 | đoạn | tướng với tới **Lv1** | vai trò |
|---|---|---|---|---|---|---|---|---|---|---|
| f01 | -4.60 | 8.60 | 0.93 | 0.094 | 0.62 | 1.29 | 1.78 | 1 | batigol · d10s · árbitro · pulga | mở màn |
| f02 | -3.50 | 4.10 | 1.14 | 0.177 | 0.00 | 0.69 | 1.49 | 1 | d10s · árbitro · pulga | ô sườn trái |
| f03 | 3.60 | 4.80 | 0.94 | 0.322 | 0.61 | 1.29 | 1.79 | 1 | batigol · d10s · árbitro · pulga | cửa vào túi |
| **f04** | **1.90** | **0.70** | **0.66** | 0.480 | 3.07 | **4.36** | 4.85 | **2** | tất cả | 🟥 **TÚI 1** |
| **f05** | **2.80** | **-1.00** | **0.64** | 0.513 | 3.19 | **4.31** | 4.77 | 1 | tất cả | 🟥 **TÚI 2** |
| **f06** | **1.90** | **-2.70** | **0.65** | 0.596 | 3.04 | **4.33** | 4.84 | **2** | tất cả | 🟥 **TÚI 3** |
| **f07** | **2.20** | **-4.40** | **0.61** | 0.741 | 2.85 | **3.91** | 4.91 | **2** | tất cả | 🟥 **TÚI 4** |
| f08 | 2.10 | 2.90 | 1.32 | 0.405 | 0.00 | 0.00 | 1.45 | 2 | **chỉ la_pulga** | ô tầm xa — mở ra ở R≥1.8 (chord 5.35) |
| f09 | -4.20 | -5.60 | 0.90 | 0.877 | 0.62 | 1.16 | 1.60 | 1 | batigol · d10s · árbitro · pulga | đường về |
| f10 | -0.90 | -7.50 | 1.14 | 0.969 | 0.00 | 0.67 | 1.49 | 1 | d10s · árbitro · pulga | chốt cuối |
| gk01 | 0.70 | -8.30 | 1.06 | 1.000 | — | — | — | — | **chỉ dibu** | cách cầu môn **1.06** |

**Ô cố ý là "ô tầm xa" (Lv2+ mới dùng được với đa số tướng):** f02, f08, f10.
- f08 (d = 1.32) là ô **đầu tư dài hạn** duy nhất của map: ở Lv1 `la_pulga` chỉ phủ 1.45 u, nhưng ở tầm 1.8
  (`la_pulga` Lv3) nó phủ **5.35 u và cắt 2 đoạn đường** vì nó nhìn chéo qua miệng túi. Đây là ô thưởng cho
  người chơi biết nhìn xa — và là cái bẫy cho người mua nó ở W2.
- f02 và f10 là ô **hạ tầng**: chord thấp, tồn tại để vá hai đoạn đường mà túi không với tới.

### Phân bố sức mạnh — bằng chứng cho "rải đều là thua"

| nhóm | số ô | Σ chord@1.2 | % tổng |
|---|---|---|---|
| **Túi (f04–f07)** | 4 | **16.91** | **77%** |
| Ngoài túi (6 ô còn lại) | 6 | 5.10 | 23% |

Một ô trong túi ≈ **5.0 lần** một ô ngoài túi (4.23 vs 0.85 chord@1.2 trung bình).

---

## 4. Bảng phủ synergy — combo pocket

Bốn ô trong túi phủ **nối tiếp không hở** dọc đường. Khoảng cách giữa các ô kề nhau đều là **1.73–1.92 unit**
(63–70 pt) — đủ xa để chạm không nhầm, đủ gần để tầm 1.2 đã chồng nhau.

### 4a. Ở Lv1 (mọi ô đặt `d10s`/`el_arbitro`, R = 1.2)

| ô | đoạn đường phủ (f) | dài | chồng với ô kế | dài chồng |
|---|---|---|---|---|
| f04 | 0.405–0.503 | 4.36 u | f04×f05 tại f 0.490–0.503 | **0.58 u** |
| f05 | 0.490–0.587 | 4.31 u | f05×f06 tại f 0.574–0.587 | **0.58 u** |
| f06 | 0.574–0.672 | 4.33 u | f06×f07 tại f 0.650–0.672 | **0.98 u** |
| f07 | 0.650–0.690 + 0.717–0.764 | 3.91 u | — | — |
| **tổng** | **0.405 → 0.764** | **15.9 u = 36% đường** | | |

### 4b. Ở đội hình "đáp án" (f04 `el_arbitro` Lv2 1.4 · f05 `d10s` Lv3 1.6 · f06 `la_pulga` Lv2 1.6 · f07 `batigol` Lv3 1.4)

| cặp | đoạn chồng (f) | dài chồng | tăng so Lv1 | cặp tướng được thưởng |
|---|---|---|---|---|
| f04 × f05 | 0.480–0.508 | **1.26 u** | +117% | `el_arbitro` làm chậm → `d10s` Lv3 `ban_thang_the_ky` **+20% dame lên mục tiêu đang bị chậm** |
| f05 × f06 | 0.563–0.597 | **1.49 u** | +157% | `d10s` Lv3 + `la_pulga` Lv2 `nhan_quan` **+25% lên mục tiêu đang bị chậm** — cả hai ăn cùng một nguồn chậm từ f04 |
| f06 × f07 | 0.644–0.682 | **1.69 u** | +72% | `la_pulga` Lv2 + `batigol` Lv3 (nổ lan 1.4) dọn quân yếu, `la_pulga` `so_10` xuyên 2 quân phía sau |

🔴 **Đây là điểm mấu chốt của map.** `d10s` Lv3 mang cờ `requiresExternalSlowSource: true` — **nó KHÔNG tự làm
chậm được**. Nguồn chậm duy nhất khả dĩ là `el_arbitro` hoặc một `d10s` Lv1 khác. Túi ép hai thứ đó đứng cạnh
nhau: `el_arbitro` ở f04 với tầm Lv2 = 1.4 phủ f 0.399–0.508, và `d10s` Lv3 ở f05 phủ f 0.480–0.597 —
**mọi con quái đi qua f 0.480–0.508 đều đang bị chậm khi ăn đạn Lv3.** Ngoài túi, không cặp ô nào chồng nhau
ở bất kỳ cấp tầm nào.

⚠️ **Ràng buộc phải kiểm khi làm bảng cân bằng:** `el_arbitro` chiếm một trong 4 ô túi. Với chỉ 10 ô và trần
chi tiêu 10 676, đổi 1 ô túi lấy tiện ích là **quyết định đắt nhất map**. Nếu test cho thấy build không có
`el_arbitro` vẫn thắng dễ hơn build có → túi chưa đủ ép, phải nâng máu boss hoặc giảm chord của ô túi.

---

## 5. Bảng 20 wave

Tất cả trên lane `L1`. Boss spawn **sau cùng** trong wave.

| W | adepto | tifoso | tambor | boss | tổng sinh |
|---|---|---|---|---|---|
| 1 | 7 | 0 | 0 | | 7 |
| 2 | 11 | 0 | 0 | | 11 |
| 3 | 9 | 3 | 0 | | 12 |
| 4 | 9 | 7 | 0 | | 16 |
| 5 | 7 | 5 | 1 | | 13 |
| 6 | 9 | 7 | 1 | | 17 |
| 7 | 7 | 7 | 3 | | 17 |
| 8 | 7 | 5 | 1 | | 13 |
| 9 | 9 | 7 | 1 | | 17 |
| 10 | 7 | 7 | 3 | **✔** | 18 |
| 11 | 9 | 8 | 3 | | 20 |
| 12 | 9 | 9 | 3 | | 21 |
| 13 | 9 | 11 | 4 | | 24 |
| 14 | 11 | 11 | 4 | | 26 |
| 15 | 9 | 9 | 4 | | 22 |
| 16 | 9 | 10 | 4 | | 23 |
| 17 | 11 | 10 | 4 | | 25 |
| 18 | 11 | 12 | 5 | | 28 |
| 19 | 13 | 13 | 6 | | 32 |
| 20 | 15 | 17 | 7 | **✔** | 40 |

Bảng này = map gốc **giảm ~14%**, ép bởi trần chi tiêu 10 ô (xem §6). Máu giữ công thức
`hp = baseHp × 0.67 × 1.09^(wave-1)`.

**Chỗ vỡ dự kiến của build "rải đều":** W12–W14. W13 có 11 tifoso (tốc 1.4, máu ×2.05) đi thành dòng —
tướng Lv1 rải khắp map chỉ chạm mỗi con ~0.9 unit đường, trong khi cùng số tiền dồn vào túi chạm ~4.3 unit.

**Đỉnh quân sống cùng lúc — ước lượng:** map gốc đo 27 con ở W20 với 47 con sinh. M02 W20 sinh 40 (85%) trên
đường dài hơn 5.2%, nhưng túi giết tập trung ở giữa đường → ước **≈ 22–25 con**. Dưới trần **40**.
⚠️ Phải đo bằng `max(m.Enemies.Count)` trong test winnability.

---

## 6. Kinh tế đề xuất

| Mục | Giá trị | Nguồn |
|---|---|---|
| `startingCash` | **700** (điểm khởi đầu để quét) | map gốc đo được 700 |
| `goalHealth` | 20 | dùng chung |
| Số ô sân | **10** (suy từ `slots`) | |
| **TRẦN CHI TIÊU** | 10 × 1020 + 1 × 476 = **10 676** | `docs/08` §5 |
| Tiền cả trận (lý thuyết) | **10 271** = 700 + 7 730 thưởng quái + 1 385 clear + 456 skip | tính từ §5 |
| Biên dưới trần | **+405 (3.8%)** | ✅ luật #21 đạt |

🔴 **Đây là ràng buộc chặt nhất của map.** Bảng wave map gốc đặt nguyên vào M02 sẽ cho **11 491 > 10 676 =
vỡ trần 7.6%** — đúng cái lỗi vòng 20/22 mà `docs/08` §5 cảnh báo. Mỗi lần thêm quân vào M02 **phải** chạy
lại phép tính này.

**Đích headroom:** dải cứng [1.05, 1.65]; M02 là ★★ nên nhắm **[1.10, 1.45]**, kết trận mong đợi **còn
9–13/20 máu**. Ba số cần ĐO bằng engine thật (test winnability 20 wave, quét `startingCash` dò nhị phân —
`docs/08` §6.3), **không** chốt bằng `tools/balance_sim.py`: vòng 22 sim báo 2.09 trong khi engine thật
thắng còn 11/20.

---

## 7. Đường thắng dự kiến

**Build QUA ĐƯỢC — "khoá túi":**

| Giai đoạn | Hành động | Chi | Lý do |
|---|---|---|---|
| Kickoff | `batigol` f04 (120) + `batigol` f06 (120) + `d10s` f05 (240) | 480 | 3/4 ô túi có tướng ngay W1; batigol Lv1 (R=1.0) trong túi đã phủ 3.0 u |
| W3–W7 | `batigol` f07 (120); `d10s` f05 → Lv2 (192) | 312 | lan 1.7 quét cả hai nhánh trong túi |
| W8–W11 | **`el_arbitro` f04** thay batigol (bán 72, mua 240) | 408 | mở nguồn chậm — điều kiện cho mọi thưởng sau |
| W12–W15 | `d10s` f05 → **Lv3** (384); `la_pulga` f06 (bán batigol 72, mua 300) | 612 | Lv3 +20% lên mục tiêu bị chậm; f04×f05 chồng 1.26 u |
| W16–W20 | `la_pulga` f06 → Lv2 (240); `el_arbitro` f04 → Lv2 (192); `batigol` f07 → Lv3 (288); `dibu` gk01 (140) | 860 | chuỗi f04→f05→f06→f07 chồng liên tiếp |

Tổng ≈ 2 670 vào túi + phần còn lại vá f01/f03/f09/f10 ở Lv1–Lv2.

**Build CỐ Ý THUA — "rải đều 10 ô":** mua 1 tướng Lv1 cho mỗi ô (~1 800 Peso), không nâng cấp gì.
Σ chord thực tế ≈ 22.0 u nhưng **phân tán trên 44.5 u đường**, không cặp nào chồng nhau → không kích được
`nhan_quan`, không kích được `ban_thang_the_ky` (không có mục tiêu nào vừa bị chậm vừa trong tầm tướng dame).
Dự đoán vỡ ở **W13–W14**.

**Build CỐ Ý THUA — "tất cả vào f08":** `la_pulga` f08 lên thẳng Lv3 (1 020 Peso) vì bảng chord nói nó phủ
5.35 u. Nhưng 5.35 u đó nằm ở f 0.30–0.45 (miệng túi), quân đi qua đúng **một lần**, và 1 020 Peso là 10%
tổng tiền cả trận đổ vào **một** ô không chồng với ô nào. Vỡ ở W11–W12.

**Bài kiểm tra boss (W20, 22 000 máu, kháng chậm 75% → chậm thực tế tối đa 17.5%):** boss đi tốc 0.4, mất
~111 s hết đường. Trong túi nó ăn hoả lực 4 ô liên tục suốt 15.9 u ≈ 40 s. Build "đám đông" (toàn `batigol`
+ `d10s` Lv2, tối ưu wave đông) phải cho headroom **<1.0** trước boss; build có `la_pulga` Lv3 (`so_10`) +
`el_arbitro` Lv2 phải **>1.0**. Chưa đo — tiêu chí PASS/FAIL của test winnability.

---

## 8. Rủi ro

| # | Rủi ro | Vì sao đáng lo | Giảm thiểu |
|---|---|---|---|
| R1 | **Túi quá mạnh → map thành "đặt 4 ô rồi bấm skip"** | 4/10 ô gánh 77% hoả lực. Nếu túi thừa sức, 6 ô kia thành nội dung chết và map chỉ còn một quyết định | Nếu test cho thấy build 4-ô-túi thắng với >15/20 máu → đẩy ô túi ra d≈0.90 (chord@1.2 rơi từ ~4.3 xuống ~2.6) TRƯỚC khi động vào wave |
| R2 | **Biên trần chi tiêu chỉ 3.8%** | Thêm 4 tifoso ở Act 3 là vỡ trần. Vòng 20 đã dính đúng lỗi này ở map gốc và phải sửa dây chuyền 4 tầng | Luật validator #21 phải chạy tự động, không để trong ghi chú |
| R3 | **6 ô ngoài túi bị người chơi coi là ô hỏng** | chord@1.2 trung bình 0.85 — thấp hơn ô yếu nhất map gốc. Lịch sử vòng 17: người chơi báo ba lần "tướng không gây dame" | Chúng phải có vai trò kể được: f01/f03 chặn rò trước túi, f09/f10 chặn rò sau túi. Nếu playtest vẫn báo hỏng → kéo f02 và f10 vào d≈0.85 |
| R4 | **`el_arbitro` không được mua** | Nó chiếm 1 trong 4 ô túi và không gây dame. Ở map gốc, optimizer không mua nó tới tận W11 | Đo: nếu build không-Árbitro thắng dễ hơn build có → túi chưa ép đủ; tăng máu boss W20 thay vì sửa hình học |
| R5 | **Túi làm quân chồng hình** | 4 khúc đường cách nhau 1.7 u trong một vùng 3.4×6.8 u; ở W20 với 40 quân, khúc gấp sẽ đông đặc | Đây là vấn đề art/độ đọc, không phải cân bằng. `docs/06` cần quy định độ rộng vệt đường ≤0.8 u trong túi |
| R6 | **Đường 44.49 ≠ 42.28** | Mọi hằng số `balanceModel` của map gốc sai với map này; chord/2R trung bình 1.16 vs 0.77 → mô hình đánh giá thấp phe thủ 16% | Không chép `hpScaling`; chạy test winnability riêng |
