# TASK ARCHIVE — D10S ném theo cấp (bóng/chai/cối)

**Ngày:** 2026-07-30
**Triage:** Standard
**Kết quả:** PASS

## Yêu cầu
`images/d10.png` làm art cho D10S: Lv1 ném bóng, Lv2 ném chai rượu, Lv3 bắn súng cối.

## Quyết định chốt (user xác nhận "cứ theo khuyến nghị")
1. CHỈ gắn ART theo cấp lên cơ chế Splash sẵn có — KHÔNG đổi damage/radius/cooldown/balance 3 kỹ năng D10S trong `towers.json`.
2. Đạn (bóng/chai/cối) BAY từ tướng tới mục tiêu rồi mới nổ, không nổ dính liền tại chỗ.

## Việc đã làm
- Declak `images/d10.png` (checker giả baked cứng, alpha=255) → `LaMuralla/Assets/_Project/Art/Characters/d10s_hero.png`. Script + ghi chú kỹ thuật ở memory `la-muralla-art-pipeline.md` (mục "D10S 2026-07-30").
- Cắt sprite chai rượu từ frame 14 → `LaMuralla/Assets/_Project/Resources/Art/d10s_bottle.png` (PPU=232, không dùng ProjPpu 900).
- `SpriteAnimBaker.cs`: thêm `BakeD10()` + `BuildD10Controller` + `ConfigureProjectile` nhận tham số `ppu`, wire vào `BakeAll()`.
- `MatchView.cs`: thêm `BottleHash`/`MortarHash`, field `Shot.SplashRadius`, `TickShots` nổ khi đạn tới (thay vì impact thường) nếu `SplashRadius>0`, `OnShotFired` nhận diện tướng `d10s` để gọi `SpawnD10Thrown` thay vì nổ tức thời, method mới `SpawnD10Thrown` (Lv1 tái dùng cầu la_pulga, Lv2 sprite chai mới cắt, Lv3 viên đạn vẽ code — sheet không có frame đạn cối bay tách rời).
- Bake headless qua Unity CLI (`BakeAll`) — biên dịch sạch, sinh đủ `d10s.controller` + 4 `.anim`.

## Việc CHƯA làm / cần user
- Chưa test bằng mắt trong Unity Editor (Play mode) — không tự lái GUI được. User mở Unity, Play, kiểm animation Lv1/2/3 D10S và góc bay của chai (`BottleBakedDeg=150f`, đo tay, có thể lệch).
- Chưa cắt sprite viên đạn cối riêng (không có frame phù hợp trong sheet) — đang dùng hình tròn vẽ code tạm.

## Phát hiện ngoài phạm vi (KHÔNG sửa, chỉ ghi nhận)
6/219 core test fail, xác nhận KHÔNG liên quan tới thay đổi lần này (không đụng `core/` hay `towers.json`). Nguyên nhân: damage Batigol trong `towers.json` (20/30/45) lệch so với giá trị test đang assert (50/…). Chi tiết ở memory `la-muralla-core-test-debt.md`.

## Sanity check
Build sạch (0 lỗi compile), bake headless OK, `dotnet test` chạy được (213/219 pass, 6 fail pre-existing không liên quan).
