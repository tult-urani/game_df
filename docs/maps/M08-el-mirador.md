# M08 — El Mirador ★★★★

> **2 tuyến · 10 ô sân + 1 ô thủ môn · boss kháng chậm 85%**
> Mọi toạ độ đã kiểm bằng bản copy `tools/path_check.py` (đa tuyến) trong thư mục nháp.
> Mọi con số cân bằng dưới đây là **ĐÍCH CẦN ĐO** bằng test 20-wave chạy engine thật.
> ⚠️ `tools/balance_sim.py` báo sai (nói dễ, engine thật thắng sát nút) — **không dùng nó để chốt số**.

---

## 0. Số chốt — sinh từ config

> Bảng dưới đây là **số đang chạy trong game**, sinh từ `config/maps/` bằng
> `tools/gen_docs.py`. Các bảng thiết kế ở những mục sau là **bản vẽ ban đầu**,
> viết trước khi đo trên engine — giữ lại làm lịch sử ý đồ, KHÔNG phải số thật.

<!-- GEN:map_m08 -->
**Nguồn: `config/maps/m08-*.json` — sinh bằng `tools/gen_docs.py`. Đừng sửa tay; sửa JSON rồi chạy lại.**

| | |
|---|---|
| Tuyến | 2 — `W` 25.3u · `E` 25.3u |
| Ô sân / thủ môn | 14 / 1 |
| Σchord@1.4 mỗi ô | 2.44 |
| Tiền khởi đầu | 800 |
| Máu quái | ×0.67 × 1.07^(W−1) → W20 ×2.42 |
| Tổng quân 20 wave | 335 |

| W | ×máu | `W` | `E` | boss | quân | tổng máu |
|---|------|------|------|------|------|----------|
| 1 | ×0.67 | 8/0/0 | — | — | 8 | 536 |
| 2 | ×0.72 | — | 11/0/0 | — | 11 | 792 |
| 3 | ×0.77 | 8/4/0 | — | — | 12 | 1 292 |
| 4 | ×0.82 | — | 8/8/0 | — | 16 | 2 104 |
| 5 | ×0.88 | 0/5/0 | 1/5/1 | — | 12 | 2 501 |
| 6 | ×0.94 | 0/6/1 | 0/6/1 | — | 14 | 3 518 |
| 7 | ×1.01 | 0/6/1 | 1/5/2 | — | 15 | 4 191 |
| 8 | ×1.08 | 1/5/1 | 0/5/0 | — | 12 | 3 070 |
| 9 | ×1.15 | 0/6/1 | 0/6/1 | — | 14 | 4 302 |
| **10** | ×1.23 | 0/4/2 | 0/5/2 | `o_capitao`@`E` | 14 | 7 647 |
| 11 | ×1.32 | 0/5/1 | 1/4/2 | — | 13 | 4 917 |
| 12 | ×1.41 | 1/5/3 | 0/5/2 | — | 16 | 7 121 |
| 13 | ×1.51 | 0/8/2 | 0/7/2 | — | 19 | 8 300 |
| 14 | ×1.61 | 1/6/3 | 0/7/3 | — | 20 | 10 104 |
| 15 | ×1.73 | 0/5/2 | 1/5/3 | — | 16 | 8 723 |
| 16 | ×1.85 | 1/6/2 | 0/6/2 | — | 17 | 9 137 |
| 17 | ×1.98 | 0/7/3 | 0/7/3 | — | 20 | 12 618 |
| 18 | ×2.12 | 0/9/3 | 0/10/3 | — | 25 | 15 838 |
| 19 | ×2.26 | 0/9/4 | 1/9/5 | — | 28 | 20 404 |
| **20** | ×2.42 | 0/11/5 | 0/13/5 | `o_capitao`@`E` | 35 | 35 922 |

Ba số trong ô tuyến là `adepto/tifoso/tambor`.
<!-- /GEN:map_m08 -->

---

## 1. Ý đồ thiết kế

Map này kiểm tra **kỷ luật nâng cấp trước khi trải rộng**. Đa số ô nằm cách đường 1.30–1.68 units, tức Lv1 của El Cinco (1.0), D10S (1.2) và Árbitro (1.2) **không với tới** — chỉ La Pulga Lv1 (1.4) và các tướng Lv2+ dùng được. Người chơi quen "mua nhiều tướng rẻ" sẽ đặt tướng vào ô câm và thấy nó đứng im.

Đồng thời map tách quân làm hai tuyến, ép chia hoả lực: một tuyến dài quanh co (**Ladera**, 24.83u) và một tuyến ngắn hơn (**Cornisa**, 23.56u). Chỉ **2 ô** (`f03`, `f10`) canh được cả hai tuyến, và cả hai đều là ô tầm xa.

Cuối cùng, boss kháng chậm 85% xoá gần hết giá trị của việc kéo dài thời gian phơi sáng — boss phải chết bằng **sát thương đơn mục tiêu thô**, không bằng mẹo làm chậm.

---

## 2. Toạ độ waypoint

Nội suy: **Catmull-Rom**, giống map gốc. Cả hai tuyến chạm cùng một `goalPoint` `(0.0, -9.13)`.

### Tuyến W — "Ladera" (dài, quanh co)

| # | x | y |  | # | x | y |
|---|---|---|---|---|---|---|
| 1 | -3.40 | 9.36 | (spawn W) | 7 | -4.60 | -0.90 |
| 2 | -4.70 | 7.90 | | 8 | -3.90 | -3.40 |
| 3 | -4.50 | 5.40 | | 9 | -2.00 | -4.70 |
| 4 | -2.60 | 4.40 | | 10 | -2.70 | -6.90 |
| 5 | -1.30 | 2.40 | | 11 | -1.20 | -8.30 |
| 6 | -2.90 | 0.90 | | 12 | 0.00 | -9.13 | (goal) |

### Tuyến E — "Cornisa" (ngắn, ít khúc)

| # | x | y |  | # | x | y |
|---|---|---|---|---|---|---|
| 1 | 3.40 | 9.36 | (spawn E) | 6 | 3.60 | -0.70 |
| 2 | 4.60 | 7.60 | | 7 | 3.20 | -3.30 |
| 3 | 3.60 | 5.20 | | 8 | 1.40 | -5.10 |
| 4 | 1.60 | 3.40 | | 9 | 2.30 | -7.30 |
| 5 | 1.30 | 1.20 | | 10 | 0.00 | -9.13 | (goal) |

### Kết quả đo

| Tuyến | Độ dài spline | Hộp bao x | Hộp bao y | Tràn viewport |
|-------|---------------|-----------|-----------|----------------|
| W (Ladera) | **24.83 u** | [-4.81, 0.00] | [-9.13, 9.36] | ❌ không |
| E (Cornisa) | **23.56 u** | [0.00, 4.60] | [-9.13, 9.36] | ❌ không |

Viewport `x ∈ [-5.40, 5.40]`, `y ∈ [-9.60, 9.60]` — cả hai tuyến nằm trọn trong khung, lề ngang nhỏ nhất 0.59u (tuyến W tại x = -4.81).

🔴 **Hệ quả CHÍNH của map hai tuyến:** mỗi con quân chỉ đi **~24u** thay vì 42.28u của map gốc → thời gian phơi sáng của MỘT con giảm ~43%. Bù lại quân chia đôi nên mỗi tuyến chỉ chịu một nửa lưu lượng. Tổng hoả lực cần thiết xấp xỉ không đổi, **nhưng phân bổ sai tuyến là thua ngay** — đó chính là bài kiểm tra.

---

## 3. Toạ độ ô

`d` = khoảng cách vuông góc nhỏ nhất tới spline của tuyến tương ứng.

| id | type | x | y | d[W] | d[E] | f[W] | f[E] | Tướng với tới ở **Lv1** | Ghi chú |
|----|------|------|------|------|------|------|------|--------------------------|---------|
| `f01` | field | -3.92 | 6.71 | **0.88** | 6.44 | 0.13 | — | El Cinco · D10S · Árbitro · Pulga | ô mở màn tuyến W |
| `f02` | field | 3.61 | 7.03 | 6.44 | **0.88** | — | 0.13 | El Cinco · D10S · Árbitro · Pulga | ô mở màn tuyến E |
| `f03` | field | -0.15 | 1.72 | **1.30** | **1.30** | 0.38 | 0.39 | **Pulga** | 🔑 **ô eo** — canh CẢ HAI tuyến |
| `f04` | field | -2.33 | 2.70 | **0.91** | 3.53 | 0.34 | — | El Cinco · D10S · Árbitro · Pulga | 🔒 khoá synergy, cặp với `f03` |
| `f05` | field | -3.09 | -1.26 | **1.51** | 5.03 | 0.53 | — | — | ⛰️ **ô tầm xa** (cần R ≥ 1.6) |
| `f06` | field | -1.80 | -3.37 | **1.10** | 3.62 | 0.74 | — | D10S · Árbitro · Pulga | |
| `f07` | field | 1.66 | 5.25 | 3.83 | **1.30** | — | 0.27 | **Pulga** | |
| `f08` | field | 2.17 | -1.29 | 4.92 | **1.48** | — | 0.51 | — | ⛰️ **ô tầm xa** (cần R ≥ 1.6) |
| `f09` | field | 1.09 | -3.71 | 3.24 | **1.09** | — | 0.74 | D10S · Árbitro · Pulga | |
| `f10` | field | 0.02 | -7.12 | **1.68** | **1.68** | 0.95 | 0.95 | — | 🔭 **El Mirador** — canh cả hai tuyến, **chỉ Pulga Lv3 (1.8) với tới** |
| `gk01` | goalkeeper | 0.00 | -8.60 | **0.44** | **0.44** | 0.99 | 0.99 | Dibu (mọi cấp) | canh đoạn cuối cả hai tuyến |

### Thang tầm — ô nào dùng được ở Lv1

| Tướng | Tầm Lv1 | Số ô với tới |
|-------|---------|--------------|
| El Cinco | 1.0 | **3/10** (`f01` `f02` `f04`) |
| D10S | 1.2 | **5/10** (+ `f06` `f09`) |
| El Árbitro | 1.2 | **5/10** |
| La Pulga | 1.4 | **7/10** (+ `f03` `f07`) |
| — không tướng Lv1 nào | | **3/10**: `f05` (1.51) · `f08` (1.48) · `f10` (1.68) |

**Ô cố ý là "ô tầm xa":** `f05`, `f08` (cần R ≥ 1.6 → Pulga Lv2 / D10S Lv3 / Árbitro Lv3) và `f10` (cần R ≥ 1.8 → **chỉ Pulga Lv3**).
🔴 `f10` có một đánh đổi cố ý: Pulga **Lv2** (tầm 1.6) KHÔNG với tới, phải lên **Lv3** — mà Lv3 đổi kỹ năng từ "+25% lên mục tiêu bị chậm" sang "xuyên 2 quân". Muốn dùng Mirador thì **phải bỏ synergy làm-chậm** trên chính con tướng đó.

**Chống bế tắc W1:** ba ô `f01` `f02` `f04` (d 0.88–0.91) nhận mọi tướng ngay Lv1. Với 700 Peso khởi đầu, khai cuộc chuẩn là El Cinco `f01` (120) + El Cinco `f02` (120) + Pulga `f03` (300) = **540** — đã có mặt trên cả hai tuyến và một ô eo canh chung.

### Ràng buộc vật lý (đã kiểm)

| Ràng buộc | Kết quả |
|-----------|---------|
| Ô chết (không tướng Lv1 nào với tới, không khai `farSlot`) | 0 — 3 ô tầm xa đều khai `farSlot: true` |
| Ranh giới dao cạo (\|d − tầm\| < 0.05) | 0 vi phạm |
| Cặp ô gần nhau nhất | `f10`–`gk01` = 1.48 u = **54 pt** ≥ 48 pt ✅ |
| Vùng chạm 48pt tràn mép khung | 0 vi phạm |
| chord thật / 2R (tổng trên mọi tuyến) | R=1.4: tb **0.50** · R=1.8: tb **0.93** |

🔴 **Đoạn hở** (luật gốc `MAX_GAP = 0.15` **không áp dụng được** cho map đa tuyến — xem §8):
- tuyến W hở **21%** (5.3u) tại f = 0.74 → 0.95
- tuyến E hở **23%** (5.4u) tại f = 0.51 → 0.74

Với 10 ô chia cho 2 tuyến, sàn toán học của đoạn hở là ~1/(số ô canh mỗi tuyến + 1) ≈ 0.17. **Không tồn tại cách xếp nào đạt 0.15.** Đoạn hở tuyến W trước ô Mirador là **cố ý**: `f10` chính là câu trả lời cho nó, và nó chỉ mở ra khi người chơi có Pulga Lv3.

---

## 4. Bảng phủ synergy

Nhắc luật: nguồn chậm là **El Árbitro** (thẻ vàng 50% / thẻ đỏ 70% — **vĩnh viễn, theo người**) hoặc **D10S Lv1** (50% trong 3s, mỗi 15s — **cục bộ, theo tầm**). Bên hưởng là **D10S Lv3** (+20% cho MỌI tướng lên mục tiêu bị chậm) và **La Pulga Lv2** (+25% lên mục tiêu bị chậm).

### 4a. Chồng lấn hình học (bắt buộc cho nguồn chậm CỤC BỘ — D10S Lv1, aura Árbitro)

| Ô làm chậm | Ô sát thương | Tuyến | Giao ở R 1.4 × 1.6 | Giao ở R 1.6 × 1.8 |
|------------|--------------|-------|--------------------|--------------------|
| `f04` | `f03` | W | **1.17 u** | **1.65 u** |
| `f03` | `f04` | W | 0.63 u | 1.17 u |
| *mọi cặp khác* | | | **0.00 u** | **0.00 u** |

🔴 **Phát hiện quan trọng:** ngoài cặp `f04`–`f03`, **không có cặp ô nào khác trên map giao nhau** — kể cả ở tầm lớn nhất game. Nguyên nhân là luật chống-đoạn-hở: ô càng rải đều theo quãng đường thì vòng tròn tầm càng không chạm nhau. **Rải đều và synergy cục bộ là hai mục tiêu loại trừ nhau ở ngân sách 10 ô.** Cặp `f04`–`f03` là "ổ khoá" được đặt **có chủ đích** (hai ô ở hai phía cùng một đoạn đường), và cái giá của nó là đoạn hở tuyến W tăng từ 0.21 lên đúng mức hiện tại.

### 4b. Thẻ vĩnh viễn — không cần chồng lấn

Thẻ của Árbitro theo con quân tới hết đường (`docs/01` §8 FM-16). Nên **mọi ô nằm SAU ô Árbitro đều hưởng lợi**, dù vòng tròn tầm không chạm nhau.

| Ô đặt Árbitro | Canh được tuyến (ở Lv3, R=1.6) | % quãng đường nằm SAU ô — W / E |
|---------------|-------------------------------|----------------------------------|
| `f01` | W | 87% / 68% |
| `f02` | E | 67% / 87% |
| **`f03`** | **W + E** | **62% / 61%** |
| `f04` | W | 73% / 68% |
| `f05` | W | 47% / 58% |
| `f07` | E | 67% / 73% |
| `f08` | E | 61% / 49% |

**Kết luận bố trí:** Árbitro nên đứng ở `f04` (Lv1 với tới ngay, 73% tuyến W nằm sau) hoặc `f03` (Lv3 mới với tới cả hai tuyến, nhưng phủ 62%/61% — cân bằng nhất). Ô sát thương đặt xuôi dòng: `f05` `f06` `f10` (tuyến W), `f08` `f09` `f10` (tuyến E).

---

## 5. Bảng 20 wave

`hp = baseHp × 0.67 × 1.09^(wave-1)` · spawn cách 0.7s · nghỉ 8s (skip 3 Peso/giây).
Cột **W/E** ghi số quân đi tuyến nào. Boss ra **sau cùng** trong wave.

| W | Adepto (W/E) | Tifoso (W/E) | Tambor (W/E) | Boss | Quân | Tổng máu | Thưởng hạ | Clear | Tiền wave | Ví cộng dồn |
|---|--------------|--------------|--------------|------|------|----------|-----------|-------|-----------|-------------|
| 1 | 8 (8/0) | — | — | — | 8 | 536 | 40 | 20 | 60 | 760 |
| 2 | 12 (6/6) | — | — | — | 12 | 876 | 60 | 25 | 85 | 845 |
| 3 | 10 (5/5) | 4 (0/4) | — | — | 14 | 1 500 | 94 | 30 | 124 | 969 |
| 4 | 10 (5/5) | 8 (2/6) | — | — | 18 | 2 398 | 138 | 35 | 173 | 1 142 |
| 5 | 8 (4/4) | 6 (1/5) | 1 (1/0) | — | 15 | 2 528 | 130 | 40 | 170 | 1 312 |
| 6 | 10 (5/5) | 8 (2/6) | 1 (1/0) | — | 19 | 3 413 | 162 | 45 | 207 | 1 519 |
| 7 | 8 (4/4) | 8 (2/6) | 3 (3/0) | — | 19 | 4 726 | 200 | 50 | 250 | 1 769 |
| 8 | 8 (4/4) | 6 (1/5) | 2 (2/0) | — | 16 | 3 938 | 214 | 55 | 269 | 2 038 |
| 9 | 10 (5/5) | 8 (2/6) | 2 (2/0) | — | 20 | 5 160 | 258 | 60 | 318 | 2 356 |
| **10** | 8 (4/4) | 8 (2/6) | 3 (3/0) | **1 → E** | 20 | **8 128** | 258 + **200** | 65 | 523 | 2 879 |
| 11 | 10 (5/5) | 10 (3/7) | 3 (3/0) | — | 23 | 7 696 | 322 | 70 | 392 | 3 271 |
| 12 | 12 (6/6) | 10 (3/7) | 4 (4/0) | — | 26 | 9 680 | 370 | 75 | 445 | 3 716 |
| 13 | 10 (5/5) | 14 (4/10) | 4 (4/0) | — | 28 | 11 834 | 416 | 80 | 496 | 4 212 |
| 14 | 14 (7/7) | 13 (4/9) | 5 (4/1) | — | 32 | 14 396 | 463 | 85 | 548 | 4 760 |
| 15 | 12 (6/6) | 10 (5/5) | 4 (2/2) | — | 26 | 12 542 | 484 | 90 | 574 | 5 334 |
| 16 | 12 (6/6) | 14 (7/7) | 4 (2/2) | — | 30 | 15 814 | 564 | 95 | 659 | 5 993 |
| 17 | 14 (7/7) | 14 (7/7) | 5 (3/2) | — | 33 | 19 229 | 626 | 100 | 726 | 6 719 |
| 18 | 14 (7/7) | 16 (8/8) | 6 (3/3) | — | 36 | 23 838 | 710 | 105 | 815 | 7 534 |
| 19 | 16 (8/8) | 16 (8/8) | 8 (4/4) | — | 40 | 30 080 | 816 | 110 | 926 | 8 460 |
| **20** | 18 (9/9) | 20 (10/10) | 10 (5/5) | **1 → E** | 49 | **48 302** | 1 002 + **450** | 150 | 1 602 | **10 062** |

**Tổng:** 484 quân · 226 614 máu.

### Boss `O Capitão` trên M08

| | W10 | W20 |
|---|-----|-----|
| Máu | **2 000** | **8 000** |
| Tuyến | **E (Cornisa)** — tuyến ngắn, ít ô canh | **E (Cornisa)** |
| Thưởng | 180 | 450 |
| Tốc độ | 0.4 | 0.4 |
| **Kháng chậm** | **85%** | **85%** |
| Trừ máu khi lọt | 5 | 5 |

Kháng 85% → chậm thực tế tối đa = `cap 70% × (1 − 0.85)` = **10.5%** → tốc boss 0.358 u/s (so với 0.4 khi không chậm gì). Kéo dài thời gian phơi sáng đúng **11.7%** — gần như vô nghĩa. Đây là **đánh đổi cố ý**: boss ép bỏ build làm-chậm, chuyển sang sát thương đơn mục tiêu.

**Cách dẫn xuất máu boss (không bịa):** đo tổng chord thật của mọi ô lên tuyến E ở R=1.8 (= 15.85 u), chia cho tốc boss 0.358 → **44.3 tower-giây**. Map gốc chuẩn ở **237.5 máu / tower-giây** (boss W20 22 000 ÷ 92.6 tower-giây). Giữ nguyên chuẩn đó cho M08 rồi hạ nhẹ để headroom ≈ 1.13 → **8 000**. W10 = 0.25 × W20, đúng tỉ lệ map gốc.

**Nhịp dạy chơi:** W1 chỉ tuyến W (học map). W2 mở tuyến E. W3–W13 phân tuyến **lệch**: Tambor luôn đi W (dài, nhiều ô), Tifoso dồn về E (ngắn, nhanh). **W14 là bước ngoặt** — lần đầu Tambor ra tuyến E. Từ **W15 phân tuyến về đối xứng 50/50**: ai chỉ xây một bên vỡ ở đây, không phải ở W20.

---

## 6. Kinh tế đề xuất

| Tham số | Giá trị | Ghi chú |
|---------|---------|---------|
| `startingCash` | **700** | đủ mua 2 El Cinco + 1 Pulga ô eo (540) |
| `goalHealth` | 20 | không đổi |
| `bountyMultiplier` theo act | **1.00 / 1.40 / 1.85** | 🔴 **hạ so với 1.0 / 1.6 / 2.2 của map gốc** |
| Clear bonus | 15 + 5×W (W20 = 150) | không đổi |
| `skipBonusPerSecond` | 3 | không đổi |
| `sellRefundRatio` | 0.6 | không đổi |

### Trần chi tiêu

```
trần   = số_ô_sân × 1020 + 476 = 10 × 1020 + 476 = 10 676
tiền cả trận (700 + thưởng 7 977 + clear 1 385)  = 10 062
biên   = +614  (+5.8 %)              ← map gốc: +661 (+5.7 %) — cùng mức khan hiếm
tiền cả trận + skip tối đa (19 × 24) = 10 518  <  10 676  ✅
```

🔴 **Vì sao phải hạ `bountyMultiplier`:** map chỉ có 10 ô sân, trần thô tụt từ 11 696 (11 ô) xuống 10 676. Giữ nguyên hệ số 1.0/1.6/2.2 thì tiền cả trận = **11 298 > trần** → người chơi mua được TOÀN BỘ đội hình tối đa và cuối game hết lựa chọn. Hạ hệ số là đòn bẩy **rẻ nhất** vì nó không đụng bảng máu, không đụng số quân, không đụng hình học.

⚠️ **Cần ĐO bằng engine thật (không dùng `balance_sim.py`):** chạy test 20-wave với người chơi tối ưu, mục tiêu **headroom mỗi wave trong [1.05, 1.65]**, đích [1.10, 1.50]. Nếu vỡ sàn ở Act 1, đòn bẩy đầu tiên là `startingCash` (700 → 760), **không** phải nới tầm tướng — nới tầm làm chord siêu tuyến tính và phá thang tầm (bài học vòng 5 ở `config/path.json`).

---

## 7. Đường thắng dự kiến & build cố ý thua

### 7a. Bảng đối chứng boss W20 (8 000 máu, tuyến E, kháng chậm 85%)

Toàn bộ ô canh được tuyến đó, cùng một tướng cùng một cấp. Tổng chord đo thật, không phải `2R`.

| Đội hình | Tầm | Tổng chord tuyến E | Sát thương gây được | **Headroom** |
|----------|-----|--------------------|---------------------|--------------|
| **Pulga Lv3** (204 dps) | 1.8 | 15.85 u | **9 032** | **1.13** ✅ |
| Pulga Lv2 (108 dps) | 1.6 | 11.18 u | 3 372 | 0.42 ❌ |
| Pulga Lv1 (54 dps) | 1.4 | 5.92 u | 894 | 0.11 ❌ |
| D10S Lv3 (60 dps) | 1.6 | 11.18 u | 1 874 | 0.23 ❌ |
| D10S Lv2 (38.4 dps) | 1.4 | 5.92 u | 635 | 0.08 ❌ |
| El Cinco Lv3 (27 dps) | 1.4 | 5.92 u | 447 | 0.06 ❌ |

*(Nếu boss ra tuyến W: Pulga Lv3 = 10 086 → headroom 1.26. Tuyến E khó hơn — đó là lý do boss ra tuyến E.)*

### 7b. Build CỐ Ý THUA — "đám đông"

**8 × El Cinco Lv3 + 2 × D10S Lv2**, chi ~4 100 Peso. Dọn W1–W19 rất tốt (nổ lan 1.4 chống 24 quân/wave), nhưng ở W20 gây **≤ 450** sát thương lên boss 8 000 → **headroom 0.06**. Boss lọt → **−5 máu**. Và vì El Cinco Lv3 tầm 1.4 nên `f05` `f08` `f10` là ô câm — mất 3/10 ô.

### 7c. Đường thắng — "hai gọng, một mũi khoan"

| Wave | Hành động | Chi | Luỹ kế |
|------|-----------|-----|--------|
| W1 | El Cinco `f01` (120) · El Cinco `f02` (120) · Pulga `f03` (300) | 540 | 540 |
| W3–4 | Pulga `f07` (300) — chặn Tifoso trên E | 300 | 840 |
| W5–7 | El Árbitro `f04` (240) → **ổ khoá `f04`×`f03`** · nâng Pulga `f03` → Lv2 (240) | 480 | 1 320 |
| W8–10 | D10S `f06` (240) + `f09` (240) · Dibu `gk01` (140) | 620 | 1 940 |
| W11–14 | Pulga `f05` **Lv2** (300+240=540) — mở ô tầm xa · nâng `f07` → Lv2 (240) · Árbitro `f04` → Lv2 (192) | 972 | 2 912 |
| W15–17 | Pulga `f08` Lv2 (540) · nâng `f03` → **Lv3** (480) · D10S `f06` → Lv3 (192+384=576) | 1 596 | 4 508 |
| W18–19 | **Pulga `f10` Lv3** (300+240+480 = 1 020) — mở El Mirador, bịt đoạn hở f 0.74→0.95 | 1 020 | 5 528 |
| W20 | nâng `f07` → Lv3 (480) · nâng `f08` → Lv3 (480) · Dibu → Lv3 (336) | 1 296 | **6 824** |

Chi 6 824 / có 10 062 → **dư 3 238** cho sai lầm, bán-xây-lại và sink sửa cầu môn. Bốn nguồn sát thương Lv3 trên tuyến E ở W20 (`f03` `f07` `f08` `f10`) + Dibu `Cản Phá` là đường thắng.

### 7d. Chứng minh vẫn có đường thắng dù kháng chậm 85%

1. **Đường thắng không phụ thuộc làm chậm.** Bảng 7a đo với tốc boss 0.358 (đã tính kháng 85%): Pulga Lv3 vẫn đạt **1.13**. Không cần một điểm chậm nào.
2. **Nếu luật buff là "bị chậm > 0"** (đúng như code hiện tại `AbilityEngine.cs:130`): thẻ Árbitro vẫn dán được lên boss (10.5% hiệu lực) → cờ "bị chậm" **vẫn bật** → D10S Lv3 cộng +20% cho mọi tướng → 9 032 × 1.2 = **10 838**, headroom **1.35**. Dư dả.
3. **Nếu luật đổi thành ngưỡng** (ví dụ buff chỉ chạy khi chậm thực tế ≥ 15%) thì boss M08 **không bao giờ** kích buff — và đường thắng ở mục 1 vẫn đứng. **Cả hai cách đọc luật đều có đường thắng.** Xem §8 mục 6, đây là quyết định luật phải chốt.
4. **Van an toàn cuối:** Dibu `Cản Phá` cản được boss (`docs/03` §6 — "cản được, và đó là điểm mấu chốt"). Cầu môn 20 máu chịu được **tối đa 3 lần** boss/Tambor lọt trước khi thua.

---

## 8. Yêu cầu kỹ thuật

Hiện tại `MatchController` chỉ giữ **MỘT** `EnemyPath`; `Enemy` và `ScheduledSpawn` **không có** trường tuyến. Map này không chạy được nếu không có các thay đổi sau.

| # | Hạng mục | Cần làm |
|---|----------|---------|
| 1 | `config/path.json` schema | `path: {…}` → `paths: [{ id, spawnPoint, goalPoint, waypoints, lengthUnits }]`. `slots[]` thêm `farSlot: bool` (đánh dấu ô cố ý ngoài tầm Lv1). |
| 2 | `MatchController` | `EnemyPath` → `IReadOnlyDictionary<string, EnemyPath>`. Mọi chỗ `_path.PositionAt(t)` phải tra theo `enemy.PathId`. |
| 3 | `Enemy` | Thêm `string PathId` (bất biến sau khi spawn — quân **không đổi tuyến**). |
| 4 | `ScheduledSpawn` / `WaveSpawner` | Mỗi nhóm quân trong wave khai `lane`. Thứ tự spawn xen kẽ hai tuyến theo `spawnIntervalSec` chung (không phải hai hàng đợi độc lập, nếu không hai tuyến lệch pha). |
| 5 | Boss | `slowResistPercent` phải khai **theo map** (85 ở đây, 75 ở map gốc), không phải hằng số trong `enemies.json`. Boss cũng cần `lane`. |
| 6 | 🔴 `AbilityEngine.cs:130` | `if (ctx.AnyGlobalSlowedDamageBuff && ctx.SlowPercentOn(target.Id) > 0)` — điều kiện là **nhị phân**. Kháng chậm 85% KHÔNG làm yếu buff "+20% lên mục tiêu bị chậm"; nó chỉ giảm phần *đi chậm*. Nếu ý đồ là "kháng chậm giết synergy" thì phải thêm `slowedBuffThresholdPercent` (đề xuất 15) vào `economy.json` và đổi điều kiện thành `>= threshold`. **Đây là quyết định luật, không phải bug — nhưng phải chốt trước khi đo cân bằng, vì nó lệch headroom boss 20%.** |
| 7 | `tools/path_check.py` | (a) duyệt nhiều tuyến, in `d` tới TỪNG tuyến; (b) luật ô chết bỏ qua ô có `farSlot: true`; (c) luật đoạn hở tính **theo tuyến** và nới ngưỡng: `MAX_GAP = max(0.15, laneCount × 1.2 / fieldSlotCount)` — với 2 tuyến / 10 ô = 0.24. Giữ nguyên 0.15 sẽ báo lỗi cho mọi map đa tuyến khả thi. |
| 8 | `ConfigValidator` | Bất biến `Doc_du_14_waypoint_va_12_o` viết cứng số waypoint/ô → phải đọc theo map đang tải. |
| 9 | Vẽ (`Pitch.cs` / `Draw.cs`) | Vẽ hai đường + hai vạch spawn riêng. Ô canh cả hai tuyến (`f03` `f10` `gk01`) khi hiện vòng tầm phải cho thấy nó cắt **cả hai** đường — nếu không người chơi không hiểu vì sao ô đó đắt giá. |
| 10 | Kinh tế theo map | `startingCash`, `bountyMultiplier[]`, `hpScaling` phải nằm trong file map, không ở `economy.json` toàn cục. |

---

## 9. Rủi ro

| # | Rủi ro | Mức | Đánh giá / giảm thiểu |
|---|--------|-----|-----------------------|
| R1 | **Map có bất khả thi không?** | 🟢 KHÔNG | §7a chứng minh có đội hình đạt headroom 1.13 lên boss với chi phí 6 × 1 020 = 6 120 < 10 062 tiền cả trận. Ba ô gần (`f01` `f02` `f04`) bảo đảm W1 không bế tắc. |
| R2 | Synergy cục bộ gần như không tồn tại: **chỉ 1 cặp ô** (`f04`×`f03`) giao nhau | 🔴 CAO | Đây là mâu thuẫn **cấu trúc** giữa luật chống-đoạn-hở và luật chồng-lấn-tầm ở ngân sách 10 ô. Đã xử bằng ổ khoá cố ý + dựa vào **thẻ vĩnh viễn** của Árbitro (§4b) làm kênh synergy chính. Nếu muốn thêm ổ khoá phải chấp nhận đoạn hở > 25%. |
| R3 | Ô tầm xa làm người chơi tưởng game lỗi ("tướng không gây dame") | 🟡 TRUNG | Đã từng xảy ra thật (ghi trong `config/path.json`, vòng 17: người chơi báo BA LẦN). **Bắt buộc UI**: ô `farSlot` vẽ khác màu + panel mua hiện "cần tầm ≥ 1.6" và xám các tướng không đủ tầm ở cấp đang có. |
| R4 | Đoạn hở 21–23% giữa hai ô cuối mỗi tuyến | 🟡 TRUNG | Cố ý (§3). `f10` + `gk01` bịt đoạn cuối, nhưng chỉ khi người chơi lên Pulga Lv3. Cần đo tỉ lệ lọt ở W13–W16 — nếu > 2 con/wave thì kéo `f06`/`f09` xuống f = 0.82. |
| R5 | Boss chỉ có **một** câu trả lời (Pulga Lv3) | 🟡 TRUNG | Bảng 7a cho thấy mọi lựa chọn khác ≤ 0.42. "Ép đổi đội hình" đạt được, nhưng ranh giới giữa *ép* và *chỉ có một đường* rất mỏng. Van thứ hai là Dibu `Cản Phá`; van thứ ba là chịu −5 máu. Cần đo: nếu người chơi thử nghiệm luôn thua W20 thì hạ boss xuống 7 000. |
| R6 | Phân tuyến 50/50 từ W15 có thể quá đột ngột | 🟢 THẤP | W14 đã báo trước bằng 1 Tambor lẻ ra tuyến E. Nếu telemetry cho thấy vỡ ở W15, dịch bước ngoặt sớm về W12. |
| R7 | Hai tuyến làm thời lượng trận dài hơn (spawn xen kẽ) | 🟢 THẤP | Tổng quân 484 × 0.7s = 339s spawn; đường ngắn hơn nên đuôi wave ngắn lại. Ước ~11 phút ở 1×, tương đương map gốc. |
