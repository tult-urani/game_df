# TASK ARCHIVE — Gỡ nhãn ô · UI sang EN · Thiết kế lại thẻ chọn tướng

**Ngày:** 2026-08-20
**Triage:** Standard (3 việc trên 3 file view/editor, không đụng gameplay)
**Kết quả:** PASS phần code · chờ bake chân dung + Play-test

## Yêu cầu
"xóa các label f01,02,... đi. đổi các text sang EN. chỉnh sửa lại UI của ô chọn
tướng cho đẹp, có thể bổ sung hình cho đẹp"

## 1. Gỡ nhãn ô
- `MatchView` — bỏ `_slotLabels`, `SlotLabelDrop`, và dòng `Encapsulate` nới
  bounds xuống dưới ô để chừa chỗ cho nhãn.
- `Hud.DrawSlotLabels` → `DrawGoalLabel`: **giữ nhãn máu cầu môn** (nó là điều
  kiện thua, phải nằm chỗ mắt đang nhìn), chỉ bỏ vòng lặp id ô.
- ⚠️ Nhãn ô từng có lý do thật, ghi ở chính code cũ: "người chơi cần gọi tên ô để
  mô tả lỗi (ô 4 không bắn)". User yêu cầu gỡ → gỡ, nhưng để lại ghi chú trong
  `DrawGoalLabel` để lần sau cần thì bật lại chứ đừng dựng lại từ đầu.
- Hệ quả đo được: `ComputeContentBounds` mất 0.72 unit nới xuống dưới mỗi ô ⇒
  hộp co lại ~0.06 unit (vòng chạm 0.66 vẫn được encapsulate) ⇒ camera zoom vào
  một chút. Chấp nhận.

## 2. Text sang EN
`Hud.cs`: **0 chuỗi tiếng Việt còn lọt ra UI** (kiểm bằng script quét mọi string
literal ngoài comment).
START · WAVE n · Wave n/20 · saves · Sell · Skip · VICTORY / DEFEAT ·
"Goal HP left" · "Conceded on wave" · dmg / slow / cards / control / % save ·
rng · "out of range" · "need N 💰" · "can't place here".
**KHÔNG đổi:** 3 chuỗi `Debug.Log`/`LogError` trong `MatchView` — đó là log cho
lập trình viên, không phải UI người chơi.

## 3. Thẻ chọn tướng
- `SpriteAnimBaker.BakePortraits()` (mới) — cắt bbox frame 0 của 5 sheet tướng,
  đóng khung vuông + lề 12%, ghi `Resources/Art/portrait_<id>.png` (~20KB/file).
  HUD vẽ bằng IMGUI nên cần `Texture2D`, mà sheet gốc nằm ngoài `Resources/`.
  Bật `isReadable` tạm rồi TRẢ LẠI — để vĩnh viễn là nhân đôi bộ nhớ texture
  trong build để đổi lấy đúng một lần đọc.
- Thẻ mới: nền tối, dải màu nhận dạng ở đầu, **chân dung**, tên, giá, và dòng
  cuối = chỉ số *hoặc* lý do không mua được. Viền sáng khi rê.
  Chưa bake chân dung thì rơi về ô màu như bản cũ — vẫn chơi được.
- `BarHeight` 0.20 → **0.25** bề ngang màn: ở 0.20 thì chân dung chỉ còn ~70px
  và ba dòng chữ chồng nhau.
- `DrawOwnedTower` cũng dùng chân dung — cùng một tướng, cùng một mặt.

## Hai lỗi layout bắt được TRƯỚC khi mở Unity
Dựng mock đúng công thức layout bằng Python rồi soi:
1. **Dòng chỉ số và dòng "vì sao không mua được" đè nhau 25px** (199-235 vs
   210-246). Sửa: dòng cuối hiện MỘT trong hai — không mua được thì lý do quan
   trọng hơn chỉ số.
2. **Ký tự `◈`** tôi định dùng cho tiền bị **tofu** ở font Arial. Trả lại `💰` —
   đó là ký tự dự án đã dùng và đang hiển thị bình thường trên máy user.
3. Ô thủ môn chỉ mua được Dibu → chia đều thì thẻ rộng cả màn mà chân dung vẫn bé.
   Sửa: `bw = W / max(n, 3)`, canh giữa khi n < 3.

## Kiểm chứng
- `LaMuralla.Unity` + `LaMuralla.Editor`: **0 error** (126 warning — 120 nền +6
  CS8632 mới từ `Texture2D?`, cùng loại với ~112 warning sẵn có của dự án).
- Mock layout: đáy thẻ 280 / thanh 290 — không tràn.
- `core/`: 204/210 — **y hệt trước khi sửa**, không đụng `core/` hay `config/`.

## Blocked / cần user
1. Unity: **La Muralla → Bake Tower Portraits** (hoặc `Bake All Hero Anim` đã gọi
   sẵn nó). Chưa bake thì thẻ vẫn chạy nhưng là ô màu, không có mặt tướng.
2. Play-test: cỡ chữ trên thẻ ở máy thật, và thanh 0.25 có che nhiều quá không.
3. 6 test đỏ ở `core/` vẫn chờ user chốt số dame batigol (20/30/45 hay 60/96/150).

---

## Vòng 2 — thiết kế lại nút (START, nâng cấp, Sell, ×2, tắt tiếng)

**Gốc của cái xấu:** tất cả đang dùng `GUI.skin.button` mặc định của Unity — nút
xám bo góc kiểu 2010, và nó KHÔNG scale theo bề ngang màn nên đặt cạnh thẻ tướng
nền tối thì vừa nhạt vừa lạc lõng.

**Bộ nút mới `Hud.Button(rect, label, kind, u, enabled)`** — vẽ tay bằng đúng
`Fill`/`Outline` của thẻ tướng để cả HUD nói cùng một ngôn ngữ hình:
thân màu · mép dưới tối hơn làm **chân đế 5u** · vệt sáng 2u ở mép trên ·
viền đen 45% · rê vào thì trộn 14% trắng.
4 loại: `Primary` xanh lá (START) · `Upgrade` xanh dương (nâng cấp, ×2 khi bật) ·
`Danger` đỏ (Sell) · `Neutral` xám đen (×2 khi tắt, tắt tiếng).

Hit-test vẫn để `GUI.Button(GUIStyle.none)` lo — tự bắt sự kiện chuột là mở đường
cho lỗi "bấm xuyên qua hai nút chồng nhau".

**Căn chữ:** nhãn vẽ trong rect dịch lên `lip/2`, nên nó nằm giữa **THÂN** nút chứ
không giữa cả khối kể cả chân đế. Lệch 2-3px là nhìn ra ngay.

**`enabled = false`** → xám, luôn trả về false, và **bấm vào vẫn kêu `denied`** —
cùng bài học đã học ở thẻ mua tướng: im lặng thì người chơi kết luận nút hỏng chứ
không kết luận mình thiếu tiền. (Bản cũ dùng `GUI.enabled = false`, nuốt luôn cú bấm.)

## Tỉ lệ đã chỉnh — đều là số đo, không phải cảm tính
| | Cũ | Mới | Vì sao |
|---|---|---|---|
| Nút nâng/bán | `h − 32u` = **238×238** | `h × 0.52` = **238×140**, canh giữa | 238×238 nhìn ra ô gạch chứ không ra nút |
| Nút START | `48pt × 2.4` = **316px cao** | `× 1.6` = **211px** | 316px = 1/6 chiều cao màn, nhìn ra tấm biển. 48pt là SÀN của HIG, không phải cỡ nên dùng |
| Rect chữ tên tướng | `W × 0.45` cố định | `W − bw×2 − 50u − tx` | rect cũ thò xuống dưới nút nâng cấp; tên dài (El Árbitro) sẽ đè lên nút, mà chữ đè nút là chỗ bấm nhầm |

## Kiểm chứng (mock đúng công thức, trước khi mở Unity)
- rect chữ `211..555` · nút nâng bắt đầu ở `575` ⇒ **không đè**.
- chữ trong nút: `172px / 238px` và `165px / 238px` ⇒ còn ~33px lề mỗi bên.
- Nhãn căn giữa thân: công thức C# `r.y + r.height/2 − lip/2` = tâm thân
  `r.y + (r.height − lip)/2`. Khớp.
- Build Unity + Editor: **0 error**, 126 warning (nguyên trạng).
- `core/`: 204/210 — không đụng.

## Còn lại
`_small` (GUIStyle từ `GUI.skin.button`) đã gỡ hẳn, thay bằng `_btn` là label
căn giữa. Không còn chỗ nào trong HUD dùng skin mặc định của Unity.

---

## Vòng 3 — nút âm thanh + tốc độ: sang trái, gọn hơn, nền trong suốt

**Phát hiện trước khi sửa:** nút cũ `150×110px`. Trên 1080-design, `path.json → ui`
cho 1pt = 1080/393 = 2.748px ⇒ **110px = 40pt**, tức chiều cao đã **DƯỚI ngưỡng
chạm 48pt** mà chính config đặt ra. "Làm nhỏ hơn" theo nghĩa đen là làm tệ thêm.

**Cách làm:** tách VÙNG CHẠM khỏi PHẦN VẼ.
- vùng chạm: `(MinTouchTargetPt / DesignWidthPt) × Screen.width` = **132×132px = đúng 48pt**,
  lấy thẳng từ config nên đổi `minTouchTargetPt` là hai nút tự đi theo.
- phần vẽ: thụt vào `ChipInset = 19%` mỗi bên ⇒ **82×82px**.
Nhìn gọn hơn hẳn (82 so với 150×110) mà vùng bấm lại RỘNG HƠN và lần đầu tiên đạt chuẩn.

**`Hud.Chip(touch, label, active, u)`** (mới) — nền trong suốt:
- tắt: thân `black 28%` (rê vào 42%), viền `white 22%` (rê 38%), chữ `#DBE0EB`
- bật: thân `accent 34%`, viền `accent 92%`, chữ trắng — ×2 đang bật và "đã tắt
  tiếng" đều là trạng thái BẬT nên đều sáng xanh.
- cỡ chữ 30u (nút thường 34u).

**Vị trí:** góc **trái** dưới, `x = 16u` và `x = 16u + 132`. Tốc độ trước, âm thanh sau.

**Kiểm chứng:** mock trên nền cỏ sọc + một khúc đường nâu để thử độ đọc của nền
trong suốt — chữ và viền đọc được trên cả hai nền. (Mock lần đầu vẽ sai: PIL không
blend alpha trên ảnh RGBA nên nút hiện ra ĐẶC. Phải vẽ lên layer riêng rồi
`alpha_composite` mới thấy đúng. Code Unity không sai — `GUI.color` blend chuẩn.)

Build: **0 error**, 126 warning (nguyên trạng).

## Còn tồn — CHƯA làm, ngoài phạm vi yêu cầu
`DrawSpeed` chỉ được gọi ở phase `Fighting`. Nghĩa là **không tắt tiếng được lúc
đang chuẩn bị** (phase `Preparing`). Sửa là dời một lời gọi, nhưng đó là đổi LÚC
NÀO nút hiện — không nằm trong yêu cầu "dời sang trái, làm gọn, nền trong suốt".
