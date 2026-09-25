# La Muralla — Tower Defense World Cup

> **Working title:** La Muralla ("Bức tường")
> **Trạng thái:** 🟡 Define phase — chưa code
> **Nền tảng:** iOS (test trước) + Android
> **Engine:** Unity 6 LTS · 2D URP · C#

---

## Pitch

Cầu môn Argentina đang bị bao vây. Từng đợt cổ động viên Bồ Đào Nha tràn xuống sân, và cách duy nhất để chặn họ là dựng một hàng phòng ngự bằng chính những huyền thoại Argentina — mỗi người một vai, mỗi cấp mở thêm một kỹ năng mới. Hai lần trong trận, thủ lĩnh của họ — **`O Capitão`** — tự mình bước xuống sân.

Và có một trọng tài. Ông ấy hoàn toàn công tâm. Chỉ là túi quần hơi nặng.

Cổ động viên lọt vào cầu môn = mất máu. Hết máu = thua.

Tower Defense top-down 2D, một trận đủ 20 wave: ~11 phút ở tốc thường, ~7 phút khi bật ×2. Chơi được bằng một tay.

---

## Bộ tài liệu

Đọc theo thứ tự này. Mỗi doc tự đứng được.

> ### ⚠️ Nguồn chân lý số liệu = `config/*.json` — KHÔNG phải docs
>
> Mọi bảng số trong docs nằm giữa marker `<!-- GEN:x -->` và **được sinh tự động**. Sửa tay sẽ bị ghi đè.
>
> ```bash
> python3 tools/gen_docs.py           # sửa JSON xong thì chạy cái này
> python3 tools/gen_docs.py --check   # CI: đỏ nếu docs lệch JSON
> python3 tools/path_check.py --check # CI: hình học đường + ô (chord, ô chết, vùng chạm, đoạn hở)
> python3 tools/balance_sim.py        # bảng cân bằng: 20 wave, headroom, build tham chiếu
> python3 tools/balance_sim.py --solve # dò lại hpScaling sau MỌI thay đổi tầm/giá/thưởng
> ```
>
> **Đổi số liệu = sửa `config/*.json`, không bao giờ sửa docs.**
>
> Lý do: chỉ số tướng từng sống ở 3 chỗ, và cùng một lớp bug trùng lặp đã xảy ra **3/3 vòng review**. Xem `04` mở đầu.

| # | Doc | Nội dung | Trạng thái |
|---|-----|----------|-----------|
| 1 | [`docs/01-GAME-DESIGN.md`](docs/01-GAME-DESIGN.md) | Core loop, điều kiện thắng/thua, các phase trong 1 trận, điều khiển, phạm vi MVP | 🟢 Draft 1 |
| 2 | [`docs/02-TOWERS.md`](docs/02-TOWERS.md) | **6 đơn vị** phòng ngự (5 cầu thủ + trọng tài): chỉ số, 18 kỹ năng, cây nâng cấp 3 cấp | 🟢 Draft 3 |
| 3 | [`docs/03-ENEMIES-WAVES.md`](docs/03-ENEMIES-WAVES.md) | 3 loại cổ động viên, công thức scaling, bảng 20 wave | 🟢 Draft 1 |
| 4 | [`docs/04-ECONOMY-BALANCE.md`](docs/04-ECONOMY-BALANCE.md) | **Nguồn chân lý số liệu.** Kinh tế, dòng tiền, TTK/DPS math, kiểm chứng cân bằng | 🟢 Draft 1 |
| 5 | [`docs/05-TECH-ARCHITECTURE.md`](docs/05-TECH-ARCHITECTURE.md) | Unity 6, cây thư mục, module + trách nhiệm, schema JSON, quy trình build iOS | 🟢 Draft 1 |
| 6 | [`docs/06-ART-UX.md`](docs/06-ART-UX.md) | Art direction, palette, HUD, spec asset, guardrail bản quyền | 🟢 Draft 1 |
| 7 | [`docs/07-ROADMAP.md`](docs/07-ROADMAP.md) | Milestone M0 → M3, definition of done từng mốc | 🟢 Draft 1 |
| — | [`docs/OPEN-QUESTIONS.md`](docs/OPEN-QUESTIONS.md) | **Đọc cái này trước khi bàn tiếp.** Những gì còn phải chốt | 🔴 Cần bàn |

---

## Quyết định đã chốt

| # | Quyết định | Lý do | Ngày |
|---|-----------|-------|------|
| 1 | Unity 6 LTS + 2D URP, C# | Ecosystem Tower Defense 2D chín nhất; 1 codebase cho cả iOS + Android; sẵn IAP/Ads khi cần monetize | 2026-07-16 |
| 2 | Số liệu cân bằng data-driven (ScriptableObject seed từ JSON) | Số liệu sẽ đổi hàng chục lần trước khi ship — tune không cần rebuild | 2026-07-16 |
| 3 | Dùng biệt danh, không dùng tên thật cầu thủ | Quyền hình ảnh cầu thủ có chủ sở hữu; mục tiêu là phát hành lên store | 2026-07-16 |
| 4 | Top-down 2D, đặt tower theo ô lưới cố định | Tap chính xác trên màn 6"; pathing tĩnh, không cần re-path động | 2026-07-16 |
| 5 | **Không ăn theo World Cup 2026** | Giải kết thúc 19/07/2026; ship sớm nhất là tháng 9. Chủ đề bóng đá là vĩnh viễn, không gắn giải cụ thể. | 2026-07-16 |
| 6 | **Mỗi cấp mở 1 kỹ năng mới, cộng dồn** (18 kỹ năng) | Nâng cấp phải cảm thấy như được thêm. Kỹ năng cũ **không** tự mạnh lên — nếu không Lv3 mạnh 5–6× mà chỉ tốn 3.4× tiền. | 2026-07-16 |
| 7 | **Thêm boss `O Capitão`** (W10, W20) | 5 huyền thoại có tên vs 3 cổ động viên vô danh là bất đối xứng làm nhạt nửa game. Boss cũng là chốt chặn chống đội hình lệch. | 2026-07-16 |
| 8 | ⚠️ **Không bao giờ dùng "CR7"** — dùng `O Capitão` | CR7 là **nhãn hiệu đã đăng ký** (quần áo, khách sạn, nước hoa) — bảo hộ mạnh hơn cả tên riêng | 2026-07-16 |
| 9 | **Thêm `El Árbitro`** — tướng thứ 6, 0 sát thương, chậm vĩnh viễn bằng thẻ | Trọng tài **không phải cầu thủ** → không phá tiêu chí "5 cầu thủ". Nó nhân sức mạnh 10 ô còn lại thay vì cộng thêm DPS. | 2026-07-16 |
| 10 | Trọng tài **thiên vị** (trò đùa "bị mua") | User chốt, có cân nhắc đánh đổi — xem `06` §5 | 2026-07-16 |
| 11 | **Thẻ đỏ chỉ làm chậm 70%, KHÔNG xoá sổ ai** | Kỹ năng xoá sổ quân là thứ dễ gãy nhất trong TD | 2026-07-16 |
| 12 | Boss kháng chậm **50% → 75%** | Ở 50%, Trọng tài một mình xoá sổ bài kiểm tra căng nhất game (boss W20: 1.18× → 1.68×) | 2026-07-16 |

**Ranh giới đã bị đảo** (đừng khôi phục nhầm): "chỉ 3 loại cổ động viên, không boss" · "1 kỹ năng/tướng" · "5 tướng" là **luật đã chết**. Chi tiết: `.dev-agent/.local/STATE.md § Boundaries`.

---

## 🔴 Trạng thái cân bằng — đọc trước khi tin bảng số

`04` §4 **để trống W12–W20**. Đó là cố ý.

Mô hình giải tích đã vượt khả năng tính tay: ba hệ số ước lượng (`η × σ × τ`) nhân nhau (lệch tổng tới 73%), `τ` phụ thuộc số quân trong wave, và Reference Build đổi thành phần theo ngân sách từng wave. Điền số vào đó chỉ tạo ra **vẻ chính xác giả**.

**Chặn:** bảng tính mô phỏng ở M0 việc 2b (~nửa ngày). Cho tới lúc đó, mọi con số Act 3 ở `03` là **tạm tính**.

**W1–W11 vẫn có hiệu lực** — chưa có Trọng tài, build khớp ngân sách, chỉ 2 giả định.

Chi tiết lý do (ADR đầy đủ): `.dev-agent/.local/plans/FEATURE-td-worldcup-define.md`

---

## Ranh giới hiện tại (DO-NOT)

Phase này là **define**, không phải build. Cụ thể:

- ❌ Không viết code (C#, shader, script)
- ❌ Không tạo project Unity (`.unity`, `.meta`, `Packages/`)
- ❌ Không dùng tên thật cầu thủ ngoài bảng mapping nội bộ ở `docs/02`
- ❌ Không thêm dependency / package

Cấu trúc source được **mô tả** trong `docs/05`, chưa tạo. Khi nào docs đủ chín → mới scaffold.

---

## Con số nhanh

| Chỉ số | Giá trị |
|--------|---------|
| Tiền khởi đầu | 550 Peso ⚽ |
| Máu cầu môn | 20 bàn thua |
| Số đơn vị | **6** — 5 cầu thủ + 1 trọng tài (`El Árbitro`) |
| **Số kỹ năng** | **18** — mỗi cấp mở 1 cái mới, cộng dồn |
| Số loại cổ động viên | 3 thường + **1 boss** (`O Capitão`, W10 & W20) |
| Số ô đặt tower | 11 ô sân + 1 ô thủ môn |
| Số wave (MVP) | 20, chia 3 act. Máu = `0.69 × 1.09^(wave-1)`, **20/20 wave trong dải headroom** |
| Tổng quân 1 trận | 363 + 2 boss |
| Tổng tiền cả đời 1 trận | 11 276 Peso (trần chi 11 696 → biên khan hiếm **+3.6%**) |
| Thời lượng 1 trận | ~11 phút (1×) · ~7 phút (2×) |
