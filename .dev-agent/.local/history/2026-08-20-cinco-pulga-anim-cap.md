# TASK ARCHIVE — El Cinco + La Pulga: animation theo cấp

**Ngày:** 2026-08-20
**Triage:** Standard (view/editor + art, không đụng gameplay)
**Kết quả:** PASS phần code · BLOCKED phần art (chờ 2 sheet mới từ user)

## Yêu cầu
"kiểm tra lại 2 tướng el Cinco và la pulga đã bị sai animation khi nâng level"
→ user chốt phương án **A: vẽ lại sheet** (giống quy trình D10S 2026-07-31).

## Chẩn đoán
Lớp code SẠCH ở cả 4 mắt xích — đã đối chiếu từng cái:
- `MatchView.cs:304` map cấp→trigger đúng thiết kế `towers.json`
- `batigol.controller:194,200,206` có đủ param Punch/Kick/Stomp
- `.anim` trỏ đúng frame: punch=5-8 · kick=10-13 · stomp=15-18
- alpha 20 ô đều `[0,0,0,0]` — nền sạch, không sót checker

Gốc rễ nằm ở ART:
1. **batigol_hero.png** — hàng 3 (f10-14) và hàng 4 (f15-19) KHÔNG phải đá/đạp.
   Cả hai là CÙNG một chu kỳ CHẠY lệch nửa pha (hai nắm đấm thủ trước ngực, chân
   guồng). Chỉ hàng 2 (f5-8) là đòn thật. ⇒ El Cinco Lv2/Lv3 chạy tại chỗ và
   giống hệt nhau. Đối chứng: d10s_hero.png hàng 3↔4 chênh 33-55 pixel/kênh,
   batigol chỉ 17-23.
2. **la_pulga** — chưa từng có animation theo cấp: `la_pulga.controller:64` chỉ 1
   param `Kick`, `BakeLaPulga` bake 1 clip, `MatchView.cs:303` dùng nhánh mặc định
   ⇒ Lv1/2/3 kích cùng một trigger. Không phải hồi quy, là hạng mục chưa làm.

## Việc đã làm (3 file)
1. **`Editor/SpriteAnimBaker.cs`** — `BakeLaPulga` theo đúng hợp đồng 4 hàng như
   d10s/dibu/batigol: kick=5-8 · curl=10-13 · volley=15-18, @12fps.
   `BuildKickController` (1 trigger) → `BuildPulgaController` (Kick/Curl/Volley).
   Gỡ helper `Slice()` vì hết người gọi.
2. **`Unity/MatchView.cs`** — thêm `CurlHash`/`VolleyHash` + nhánh `la_pulga` theo
   cấp. Thêm `FireTrigger(slotId, anim, hash, fallback)`: đọc bộ param CÓ THẬT của
   controller 1 lần rồi lọc, param thiếu thì rơi về fallback, thiếu cả hai thì im
   lặng. Dibu + trọng tài chuyển sang dùng chung. Bán tướng dọn `_animTriggers`.
   Sửa 4 khối chú thích khẳng định sai "SetTrigger là no-op an toàn".
3. **`docs/06-ART-UX.md` §4b** — mục mới "Bố cục sprite sheet hero — hợp đồng 4
   hàng": canvas/lưới/PPU/hướng mặt + bảng hàng↔cấp cho cả 5 tướng + ghi nợ art.

## Vì sao CHƯA bake lại
`FireTrigger` khiến thay đổi này **trung tính hoàn toàn** trước khi bake:
`la_pulga.controller` trên đĩa vẫn chỉ có `Kick` ⇒ Curl/Volley rơi về Kick ⇒ hành vi
y hệt hôm nay. Bake NGAY BÂY GIỜ với sheet cũ thì clip Lv1 đổi từ f9-14 sang f5-8 =
**xấu hơn** (f5-8 của sheet cũ chỉ là dáng chạy). Bake sau khi có art mới.

## Kiểm chứng
- `dotnet build LaMuralla.Unity.csproj` + `LaMuralla.Editor.csproj`: **0 error**.
  111 warning = nguyên trạng nền, MatchView.cs và SpriteAnimBaker.cs đóng góp **0**.
- `core/` không bị chạm (git diff xác nhận) — 204/210 test pass.

## Blocked / cần user
1. **2 sheet mới** 1152×928 lưới 5×4, alpha thật, mặt sang phải, theo bảng ở
   docs/06 §4b: `images/defender.png` (đấm/đá/đạp) và `images/pulga.png`
   (xút thường/xút xoáy/vô-lê).
2. Sau khi có: `python tools/declak.py <src> Assets/_Project/Art/Characters/<id>_hero.png`
   → Unity menu **La Muralla → Bake All Hero Anim** → Play-test 3 cấp.
   (Bake headless vẫn bị chặn: Editor của user đang mở, PID 62753 giữ UnityLockfile.)

## Phát hiện bên lề — KHÔNG sửa
- 6/210 test core FAIL sẵn từ trước, không liên quan animation:
  `Sat_thuong_co_ban_lay_dung_cap` expect 50 nhận 20. `towers.json` batigol ghi
  dame 20/30/45 nhưng chính khối `_redesign` của nó nói 60/96/150. Config lệch
  design note lệch test — cần một vòng chốt balance riêng.
- `docs/06-ART-UX.md` bảng "Nhịp animation" ghi Batigol `attackRate` 0.4, config
  hiện là 0.6 (đã đổi ở vòng 17). Doc cũ.
- `LaMuralla/Assets/_Recovery/0.unity` mới xuất hiện (untracked) — Unity tự sinh
  khi khôi phục scene chưa lưu, không do vòng này tạo.

## Không đụng tới
`config/*.json`, `core/**` — balance và kỹ năng giữ nguyên.
