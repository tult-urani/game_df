# TASK ARCHIVE — D10S: declak lại sheet mới + apply animation

**Ngày:** 2026-07-31
**Triage:** Standard (art/asset, không đụng gameplay)
**Kết quả:** PASS (còn 1 bước Play-test thuộc về user)

## Yêu cầu
"tôi đã update lại images/d10.png hãy apply lại animation nhớ xóa backgroud và border cho các frame"

## Phân tích
Sheet mới vẫn 1152×928, lưới 5×4, cell 230.4×232, vai trò hàng KHÔNG đổi
(row0 idle · row1 ném bóng · row2 ném chai · row3 bắn cối) → mapping frame trong
`SpriteAnimBaker.BakeD10()` giữ nguyên, không sửa.
Nền vẫn là checker giả baked vào RGB, alpha đồng loạt 255.

## Việc đã làm
1. **`tools/declak.py`** (mới, lưu vào repo) — script xoá checker + đường kẻ lưới.
   3 pha: (a) flood từ mép từng ô trên mask checker, trừ protect mask
   `dilate(sat>30 | lum<130, 3)`; (b) `binary_propagation` lan từ nền đã xoá qua
   vùng `sat≤26 & lum≥165 & ~protect` — bắt halo xám ~226-232 lọt khe giữa hai tông
   checker 215/244 (không có bước này sót 839 mảnh / 9136 px); (c) xoá vô điều kiện
   mọi px `sat≤30` trong dải ±9px quanh mọi đường kẻ lưới — an toàn vì đã đo trước:
   0 px nhân vật (`sat>60`) nằm trong dải đó; bước này lấy nốt viền anti-alias
   lum≈163 mà cả (a) và (b) đều trượt.
   Kết quả: xoá 64.1% ảnh, còn 15 mảnh <500px (tổng 235 px).
2. **`Assets/_Project/Art/Characters/d10s_hero.png`** — ghi đè bằng bản sạch.
3. **`Assets/_Project/Resources/Art/d10s_bottle.png`** — crop lại. Chai đã đổi vị
   trí trong sheet mới (từ giữa ô sang góc phải-trên frame row2/col4). Bbox mới
   global (1085,550)-(1144,598), kích thước 59×48 (cũ 67×43).
4. **`Assets/_Project/Unity/MatchView.cs:432`** — `BottleBakedDeg` 150f → 25f.
   Đo lại: tâm vệt tốc độ (152,126) → tâm chai (192,109) trong toạ độ local ô
   ⇒ góc bay baked = atan2(+17, +40) ≈ +23° (đảo dấu y vì y ảnh hướng xuống).

## Kiểm chứng
- Composite lên nền hồng đặc, toàn sheet + zoom NEAREST 2× ô idle và ô ném chai:
  sạch, không viền, không đường kẻ, áo/quần/sọc không bị ăn.
- Đếm connected component <500px: 15 (trước khi thêm pha halo: 839).
- `.meta` chai đã sẵn `spriteMode:1, spritePixelsToUnits:232, alphaIsTransparency:1`
  → file mới không cần đổi meta.
- 20 slice trong `d10s_hero.png.meta` + `d10s.controller` + 4 `.anim` còn nguyên và
  vẫn khớp (canvas + lưới không đổi) ⇒ **không cần bake lại**.

## Blocked / cần user
- Bake headless bị chặn: Unity Editor của user đang mở, giữ `Temp/UnityLockfile`
  (PID 11237). Không tắt Editor của user. Không chặn gì vì vòng này không cần bake.
- Play-test: xác nhận slot D10S hết viền/nền, và góc bay chai Lv2 với 25f.
  Nếu chai nghiêng sai, báo lệch bao nhiêu độ để chỉnh hằng số.

## Không đụng tới
`config/towers.json`, `core/**` — balance/kỹ năng D10S giữ nguyên như vòng 2026-07-30.
