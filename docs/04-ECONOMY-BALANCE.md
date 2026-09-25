# 04 — Kinh tế & Cân bằng

> **Doc này giữ MÔ HÌNH và LẬP LUẬN. Nó KHÔNG giữ số.**
> Nguồn chân lý số liệu là **`config/*.json`**. Mọi bảng số dưới đây nằm giữa marker `<!-- GEN:x -->` và được **sinh tự động** bằng `tools/gen_docs.py`. Sửa tay vào đó sẽ bị ghi đè, và CI sẽ đỏ.
> ⚠️ Mô hình ở đây chồng **ba giả định** (`η × σ × τ`) — đọc §8 trước khi tin bất kỳ con số nào.
> ⚠️ Các ví dụ lịch sử dùng W10/W20 và số cũ chỉ giải thích quá trình thiết kế. Hợp đồng hiện hành: boss W5/W10/W15/W20 = 1/1/2/2; xem bảng sinh ở `03` và `config/`.

### Vì sao tách mô hình khỏi số

Bản trước, doc này tự xưng "nguồn chân lý số liệu của toàn project" — trong khi `05` §3 luật 3 nói *"Không số liệu nào hardcode trong C#. Mọi chỉ số đến từ `Resources/Config/`"*. **Hai nguồn chân lý.** Code đọc JSON; markdown thì code không đọc được.

Kết quả: cùng một lớp bug ở **cả 3 vòng review** — headroom `1.06` vs `1.08`, máu boss `7 000` vs `5 500`, `hpMultiplier` `3.2` vs `3.8`. Mỗi lần đều "sửa cẩn thận", mỗi lần grep vẫn ra chỗ sót.

Giờ: **JSON giữ số, markdown giữ lý do.** Markdown giỏi giải thích *tại sao* `550` chứ không phải `600`; nó tệ ở việc là bản sao thứ ba của con số `550`.

---

## 1. Đơn vị tiền

**Peso ⚽** — chỉ tồn tại trong 1 trận. Không mang qua trận sau, không mua bằng tiền thật, không có meta-progression.

### Nguồn tiền vào

| Nguồn | Công thức | Ghi chú |
|-------|-----------|---------|
| Khởi đầu | **700** | Cố định |
| Hạ cổ động viên | `thưởng_gốc × bountyMultiplier` | Trả cho tướng gây **đòn kết liễu**, một lần duy nhất |
| Hạ `O Capitão` | **40/80/110/180 mỗi boss** tại W5/W10/W15/W20 | Ghi thẳng, không nhân hệ số; tổng 6 boss |
| Clear wave | `20 + 5 × (wave − 1)`, riêng W20 = **150** | Trả khi con cuối cùng rời sân |
| Skip thời gian nghỉ | `giây_còn_lại × 3` | Tối đa 8 × 3 = **24**/wave |
| Bán tướng | `60% × tổng đã đầu tư` | Hoàn vốn, không phải nguồn mới |

### Nguồn tiền ra

| Sink | Giá |
|------|-----|
| Mua tướng | 140 – 300 tuỳ tướng (doc 02) |
| Nâng cấp Lv2 | `0.8 × giá mua` — mở kỹ năng #2 |
| Nâng cấp Lv3 | `1.6 × giá mua` — mở kỹ năng #3 |
| **Sửa cầu môn** | **200**, tăng **×1.5** mỗi lần dùng → 200 / 300 / 450 / 675 / 1 012 … |

**Sửa cầu môn** hồi đúng **1 máu**, tối đa về lại 20. Đây là sink duy nhất không tạo ra sức mạnh — mua nó là thừa nhận đã chơi hỏng.

💡 *Why giá tăng cấp số nhân ×1.5: để nó là phao cứu sinh chứ không phải chiến lược. Nếu giá cố định 200, người chơi tối ưu sẽ cố tình thả quân lọt lưới ở wave sớm để tiết kiệm tiền xây tướng, rồi mua máu bù sau — kinh tế vỡ hoàn toàn.*

---

## 2. Bảng số liệu tướng (đầy đủ)

<!-- GEN:tower_stats -->
| Tướng | Cấp | Giá | Sát thương | Tốc đánh | DPS | Tầm | Chord (2R) | Kỹ năng mở | Đầu tư cộng dồn |
|-------|-----|-----|-----------|----------|-----|-----|-----------|-----------|-----------------|
| **La Pulga** | 1 | 300 | 45 | 1.2 | 54 | 1.4 | 2.8 | `solo_run` | 300 |
|  | 2 | 240 | 90 | 1.2 | 108 | 1.6 | 3.2 | `nhan_quan` | 540 |
|  | 3 | 480 | 170 | 1.2 | **204** | 1.8 | 3.6 | `so_10` | **1 020** |
| **D10S** | 1 | 240 | 30 lan | 0.8 | 24 /mt | 1.2 | 2.4 | `ban_tay_cua_chua` | 240 |
|  | 2 | 192 | 48 lan | 0.8 | 38 /mt | 1.4 | 2.8 | `cu_cham_thien_tai` | 432 |
|  | 3 | 384 | 75 lan | 0.8 | **60** /mt | 1.6 | 3.2 | `ban_thang_the_ky` | **816** |
| **El Cinco** | 1 | 120 | 20 lan | 0.6 | 12 /mt | 1.0 | 2 | `cu_dam` | 120 |
|  | 2 | 96 | 30 lan | 0.6 | 18 /mt | 1.2 | 2.4 | `cu_da` | 216 |
|  | 3 | 192 | 45 lan | 0.6 | **27** /mt | 1.4 | 2.8 | `cu_dap` | **408** |
| **Dibu** | 1 | 140 | — | — | **0** | 1.4 | 2.8 | `can_pha` | 140 |
|  | 2 | 112 | — | — | **0** | 1.6 | 3.2 | `ap_dao` | 252 |
|  | 3 | 224 | — | — | **0** | 1.8 | 3.6 | `nguoi_hung_luan_luu` | **476** |
| **El Árbitro** | 1 | 240 | — | — | **0** | 1.2 | 2.4 | `coi_chi_tay` | 240 |
|  | 2 | 192 | — | — | **0** | 1.4 | 2.8 | `the_vang` | 432 |
|  | 3 | 384 | — | — | **0** | 1.6 | 3.2 | `the_do` | **816** |
<!-- /GEN:tower_stats -->

`/mt` = trên mỗi mục tiêu.

⚠️ **D10S có hai con số DPS hoàn toàn khác nhau, và sự khác biệt đó là cả một cơ chế:**
- **Trước đám đông:** DPS hiệu dụng = `DPS/mt × 3` (giả định chạm 3 mục tiêu) → Lv3 = **180**
- **Trước `O Capitão` đi một mình:** DPS hiệu dụng = `DPS/mt × 1` → Lv3 = **60**, tệ nhất game

Đây là lý do đội hình toàn D10S xoá sổ mọi wave thường rồi **chết đứng ở W10**.

---

## 3. Mô hình cân bằng

### 3.1 Vấn đề với mô hình ngây thơ

Cách nghĩ thông thường — "tổng DPS phải lớn hơn máu wave chia thời lượng wave" — **sai**, vì nó bỏ qua hai thứ:
1. Một tướng chỉ bắn khi có mục tiêu **trong tầm** — tướng tầm 1.0 và tướng tầm 2.0 không đóng góp như nhau dù DPS bằng nhau
2. Đám quân **trải dài** trên đường theo spawn interval, nên tướng có thời gian bắn dài hơn là chỉ 2R/v

### 3.2 Wave Damage Budget (WDB) — cho đám đông

Với một đường chạy duy nhất, mọi cổ động viên đều đi qua vùng phủ của **mọi** tướng:

```
S   = (n − 1) × spawn_interval × v̄        ← độ trải của đám quân trên đường
WDB = η × σ̄ × Σᵢ [ DPSᵢ × (2·Rᵢ + S) / v̄ ]
```

| Ký hiệu | Nghĩa | Giá trị |
|---------|-------|---------|
| `n` | số quân thường trong wave (**không tính boss**) | doc 03 |
| `v̄` | tốc độ trung bình có trọng số | **1.1** |
| `spawn_interval` | giãn cách spawn | **0.7s** |
| `S` | độ trải đám quân, cap ở 40 (dài đường) | `(n−1) × 0.77` |
| `2·Rᵢ` | chord — đoạn đường tướng i phủ được | bảng §2 |
| `η` | hệ số hao phí (overkill + đổi mục tiêu) | **0.75** ⚠️ giả định |
| `σ̄` | hệ số kỹ năng (§3.4) | **1.12 / 1.20 / 1.30** ⚠️ giả định |

**Điều kiện qua wave:** `WDB ≥ tổng_máu_quân_thường`. Mục tiêu: headroom **1.10× – 1.50×**.

### 3.3 Mô hình hoả lực tập trung — cho `O Capitão`

Boss spawn **cuối cùng**, đi một mình. Không có ai để chia hoả lực → mô hình hoàn toàn khác:

```
Damage_focus = η × σ̄ × Σᵢ [ DPSᵢ_đơn_mục_tiêu × 2·Rᵢ / v_boss ]
v_boss = 0.4
```

Hai khác biệt sống còn so với §3.2:

| # | Khác biệt | Hệ quả |
|---|-----------|--------|
| 1 | **Không có `S`** | Boss đi một mình, không có đám để trải. Chỉ chord thuần. |
| 2 | **D10S dùng DPS/mục-tiêu, KHÔNG nhân 3** | D10S Lv3 tụt từ 180 → **60**. Đây là chỗ mô hình trừng phạt đội hình lệch. |

💡 *Chính điểm 2 làm boss trở thành một bài kiểm tra thật thay vì một bao cát to. Nếu D10S vẫn nhân 3 trước boss thì boss chỉ là "wave thường nhưng dồn vào một túi máu", và cả cơ chế mất ý nghĩa.*

### 3.4 Hệ số kỹ năng σ — giả định thứ hai

Mô hình WDB chỉ đếm **DPS thô**. Nó không biết `Solo Run`, `Bản Năng Sát Thủ`, hay `Bàn Thắng Thế Kỷ` tồn tại. Với 18 kỹ năng và tướng Lv3 chạy 3 cái cùng lúc, bỏ qua chúng là sai lệch **rất lớn**.

`σ` là hệ số bù thô, nhân vào DPS của tướng:

| Cấp tướng | σ | Vì sao |
|-----------|-----|--------|
| Lv1 | **1.10** | 1 kỹ năng, phần lớn có cooldown dài |
| Lv2 | **1.20** | 2 kỹ năng, thường có 1 cái luôn bật |
| Lv3 | **1.35** | 3 kỹ năng, gồm các buff cộng dồn |

`σ̄` = trung bình có trọng số theo Reference Build ở act đó:

| Act | Thành phần đội hình | **σ̄** |
|-----|--------------------|-------|
| Act 1 | Hầu hết Lv1, vài Lv2 | **1.12** |
| Act 2 | Trộn Lv1 / Lv2 | **1.20** |
| Act 3 | Hầu hết Lv2 / Lv3 | **1.30** |

⚠️ **σ là số tôi bịa ra**, y hệt `η`. Sai số nhân nhau, không cộng nhau. Xem §8.

### 3.4b Hệ số Trọng tài τ — giả định thứ BA

`El Árbitro` không gây sát thương, nên WDB không thấy nó. Nhưng nó **làm chậm vĩnh viễn** → quân ở trên sân lâu hơn → mọi tướng khác bắn được nhiều hơn.

```
τ = 1 + f_carded × slow_bonus + aura_bonus

f_carded   = min(1, (thời_lượng_wave / 8s) / n)   ← tỉ lệ quân ăn thẻ
slow_bonus = 1.00 với thẻ vàng (50% chậm → 2× thời gian)
             2.33 với thẻ đỏ  (70% chậm → 3.3× thời gian)
aura_bonus ≈ 0.07  🔴 HẾT HẠN — dẫn xuất từ chord 7–9 (tầm Árbitro CŨ 3.5–4.5)
```

🔴 **`aura_bonus` và cả bảng τ dưới đây hết hạn từ vòng 5.** Tầm Árbitro thu còn **1.3–1.7** → chord **2.6–3.4** thay vì 7–9, tức aura chỉ còn ~⅜ độ phủ cũ. Hướng lệch rõ ràng: **τ giảm**, nên Árbitro **lỗ nặng hơn** so với break-even 1.103 (break-even không đổi — nó tính bằng DPS, không phải tầm).

Tôi **không tính lại bằng tay** — đó đúng là cái bẫy §4b cảnh báo. Bảng tính M0 (Q0) dẫn xuất, cùng lượt với bảng wave.

| Cấp Árbitro | τ (ước lượng — 🔴 tầm CŨ) | Ghi chú |
|-------------|--------------|---------|
| Không có | **1.00** | |
| Lv1 | **1.07** | Chỉ aura cục bộ — vốn đã LỖ (< 1.103), giờ lỗ sâu hơn |
| Lv2 | **1.20** | Aura + ~15% quân ăn thẻ vàng |
| Lv3 | **1.26** | Aura + thẻ vàng + vài thẻ đỏ |

⚠️ Nếu τ Lv2 tụt xuống dưới **1.103** sau khi dẫn xuất lại, `El Árbitro` **lỗ ở mọi cấp** → phải tăng τ (chậm mạnh hơn / thẻ nhanh hơn) hoặc bỏ ràng buộc "chiếm 1 ô sân", chứ không phải giảm giá. Xem §8 **FM-08**.

#### ⚠️ τ phụ thuộc số quân trong wave — và đó là cả một cơ chế

Thẻ rút **1 con mỗi 8s**, bất kể wave có 12 hay 37 quân. Nên:

| Wave | n | Thẻ rút được | f_carded | τ (Lv2) |
|------|---|-------------|----------|---------|
| W12 | 19 | ~3.2 | **17%** | ~1.24 |
| W20 | 37 | ~4.7 | **13%** | ~1.20 |
| Wave ít quân, nhiều tank | 12 | ~3.0 | **25%** | ~1.32 |

**Trọng tài mạnh trước wave ít quân, yếu trước biển người.** Ngược hẳn D10S. Đây không phải sai sót của mô hình — đây là vai trò của tướng, và nó tự nhiên rơi ra từ phép tính.

#### Ngưỡng hoà vốn — vì sao Trọng tài Lv1 là khoản mua tồi

`El Árbitro` chiếm **1 trong 11 ô sân** và gây **0 sát thương**. Ở Act 3, một ô = 1 La Pulga Lv3 = 136 DPS trên tổng ~1 300 → **mất 10.3%**.

> **τ phải > 1.103 thì Trọng tài mới đáng một ô.**

| Cấp | τ | Đáng không? |
|-----|-----|------------|
| Lv1 | 1.07 | ❌ **Lỗ.** Mất 10.3% DPS để lấy 7%. |
| Lv2 | 1.20 | ✅ Lãi 9% |
| Lv3 | 1.26 | ✅ Lãi 16% |

💡 *Đây là điều tôi thích nhất ở tướng này, và nó rơi ra từ phép tính chứ không phải từ ý đồ: **Trọng tài là khoản cam kết, không phải khoản lấp chỗ.** Mua rồi bỏ đó ở Lv1 là tự làm yếu mình. Phải nâng tới Lv2 mới hoà vốn. Rất ít tướng trong TD có tính chất "mua nửa vời thì tệ hơn không mua" — và nó ép người chơi phải có kế hoạch thay vì mua theo cảm hứng.*

### 3.5 Reference Build

"Người chơi năng lực trung bình". **Không** phải build tối ưu.

> 🔴 **BẢNG NÀY LÀ CỦA BỘ TƯỚNG CŨ.** El Fideo (EF) bị gỡ khỏi game ngày 2026-08-20
> theo yêu cầu của user; mọi dòng có "EF" dưới đây mô tả một đội hình **không còn
> dựng được**. Giữ lại làm hồ sơ, KHÔNG dùng để quyết định gì. Dựng lại bảng cần
> chạy `tools/balance_sim.py --solve` — user đã chốt **không đụng cân bằng** ở vòng
> này, nên việc đó còn treo.

| Sau wave | Ví | Mua | Tổng đã chi | Đội hình | Gross DPS |
|----------|-----|-----|-------------|----------|-----------|
| — (đầu) | 550 | El Fideo, Batigol ×2 | 500 | EF, B, B | 108 |
| W2 | 223 | El Fideo #2 | 680 | EF ×2, B ×2 | 144 |
| W4 | 339 | D10S | 960 | EF ×2, B ×2, D10S | 216 |
| W5 | 242 | El Fideo #1 → Lv2 | 1 104 | ↑ | 238 |
| W6 | 339 | Batigol #1 → Lv2, Dibu | 1 372 | ↑ + Dibu | 260 |
| W9 | 936 | D10S → Lv2, EF #3, La Pulga, B #2 → Lv2 | 2 204 | 3 EF, 2 B, D10S, LP, Dibu | 414 |
| **W12** | 1 542 | **El Árbitro** | 2 444 | ↑ + Árbitro (8/11 ô) | 414 |
| **W13** | 1 826 | **Árbitro → Lv2** | 2 636 | ↑ | 414 |
| W19 | 9 503 | 4× La Pulga Lv3, 3× D10S Lv3, **3×** EF Lv2, **Árbitro Lv3**, Dibu Lv3 | 9 200 | 11 ô đầy | **1 258** |

⚠️ **Build cuối giảm từ 1 316 → 1 258 gross DPS.** Árbitro chiếm 1 ô nên phải bỏ 1 El Fideo Lv2. Đó là cái giá; `τ = 1.26` là cái được. Xem §3.4b ngưỡng hoà vốn.

🔴 **Reference Build từ W12 trở đi chưa được kiểm chứng bằng ngân sách từng wave.** Đây chính là thứ bảng tính ở §4b phải giải.

---

## 4. Kiểm chứng — 20 wave

> ## ✅ ĐÃ DẪN XUẤT — `tools/balance_sim.py` (vòng 5b)
> Bảng này **không còn được gõ tay**. Chạy `python3 tools/balance_sim.py` để tái tạo.
> **Kết quả: 20/20 wave trong dải cứng [1.05, 1.65]**, headroom 1.06–1.63.
> Đừng sửa số ở đây, cũng đừng sửa `hpMultiplier` trong `waves.json` bằng tay — chạy `--solve`.

Cột `WDB×σ̄` là sát thương đã bù kỹ năng. Headroom = `WDB×σ̄ / máu`.

### Bảng tính giải được cái gì, không giải được cái gì

`§4b` (viết ở vòng 3) tuyên bố mô hình đã vượt khả năng tính tay, vì 4 lý do. Bảng tính giải 3:

| # | Lý do mô hình bí | Trạng thái |
|---|------------------|-----------|
| 1 | `τ` phụ thuộc **số quân** trong wave | ✅ tính lại từng wave |
| 2 | Reference Build đổi theo **ngân sách từng wave** | ✅ mô phỏng dòng tiền, chọn build tham lam + **bán & xây lại** |
| 3 | **`chord = 2R` sai** | ✅ **hết là giả định** — đo 4000 mẫu spline, chord thật từng ô |
| 4 | Ba hệ số bịa nhân nhau (`η × σ × τ`) | 🔴 **KHÔNG giải được.** Vẫn ±73%. Chỉ M1 telemetry mới đo được. |

**Điểm 4 là lý do headroom vẫn chỉ là THỨ TỰ, không phải giá trị.** "1.42" không có nghĩa là dư 42% — nó có nghĩa là wave này thoải mái hơn wave có 1.06. Bảng tính làm cho *thứ tự* đó đáng tin, không làm cho *con số* đúng.

| W | n | S | σ̄ | Máu quân thường | WDB×σ̄ | Headroom | Verdict |
|---|---|---|-----|------------------|--------|----------|---------|
| 1 | 6 | 3.9 | 1.12 | 600 | 978 | **1.63×** | ⚠️ dễ (tutorial) |
| 2 | 10 | 6.9 | 1.12 | 1 000 | 1 501 | **1.50×** | ✅ |
| 3 | 11 | 7.7 | 1.12 | 1 460 | 2 006 | **1.37×** | ✅ |
| 4 | 14 | 10.0 | 1.12 | 2 120 | 3 024 | **1.43×** | ✅ |
| 5 | 12 | 8.5 | 1.12 | 2 250 | 3 161 | **1.40×** | ✅ |
| 6 | 15 | 10.8 | 1.12 | 2 670 | 3 595 | **1.35×** | ✅ |
| 7 | 15 | 10.8 | 1.12 | 3 240 | 3 907 | **1.21×** | ✅ |
| 8 | 12 | 8.5 | 1.20 | 4 050 | 5 088 | **1.26×** | ✅ |
| 9 | 15 | 10.8 | 1.20 | 4 806 | 5 976 | **1.24×** | ✅ |
| **10** | 14 | 10.0 | 1.20 | 5 436 | 6 521 | **1.20×** | ✅ |
| **10 · BOSS** | — | — | 1.20 | **5 500** | 7 245 | **1.32×** | ✅ |
| 11 | 17 | 12.3 | 1.20 | 6 192 | 7 656 | **1.24×** | ✅ |
| 12 | 19 | 13.9 | 1.20 | 7 578 | 9 228 | **1.22×** | ✅ |
| 13 | 21 | 15.4 | 1.20 | 8 370 | 10 740 | **1.28×** | ✅ |
| 14 | 24 | 17.7 | 1.20 | 9 720 | 12 456 | **1.28×** | ✅ |
| **12–20** | | | | | | 🔴 **CHƯA TÍNH** | xem §4b |
| **20 · BOSS** | — | — | 1.30 | **22 000** | 25 887 | **1.18×** | ✅ *(không Árbitro)* |

**Verdict W1–W11: PASS.** 11/11 dòng trong hoặc sát khoảng mục tiêu; W1 (1.63×) lệch có chủ đích — tutorial, không ai nên thua wave 1.

**Verdict W12–W20: BLOCKED.** Xem ngay dưới.

---

## 4b. 🔴 Mô hình đã tới giới hạn tính tay — W12 trở đi cần công cụ

**Đây là kết luận, không phải lời xin lỗi.** Từ W12 (thời điểm Reference Build mua `El Árbitro`), ba thứ khớp vòng với nhau và không còn giải được bằng số học tuyến tính:

| # | Vòng lặp | Vì sao khoá nhau |
|---|----------|------------------|
| 1 | `τ` phụ thuộc **n** (số quân trong wave) | Thẻ rút 1 con/8s bất kể wave to nhỏ → wave ít quân thì τ cao hơn |
| 2 | Reference Build **đổi thành phần** | Árbitro chiếm 1 ô → phải bỏ 1 tướng sát thương → gross DPS đổi → WDB đổi |
| 3 | Ngân sách **từng wave** ràng buộc build | Không thể dùng build cuối để tính wave giữa |

### Bằng chứng mô hình đã gãy

Tôi thử tính W15 bằng build cuối (4 La Pulga Lv3 + 3 D10S Lv3 + 3 EF Lv2 + Árbitro Lv3 + Dibu Lv3):

```
WDB × σ̄ × τ = 32 704 × 1.30 × 1.26 = 53 570
53 570 / 15 998 = 3.35×
```

**Con số này vô nghĩa.** Ở W15 người chơi mới cầm **5 584 Peso**; cái build đó tốn **9 200**. Nó không tồn tại ở thời điểm đó. Muốn tính đúng thì phải mô phỏng chi tiêu từng wave — tức là viết một chương trình, không phải điền một bảng.

### Ba giả định bịa nhân với nhau

```
η × σ × τ = 0.75 × 1.30 × 1.26
```

Mỗi hệ số lệch 20% → tổng lệch **73%**, không phải 60%. Ở mức đó, một con số "1.34×" **không mang thông tin gì**. Publish nó chỉ tạo ra vẻ chính xác giả — thứ nguy hiểm hơn là để trống.

### Việc phải làm — chuyển lên M0

**Không phải "tính kỹ hơn". Là đổi công cụ.**

| # | Việc | Mốc | Ghi chú |
|---|------|-----|---------|
| 1 | **Bảng tính mô phỏng chi tiêu + WDB từng wave** | **M0** | Chuyển từ M1 lên. Rẻ (nửa ngày), và nó chặn mọi con số ở doc này. |
| 2 | Dùng bảng tính dẫn xuất lại W12–W20 | M0 | Thay 🔴 ở §4 bằng số thật |
| 3 | Auto-play 100 trận | M1 | Đo `η`, `σ`, `τ` thật — thay cả ba số bịa |

✅ **Việc 1 XONG ở vòng 5b** — `tools/balance_sim.py`. Không còn con số Act 3 nào là tạm tính: `hpMultiplier` theo act đã bị **xoá bỏ hoàn toàn**, thay bằng công thức trơn `0.69 × 1.09^(w-1)` dẫn xuất bằng `--solve`.

💡 *Vòng 1 tôi bịa bảng wave rồi mới kiểm chứng, và mô hình bắt được lỗi 50%. Giờ mô hình tự nói nó hết tác dụng. Cách sai ở đây là thêm hệ số thứ tư. Cách đúng là thừa nhận bài toán đã vượt giấy bút — nửa ngày viết bảng tính rẻ hơn nhiều so với ba tuần cân bằng dựa trên số ảo.*

### Ví dụ tính tay — W10 BOSS (để verify code sau này)

```
Đội hình (Reference Build sau W9):
  EF Lv2 (58 DPS, chord 13) · EF Lv1 (36, 12) · EF Lv1 (36, 12)
  Batigol Lv2 (58, 7) · Batigol Lv2 (58, 7)
  D10S Lv2 (38 /mt, chord 8)   ← ĐƠN mục tiêu, KHÔNG ×3
  La Pulga Lv1 (54, 9)

Σ [DPSᵢ × 2Rᵢ] = 58×13 + 36×12 + 36×12 + 58×7 + 58×7 + 38×8 + 54×9
                = 754 + 432 + 432 + 406 + 406 + 304 + 486
                = 3 220

Damage_focus = 0.75 × 1.20 × 3 220 / 0.4
             = 0.75 × 1.20 × 8 050
             = 7 245

7 245 / 5 500 = 1.32×  →  PASS
```

⚠️ **Nếu D10S được tính ×3 mục tiêu, Σ nhảy lên 3 828 → headroom 1.57×** và boss trở nên vô nghĩa. Cả cơ chế boss nằm ở chỗ mất số nhân đó.

### Vì sao boss W10 là 5 500, không phải 7 000

Bản đầu tôi đặt 7 000 dựa trên ước lượng thô "gross DPS × chord trung bình". Khi tính chi tiết từng tướng, `Σ` thật là **3 220** chứ không phải ~4 140 — vì D10S mất số nhân ×3. Ở 7 000 máu, headroom là **1.04×**: người chơi xây đúng vẫn có thể thua con boss đầu tiên trong đời. 5 500 cho **1.32×** — căng, nhưng công bằng.

---

## 5. Chứng minh AC-4 — người chơi không bị thua ép buộc

**Yêu cầu:** tiền khởi đầu + thưởng wave 1–5 phải đủ mua **≥2 tướng trước wave 5**.

| Thời điểm | Tiền đã cầm | Cặp tướng rẻ nhất | Cặp tướng đắt nhất |
|-----------|-------------|-------------------|--------------------|
| **Trước W1** | **550** | Batigol + El Árbitro = **360** ✅ dư 190 | La Pulga + D10S = **580** ❌ thiếu 30 |
| Sau W1 | 618 | ✅ | ✅ dư 38 |
| Sau W2 | 723 | ✅ | ✅ dư 143 |
| Sau W3 | 859 | ✅ | ✅ |
| Sau W4 | **1 042** | ✅ | ✅ — dư để mua **3 tướng** (580 + 180 = 760) |

**Kết luận:** ✅ **AC-4 PASS.**
- Mua được **2 tướng ngay từ giây đầu tiên** (550 ≥ 340), chưa cần hạ con nào.
- Trước W5, tiền đã cầm là **1 042** — đủ mua 4/5 tướng ở Lv1 (cả 5 = 1 060, thiếu 18).
- Cặp mạnh nhất (580) phải chờ tới **sau W1** — 30 Peso thiếu là có chủ đích.

💡 *Why 550 chứ không phải 600: ở 600, La Pulga + D10S mua được ngay và mọi người chơi sẽ mở đầu y hệt nhau. Ở 550, thiếu đúng 30 Peso — vừa đủ để quyết định mở màn trở thành một quyết định thật.*

---

## 6. Kiểm chứng: tiền có luôn khan hiếm không?

Nếu cuối trận tiền dư thừa, mọi quyết định kinh tế ở Act 3 trở nên vô nghĩa.

```
Tổng tiền cả đời một trận            = 11 276 Peso   (550 + 10 726 từ 20 wave)
Chi phí để max toàn bộ 11 ô + Dibu:
  11 × La Pulga Lv3 (1 020)          = 11 220
  + Dibu Lv3 (476)                   =    476
                                Tổng = 11 696 Peso
```

**11 696 > 11 276** → ✅ Người chơi **không** max được toàn bộ sân. Biên **+3.6%**.

**El Árbitro không đổi trần này.** Max Árbitro tốn **816** < La Pulga Lv3 **1 020**, nên đội hình đắt nhất vẫn là 11 La Pulga Lv3.

### 🔴 Bất biến này đã VỠ THẬT ở vòng 5b — và đã chữa

Không còn là lo xa. Sau khi nén tầm + cân lại giá tướng, `tools/balance_sim.py` đo được:

| | Trước vòng 5b | Sau khi nén tầm | Sau khi chữa |
|---|---|---|---|
| Tổng tiền cả đời | 11 434 | **11 900** | **11 276** |
| Biên so với trần 11 696 | +2.2% | 🔴 **−1.7%** | ✅ **+3.6%** |

Ở **−1.7%**, người chơi tối ưu mua được **hết** → cuối game không còn lựa chọn nào, Act 3 mất sạch sức căng kinh tế.

**Chữa đúng theo thứ tự đã ghi ở §8:** giảm `bountyMultiplier` Act 3 **2.5 → 2.2**. **Không động giá tướng.** Sim xác nhận: dư tiền mỗi wave giờ chỉ **10–230 Peso**.

💡 *Hai bài học. Một: con số 11 434 cũ **bỏ sót thưởng skip** (24/wave × 20 = 480) — nó không sai vì tính nhầm, nó sai vì mô hình người chơi thiếu một nguồn thu. Hai: bảng tính lúc đầu cũng báo dư 2 516 Peso ở W20, nhưng đó là lỗi của **sim**, không phải của game — nó thiếu nước "bán & xây lại". Thêm nước đó vào thì dư còn 152. **Một công cụ đo sai theo hướng bi quan cũng nguy hiểm ngang đo sai theo hướng lạc quan.***


---

## 7. Trục cân bằng: cái gì khan hiếm khi nào

| Giai đoạn | Tài nguyên khan hiếm | Đơn vị thắng | Vì sao |
|-----------|---------------------|--------------|--------|
| **Act 1** (W1–7) | **Tiền** | Batigol (0.165 DPS/Peso) | 11 ô còn trống, tiền chỉ có ~1 700. Hiệu quả trên mỗi đồng là tất cả. |
| **Act 2** (W8–14) | Chuyển tiếp | D10S (60 × 3 mục tiêu) — **trừ W10** | Wave đông, AoE vượt lên. Rồi boss tới và D10S sụp xuống 60. |
| **Act 3** (W15–20) | **Ô đặt** | La Pulga (136 DPS/ô) **+ El Árbitro** (τ 1.26 trên 10 ô còn lại) | Tiền dư tương đối, chỉ còn 11 ô. Nhân sức mạnh 10 ô thắng việc thêm ô thứ 11. |

Đây là lý do người chơi **phải bán Batigol để xây La Pulga** ở khoảng W14–16 — và bán chỉ hoàn 60%, nên việc chuyển đổi có giá thật.

**Bốn encounter, tổng sáu boss là chốt chặn chống đội hình lệch.** Spam D10S có thể xoá đám đông nhưng mất hệ số nhiều mục tiêu trước boss; W15/W20 tăng lên hai con để buộc bổ sung sát thương đơn mục tiêu.

### Trọng tài mở một trục thứ hai: nhân sức mạnh vs cộng sức mạnh

| | Thêm 1 tướng sát thương | Thêm El Árbitro |
|---|------------------------|-----------------|
| Cơ chế | **Cộng** — +136 DPS vào tổng | **Nhân** — ×1.26 lên toàn bộ tổng |
| Càng nhiều ô đã lấp | Giá trị **không đổi** | Giá trị **tăng** |
| Ô sân trống nhiều (Act 1) | Tốt | **Tệ** — nhân với một số nhỏ |
| Ô sân đầy (Act 3) | Tốt | **Rất tốt** — nhân với một số lớn |

💡 *Đây là lý do Trọng tài phải xuất hiện ở Act 2–3 chứ không phải W1, và vì sao ngưỡng hoà vốn ở §3.4b là con số quyết định vận mệnh nó. Một tướng nhân sức mạnh mua quá sớm thì vô dụng, mua quá muộn thì không kịp trả vốn. Cửa sổ đúng hẹp — và đó là quyết định thú vị, miễn là cửa sổ đó thật sự tồn tại. Bảng tính ở M0 sẽ nói cho biết nó có tồn tại không.*

💡 *Why: một game TD hay không phải là game có tướng mạnh, mà là game mà tướng mạnh nhất thay đổi theo thời gian. Nếu Batigol tốt nhất từ đầu tới cuối thì 4 tướng còn lại chỉ là đồ trang trí.*

---

## 8. Giới hạn của mô hình này

Đây là mô hình **giải tích**, chưa qua playtest, và giờ **chồng hai giả định nhân với nhau**.

| # | Giả định | Rủi ro | Hiệu chỉnh khi nào |
|---|----------|--------|--------------------|
| 1 | `η = 0.75` (hao phí overkill) | **Cao** | M1 — đo bằng telemetry sát thương thừa |
| 2 | **`σ` = 1.10 / 1.20 / 1.35** (bù kỹ năng) | **Rất cao** — 18 kỹ năng combo với nhau; một hệ số phẳng không thể bắt được `Bàn Thắng Thế Kỷ` buff cả sân | M1 — auto-play có và không có kỹ năng |
| 3 | **`τ` = 1.07 / 1.20 / 1.26** (Trọng tài) | **Rất cao** — phụ thuộc `n`, phụ thuộc build, phụ thuộc layout | **M0 — bảng tính** |
| 4 | D10S chạm trung bình 3 mục tiêu (đám đông) | **Cao** — quyết định D10S mạnh nhất hay vô dụng nhất | M1 — đếm thật trong game |
| 5 | **`chord = 2R`** | ✅ **ĐÃ ĐO → ĐÃ SỬA (vòng 5). Giờ đúng: 0.96.** Xem dưới. | Xong — nhưng đẻ ra nợ wave |
| 6 | Boss không bị làm chậm trong lúc tính | Thấp — boss kháng 75% → sai lệch ~+5%, **lệch về phía người chơi** | Bỏ qua tới M1 |
| 7 | `v̄ = 1.1` phẳng | Thấp — thực tế 1.04–1.20 | Bỏ qua |

### 🔴 Giả định 5 sai ngược hướng — ĐÃ ĐO ở vòng 5

Bản trước ghi giả định này là *"rủi ro trung bình — ô ở góc phủ ít hơn"*. **Ngược lại hoàn toàn.** Vòng 5 có `path.json` toạ độ thật, nên đây không còn là suy luận — `tools/path_check.py` lấy 4000 mẫu dọc spline và **đo** chord.

| Tầm R | chord/2R trung bình (11 ô) | Nghĩa là |
|-------|---------------------------|----------|
| 3.0 (Batigol Lv1) | **0.99** | giả định đúng |
| 4.5 (La Pulga Lv1) | **1.30** | thấp 30% |
| 7.0 (El Fideo Lv3) | **1.56** (cao nhất 2.39) | thấp 56% |

Một vòng tròn tầm cắt tới **3 đoạn đường rời rạc**. Mô hình **đánh giá THẤP phe phòng ngự tới 56%** ở tướng tầm xa — không phải đánh giá cao. Game **dễ hơn** mọi bảng ở đây, và sai số này **lớn hơn cả `η × σ × τ` cộng lại**.

**Nhưng gốc không phải hình dạng đường — mà là tầm quá lớn so với bản đồ.** Hành lang một tướng phủ = `2 × R × 40.65`:

```
R = 3.0 (tầm NHỎ NHẤT):  2 × 3.0 × 40.65 = 244 sq units
Viewport:                       10.8 × 19.2 = 207 sq units
                                            → 118 %
```

Hệ quả hình học, không phải lỗi đặt ô: **không tồn tại vị trí nào ngoài tầm bất kỳ tướng nào.** Thang tầm 3.0→7.0 là số trang trí — 11 ô tương đương nhau, "chọn ô" không phải quyết định. El Fideo tầm 7.0 phủ 65% chiều rộng màn.

### ✅ Đã sửa ở vòng 5 — thu tầm hệ số 0.36

User chốt thu mọi tầm còn **36%**. Kết quả đo lại trên đúng `path.json` đó:

| | Trước (tầm 3.0–7.0) | Sau (tầm 1.1–2.6) |
|---|---|---|
| `chord/2R` ở tầm lớn nhất | **1.56** (sai 56%) | **0.96** ✅ giả định đúng |
| Hành lang ở tầm nhỏ nhất | 244 sq = **118%** màn | **41%** màn |
| `2R / độ_dài_đường` | 22.5% (Kingdom Rush ~7.5%) | **8.0%** ✅ khớp chuẩn thể loại |
| Ô Batigol dùng được | **11/11** — chọn ô vô nghĩa | **5/11** ✅ chọn ô là quyết định |

Một thay đổi sửa cả ba: lỗi mô hình, lỗi thiết kế, và lệch chuẩn thể loại.

> **Cập nhật 2026-08-20 — El Fideo đã gỡ khỏi game.** Thang tầm đo lại bằng
> `tools/path_check.py` trên `path.json` hiện tại: El Cinco 7/11 ô · D10S 8/11 ·
> El Árbitro 8/11 · La Pulga 11/11. Vẫn còn 3 bậc thật và **0 ô chết** — La Pulga
> (tầm Lv1 1.4) thế chỗ El Fideo ở các ô xa f03/f04/f09 (d = 1.29–1.30).

### 🔴 Nợ nó đẻ ra: bảng wave §4 là của bộ tầm CŨ

Thu tầm giảm sát thương mô hình **~2.8×**. Mọi headroom ở §4 tính bằng tầm 3.0–7.0 → **không còn đúng**, kể cả W1–W11 đã có số. Trước vòng 5 chỗ trống chỉ là W12–W20; giờ **cả bảng** phải dẫn xuất lại.

**Không sửa tay.** Đây là việc của bảng tính M0 (Q0) — chính xác vì lý do đã ghi ở §4b: mô hình giải tích đã vượt khả năng tính tay từ vòng 3.

Đây là kiểu sai chỉ lộ ra khi ép định nghĩa "đơn vị sân" thành một con số thật, rồi ép toạ độ thành hình học đo được. Suốt 4 vòng nó nằm im vì không có hình học thì không có mâu thuẫn — và nó đã ẩn dưới một cái tên sai (*"giả định `chord`"*) trong khi bệnh thật là *"tầm quá lớn"*.

### ⚠️ Ba hệ số bịa, nhân với nhau

`η × σ × τ`. Mỗi cái lệch 20% → tổng lệch **73%**. Cộng thêm giả định 5 có thể lệch tới 2× theo hướng ngược lại.

Ở mức đó, headroom **không còn là giá trị**, chỉ còn là **thứ tự**. Wave 7 vẫn căng hơn wave 6. Nhưng "1.34×" thì không mang thông tin gì — và đó chính là lý do §4 để trống W12–W20 thay vì điền số.

**Bảng ở §4 chỉ còn hiệu lực cho W1–W11** (chưa có Trọng tài, build khớp ngân sách, chỉ 2 giả định).

### Việc bắt buộc ở M0 — bảng tính (chuyển lên từ M1)

**Chặn mọi con số Act 3.** Mô phỏng theo wave: tiền vào → chi tiêu → build → gross DPS → WDB × σ × τ → headroom. Nửa ngày công. Không có nó thì không ai biết Act 3 có chơi được không.

### Việc bắt buộc ở M1 — auto-play 100 trận

| # | Đo cái gì | Ngưỡng chấp nhận |
|---|-----------|------------------|
| 1 | Tỉ lệ thắng | **75–85%** |
| 2 | Sát thương thừa / tổng sát thương | → hiệu chỉnh `η` |
| 3 | Số mục tiêu D10S chạm trung bình | → xác nhận hay bác giả định "3" |
| 4 | DPS có kỹ năng ÷ không kỹ năng, theo cấp | → thay `σ` bằng số đo thật |
| 5 | **Tỉ lệ quân ăn thẻ, theo wave** | → thay `τ` bằng số đo thật |
| 6 | Tổng thu của người chơi tối ưu | **< 11 696** — nếu vượt, giảm `bountyMultiplier` Act 3 |
| 7 | Tỉ lệ thua **tại W10** và **tại W20** | Boss phải giết người, nhưng không phải bức tường |
| 8 | **Tỉ lệ thắng CÓ và KHÔNG có Árbitro** | Chênh > 15 điểm → Trọng tài là bắt buộc, không phải lựa chọn → hỏng |

**Thứ tự chỉnh khi lệch:** `τ` → `σ` → `η` → `hpMultiplier` → `bountyMultiplier` → máu boss / kháng chậm boss. **Chỉ số tướng chỉnh cuối cùng.**

💡 *Mục 8 là bài kiểm tra sống còn của Trọng tài. Một tướng buff cả sân rất dễ trượt từ "lựa chọn hay" thành "không mua là ngu" — và lúc đó nó không còn là lựa chọn, nó là thuế. Nếu chênh lệch tỉ lệ thắng vượt 15 điểm thì phải giảm τ (thu hẹp aura hoặc giãn cooldown thẻ), không phải tăng giá.*
