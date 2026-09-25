# 02 — Tướng phòng ngự (Towers)

> **5 cầu thủ + 1 trọng tài = 6 đơn vị.** Sức mạnh cầu thủ phân theo **độ nổi tiếng**, giá phân theo sức mạnh.
> **Mỗi cấp mở 1 kỹ năng MỚI và cộng dồn** → 18 kỹ năng, Lv3 chạy 3 kỹ năng cùng lúc.
> Số liệu ở đây là bản trích. Nguồn chân lý + phần chứng minh cân bằng: [`04-ECONOMY-BALANCE.md`](04-ECONOMY-BALANCE.md).

💡 *Trọng tài **không phải cầu thủ**, nên nó không phá tiêu chí "5 cầu thủ Argentina nổi tiếng nhất". Số 5 vẫn đúng — chỉ là sân có thêm một người không mặc áo đấu.*

---

## 1. Bảng mapping nội bộ — KHÔNG ĐƯA VÀO GAME

⚠️ **Đây là bảng duy nhất trong toàn bộ repo được phép ghi tên thật.** Mục đích: để dev/artist hiểu đang làm ai. Tên thật **không** được xuất hiện trong code, asset, UI, store listing, hay bất kỳ doc nào khác.

| Biệt danh trong game | Nguyên mẫu | Vì sao chọn |
|---------------------|-----------|-------------|
| **La Pulga** | Lionel Messi | Cầu thủ Argentina nổi tiếng nhất mọi thời — tier S |
| **D10S** | Diego Maradona | Ngang tier S; biểu tượng lịch sử, áo số 10 |
| **Batigol** | Gabriel Batistuta | Tier A; tay săn bàn kinh điển thập niên 90 |
| **Dibu** | Emiliano Martínez | Tier B về độ nổi tiếng, **nhưng là thủ môn** → vai trò riêng |

### ⚠️ Dibu là một lệch chuẩn có chủ đích — đọc trước khi thắc mắc

Tiêu chí ban đầu là **"5 cầu thủ nổi tiếng nhất"**. Dibu không thuộc top-5 đó — Kempes (Vua phá lưới WC 1978), Riquelme, Agüero, Crespo, Zanetti đều nổi tiếng hơn xét cả sự nghiệp.

Dibu được chọn vì **thủ môn là vai trò duy nhất hợp với việc thành là cầu môn**. Đây là tiêu chí thiết kế đè lên tiêu chí độ nổi tiếng, và **user đã xác nhận giữ nguyên** (2026-07-16). Không phải sơ suất.

**Messi vs Maradona** để **cùng tier S**, không xếp trên dưới. Đây là tranh cãi không có đáp án ở Argentina, và game không cần phân xử nó.

**Guardrail bản quyền (bắt buộc):** biệt danh + art cách điệu. Không mặt thật, không áo đấu chính thức có logo/nhà tài trợ, không số áo thật, không giọng nói/chữ ký. Chi tiết ở [`06-ART-UX.md`](06-ART-UX.md) §5.

💡 *Biệt danh không phải là né tránh nửa vời — "La Pulga" và "D10S" là những từ mà chính người hâm mộ dùng. Nhận diện gần như nguyên vẹn, rủi ro pháp lý thì gần như bằng không.*

---

## 2. Tổng quan 5 đơn vị

<!-- GEN:tower_overview -->
| Đơn vị | Tier | Vai trò | Giá | Sát thương | Tầm | Tốc đánh | DPS |
|--------|------|---------|-----|-----------|-----|----------|-----|
| **La Pulga** | S | Sát thương đơn mục tiêu cao nhất, tầm xa | 300 | 45 | 1.4 | 1.2/s | **54** |
| **D10S** | S | Sát thương lan (AoE), khống chế đám đông | 240 | 30 (lan r1.2) | 1.2 | 0.8/s | **24**/mục tiêu |
| **El Cinco** | A | Burst một phát nặng, rất chậm, tầm ngắn | 120 | 20 (lan r0.8) | 1.0 | 0.6/s | **12**/mục tiêu |
| **Dibu** | B | Thủ môn — chặn lọt lưới. Chỉ đặt ở ô thủ môn | 140 | — | 1.4 | — | — |
| **El Árbitro** | — | **Trọng tài** — chậm vĩnh viễn, khắc chế tank | 240 | — | 1.2 | — | **0** |
<!-- /GEN:tower_overview -->

**Quy tắc chung:**
- Tướng tự động bắn mục tiêu **đi đầu trong tầm** (first-in-range). Không có chế độ chọn mục tiêu thủ công ở MVP.
- Tầm tính bằng đơn vị sân (đường chạy dài ~40 đơn vị).
- Đặt được **nhiều instance cùng loại** — 4 con La Pulga trên sân là hợp lệ.
- **El Árbitro chiếm 1 trong 11 ô sân** như tướng thường. Không giới hạn số lượng — nhưng xem §4.6 vì sao spam nó là tự sát.
- Dibu là ngoại lệ duy nhất: tối đa **1 con**, chỉ ở ô thủ môn.

---

## 3. Cây nâng cấp

### 3.1 Chỉ số

| Cấp | Hệ số sát thương | Giá nâng cấp | Tầm | Kỹ năng |
|-----|-----------------|--------------|-----|---------|
| Lv1 | ×1.0 (base) | — (giá mua ban đầu) | base | Mở **kỹ năng #1** |
| Lv2 | ×1.6 | 0.8 × giá mua | base + 0.5 | Giữ #1, **mở thêm #2** |
| Lv3 | ×2.5 | 1.6 × giá mua | base + 1.0 | Giữ #1 + #2, **mở thêm #3** |

**Tốc đánh KHÔNG đổi theo cấp.** Chỉ sát thương và tầm tăng.

**Hai đơn vị 0 DPS — Dibu và El Árbitro — bỏ qua cột hệ số sát thương** (nhân với 0 vẫn là 0). Với chúng, nâng cấp **chỉ** mua thêm tầm và kỹ năng mới. Giá vẫn theo đúng công thức 0.8× / 1.6×.

💡 *Why: nếu vừa tăng dame vừa tăng tốc đánh, DPS scale theo tích số — Lv3 sẽ mạnh gấp ~4× thay vì 2.5×, và mọi bảng cân bằng ở doc 04 sẽ sai. Một trục scale mỗi cấp, giữ toán học kiểm chứng được.*

### 3.2 Luật kỹ năng (quan trọng)

| # | Luật | Vì sao |
|---|------|--------|
| 1 | **Cộng dồn.** Lv3 chạy đồng thời cả 3 kỹ năng. | Nâng cấp phải cảm thấy như được thêm, không phải đánh đổi |
| 2 | **Kỹ năng cấp thấp KHÔNG tự mạnh lên.** `Solo Run` ở Lv3 vẫn ×2 / 12s y như ở Lv1. | Sức mạnh đến từ **số lượng** kỹ năng, không từ việc kỹ năng cũ phình to. Nếu cả hai cùng scale, Lv3 sẽ mạnh gấp ~4× và mô hình cân bằng vỡ lần nữa. |
| 3 | **Mọi kỹ năng đều bị động.** Không có nút kích hoạt. | Chơi một tay; MVP không có chỗ cho quản lý cooldown thủ công |
| 4 | Nhiều tướng cùng loại → cooldown **độc lập** từng con | Đơn giản, dự đoán được |

**Bán tướng:** hoàn **60%** tổng tiền đã đầu tư (giá mua + mọi lần nâng cấp).

💡 *Luật 2 là thứ giữ cho thiết kế này không nổ tung. "Mỗi cấp một kỹ năng mới" nghe rất đã, nhưng nếu kỹ năng cũ cũng mạnh lên theo cấp thì Lv3 có 3 kỹ năng đều ở mức mạnh nhất — cộng với ×2.5 sát thương, tướng Lv3 sẽ mạnh gấp 5–6× Lv1 trong khi chỉ tốn 3.4× tiền. Không ai xây gì khác ngoài Lv3 nữa.*

---

## 4. Chi tiết từng tướng

### 4.1 La Pulga — Tier S — DPS đơn mục tiêu

Đứng yên, nhìn có vẻ không làm gì. Rồi đột nhiên mục tiêu khoẻ nhất trên sân bốc hơi.

| Cấp | Giá | Sát thương | Tốc đánh | DPS | Tầm | Cộng dồn đầu tư |
|-----|-----|-----------|----------|-----|-----|-----------------|
| Lv1 | 300 | 45 | 1.2/s | 54 | 4.5 | 300 |
| Lv2 | 240 | 72 | 1.2/s | 86 | 5.0 | 540 |
| Lv3 | 480 | 113 | 1.2/s | **136** | 5.5 | 1 020 |

| Cấp | Kỹ năng mở | Nội dung |
|-----|-----------|----------|
| **Lv1** | `Solo Run` | Mỗi **12s**, khoá mục tiêu có **máu tuyệt đối cao nhất** trong tầm, gây **×2 sát thương** trong **4s** |
| **Lv2** | `Nhãn Quan` | **+25% sát thương** lên mọi mục tiêu **đang bị chậm**. Đạn không bao giờ trượt mục tiêu bị chậm. |
| **Lv3** | `Số 10` | Mỗi đòn **thứ 4** xuyên qua **2** cổ động viên phía sau mục tiêu, gây full sát thương |

**Điểm mạnh:** DPS/ô cao nhất game. `Nhãn Quan` biến La Pulga thành cặp bài trùng với D10S — mọi thứ D10S làm chậm, La Pulga đánh mạnh hơn.
**Điểm yếu:** đắt nhất. Vô dụng trước đám đông máu thấp — bắn 113 dame vào một Adepto 100 máu là phí 13%.

---

### 4.2 D10S — Tier S — AoE + khống chế

Chạm bóng một cái, cả đám ngã.

| Cấp | Giá | Sát thương lan | Bán kính lan | Tốc đánh | DPS/mục tiêu | Tầm | Cộng dồn |
|-----|-----|---------------|-------------|----------|--------------|-----|----------|
| Lv1 | 280 | 30 | 1.2 | 0.8/s | 24 | 3.5 | 280 |
| Lv2 | 224 | 48 | **1.7** | 0.8/s | 38 | 4.0 | 504 |
| Lv3 | 448 | 75 | **1.7** | 0.8/s | **60** | 4.5 | 952 |

| Cấp | Kỹ năng mở | Nội dung |
|-----|-----------|----------|
| **Lv1** | `Bàn Tay Của Chúa` | Mỗi **15s**, làm **chậm 50%** mọi cổ động viên trong tầm, kéo dài **3s** |
| **Lv2** | `Cú Chạm Thiên Tài` | **Bán kính lan +0.5 vĩnh viễn** (1.2 → 1.7). Không cooldown, luôn bật. |
| **Lv3** | `Bàn Thắng Thế Kỷ` | Trong lúc `Bàn Tay Của Chúa` đang chạy, **mọi tướng khác +20% sát thương** lên mục tiêu bị chậm |

⚠️ **Slow cap cứng ở 70%.** Nhiều D10S không cộng dồn slow — lấy giá trị cao nhất. Cổ động viên phải luôn còn di chuyển (xem `01` §8 **FM-08**).

**Điểm mạnh:** DPS hiệu dụng thật = `24 × số mục tiêu trong bán kính`. `Cú Chạm Thiên Tài` ở Lv2 nâng số mục tiêu chạm được lên rõ rệt. `Bàn Thắng Thế Kỷ` biến D10S thành trung tâm của mọi đội hình late game — nó không mạnh, nó làm cả sân mạnh.
**Điểm yếu:** trước mục tiêu đơn lẻ (đặc biệt là **boss `O Capitão`**, kháng chậm 75%), 24 DPS là tệ nhất game.

---

### 4.3 Batigol — Tier A — burst

Một cú. Chỉ cần một cú.

| Cấp | Giá | Sát thương | Tốc đánh | DPS | Tầm | Cộng dồn |
|-----|-----|-----------|----------|-----|-----|----------|
| Lv1 | 160 | 90 | 0.4/s | 36 | 3.0 | 160 |
| Lv2 | 128 | 144 | 0.4/s | 58 | 3.5 | 288 |
| Lv3 | 256 | 225 | 0.4/s | **90** | 4.0 | 544 |

| Cấp | Kỹ năng mở | Nội dung |
|-----|-----------|----------|
| **Lv1** | `Sút Xuyên` | Đạn **xuyên qua mọi cổ động viên** trên đường thẳng quỹ đạo, gây full sát thương cho từng người |
| **Lv2** | `Bản Năng Sát Thủ` | **+30% sát thương** lên cổ động viên còn **máu > 80%** |
| **Lv3** | `Cú Vô-lê` | Mỗi **10s**, đòn tiếp theo gây **×2.5 sát thương** và **đẩy lùi** mục tiêu **1.5 đơn vị** dọc đường chạy |

⚠️ `Cú Vô-lê` **đẩy lùi, không làm choáng.** Cổ động viên vẫn di chuyển (ngược lại), không vi phạm luật "luôn phải còn di chuyển" ở `01` §8 **FM-08**. Cấm mọi cơ chế đứng yên hoàn toàn.

**Điểm mạnh:** rẻ nhất trong nhóm sát thương. `Sút Xuyên` + `Bản Năng Sát Thủ` khiến Batigol tàn sát đám vừa spawn, còn nguyên máu, xếp hàng dọc — đặt ở đoạn thẳng dài **ngay sau vạch spawn**, không phải khúc cua cuối.
**Điểm yếu:** tầm 1.0 ngắn nhất game — chỉ với tới đường từ 5/11 ô. 2.5 giây mới bắn một phát — mục tiêu chạy nhanh (Tifoso, tốc 1.4) có thể qua tầm mà chỉ ăn đúng 1 phát. `Bản Năng Sát Thủ` vô dụng ở cuối đường, nơi mọi thứ đã bị bắn sứt.

---

### 4.4 Dibu — Tier B — thủ môn (ô đặc biệt)

Không ghi bàn. Chỉ đảm bảo đối phương cũng không.

| Cấp | Giá | Tầm | Cộng dồn |
|-----|-----|-----|----------|
| Lv1 | 140 | 2.0 | 140 |
| Lv2 | 112 | 2.5 | 252 |
| Lv3 | 224 | 3.0 | **476** |

| Cấp | Kỹ năng mở | Nội dung |
|-----|-----------|----------|
| **Lv1** | `Cản Phá` | Mỗi **20s**, chặn **1** cổ động viên chạm vạch cầu môn: bị đẩy văng, biến mất, **KHÔNG trừ máu**, **KHÔNG rơi tiền** |
| **Lv2** | `Áp Đảo` | Mọi cổ động viên trong tầm bị **chậm 30%**. Luôn bật, không cooldown. |
| **Lv3** | `Người Hùng Luân Lưu` | Mỗi **3 lần cản thành công** → **hồi 1 máu cầu môn** (tối đa về 20) |

**Về `Cản Phá`:**
- Không phân biệt loại — cản `Tambor` (2 máu) hay `O Capitão` (**5 máu**) cũng chỉ tốn 1 lượt cản
- **Để dành cho boss.** Cản một `O Capitão` = cứu 25% máu cầu môn bằng một kỹ năng miễn phí.

**Điểm mạnh:** lưới an toàn cuối cùng. `Người Hùng Luân Lưu` ở Lv3 là nguồn hồi máu **duy nhất không tốn tiền** trong game. `Áp Đảo` kéo dài thời gian mọi tướng khác được bắn vào đoạn cuối đường.
**Điểm yếu:** 0 sát thương ở mọi cấp. 476 Peso để max là số tiền không nhỏ ở Act 1.

---

### 4.5 El Árbitro — Trọng tài — chậm vĩnh viễn, khắc chế tank

Ông ấy hoàn toàn công tâm. Chỉ là túi quần hơi nặng.

| Cấp | Giá | Tầm | Cộng dồn |
|-----|-----|-----|----------|
| Lv1 | 240 | 3.5 | 240 |
| Lv2 | 192 | 4.0 | 432 |
| Lv3 | 384 | 4.5 | **816** |

| Cấp | Kỹ năng mở | Nội dung |
|-----|-----------|----------|
| **Lv1** | `Còi Chỉ Tay` | **Aura thường trực**: mọi cổ động viên **trong tầm** bị **chậm 25%**. Hết tầm là hết chậm. Không cooldown. |
| **Lv2** | `Thẻ Vàng` | Mỗi **8s**, rút thẻ cho con **đi đầu trong tầm chưa có thẻ**: **chậm 50% VĨNH VIỄN** — theo nó tới hết đường, kể cả ra khỏi tầm |
| **Lv3** | `Thẻ Đỏ` | Mỗi **8s**, nếu con đi đầu trong tầm **đã có thẻ vàng** → rút thẻ đỏ: **chậm 70% VĨNH VIỄN** (kịch cap) |

**Trọng tài không gây một điểm sát thương nào. Ở bất kỳ cấp nào.**

#### Aura ≠ thẻ — khác biệt sống còn

| | `Còi Chỉ Tay` (aura) | `Thẻ Vàng` / `Thẻ Đỏ` |
|---|---|---|
| Phạm vi | Chỉ **trong tầm** | **Vĩnh viễn**, theo quân tới hết đường |
| Đối tượng | Mọi quân trong tầm | **1 con mỗi 8s** |
| Giá trị thật | Nhỏ — cục bộ | **Lớn** — nhân đôi thời gian con đó ở trên sân |

Một con bị thẻ vàng đi ở **50% tốc độ suốt quãng đường còn lại** → mọi tướng phía sau có **gấp đôi thời gian** bắn nó. Thẻ đỏ (30% tốc độ) → **gấp 3.3 lần**.

#### Vì sao đây là tướng khắc chế tank, không phải tướng làm chậm

Kỹ năng thẻ chọn **con đi đầu trong tầm**. Quân càng chậm càng ở trong tầm lâu → càng dễ ăn thẻ. Và khi đã ăn thẻ thì càng chậm hơn → **ở lại lâu hơn → ăn tiếp thẻ đỏ**.

Vòng lặp đó tự dồn vào đúng những thứ chậm nhất game: **Tambor** (tốc 0.6) và **`O Capitão`** (tốc 0.4). Adepto tốc 1.0 thường vọt qua tầm trước khi tới lượt rút thẻ.

💡 *Đây là lý do Trọng tài không đè lên D10S. D10S khắc chế **đám đông** — càng nhiều mục tiêu càng mạnh. Trọng tài khắc chế **cá thể lì đòn** — càng ít mục tiêu, càng chậm, càng mạnh. Hai tướng cùng "làm chậm" nhưng giải hai bài toán ngược nhau.*

#### Trước `O Capitão`

Boss **kháng chậm 75%** (`03` §2), nên mọi con số chỉ còn **1/4**:

| Hiệu ứng | Quân thường | `O Capitão` |
|----------|-------------|-------------|
| `Còi Chỉ Tay` | 25% | **6.25%** |
| `Thẻ Vàng` | 50% | **12.5%** |
| `Thẻ Đỏ` | 70% | **17.5%** (cap boss) |

Boss chậm 17.5% vĩnh viễn = ở trên sân lâu hơn **21%** = mọi tướng có thêm 21% thời gian bắn nó. Đáng, nhưng không giải hộ được con boss.

⚠️ **Kháng chậm boss vốn là 50%, nâng lên 75% CHÍNH VÌ tướng này.** Ở 50%, Trọng tài một mình đưa headroom boss W20 từ 1.18× lên **1.68×** — xoá sổ bài kiểm tra căng nhất game. Xem `03` §2. Nếu ai đó chỉnh nó về 50%, Trọng tài sẽ lặng lẽ phá boss lần nữa.

⚠️ **Boss KHÔNG bị truất quyền thi đấu.** Thẻ đỏ ở game này chỉ làm chậm, không xoá sổ ai — kể cả quân thường. Đây là quyết định cân bằng có chủ đích (xem `OPEN-QUESTIONS` bảng cuối, mục 15).

**Điểm mạnh:** biến `Nhãn Quan` (La Pulga, +25% lên mục tiêu bị chậm) và `Bàn Thắng Thế Kỷ` (D10S Lv3, +20% cho **cả sân**) từ **thỉnh thoảng** thành **gần như luôn bật**. Giá trị thật của Trọng tài nằm ở cột DPS của những tướng khác, không phải của nó.

**Điểm yếu:** **0 DPS mà vẫn chiếm 1 ô sân.** Ở Lv1, aura 25% cục bộ không bù nổi việc mất một tướng sát thương — Trọng tài là **khoản đầu tư dài hạn, mua rồi phải nâng**, không phải tướng lấp chỗ. Xem `04` §3.4 về ngưỡng hoà vốn.

**Vì sao spam Trọng tài là tự sát:** nhiều aura **không cộng dồn** (luật max, `01` §8 **FM-14**). 5 con Trọng tài = vẫn chậm 25%, chỉ phủ rộng hơn — và **0 sát thương × 5 = thua**. Luật max tự bảo vệ game, không cần giới hạn số lượng.

---

## 5. Bảng so sánh hiệu quả đầu tư

Chỉ tính các tướng gây sát thương (Dibu và El Árbitro là utility, không so được). **Chưa tính kỹ năng** — xem cảnh báo bên dưới.

| Tướng | Đầu tư để max | DPS ở Lv3 | DPS / Peso | DPS / **ô** |
|-------|--------------|-----------|-----------|-------------|
| La Pulga | 1 020 | 136 | 0.133 | **136** |
| Batigol | 544 | 90 | **0.165** | 90 |
| D10S | 952 | 60 (×n mục tiêu) | 0.063 / 0.189 khi n=3 | 60 / 180 khi n=3 |

⚠️ **Bảng này nói dối ngày càng nhiều theo cấp.** Nó chỉ đếm DPS thô, trong khi tướng Lv3 chạy 3 kỹ năng. Doc 04 §3.4 đưa hệ số **σ** để bù, nhưng σ cũng là con số ước lượng. Đừng dùng bảng này để quyết định gì ở Act 3.

**Đây là trục cân bằng chính của game:**

- **Đầu game — tiền là thứ khan hiếm.** Batigol thắng: hiệu quả nhất trên mỗi đồng Peso.
- **Cuối game — ô đặt là thứ khan hiếm** (chỉ có 11 ô, tiền thì dư). La Pulga thắng: DPS trên mỗi ô cao nhất.
- **D10S đảo lộn cả hai** khi wave đông — và tất cả các wave sau W6 đều đông. Ở Lv3, `Bàn Thắng Thế Kỷ` buff cả sân, nên giá trị thật của nó không nằm ở cột DPS nào cả.
- **El Árbitro không có mặt trong bảng này và đó chính là vấn đề của nó.** 0 DPS, 0 DPS/Peso, 0 DPS/ô — mọi cột đều bằng không. Giá trị của nó nằm hoàn toàn ở việc **nhân sức mạnh 10 ô còn lại**. Không có thước đo nào ở đây đo được nó; chỉ mô hình `04` §3.4 đo được, và mô hình đó dựa trên một hệ số bịa.

💡 *Why: nếu một tướng thắng ở cả hai giai đoạn thì 4 tướng còn lại là đồ trang trí. Việc "cái gì khan hiếm" đổi từ tiền sang ô đặt ở giữa trận chính là thứ ép người chơi phải bán và xây lại đội hình — đó là quyết định thú vị nhất mà thể loại này có.*

Kiểm chứng bằng số cho từng wave: [`04-ECONOMY-BALANCE.md`](04-ECONOMY-BALANCE.md) §4.

---

## 6. Tổng hợp 15 kỹ năng

**ID kỹ năng theo cấp** (sinh từ `config/towers.json` — mô tả đầy đủ ở §4):

<!-- GEN:abilities -->
| Đơn vị | Lv1 | Lv2 | Lv3 |
|--------|-----|-----|-----|
| **La Pulga** | `solo_run`<br>×2 dame lên mục tiêu máu cao nhất, 4s / mỗi 12s | `nhan_quan`<br>+25% dame lên mục tiêu **đang bị chậm**; không trượt | `so_10`<br>Mỗi đòn thứ 4 xuyên qua 2 quân phía sau |
| **D10S** | `ban_tay_cua_chua`<br>Chậm 50% trong tầm, 3s / mỗi 15s | `cu_cham_thien_tai`<br>Bán kính lan rộng hơn — 1.2 (Lv1) → 1.7. Con số nằm ở `levels[1].splashRadius`. | `ban_thang_the_ky`<br>Mọi tướng +20% dame lên mục tiêu bị chậm — nhưng D10S Lv3 KHÔNG tự làm chậm được nữa |
| **El Cinco** | `cu_dam`<br>**Đấm** → nổ nhỏ tại vị trí quái. Mọi quân trong vùng nổ bị trừ máu. | `cu_da`<br>**Đá** → nổ vừa, dame mạnh hơn. Vùng nổ rộng hơn cấp 1. | `cu_dap`<br>**Đạp** → nổ lớn, dame rất mạnh. Vùng nổ rộng nhất. |
| **Dibu** | `can_pha`<br>**20%** mỗi lần thử: xoá sổ quân bất kỳ, kể cả boss. Có rơi tiền như cú giết thường. | `ap_dao`<br>**30%** mỗi lần thử: xoá sổ quân bất kỳ, kể cả boss. Có rơi tiền như cú giết thường. | `nguoi_hung_luan_luu`<br>**40%** mỗi lần thử: xoá sổ quân bất kỳ, kể cả boss. Có rơi tiền như cú giết thường. |
| **El Árbitro** | `coi_chi_tay`<br>**Aura** chậm 25% — chỉ trong tầm | `the_vang`<br>Aura nền chậm **25%** MỌI con trong tầm + ném **thẻ vàng** 1 con / 4s → con trúng chậm **45% VĨNH VIỄN** | `the_do`<br>Aura nền chậm **25%** MỌI con trong tầm + ném **thẻ đỏ** 1 con / 4s → con trúng chậm **65% VĨNH VIỄN** |
<!-- /GEN:abilities -->

### Cơ chế chậm là trục trung tâm — và giờ thì rõ hẳn

**5/6 combo đi qua "làm chậm":**

| Combo | Cách hoạt động |
|-------|----------------|
| **El Árbitro Lv2 + La Pulga Lv2** | Thẻ vàng chậm **vĩnh viễn** → `Nhãn Quan` +25% **luôn bật** trên con đó |
| **El Árbitro Lv2 + D10S Lv3** | Thẻ vàng chậm vĩnh viễn → `Bàn Thắng Thế Kỷ` +20% cho **cả sân**, không còn phụ thuộc cooldown 15s |
| **El Árbitro Lv3 + Batigol** | Thẻ đỏ ghim tank ở 30% tốc → Batigol (2.5s/phát) cuối cùng cũng bắn kịp thứ gì đó |
| D10S Lv1 + La Pulga Lv2 | `Bàn Tay Của Chúa` chậm 50% → `Nhãn Quan` +25% dame (bùng nổ, 20% uptime) |
| Dibu Lv2 + tướng gần cầu môn | `Áp Đảo` chậm 30% → mọi tướng cuối đường có thêm thời gian bắn |
| Batigol Lv1 + Lv2 | `Sút Xuyên` bắn xuyên hàng dọc, `Bản Năng Sát Thủ` +30% vì quân vừa spawn còn full máu |

💡 *Việc thêm Trọng tài không tạo ra trục mới — nó **hoàn thiện trục đã có**. `Nhãn Quan` và `Bàn Thắng Thế Kỷ` được thiết kế từ vòng trước với uptime 20%; giờ có một tướng biến chúng thành thường trực. Đó là điều làm nó nguy hiểm, không phải con số 25% của aura.*

⚠️ **Và đó cũng chính là rủi ro cân bằng lớn nhất của bản này.** Một tướng 0 DPS có thể là tướng mạnh nhất game vì nó nhân sức mạnh mọi tướng khác. Mô hình ở `04` §3.4 thêm hệ số `τ` để bù — nhưng `τ` là **giả định thứ ba** chồng lên hai giả định đã có. Đọc `04` §8 trước khi tin bảng số.

### Hai tướng "làm chậm" giải hai bài toán ngược nhau

| | D10S | El Árbitro |
|---|------|-----------|
| Khắc chế | **Đám đông** — càng nhiều mục tiêu càng mạnh | **Cá thể lì đòn** — càng chậm càng mạnh |
| Kiểu chậm | Bùng nổ 50–70%, 3s mỗi 15s, **cục bộ** | Thẻ 50–70%, **vĩnh viễn**, theo quân |
| Mục tiêu tự nhiên | Adepto, Tifoso (đi thành đàn) | Tambor, `O Capitão` (đi chậm, ở lâu trong tầm) |
| Trước boss | **Sụp** — mất số nhân ×3, còn 60 DPS | **Toả sáng** — boss chậm nhất game = mồi ngon nhất của thẻ |

Chúng bù nhau đúng ở chỗ đối phương mạnh nhất của mỗi bên.
