# M03 — Dos Ríos

> **Trạng thái:** 🟡 Plan. Hình học ĐÃ ĐO bằng bản copy `tools/path_check.py` (chạy RIÊNG từng tuyến); số cân bằng là **ĐÍCH CẦN ĐO**.
> **Schema:** theo `docs/08-MAPS-ARCHITECTURE.md` §2 — `lanes` MẢNG 2 phần tử `L1`/`L2`, `spawns` MẢNG `[{enemy,count,lane}]`, `fieldSlots` suy từ `slots`.
> **2 tuyến** · **12 ô sân** + 1 ô thủ môn · boss kháng chậm **78%** · độ khó ★★

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m03 -->
**Nguồn: `config/maps/m03-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 2 — `L1` 23.8u · `L2` 23.9u |
| Ô sân / thủ môn | 12 / 1 |
| Σchord@1.4 mỗi ô | 3.30 |
| Tiền khởi đầu | 800 |
| Máu quái | ×0.67 × 1.08^(W−1) × milestone → W20 ×5.78 |
| Tổng quân 20 wave | 413 |

| W | ×máu | `L1` | `L2` | boss | quân | tổng máu |
|---|------|------|------|------|------|----------|
| 1 | ×0.67 | 8/0/0 | — | — | 8 | 536 |
| 2 | ×0.72 | — | 11/0/0 | — | 11 | 792 |
| 3 | ×0.78 | 8/4/0 | — | — | 12 | 1 312 |
| 4 | ×0.84 | — | 8/8/0 | — | 16 | 2 160 |
| **5** | ×1.09 | 8/5/1 | — | `o_capitao`@`L1` | 15 | 3 679 |
| 6 | ×1.18 | — | 9/8/0 | — | 17 | 3 142 |
| 7 | ×1.28 | 8/8/3 | — | — | 19 | 5 378 |
| 8 | ×1.38 | — | 8/5/1 | — | 14 | 3 377 |
| 9 | ×1.49 | 9/8/0 | — | — | 17 | 3 957 |
| **10** | ×1.88 | 4/4/2 | 4/4/1 | `o_capitao`@`L1` | 20 | 10 401 |
| 11 | ×2.03 | 8/8/4 | — | — | 20 | 9 648 |
| 12 | ×2.19 | — | 8/8/4 | — | 20 | 10 412 |
| 13 | ×2.36 | 8/10/5 | — | — | 23 | 13 583 |
| 14 | ×2.55 | — | 12/11/4 | — | 27 | 14 843 |
| **15** | ×3.35 | 8/8/4 | — | `o_capitao`@`L1` + `o_capitao`@`L2` | 22 | 21 528 |
| 16 | ×3.61 | — | 8/10/5 | — | 23 | 20 773 |
| 17 | ×3.90 | 12/11/4 | — | — | 27 | 22 702 |
| 18 | ×4.21 | — | 12/12/6 | — | 30 | 30 084 |
| 19 | ×4.55 | 14/14/7 | — | — | 35 | 37 905 |
| **20** | ×5.78 | 8/9/4 | 7/9/4 | `o_capitao`@`L2` + `o_capitao`@`L1` | 43 | 67 014 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m03 -->

---

## 1. Ý đồ thiết kế

Map đầu tiên có hai tuyến, nên nó phải dạy đúng **một** thứ mới: **tiền không mua được sự có mặt ở hai nơi**.

Hai dòng chạy **luân phiên** — wave lẻ ra tuyến trái `L1`, wave chẵn ra tuyến phải `L2` — nên mỗi tướng
đơn tuyến chỉ làm việc **một nửa số wave**. Người chơi có hai lối đi và cả hai đều đúng một nửa:

- **Ô đơn tuyến ở khuỷu cua** (f02/f03/f08/f09) mạnh khủng khiếp — chord@1.4 = **4.55–4.61**, gấp 2.6 lần
  trung bình map gốc — nhưng **nằm im ở nửa số wave**. Giá trị trung bình mỗi wave chỉ còn ~2.3.
- **Ô đôi ở eo** (f01/f04/f07/f10, x = 0) chỉ chord@1.4 = **1.39–1.47 mỗi tuyến**, yếu hơn hẳn — nhưng
  **làm việc ở MỌI wave**, và một khẩu ở đó thay được hai khẩu đơn tuyến.

Thứ M01 và M02 không dạy được: **synergy bị tuyến chia đôi.** `d10s` Lv3 (`ban_thang_the_ky`,
`requiresExternalSlowSource`) cần nguồn chậm đánh trúng **cùng một con quái**. Nguồn chậm đặt ở khuỷu `L1`
hoàn toàn vô dụng với sát thương đặt ở khuỷu `L2`. Trên map này chỉ có **một** vùng mà slow và damage chắc
chắn gặp nhau ở mọi wave: **đoạn song song cuối sân** (f11/f12/gk01). Nhưng nó nằm ở 8% cuối đường — cứu
được thì cứu, mà cứu hụt là thủng lưới ngay.

---

## 2. Hai tuyến

### 2.1 Vì sao chọn LUÂN PHIÊN theo wave (không phải cùng lúc)

| Phương án | Ưu | Nhược | Quyết định |
|---|---|---|---|
| **Luân phiên theo wave** (lẻ→L1, chẵn→L2) | Không cần cơ chế lập lịch mới (`docs/08` §3.2); đỉnh quân đồng thời không đổi; bài học "một nửa thời gian" rõ ràng và **đọc được trên màn hình** | Nửa sân đứng im mỗi wave — nếu art không báo trước tuyến nào sắp ra thì thành đánh đố | ✅ **CHỌN** |
| Cùng lúc hai tuyến mọi wave | Kịch tính hơn | Đỉnh quân đồng thời **gấp đôi** (rủi ro R4, trần 40 con); và nó là bài của M04 theo `docs/08` §3.2 | ❌ |
| Lệch nhịp (`delaySec`) | | Cần thêm trường `delaySec` — bài của M07 | ❌ |

🔴 **Điều kiện bắt buộc của phương án luân phiên:** HUD phải báo tuyến của wave sau **trong lúc nghỉ 8 giây**,
không phải lúc quân đã ra. Không có cái đó thì map này là trò tung đồng xu. Đây là yêu cầu UI, ghi vào `docs/06`.

**Ngoại lệ — hai wave boss là "wave đôi":** W10 và W20 ra **cả hai tuyến cùng lúc**, chia đôi số quân. Đây là
lần duy nhất người chơi phải phòng thủ hai bên một lúc, và là lý do tồn tại của các ô đôi.

### 2.2 Toạ độ

**Tuyến `L1` (trái) — 14 waypoint, dài 38.06 units**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 0 | -3.00 | 9.30 | `spawnPoint` | 7 | -4.45 | -2.10 |
| 1 | -4.35 | 8.80 | | 8 | -0.80 | -3.80 | **eo 4** |
| 2 | -0.80 | 7.60 | **eo 1** | 9 | -2.40 | -4.90 |
| 3 | -4.45 | 5.60 | | 10 | -0.80 | -5.80 |
| 4 | -0.80 | 3.60 | **eo 2** | 11 | -0.80 | -7.20 |
| 5 | -4.45 | 1.70 | | 12 | -0.80 | -8.30 |
| 6 | -0.80 | -0.20 | **eo 3** | 13 | -0.80 | -9.10 | `goalPoint` |

**Tuyến `L2` (phải) — 14 waypoint, dài 38.00 units**

| # | x | y | | # | x | y |
|---|---|---|---|---|---|---|
| 0 | 3.00 | 9.30 | `spawnPoint` | 7 | 4.45 | -2.20 |
| 1 | 4.35 | 8.70 | | 8 | 0.80 | -3.90 | **eo 4** |
| 2 | 0.80 | 7.50 | **eo 1** | 9 | 2.40 | -5.00 |
| 3 | 4.45 | 5.50 | | 10 | 0.80 | -5.90 |
| 4 | 0.80 | 3.50 | **eo 2** | 11 | 0.80 | -7.20 |
| 5 | 4.45 | 1.60 | | 12 | 0.80 | -8.30 |
| 6 | 0.80 | -0.30 | **eo 3** | 13 | 0.80 | -9.10 | `goalPoint` |

**Kết quả kiểm (bản copy `tools/path_check.py`, chạy RIÊNG từng tuyến):**

| Mục | L1 | L2 | Kết |
|---|---|---|---|
| Độ dài spline thật | **38.06** (khai 38.06) | **38.00** (khai 38.00) | ✅ lệch 0.0% |
| Hộp bao | x [-4.5, -0.7] · y [-9.1, 9.3] | x [0.7, 4.5] · y [-9.1, 9.3] | ✅ trong viewport |
| Chênh lệch độ dài hai tuyến | **0.06 unit = 0.16%** | | ✅ luân phiên công bằng |
| Đoạn không ô nào canh | f=0.00→0.14 = 14% | f=0.00→0.14 = 14% | ✅ ≤15% |
| Hai ô gần nhau nhất | f11–f12 = 1.40 u = **51 pt** | | ✅ ≥48pt |
| Khoảng cách nhỏ nhất giữa HAI tuyến | **1.36 units** | | ✅ |

⚠️ **Hai tuyến KHÔNG hợp lưu.** Chúng chạy song song cách nhau **1.60 unit** từ y = -5.8 xuống cầu môn và vào
lưới ở hai mép khung thành (x = ∓0.80). Đây là quyết định có chủ ý để **né hẳn** rủi ro R5 của `docs/08`
(hai tuyến trùng hình → quân chồng lên nhau, trông như bug). Không cần mẹo lệch 0.25 unit, không cần waypoint
trùng nhau.

🔴 **Giới hạn của công cụ hiện tại — phải ghi rõ:** `tools/path_check.py` chỉ biết MỘT đường. Chạy nó trên
`L1` với đủ 12 ô thì nó báo **3 "Ô CHẾT" giả** (f03, f06, f09 — cách `L1` 4.5–5.0 unit) vì nó không biết ba ô
đó phục vụ `L2`. Chạy trên `L2` thì báo f02/f05/f08. Đo bằng bộ kiểm hai tuyến (khoảng cách = min tới hai
tuyến): **0 ô chết**. Đây đúng là việc `docs/08` §8 yêu cầu — `path_check` phải nhận `--map` và kiểm từng
tuyến với đúng tập ô phục vụ tuyến đó.

---

## 3. Ô đặt tướng — 12 ô sân + 1 thủ môn

`d1`/`d2` = khoảng cách tới `L1`/`L2`. Ô "đôi" = ≤1.4 tới **cả hai**.

| ô | x | y | d1 | d2 | f1 | f2 | chord@1.4 L1 | chord@1.4 L2 | loại | tướng với tới Lv1 |
|---|---|---|---|---|---|---|---|---|---|---|
| f01 | 0.00 | 7.55 | 0.80 | 0.80 | 0.143 | 0.144 | 1.39 | 1.39 | 🔵 **ĐÔI** (eo 1) | tất cả |
| f02 | -3.40 | 5.60 | 0.66 | 4.60 | 0.282 | — | **4.55** | 0 | 🟠 chỉ L1 (khuỷu) | tất cả |
| f03 | 3.40 | 5.50 | 4.59 | 0.66 | — | 0.283 | 0 | **4.55** | 🟠 chỉ L2 (khuỷu) | tất cả |
| f04 | 0.00 | 3.55 | 0.80 | 0.80 | 0.367 | 0.368 | 1.44 | 1.42 | 🔵 **ĐÔI** (eo 2) | tất cả |
| f05 | -4.00 | 0.10 | 1.08 | 4.82 | 0.506 | — | 1.64 | 0 | 🟠 chỉ L1 (sườn) | d10s · árbitro · pulga |
| f06 | 4.20 | 0.10 | 5.01 | 1.07 | — | 0.503 | 0 | 1.60 | 🟠 chỉ L2 (sườn) | d10s · árbitro · pulga |
| f07 | 0.00 | -0.25 | 0.80 | 0.80 | 0.589 | 0.591 | 1.44 | 1.42 | 🔵 **ĐÔI** (eo 3) | tất cả |
| f08 | -3.40 | -2.10 | 0.61 | 4.56 | 0.673 | — | **4.61** | 0 | 🟠 chỉ L1 (khuỷu) | tất cả |
| f09 | 3.40 | -2.20 | 4.49 | 0.61 | — | 0.675 | 0 | **4.61** | 🟠 chỉ L2 (khuỷu) | tất cả |
| f10 | 0.00 | -3.85 | 0.76 | 0.77 | 0.811 | 0.813 | 1.47 | 1.46 | 🔵 **ĐÔI** (eo 4) | tất cả |
| f11 | 0.00 | -6.10 | 0.69 | 0.69 | 0.922 | 0.923 | 2.04 | 1.98 | 🔵 **ĐÔI** (đoạn song song) | tất cả |
| f12 | 0.00 | -7.50 | 0.80 | 0.80 | 0.958 | 0.958 | 2.37 | 2.38 | 🔵 **ĐÔI** (đoạn song song) | tất cả |
| gk01 | 0.00 | -8.90 | 0.80 | 0.80 | 0.995 | 0.995 | — | — | thủ môn, canh **cả hai** miệng lưới | chỉ dibu |

**Ô cố ý là "ô tầm xa" (Lv2+ mới dùng được với `batigol`):** f05 (d1 = 1.08) và f06 (d2 = 1.07) —
`batigol` Lv1 (1.0) không với tới, phải lên Lv2 (1.2).

🟢 **gk01 canh được CẢ HAI cầu môn:** cách mỗi tuyến 0.80 và cách hai điểm goal (∓0.80, -9.10) đúng **0.82** —
`dibu` Lv1 (tầm 1.4) phủ trọn cả hai miệng lưới. Đây là ô duy nhất trong ba map mà thủ môn canh hai dòng.

### Bảng so sánh quyết định — vì sao ô đôi không hiển nhiên là tốt hơn

| | ô khuỷu đơn tuyến (f02/f08) | ô đôi ở eo (f01/f04/f07/f10) |
|---|---|---|
| chord@1.4 khi tuyến đó chạy | **4.55–4.61** | 1.39–1.47 |
| số wave nó làm việc | 10/20 (một nửa) | **20/20** |
| chord trung bình mỗi wave | **~2.30** | ~1.43 |
| số tướng cần để canh cả hai tuyến | **2** (một mỗi bên) | **1** |
| chord trung bình mỗi **Peso** (quy về 1 tướng) | 2.30 | 1.43 |
| tồn tại ở wave đôi W10/W20 | chỉ nửa lượng quân | **toàn bộ** |

👉 Ô khuỷu thắng về chord thuần; ô đôi thắng về **ô** (chỉ tốn 1 ô cho 2 tuyến) và về **wave đôi**. Với 12 ô
và trần chi tiêu 12 716, người chơi không đủ tiền làm cả hai — đây chính là quyết định mà map muốn ép.

---

## 4. Bảng phủ synergy

### 4a. Ở giữa sân — synergy BỊ CẤM bởi hình học

Bốn ô đôi cách nhau **3.6–4.0 unit**, xa hơn tổng tầm của bất kỳ cặp tướng nào (tối đa 1.8 + 1.8 = 3.6 và
còn phải cùng chạm một điểm trên đường). Bốn ô khuỷu cách ô đôi gần nhất **≥3.4 unit**. Kết quả đo:
**không cặp ô nào ở nửa trên sân chồng vùng phủ, ở bất kỳ cấp tầm nào.**

Đây là **cố ý**. Nó buộc `d10s` Lv3 và `la_pulga` Lv2 phải xuống đáy sân nếu muốn ăn thưởng "mục tiêu đang bị
chậm", và cái giá là quân đã đi 90% quãng đường trước khi bị bắn.

### 4b. Ở đáy sân — vùng synergy DUY NHẤT

| cặp | khoảng cách | tầm | đoạn chồng trên L1 (f) | dài chồng | cặp tướng được thưởng |
|---|---|---|---|---|---|
| f11 × f12 | 1.40 u | cả hai R = 1.2 (Lv1) | 0.932–0.945 | **0.48 u** | `el_arbitro` Lv1 + `d10s` Lv1 |
| f11 × f12 | 1.40 u | cả hai R = 1.6 (Lv3/Lv2) | 0.920–0.958 | **1.43 u** (+198%) | `el_arbitro` Lv3 làm chậm → `d10s` Lv3 +20% **và** `la_pulga` Lv2 +25% |
| f12 × gk01 | 1.40 u | — | — | — | `dibu` chỉ cản, không cộng dame; nhưng nó giữ quân TRONG vùng chồng lâu hơn |

Con số y hệt trên `L2` (hai tuyến đối xứng, lệch ≤0.06 unit).

🔴 **Hệ quả thiết kế phải nói thẳng:** vùng synergy nằm ở f = 0.92–0.96, tức **cách cầu môn 1.5–3.0 unit**.
Quân bị chậm ở đó vẫn có thể lọt. Map này thưởng cho synergy **ít hơn** M02 và bù lại bằng việc cho 12 ô.
Nếu test cho thấy người chơi bỏ hẳn `el_arbitro` vì nó chỉ có chỗ ở đáy → phải kéo f11/f12 lên trên
(y = -5.2 và -6.6) để vùng chồng lùi về f ≈ 0.87–0.92.

### 4c. Boss kháng chậm **78%** (không phải 75%)

Vùng chồng ở đáy là chỗ duy nhất boss có thể bị chậm **và** ăn thưởng cùng lúc — nếu để 75% như hai map kia,
đoạn đáy sẽ tự nó giải quyết boss. Ở 78%: chậm thực tế tối đa = 70% × (1 − 0.78) = **15.4%** (map gốc: 17.5%).
Boss đi tốc 0.4 qua 38.06 unit ≈ 95 s; 15.4% chậm chỉ thêm ~8 s trong tầm — đủ để `el_arbitro` "giúp", chưa
đủ để "giải hộ". ⚠️ Đây là số **suy ra**, chưa đo; `bossOverrides.o_capitao.slowResistPercent = 78` là thứ
đầu tiên phải chỉnh nếu test boss lệch.

---

## 5. Bảng 20 wave

**Luật ra quân:** wave **lẻ → `L1`** · wave **chẵn → `L2`** · **W10 và W20 → CẢ HAI** (chia đôi, boss đi một tuyến).

| W | tuyến | adepto | tifoso | tambor | boss | tổng sinh | ghi chú |
|---|---|---|---|---|---|---|---|
| 1 | L1 | 8 | 0 | 0 | | 8 | dạy luật |
| 2 | L2 | 13 | 0 | 0 | | 13 | **lần đầu đổi tuyến** |
| 3 | L1 | 10 | 4 | 0 | | 14 | |
| 4 | L2 | 11 | 8 | 0 | | 19 | |
| 5 | L1 | 9 | 6 | 1 | | 16 | |
| 6 | L2 | 11 | 8 | 1 | | 20 | |
| 7 | L1 | 9 | 8 | 3 | | 20 | kiểm tra cuối Act 1 |
| 8 | L2 | 9 | 6 | 1 | | 16 | reset Act 2 |
| 9 | L1 | 11 | 8 | 1 | | 20 | |
| 10 | **L1+L2** | 5+5 | 4+4 | 1+1 | **✔ L1** | 21 | **wave đôi + boss** |
| 11 | L1 | 11 | 9 | 3 | | 23 | |
| 12 | L2 | 11 | 10 | 4 | | 25 | |
| 13 | L1 | 11 | 13 | 4 | | 28 | |
| 14 | L2 | 14 | 13 | 5 | | 32 | cao trào Act 2 |
| 15 | L1 | 11 | 10 | 4 | | 25 | reset Act 3 |
| 16 | L2 | 11 | 13 | 4 | | 28 | |
| 17 | L1 | 14 | 13 | 5 | | 32 | |
| 18 | L2 | 14 | 15 | 6 | | 35 | |
| 19 | L1 | 16 | 16 | 8 | | 40 | wave đơn tuyến nặng nhất |
| 20 | **L1+L2** | 8+8 | 9+9 | 4+4 | **✔ L2** | 43 | **wave đôi + boss cuối** |

Máu giữ `hp = baseHp × 0.67 × 1.09^(wave-1)`.

**Vì sao W10/W20 chia đôi chứ không nhân đôi:** giữ tổng số quân xấp xỉ wave thường để không đội đỉnh quân
đồng thời. Cái khó của wave đôi là **phải có mặt hai nơi**, không phải số lượng.

### Đỉnh quân sống cùng lúc — ước lượng cho map hai tuyến

| Mốc | Ước tính | Cách suy |
|---|---|---|
| Map gốc W20 (đo thật) | **27** | 47 con sinh, 1 tuyến, đường 42.28 |
| M03 **W19** (đơn tuyến, 40 con) | **≈ 23–24** | 40/47 × 27 × (38.06/42.28) ≈ 20.7, cộng biên vì đường ngắn hơn nên quân dồn hơn |
| M03 **W20** (wave đôi, 21+21+boss) | **≈ 30–34** | mỗi tuyến hành xử như wave 21 con (≈ W11 map gốc = 20 con đỉnh) × 2 tuyến, trừ đi phần chết sớm |
| Trần đề xuất | **40** | `docs/08` §9.1 |

⚠️ **Đây là ước lượng, không phải đo.** Wave đôi làm **tốc độ sinh tổng tăng gấp đôi** (0.7 s/con mỗi tuyến =
2.86 con/giây toàn map) trong khi thời gian sống mỗi con gần như không đổi → đỉnh có thể vọt hơn dự đoán.
**Bắt buộc** đo `max(m.Enemies.Count)` ở W10 và W20 trong test winnability; nếu >40 thì giảm số quân wave đôi
hoặc giãn `spawnIntervalSec` riêng cho wave đôi lên 1.0 s.

---

## 6. Kinh tế đề xuất

| Mục | Giá trị | Nguồn |
|---|---|---|
| `startingCash` | **700** (điểm khởi đầu để quét) | map gốc đo 700 |
| `goalHealth` | 20 | dùng chung |
| Số ô sân | **12** (suy từ `slots`) | |
| **TRẦN CHI TIÊU** | 12 × 1020 + 1 × 476 = **12 716** | `docs/08` §5 |
| Tiền cả trận (lý thuyết) | **11 469** = 700 + 8 928 thưởng quái + 1 385 clear + 456 skip | tính từ §5 |
| Biên dưới trần | **+1 247 (9.8%)** | ✅ luật #21 đạt |

Biên rộng nhất trong ba map — **cố ý**. Map hai tuyến buộc người chơi mua **hai bộ** hàng thủ, nên tiền phải
dư hơn map một tuyến. Nhưng dư tiền mà thiếu ô: 12 ô × giá trung bình ≈ 2 400 Peso cho một lượt phủ Lv1, và
để nâng hết lên Lv3 cần **12 716** — nhiều hơn tổng tiền cả trận. Người chơi **không bao giờ** phủ kín.

**Đích headroom:** dải cứng [1.05, 1.65]; M03 là ★★ nên nhắm **[1.10, 1.45]**, kết trận mong đợi **còn
9–13/20 máu**. Thêm một ràng buộc riêng cho map này: **wave đôi W10 và W20 phải là hai wave headroom thấp
nhất cả trận** — nếu chúng không phải là đáy thì cơ chế hai tuyến chưa có răng.

🔴 Tất cả số trên là **ĐÍCH CẦN ĐO**. `tools/balance_sim.py` **không dùng được** (vòng 22: sim báo 2.09,
engine thật thắng còn 11/20) và với map hai tuyến nó còn sai thêm một tầng — `Geometry` của nó chỉ tính chord
trên MỘT đường (`docs/08` §8).

---

## 7. Đường thắng dự kiến

**Build QUA ĐƯỢC — "xương sống giữa sân":**

| Giai đoạn | Hành động | Chi | Lý do |
|---|---|---|---|
| Kickoff | `batigol` f01 (120) + `batigol` f07 (120) + `batigol` f04 (120) | 360 | ba ô đôi, làm việc ở **mọi** wave — sống sót W1–W6 với đúng 360 Peso |
| W4–W8 | `d10s` f10 (240); `batigol` f01 → Lv2 (96) | 336 | f10 (eo 4) là ô đôi cuối trước đoạn song song |
| W9–W12 | `batigol` f02 (120) + `batigol` f03 (120) — **cặp khuỷu đối xứng** | 240 | 240 Peso mua 9.1 chord, rẻ nhất map; nhưng phải mua **hai cái** |
| W13–W16 | `el_arbitro` f11 (240) → Lv2 (192); `d10s` f12 (240) | 672 | mở vùng chồng ở đáy: f11×f12 = 0.48 u ở Lv1 |
| W17–W20 | `d10s` f12 → **Lv3** (384); `el_arbitro` f11 → Lv3 (384); `dibu` gk01 (140) → Lv2 (112) | 1 020 | vùng chồng lên **1.43 u**; `dibu` canh hai miệng lưới cho wave đôi W20 |

**Build CỐ Ý THUA — "dồn một bên":** đổ hết tiền vào f02 + f05 + f08 (toàn bộ khuỷu và sườn `L1`).
Σ chord@1.4 trên `L1` = **10.80** — mạnh hơn cả map gốc. Nhưng mọi wave **chẵn** đi `L2` và gặp đúng 0 tướng.
10 wave × ~20 quân lọt = thua từ W6.

**Build CỐ Ý THUA — "chỉ ô đôi":** mua đủ 6 ô đôi (f01/f04/f07/f10/f11/f12) và nâng chúng.
Σ chord@1.4 = 1.39 + 1.44 + 1.44 + 1.47 + 2.04 + 2.37 = **10.15** trên tuyến đang chạy — nghe đủ, nhưng
**không ô đôi nào ở nửa trên sân chồng vùng phủ với ô nào** (§4a), nên không kích được `nhan_quan` (+25%)
hay `ban_thang_the_ky` (+20%) cho tới khi quân xuống đáy. Thiếu ~20–25% sát thương ở Act 3 → vỡ W17–W18.

**Build CỐ Ý THUA — "bỏ wave đôi":** tối ưu cho 18 wave đơn tuyến bằng khuỷu hai bên, bỏ hẳn f11/f12.
Sống tốt tới W19, rồi W20 hai dòng cùng đổ về hai miệng lưới không ai canh. Chết đúng ở wave cuối — đây là
**bài kiểm tra chính** của map.

**Bài kiểm tra boss:** boss W20 (22 000 máu, kháng chậm **78%**) đi `L2` cùng lúc 21 quân thường đổ `L1`.
Build tối-ưu-đám-đông phải cho headroom **<1.0**; build có `d10s` Lv3 + `el_arbitro` Lv3 ở f11/f12 (vùng
chồng 1.43 u) phải **>1.0**. Chưa đo — tiêu chí PASS/FAIL của test winnability.

---

## 8. Rủi ro

| # | Rủi ro | Vì sao đáng lo | Giảm thiểu |
|---|---|---|---|
| R1 | **`path_check.py` báo 3 "ô chết" GIẢ mỗi tuyến** | Công cụ chỉ biết một đường; chạy `--check` sẽ exit 1 và chặn build dù map đúng | Bắt buộc làm việc ở `docs/08` §8 trước khi M03 vào CI: `path_check` nhận `--map`, tính d = min qua các tuyến |
| R2 | **Luân phiên tuyến biến map thành trò đoán nếu HUD không báo trước** | Người chơi xây xong mới biết wave sau ra bên kia = mất tiền vì lý do ngoài tầm kiểm soát | Yêu cầu UI cứng: nhãn tuyến của wave kế hiện suốt 8 giây nghỉ. Không có nó thì **không ship map này** |
| R3 | **Đỉnh quân wave đôi có thể vượt trần 40** | Tốc độ sinh tổng gấp đôi (2.86 con/s). Ước 30–34 nhưng chưa đo, và `docs/08` R4 nói phải đo lại cho map đa tuyến | Đo `max(m.Enemies.Count)` ở W10/W20; nếu vượt thì giãn `spawnIntervalSec` cho wave đôi lên 1.0 s |
| R4 | **Nửa sân đứng im mỗi wave nhìn như game hỏng** | 6 trong 12 tướng không bắn suốt cả wave | Art/VFX: làm mờ tuyến không hoạt động, tướng bên đó đứng nghỉ có animation riêng. Vấn đề đọc hiểu, không phải cân bằng |
| R5 | **Ô khuỷu (chord 4.6) mạnh tới mức ô đôi thành nội dung chết** | chord ô khuỷu gấp **3.2 lần** ô đôi. Nếu người chơi mua đôi khuỷu cho cả hai bên là xong, ô đôi không ai đụng | Đếm trong test: nếu build thắng nào cũng bỏ trống ≥3 ô đôi → kéo ô khuỷu ra d≈1.10 (chord@1.4 rơi còn ~2.6) |
| R6 | **Đường mỗi tuyến chỉ 38.06 unit** (map gốc 42.28, −10%) | Quân nằm dưới hoả lực ít hơn 10% → map khó hơn ở cùng bảng wave, và mọi hằng số `balanceModel` sai | Đã tính vào việc để biên tiền rộng (9.8%). Vẫn phải đo lại `startingCash` bằng engine |
| R7 | **Kháng chậm 78% là số bịa** | Chênh 3 điểm so với hai map kia, suy ra chứ không đo | Nó là số đầu tiên nên chỉnh khi test boss lệch; đừng chỉnh hình học trước |
| R8 | **Hai tuyến đối xứng gương → sân nhìn như test pattern** | Chênh lệch độ dài chỉ 0.06 unit, các eo gần như trùng y | Art phải phá đối xứng: khán đài lệch, bóng đổ một hướng, cỏ khác vệt. Hình học giữ đối xứng vì **luân phiên phải công bằng** |
