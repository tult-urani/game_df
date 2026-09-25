# STATE — La Muralla (TD World Cup)

**Cập nhật:** 2026-07-16
**Phase:** Define (docs-only) — vòng sửa 2
**Triage:** Complex

---

## Summary

Bộ 9 docs đã giao ở vòng 1. Vòng 2 đang áp 2 thay đổi user chốt sau khi review, cả hai **đảo ngược ràng buộc user đặt ở vòng 1**.

## Boundaries — ĐANG HIỆU LỰC

| # | Constraint | Nguồn | Ngày |
|---|-----------|-------|------|
| 1 | ~~DO NOT viết code~~ → **NỚI ở vòng 4**, xem dưới | user | 2026-07-16 |
| 2 | DO NOT tạo project Unity / `.unity` / `.meta` / `Packages/` | user | 2026-07-16 |

### 🔓 Ranh giới 1 được NỚI ở vòng 4 — phạm vi chính xác

User chốt "JSON là nguồn + sinh bảng tự động" sau khi review chỉ ra: chỉ số tướng sống ở 3 chỗ (`02`, `04`, `05`), và **cùng một lớp bug trùng lặp đã xảy ra 3/3 vòng**.

| ✅ ĐƯỢC viết | ❌ VẪN CẤM |
|-------------|-----------|
| `config/*.json` — file **dữ liệu**, nguồn chân lý số | Bất kỳ file `.cs` nào |
| `tools/*.py`, `tools/*.sh` — công cụ sinh/kiểm docs | Project Unity, `.unity`, `.meta`, `Packages/` |
| Marker `<!-- GEN:x -->` trong docs | Logic game dưới mọi hình thức |

**Lý do nới:** JSON và script sinh docs **không phải game code** — chúng là hạ tầng tài liệu. Để tới M0 mới làm thì lúc đó số liệu đã trùng ở 5 chỗ thay vì 3, cộng test viết dựa trên số cũ.
| 3 | DO NOT dùng tên thật cầu thủ ngoài `docs/02-TOWERS.md` §1 | user Q3 | 2026-07-16 |
| 4 | **DO NOT dùng "CR7"** — nhãn hiệu Ronaldo đăng ký (quần áo, khách sạn, nước hoa). Cũng cấm số áo 7. | agent phát hiện, user chốt | 2026-07-16 |
| 5 | DO NOT thêm dependency / package | scope | 2026-07-16 |
| 6 | DO NOT gắn sản phẩm vào World Cup 2026 — user chốt **không ăn theo mùa giải**. Store listing không nhắc giải đấu thật. | user | 2026-07-16 |
| 7 | DO NOT cho boss có kỹ năng ở MVP — chỉ là khối máu đi chậm | agent đề xuất, user không phản đối | 2026-07-16 |

## Boundaries — ĐÃ GỠ (user đảo ngược, không khôi phục)

| # | Ràng buộc cũ | Ai đặt | Ai gỡ | Thay bằng |
|---|-------------|--------|-------|-----------|
| A | "Khoảng 3 loại cổ động viên", không boss | user, vòng 1 | **user, vòng 2** | 3 loại thường + **1 boss `O Capitão`** ở W10 và W20 |
| B | 1 kỹ năng/tướng, mạnh dần qua 3 cấp | agent hiểu sai ý user | **user, vòng 2** | **Mỗi cấp mở 1 kỹ năng MỚI, cộng dồn** → 15 kỹ năng, Lv3 chạy 3 cùng lúc |

⚠️ Doc `01` §7 và `03` §1 ở vòng 1 ghi "❌ không có boss" — đó là **luật đã chết**. Nếu thấy nó còn sót ở đâu, đó là sót chưa sửa, không phải ràng buộc.

## Quyết định chốt vòng 2

| # | Câu hỏi | Chốt |
|---|---------|------|
| 1 | Thời điểm / ăn theo WC 2026 | **Không** — làm tử tế, roadmap 7 tuần giữ nguyên |
| 2 | Dibu không thuộc top-5 nổi tiếng | **Giữ Dibu** — thủ môn hợp theme, chấp nhận lệch tiêu chí |
| 3 | Phe địch vô danh | **Thêm boss** ở W10 + W20 |
| 4 | "Kỹ năng khi lên cấp" | **Mỗi cấp 1 kỹ năng mới, cộng dồn** |
| 5 | Tên boss | **`O Capitão`** — tiếng Bồ "Đội trưởng", generic, không ai sở hữu |

## Worked (with evidence)

- Vòng 1: 9 docs, 7/7 AC pass. Archive: `.dev-agent/.local/history/2026-07-16-td-worldcup-define.md`
- Phát hiện "CR7 là nhãn hiệu đăng ký" trước khi nó vào docs — user vốn đã chọn tuỳ chọn ghi "CR7-parody"

## Did NOT work (and why)

- **Vòng 1 hiểu sai "kỹ năng khi lên cấp"** → thiết kế 5 kỹ năng thay vì 15. Không hỏi lại vì tưởng câu chữ đã rõ. Bài học: cụm từ ngắn mô tả cơ chế game gần như luôn có 2 cách đọc — hỏi, đừng đoán.
- **Nguyên tắc "một hệ số, hai cột"** (`03` §2 vòng 1) — tôi khen nó là thiết kế đẹp. Nó **sai**: HP và thưởng scale cùng hệ số nghĩa là độ khó tăng đúng bằng sức mua → độ khó thực tế **phẳng**. Vòng 2 tách đôi: `hpMultiplier` > `bountyMultiplier`.

## Vòng 2 — HOÀN TẤT 2026-07-16

8/9 file sửa. Boss `O Capitão` + 15 kỹ năng cộng dồn + tách hpMult/bountyMult.

## Vòng 3 — HOÀN TẤT 2026-07-16

**Thêm `El Árbitro`** (tướng thứ 6, trọng tài). 9/9 file sửa.

| # | Quyết định vòng 3 | Chốt |
|---|-------------------|------|
| 1 | Tướng thứ 6 = trọng tài, 0 DPS, chậm vĩnh viễn bằng thẻ | `El Árbitro`, cost 240 |
| 2 | Kể chuyện | **Thiên vị** ("bị mua"). Agent nêu đánh đổi (hàm ý ARG gian lận); user chốt vẫn làm. **Không mở lại.** |
| 3 | Thẻ đỏ | **Không xoá sổ** — chỉ chậm 70% vĩnh viễn |
| 4 | Ô đặt | Chiếm 1 trong 11 ô sân |
| 5 | Boss kháng chậm | **50% → 75%** (agent phát hiện: ở 50% Trọng tài đưa boss W20 từ 1.18× → 1.68×) |
| 6 | Act 3 `hpMultiplier` | 3.2 → **3.8** (hiệu chỉnh bậc nhất, chưa dẫn xuất) |

### 🔴 Boundary mới quan trọng nhất

**`04` §4 để trống W12–W20 — CỐ Ý.** Mô hình giải tích đã vượt khả năng tính tay:
- 3 hệ số bịa nhân nhau (`η × σ × τ`) → lệch tổng 73%
- `τ` phụ thuộc số quân trong wave
- Reference Build đổi thành phần theo ngân sách từng wave

**KHÔNG điền số vào đó bằng tay.** Chờ bảng tính M0 việc 2b. Ai thấy chỗ trống mà "tính giúp" là đang tạo vẻ chính xác giả.

## Boundaries — GỠ thêm ở vòng 3

| # | Ràng buộc cũ | Thay bằng |
|---|-------------|-----------|
| C | "5 tướng" | **6 đơn vị** = 5 cầu thủ + 1 trọng tài. Trọng tài **không phải cầu thủ** → tiêu chí "5 cầu thủ nổi tiếng nhất" vẫn nguyên. |

## Vòng 4 — HOÀN TẤT 2026-07-16 (review trước khi code)

**Review verdict: FAIL → đã sửa.** Phát hiện CRITICAL: `04` và `05` cùng tự xưng nguồn chân lý số liệu. Code đọc JSON, doc là markdown → chắc chắn lệch. Bằng chứng: **cùng lớp bug ở 3/3 vòng** (1.06/1.08 · 7000/5500 · 3.2/3.8).

### Đã làm

| Việc | Kết quả |
|------|---------|
| `config/*.json` (5 file) | **Nguồn chân lý số liệu duy nhất** |
| `tools/gen_docs.py` | Sinh 8 bảng vào docs từ JSON. `--check` cho CI. **Đã test: sửa JSON → check đỏ exit 1.** |
| Lấp 4 lỗ hổng đặc tả | `path.json` (trước đây KHÔNG tồn tại), quy đổi đơn vị, tốc độ đạn, spec tick ×2 |
| Lấp 5 định nghĩa thiếu | tie-break targeting, tâm AoE, hình học `sut_xuyen`, save key + version, ID failure mode |

### 🔴 Hai phát hiện mới, cả hai đều do ép số liệu thành hình thật

1. **Rounding**: `Tambor` thưởng 25 × 2.5 = **62.5**. Python `round()` VÀ C# `Math.Round()` mặc định = **banker's rounding** → 62, không phải 63. Lệch **28 Peso** cả Act 3. → Chốt `roundingMode: "half_up"` ở `economy.json`. **Generator bắt được ngay lần chạy đầu.**

2. **`chord = 2R` SAI NGƯỢC HƯỚNG** (`04` §8 mục 5): sau khi chốt đơn vị, đường chữ S dài 40 trong hộp 7×17 → các đoạn cách nhau 3–5 units, mà tầm El Fideo Lv3 = 7.0 → **vòng tròn cắt 2–3 đoạn cùng lúc** → chord thật ≈ 28 chứ không phải 14. Mô hình đang **đánh giá THẤP phe phòng ngự ~2×**, không phải cao. **Sai số này có thể lớn hơn cả `η × σ × τ` cộng lại.**

## Ranh giới MỚI (vòng 4)

- **`config/` KHÔNG được fork vào `Assets/`** — M0 phải copy/symlink. Fork = quay lại đúng lớp bug vừa diệt.
- **Không gõ số vào docs.** Sửa `config/*.json` rồi chạy `python3 tools/gen_docs.py`.
- Marker `<!-- GEN:x -->` là vùng máy sinh — sửa tay sẽ bị ghi đè.

## Vòng 5 — 2026-07-16 (toạ độ thật)

**Q1 ĐÓNG.** `tools/path_check.py` (mới) lấy 4000 mẫu spline, đo chord bằng hình học thay vì giả định.

| Việc | Kết quả |
|------|---------|
| Waypoints (14 điểm, 4 lượt quét) | spline **40.65** vs khai 40 = lệch **1.6%** ✅, hộp bao x[-4.6,4.3] y[-7.8,8.0] trong viewport ✅ |
| 11 ô sân + 1 ô GK | rải theo **quãng đường** f=0.04→0.91, thang khoảng cách 0.50→2.11, **0 ô chết** ✅ |
| `chord` đo được | R=3.0 → 0.99 · R=4.5 → 1.30 · R=7.0 → **1.56** (max 2.39), 1 vòng tròn cắt **3 đoạn** |

### 🔴 Phát hiện vòng 5 — gốc của `chord`: TẦM QUÁ LỚN, không phải hình dạng đường

Hành lang = `2 × R × 40.65`. Ở tầm **nhỏ nhất** (Batigol 3.0) = 244 sq = **118% viewport (207 sq)**.
→ Hệ quả hình học: **không tồn tại ô nào ngoài tầm bất kỳ tướng nào.** Thang tầm 3.0→7.0 là trang trí, 11 ô tương đương, "chọn ô" không phải quyết định. El Fideo 7.0 phủ 65% chiều rộng màn.
→ **Đo được:** thu tầm về **~36%** → `chord/2R = 0.97` (giả định `chord = 2R` THÀNH ĐÚNG) + hành lang 41% màn (chọn ô thành quyết định). **Một thay đổi sửa cả hai lỗi.**
→ ⚠️ Thu tầm giảm sát thương mô hình ~2.8× → **bảng wave W1–W11 phải dẫn xuất lại**. Việc của bảng tính M0.

### Hai lỗi tự bắt được ở vòng 5

1. **`path_check.py` trộn tầm Dibu (2.0, thủ môn) vào bảng ô SÂN** → cột "R=2.0" vô nghĩa. Tầm sân thật là 3.0–7.0. Sửa: lọc `slotType == "field"`.
2. **Luật kiểm "ô xa đường = lỗi" sai** — xoá mất yếu tố thiết kế (ô xa chỉ tướng tầm xa dùng được). Sửa: chỉ bắt **ô chết** (`d > 6.0`), thêm luật mới bắt degenerate (mọi ô trong tầm tướng ngắn nhất → tầm không phải đánh đổi).

### Luật đã chết ở vòng 5

**`06` §5 "mọi ô đặt nằm ở nửa dưới màn hình"** — bất khả thi. Ô rải theo đường nên trải y +8.57→−6.12, 6/11 ở nửa TRÊN. Ép xuống dưới = nửa đầu đường không ai thủ = wave 1 thủng. Thay bằng "mọi ô **chạm được**, không bị HUD che" — đặt tướng xảy ra lúc nghỉ giữa wave, không phải thao tác gấp.

### Thu tầm — user CHỐT, đã áp dụng

Hệ số **0.36**, làm tròn 0.1. Batigol 3.0→**1.1** · D10S 3.5→**1.3** · Árbitro 3.5→**1.3** · La Pulga 4.5→**1.6** · El Fideo 6.0→**2.2** · Dibu 2.0→**0.7**.

| | Trước | Sau |
|---|---|---|
| `chord/2R` tầm lớn nhất | 1.56 (sai 56%) | **0.96** ✅ |
| Hành lang ở tầm nhỏ nhất | 118% màn | **41%** |
| `2R / đường` | 22.5% (chuẩn ~7.5%) | **8.0%** ✅ |
| Ô Batigol dùng được | 11/11 | **5/11** ✅ |

Thang tầm 4 bậc thật: Batigol 5/11 · D10S 6/11 · Árbitro 6/11 · La Pulga 8/11 · El Fideo 11/11.
f04 dời 1.31 → **1.20**: 1.31 trượt tầm D10S 1.30 đúng 0.01 — ranh giới dao cạo, người chơi sẽ tưởng game lỗi.

## 🔴 NỢ LỚN NHẤT — bảng wave W1–W20 hết hạn TOÀN BỘ

Thu tầm giảm sát thương mô hình **~2.8×**. Trước vòng 5 chỗ trống chỉ là W12–W20; **giờ cả 20 wave**, kể cả W1–W11 vốn "đã kiểm chứng". Đã gắn cảnh báo ở `04` §4, `economy.json → _wavesStale`.
**ĐỪNG SỬA TAY.** Bảng tính M0 việc 2b dẫn xuất.

Kéo theo: `aura_bonus ≈ 0.07` và bảng **τ** (`04` §3.4b) cũng hết hạn — dẫn xuất từ chord 7–9 của Árbitro tầm cũ, giờ chord 2.6–3.4 (~⅜). Hướng lệch: **τ giảm → Árbitro lỗ sâu hơn**. Nếu τ Lv2 tụt dưới break-even 1.103 → Árbitro lỗ MỌI cấp → tăng τ hoặc bỏ ràng buộc "chiếm 1 ô", **không giảm giá**.

## Vòng 5b — audit art/animation ("có đủ để code chưa?")

**Kết luận: art KHÔNG phải chỗ thiếu.** `06` §4 đã đủ ở mức số lượng asset. Chỗ thiếu là **hợp đồng kỹ thuật** — thang vật lý unit→px→pt, kích thước sprite, nhịp anim. Đã viết `06` §4b.

Ép hợp đồng đó thành số thì bắt được **2 lỗi thật**:

| # | Lỗi | Sửa |
|---|-----|-----|
| 1 | **f01 (y=8.57): radial menu xoè lên tới y=10.96 > mép trên 9.6** → nút bị cắt khỏi màn, không bấm được | dời f01 → **(1.10, 7.00)**, d=1.49, vẫn thủ đầu đường (mẫu 0/3992) |
| 2 | **El Fideo `attackRate` 2.0 → ở ×2 giãn cách chỉ 0.25s.** Anim 4 frame @12fps = 0.33s → anim DÀI HƠN giãn cách | chốt **24fps là sàn** (0.17s). Luật: cắt anim, KHÔNG hoãn phát bắn — logic bắn là chân lý |

Đã thêm 3 luật vào `path_check.py`, **thi hành bằng `--check`** chứ không phải bằng prose: vùng chạm 48pt · menu tràn mép màn · **ranh giới dao cạo**.

🔴 **Tự vấp "ranh giới dao cạo" 2 lần** (f04 d=1.31 vs D10S 1.30; rồi f01 d=1.28 vs 1.30 — chính lúc đang sửa lỗi kia). Ô cách đường ≈ đúng bằng tầm một tướng → lệch 0.01 unit (0.4pt) lật ✅ thành ❌, người chơi thấy tướng không bắn và tưởng game lỗi. Giờ là check, không còn là sự cẩn thận của tôi. **Đã thử ngược: ép ô về d=1.30 → exit 1 ✅.**

Hằng số UI nằm ở `config/path.json → ui`, không gõ trong code hay docs.

## Vòng 5c — BẢNG TÍNH M0 (việc 2b) HOÀN TẤT

`tools/balance_sim.py` (mới). **Q0 ĐÓNG.** 20/20 wave trong dải cứng [1.05, 1.65], headroom 1.06–1.63.

Giải 3/4 lý do mô hình bí: `τ` tính từng wave · build theo ngân sách từng wave (+ bán & xây lại) · **`chord` hết là giả định** (đo 4000 mẫu spline). **KHÔNG giải được `η × σ × τ`** — vẫn ±73%, chỉ M1 telemetry đo được → headroom vẫn chỉ là THỨ TỰ, không phải giá trị.

### 5 lỗi bảng tính bắt được mà 5 vòng review không bắt

| # | Lỗi | Bằng chứng | Chữa |
|---|-----|-----------|------|
| 1 | **La Pulga + Batigol là nội dung chết** — ngược spec "mạnh theo độ nổi tiếng" | 20 wave, người chơi tối ưu **không mua lần nào**. La Pulga nổi tiếng nhất = tệ nhất (0.54 vs El Fideo 1.28) | user chốt nén tầm ≤2.0 + nâng La Pulga ×1.25 dmg, Batigol 160→120, Fideo 180→195 |
| 2 | **chord SIÊU TUYẾN TÍNH** — gốc của lỗi 1 | tầm ×2.4 → chord **×5.3**. Ngưỡng nổ **R≈2.0**. Tướng tầm xa nhất luôn thắng bất kể DPS | mọi tầm ≤2.0 → giá trị/Peso lệch **2.8× → 1.06×**, sức mạnh/ô khớp đúng thứ tự nổi tiếng |
| 3 | **hệ số máu theo act = vách đứng** | W8 **1.00**, W15 **0.97** (thua) vs W14 **1.81** (quá dễ) — biên độ 1.81× trong cùng act | **xoá hẳn hpMultiplier theo act** → công thức trơn `0.69 × 1.09^(w-1)` |
| 4 | **tiền cả trận vượt trần chi** | 11 900 > 11 696 = biên **âm 1.7%** → mua được hết | Act 3 `bountyMultiplier` 2.5→2.2 → biên **+3.6%**, dư mỗi wave 10–230 |
| 5 | **boss cũ bất khả thắng** | máu 5500/22000 → headroom **0.49 / 0.28** | W10 2100 · W20 **6700** |

### Boss W20 — cơ chế hoạt động, có số chứng minh

Máu 6700 **tách đôi** hai kiểu build: tối-ưu-đám-đông **0.73** (thua, buộc bán xây lại) vs có-tính-boss **1.55** (thắng). Tính chất **tự nảy sinh**: đầu game thiếu tiền → Batigol rẻ (đơn mục tiêu) → hợp boss; cuối game hết ô → dồn D10S (AoE) → boss trừng phạt **đúng cái tối ưu đó**. W10 (2100) dạy, W20 kiểm tra.

### 4 lỗi TRONG CHÍNH CÔNG CỤ, tự bắt

1. **Ép mua Árbitro** → W1 headroom 0.46. Ép nó = giả định sẵn điều đang cần chứng minh. Sửa: cho sim tự chọn → nó KHÔNG mua W1–10, mua Lv2 từ W11. **`04` nói sai**: break-even 1.103 chỉ đúng khi HẾT Ô; còn ô trống thì Árbitro cạnh tranh bằng TIỀN.
2. **Thiếu nước "bán & xây lại"** → sim kẹt ở Bati3 (225/ô) trong khi dư 2 516 Peso. Thêm vào: dư còn **152**, sát thương W20 **+28%**. Chính `waves.json` đã ghi ý đồ Act 3 là "bán và xây lại" — thiết kế lường trước, sim thì chưa.
3. **Chấm boss bằng build đám-đông** → kêu ❌ oan. Build đó thua boss **LÀ CƠ CHẾ**. Sửa: chấm bằng build có-tính-boss, in cả hai số.
4. **Ràng buộc ô sai** — ép vị trí trong không gian thay vì ép **đoạn đường ô đó canh** → 17% đầu đường không ai canh. Sửa + thêm luật `--check`.

### Suýt sửa nhầm

Máu **gốc** giảm ở W8/W15 trông như bug. Không phải: hệ số đơn điệu nên **máu từng con quái luôn tăng**; chỉ *kích thước wave* reset đầu act = nhịp xả hơi cố ý. Giữ nguyên.

### Luật mới trong `path_check.py` (đều đã thử ngược → exit 1)

vùng chạm 48pt · menu tràn mép màn · **ranh giới dao cạo** (tự vấp 2 lần) · **đoạn hở >15%**

---

# VÒNG 6 — BẮT ĐẦU CODE (2026-07-16)

## 🔓 Ranh giới GỠ — user chốt "bắt đầu code đi"

| # | Ràng buộc cũ | Trạng thái |
|---|-------------|-----------|
| 1 | DO NOT viết code | **GỠ** — user, vòng 6 |
| 2 | DO NOT tạo project Unity | **GỠ** — nhưng CHƯA làm, xem dưới |

## 🔴 PHÁT HIỆN NỀN TẢNG: máy KHÔNG có Unity

`dotnet 10` ✅ · Xcode 16.2 ✅ · **Unity ❌**

→ Mọi dòng phụ thuộc `UnityEngine` viết ra là **không compile được, không test được** — chỉ viết rồi hy vọng. Vi phạm luật "không tuyên bố gì mà không có bằng chứng".

**Kiến trúc đối phó (không phải để đẹp — để verify được):**
- `core/LaMuralla.Core/` — **C# THUẦN, netstandard2.1, ZERO dependency**, không chạm UnityEngine → `dotnet test` chạy được ngay
- Unity chỉ còn là **lớp vỏ vẽ hình**, làm sau khi có Unity

## 3 bẫy chỉ lộ ra khi dựng thật

| # | Bẫy | Chữa |
|---|-----|------|
| 1 | `dotnet new` mặc định ra **net10.0** — Unity 6 KHÔNG đọc được | ghim `netstandard2.1` + lý do trong csproj |
| 2 | **`System.Text.Json` KHÔNG có** trong netstandard2.1. `JsonUtility` của Unity cũng vô dụng — `spawns` là Dictionary | user chốt **bake JSON → ScriptableObject trong Editor**; tự viết parser, **không ship vào app** |
| 3 | **`init` KHÔNG có** trong netstandard2.1 (thiếu `IsExternalInit`) | bỏ `init` → `{ get; set; }`. KHÔNG thêm shim: nó có thể đụng độ định nghĩa Unity, mà điều đó **không kiểm chứng được** ở đây |

## 🔴 Validator bắt được lỗi trong CHÍNH DOCS

`docs/05` §5 luật 15 nói `scope` chỉ có `in_range | permanent`. Chạy lên config thật → nổ ngay: `d10s.ban_tay_cua_chua` dùng **`scope: timed`** (chậm 50% trong 3s). Có **BA** cơ chế chậm, docs ghi hai. **Docs sai, config đúng.**

Siết thêm theo đúng ý đồ gốc ("mặc định ngầm là bug im lặng"): `timed` thiếu `durationSec` → throw · scope khác mà khai `durationSec` → throw (một trong hai trường đang nói dối).

Luật 10 cũng đã viết lại ở vòng 5b (`hpMultiplier` theo act đã bị xoá) → đo `hpScaling.MultiplierAt(act.LastWave) >= act.bountyMultiplier`.

## Đã xong — CÓ BẰNG CHỨNG (70/70 test xanh, build 0 lỗi 0 cảnh báo)

| Thành phần | Bằng chứng |
|-----------|-----------|
| `Json/` parser | đọc được **cả 5 config thật**, tiếng Việt có dấu, Dictionary. Khoá trùng → ném (bug thật vòng 3). Lỗi chỉ đúng đường dẫn `towers[0].levels[0].cost`. **InvariantCulture** — máy locale vi-VN đọc "0.69" thành 69 nếu quên |
| `Config/` model + mapper + **16 luật validate** | config thật qua hết. **21 test ÂM** — bẻ config, mỗi luật phải nổ |
| `Match/EnemyPath` | 🔴 **khớp `tools/path_check.py` tới 9 chữ số thập phân**: 40.650343114306565, 3992 mẫu. Hai cài đặt độc lập cùng một số → game và bảng cân bằng chắc chắn nói về CÙNG một đường |
| `Match/SlowStack` (FM-14) | thứ tự MAX→cap→kháng cho **17.5%**, sai thứ tự cho **32.5%** — test *assert* khác biệt 1.86× là có thật |
| `Match/EconomyService` (luật bất khả xâm phạm #1) | sổ cái đối soát: số dư = tổng mọi biến động. Bán = hoàn vốn, KHÔNG tính vào LifetimeEarned |

**Vì sao 21 test âm:** "config qua hết 16 luật" một mình **không chứng minh gì** — luật viết hỏng thành no-op cũng xanh y hệt. Đã dính bẫy này 2 lần với `path_check.py`.

## Lỗi tự bắt ở vòng 6

- Test `WaveClearBonus(19)` tôi viết 115, đúng là **110** (`20 + 5×18`). Lỗi ở TEST, không phải code — sửa test.

## Next concrete

**Chặn:** cài **Unity 6 LTS** qua Unity Hub. Chưa có thì mọi thứ Unity là viết-mù.

Làm được ngay (C# thuần, verify được):
1. `TargetingSystem` — `first-in-range` + tie-break "quãng đường đi được LỚN NHẤT" (`05` §4.5 luật 1)
2. `DamageSystem` — **luật bất khả xâm phạm #2**: nơi DUY NHẤT quyết định đòn kết liễu (FM-07: hai tướng cùng bắn phát cuối → cộng tiền 2 lần → kinh tế vỡ sau 10 wave)
3. `GoalHealth` — lọt lưới trừ máu
4. `WaveSpawner` — đọc `waves.json`, spawn theo interval

Cần Unity (viết được, chạy KHÔNG được):
5. Editor baker JSON → ScriptableObject
6. M0 việc 1/3/7 — project, scene, build iPhone

Nợ cũ: 21 failure mode → ID `FM-NN` cố định (7 test ở `05` tham chiếu theo số thứ tự).

## Blocking

**Không còn gì chặn việc code.** Q1 (layout) đóng, tầm đã chốt. Chặn duy nhất là **số cân bằng** — và đó là việc của M0, không phải của define phase.

## Bài học vòng 5

- **Công cụ đo bắt được cái review 4 vòng không bắt được.** `chord` bị nghi từ vòng 4 nhưng chỉ là lời văn; viết 60 dòng Python lấy mẫu spline thì nó thành con số — và con số chỉ ra bệnh thật nằm chỗ khác (tầm), không phải chỗ tôi đặt tên (chord).
- **Đặt tên sai vấn đề = chữa sai chỗ.** Suốt vòng 4 tôi gọi nó là "giả định `chord` sai". Gốc là "tầm quá lớn so với bản đồ". Sửa cái tên đúng thì một thay đổi chữa cả 3 triệu chứng.
- **Luật kiểm tra của chính mình cũng phải bị nghi.** 2/2 luật đầu tiên tôi viết trong `path_check.py` đều sai (trộn tầm thủ môn vào ô sân; coi "ô xa đường" là lỗi thay vì là thiết kế).
- **Generator không cứu được prose.** Grep bắt 4 chỗ tầm cũ nằm ngoài marker `GEN` ở `02`/`03`/`04`. Marker chỉ bảo vệ bảng, không bảo vệ câu văn — vẫn phải grep sau mọi lần đổi số.

## Nợ đã ghi nhận

- `η` = 0.75, `σ` = 1.10/1.20/1.35, `τ` = 1.07/1.20/1.26 — **cả ba đều bịa**, chưa đo
- Margin khan hiếm tiền 2.2%, và build tối ưu giờ **rẻ hơn trần** → càng dễ vỡ
- Trọng tài có thể thành "thuế" — đo ở M1 (`04` §8 mục 8), chênh > 15 điểm thì giảm τ không phải tăng giá
- Bảng màu: 3 sắc đỏ + 2 sắc vàng — màu đã hết chỗ, giải bằng hình dạng
- Roadmap 35 → 40 → **45** người-ngày qua 3 vòng

## Nợ đã ghi nhận

- `η = 0.75` (hao phí overkill) — chưa đo
- `σ` (hệ số kỹ năng) — **giả định thứ hai chồng lên giả định thứ nhất**. Mô hình WDB giờ là ước lượng hai tầng.
- Margin khan hiếm tiền tụt từ 9% → ~2% sau khi thêm boss bounty — mỏng, cần theo dõi ở M1

---

# VÒNG 7 — UNITY DỰNG XONG, APP ĐÃ CHẠY TRÊN IPHONE

## Trạng thái: đường ống C# → iPhone đã thông đầu-cuối

| Mắt xích | Bằng chứng |
|---|---|
| Unity 6000.5.4f1 + iOSSupport | `PlaybackEngines/iOSSupport/UnityEditor.iOS.Extensions.dll` đăng ký OK |
| Licence | Editor chạy batchmode → `Exiting batchmode successfully now!` |
| Project `LaMuralla/` | từ template `2d-cross-platform-2d-6.1.5` (URP 17.6.0) |
| Cấu hình mobile | `defaultScreenOrientation: 0` · `iPhone: com.tuantu.lamuralla` · `scriptingBackend iPhone: 1` (IL2CPP) · Team `PJS4V57G3Q` |
| Core → Unity | `Library/ScriptAssemblies/LaMuralla.Core.dll` 48 KB, 0 lỗi |
| Core → dotnet | 70/70 test xanh **sau khi chuyển vị trí** |
| IL2CPP | `ios-build/Il2CppOutputProject/` — C# đã thành C++ |
| Ký + cài | `codesign`: Apple Development → WWDR → Apple Root CA; cài OK |
| **Chạy** | **PID 41460, sống ổn định, 0 crash log** |

## Quyết định kiến trúc: MỘT NGUỒN, HAI TRÌNH BIÊN DỊCH

Nguồn core sống ở `LaMuralla/Assets/_Project/Core/` (Unity chỉ dịch thứ dưới `Assets/`).
`core/LaMuralla.Core/LaMuralla.Core.csproj` trỏ ngược vào bằng glob:
`<Compile Include="../../LaMuralla/Assets/_Project/Core/**/*.cs" />` + `EnableDefaultCompileItems=false`.

Đã cân nhắc và **loại**:
- copy sang hai chỗ → lệch âm thầm
- build DLL rồi thả vào `Assets/Plugins/` → DLL cũ đi mà không ai báo
- symlink → hành vi của Unity với symlink không kiểm chứng được ở đây

## `noEngineReferences: true` — luật giờ do máy ép, không do người nhớ

`Assets/_Project/Core/LaMuralla.Core.asmdef`. **Đã thử ngược để chứng minh**: thả
`Probe.cs` có `using UnityEngine` → Unity từ chối, `error CS0246: Vector2 could not
be found`, exit 1. Probe đã gỡ, build xanh lại.

Đây là nâng cấp thật so với vòng 6: khi đó "core không đụng UnityEngine" chỉ được
bảo vệ bằng grep (và grep ĐÃ báo nhầm — nó khớp phải comment trong `Vec2.cs`).

## Bẫy đã gặp và cách qua

- **Cert Apple hết hạn 21/01/2026** (~6 tháng trước), không phải lỗi đăng nhập.
  `security find-identity -v` báo `0 valid` trong khi cert vẫn nằm trong keychain →
  `find-identity` (không `-v`) mới lộ `CSSMERR_TP_CERT_EXPIRED`.
  Cách qua: `xcodebuild -allowProvisioningUpdates` **tự cấp lại**. Sẽ hết hạn lại
  sau ~1 năm → cùng cờ ấy xử lý.
- **`timeout` không tồn tại trên macOS** — lần probe licence đầu im lặng exit rỗng,
  suýt kết luận nhầm là Unity hỏng. Dùng timeout của tool, đừng dùng `timeout`.
- **Trust profile là bước TAY, không bỏ qua được.** Personal Team → iOS chặn launch
  với `profile has not been explicitly trusted by the user`. Không có API.
  Người dùng: Settings → General → VPN & Device Management → Trust.
- **grep "error" trong log Unity toàn báo nhầm** — khớp tên file (`FallbackError.shader`,
  `ErrorIcon_Small.png`). Phải lọc `error CS` / `^\*\* BUILD`.

## Việc tiếp — KHÔNG cần Unity, test bằng `dotnet test`

1. `TargetingSystem` — first-in-range, hoà thì chọn con đi xa nhất dọc spline (`05` §4.5 luật 1)
2. `DamageSystem` — luật bất khả xâm phạm #2, nơi DUY NHẤT quyết định đòn kết liễu (chặn FM-07 cộng tiền 2 lần)
3. `GoalHealth`
4. `WaveSpawner`

## Việc cần Unity (sau)

- Editor baker: JSON → ScriptableObject (đọc `config/` lúc edit-time → `config/` KHÔNG cần vào `Assets/`, giữ đúng luật "không fork")
- Scene Match, sprite, input, HUD
- Module Android + NDK (~6 GB) — hoãn, Hub thêm module vào Editor đã cài bất cứ lúc nào

## Nợ cũ vẫn còn

- 21 failure mode chưa có ID ổn định `FM-NN` (7 test ở `05` tham chiếu theo vị trí)
- `η` / `σ` / `τ` — cả ba vẫn bịa, chỉ M1 telemetry đo được
- Margin khan hiếm tiền +3.6% — mỏng

---

# VÒNG 8 — LÁT CẮT DỌC CHẠY TRÊN IPHONE, VÀ NÓ BẮT ĐƯỢC LỖI THIẾT KẾ

## Lỗi mà 15 luật hình học không thấy, mắt người dùng thấy trong 5 giây

Người dùng nhìn máy: *"đường tròn 2 bên sát màn hình quá nên có 1 số vòng xanh bị khuất"*.

Đo trên iPhone 12 thật:
```
màn 1170×2532  aspect 0,462  (19.5:9)
thiết kế path.json: viewport 10.8 × 19.2 = 1080×1920 = 16:9
```

**Gốc: thiết kế 16:9, máy thật 19.5:9.** Camera đặt thẳng `cameraOrthographicSize: 9.6`
(nửa chiều CAO) → Unity khớp chiều cao → bóp chiều ngang còn ~8.9 unit → ô rìa
(f02 x=-4.9, f05 x=4.9) trôi ra ngoài.

Sửa: **khớp theo chiều NGANG**, để chiều dọc tràn.
`ortho = (viewportWidth/2) / aspect` = 11.69 → thấy đúng 10.80 × 23.37 unit.
Chiều cao dôi ra là cỏ thừa — sau này là đất của HUD.
`cameraOrthographicSize` ở config giờ chỉ còn là **con số thiết kế ở 16:9**, KHÔNG
phải thứ đặt thẳng vào camera.

safeArea che trên 1.30 / dưới 0.94 unit — không chạm đường (spawn y=8.0, cầu môn y=-7.8).

## Cùng một điểm mù ở HAI chỗ: kiểm TÂM ô thay vì VÙNG CHẠM

- `PathConfigTests.Moi_o_nam_trong_khung_hinh` (C#) — xanh, vì chỉ so `|x| ≤ 5.4`
- `tools/path_check.py` — in "✅ Mọi ràng buộc hình học đều đạt", vì cả 15 luật
  đều hỏi về TÂM ô

Thực tế: ô có tâm trong màn nhưng **vùng chạm 48pt tràn mép** → ngón tay không bấm
hết được → ô hỏng. `|x|` tối đa thật = 5.4 − 0.66 = **4.74**, không phải 5.4.

Đã sửa cả hai. Kiểm ngược cả hai:
- C#: test viết lại → ĐỎ với toạ độ cũ (`f02 x=-4.90: tràn 0.16 unit`)
- Python: toạ độ cũ → exit 1 · toạ độ mới → exit 0

## Dời ô mà KHÔNG đụng cân bằng

`tools/fix_edge_slots.py` (mới): cố định x ở mép cho phép, quét y để giữ NGUYÊN
cả `d` (khoảng cách tới đường → quyết định tầng thang tầm) lẫn `f` (vị trí theo
quãng đường → phân bố ô).

```
f02  (-4.90, +3.80) d=1.31 f=0.167  →  (-4.73, +3.69) d=1.31 f=0.169
f05  (+4.90, +1.50) d=0.61 f=0.405  →  (+4.73, +1.10) d=0.61 f=0.410
```

Thang tầm sau khi dời: **y hệt trước** — Batigol 5/11 · D10S 6/11 · Árbitro 6/11 ·
La Pulga 8/11 · El Fideo 11/11. Lỗ hổng 11%. Cân bằng không phải tính lại.

## Unity tự thêm 40 package không ai yêu cầu

Template `2d-cross-platform` cho 17 gói → sau import đầu thành **57**. Gồm
`com.unity.purchasing` (gắn quyền **In-App Purchase** vào app) và
`com.unity.analytics` (gửi dữ liệu người dùng).

Phát hiện được là nhờ build HỎNG: Personal team không được dùng IAP → `xcodebuild`
từ chối ký. Lỗi đó đang bảo vệ dự án.

Cắt còn 20 gói → `StoreKit` biến mất khỏi Xcode project (grep = 0) → build 1168 → 1006 MB.
Lý do ghi ở `LaMuralla/Packages/README.md`.
⚠️ `manifest.json` KHÔNG cho khoá chú thích — kể cả `_comment`. Package Manager
kiểm mọi khoá như tên gói. (Khác `config/*.json`, nơi khoá `_` LÀ chú thích.)

## Lỗi im lặng: build xanh + hình đúng vẫn có thể đang hỏng

Cắt `com.unity.modules.physics` (đúng, game 2D) làm `GameObject.CreatePrimitive`
hỏng — nó là API 3D, cố gắn `MeshCollider`. Nhưng **hình vẫn vẽ ra**, nên nhìn
màn hình không biết. Chỉ console thiết bị lộ:
`Can't add component because class 'MeshCollider' doesn't exist!`
Sửa: `SpriteRenderer` + sprite 1×1 sinh trong code.

## Locale vi-VN xác nhận phòng thủ viết từ vòng 6

Log máy in `spline 40,65` — dấu **phẩy**. Máy chạy locale vi-VN. Nếu `JsonParser`
không dùng `CultureInfo.InvariantCulture` thì `"0.69"` ở waves.json đã đọc thành
`69`. Hiển thị theo locale, parse thì không. Giả thuyết vòng 6 → máy thật xác nhận.

## Kiến trúc config vào máy

`config/` (gốc repo, dùng chung với tools/*.py) → `ConfigBaker` (Editor, đọc
edit-time) → `BakedConfig.asset` chở **JSON thô** → runtime parse bằng JsonParser.

KHÔNG mirror 60+ trường ra ScriptableObject: mirror = bản sao thứ hai của lược đồ,
đổi config phải nhớ đổi mirror, không gì nhắc. Chở chuỗi thô → lược đồ chỉ có một,
ở `ConfigMapper`, đã có test. Giá phải trả: parse lúc chạy (rẻ, không reflection →
IL2CPP an toàn). Đổi lại: **validate lúc bake** → config sai thì KHÔNG build được.

## Trạng thái

- 77/77 test C# xanh, 0 warning
- `path_check.py --check` exit 0
- App chạy trên iPhone 12, PID 41516, 0 cảnh báo, 0 crash
- Ô `f02`/`f05` đã trong mép — chờ user xác nhận bằng mắt

## Việc tiếp (không cần Unity)

`TargetingSystem` → `DamageSystem` → `GoalHealth` → `WaveSpawner`

---

# VÒNG 9 — TargetingSystem

Người dùng xác nhận trên máy: *"2 vòng đã đủ rồi"* → hình học vòng 8 ĐÓNG.

## Đã viết

`Assets/_Project/Core/Match/TargetingSystem.cs` — 4 selector có thật ở towers.json:

| Selector | Dùng ở |
|---|---|
| `FirstInRange` | đòn đánh thường mọi tướng |
| `HighestAbsoluteHp` | `la_pulga.solo_run` — máu TUYỆT ĐỐI, không phải % |
| `FirstInRangeWithoutCard` | `el_arbitro.the_vang` |
| `FirstInRangeWithYellowCard` | `el_arbitro.the_do` |

## Thứ tự toàn phần (docs/05 §4.5 mục 1) — và bằng chứng nó có răng

Sắp: `DistanceTravelled` giảm dần → `Id` tăng dần.

`Id` KHÔNG phải trang trí. `03` §6 cho phép quân chồng nhau; hai con cùng loại
spawn cùng frame có `DistanceTravelled` trùng tới từng bit. Không có `Id` thì mục
tiêu do thứ tự phần tử trong list quyết định — mà list bị sắp xếp lại mỗi khi một
con chết ở giữa → tướng đổi mục tiêu vô cớ, và bug không tái hiện được vì nó phụ
thuộc thứ tự chết.

**Kiểm ngược (probe):** thay `return a.Id < b.Id;` → `return false;`
```
Moi_hoan_vi_cho_cung_ket_qua          [FAIL]
Ket_qua_khong_doi_khi_dao_thu_tu_list [FAIL]
Failed: 2, Passed: 89
```
Khôi phục → 91/91. Test có răng, không phải trang trí.

`Moi_hoan_vi_cho_cung_ket_qua` chạy đủ **120 hoán vị** của 5 con (3 con trùng
quãng đường) và ép `Distinct().Count() == 1`.

## Tách `Eligible` khỏi `Better`

"Được phép bắn" (thẻ) và "bắn con nào trước" (quãng đường) là hai câu hỏi khác
nhau. Gộp lại là chỗ bug thích trốn — ví dụ thẻ đỏ khi chưa ai có thẻ vàng phải
trả `null`, KHÔNG được rơi xuống nhắm bừa con khác. Có test riêng cho ca đó.

## Bẫy mới: exit code Unity batchmode KHÔNG đáng tin tuyệt đối

Một lần chạy trả **exit 132** (SIGILL) trong khi:
- `error CS` = 0
- log kết thúc bằng `Batchmode quit successfully invoked - shutting down!`
- DLL vẫn sinh ra (48128 → 52736 bytes)

Chạy lại 3 lần → 3/3 exit 0. Là flake lúc TẮT tiến trình, sau khi việc đã xong
(kèm `Curl error 7` — không nối được server Unity).

→ Luật: kiểm `error CS` + sản phẩm sinh ra, ĐỪNG chỉ tin exit code.

## Trạng thái

- 91/91 test xanh (77 → +14)
- Unity dịch core OK, 0 error CS
- `path_check.py --check` exit 0

## Việc tiếp

`DamageSystem` (luật bất khả xâm phạm #2 — nơi DUY NHẤT quyết đòn kết liễu, chặn
FM-07 cộng tiền 2 lần) → `GoalHealth` → `WaveSpawner`

---

# § BOUNDARIES — chốt bởi user, KHÔNG được xét lại

## B-01 · Kỹ năng KHÔNG tích luỹ (2026-07-17)

User: *"khi nâng cấp lên thì chỉ sử dụng kỹ năng cao nhất chứ không phải tất cả cùng lúc"*

Chốt qua 2 câu hỏi:
1. Cách hiểu = **"Chỉ kỹ năng Lv cao nhất, thiết kế lại"** (không phải "chỉ kỹ năng chủ động", không phải "giữ tích luỹ")
2. Lv3 có giữ hiệu ứng Lv1 không = **"Không giữ — thay thế hẳn"**

→ Tướng cấp N chạy ĐÚNG MỘT kỹ năng: kỹ năng có `unlockLevel == N`.
→ Kỹ năng `unlockLevel < N` KHÔNG chạy.
→ User đã thấy và chấp nhận hệ quả in trong preview:
  *"Lv3 đánh đám đông TỆ HƠN Lv1"* · *"người chơi có thể CỐ TÌNH không nâng"*

**Trạng thái: ĐÓNG.** Mọi thiết kế sau phải tuân theo.

## Hệ quả phải giải (chưa chốt cách giải)

`towers.json` hiện tại thiết kế cho tích luỹ. 4/6 tướng có Lv3 KHÔNG chạy độc lập được:

| Tướng | Kỹ năng Lv3 | Phụ thuộc cấp dưới | Hỏng thế nào |
|---|---|---|---|
| El Árbitro | `the_do` | `targetSelector=first_in_range_with_yellow_card` ← thẻ vàng do `the_vang` (Lv2) tạo | không ai có vàng → không bao giờ bắn |
| El Fideo | `duong_chuyen_vang` | `overridesTatCanhN=3` ← sửa `tat_canh` (Lv1) | override thứ không tồn tại |
| D10S | `ban_thang_the_ky` | `onlyWhileOwnSlowActive=true` ← slow của `ban_tay_cua_chua` (Lv1) | không bao giờ kích hoạt |
| Dibu | `nguoi_hung_luan_luu` | `healGoalPerNBlocks=3` ← cú cản của `can_pha` (Lv1) | không cản bóng, không hồi máu |

Batigol không "chết" nhưng Lv3 mất `sut_xuyen` → đánh đám đông tệ hơn Lv1.
La Pulga Lv3 mất `solo_run` → mất vai trò diệt boss (mô hình focus-fire ở `04` §3.3 dựa vào nó).

## Nợ mới sinh từ B-01

- `TargetingSystem.FirstInRangeWithYellowCard` có thể thành CODE CHẾT — phụ thuộc `the_do` được thiết kế lại ra sao. Đã viết + test ở vòng 9.
- Bảng cân bằng W1–W20 (`04` §4) dẫn xuất từ giả định tích luỹ → **phải chạy lại `balance_sim.py`**.
- `balance_sim.py` mua tham lam theo damage/Peso, giả định nâng cấp luôn tốt hơn → giả định này giờ SAI.
- Thang giá (Batigol 120/96/192, tổng Lv3 = 408) giả định nâng cấp là cải thiện. Nếu Lv3 là đánh đổi chứ không phải mạnh hơn, trả 3.4× giá để chuyên biệt hoá cần được kiểm lại.

---

# VÒNG 10 — THỰC THI B-01

## Kết quả: B-01 áp xong, cân bằng GIỮ ĐƯỢC 20/20

```
headroom: min 1.06 · tb 1.36 · max 1.53 · vỡ dải cứng [1.05, 1.65]: 0
boss W20: build đám-đông 0.58 → ÉP đổi đội hình ✅
cả 5 tướng sân đều được mua ở nhiều wave — 0 nội dung chết
91/91 test C# · path_check exit 0
```

## Thiết kế lại (towers.json)

`_upgradeRules.abilitiesAreCumulative: true → false`

| Tướng | Sửa gì | Vì sao |
|---|---|---|
| El Árbitro `the_do` | selector `..._with_yellow_card` → `..._without_card` | thẻ vàng do Lv2 tạo, Lv3 không chạy Lv2 → đứng im vĩnh viễn |
| El Fideo `duong_chuyen_vang` | trigger `passive` → `every_nth_attack`, tự chứa n/splash | `overridesTatCanhN` sửa kỹ năng Lv1 đã tắt |
| D10S `ban_thang_the_ky` | bỏ `onlyWhileOwnSlowActive`, thêm `requiresExternalSlowSource` | trỏ slow của Lv1. Cho nó tự slow = lén tích luỹ qua cửa sau → bắt nó cần Árbitro |
| D10S Lv3 | `splashRadius: 1.7 → 1.2` | 1.7 = 1.2 + 0.5 của `cu_cham_thien_tai` (Lv2) CỘNG SẴN vào level → mã hoá giả định tích luỹ |
| Dibu | cả 3 cấp chứa `blocksAtGoalLine` | **NGOẠI LỆ CÓ TÊN** — xem `_b01Exception` |

## Ngoại lệ Dibu — ghi rõ, không ngầm

Dibu: damage 0, attackRate 0, tầm 0.7–1.1. Cản bóng là thứ DUY NHẤT nó làm. Bỏ
`can_pha` ở Lv2/Lv3 = thủ môn không bắt bóng, và Lv3 hồi máu "mỗi 3 cú cản" khi
không còn cú cản nào. Không phải đánh đổi — là tướng hỏng.

Luật 18 chỉ miễn cho `slotType == "goalkeeper"`. 5 tướng sân KHÔNG mượn được cửa này.

## Defect phát hiện dọc đường

- **`rangeIncrementPerLevel: 0.5` là số chết** — mọi tướng thật đều `+0.2`. Sót từ
  trước lần thu tầm ×0.36 (vòng 5). Không luật nào đối chiếu `_upgradeRules` với
  `levels` → nó nằm im. Đã sửa về 0.2.
- **docs/05 dòng 356 vẫn ghi selector đã xoá** — grep bắt được. Đã sửa + thêm ghi chú B-01.
- **`splashRadius` CÓ trong levels** (tôi báo nhầm là thiếu, do bản dump không in trường đó).

## Luật mới — cả hai đã kiểm ngược

- **Luật 17** `targetSelector` phải thuộc tập code cài đặt được. Sinh ra vì `the_do`
  từng mang giá trị giờ không tồn tại, và 16 luật cũ để nó trôi qua.
- **Luật 18** ép B-01: `abilitiesAreCumulative` phải false · không kỹ năng nào được
  mang tham số trỏ cấp khác (`overridesTatCanhN`, `onlyWhileOwnSlowActive`) · mỗi
  `unlockLevel` đúng một kỹ năng (trừ thủ môn).

Probe 3/3 bắt đúng thông điệp:
```
cumulative  → [18] abilitiesAreCumulative = true — B-01 chốt là false
selector    → [17] el_arbitro.the_do targetSelector = `first_in_range_with_yellow_card`
crosslevel  → [18] d10s.ban_thang_the_ky có `onlyWhileOwnSlowActive`
```

`GameConfig.AbilitiesAreCumulative` vào model + mapper — cờ mà code không đọc chỉ là
lời hứa. Mapper KHÔNG dùng `Opt(...) ?? false`: thiếu cờ = lỗi, không phải mặc định im lặng.

## Bài học lớn nhất vòng này: TÁCH THAY ĐỔI TRƯỚC KHI ĐỔ LỖI

Chạy lại lần đầu: W15 = 1.67 vỡ dải. Tôi suýt kết luận "B-01 phá cân bằng".
Nhưng tôi đã đổi HAI thứ. Cô lập bằng probe:

```
B-01 một mình (giữ hằng số 3):  0/20 vỡ · max 1.63
B-01 + tỉ lệ bán kính của tôi:  1/20 vỡ · max 1.67
```

**B-01 vô can.** Thứ phá là thay đổi của chính tôi.

## Bẫy suýt sập: dải số đẹp đổi bằng một tướng chết

Neo `d10sAverageTargetsHit` ở r=1.7 cho **20/20 wave đạt dải** — trông như thắng.
Nhưng đọc cột build: **D10S biến mất khỏi MỌI wave**, và boss W20 mất nanh
(build đám-đông 0.72 → 1.17 = boss không ép gì nữa).

Tức tôi đã đổi một tướng tier S thành nội dung chết + giết cơ chế boss, để mua một
con số đẹp. Vòng 5 đã học đúng bài này (El Fideo thống trị → 2/5 tướng chết).

Neo ở r=1.2 (giữ hiệu chuẩn Lv1 cũ, sửa việc mô hình ĐẾM THIẾU Lv2) → D10S sống.

## σ: đổi vì B-01, không phải vì vặn cho vừa

`sigmaByTowerLevel: [1.10, 1.20, 1.35] → [1.10, 1.15, 1.20]`

Thang cũ dốc VÌ cộng dồn (Lv3 = 3 kỹ năng). B-01 xoá tiền đề → σ phải bớt dốc.
Đây là suy ra từ luật mới, không phải tinh chỉnh mò. Quét thử:

| σ | vỡ | D10S | boss W20 |
|---|---|---|---|
| [1.1, 1.2, 1.35] | 1 | 11 wave | 0.72 ép ✅ |
| **[1.1, 1.15, 1.2]** | **0** | **14 wave** | **0.58 ép ✅** |
| [1.1, 1.13, 1.16] | 1 (thủng SÀN 1.04) | 14 | 0.56 |
| [1.1, 1.1, 1.1] | 1 (thủng SÀN 1.01) | 15 | 0.54 |

Thắng mọi chiều cùng lúc. Vẫn là số BỊA — chỉ M1 đo được.

## Nợ

- η / σ / τ vẫn bịa. B-01 làm σ **ít căn cứ hơn trước**, không phải hơn.
- `d10sAverageTargetsHit = 3 @ r=1.2` — neo tuỳ chọn, đo ở M1.
- Kỹ năng vẫn chưa được mô hình hoá riêng lẻ: sim gộp tất cả vào σ. Nên nó KHÔNG
  thấy Batigol Lv1 xuyên hàng vs Lv3 nuke đơn — đúng đánh đổi mà B-01 tạo ra.
  Đây là nợ THẬT: mô hình đang mù trước chính luật mới.
- 21 failure mode chưa có ID `FM-NN`.

---

# VÒNG 11 — DamageSystem

`Assets/_Project/Core/Match/DamageSystem.cs` · 106/106 test (91 → +15) · Unity dịch OK

## Quyết định cốt lõi: DamageSystem GIỮ máu, quân không giữ

Nếu quân tự giữ máu thì ai cũng `enemy.Hp -= dmg` được, và ai cũng tự kết luận
"tôi giết nó". Ở đây tướng KHÔNG CÓ CÁCH NÀO chạm vào máu — nó không cầm tham
chiếu, chỉ gọi được `Apply()`. **Luật ép bằng khả năng truy cập, không bằng kỷ
luật người viết.**

`Dictionary<int, double> _hp` sống trong DamageSystem. `Register` lúc spawn.

## KHÔNG cộng tiền ở đây

docs/05 bảng §3 dòng 126 có cột "không làm": `Combat/DamageSystem | Áp sát thương,
quyết định ai gây đòn kết liễu | **Cộng tiền (phát event)**`.

→ Lớp này phát `EnemyKilled`; MatchController nghe rồi mới gọi EconomyService.
Gọi thẳng thì luật #1 (ví một cổng vào) và luật #2 giẫm lên nhau.

## `RemoveWithoutKill` tách riêng — không phải cầu kỳ

Dibu `can_pha` khai `dropsBounty: false`. Quân lọt lưới cũng không rơi tiền.
Dùng `Apply(∞)` cho hai ca này sẽ phát KillInfo → MatchController trả tiền →
**người chơi được thưởng vì để quân tới tận cầu môn**. Nên có hàm riêng, không
phát event.

## Thứ tự 2 dòng là load-bearing — chứng minh bằng probe

`_hp.Remove(enemyId)` PHẢI trước `EnemyKilled?.Invoke(...)`.

Probe đảo thứ tự → test `Nguoi_nghe_goi_nguoc_lai_khong_gay_phat_hai_lan`:
```
at DamageSystem.Apply(Int32, Double, String)
at DamageSystemTests.<Nguoi_nghe_goi_nguoc_lai>b__0(KillInfo)
at DamageSystem.Apply(Int32, Double, String)
...
Test Run Aborted.        ← đệ quy vô hạn, tràn stack, sập cả test host
```
Không phải "test đỏ" — là sập. Người nghe event gọi ngược lại Apply (hiệu ứng dây
chuyền) phải thấy con đã chết.

## Test FM-07 (`01` §8 mục 7)

- `Hai_tuong_cung_ban_phat_cuoi_chi_mot_lan_ghi_cong` — Bati 90 rồi Pulga 56 lên
  con còn 10 máu → 1 event, KillerSlotId = "f01"
- `Muoi_tuong_cung_ban_van_dung_mot_event` — dạng mạnh nhất của bất biến
- `Danh_thua_thi_vut_phan_thua` — Dealt = 10 (máu có thật), Overkill = 80 (vứt)
- `Xoa_khong_qua_sat_thuong_thi_khong_ai_duoc_ghi_cong` — 0 event = 0 tiền

`KillInfo.Overkill` giữ lại để đo `etaWasteFactor = 0.75` ở M1 — một trong ba hệ
số bịa.

## Việc tiếp

`GoalHealth` → `WaveSpawner` → rồi mới tới 18 kỹ năng (hiện 0 dòng).

---

# VÒNG 12 — GoalHealth · WaveSpawner · SlotManager · UpgradeService

150/150 test (106 → +44) · Unity 0 error CS · core 15 file / 2405 dòng / dll 70 KB

## GoalHealth

- Cột "không làm" (`05` §3): **"Biết về Dibu"** — lớp này không biết thủ môn tồn tại.
  Dibu cản qua `DamageSystem.RemoveWithoutKill`; Lv3 hồi máu gọi `Heal()` từ ngoài.
- **KHÔNG có luật "trừ tối đa còn 1"** — `01` §8 mục 9: boss trừ đúng 5, lọt lưới
  khi máu ≤ 5 là THUA. Cài "để lại 1 máu" phá điều kiện thua và làm boss vô hại
  đúng lúc nó đáng sợ nhất.
- **`CleanSheet` ≠ `Current == Max`.** `01` §3 định nghĩa 3 sao bằng ngoặc đơn:
  "(clean sheet — không thủng lưới lần nào)". Thủng rồi mua "Sửa cầu môn" về 20 →
  đầy máu nhưng KHÔNG clean sheet → 2 sao. Chấm bằng `Current == Max` thì tiền mua
  được sao.

## WaveSpawner

- Sinh TOÀN BỘ lịch một lần (`Schedule(wave)`), không tick từng frame → tất định,
  test kiểm được từng mốc, lớp vỏ Unity chỉ đi dọc list.
- Cột "không làm": **"Biết quân chết thế nào"** — không cầm DamageSystem.
- **Boss ra CUỐI** (`waves.json._bossSpawnNote`): giả định nền của mô hình hoả lực
  tập trung `04` §3.3. Test ép ở W10 và W20.
- **Máu boss lấy THẲNG từ `appearances`**, KHÔNG nhân hpScaling — nhân nữa là nhân
  đôi độ khó mà không ai định.

### ⚠️ LUẬT TÔI TỰ CHỌN, KHÔNG CÓ TRONG DOCS: thứ tự spawn

`waves.json` chỉ khai SỐ LƯỢNG mỗi loại, không khai thứ tự. `_bossSpawnNote` chốt
boss ra cuối nhưng quân thường thì im lặng.

Chọn **xen kẽ đều theo tỉ lệ, tất định**:
- Mô hình `04` tính `S = (n−1) × interval × v̄` — coi wave là DÒNG ĐỀU. Gom theo
  loại thì 7 tambor (máu ×2) dồn cuối wave thành một cục → mô hình nói về trận khác.
- Tất định → bảng cân bằng tái hiện được. Ngẫu nhiên thì cùng wave khó dễ khác nhau
  mỗi lần, không debug được.
- `ids.Sort(Ordinal)` để thứ tự KHÔNG phụ thuộc thứ tự khoá Dictionary.

Đo được: W20 = `T A D T A T A D T A T D A T A T D A T D A T A T D T A D T A T A T A T A T`
(16 tifoso, 14 adepto, 7 tambor) — chuỗi dài nhất cùng loại = **1**.

🔴 CHƯA ĐO TRÊN MÁY. Gom theo loại tạo nhịp "sóng" có thể hay hơn về cảm giác.
Đổi ở `waves.json → spawnOrder` (chưa cài) sau khi chơi thử.

## SlotManager

- Cột "không làm": **"Mua tướng"** — không đụng tiền.
- Constructor đối chiếu `path.json` (12 ô) với `economy.json` (`fieldSlots: 11` +
  `goalkeeperSlots: 1`). Lệch = ném lúc DỰNG, không phải lúc người chơi bấm.
- **Luật "Dibu chỉ ở ô thủ môn" là HAI CHIỀU.** Chỉ ép một chiều thì Batigol đứng
  được ở gk01 — sau cầu môn, tầm 1.0, gần như không canh gì, người chơi mất 120 Peso
  vì một luật thiếu. Có test cho cả hai chiều.
- Nâng cấp reset `CooldownRemaining` + `AttacksFired`: `01` §8 mục 6, và theo B-01
  lý do mạnh hơn — cấp mới chạy kỹ năng KHÁC HẲN nên cooldown cũ vô nghĩa.

## UpgradeService — chỗ SlotManager gặp EconomyService

Tồn tại vì mỗi thao tác phải đổi **CẢ HAI hoặc KHÔNG GÌ**:
- Trừ tiền xong mới phát hiện ô đầy → mất tiền, không có tướng
- Đặt tướng xong mới phát hiện thiếu tiền → tướng miễn phí

Hỏi hết điều kiện TRƯỚC, chỉ chạm state khi cả hai đều thuận. Có test cho cả hai
chiều (`Thieu_tien_thi_khong_dat_tuong`, `Luat_o_hong_thi_khong_tru_tien`).

## Model bổ sung

- `TowerDef.MaxInstances` (vắng = 0 = vô hạn; chỉ Dibu khai 1). `?? 0` ở đây ĐÚNG,
  khác `abilitiesAreCumulative` (thiếu = lỗi) — "không khai" có nghĩa rõ ràng.
- `TowerDef.AbilityAt(level)` — B-01: đúng một kỹ năng, của cấp hiện tại.

## `InternalsVisibleTo("LaMuralla.Core.Tests")`

State chạy của tướng để `internal set` — chỉ core sửa, UI chỉ đọc. Test cần dựng
"tướng đang còn 7.5s cooldown" mà chưa có API tua thời gian (thuộc hệ chiến đấu,
chưa viết). Mở cho đúng assembly test thay vì nới `public` — nới public thì bất
biến mất vĩnh viễn để đổi lấy tiện lợi của một test.

## Việc tiếp

Cơ chế bắn (đơn/lan/xuyên) → 18 kỹ năng → MatchController → lớp Unity → HUD

---

# VÒNG 13 — CombatResolver + luật 19

171/171 test (150 → +21)

## CombatResolver — hình học thuần, tách khỏi DamageSystem

"Trúng ai" là hình học · "chết chưa, ai ghi công" là luật kinh tế. Gộp lại thì
không test được cái nào mà không dựng cái kia.

### 🔴 Bug thật test bắt được: tia xuyên là CHỮ NHẬT, không phải VIÊN NANG

Cách viết hiển nhiên — đo khoảng cách từ điểm tới ĐOẠN thẳng rồi so nửa bề rộng —
mô tả một VIÊN NANG: chữ nhật + hai chỏm tròn bán kính `halfW` = 0.25.

Hai chỏm đó cho Batigol **+0.25 unit ngoài tầm khai báo = +18% trên tầm Lv3 (1.4)**.
Không ai khai, không ai đo. Dự án này đã suýt chết một lần vì tầm trôi (chord siêu
tuyến tính → phải thu tầm ×0.36 ở vòng 5).

Sửa: chiếu lên hướng tia, ép `0 ≤ along ≤ length` VÀ `|perp| ≤ halfWidth`.
Test `Tia_la_chu_nhat_khong_phai_vien_nang` quét quanh mũi tia: 1.40 trúng, 1.41 trượt.

### Luật docs đã ép

- **Tâm lan = VỊ TRÍ MỤC TIÊU, không phải điểm đạn chạm** (`05` §4.5 mục 2). Đạn
  BÁM mục tiêu (`05` §4.3) nên lúc chạm quân đã đi tiếp. Lấy điểm đạn chạm thì vùng
  lan lệch về sau, và giả định "D10S chạm 3 mục tiêu" (`04` §8 mục 4) sai.
  Test dựng đúng tình huống: tâm đúng → 3 con, tâm sai → 2 con (bắt nhầm con khác).
- **Tia chốt hướng LÚC BẮN**, không homing (`05` §4.3: "Trừ `sut_xuyen` của Batigol
  — nó bay theo đường thẳng chốt lúc bắn, và đó chính là cơ chế xuyên"). Để nó
  homing thì tia cong theo mục tiêu → luôn trúng cả hàng bất kể đường cong → bản sắc
  Batigol (thưởng cho vị trí tốt) biến mất.
- **"Trên khúc cua có thể chỉ trúng 1 con — ĐÓ LÀ Ý ĐỒ"** (`05` §4.5 mục 3). Có test
  khoá, để người sau không "sửa" nó.
- Quân là ĐIỂM, không bán kính va chạm — `03` §6: va chạm giữa quân "không có,
  chồng lên nhau chỉ là vấn đề hình ảnh".

## 🔴 Luật 19 — quả mìn TÔI tự đặt ở vòng 10

`d10s.cu_cham_thien_tai` khai `splashRadiusBonus: 0.5`, trong khi
`levels[1].splashRadius` đã là **1.7 = 1.2 + 0.5** — bonus ĐÃ cộng sẵn.

Code đọc cả hai ra **2.2**. Không crash, không log. D10S Lv2 chỉ lặng lẽ mạnh hơn
thiết kế 29%, và bảng cân bằng 20/20 wave nói về một trận không tồn tại.

Không có gì trong repo chặn được — 18 luật cũ đều không hỏi câu này.

Sửa: gỡ `splashRadiusBonus`. **`levels[]` là nguồn chân lý của chỉ số; kỹ năng chỉ
đặt TÊN cho chênh lệch giữa các cấp, không cộng thêm lần nữa.**

Luật 19 chặn cả LỚP bug: `splashRadiusBonus`/`rangeBonus`/`damageBonus`/
`attackRateBonus` — mọi tham số trùng vai với trường ở `levels`.

Probe: trả lại quả mìn → luật 19 bắt đúng thông điệp. Gỡ ra → 171/171.

## Việc tiếp

18 kỹ năng (0 dòng) → MatchController → lớp Unity gameplay → HUD

⚠️ Khi viết kỹ năng, NHỚ: `levels[].splashRadius` / `range` / `damage` /
`attackRate` là chỉ số CUỐI CÙNG. Kỹ năng KHÔNG cộng thêm vào chúng.

---

# VÒNG 14 — AbilityEngine (18 kỹ năng)

200/200 test (171 → +29) · Unity 0 error CS

## Kiến trúc: AttackPlan là DỮ LIỆU, không phải hành động

```
AbilityEngine.Plan()  → AttackPlan (bắn ai, bao nhiêu, hình gì)
CombatResolver        → trúng những ai
DamageSystem          → trừ máu, ai ghi công
```
Ba bước tách rời → test được "tính sát thương đúng chưa" mà không dựng cả trận.

## B-01 trong code: không có vòng lặp duyệt kỹ năng

`def.AbilityAt(level)` — MỘT cái, hoặc null. Không `foreach (ability in t.Abilities)`.
Đó là B-01 được ép bằng cấu trúc, không bằng điều kiện `if`.

Test khoá: Batigol Lv1 = `Line` (xuyên) · Lv2 = `Single` · Lv3 = `Single`.
Lv3 KHÔNG còn xuyên — đó là ý đồ.

## 🔴 Luật 19 được ép trong engine

`case "cu_cham_thien_tai"` KHÔNG cộng bán kính — nó lấy thẳng `lv.SplashRadius`.
Test `D10S_lan_dung_bang_levels_KHONG_cong_them`:
```
Lv1 → 1.2 · Lv2 → 1.7 (🔴 KHÔNG phải 2.2) · Lv3 → 1.2
```

## Model bổ sung

- `TowerLevel.SplashRadius` — vắng = 0 = không lan. Chỉ D10S khai.
- `ITarget.MaxHp` — `ban_nang_sat_thu` là "+30% khi máu > 80%", % của máu TỐI ĐA.
  Không có MaxHp thì không tính được %, và trình biên dịch đã chỉ đúng 2 chỗ thiếu.
- `TowerInstance`: `AbilityActiveRemaining` (chỉ `solo_run` dùng — ×2 trong 4s),
  `SustainedFireSec` (`suc_ben` ramp), `BlocksMade` (`nguoi_hung_luan_luu`).

## `default:` trong switch NÉM, không im lặng

Kỹ năng lạ → `ConfigException`. Thêm config mà quên thêm code = tướng im lặng
không làm gì, không crash, không log. Test `Moi_ky_nang_trong_config_deu_duoc_cai`
duyệt 6 tướng × 3 cấp — thêm kỹ năng mới mà quên cài là ĐỎ ngay.

## Đánh đổi B-01 đo được trong test

- Batigol: Lv1 xuyên hàng ↔ Lv3 nuke ×2.5 đơn mục tiêu
- D10S: Lv2 lan 1.7 ↔ Lv3 lan 1.2 + buff toàn đội 20% (và KHÔNG tự làm chậm được)
- El Fideo: Lv2 nhịp bắn +30% bền ↔ Lv3 tạt cánh mỗi 3 đòn (thay vì 5)

## Việc tiếp

MatchController → lớp Unity gameplay → HUD → FM-NN → build+đo

⚠️ Chưa cài: phần ÁP HIỆU ỨNG theo cooldown/aura (slow, thẻ, cản bóng, hồi máu).
AbilityEngine mới lo phần SÁT THƯƠNG + HÌNH DẠNG. MatchController sẽ lo phần kia.

---

# VÒNG 15 — MatchController · GAME CHƠI ĐƯỢC ĐẦU-CUỐI

218/218 test (202 → +16) · Unity 0 error CS

## 🎮 Cột mốc: trận đấu thật chạy hết 20 wave

```
Won · wave 20 · máu 17/20 (2 sao)
ví 4596 · kiếm cả trận 10754 · overkill 56888
build cuối: batigol3 ×5 · el_fideo3 ×6 · dibu1
```
Người chơi tự động, không mock, mọi module cùng chạy.

## Luật #1 gặp luật #2 — 6 dòng, nhìn thấy được

```csharp
Damage.EnemyKilled += OnEnemyKilled;   // ctor MatchController
```
DamageSystem quyết AI GIẾT (nó KHÔNG được cộng tiền) → MatchController nghe →
EconomyService.EarnKill (cổng duy nhất của ví). Đây là chỗ DUY NHẤT hai luật chạm nhau.

`KillerSlotId == null` → không ai ghi công → không tiền (Dibu cản, quân lọt lưới).

## Enemy KHÔNG giữ máu

`Enemy.Hp => _damage.HpOf(Id)`. Nhìn ngược đời (một "con quân" không biết máu mình),
nhưng đó chính là điểm: nó KHÔNG ĐƯỢC biết, vì biết thì sẽ có người sửa → FM-07.

## Cooldown chỉ reset khi kỹ năng THỰC SỰ nổ

`if (fired) t.CooldownRemaining = ab.CooldownSec;`

Thẻ vàng không tìm được ai → không tiêu cooldown, thử lại frame sau. Reset vô điều
kiện thì Árbitro "phí" 8 giây vì không có mục tiêu, và người chơi thấy nó đứng im
mà không hiểu vì sao.

## Ba test tôi viết SAI, và mỗi cái dạy một điều

1. **`Aura_het_tac_dung`** — đặt Árbitro ở `f02`. f02 cách đường **1.31**, Árbitro
   Lv1 tầm **1.2** → không với tới, không con nào bị chậm. Không phải bug: đó là
   thang tầm ở path.json (`f02 1.31 → Árbitro ❌`) đang chạy đúng. Đổi sang `f07` (0.79).

2. **Cùng test đó, lần hai** — kiểm "ra khỏi tầm" ở quãng đường 20. Nhưng đường là
   SERPENTINE: f07 nằm ở arc 0.59 ≈ quãng 24. Ở quãng 20 quân đang QUAY LẠI gần f07.
   **"Đi xa hơn" trên đường thẳng ≠ "ra xa tháp" trên đường cong.** Đổi sang quãng 33.

3. **`Nguoi_choi_biet_tieu_tien`** — hai lần sai chiến lược:
   - Mua trước nâng sau → tiêu 1320 lấp 11 ô, hết tiền nâng → **thua W7**, build toàn Lv1
   - Chỉ biết Batigol → Batigol Lv1 tầm 1.0 chỉ với tới 5/11 ô → max 5 ô rồi **ngồi
     trên 5956 Peso** không tiêu được → **thua W19**

   Sửa: nâng TRƯỚC mua SAU (nâng 96 rẻ hơn mua 120 mà dame ×1.6), và dùng El Fideo
   (tầm 1.6, với tới 11/11) cho 6 ô xa → **THẮNG**.

   ⚠️ Cái thua W19 KHÔNG phải engine hỏng — balance_sim ở W19 cũng dùng SÁU loại
   tướng. Game đang ép đa dạng đội hình, đúng thiết kế.

## 🔴 NỢ MỚI, SỐ LỚN: overkill 56888 vs kiếm cả trận 10754

`etaWasteFactor = 0.75` giả định phí 25%. Đo thật trên build Batigol/Fideo:
overkill **gấp 5.3 lần** tổng tiền kiếm được. Batigol Lv3 = 225 dame vào adepto
~100 máu → thừa 125 mỗi phát.

Chưa quy đổi được overkill → η vì hai đơn vị khác nhau (sát thương vs Peso), nhưng
độ lớn đủ để nghi η = 0.75 SAI NHIỀU với build nặng đơn-mục-tiêu.

→ Đây chính là món nợ mô hình đã hẹn: giờ CÓ `MatchController.TotalOverkill`, tức
đã đo được. Việc còn lại là tính `η_thật = sát_thương_hữu_ích / tổng_sát_thương`.

## Việc tiếp

Lớp Unity gameplay → HUD → FM-NN → build lên máy

---

# VÒNG 16 — GAME CHƠI ĐƯỢC TRÊN IPHONE

218/218 test · Unity 0 error CS · `** BUILD SUCCEEDED **` · PID trên máy, 0 lỗi runtime

## Lớp Unity — 4 file, KHÔNG chứa luật chơi nào

| File | Việc |
|---|---|
| `Draw.cs` | sprite/vòng sinh trong code, bảng màu tạm. 0 file art. |
| `MatchView.cs` | đẩy Tick, vẽ quân/tướng, nhận chạm |
| `RadialMenu.cs` | menu xoè quanh ô — bán kính đọc từ `path.json → ui`, KHÔNG gõ tay |
| `Hud.cs` | tiền/máu/wave/nút, màn thắng-thua (IMGUI, không prefab) |

Ranh giới: *"Nếu thấy mình sắp viết `if (enemy.Hp <= 0)` ở đây thì dừng lại."*

## Bẫy đã gặp

- **`activeInputHandler: 1`** = Input System ONLY → `Input.GetMouseButtonDown` KHÔNG
  chạy (API legacy đã tắt). Dùng `Touchscreen.current` / `Mouse.current`, và asmdef
  phải tham chiếu `Unity.InputSystem`.
- **`Draw.Sprite()` trùng tên với kiểu `UnityEngine.Sprite`** → CS0119. Đổi thành
  `Draw.Make()`, và khai `UnityEngine.Sprite` tường minh cho kiểu trả về.
- Để sót một biểu thức rác (`m.Upgrades.TowerById is var _ ? ... : null!`) — vô
  nghĩa, lẽ ra phải xoá lúc viết. CS0837 bắt được.

## Nút ×2 KHÔNG phải Time.timeScale

`05` §4.4: thời gian NGHỈ không đổi khi ×2, chỉ thời gian TRẬN nhanh lên. Nên nó
nhân vào tham số `MatchController.Tick(dt × mult)`, không nhân vào đồng hồ Unity.

## HUD tôn trọng safe area

Notch che 1.30 unit trên, home indicator 0.94 dưới (đo trên iPhone 12, vòng 8).
Cỡ chữ theo `Screen.width / 1080f` — gõ `fontSize = 40` thì máy khác ra cỡ khác.

## FM-01…FM-21 — nợ cũ nhất repo, đã trả

`docs/01` §8 đánh số theo VỊ TRÍ (1–21), 26 chỗ trỏ theo số → chèn một dòng là lệch hết.

Đổi bảng sang `| FM-01 |`, header `| # |` → `| ID |`, và 26 tham chiếu → `**FM-NN**`.

⚠️ Cạm bẫy: có HAI danh sách đánh số. `04` §8 là GIẢ ĐỊNH CÂN BẰNG, không phải
failure mode. Đổi nhầm là tạo tham chiếu trỏ sai file. Regex chỉ khớp `01` §8;
4 chỗ trỏ `04` §8 giữ nguyên — đã kiểm ngược.

## Trạng thái TOÀN BỘ

```
Core:     16 file · 218 test · 0 warning · Unity dịch 0 lỗi
Config:   5 json · 19 luật validate · path_check exit 0
Balance:  20/20 wave trong dải [1.05, 1.65] · boss W20 ép đổi đội hình
Game:     CHƠI ĐƯỢC trên iPhone 12 — mua/nâng/bán, 20 wave, thắng/thua/3 sao
```

## Còn lại (task #29)

- Chơi thử trên máy → đo `η` thật từ `MatchController.TotalOverkill`
- **Đo được rồi nhưng CHƯA quy đổi**: overkill 56888 vs kiếm 10754 (build Bati/Fideo).
  η = 0.75 khả năng SAI NHIỀU với build nặng đơn-mục-tiêu.
- Art thật (docs/06) — không thay đổi gameplay
- Thứ tự spawn: xen kẽ (tôi chọn) vs gom theo loại — chỉ tay người chơi trả lời được

---

# VÒNG (art) — 2026-07-30: D10S ném theo cấp (bóng/chai/cối)

Chi tiết đầy đủ + toàn bộ quyết định kỹ thuật: `.dev-agent/.local/history/2026-07-30-d10s-throw-skill.md`
và memory `la-muralla-art-pipeline.md` (mục "D10S 2026-07-30").

Tóm tắt: art từ `images/d10.png` (declak + bake) cho D10S, CHỈ đổi lớp animation/ném
theo cấp, KHÔNG đụng balance/`towers.json`. Sửa `SpriteAnimBaker.cs` (thêm `BakeD10`)
+ `MatchView.cs` (đạn bay rồi mới nổ, thay vì nổ tức thời). Bake headless OK, build 0
lỗi. `dotnet test`: 213/219 — 6 fail xác nhận **pre-existing, không liên quan** (xem
memory `la-muralla-core-test-debt.md`: Batigol damage trong `towers.json` lệch so với
assert cũ trong test).

## Việc tiếp
Play-test trong Unity Editor để xác nhận animation D10S 3 cấp + góc bay chai
(`BottleBakedDeg` ở `MatchView.cs`, đo tay, có thể lệch — nói tôi biết để chỉnh).

## Blocking
None.

---

# VÒNG (art) — 2026-07-31: D10S declak lại sheet mới

User export lại `images/d10.png` (vẫn 1152×928, lưới 5×4, layout hàng giữ nguyên:
idle=row0, bóng=row1, chai=row2, cối=row3). Xử lý lại nền + viền.

- Script declak dựng lại và **lưu hẳn vào repo: `tools/declak.py`** (scratchpad bị
  xoá mỗi phiên, đã phải viết lại 3 lần). Cần venv có pillow/numpy/scipy.
- Thuật toán bổ sung 2 bước so với lần trước: (a) lan truyền vùng xám để quét halo
  ~226-232 lọt khe giữa 2 tông checker (215/244) — trước đó sót 839 mảnh / 9k px;
  (b) xoá vô điều kiện mọi px `sat≤30` trong dải ±9px quanh mọi đường kẻ lưới, sau
  khi đã đo chắc chắn 0 px nhân vật (`sat>60`) nằm trong dải đó.
- Kết quả: còn 15 mảnh nhỏ / 235 px, nhân vật + áo + quần nguyên vẹn.
- Xuất `Art/Characters/d10s_hero.png` + crop lại `Resources/Art/d10s_bottle.png`
  (chai đổi vị trí sang góc phải-trên frame, bbox mới 1085-1143 × 550-597, 59×48).
- `MatchView.cs`: `BottleBakedDeg` 150f → **25f** (đo lại từ vệt tốc độ → tâm chai).
- **KHÔNG cần bake lại Unity**: canvas + lưới không đổi → 20 slice trong
  `d10s_hero.png.meta` và `d10s.controller` + 4 `.anim` vẫn hợp lệ; chỉ cần Unity
  reimport PNG (tự động khi focus Editor).

## Việc tiếp (vòng này)
Play-test: xác nhận hết viền/nền ở slot D10S, và góc bay chai ở Lv2 với `BottleBakedDeg=25f`.

## Blocking (vòng này)
Không bake headless được — Unity Editor của user đang mở, giữ `Temp/UnityLockfile`
(PID 11237). Không tự tắt Editor. Không chặn việc gì vì vòng này không cần bake.

---

# VÒNG 22 — 2026-08-21: cân bằng tiền + tăng số quái + sửa nghịch lý giá La Pulga

## Yêu cầu user (3 lượt, gộp)
1. Cân bằng lại tiền khởi đầu + tiền khi quái chết, tăng số quái mỗi wave
2. Giảm dame La Pulga xuống 45, giảm giá D10S còn 240
3. Tướng nào nhiều tiền mà dame không tương xứng thì tự fix theo đề xuất của agent

User chốt hướng: **nới tiền, giữ độ khó** · quái **+25%** · batigol **giữ 20/30/45, sửa test theo config**.

## Đã đổi

| File | Thay đổi |
|------|----------|
| `config/waves.json` | mọi spawn ×1.25 half_up → 363 → **465 con** cả trận (+28%) |
| `config/enemies.json` | baseBounty 8/14/25 → **5/11/24**; boss hp 2100/6700 → **2500/9800** |
| `config/economy.json` | startingCash 550 → **700**; lifetimeCashExpected → 11515 |
| `config/towers.json` | la_pulga Lv1 dmg 56→45, **Lv3 dmg 141→170**; d10s giá 280/224/448 → **240/192/384** |
| `tools/balance_sim.py` | sửa `--solve` (KeyError `hpMultiplier`, chết im lặng nhiều vòng) |
| 7 file test | số bám config → suy TỪ config; fixture 20-wave viết lại |
| `docs/02,03,04` | sinh lại bằng gen_docs.py |

## Worked (with evidence)

- `dotnet test`: **210/210** (trước: 204/210 — 6 đỏ)
- `MatchControllerTests.Nguoi_choi_biet_tieu_tien_thi_thang_duoc_20_wave`: **Won · wave 20 · máu 11/20** (2 sao). Baseline trước vòng này: **Lost · wave 5**.
- `startingCash 700` đo từ engine, không gõ tay. Vách rất sắc: 650 thua W10 · 670 tới W20 rồi chết 0/20 · **700 thắng 11/20** · 730/760 y hệt 700 (bão hoà).
- Boss headroom: W10 **1.31** · W20 **1.30**, build đám-đông 0.39/0.60 → vẫn ÉP đổi đội hình.
- `gen_docs.py --check` khớp · `path_check.py` exit 0.

## Did NOT work (và vì sao)

- **Mọi phương án "nới bounty" đều VỠ trần chi tiêu.** +25% quái mà giữ bounty 8/14/25 → tiền cả trận 13562 vs trần 11696 = **vượt 16%**. Phẳng hoá + nâng → vượt 30–38%. Đó là lý do bounty phải HẠ dù user chọn "nới tiền": tổng tiền cả trận VẪN TĂNG (11276 → 11515), chỉ là tăng qua SỐ QUÂN thay vì qua thưởng mỗi con.
- **Buff dame La Pulga Lv3 một mình làm vỡ boss W20** (1.58 → 1.91, trần 1.65). Chỉ có 4% dư địa. Phải nâng máu boss kèm theo, số lấy từ `--solve`.
- **Cắt giá La Pulga để chữa nghịch lý: không được.** Luật 3 ép Lv2/Lv3 = 0.8×/1.6× giá mua, mà trần maxBuildCost = 11×(La Pulga full) + Dibu → hạ giá là hạ trần, lifetime lập tức vượt.

## 🔴 CÒN NỢ — mô hình và engine ĐANG NÓI HAI CHUYỆN KHÁC NHAU

`balance_sim.py` nói **tb 2.09 · 18/20 wave vỡ dải, toàn hướng QUÁ DỄ**.
Engine thật nói **thắng sát nút, còn 11/20 máu**.

Bằng chứng mô hình sai chứ không phải engine sai: engine đo `overkill 22567` trên
`kiếm cả trận 10945` — hao phí thực tế cao hơn HẲN `etaWasteFactor 0.75` mà mô hình
giả định. `economy.json` đã tự cảnh báo: η, σ, τ đều là ước lượng chưa đo, nhân nhau
thì lệch tới 73%.

**Đừng chỉnh cân bằng theo bảng sim cho tới khi η được đo lại ở M1.** Thước đo dùng
được lúc này là bài 20-wave trong `MatchControllerTests`.

## Còn lại

- **Bake lại config trong Unity**: `La Muralla → Bake Config`. `BakedConfig.asset` là sản phẩm phái sinh, chưa sinh lại (không chạy được Unity headless từ phiên này).
- Chơi thử để xác nhận 47 con ở W20 không tụt FPS trên iPhone 12 — số quân cùng lúc chưa từng cao thế này.
- `so_10` (La Pulga Lv3) vẫn là kỹ năng ĐÁM ĐÔNG gắn trên tướng ĐƠN MỤC TIÊU — buff dame chỉ che triệu chứng. Sửa gốc cần kỹ năng Lv3 mới + code AbilityEngine.
- Biên trần chi tiêu còn **+1.5%** (trước +3.6%). Mỏng. Lần đổi thưởng/số quân sau phải tính lại trước khi gõ.

## Build iPhone — 2026-08-21 09:11 ✅

Chuỗi `Unity → Xcode → xcodebuild → iPhone` chạy hết, cài và khởi chạy được trên iPhone 12 `Tult_ip` (iOS 18.2).

- `[BuildScript] OK — 1019 MB, 17s → ../ios-build`
- `** BUILD SUCCEEDED **` · ký bằng `Apple Development: letuantu13dt1@gmail.com (P2TS2X2378)`, profile `iOS Team Provisioning Profile: com.tuantu.lamuralla`
- Cài xong `com.tuantu.lamuralla`, launch tới `sceneWillEnterForeground()`, PID còn sống sau 25s — không crash.
- Xác nhận bản build mang cân bằng MỚI: `ios-build/Data/resources.assets` chứa `startingCash = 700`.
- `MatchView.TestExtraCash` **đặt về 0** → máy chạy đúng 700 Peso thiết kế, không phải 1650.

### Hai cái bẫy đã vấp, ghi lại để khỏi vấp nữa

1. **Play trong Editor ra màn hình trống** không phải lỗi code: Editor chạy scene ĐANG MỞ, không chạy scene đầu Build Settings. Phải mở `Assets/_Project/Scenes/Match.unity`. `SampleScene` chỉ có Camera + Light.
2. **`devicectl` Identifier ≠ `xcodebuild` destination id.** `devicectl list devices` in ra CoreDevice UUID (`7B4409DB-…`) nhưng `xcodebuild -destination id=` cần UDID (`00008101-000E446C3CFA001E`). Lấy UDID bằng `xcrun xctrace list devices`.

### Việc tiếp
Chơi thử W15–W20 trên máy: (a) 47 con cùng lúc ở W20 có tụt FPS không, (b) độ khó có khớp "thắng sát nút 11/20 máu" mà engine dự đoán không.

---

# VÒNG 23 — 2026-08-21: sửa giật/đứng hình trên iPhone

User báo "thỉnh thoảng bị giật đứng màn hình". Tìm được **3 lỗi cấp phát trong vòng lặp nóng**.
Cả ba CÓ SẴN từ trước; tăng 28% số quái ở vòng 22 chỉ làm chúng lộ ra.

| # | Lỗi | Tần suất trước sửa | Sửa |
|---|-----|-------------------|-----|
| A | `Draw.cs:55` `new Material()` mỗi lần gọi; gán qua `.material` nên Unity nhân bản thêm → **2 material/sự kiện**, không bao giờ huỷ | mỗi cú đánh trúng + mỗi vụ nổ ≈ 20–40/giây ở W20 | 1 Material dùng chung + `sharedMaterial` ở cả 5 nơi gán |
| B | `Hud.cs` `new GUIStyle`/`new GUIContent` trong `OnGUI` (4 chỗ nóng) | ~1000/giây (OnGUI chạy ≥2 lần/frame) | mượn-trả `alignment`, tái dùng `GUIContent` + 3 style cache |
| C | `MatchView` `Resources.Load<RuntimeAnimatorController>` ở đường spawn | 47 lần ở W20 + mỗi lần dựng tướng | `AnimCtrl()` có dictionary cache, nhớ cả `null` |

**A nguy hiểm nhất và là nghi phạm chính của chữ "ĐỨNG"**: Material là đối tượng NATIVE — GC
managed không thu, chỉ `Resources.UnloadUnusedAssets()` mới thu, mà hàm đó không tự chạy giữa
trận. Chúng tích luỹ suốt ván; tới lúc Unity/iOS buộc phải dọn thì cú dọn khoá nhiều frame.

An toàn của A đã kiểm: toàn bộ màu LineRenderer đi qua `startColor`/`endColor` (vertex color,
thuộc renderer), KHÔNG chỗ nào trong project ghi `material.color` → dùng chung được.

CỐ Ý không sửa `Hud.cs:656,800` — chúng ở màn Pause và Kết thúc, gameplay đã dừng.

## Worked (with evidence)
- `dotnet build LaMuralla.Unity.csproj`: **0 errors** (137 warning CS8632 có sẵn từ trước)
- `dotnet test`: 210/210
- Chuỗi build lại: `[BuildScript] OK — 1020 MB, 16s` → `** BUILD SUCCEEDED **` → cài xong `com.tuantu.lamuralla`

## CHƯA xác minh
Launch tự động bị chặn vì máy KHOÁ MÀN HÌNH (`FBSOpenApplicationErrorDomain error 7: Locked`) —
không phải lỗi code. **Chưa ai xác nhận hết giật.** Ba lỗi trên là defect thật đã sửa, nhưng
việc chúng CÓ PHẢI nguyên nhân user gặp hay không thì phải user chơi lại mới biết.

## Nếu vẫn còn giật — nghi phạm kế tiếp (chưa đụng)
Churn GameObject không pool: mỗi phát bắn/nổ/đồng xu/thanh máu đều `new GameObject` + `Destroy`.
Đó là refactor lớn hơn (object pool), chỉ nên làm nếu 3 fix trên chưa đủ.

---

# VÒNG 24 — 2026-08-21: nền đa map + 3 map mới (đợt 1)

User duyệt toàn bộ đề xuất ở `docs/09-MAPS-REVIEW.md` §7 và bảo bắt đầu code.
Chi tiết đầy đủ: `docs/08-MAPS-ARCHITECTURE.md` §11.

## Đã làm

| Việc | Bằng chứng |
|------|-----------|
| **L1** buff phụ thuộc-chậm nhị phân → TỈ LỆ (phương án B) | `AbilityEngine.SlowRatio`; test `Khang_cham_lam_YEU_buff_chu_khong_phai_bat_tat` |
| `path` (một) → `lanes` (MẢNG) | `LaneDef`, `PathDef.Lanes` |
| `config/path.json` → `config/maps/m00-la-muralla.json` | m00 KHÔNG khai override → cân bằng không đổi |
| File map ghi đè waves/acts/hpScaling/economy/boss | `ConfigMapper.MapBosses`, `MapEconomy(o, map)` |
| `maxGapFraction` theo map | thay hằng số `MAX_GAP` cứng |
| `--map` cho `path_check.py` + `balance_sim.py` | `load_map()` |
| **R3** bỏ danh sách ô gõ cứng | `GreedyPlayer` chọn ô theo chord ĐO từ hình học |
| **Luật #21** trần chi tiêu theo map | `MapPlayableTests.Tien_ca_tran_khong_vuot_tran_chi_tieu` |
| Bài kiểm "chơi được không" cho MỖI map + trần 40 quân | `MapPlayableTests` |
| Bake MỌI map, chọn map lúc chạy | `ConfigBaker` validate từng map; `MatchView.MapId` |
| 3 map mới M01/M02/M06 | `config/maps/` |

## Số đo cuối (engine thật)

| Map | Ô | cash | Kết cục | Đỉnh quân | Biên trần |
|-----|--:|--:|---|--:|--:|
| m00 | 11 | 700 | Won · 11/20 | 27 | 3.9% |
| m01 | 11 | 700 | Won · 10/20 | 22 | 14.2% |
| m02 | 10 | 700 | Won · 10/20 | 25 | 10.4% |
| m06 |  9 | 600 | Won ·  8/20 | 32 | 10.2% |

`dotnet test` **219/219** · `path_check --map` 4/4 đạt · Unity 0 lỗi · `gen_docs --check` khớp.

## Ba thứ học được, đắt tiền

1. **`slowResistPercent` từng là VÁCH ở 100, không phải núm xoay.** Cả `nhan_quan` và
   `ban_thang_the_ky` kiểm `slow > 0` nên kháng 75% và 90% cho cùng hệ số 1.50×. Suýt cân
   10 map quanh một núm giả.
2. **Số ô KHÔNG phải núm độ khó** — nó là núm TRẦN KINH TẾ (`ô × 1020 + 476`). Ít ô buộc
   cắt số quái, tức map NHẸ đi. m06 (9 ô, ★★★★) ở growth 1.09 thắng dễ hơn cả m00
   (20/20 so với 11/20). Núm độ khó thật là MÁU QUÁI → m06 dùng growth 1.12 riêng.
3. **Ngưỡng phân loại ô phải có nghĩa.** `GreedyPlayer` bản đầu dùng `chord > 0` và THUA
   m00 ở W6; đổi sang `chord >= tầm` (đường quét được phải dài ít nhất bằng tầm tướng)
   thì tái tạo ĐÚNG danh sách gõ cứng cũ và đúng kết quả cũ.

## CHƯA làm (nói rõ, không giấu)

1. **Máu boss chưa đo lại sau L1.** `GreedyPlayer` dựng build D10S Lv2 + Pulga Lv3 —
   theo B-01 build đó KHÔNG có nguồn làm chậm nên không chạm buff synergy. Cần thêm
   fixture build synergy (Árbitro + D10S Lv3) mới đo được ảnh hưởng thật lên boss.
2. `spawns` vẫn là object, chưa thành mảng — đẩy sang đợt 2 để migrate một lần.
3. Đa tuyến (`LaneId`) chưa động — đợt 2, 7 map còn lại.
4. Màn chọn map chưa có — đổi map bằng `MatchView.MapId` trong Inspector.
5. `balance_sim.py` vẫn báo số không tin được (18–22 wave "ngoài dải" ở cả 4 map trong khi
   engine nói cả 4 đều thắng). Chạy được `--map` nhưng ĐỪNG dùng để chốt số.

## Việc tiếp
Bake config trong Unity (`La Muralla → Bake Config`) rồi chơi thử m01/m02/m06 trên máy
thật bằng cách đặt `MatchView.MapId`.

---

# VÒNG 25 — 2026-08-21: hết lag + màn chọn map + sao + mở khoá

## Lag: SAI CHẨN ĐOÁN 3 VÒNG — nguyên nhân thật là TRẦN 30 FPS

🔴 **Đọc mục này trước khi đụng vào hiệu năng.**

Số đo trên iPhone 12 lúc user báo giật: `avg 33.4ms · worst 33.6ms · alloc 4 KB/s · GC 51 · quân 16`.

`worst` chỉ hơn `avg` **0.2ms**. Nếu là thu gom rác thì `worst` phải vọt gấp 2–3 lần.
Độ lệch gần bằng KHÔNG là chữ ký của một TRẦN CỨNG — và 33.3ms đúng bằng 1/30 giây.

Nguyên nhân: Unity mặc định `Application.targetFrameRate = 30` trên iOS, và project
KHÔNG CHỖ NÀO đặt lại. Game chạy 30 FPS từ đầu tới giờ.

Sửa: `MatchView.UnlockFrameRate()` — `RuntimeInitializeOnLoadMethod`, đặt 60, áp cho cả
màn chọn map.

**Bài học:** đã sửa BẢY chỗ cấp phát mỗi frame qua hai vòng TRƯỚC KHI đo lấy một lần.
Cả bảy đều là defect thật (và `alloc 4 KB/s` chứng minh chúng có tác dụng) nhưng KHÔNG
cái nào là nguyên nhân. Lẽ ra phải dựng `PerfHud` ngay lần user báo giật ĐẦU TIÊN.
`PerfHud` giờ hiện luôn `target fps` để phân biệt "chạm trần" với "quá tải".

## Bảy chỗ cấp phát đã sửa (đúng, nhưng không phải nguyên nhân)

Tổng cộng 7 chỗ cấp phát/tra cứu mỗi frame đã sửa qua hai vòng:

| Vòng | Chỗ | Trước |
|---|---|---|
| 23 | `Draw.UnlitMaterial` | `new Material` mỗi lần gọi, gán qua `.material` → rò 2 material/sự kiện, KHÔNG BAO GIỜ huỷ (native, GC không thu) |
| 23 | `Hud` 4 chỗ trong `OnGUI` | `new GUIStyle`/`new GUIContent` mỗi frame |
| 23 | `MatchView.AnimCtrl` | `Resources.Load` mỗi lần spawn quái |
| 25 | `Hud.Centred` | `new GUIStyle` mỗi lần gọi — 12 cái/lượt OnGUI khi mở thanh mua |
| 25 | `Hud` mảng id tướng | `new[]{...}` mỗi lượt OnGUI |
| 25 | `Hud.DrawOwnedTower` | 4 chuỗi nội suy mỗi lượt OnGUI (đường "focus vào hero") |
| 25 | `MatchView` thanh máu + aura | `GetComponent<SpriteRenderer>()` mỗi quân mỗi frame (27 lần/frame ở W20) |

Thêm `PerfHud.cs` — bảng đo trên máy (avg/worst ms, KB rác/giây, số lần GC, số quân).
Dựng để KHỎI ĐOÁN lần thứ tư; cuối cùng không cần dùng vì lag đã hết, nhưng giữ lại.
`MatchView.ShowPerfHud` — ĐẶT VỀ false trước khi phát hành.

## Màn chọn map (G4 — trước đây hoãn)

| File | Việc |
|---|---|
| `MapProgress.cs` | Sao theo map, lưu `PlayerPrefs` khoá `lamuralla.progress.v1.*`. CHỈ ghi đè khi tốt hơn. Luật mở khoá: map sau mở khi map trước ≥1 sao |
| `MapSession` | Map đã chọn, chở qua lần nạp lại scene |
| `MapSelect.cs` | Lưới thẻ IMGUI: số thứ tự, tên, id, 3 sao. Thẻ khoá ghi "thắng X để mở" |
| `MatchView` | Rẽ nhánh ở `Start`: chưa chọn map → dựng menu rồi DỪNG. `NextMapId`, `GoToMap`, `BackToMenu`, ghi sao một lần khi trận kết thúc |
| `Hud` | Màn kết thúc: `RESTART` · `NEXT` · `MAPS`. BỎ `QUIT` (user yêu cầu) |

Sao dùng `StarRating` có sẵn của Core: 1–9 máu = 1 · 10–19 = 2 · đúng 20 = 3.

`MatchView.UnlockAllMaps = true` — user yêu cầu mở hết để test. ĐẶT VỀ false trước khi phát hành.

## Hai quyết định kỹ thuật

1. **Về menu bằng NẠP LẠI SCENE**, không bật/tắt object. Một trận dựng hàng trăm
   GameObject; tự dọn là chỗ chắc chắn rò rỉ. `MatchView.Restart()` vốn đã dùng cách này.
2. **Bắt chạm bằng `hotControl`**, không `MouseUp + Contains`. Bản đầu tôi viết cách
   ngây thơ; `Hud.Interact` đã có chú thích giải thích vì sao nó hỏng trên máy thật
   (nhấn chỗ khác nhả trúng thẻ vẫn tính là chọn → cuộn tay là vào nhầm map). Sửa trước khi build.

`dotnet test` 219/219 · Unity 0 lỗi · đã cài lên iPhone 12.

## Việc tiếp
- Test màn chọn map trên máy: thắng m00 → NEXT sang m01 → sao có được ghi không.
- Trước khi phát hành: `ShowPerfHud = false`, `UnlockAllMaps = false`, `TestExtraCash = 0` (đã là 0).
- Đợt 2 vẫn còn nguyên: 7 map đa tuyến, `LaneId`, `spawns` thành mảng.

---

# VÒNG 26 — 2026-08-21: 60 FPS · HOME · độ khó ngược

## 1. Lag = TRẦN 30 FPS, không phải rác (xem mục "SAI CHẨN ĐOÁN 3 VÒNG" ở trên)

## 2. Nút QUIT trong trận → HOME

`Hud` menu tạm dừng: `QUIT` (gọi `Application.Quit()`) → `HOME` (gọi `BackToMenu()`).
Xác nhận "RỜI TRẬN?" / "VỀ CHỌN MAP". Thoát hẳn app bằng nút trên điện thoại gần như
không bao giờ là thứ người chơi muốn.

## 3. 🔴 ĐỘ KHÓ NGƯỢC — lỗi ở khâu RA ĐỀ của agent

User báo: 4 map đầu độ khó GIẢM dần. Đo ra nguyên nhân:

| Map | Σchord@1.4 mỗi ô | so m00 | đoạn/ô |
|-----|---:|---:|---:|
| m00 | 1.74 | — | 1.00 |
| m01 | 1.79 | ×1.03 | 1.36 |
| m02 | **2.90** | **×1.67** | 1.20 |
| m06 | **3.03** | **×1.74** | 1.56 |

Chỉ số áp lực (máu W20 ÷ Σchord) TRƯỚC khi sửa: m00 2009 · m01 1739 · m02 **1081** · m06 1807.
m02 dễ hơn m00 gần HAI LẦN.

**Gốc:** tôi yêu cầu agent làm "hình học thú vị" (zigzag / túi combo / xoáy ốc) mà KHÔNG
nhận ra cả ba đều làm ĐƯỜNG TỰ ÁP SÁT CHÍNH NÓ → một vòng tầm cắt nhiều khúc rời nhau →
một tướng đánh nhiều lượt → tướng mạnh lên. Map càng "thú vị" càng DỄ.

**Chữa:** bù bằng `hpScaling.growthPerWave` riêng từng map (KHÔNG đảo thứ tự — thứ tự
hiện tại đúng mạch dạy dần). m01 1.09→1.12 · m02 1.09→1.12 · m06 1.12→1.13.
Thang đo lại: **11 → 10 → 7 → 2** máu còn lại, tăng dần đúng thứ tự. Đỉnh quân 27/29/31/33 < 40.

⚠️ **Giới hạn phải nhớ:** `GreedyPlayer` khai thác hình học KÉM HƠN người chơi thật, nên
nó là SÀN không phải trần. Không đẩy máu cao hơn được vì vượt mức đó thì `MapPlayableTests`
thất bại và mất luôn bằng chứng "map qua được". Nếu người chơi VẪN thấy dễ → núm tiếp theo
là SỬA ĐƯỜNG ĐI cho bớt tự áp sát, KHÔNG phải tăng máu.

## 4. Đợt 2 (7 map đa tuyến) — ĐỀ XUẤT LÀM RÀNG BUỘC TRƯỚC

Cả 7 map còn lại dính đúng lỗi trên và NẶNG HƠN: đa tuyến nên còn có ô canh NHIỀU TUYẾN
cùng lúc (đo ở vòng review: M09 `chord/2R` tb = 1.41, M10 = 1.20). Dựng đúng bản thiết kế
là tái tạo bài toán độ khó ngược thêm 7 lần.

→ Đề xuất: thêm **`Σchord/ô` thành ràng buộc thiết kế** mà `path_check` đo và ép (mỗi map
khai mức mục tiêu, lệch thì đỏ). Làm một lần rồi 7 map sinh ra đã đúng.
CHỜ USER CHỌN: (a) ràng buộc trước rồi code — hay (b) code thẳng rồi cân sau.

`dotnet test` 219/219 · path_check 4/4 · gen_docs khớp · đã cài lên iPhone.

---

# VÒNG 27 — 2026-08-22: KINH TẾ ĐA TUYẾN (đang làm dở)

## 1. 🔴 PHÁT HIỆN CHÍNH — 7/7 map đa tuyến KHÔNG thắng nổi ở số thiết kế

Đo bằng `GreedyPlayer` 20 wave, đúng số trong `config/maps/`:
4/4 map một tuyến THẮNG (m00 11 · m01 10 · m02 9 · m06 3 máu còn).
7/7 map đa tuyến THUA, và thua ở **act 1** (W4–W18).

**Nguyên nhân:** một khẩu chỉ bắn tuyến nó với tới → hai tuyến là HAI hàng thủ
phải nuôi cùng lúc, mà thu nhập không nhân đôi. `coveragePerSlot` (Σchord cộng qua
mọi tuyến) ĐẾM THỪA: tướng bắn một mục tiêu một lúc, canh hai tuyến làm nó BẬN
hơn chứ không MẠNH hơn. Chi tiết + số đo: `docs/09-MAPS-REVIEW.md` §8.

**Đã loại trừ ba nghi phạm trước khi kết luận** (docs/09 §8.4):
1. Bot mua ngu → viết lại cho biết trọng số tuyến; map một tuyến tốt lên, đa tuyến không thoát.
2. Thiếu lệch nhịp M07 → đã bù, xác minh tới engine, kết cục KHÔNG đổi một chữ.
3. Bot không đọc được HUD → đã dựng HUD + cho bot dùng; m05 W9→W18, m10 W4→W6, vẫn thua.

**Đã loại trừ cả "sửa lại đường đi"** (docs/09 §8.5): mật độ phủ của m10 đã tương
đương m00 (0.32–0.56 vs 0.452 chord/unit). Kéo dài tuyến mà không thêm ô thì Σchord
đứng yên; rải lại ô cho đều thì Σchord GIẢM. Núm là `startingCash`, không phải waypoint.

## 2. Đã sửa xong trong vòng này

| # | Việc | Bằng chứng |
|---|------|-----------|
| 1 | `gen_docs.py` HỎNG từ khi `spawns` thành mảng (docs không còn ràng với config) | `--check` xanh |
| 2 | Bảng số 10 map thành khối SINH TỰ ĐỘNG (`GEN:map_mXX`) + thang 11 map ở docs/08 §0 | `gen_docs --check` |
| 3 | M07 thiếu lệch nhịp 3 tuyến (thiết kế §5.1: 0/5.6/11.2 s) | `_laneDelayNote`, đo t=0.00/5.60/11.20 |
| 4 | HUD báo trước CỬA ra quân — điều kiện bắt buộc của map luân phiên (M03 §3) | `MatchController.LanesOfWave` + `LanePreviewTests` (4 test) |
| 5 | Chỉ m00 được validator kiểm; 10 map mới chỉ kiểm "JSON đọc được" | `Moi_map_deu_qua_validator` |
| 6 | `path_check`: thêm mục ĐỘ PHỦ THEO TỪNG TUYẾN (tôn trọng `farSlots`) | 11/11 map xanh |
| 7 | Màn chọn map: 11 thẻ TRÀN màn hình trên máy tỉ lệ ngắn (150u × 11 = 1848u) | cardH suy từ chỗ còn lại |
| 8 | Màn chọn map: `new GUIStyle` mỗi thẻ mỗi khung (1 320 rác/giây) | style dựng một lần |
| 9 | Nhãn `CỬA:` ban đầu dựng StringBuilder mỗi khung | dựng một lần mỗi wave |
| 10 | m00 gắn ★2 trong khi nó là map DỄ NHẤT (m01 ★1 mà khó hơn) | difficulty 2 → 1 |

## 3. CÒN DỞ — việc tiếp theo phải làm

1. **Chốt bộ số 11 map.** Vòng quét `ZTune` (11 map × cash 700–3100 × growth 1.02–1.14
   × k 1.0–0.6, mỗi điểm kiểm 3 hạt RNG + một mức tiền thấp hơn) đang chạy, log ở
   `scratchpad/tune5.log`. Kết quả các vòng trước cho thấy map đa tuyến cần
   `startingCash` **1 100–2 900** (thiết kế ghi 680–780) và `k` 0.6–0.9.
2. Chọn thang bằng `scratchpad/pick_ladder.py` (quy hoạch động, ép máu còn lại GIẢM
   DẦN theo thứ tự map), rồi ghi bằng `tools/apply_balance.py`.
3. Cập nhật sàn trong `MapPlayableTests.Thang` (11 dòng) — test `Thang_do_kho_giam_dan`
   đã viết sẵn, nó ép thứ tự độ khó bằng SỐ ĐO chứ không chỉ bằng sàn.
4. `dotnet test` toàn bộ · `path_check` 11/11 · `gen_docs --check`.
5. **XOÁ `core/LaMuralla.Core.Tests/ZTune.cs`** — bàn thí nghiệm, không phải test.
6. Bake + build iPhone.
7. Trước khi phát hành: `ShowPerfHud = false`, `UnlockAllMaps = false`.

## 4. ⚠️ Quyết định cần user biết

Để 7 map đa tuyến thắng được thì tiền khởi đầu tăng **1.5–3 lần** so với bản thiết kế.
Hệ quả: cảm giác đầu trận của map đa tuyến khác hẳn map một tuyến (mở màn 5–8 khẩu
thay vì 2). Đây là cùng thang thuốc đã dùng cho m00 ở vòng 22 (550→700, xem
`economy.json` → `_startingCashNote`), nhân với số tuyến — nhưng nó là thay đổi CẢM
GIÁC CHƠI, không chỉ là con số.


## 5. 🔴 LUẬT 10 — sàn `growthPerWave` là 1.069, không phải tuỳ ý

Bài kiểm mới `Moi_map_deu_qua_validator` bắt ngay bộ số đầu tiên tôi chọn: luật 10
đòi `hpMultiplier(cuối act) >= bountyMultiplier(act)`. Với `base = 0.67` và thưởng
act 1.0/1.6/2.2:

| mốc | growth tối thiểu |
|-----|-----------------|
| W7 · thưởng ×1.0 | 1.0690 |
| **W14 · thưởng ×1.6** | **1.0693** ← ràng buộc chặt nhất |
| W20 · thưởng ×2.2 | 1.0646 |

Mọi map PHẢI có `growthPerWave >= 1.070`. Bộ số đang nằm trong config vi phạm ở
m03 (1.06) · m05 (1.03) · m07 (1.02) · m09 (1.02) · m10 (1.03) — đang quét lại sáu
map trong dải hợp lệ (`ZW.cs`, log `/tmp/lamuralla-zw.log`).

⚠️ **Không được nới luật 10 để bộ số vừa.** Nó tồn tại để chặn "độ khó phẳng": nếu
máu quái không vượt sức mua thì cuối trận người chơi giàu hơn tốc độ map khó lên.

⚠️ Log quét để ở `/tmp/`, KHÔNG để trong scratchpad của phiên — scratchpad bị xoá
khi phiên khởi động lại và mất sạch dữ liệu quét một lần rồi.


## 6. ✅ CHỐT — bộ số 11 map (2026-08-22)

`dotnet test` **242/242 xanh** · `path_check` **11/11** · `gen_docs --check` khớp.

| # | map | máu còn | tiền đầu | growth | quân | ô sân |
|---|-----|--------:|---------:|-------:|-----:|------:|
| 0 | m00 | 11 | 700 | 1.090 | 465 | 11 |
| 1 | m01 | 10 | 700 | 1.120 | 429 | 11 |
| 2 | m02 | 9 | 700 | 1.120 | 400 | 10 |
| 3 | m03 | 8 | 2 300 | 1.090 | 431 | 12 |
| 4 | m04 | 8 | 2 100 | 1.070 | 446 | 12 |
| 5 | m05 | 8 | **700** | 1.070 | 452 | 11 |
| 6 | m06 | 8 | 900 | 1.130 | 246 | 9 |
| 7 | m07 | 8 | 2 100 | 1.070 | **640** | **20** |
| 8 | m08 | 8 | 2 700 | 1.090 | 287 | 10 |
| 9 | m09 | 8 | 1 100 | 1.090 | 428 | 11 |
| 10 | m10 | 7 | **4 100** | 1.080 | 457 | **21** |

Thang GIẢM DẦN đúng thứ tự map — `Thang_do_kho_giam_dan` ép bằng SỐ ĐO, không phải
bằng sàn. Đỉnh quân đồng thời cao nhất 39 (< trần 40). Mọi map dưới trần chi tiêu.

### Thay đổi thiết kế đã làm (không chỉ chỉnh số)
1. **m07 13→20 ô · m10 12→21 ô** (`tools/add_slots.py`). Lý do đo được: tuyến biên
   của chúng chỉ đạt 47–61% mức Σchord của m00 — một con quái đi tuyến đó ăn chưa
   tới nửa lượng sát thương. Với m07 việc này hạ tiền cần **3 300 → 2 100** VÀ cho
   TĂNG số quân (567→640) thay vì phải cắt 40%. Với m10 nó KHÔNG hạ được tiền
   (4 100 vẫn là 4 100) nhưng giữ được số quân.
2. **m10 mở tuyến chậm lại** — C mọi wave · O từ W6 · E từ W11 (thiết kế cũ: W4/W7).
3. **m07 khai `maxGapFraction = 0.21`** — luật khe hở tính theo TỈ LỆ chiều dài nên
   phạt map tuyến ngắn; khe hở thật của m07 là 4.3–5.4 units còn m00 được chấp nhận
   ở 6.16 units. Có `_gapNote` ghi rõ.

### Núm nào làm gì (đo được, không suy đoán)
- `startingCash` — núm của GIAI ĐOẠN ĐẦU, và là núm DUY NHẤT cứu được map đa tuyến.
- `growthPerWave` — núm của độ khó cuối. **Sàn cứng 1.070** (luật 10).
- số quân — núm của TRẦN KINH TẾ, KHÔNG phải núm độ khó.
- số Ô — núm của SÁT THƯƠNG MỖI CON QUÁI trên từng tuyến. Núm này bị bỏ quên suốt
  các vòng trước và nó chính là thứ m07 cần.


---

# VÒNG 27b — 2026-08-22: BỎ MAP BA TUYẾN, TIỀN VỀ MỨC BÌNH THƯỜNG

User báo 3 thứ: tiền khởi đầu quá nhiều · vài nút còn tiếng Việt · vẫn giật.
Đo ra CẢ BA CÙNG MỘT GỐC: map càng nhiều tuyến thì càng phải nuôi nhiều hàng thủ
(tiền cao) và quân chia nhiều dòng song song (đông quân + nhiều ô = giật).

User chọn: **tất cả tiếng Anh** · **một thân chung, hai lối ra** · **đổi cả 5 map**.

## Kết quả

| map | tuyến | ô | tiền trước → sau |
|-----|------:|--:|-----------------:|
| m03 | 2 | 12 | 2 300 → **1 200** |
| m04 | 2 | 13 | 2 100 → **800** |
| m05 | 2 | 11 | 700 → **600** |
| m07 | 3→**2** | 13 | 2 100 → **900** |
| m08 | 2 | 16 | 2 700 → **1 200** |
| m10 | 3→**2** | 13 | 4 100 → **900** |

Thang máu còn: 11·10·9·8·8·6·6·5·3·3·2 (giảm dần đúng thứ tự map).
Mọi map ≤ 16 ô, ≤ 465 quân, wave đông nhất 47 — **bằng đúng m00** (đã chạy 60 fps).
`dotnet test` 242/242 · `path_check` 11/11 · `gen_docs --check` khớp · đã cài iPhone 12.

## Công cụ mới
- `tools/make_fork_map.py` — dựng đường "một thân chung, hai lối ra".
  🔴 `THAN_MOC` lấy nhịp của m00. Bản đầu tôi tự bịa zigzag dày (bước dọc 1.5u) và
  đo ra MỘT ô canh 5.3u — gấp 3 lần m00, map dễ gấp ba mà nhìn không ra.
- `tools/add_slots.py` — rải ô, có `--cap` (trần chord mỗi ô), `--max-slots`,
  `--reset`, và luật "cứ ba ô một ô nằm ngoài tầm tướng ngắn nhất".

## Ba bộ canh đã bắt lỗi CỦA CHÍNH TÔI trong vòng này
1. **Đoạn hở tính bằng units** — m04/m08 hở 6.57u và 7.04u > mức 6.16u của m00.
   Tôi đã có thể khai nới `maxGapFraction` cho qua; bộ canh chặn đúng chỗ đó.
2. **Tầm bắn không còn là đánh đổi** — mọi ô đều trong tầm 1.0.
3. **Luật 10** (vòng 27) — growth < 1.069 làm độ khó phẳng.

## ⚠️ Bài học quy trình
Bộ dò nhân số quân TRONG BỘ NHỚ còn script ghi nhân TRÊN FILE → làm tròn từng nhóm
lệch vài con, và vài con đủ đổi kết cục (m03 đo 9, thực tế 12). **Luôn dò trên đúng
file đã ghi.** `ZW.cs` bản cuối làm đúng vậy (không có tham số `k`).

## Còn nợ
- `ConfigBaker.cs:66` gán `order = entries.Count` (thứ tự TÊN FILE), bỏ qua trường
  `order` trong JSON. Hôm nay trùng nhau do may mắn đặt tên m00…m10.
- Chưa đo FPS trên máy thật sau khi sửa — mới suy luận từ tải cảnh.
- Trước phát hành: `ShowPerfHud = false`, `UnlockAllMaps = false`.
