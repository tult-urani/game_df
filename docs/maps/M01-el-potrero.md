# M01 — El Potrero

> **Trạng thái:** 🟡 Plan. Hình học ĐÃ ĐO bằng bản copy `tools/path_check.py`; số cân bằng là **ĐÍCH CẦN ĐO**, chưa chốt.
> **Schema:** theo `docs/08-MAPS-ARCHITECTURE.md` §2 — `lanes` là MẢNG (map này 1 phần tử), `spawns` là MẢNG `[{enemy,count,lane}]`, `fieldSlots` suy từ `slots`.
> 1 tuyến · **11 ô sân** + 1 ô thủ môn · boss kháng chậm 75% · độ khó ★

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m01 -->
**Nguồn: `config/maps/m01-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 1 — `L1` 43.8u |
| Ô sân / thủ môn | 11 / 1 |
| Σchord@1.4 mỗi ô | 1.79 |
| Tiền khởi đầu | 700 |
| Máu quái | ×0.67 × 1.12^(W−1) × milestone → W20 ×11.54 |
| Tổng quân 20 wave | 429 |

| W | ×máu | `L1` | boss | quân | tổng máu |
|---|------|------|------|------|----------|
| 1 | ×0.67 | 8/0/0 | — | 8 | 536 |
| 2 | ×0.75 | 12/0/0 | — | 12 | 900 |
| 3 | ×0.84 | 10/3/0 | — | 13 | 1 395 |
| 4 | ×0.94 | 10/7/0 | — | 17 | 2 389 |
| **5** | ×1.27 | 8/6/1 | `o_capitao`@`L1` | 16 | 4 380 |
| 6 | ×1.42 | 9/7/1 | — | 17 | 4 241 |
| 7 | ×1.59 | 8/8/2 | — | 18 | 5 810 |
| 8 | ×1.78 | 8/6/1 | — | 15 | 4 748 |
| 9 | ×1.99 | 9/8/1 | — | 18 | 6 390 |
| **10** | ×2.60 | 8/8/2 | `o_capitao`@`L1` | 19 | 12 018 |
| 11 | ×2.91 | 9/8/3 | — | 20 | 12 553 |
| 12 | ×3.26 | 9/9/3 | — | 21 | 14 781 |
| 13 | ×3.65 | 9/12/4 | — | 25 | 20 973 |
| 14 | ×4.09 | 12/12/4 | — | 28 | 24 712 |
| **15** | ×5.57 | 9/9/4 | `o_capitao`@`L1` + `o_capitao`@`L1` | 24 | 33 886 |
| 16 | ×6.23 | 9/12/4 | — | 25 | 35 787 |
| 17 | ×6.98 | 12/12/4 | — | 28 | 42 168 |
| 18 | ×7.82 | 12/14/5 | — | 31 | 54 969 |
| 19 | ×8.76 | 14/15/7 | — | 36 | 74 888 |
| **20** | ×11.54 | 16/18/8 | `o_capitao`@`L1` + `o_capitao`@`L1` | 44 | 124 950 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m01 -->

---

## 1. Ý đồ thiết kế

Đây là map làm quen NGAY SAU map gốc, nên nó phải dạy đúng **một** thứ mới và không được khó hơn map gốc.
Thứ nó dạy: **đường cong quyết định giá trị của ô, không phải khoảng cách tới đường.** Cả map là một đường
zigzag 9 cua gắt (góc đổi hướng ~83°), và hệ quả hình học đo được là: ô nằm **trong lòng cua** thấy hai đoạn
đường cùng lúc, còn ô nằm **sau lưng cua** thấy đúng một đoạn — dù hai ô cách đường y hệt nhau.

Ba điều map gốc KHÔNG dạy được và map này dạy:
- **Nổ lan được thưởng thật.** Ở lòng cua, quân của hai nhánh đường ở gần nhau trong không gian → một quả nổ
  `d10s`/`batigol` chạm cả hai nhánh. Map gốc chưa có ô nào cắt 2 đoạn đường (`đoạn rời = 1` ở mọi ô).
- **Tầm xa đặt sai bị phạt bằng số.** `la_pulga` ở f10 (tầm Lv1 1.4) chỉ phủ **1.17 unit** đường; `batigol` rẻ
  gấp 2.5 lần ở f03 phủ **3.13 unit**. Giá cao không mua được vị trí.
- **Ô "chỉ La Pulga với tới" là bẫy chứ không phải phần thưởng.** f02/f07/f10 (d≈1.26–1.28) chỉ `la_pulga`
  Lv1 dùng được, và hai trong ba ô đó có chord thấp hơn trung bình map.

---

## 2. Đường chạy (lane `L1`)

| # | x | y | ghi chú |
|---|---|---|---|
| 0 | 0.00 | 9.30 | `spawnPoint` |
| 1 | -2.12 | 8.10 | cua 1 |
| 2 | 2.28 | 6.20 | cua 2 |
| 3 | -2.03 | 4.35 | cua 3 |
| 4 | 2.37 | 2.45 | cua 4 |
| 5 | -2.16 | 0.45 | cua 5 |
| 6 | 2.24 | -1.55 | cua 6 |
| 7 | -2.28 | -3.55 | cua 7 |
| 8 | 2.08 | -5.45 | cua 8 |
| 9 | -1.54 | -7.20 | cua 9 |
| 10 | 0.00 | -9.10 | `goalPoint` |

**Kết quả kiểm (bản copy `tools/path_check.py`, thư mục nháp):**

| Mục | Giá trị | Kết |
|---|---|---|
| Độ dài spline thật | **43.85 units** (khai 43.85, lệch 0.0%) | ✅ |
| Hộp bao | x [-2.3, 2.4] · y [-9.1, 9.3] | ✅ trong viewport x±5.4 y±9.6 |
| Ô chết (d > tầm Lv1 lớn nhất 1.4) | 0 | ✅ |
| Đoạn đường không ô nào canh | f=0.75→0.88 = 14% (6.0 u) | ✅ ≤15% |
| Hai ô gần nhau nhất | f10–gk01 = 1.41 u = **51 pt** | ✅ ≥48pt |
| Vùng chạm tràn mép | không | ✅ |
| Số đoạn đường 1 vòng tầm cắt được | **tối đa 2** (map gốc: 1) | ✅ đây là cơ chế |

⚠️ Đường dài **43.85** chứ không phải 42.28 của map gốc (+3.7%). Đây là **cố ý** — quân nằm dưới hoả lực lâu
hơn 3.7% là một trong hai đòn bẩy làm map dễ hơn. Nhưng nó cũng có nghĩa: mọi con số dẫn xuất từ
`pathLengthUnits` trong `economy.json → balanceModel` KHÔNG dùng lại được cho map này.

---

## 3. Ô đặt tướng — 11 ô sân + 1 thủ môn

`d` = khoảng cách vuông góc tới đường. `chord@R` = độ dài đường nằm trong tầm R. `đoạn` = số khúc đường rời
nhau mà vòng tầm cắt được (2 = ô lòng cua).

| ô | x | y | d | f | chord@1.0 | chord@1.2 | chord@1.4 | đoạn | tướng với tới ở **Lv1** | vai trò |
|---|---|---|---|---|---|---|---|---|---|---|
| f01 | -2.80 | 7.80 | 0.74 | 0.06 | 0.78 | 1.19 | 1.60 | 1 | batigol · d10s · árbitro · pulga | ô mở màn, ai cũng đặt được |
| f02 | 0.90 | 8.30 | 1.26 | 0.12 | 0.00 | 0.00 | 1.31 | 2 | **chỉ la_pulga** | ô tầm xa — Lv2+ mới đáng |
| f03 | -0.40 | 6.00 | 0.84 | 0.23 | 1.05 | 1.69 | **3.13** | **2** | batigol · d10s · árbitro · pulga | 🥇 **cua vàng** — ô mạnh nhất map |
| f04 | -2.30 | 3.10 | 1.11 | 0.29 | 0.00 | 0.68 | 1.26 | 1 | d10s · árbitro · pulga | lưng cua, batigol bó tay |
| f05 | 2.70 | 1.70 | 0.72 | 0.40 | 0.98 | 1.45 | 1.90 | 1 | batigol · d10s · árbitro · pulga | ô sườn tốt |
| f06 | -0.60 | 2.20 | 0.91 | 0.46 | 0.78 | 1.52 | **2.54** | **2** | batigol · d10s · árbitro · pulga | 🥈 cua vàng số 2 |
| f07 | 1.30 | 0.40 | 1.28 | 0.58 | 0.00 | 0.00 | 1.74 | 2 | **chỉ la_pulga** | ô tầm xa **có lãi** (chord 4.68 ở R=1.8) |
| f08 | -1.90 | -2.20 | 0.92 | 0.72 | 0.71 | 1.41 | 1.89 | 1 | batigol · d10s · árbitro · pulga | ô sườn |
| f09 | -2.50 | -4.80 | 1.06 | 0.75 | 0.00 | 0.85 | 1.39 | 1 | d10s · árbitro · pulga | lưng cua |
| f10 | 1.20 | -7.40 | 1.26 | 0.88 | 0.00 | 0.00 | 1.17 | 1 | **chỉ la_pulga** | 🪤 **bẫy** — chord thấp nhất map |
| f11 | -2.00 | -6.60 | 0.74 | 0.94 | 0.93 | 1.38 | 1.80 | 1 | batigol · d10s · árbitro · pulga | chốt chặn cuối |
| gk01 | 1.00 | -8.80 | 1.04 | 1.00 | — | — | — | — | **chỉ dibu** | cách cầu môn **1.04** |

**Ô cố ý là "ô tầm xa" (chỉ dùng được từ Lv2 trở lên với đa số tướng):** f02, f07, f10 (d = 1.26–1.28).
`d10s`/`el_arbitro` phải lên Lv2 (tầm 1.4) mới với tới; `batigol` phải lên **Lv3** (1.4).

🟢 **Điểm khác map gốc:** gk01 cách cầu môn **1.04 unit** nên `dibu` Lv1 (tầm 1.4) canh được **cả miệng lưới
lẫn đoạn tiếp cận**. Map gốc để gk cách cầu môn 3.19 và Dibu Lv3 không với tới lưới — lỗi vị trí đã ghi trong
`config/path.json _slotDesign` vòng 20. Map làm quen không nên tái diễn cái đó.

### Thang tầm — số ô dùng được ở Lv1

| Tướng | tầm Lv1 | số ô dùng được |
|---|---|---|
| `batigol` | 1.0 | **6/11** |
| `d10s` | 1.2 | 8/11 |
| `el_arbitro` | 1.2 | 8/11 |
| `la_pulga` | 1.4 | 11/11 |

---

## 4. Bảng phủ synergy

Map này KHÔNG có combo pocket (đó là bài của M02). Ở đây synergy đến từ **cặp ô lòng cua** — hai ô cùng cắt
2 nhánh đường nên vùng phủ của chúng dính nhau ở khúc giữa.

| cặp ô | khoảng cách | phủ chung khi nào | cặp tướng được thưởng |
|---|---|---|---|
| **f03 × f06** | 3.85 u | KHÔNG chồng ở Lv1; chồng từ **R≥1.6 cả hai** (đoạn f≈0.34–0.37) | `el_arbitro` Lv3 (1.6) ở f06 + `la_pulga` Lv2 (1.6) ở f03 → +25% lên mục tiêu bị chậm |
| **f03 × f04** | 3.19 u | chồng ở đoạn f≈0.27–0.30 khi f04 lên tầm ≥1.4 | `d10s` Lv1 ở f04 (chậm 50%/3s mỗi 15s) + `la_pulga` ở f03 |
| **f06 × f08** | 4.58 u | không chồng — cố ý | — |

🔴 **Đòn bẩy quan trọng nhất của map này KHÔNG phải synergy tầm, mà là AoE hai nhánh.** Ở f03 và f06, vòng nổ
`d10s` (lan 1.2/1.7/1.2) đặt tại điểm chạm trên nhánh này phủ luôn quân đang đi nhánh kia — hai nhánh cách nhau
~1.84 unit theo phương dọc. `d10s` **Lv2** (lan 1.7) là cấp duy nhất chạm được cả hai nhánh từ một phát nổ.

💡 Đây là lý do M01 chọn `d10s` Lv2 làm "đúng đáp án" chứ không phải Lv3: luật B-01 nói mỗi cấp chạy ĐÚNG MỘT
kỹ năng, và Lv3 (`ban_thang_the_ky`) đòi nguồn làm chậm bên ngoài (`requiresExternalSlowSource`) mà map này
cố tình chưa ép người chơi mua `el_arbitro`.

---

## 5. Bảng 20 wave

Tất cả đi trên lane `L1`. Boss `o_capitao` spawn **sau cùng** trong wave của nó.

| W | adepto | tifoso | tambor | boss | tổng sinh | ghi chú |
|---|---|---|---|---|---|---|
| 1 | 8 | 0 | 0 | | 8 | dạy luật |
| 2 | 12 | 0 | 0 | | 12 | |
| 3 | 10 | 3 | 0 | | 13 | tifoso xuất hiện |
| 4 | 10 | 7 | 0 | | 17 | |
| 5 | 8 | 6 | 1 | | 15 | tambor đầu tiên |
| 6 | 9 | 7 | 1 | | 17 | |
| 7 | 8 | 8 | 2 | | 18 | kiểm tra cuối Act 1 |
| 8 | 8 | 6 | 1 | | 15 | reset đầu Act 2 (thưởng ×1.6) |
| 9 | 9 | 8 | 1 | | 18 | |
| 10 | 8 | 8 | 2 | **✔** | 19 | **boss giữa** |
| 11 | 9 | 8 | 3 | | 20 | |
| 12 | 9 | 9 | 3 | | 21 | |
| 13 | 9 | 12 | 4 | | 25 | |
| 14 | 12 | 12 | 4 | | 28 | cao trào Act 2 |
| 15 | 9 | 9 | 4 | | 22 | reset đầu Act 3 (thưởng ×2.2) |
| 16 | 9 | 12 | 4 | | 25 | |
| 17 | 12 | 12 | 4 | | 28 | |
| 18 | 12 | 14 | 5 | | 31 | |
| 19 | 14 | 15 | 7 | | 36 | |
| 20 | 16 | 18 | 8 | **✔** | 43 | **boss cuối** |

Bảng này = bảng map gốc **giảm ~8%** ở mọi loại quân. Máu vẫn theo `hp = baseHp × 0.67 × 1.09^(wave-1)`
(giữ nguyên `hpScaling` map gốc) — tức HP giảm ~8%, tiền cũng giảm ~7%, **tỉ lệ tiền/máu gần như không đổi**,
còn phần "dễ hơn" đến từ hình học (đường dài hơn 3.7% + hai ô cắt 2 nhánh).

**Đỉnh quân sống cùng lúc — ước lượng:** map gốc đo được **27 con ở W20** với 47 con sinh ra. M01 W20 sinh 43
con (91% của map gốc) trên đường dài hơn 3.7% → ước **≈ 25–26 con**. Dưới trần **40**. ⚠️ Phải đo lại bằng
`max(m.Enemies.Count)` trong test winnability, không được suy từ tổng số quân.

---

## 6. Kinh tế đề xuất

| Mục | Giá trị | Nguồn |
|---|---|---|
| `startingCash` | **700** (điểm khởi đầu để quét, không phải số chốt) | map gốc đo được 700 thắng còn 11/20 |
| `goalHealth` | 20 | dùng chung |
| Số ô sân | **11** (suy từ `slots`, KHÔNG lấy từ `economy.json` — xem 08 §2.2c) | |
| **TRẦN CHI TIÊU** | 11 × 1020 + 1 × 476 = **11 696** | `docs/08` §5 |
| Tiền cả trận (tính lý thuyết) | **10 741** = 700 khởi đầu + 8 200 thưởng quái + 1 385 thưởng clear + 456 thưởng skip | tính từ bảng wave §5 |
| Biên dưới trần | **+955 (8.2%)** | ✅ luật #21 đạt |

Chi tiết thưởng skip: 19 lần nghỉ × 8 s × 3 Peso/s = 456 (giả định người chơi skip ngay lập tức — đây là **cận
trên**, người chơi thật lấy ít hơn).

🔴 **Ba số này là ĐÍCH CẦN ĐO, không phải số chốt:** `startingCash`, `hpScaling.base`, và ranh giới act.
`tools/balance_sim.py` **không dùng được** để chốt — vòng 22 nó báo headroom trung bình 2.09 ("quá dễ") trong
khi engine thật thắng sát nút còn 11/20 máu. Cách đo đúng: test winnability 20 wave chạy engine thật, quét
`startingCash` bằng dò nhị phân như `docs/08` §6.3.

**Đích headroom cho M01:** dải cứng [1.05, 1.65]; vì đây là map ★ nên nhắm **nửa trên [1.25, 1.60]**, kết trận
mong đợi **còn 14–17/20 máu** (map gốc: 11/20). Nếu đo ra >1.65 ở bất kỳ wave nào → giảm `startingCash`
trước, đừng đụng hình học.

---

## 7. Đường thắng dự kiến

**Build QUA ĐƯỢC — "hai lòng cua":**

| Giai đoạn | Hành động | Tiền |
|---|---|---|
| Kickoff | `batigol` f03 (120) + `batigol` f01 (120) + `d10s` f06 (240) | 480/700 |
| W3–W6 | `d10s` f06 → Lv2 (192) — lan 1.7 chạm cả hai nhánh | |
| W7–W10 | `la_pulga` f05 (300); `batigol` f03 → Lv2 (96) | |
| W11–W14 | `la_pulga` f05 → Lv2 (240); `d10s` f08 (240) | |
| W15–W20 | `el_arbitro` f04 (240) → Lv2 (192) cho boss; `la_pulga` f05 → Lv3 (480) | |

**Build CỐ Ý THUA — "mua tầm xa":** đổ tiền vào `la_pulga` ở f02 + f07 + f10 (3 × 300 = 900 ngay đầu trận).
Ba ô này cộng lại phủ **4.22 unit** ở tầm Lv1 — ít hơn **một** `batigol` 120 Peso ở f03 (3.13 u) cộng một
`batigol` ở f05 (1.90 u = 5.03 u) với giá 240. Build này hết tiền ở W8 và vỡ ở W13–W14.

**Build CỐ Ý THUA — "rải đều":** một tướng Lv1 mỗi ô. 11 ô × ~150 Peso trung bình = hết sạch tiền mà không ô
nào lên Lv2. Boss W20 (22 000 máu, kháng chậm 75%) cần hoả lực tập trung — 11 tướng Lv1 rải khắp map chỉ có
2–3 tướng bắn được boss tại mỗi thời điểm.

**Bài kiểm tra boss:** boss W20 đi tốc 0.4 → mất ~110 s để hết đường 43.85 u. Build "đám đông" (toàn `batigol`
+ `d10s` Lv1/Lv2, tối ưu cho wave đông) phải cho headroom **<1.0** trước boss; build có `la_pulga` Lv3
(`so_10`, đòn thứ 4 xuyên 2 quân) + `el_arbitro` Lv2 phải **>1.0**. Chưa đo — đây là tiêu chí PASS/FAIL của
test winnability.

---

## 8. Rủi ro

| # | Rủi ro | Vì sao đáng lo | Giảm thiểu |
|---|---|---|---|
| R1 | **Đường dài 43.85 làm mọi hằng số cân bằng của map gốc sai** | `balanceModel.pathLengthUnits = 42.28` bị nối cứng; chord/2R trung bình ở R=1.8 là **0.93** (map gốc 0.77) → phe thủ mạnh hơn ~20% mà mô hình không thấy | Đo lại bằng engine; KHÔNG chép `hpScaling` sang mà không chạy test |
| R2 | **f03 quá mạnh → chỉ có một đáp án đúng** | chord@1.4 = 3.13 gấp đôi trung bình map (1.74 ở map gốc). Nếu ai cũng đặt cùng một tướng vào f03, ô đó thành bắt buộc chứ không phải lựa chọn | Nếu test cho thấy f03 luôn được chọn đầu tiên ở mọi build thắng → đẩy f03 ra d≈1.06 (mất `batigol` Lv1) |
| R3 | **Ba ô "chỉ La Pulga" (f02/f07/f10) bị coi là ô hỏng** | Lịch sử map gốc vòng 17: người chơi thật báo BA LẦN "tướng không gây dame", tất cả đều là ô ngoài tầm | f02/f07 vẫn có chord tốt ở R=1.8 (3.28 / 4.68) nên có đường lên; **f10 là ô yếu nhất map** — nếu playtest báo hỏng thì kéo f10 vào d≈0.90 |
| R4 | **Zigzag đều đặn nhìn như đồ thị, không như sân bóng** | 9 cua cùng biên độ, cùng góc — mắt đọc ra ngay là hình sinh bằng công thức | Việc của art (`docs/06`): lệch cỏ, bóng đổ, vạch sân cắt chéo qua zigzag để phá nhịp |
| R5 | **Hành lang zigzag chỉ rộng 4.65 unit** (x ∈ [-2.3, 2.4]) — hai mép màn hình trống | Trên máy 19.5:9 phần trống còn lớn hơn | Ô đặt tướng đã đẩy ra tới x = ±2.8; phần còn lại là khán đài/art |
| R6 | **Map dễ hơn nhưng KHÔNG dễ hơn đủ** | Cả hai đòn bẩy (đường +3.7%, 2 ô cắt 2 nhánh) đều nhỏ; giảm 8% số quân lại kéo tiền xuống theo | Nếu đo ra vẫn khó bằng map gốc → nâng `startingCash` lên 760/820 (map gốc bão hoà ở 730+, map này ít ô hơn nên chưa chắc) |
