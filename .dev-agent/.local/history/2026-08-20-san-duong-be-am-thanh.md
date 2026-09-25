# TASK ARCHIVE — Nền sân, con đường, bệ đứng, âm thanh

**Ngày:** 2026-08-20
**Triage:** Complex (4 mảng, trong đó âm thanh là hệ thống chưa tồn tại)
**Kết quả:** PASS phần code (build 0 error) · còn Play-test thuộc về user

## Yêu cầu
User gửi link `assetstore.unity.com/top-assets/top-free` + "tìm hỗ trợ free để hoàn
thiện background, bệ đứng cho tướng, làm lại hình ảnh con đường, âm thanh".
Chốt: làm cả 4. User hỏi agent tự tải được không → tự tải được từ kenney.nl.

## Điều tra
- Fetch trang top-free: gần như toàn 3D (terrain texture, human animation). Game
  này 2D URP dọc → dùng được đúng 1 món (Dustyroom SFX, mà món đó cần đăng nhập).
- Tải 4 pack Kenney CC0 (5.1 MB): tower-defense-top-down, ui-audio,
  interface-sounds, impact-sounds. License.txt xác nhận CC0, ghi công không bắt buộc.
- **Loại pack sprite TD.** Mở tilesheet: toàn ô vuông trên lưới 90°. Đường của game
  là spline Catmull-Rom 14 waypoint, góc rẽ 70/77/9.6/-48/-86/-26/37/98/26/-42/-99/8.5°
  — không khớp khúc nào. Cộng lệch style với hero chibi vẽ tay.
- Hiện trạng đo được: nền = `_cam.backgroundColor` phẳng · đường = LineRenderer trắng
  alpha 30% · bệ = không có · âm thanh = **0 file, 0 AudioSource**.

## Việc đã làm

### Âm thanh (mới hoàn toàn)
- `Resources/Audio/` — 16 clip .ogg (172 KB) từ Kenney, đổi sang tên ngữ nghĩa;
  `CREDITS.txt` giữ bảng map ngược về tên gốc.
- `Unity/AudioService.cs` (mới) — pool 8 AudioSource (1 nguồn bị cắt tiếng khi có
  tiếng mới), chặn phát lại cùng key dày hơn 0.05s (El Cinco nhịp 0.6 ở ×2 là 0.3s,
  nhân 5 ô thì chồng thành tiếng rè), lệch cao độ ±6%. Thiếu file → warning ĐÚNG
  một lần rồi câm key đó (cache cả giá trị null).
- Móc vào 8 event core + 3 thao tác HUD. **Core không sửa một dòng nào.**
  ShotFired (theo tướng+cấp) · Splash · EnemyKilled · Threw · Saved · CardShown ·
  WaveStarted · WaveCleared · GoalDamaged · buy/upgrade/sell · bấm hụt → "denied".
- Nút tắt tiếng cạnh nút ×2, nhớ qua PlayerPrefs.

### Hình (sinh bằng code)
- `Unity/Pitch.cs` (mới):
  · `Field` — dải cỏ cắt ngang + vạch biên/giữa sân/vòng tròn giữa/2 vòng cấm/
    2 khu 5m50/chấm giữa. Vẽ theo HỘP THIẾT KẾ 10.8×19.2 quanh gốc, KHÔNG theo
    `_content` (bị khung thành kéo xuống y=-10.9 → vạch giữa sẽ lệch).
  · `Road` — dải lưới bám spline, texture lát theo quãng đường, mép mờ, vạch đứt giữa.
  · `Pedestal` — đĩa tròn có vành, đặt DƯỚI vòng chạm 48pt (không thay thế nó).
- `MatchView` — camera clear color đổi sang `Pitch.Stands` (khán đài tối);
  gom hằng số thứ tự vẽ; `PathLineWidth` 0.5 → 0.95 (0.5 là bề rộng VỆT, giờ là
  mặt đường thật, phải rộng hơn cổ động viên 0.7).
- `Draw.Grass` / `Draw.Path` gỡ (hết người gọi).

## Lỗi hình học BẮT ĐƯỢC TRƯỚC KHI CHẠY
Đo bán kính cong trên chính spline: nhỏ nhất **0.383 unit** quanh waypoint
(4.0, -6.55) — nhỏ hơn nửa bề rộng 0.475 ⇒ hai mép trong vượt qua nhau, dải lưới
thắt nút hình nơ.
Kẹp bề rộng theo bán kính cong TẠI CHỖ → **vẫn gấp** (đo lại: 1 đoạn gấp ngược).
Đỉnh cong chỉ rơi vào ĐÚNG một mẫu; hai mẫu kề đo ra 0.605 và 0.860 nên vẫn phình.
Sửa: lấy min bán kính trên **cửa sổ ±3 mẫu**. Đo lại: 0 đoạn gấp, 7/260 mẫu bị bóp,
chỗ hẹp nhất 0.76 unit (vẫn > 0.7 của cổ động viên). Cửa sổ ±2 cũng đủ; lấy 3 cho biên.

## Kiểm chứng
- `dotnet build LaMuralla.Unity.csproj`: **0 error**. 119 warning, tất cả là CS8632
  (`?` khi chưa bật `#nullable`) — cùng loại với 112 warning nền sẵn có của dự án,
  xuất hiện cả ở file không đụng tới (vd `Core/Match/EconomyService.cs:115`).
  Đóng góp mới: AudioService 5, Pitch 2.
- `core/`: 204/210 pass — **đúng bằng trước khi sửa**, 6 FAIL là nợ cũ về balance.
- Guardrail bản quyền trên 2 file mới + thư mục Audio: **0 hit**.
- Bản xem trước mặt sân (port thuật toán sang Python): tỉ lệ vạch vôi đọc được,
  đường không nuốt sân. `scratchpad/pitch_preview.png`.

## Blocked / cần user
- Play-test trong Unity: xác nhận màu cỏ/độ tương phản không nuốt quân, độ to
  từng tiếng, và vạch đứt trên đường có đọc ra hướng đi không.
- Muốn thêm tiếng còi trọng tài + tiếng đám đông thì phải tự tải (ZapSplat/Pixabay
  chặn bot / cần tài khoản). Thả file vào `Resources/Audio/` với tên `card.ogg`,
  `crowd.ogg` là tự dùng.

## Phát hiện bên lề — KHÔNG sửa
- **Lint bản quyền của chính dự án sẽ FAIL ngay bây giờ.** `docs/06 §5` nói grep
  trên `Assets/` phải trả về RỖNG, kể cả trong comment. Thực tế còn 4 hit, đều là
  nợ cũ trong comment: `Draw.cs:8` ("World Cup"), `MatchView.cs:49` ("Maradona"),
  `SpriteAnimBaker.cs:44` ("Messi"), `SpriteAnimBaker.cs:169` ("Maradona").
- 2 sheet art El Cinco + La Pulga từ vòng trước vẫn đang chờ.

## Không đụng tới
`core/**`, `config/*.json`, `Packages/manifest.json` (không thêm dependency nào).
