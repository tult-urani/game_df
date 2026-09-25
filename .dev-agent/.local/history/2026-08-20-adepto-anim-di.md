# TASK ARCHIVE — Adepto: animation đi bộ từ images/monster_lv1.png

**Ngày:** 2026-08-20
**Triage:** Standard (art + pipeline, không đụng gameplay)
**Kết quả:** PASS phần code · chờ user bấm Bake + Play-test · **1 cảnh báo bản quyền**

## Yêu cầu
"sử dụng images/monster_lv1.png để tạo animation cho quái 1, động tác đi bộ,
dùng 4 frame đầu, nhớ xóa background và viền đen đi"

## Đo trước khi làm
- Sheet **1408×768, lưới 4×2, ô 352×384** — KHÁC hẳn khuôn sheet tướng (1152×928, 5×4).
- alpha toàn 255 → checker baked vào RGB. Tông checker **122 / 171**
  (sheet tướng là 215/244) → hằng số trong `declak.py` không dùng lại được.
- Kẻ đen 2px tại x=351,352/703,704/1055,1056 và y=383,384.
- bbox dọc f0–f3 **giống hệt nhau** (y 48→374) ⇒ lặp không nhấp nhô. Hàng 2 lệch
  lên 26px ⇒ KHÔNG được trộn hai hàng vào một clip.
- Nhân vật cao 0.852 chiều cao ô.

## Việc đã làm
1. **`tools/declak.py`** — thêm `--cols --rows --checker LO HI --protect-dark`.
   Mặc định giữ nguyên hành vi cũ nên pipeline hero không đổi.
   `--protect-dark 100` là bắt buộc cho sheet này: mặc định 130 sẽ bảo vệ nhầm
   chính ô checker tối (lum 122) vì luật cũ coi "tối = nhân vật".
2. **`Art/Characters/adepto_enemy.png`** — bản đã bóc nền.
   Kiểm chứng: còn 28.6% px, **đúng 8 mảnh rời** (= 8 frame), **0 mảnh vụn <500px**.
3. **`Editor/SpriteAnimBaker.cs`** — `BakeAdepto()` + `BuildLoopController()`
   (một state lặp, không trigger — quái đi liên tục). Clip `adepto_walk` = frame
   0-3 @10fps (một vòng 0.4s; 12fps trông hớt hải, chậm hơn thì lết chân).
   Thêm vào `BakeAll`.
4. **`Unity/MatchView.cs`** — `MakeEnemyVisual()`: sprite + Animator nếu đã bake
   `Resources/Anim/<defId>.controller`, chưa có thì rơi về hình khối `Draw` (cùng
   lối rẽ với tướng). Cha LUÔN scale 1, mọi co giãn ở con "art".
   `FaceTravel()`: lật mặt theo **tiếp tuyến đường** + vùng chết 0.02.

## Hai lỗi tránh được nhờ đo trước
- **Thanh máu bị cỡ nhân vật kéo méo.** Cũ: cha mang `scale = cỡ quân (0.42)` nên
  `localScale/localPosition` của thanh máu bị nhân theo. Đổi cỡ quân là thanh máu
  đổi theo mà không ai chủ ý. Nay cha scale 1, thanh máu tính bằng đơn vị thế giới.
- **Rung lật ở khúc đường thẳng đứng.** Lấy hướng bằng hiệu vị trí hai khung thì ở
  đoạn gần thẳng đứng dx dao động quanh 0 → quân lật liên tục. Dùng tiếp tuyến đường.

## Số đã chốt (chưa hỏi được user nên tự quyết, ghi lại để còn sửa)
`EnemyArtScale = 0.88` → quân cao **0.75 unit**, boss ×1.8 = 1.35.
Lý do: phải nhỏ hơn tầm ngắn nhất (El Cinco Lv1 = 1.0) để "trong tầm/ngoài tầm"
còn đọc được (`docs/06 §4b`), và nhỏ hơn bề rộng đường 0.95 để nhìn như đi trên
đường. Cũ là 0.42 — quá nhỏ để thấy mặt.
`EnemyFootLift = 0.42` — bàn chân chạm đúng điểm trên đường.

## 🔴 CẢNH BÁO BẢN QUYỀN — chưa xử lý, chờ user quyết
Nhân vật mặc **áo đấu tuyển Bồ Đào Nha có phù hiệu liên đoàn ở ngực trái**
(hình khiên vàng/đỏ, ~20×26px trên sheet gốc). `docs/06 §5` cấm thẳng:
"Áo đấu chính thức (logo AFA, nhà tài trợ, hoạ tiết chính xác)".
- Trong game ở cỡ 0.75 unit thì phù hiệu chỉ còn ~2px, gần như không thấy.
- Nhưng file gốc nằm trong repo và ảnh chụp store ở độ phân giải cao thì thấy rõ.
- Ngoài ra `monster_lv1` là quái THƯỜNG (Adepto = "fan thường"), mà thiết kế để
  áo Bồ cho **boss** `O Capitão`. Fan thường mặc áo tuyển có phù hiệu là chỗ dễ bị soi nhất.
Cách sửa rẻ nhất: tô đè phù hiệu bằng chính màu đỏ của áo (1 lệnh, ~10 dòng script),
hoặc thay bằng một ký hiệu trung tính (★).

## Kiểm chứng
- `dotnet build` Unity + Editor: **0 error** (120 warning, đều là CS8632 nền sẵn có).
- `core/`: không đụng.
- Bản xem trước 4 frame trên nền cỏ + dải đường: chu kỳ đi khép kín, quân lọt
  gọn trong bề rộng đường. `scratchpad/adepto_walk.png`.

## Blocked / cần user
1. Unity menu **La Muralla → Bake Adepto Anim** (headless bị chặn, Editor đang mở).
2. Play-test: cỡ 0.75 unit có vừa mắt không, hướng lật có đúng không.
3. Quyết định về phù hiệu áo.
4. Còn `monster_lv2/3/4.png` chưa dùng — đoán là Tifoso / Tambor / O Capitão.
   Cần user xác nhận file nào ứng với con nào trước khi làm tiếp.

---

## Vòng sửa 2 (cùng ngày) — user báo 2 lỗi khi Play-test

**"nhân vật đi ngược rồi"** — tôi đọc sai hướng mặt của sheet ở vòng 1. Soi lại
frame 0 phóng to: **tai bên trái, mũi/mắt/miệng hướng sang PHẢI** ⇒ sheet quái
CÙNG quy ước với sheet tướng, không ngược như tôi ghi.
`MatchView.FaceTravel`: `flipX = dx > 0` → **`dx < 0`** (đi sang trái mới lật).
Đã sửa luôn bảng trong `docs/06` (ghi nhầm "sang TRÁI").

**"cho động tác chậm lại"** — `SpriteAnimBaker.BakeAdepto`: clip đi **10fps → 6fps**.
Một vòng 4 frame: 0.4s → **0.67s**. Ở tốc 1.0 unit/s thì sải chân ~0.67 unit/vòng.

Build lại: Unity + Editor **0 error**. Cần bake lại (fps nằm trong clip).

## Nợ mới phát hiện — CHƯA làm
`animator.speed` không ai gán. `docs/05 §4.4` chốt nó phải bằng MatchTimeScale.
Hệ quả: bật ×2 thì quân **đi nhanh gấp đôi mà chân vẫn khua 6fps** → trượt băng;
bị El Árbitro làm chậm thì ngược lại, chân khua nhanh hơn bước. Sửa đúng là cho
`animator.speed` tỉ lệ với `e.CurrentSpeed / e.BaseSpeed × SpeedMultiplier`.

---

## Vòng 3 — Tifoso Kèn (quái 2) từ images/monster_lv2.png

Yêu cầu: động tác chạy, 4 frame đầu, xóa nền + border.

**Sheet:** cùng khuôn 1408×768 lưới 4×2 như lv1, nhưng tông checker khác (đỉnh ở
105-119 và 165-179, lv1 là 120-134/165-179). Cửa sổ `--checker 105 185` phủ được cả hai.

**Nhân vật:** fan vác **cờ Bồ Đào Nha** (có quốc huy), chạy, có cụm bụi dưới chân.

**Việc đã làm**
1. `tools/declak.py` — thêm `--min-blob N`. Lần này bóc nền xong còn **29 mảnh vụn
   (854px)** vì sheet nhiều chi tiết rời. Phân bố kích thước có khe rõ: nhân vật
   ~52 000px · bụi 1 064-1 732px · rồi tụt thẳng xuống ≤134px ⇒ ngưỡng 500 cắt đúng
   khe, bỏ hết vụn mà giữ nguyên bụi. Sau lọc: **15 mảng = 8 nhân vật + 7 cụm bụi, 0 vụn.**
2. `Art/Characters/tifoso_enemy.png`.
3. `SpriteAnimBaker` — gộp `BakeAdepto` thành **`BakeEnemyWalk(id, fps)`** dùng chung;
   thêm `BakeTifoso` @8fps và cả hai vào `BakeAll`.
   **Luật fps:** tỉ lệ với `speed` trong `enemies.json`, gốc adepto 6fps @1.0
   ⇒ tifoso 1.4 → 8fps, tambor 0.6 → 4fps. Giữ vậy thì sải chân không đổi giữa các con.
4. `MatchView` — **KHÔNG cần sửa** để nhận con mới (nó đã tự nạp
   `Resources/Anim/<defId>.controller`). Nhưng phát hiện một lỗi khác, xem dưới.

**Lỗi bắt được khi đo art thật: thanh máu đè lên đầu quân.**
`BarLift` cũ để 0.62 — số đó có từ hồi quân là hình khối cao 0.42. Art thật cao
0.75 (adepto) đến 0.80 (tifoso, tính cả cờ) ⇒ thanh máu nằm trong người.
Sửa: suy thẳng từ hình học con đang dựng,
`(EnemyFootLift + EnemyArtTopUnits × EnemyArtScale) × (boss ? 1.8 : 1) + 0.10`,
chốt lúc TẠO và nhớ vào `_barGeom` vì con dùng sprite và con còn là hình khối cao
khác hẳn nhau. Kết quả: sprite thường 0.90 · boss 1.54 · hình khối 0.31/0.55
(hình khối giữ nguyên như cũ, không đổi cảm giác giữa chừng).

**Kiểm chứng:** Unity + Editor build **0 error**. Bản xem trước 4 frame chạy +
so cỡ cạnh adepto: cùng cỡ thân, chân chạm cùng một đáy, silhouette phân biệt rõ
(adepto tay không, tifoso vác cờ).

## 🔴 CẢNH BÁO BẢN QUYỀN — nặng hơn lv1
`docs/06 §5` có một dòng CẤM THẲNG, gọi đúng tên: **"Cờ quốc gia dùng làm định
danh đội → Màu sắc là đủ. Không cắm cờ."**
Con này vác nguyên **quốc kỳ Bồ Đào Nha kèm quốc huy**, chiếm ~1/3 khung hình và
là chi tiết dễ thấy nhất. Cộng thêm phù hiệu liên đoàn trên áo như lv1.
Đây không phải vùng xám — luật do chính dự án đặt ra đang bị vi phạm ở đúng chỗ nó
nêu ví dụ. Cần user quyết trước khi ship.

---

## Vòng 4 — Tambor Giáp (lv3) + O Capitão (lv4)

User chốt: lv3 = giáp (Tambor), lv4 = boss (O Capitão).

**lv3** — cùng khuôn 4×2 như lv1/lv2. Sau declak: 12 mảng = 8 nhân vật + 4 cụm bụi,
0 vụn (đã lọc 117 mảnh / 3 586px).

**lv4 — khác khuôn: lưới 4×1, ô 352×768** (nhân vật cao gấp đôi). Hai việc phát sinh:
1. Sheet vẫn có **kẻ ngang của lưới 4×2 vắt ngang ngực** ở y=384. `--rows 1` nên
   pha seam không biết tới nó → còn nguyên một vệt xám cắt ngang cả 4 frame.
2. Thêm `--hline Y` vào `declak.py`. **Làm sai một lần rồi mới đúng:**
   - Thử 1: chỉ xoá ở cột mà trên/dưới đều là nền → còn mưu kẻ thò ra sát thân.
   - Thử 2: lọc theo độ sáng (lum 90-200) trên mọi cột → **ăn mất phần trắng của
     phù hiệu trên ngực**. Sai.
   - Thử 3 (chốt): nở vùng-nền-theo-CỘT thêm 14 cột vào trong rồi mới xoá px
     `sat<=35`. Mưu kẻ nằm sát rìa nên bị bắt; phù hiệu ở giữa ngực cách rìa hàng
     trăm px nên không với tới. Sạch, phù hiệu nguyên vẹn.
   Đã ghi cảnh báo "đừng quay lại cách lọc theo độ sáng" vào `docs/06`.

**Code**
- `SpriteAnimBaker.BakeEnemyWalk(id, fps, rows = 2, ppu = EnemyPpu)` — tổng quát hoá.
  Thêm `BakeTambor` (4fps) và `BakeOCapitao` (3fps, rows 1, ppu 768). Cả 4 vào `BakeAll`.
  **Quy tắc chung: PPU = chiều cao Ô** ⇒ mọi frame cao đúng 1 unit bất kể ô to nhỏ.
- **Boss là ngoại lệ có tên của luật fps:** đúng luật là 0.4 × 6 = 2.4fps, nhưng
  dưới ~3fps thì 4 frame ra trình chiếu slide. Boss là set-piece, đọc được quan
  trọng hơn sải chân đúng vật lý → ép sàn 3fps.
- `MatchView` — bỏ hằng số `EnemyFootLift`/`EnemyArtTopUnits` dùng chung, thay bằng
  `FootFromCentre(e)` / `TopFromCentre(e)` đo riêng từng sheet:
  quái thường (ô 384) chân 0.474 · đỉnh 0.43 ; boss (ô 768) chân 0.441 · đỉnh 0.361.
  Dùng chung một con số thì **boss lơ lửng trên mặt đường 0.06 unit**.
  Ra: quái thường cao 0.80u (nâng chân 0.42, thanh máu 0.90);
      boss cao 1.27u (nâng chân 0.70, thanh máu 1.37). Boss to gấp **1.59×**
      quân thường — không phải đúng 1.8 như `docs/06 §4` ước, vì boss chiếm ít
      phần ô hơn. Đã sửa lại con số trong docs cho khớp thực tế.

**Kiểm chứng:** Unity + Editor build **0 error**. Thành phần liên thông:
tambor 12 (8 người + 4 bụi), boss 4 (đúng 4 người, không mảnh vụn).

## 🔴 CẢNH BÁO BẢN QUYỀN — lv4 là ca nặng nhất
`docs/06 §5` có bảng riêng cho `O Capitão` với hai dòng ❌ gọi đúng tên:
  ❌ "Mặt thật, đường nét khuôn mặt nhận ra được"
  ❌ "Áo tuyển Bồ chính thức, **số 7**"
và ở bảng chung: "**Số áo 7** trên boss — Boss KHÔNG mang số."
Art lv4 có **đúng cả hai**: số 7 to màu vàng chanh giữa ngực, phù hiệu liên đoàn,
áo tuyển Bồ, kiểu tóc + nét mặt theo nguyên mẫu thật. Cộng thêm vác cúp.

lv3 cũng mang **số 3** trên áo — luật chung là "không hiện số áo".

Ngoài ra lv3 (Tambor, `speed 0.6` — con CHẬM NHẤT) lại được vẽ **đang chạy nước rút
có vạch tốc độ**, và không có cái trống nào dù tên là "fan vác trống". Art và vai
trò thiết kế đang nói ngược nhau.

---

## Vòng 5 — user xuất lại lv4 (đã bỏ logo + số 7)

**Kiểm chứng yêu cầu của user:** soi lại sheet mới — số 7 và phù hiệu liên đoàn đã
sạch, áo còn đỏ-lục trơn. Đây đúng là **cột ✅** trong bảng `O Capitão` của
`docs/06 §5` ("Áo đỏ-lục không logo, không số") ⇒ hai vi phạm được gọi đúng tên
trong doc đã hết. (Nét mặt/kiểu tóc vẫn theo nguyên mẫu — đã nêu ở vòng 4, user tự quyết.)

**Nhưng chạy lại declak với tham số cũ thì hỏng nặng:**
83 742px bị bỏ dưới dạng vụn (vòng trước 21k), bbox f1/f3 vọt từ `107..723` lên `9..749`.

Nguyên nhân đo được: bản xuất mới có tông checker **~105 / ~155**, mà cửa sổ truyền
tay `--checker 105 185` bắt đầu đúng ở 105 ⇒ dải nền 100-104 rơi RA NGOÀI, không
được coi là nền, cũng không được bảo vệ, sống sót qua flood rồi bị `--min-blob` gom.
**Lệch 5 đơn vị độ sáng là đủ làm hỏng cả sheet.**

**Sửa gốc rễ — `--checker-auto`.** Lấy hai đỉnh lớn nhất của histogram độ sáng trên
px bão hoà thấp (nền chiếm 50-70% ảnh nên hai đỉnh đó luôn vượt xa mọi thứ khác),
đặt dải = [đỉnh_thấp−12, đỉnh_cao+12], và tự hạ `--protect-dark` xuống dưới dải đó.
Đây là lần thứ 3 phải dò tay trong một ngày — không dò nữa.

Chạy lại cả 4 con bằng auto, đỉnh tự dò ra **khác nhau ở cả 4**:
`122/172 · 118/168 · 112/162 · 108/158` — chứng minh dò tay vốn đã mong manh.

| Sheet | mảng | vụn bị bỏ | ghi chú |
|---|---|---|---|
| adepto | 8 | **0** | 8 nhân vật |
| tifoso | 16 | 580px | 8 nhân vật + 8 cụm bụi |
| tambor | 12 | 2 390px | 8 nhân vật + 4 cụm bụi |
| o_capitao | **4** | 334px | đúng 4 nhân vật, nhỏ nhất 100 026px |

bbox boss trở lại `107..723-727` ⇒ hằng số hình học trong `MatchView`
(chân 0.441 · đỉnh 0.361) vẫn đúng, không phải sửa.

`docs/06` đã đổi lệnh mẫu sang `--checker-auto` kèm cảnh báo vì sao đừng dò tay.

---

## Vòng 6 — user xuất lại lv3 (đã bỏ logo + số 3)

Soi lại sheet mới: **số 3 và phù hiệu đã sạch**, áo còn đỏ trơn viền lục — cùng
kiểu sửa như lv4. Chạy lại declak với `--checker-auto` (tự dò đỉnh 112/162):
**12 mảng = 8 nhân vật + 4 cụm bụi**, 2 322px vụn bị lọc, bbox f0-f3 đều
`(70-72 .. 370-371)` ⇒ đáy khớp nhau, lặp không nhấp nhô.

Hai số đo phụ, KHÔNG cần sửa code:
- chân tambor so với tâm ô = **0.466**, hằng số dùng chung là 0.474 → lệch 0.008
  unit (≈0.007 unit trên màn). Bỏ qua.
- đỉnh đầu tambor = 0.318, hằng số dùng chung 0.43 (lấy theo tifoso vác cờ) → thanh
  máu của tambor treo cao hơn cần ~0.1 unit. Chấp nhận: một độ cao thanh máu chung
  cho mọi quái đọc dễ hơn là mỗi con một kiểu.

**Còn tồn (art, không phải code):** lv1 vẫn có phù hiệu trên ngực; lv2 vẫn vác
quốc kỳ có quốc huy — `docs/06 §5` cấm thẳng "không cắm cờ". lv3 vẫn là tư thế
chạy nước rút có vạch tốc độ dù Tambor là con CHẬM NHẤT (speed 0.6) và tên nghĩa
là "fan vác trống" — user đã biết và chọn chỉ sửa phần IP.

## Nợ test — chưa ai quyết
`core/`: **204/210 pass, 6 FAIL** — y hệt lúc mở phiên, không phải hồi quy từ các
vòng art này (art chỉ đụng `Assets/`, `tools/`, `docs/`; `core/` và `config/` không
bị chạm — xác nhận bằng `git status`).
Danh sách: `SlotAndUpgradeTests.Chi_so_lay_dung_cap_khong_lech_mot_don_vi`,
`AbilityEngineTests` ×4, `MatchControllerTests.Nguoi_choi_biet_tieu_tien_thi_thang_duoc_20_wave`.
Gốc: `Sat_thuong_co_ban_lay_dung_cap` expect 50 nhận 20 — `config/towers.json` để
batigol dame 20/30/45 trong khi chính khối `_redesign` của nó nói 60/96/150.
Config lệch design-note lệch test. **Cần user chốt con số nào đúng** rồi mới sửa
được — không tự đoán vì nó là số cân bằng, sửa sai là hỏng cả bảng wave.
