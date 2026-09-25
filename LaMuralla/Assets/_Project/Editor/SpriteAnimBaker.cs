using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace LaMuralla.EditorTools
{
    /// <summary>
    /// Biến sprite sheet lưới đều thành Animation Clip + Animator Controller bằng
    /// CODE — không thao tác GUI, chạy lại được nhiều lần (idempotent). Cùng lúc
    /// cấu hình import quả cầu lửa (pivot đặt ở lõi để xoay theo hướng bay).
    ///
    /// Triết lý giống `SceneBuilder`/`ConfigBaker`: asset sinh ra tái tạo được từ
    /// mã. Đổi art chỉ cần thay PNG rồi chạy lại menu.
    ///
    /// Gọi từ CLI:
    ///   Unity -projectPath . -batchmode -quit \
    ///         -executeMethod LaMuralla.EditorTools.SpriteAnimBaker.BakeAll
    /// </summary>
    public static class SpriteAnimBaker
    {
        private const string OutDir = "Assets/_Project/Resources/Anim";
        private const string CharDir = "Assets/_Project/Art/Characters";
        private const string ArtDir = "Assets/_Project/Resources/Art";

        // Cả hai sheet dùng chung khuôn: lưới 5×4 = 20 frame, canvas 1152×928 →
        // ô 230.4×232. Nền caro giả đã bóc trong suốt + xoá border trước khi import.
        // PPU = chiều cao ô (232) để mỗi frame cao đúng 1 unit; MatchView nhân SpriteArtScale.
        private const int Cols = 5;
        private const int Rows = 4;
        private const float HeroPpu = 232f;
        // Dải kỹ năng cuồng nộ Lv3 của El Cinco là 5 frame ngang riêng; canvas
        // cao hơn sheet hero gốc nên PPU được chốt theo chiều cao dải để art không
        // phình khi MatchView áp SpriteArtScale.
        private const float BatigolBerserkPpu = 724f;
        private const string BatigolBerserkPath =
            "Assets/_Project/Art/Characters/batigol_berserk_charge.png";

        [MenuItem("La Muralla/Bake All Hero Anim")]
        public static void BakeAll()
        {
            BakeLaPulga();
            BakeElArbitro();
            BakeDibu();
            BakeBatigol();
            BakeD10();
            BakeAdepto();
            BakeTifoso();
            BakeTambor();
            BakeOCapitao();
            BakePortraits();
        }

        // ── Chân dung tướng cho thẻ mua ở HUD ───────────────────────────────
        // HUD vẽ bằng IMGUI nên nó cần `Texture2D`, mà sheet gốc nằm ở
        // `Art/Characters/` — ngoài `Resources/` nên `Resources.Load` không thấy.
        // Cắt frame 0 (tư thế đứng) ra file riêng trong Resources là cách rẻ nhất:
        // mỗi file ~20KB, không phải kéo cả sheet 1.5MB vào build runtime.
        private static readonly string[] PortraitIds =
            { "batigol", "la_pulga", "d10s", "dibu", "el_arbitro" };

        [MenuItem("La Muralla/Bake Tower Portraits")]
        public static void BakePortraits()
        {
            EnsureFolder(ArtDir);
            foreach (string id in PortraitIds) BakePortrait(id);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void BakePortrait(string id)
        {
            string sheet = $"{CharDir}/{id}_hero.png";
            var importer = AssetImporter.GetAtPath(sheet) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[SpriteAnimBaker] Không thấy sheet {sheet}.");
                return;
            }

            // GetPixels đòi texture readable. Bật tạm rồi TRẢ LẠI: để readable vĩnh
            // viễn là nhân đôi bộ nhớ texture trong build, đổi lấy đúng một lần đọc.
            bool wasReadable = importer.isReadable;
            if (!wasReadable) { importer.isReadable = true; importer.SaveAndReimport(); }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(sheet);
            int portraitRows = id == "la_pulga" ? 3 : Rows;
            int cw = Mathf.RoundToInt(tex.width / (float)Cols);
            int ch = Mathf.RoundToInt(tex.height / (float)portraitRows);
            // Frame 0 = ô trái trên. Gốc toạ độ texture ở DƯỚI nên hàng trên = y cao.
            Color[] cell = tex.GetPixels(0, tex.height - ch, cw, ch);

            if (!wasReadable) { importer.isReadable = false; importer.SaveAndReimport(); }

            // Bbox thật của nhân vật: cắt sát người rồi mới đóng khung vuông, nếu
            // không thì chân dung toàn khoảng trống và nhân vật bé tí trong thẻ.
            int minX = cw, minY = ch, maxX = -1, maxY = -1;
            for (int y = 0; y < ch; y++)
                for (int x = 0; x < cw; x++)
                    if (cell[y * cw + x].a > 0.04f)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
            if (maxX < 0)
            {
                Debug.LogError($"[SpriteAnimBaker] {id}: frame 0 rỗng, không cắt được chân dung.");
                return;
            }

            int bw = maxX - minX + 1, bh = maxY - minY + 1;
            int side = Mathf.Max(bw, bh) + Mathf.RoundToInt(Mathf.Max(bw, bh) * 0.12f);   // chừa lề
            var outPx = new Color[side * side];                                            // mặc định trong suốt
            int ox = (side - bw) / 2, oy = (side - bh) / 2;
            for (int y = 0; y < bh; y++)
                for (int x = 0; x < bw; x++)
                    outPx[(oy + y) * side + ox + x] = cell[(minY + y) * cw + minX + x];

            var portrait = new Texture2D(side, side, TextureFormat.RGBA32, false);
            portrait.SetPixels(outPx);
            portrait.Apply();

            string path = $"{ArtDir}/portrait_{id}.png";
            File.WriteAllBytes(path, portrait.EncodeToPNG());
            Object.DestroyImmediate(portrait);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var pi = (TextureImporter)AssetImporter.GetAtPath(path);
            pi.textureType = TextureImporterType.Sprite;
            pi.spriteImportMode = SpriteImportMode.Single;
            pi.alphaIsTransparency = true;
            pi.mipmapEnabled = false;
            pi.SaveAndReimport();

            Debug.Log($"[SpriteAnimBaker] chân dung {id}: {side}×{side} → {path}");
        }

        // ── La Pulga: sheet riêng 5 cột × 3 hàng, mỗi hàng là một cấp. ──
        // PNG 1619×971 không chia hết cho 5×3; SliceGrid tính từng biên bằng
        // toạ độ làm tròn nên không mượn pixel từ frame kế bên.
        //   0-4 xút thường (Lv1) · 5-9 xút phạt (Lv2) · 10-14 vô-lê (Lv3)
        [MenuItem("La Muralla/Bake La Pulga Anim")]
        public static void BakeLaPulga()
        {
            const string id = "la_pulga";
            const int pulgaCols = 5;
            const int pulgaRows = 3;
            const float pulgaPpu = 324f;
            string sheet = $"{CharDir}/{id}_hero.png";
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(sheet) == null)
            {
                Debug.LogError($"[SpriteAnimBaker] Không thấy sheet {sheet}.");
                return;
            }
            // Frame 12 (frame thứ 3 của Lv3) có 24px cuối bên phải thuộc pose kế
            // tiếp ngay trong PNG nguồn. Cắt riêng phần dư, giữ tâm ô bằng pivot.
            Sprite[] frames = SliceGrid(sheet, id, pulgaCols, pulgaRows, pulgaPpu,
                                        trimRightFrame: 12, trimRightPixels: 24);
            if (frames.Length != pulgaCols * pulgaRows)
            {
                Debug.LogError($"[SpriteAnimBaker] {id}: slice ra {frames.Length} frame, cần {pulgaCols * pulgaRows}.");
                return;
            }

            EnsureFolder(OutDir);

            Sprite[] kick = Pick(frames, 0, 1, 2, 3, 4);
            Sprite[] curl = Pick(frames, 5, 6, 7, 8, 9);
            Sprite[] volley = Pick(frames, 10, 11, 12, 13, 14);
            SaveClip(BuildClip($"{id}_idle_lv1", new[] { kick[0] }, 1, true), $"{OutDir}/{id}_idle_lv1.anim");
            SaveClip(BuildClip($"{id}_idle_lv2", new[] { curl[0] }, 1, true), $"{OutDir}/{id}_idle_lv2.anim");
            SaveClip(BuildClip($"{id}_idle_lv3", new[] { volley[0] }, 1, true), $"{OutDir}/{id}_idle_lv3.anim");
            SaveClip(BuildClip($"{id}_kick", kick, 12, false), $"{OutDir}/{id}_kick.anim");
            SaveClip(BuildClip($"{id}_curl", curl, 12, false), $"{OutDir}/{id}_curl.anim");
            SaveClip(BuildClip($"{id}_volley", volley, 12, false), $"{OutDir}/{id}_volley.anim");
            BuildPulgaController($"{OutDir}/{id}.controller",
                                 $"{OutDir}/{id}_idle_lv1.anim", $"{OutDir}/{id}_idle_lv2.anim",
                                 $"{OutDir}/{id}_idle_lv3.anim",
                                 $"{OutDir}/{id}_kick.anim", $"{OutDir}/{id}_curl.anim",
                                 $"{OutDir}/{id}_volley.anim");

            ConfigureBall();
            ConfigureProjectile(PulgaNormalBallPath, PulgaBallPpu);
            // Bóng vàng có đuôi lửa vẽ hướng sang phải. Pivot đặt ở tâm bóng
            // để điểm chạm nằm ở quả bóng, phần đuôi chỉ kéo lại phía sau.
            ConfigureProjectile(PulgaGoldBallPath, PulgaGoldBallPpu,
                                new Vector2(0.74f, 0.5f));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SpriteAnimBaker] Xong {id}: idle theo cấp · xút=0-4 · phạt=5-9 · vô-lê=10-14 → {OutDir}/{id}.controller");
        }

        // ── El Árbitro (trọng tài) ──────────────────────────────────────────
        // Cấp 1 = đứng yên (aura, KHÔNG rút thẻ) → chỉ dùng state Idle.
        // Cấp 2 = rút thẻ VÀNG; Cấp 3 = rút thẻ ĐỎ. Vai trò frame (xem sheet):
        //   0-9   đứng/thủ  · 10-11 thẻ đỏ ở ngực · 12-14 thẻ vàng ở ngực
        //   15-17 giơ thẻ đỏ cao · 18-19 giơ thẻ vàng cao
        // Anim rút thẻ: từ đứng → đưa thẻ lên → GIƠ CAO (giữ frame đỉnh).
        [MenuItem("La Muralla/Bake El Arbitro Anim")]
        public static void BakeElArbitro()
        {
            const string id = "el_arbitro";
            Sprite[] frames = LoadFrames(id);
            if (frames == null) return;

            EnsureFolder(OutDir);

            Sprite[] yellow = Pick(frames, 0, 12, 19, 18, 18);
            Sprite[] red = Pick(frames, 0, 10, 16, 15, 15);
            SaveClip(BuildClip($"{id}_idle", new[] { frames[0] }, 1, true), $"{OutDir}/{id}_idle.anim");
            SaveClip(BuildClip($"{id}_yellow", yellow, 10, false), $"{OutDir}/{id}_yellow.anim");
            SaveClip(BuildClip($"{id}_red", red, 10, false), $"{OutDir}/{id}_red.anim");
            BuildCardController($"{OutDir}/{id}.controller", $"{OutDir}/{id}_idle.anim",
                               $"{OutDir}/{id}_yellow.anim", $"{OutDir}/{id}_red.anim");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SpriteAnimBaker] Xong {id}: idle=0 · vàng=0,12,19,18 · đỏ=0,10,16,15 → {OutDir}/{id}.controller");
        }

        // ── Dibu (thủ môn, áo xanh) ─────────────────────────────────────────
        // Mỗi CẤP một đòn ném riêng + một projectile riêng (user chốt):
        //   Lv1 `can_pha`            → NÉM BÓNG  (dùng lại cầu `la_pulga_ball`)
        //   Lv2 `ap_dao`             → NÉM GĂNG  (`Art/dibu_glove`)
        //   Lv3 `nguoi_hung_luan_luu`→ NÉM CÚP   (`Art/dibu_cup`)
        // Vai trò frame (xem sheet 5×4):
        //   0-4   đứng thủ (idle=0)              · 5-9   ôm/ném bóng
        //   10-14 vụt/đấm găng                    · 15-19 nâng cúp
        [MenuItem("La Muralla/Bake Dibu Anim")]
        public static void BakeDibu()
        {
            const string id = "dibu";
            Sprite[] frames = LoadFrames(id);
            if (frames == null) return;

            EnsureFolder(OutDir);

            Sprite[] ball = Pick(frames, 5, 6, 7, 8);
            Sprite[] glove = Pick(frames, 10, 12, 13, 14);
            Sprite[] cup = Pick(frames, 15, 16, 18, 19);
            SaveClip(BuildClip($"{id}_idle", new[] { frames[0] }, 1, true), $"{OutDir}/{id}_idle.anim");
            SaveClip(BuildClip($"{id}_ball", ball, 12, false), $"{OutDir}/{id}_ball.anim");
            SaveClip(BuildClip($"{id}_glove", glove, 12, false), $"{OutDir}/{id}_glove.anim");
            SaveClip(BuildClip($"{id}_cup", cup, 12, false), $"{OutDir}/{id}_cup.anim");
            BuildDibuController($"{OutDir}/{id}.controller", $"{OutDir}/{id}_idle.anim",
                                $"{OutDir}/{id}_ball.anim", $"{OutDir}/{id}_glove.anim",
                                $"{OutDir}/{id}_cup.anim");

            ConfigureProjectile(GlovePath);
            ConfigureProjectile(CupPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SpriteAnimBaker] Xong {id}: idle=0 · bóng=5-8 · găng=10,12,13,14 · cúp=15,16,18,19 → {OutDir}/{id}.controller");
        }

        // ── El Cinco (batigol — Paredes áo #5) ──────────────────────────────
        // Mỗi cấp một đòn NỔ (splash ở core): đấm / đá / húc cuồng nộ. Vai trò frame:
        //   0-4   thủ thế (idle=0)   · 5-9   đấm (streak@7)
        //   10-14 đá/chạy (streak@11) · Lv3 dùng dải 5 frame riêng:
        //   bùng nộ → hóa cuồng → lao → húc → chỉ còn vụ nổ.
        [MenuItem("La Muralla/Bake Batigol Anim")]
        public static void BakeBatigol()
        {
            const string id = "batigol";
            Sprite[] frames = LoadFrames(id);
            if (frames == null) return;
            Sprite[] berserk = SliceBatigolBerserkFrames();
            if (berserk.Length != 4)
            {
                Debug.LogError("[SpriteAnimBaker] El Cinco: dải cuồng nộ cần đúng 4 frame.");
                return;
            }

            EnsureFolder(OutDir);

            Sprite[] punch = Pick(frames, 5, 6, 7, 8);
            Sprite[] kick = Pick(frames, 10, 11, 12, 13);
            // Idle theo cấp lấy frame đầu của CHÍNH action cấp đó. Khi tấn công xong
            // sprite không giật về tư thế gốc không liên quan, nên mắt đọc chuỗi
            // đấm/đá/húc là một động tác liền mạch.
            SaveClip(BuildClip($"{id}_idle_lv1", new[] { punch[0] }, 1, true), $"{OutDir}/{id}_idle_lv1.anim");
            SaveClip(BuildClip($"{id}_idle_lv2", new[] { kick[0] }, 1, true), $"{OutDir}/{id}_idle_lv2.anim");
            SaveClip(BuildClip($"{id}_idle_lv3", new[] { berserk[0] }, 1, true), $"{OutDir}/{id}_idle_lv3.anim");
            SaveClip(BuildClip($"{id}_punch", punch, 12, false), $"{OutDir}/{id}_punch.anim");
            SaveClip(BuildClip($"{id}_kick", kick, 12, false), $"{OutDir}/{id}_kick.anim");
            SaveClip(BuildClip($"{id}_stomp", berserk, 12, false), $"{OutDir}/{id}_stomp.anim");
            BuildCincoController($"{OutDir}/{id}.controller", $"{OutDir}/{id}_idle_lv1.anim",
                                 $"{OutDir}/{id}_idle_lv2.anim", $"{OutDir}/{id}_idle_lv3.anim",
                                 $"{OutDir}/{id}_punch.anim", $"{OutDir}/{id}_kick.anim",
                                 $"{OutDir}/{id}_stomp.anim");

            ConfigureFireStrip();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SpriteAnimBaker] Xong {id} (El Cinco): idle theo frame đầu mỗi cấp · đấm=5-8 · đá=10-13 · húc cuồng nộ=4 frame → {OutDir}/{id}.controller");
        }

        // ── D10S (Maradona, áo Argentina) ────────────────────────────────────
        // Mỗi CẤP một đòn ném (user chốt): Lv1 ném BÓNG (tái dùng cầu la_pulga) ·
        // Lv2 ném CHAI RƯỢU (`Art/d10s_bottle`) · Lv3 BẮN CỐI (đạn vẽ code — sheet
        // không có frame viên đạn cối bay tách rời, chỉ có chớp lửa nòng ở frame
        // cuối). CHỈ đổi ART theo cấp — 3 kỹ năng + balance D10S trong
        // `towers.json` GIỮ NGUYÊN; đòn đánh vẫn lan (Splash) theo
        // `levels[].splashRadius` như trước, animation này là lớp ném phủ lên
        // trên (xem `MatchView.SpawnD10Thrown`). Vai trò frame (sheet 5×4):
        //   0-4 thủ thế (idle=0) · 5-8 ném bóng · 10-13 ném chai · 15,16,18,19 bắn cối
        [MenuItem("La Muralla/Bake D10S Anim")]
        public static void BakeD10()
        {
            const string id = "d10s";
            Sprite[] frames = LoadFrames(id);
            if (frames == null) return;

            EnsureFolder(OutDir);

            Sprite[] ball = Pick(frames, 5, 6, 7, 8);
            Sprite[] bottle = Pick(frames, 10, 11, 12, 13);
            Sprite[] mortar = Pick(frames, 15, 16, 18, 19);
            SaveClip(BuildClip($"{id}_idle", new[] { frames[0] }, 1, true), $"{OutDir}/{id}_idle.anim");
            SaveClip(BuildClip($"{id}_ball", ball, 12, false), $"{OutDir}/{id}_ball.anim");
            SaveClip(BuildClip($"{id}_bottle", bottle, 12, false), $"{OutDir}/{id}_bottle.anim");
            SaveClip(BuildClip($"{id}_mortar", mortar, 12, false), $"{OutDir}/{id}_mortar.anim");
            BuildD10Controller($"{OutDir}/{id}.controller", $"{OutDir}/{id}_idle.anim",
                               $"{OutDir}/{id}_ball.anim", $"{OutDir}/{id}_bottle.anim", $"{OutDir}/{id}_mortar.anim");

            // Chai bay: cắt từ chính sheet (PPU = HeroPpu để giữ đúng tỉ lệ đã vẽ
            // so với nhân vật, không dùng ProjPpu vốn tính cho ảnh cỡ khác).
            ConfigureProjectile(BottlePath, HeroPpu);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SpriteAnimBaker] Xong {id}: idle=0 · bóng=5-8 · chai=10-13 · cối=15,16,18,19 → {OutDir}/{id}.controller");
        }

        // ── Cổ động viên ────────────────────────────────────────────────────
        // Sheet quái KHÁC khuôn sheet tướng, đừng nhầm hai cái:
        //   tướng: 1152×928, lưới 5×4, ô 230.4×232, hàng = CẤP (idle + 3 đòn)
        //   quái : 1408×768, lưới 4×2, ô 352×384,   hàng 1 = chu kỳ ĐI/CHẠY
        // Quái không có cấp (`config/enemies.json` — máu lên theo công thức trơn
        // của wave, tạo hình giữ nguyên), nên nó chỉ cần ĐI và (sau này) NGÃ.
        //
        // Dùng 4 frame ĐẦU (hàng 1). Đo trước khi chọn: bbox dọc của f0-f3 chạm đáy
        // ở cùng một chỗ (y≈374 trên cả adepto lẫn tifoso) nên nhân vật KHÔNG nhấp
        // nhô khi lặp, và một `EnemyFootLift` duy nhất dùng được cho mọi con.
        // Hàng 2 lệch lên ~26px — trộn hai hàng vào một clip là quân giật lên xuống.
        // PPU = chiều cao Ô của sheet đó → mỗi frame cao đúng 1 unit, bất kể ô to nhỏ.
        // Boss dùng ô 352×768 (lưới 4×1, nhân vật cao gấp đôi) nên PPU của nó gấp đôi.
        private const float EnemyPpu = 384f;
        private const float BossPpu = 768f;

        // FPS TỈ LỆ VỚI TỐC ĐỘ của con đó (`config/enemies.json → speed`), lấy
        // adepto 6fps @ speed 1.0 làm gốc. Giữ tỉ lệ này thì SẢI CHÂN gần như không
        // đổi giữa các con — con nhanh khua chân nhanh hơn chứ không trượt băng.
        //   adepto 1.0 → 6fps · tifoso 1.4 → 8fps · tambor 0.6 → 4fps
        // Boss là NGOẠI LỆ CÓ TÊN: 0.4 × 6 = 2.4fps, mà dưới ~3fps thì 4 frame nhìn
        // ra trình chiếu slide chứ không ra bước đi. Boss là set-piece (`docs/03 §2`),
        // đọc được quan trọng hơn sải chân đúng vật lý → ép sàn 3fps.
        [MenuItem("La Muralla/Bake Adepto Anim")]
        public static void BakeAdepto() => BakeEnemyWalk("adepto", 6);

        [MenuItem("La Muralla/Bake Tifoso Anim")]
        public static void BakeTifoso() => BakeEnemyWalk("tifoso", 8);

        [MenuItem("La Muralla/Bake Tambor Anim")]
        public static void BakeTambor() => BakeEnemyWalk("tambor", 4);

        [MenuItem("La Muralla/Bake O Capitao Anim")]
        public static void BakeOCapitao() => BakeEnemyWalk("o_capitao", 3, rows: 1, ppu: BossPpu);

        private static void BakeEnemyWalk(string id, int fps, int rows = 2, float ppu = EnemyPpu)
        {
            string sheet = $"{CharDir}/{id}_enemy.png";
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(sheet) == null)
            {
                Debug.LogError($"[SpriteAnimBaker] Không thấy sheet {sheet}.");
                return;
            }

            EnsureFolder(OutDir);

            Sprite[] frames = SliceGrid(sheet, id, 4, rows, ppu);
            if (frames.Length != 4 * rows)
            {
                Debug.LogError($"[SpriteAnimBaker] {id}: slice ra {frames.Length} frame, cần {4 * rows}.");
                return;
            }

            SaveClip(BuildClip($"{id}_walk", Pick(frames, 0, 1, 2, 3), fps, true),
                     $"{OutDir}/{id}_walk.anim");
            BuildLoopController($"{OutDir}/{id}.controller", $"{OutDir}/{id}_walk.anim");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SpriteAnimBaker] Xong {id}: đi = frame 0-3 @{fps}fps, lặp → {OutDir}/{id}.controller");
        }

        // ── Nạp + slice sheet của một tướng ─────────────────────────────────
        private static Sprite[] LoadFrames(string id)
        {
            string sheet = $"{CharDir}/{id}_hero.png";
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(sheet) == null)
            {
                Debug.LogError($"[SpriteAnimBaker] Không thấy sheet {sheet}.");
                return null;
            }
            Sprite[] frames = SliceGrid(sheet, id, Cols, Rows, HeroPpu);
            if (frames.Length != Cols * Rows)
            {
                Debug.LogError($"[SpriteAnimBaker] {id}: slice ra {frames.Length} frame, cần {Cols * Rows}.");
                return null;
            }
            return frames;
        }

        private static Sprite[] Pick(Sprite[] frames, params int[] idx) =>
            idx.Select(i => frames[i]).ToArray();

        /// <summary>
        /// Dải cuồng nộ không phải lưới 5 ô đều: ImageGen để viền lửa/trail của
        /// từng pose có bề rộng khác nhau. Chia W/5 đã khiến frame trước ăn sang
        /// frame sau. Frame 4 dùng vùng độc lập (không sát frame 3) để đủ cả đầu;
        /// frame 5 bị bỏ vì MatchView đã có FX nổ tại điểm va chạm.
        /// </summary>
        private static Sprite[] SliceBatigolBerserkFrames()
        {
            const int expectedWidth = 2172;
            Rect[] regions =
            {
                new(0, 0, 340, 724),
                new(340, 0, 350, 724),
                new(690, 0, 410, 724),
                new(1160, 0, 500, 724),
            };
            var importer = (TextureImporter)AssetImporter.GetAtPath(BatigolBerserkPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = BatigolBerserkPpu;
            // Strip gốc rộng 2172px. Mặc định 2048px làm Unity resize trước khi
            // chia, khiến mọi mốc pixel bên dưới lệch sang frame kế tiếp.
            importer.maxTextureSize = 4096;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(BatigolBerserkPath);
            if (tex.width != expectedWidth)
            {
                Debug.LogError($"[SpriteAnimBaker] El Cinco: strip cuồng nộ rộng {tex.width}px, cần {expectedWidth}px.");
                return System.Array.Empty<Sprite>();
            }

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider dp = factory.GetSpriteEditorDataProviderFromObject(importer);
            dp.InitSpriteEditorDataProvider();
            var rects = new SpriteRect[regions.Length];
            var pairs = new List<SpriteNameFileIdPair>(rects.Length);
            for (int i = 0; i < rects.Length; i++)
            {
                var sr = new SpriteRect
                {
                    name = $"batigol_berserk_{i}",
                    spriteID = GUID.Generate(),
                    rect = regions[i],
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                };
                rects[i] = sr;
                pairs.Add(new SpriteNameFileIdPair(sr.name, sr.spriteID));
            }
            dp.SetSpriteRects(rects);
            var nameIdDp = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameIdDp?.SetNameFileIdPairs(pairs);
            dp.Apply();
            importer.SaveAndReimport();

            return AssetDatabase.LoadAllAssetsAtPath(BatigolBerserkPath)
                .OfType<Sprite>()
                .OrderBy(s => FrameIndex(s.name))
                .ToArray();
        }

        // ── Slice lưới đều bằng ISpriteEditorDataProvider (API Unity 6) ──────
        private static Sprite[] SliceGrid(string pngPath, string id, int cols, int rows, float ppu,
                                          int trimRightFrame = -1, int trimRightPixels = 0)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
            int W = tex.width, H = tex.height;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider dp = factory.GetSpriteEditorDataProviderFromObject(importer);
            dp.InitSpriteEditorDataProvider();

            var rects = new SpriteRect[cols * rows];
            var pairs = new List<SpriteNameFileIdPair>(cols * rows);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    int idx = r * cols + c;
                    int x0 = Mathf.RoundToInt(c * W / (float)cols);
                    int x1 = Mathf.RoundToInt((c + 1) * W / (float)cols);
                    // Hàng đọc trên→xuống, nhưng gốc toạ độ texture ở DƯỚI → lật.
                    int y0 = Mathf.RoundToInt((rows - 1 - r) * H / (float)rows);
                    int y1 = Mathf.RoundToInt((rows - r) * H / (float)rows);
                    float pivotX = 0.5f;
                    if (idx == trimRightFrame && trimRightPixels > 0)
                    {
                        int fullWidth = x1 - x0;
                        int trimmedWidth = fullWidth - trimRightPixels;
                        if (trimmedWidth <= 0)
                        {
                            Debug.LogError($"[SpriteAnimBaker] {id}_{idx}: trim {trimRightPixels}px >= rộng {fullWidth}px.");
                            return System.Array.Empty<Sprite>();
                        }
                        // Pivot mới vẫn trỏ vào tâm ô gốc, nên animation không
                        // dịch sang trái sau khi bỏ phần nhiễu ở mép phải.
                        pivotX = (fullWidth * 0.5f) / trimmedWidth;
                        x1 -= trimRightPixels;
                    }
                    var sr = new SpriteRect
                    {
                        name = $"{id}_{idx}",
                        spriteID = GUID.Generate(),
                        rect = new Rect(x0, y0, x1 - x0, y1 - y0),
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(pivotX, 0.5f),
                    };
                    rects[idx] = sr;
                    pairs.Add(new SpriteNameFileIdPair(sr.name, sr.spriteID));
                }

            dp.SetSpriteRects(rects);
            var nameIdDp = dp.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameIdDp?.SetNameFileIdPairs(pairs);
            dp.Apply();
            importer.SaveAndReimport();

            return AssetDatabase.LoadAllAssetsAtPath(pngPath)
                .OfType<Sprite>()
                .OrderBy(s => FrameIndex(s.name))
                .ToArray();
        }

        private static int FrameIndex(string name)
        {
            int u = name.LastIndexOf('_');
            return u >= 0 && int.TryParse(name.Substring(u + 1), out int i) ? i : 0;
        }

        // ── Quả cầu lửa (ảnh đơn) ───────────────────────────────────────────
        private const string BallPath = "Assets/_Project/Resources/Art/la_pulga_ball.png";
        private const string PulgaNormalBallPath = "Assets/_Project/Resources/Art/pulga_ball_normal.png";
        private const string PulgaGoldBallPath = "Assets/_Project/Resources/Art/pulga_ball_gold.png";
        private const float BallPpu = 768f;
        private const float PulgaBallPpu = 128f;
        private const float PulgaGoldBallPpu = 900f;
        private const float BallPivotX = 0.771f;   // tâm lõi lửa (đo từ pixel)
        private const float BallPivotY = 0.492f;

        /// <summary>Cấu hình import quả cầu: ảnh đơn, pivot đặt ở LÕI để khi xoay
        /// theo hướng bay thì lõi dẫn đầu, đuôi lửa kéo sau.</summary>
        private static void ConfigureBall()
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(BallPath) == null)
            {
                Debug.LogWarning($"[SpriteAnimBaker] Không thấy {BallPath} — bỏ qua cầu lửa.");
                return;
            }
            var imp = (TextureImporter)AssetImporter.GetAtPath(BallPath);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = BallPpu;
            imp.filterMode = FilterMode.Bilinear;
            imp.mipmapEnabled = false;

            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.spriteAlignment = (int)SpriteAlignment.Custom;
            s.spritePivot = new Vector2(BallPivotX, BallPivotY);
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();
        }

        // ── Projectile Dibu (găng / cúp — ảnh đơn) ──────────────────────────
        // Vệt tốc độ đã vẽ sẵn trong ảnh nên KHÔNG xoay theo hướng bay (kẻo
        // găng/cúp bị lộn ngược); pivot ở TÂM, MatchView bay thẳng from→to.
        private const string GlovePath = "Assets/_Project/Resources/Art/dibu_glove.png";
        private const string CupPath = "Assets/_Project/Resources/Art/dibu_cup.png";
        private const string BottlePath = "Assets/_Project/Resources/Art/d10s_bottle.png";
        private const float ProjPpu = 900f;

        private static void ConfigureProjectile(string path, float ppu = ProjPpu,
                                                Vector2? customPivot = null)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            {
                Debug.LogWarning($"[SpriteAnimBaker] Không thấy {path} — bỏ qua projectile.");
                return;
            }
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = ppu;
            imp.filterMode = FilterMode.Bilinear;
            imp.mipmapEnabled = false;

            var s = new TextureImporterSettings();
            imp.ReadTextureSettings(s);
            s.spriteAlignment = customPivot.HasValue
                ? (int)SpriteAlignment.Custom
                : (int)SpriteAlignment.Center;
            if (customPivot.HasValue) s.spritePivot = customPivot.Value;
            imp.SetTextureSettings(s);
            imp.SaveAndReimport();
        }

        // ── Strip vụ nổ lửa (fire.png — 1×4, ảnh nổ nhỏ→lớn) ────────────────
        // Slice thành 4 sprite con để MatchView chạy hoạt ảnh vụ nổ El Cinco.
        private const string FirePath = "Assets/_Project/Resources/Art/fire.png";
        private const float FirePpu = 512f;

        private static void ConfigureFireStrip()
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(FirePath) == null)
            {
                Debug.LogWarning($"[SpriteAnimBaker] Không thấy {FirePath} — bỏ qua fire.");
                return;
            }
            SliceGrid(FirePath, "fire", 4, 1, FirePpu);   // 4 cột × 1 hàng → fire_0..fire_3
        }

        // ── AnimationClip từ chuỗi sprite ───────────────────────────────────
        private static AnimationClip BuildClip(string name, Sprite[] sprites, int fps, bool loop)
        {
            var clip = new AnimationClip { frameRate = fps, name = name };
            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite",
            };
            var keys = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = sprites[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clip);
            s.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, s);
            return clip;
        }

        private static void SaveClip(AnimationClip clip, string path)
        {
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
                Object.DestroyImmediate(clip);
            }
            else
            {
                AssetDatabase.CreateAsset(clip, path);
            }
        }

        // ── Controller La Pulga: mỗi cấp dùng frame đầu action làm idle.
        // MatchView đặt Level khi mua/nâng cấp, cú xút xong trở về đúng idle. ──
        private static void BuildPulgaController(string path, string idleLv1Path,
                                                 string idleLv2Path, string idleLv3Path,
                                                 string kickPath, string curlPath, string volleyPath)
        {
            AssetDatabase.DeleteAsset(path);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
            ac.AddParameter("Kick", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Curl", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Volley", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Level", AnimatorControllerParameterType.Int);

            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            AnimatorState idleLv1 = sm.AddState("Idle Lv1");
            idleLv1.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleLv1Path);
            AnimatorState idleLv2 = sm.AddState("Idle Lv2");
            idleLv2.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleLv2Path);
            AnimatorState idleLv3 = sm.AddState("Idle Lv3");
            idleLv3.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleLv3Path);
            AnimatorState kick = sm.AddState("Kick");
            kick.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(kickPath);
            AnimatorState curl = sm.AddState("Curl");
            curl.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(curlPath);
            AnimatorState volley = sm.AddState("Volley");
            volley.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(volleyPath);
            sm.defaultState = idleLv1;

            AnimatorState[] idles = { idleLv1, idleLv2, idleLv3 };
            for (int i = 0; i < idles.Length; i++)
            {
                AddTrigger(idles[i], kick, "Kick");
                AddTrigger(idles[i], curl, "Curl");
                AddTrigger(idles[i], volley, "Volley");
                for (int j = 0; j < idles.Length; j++)
                    if (i != j) AddLevelTransition(idles[i], idles[j], j + 1, false);
            }
            foreach (AnimatorState attack in new[] { kick, curl, volley })
                for (int level = 1; level <= idles.Length; level++)
                    AddLevelTransition(attack, idles[level - 1], level, true);
        }

        // ── Controller kiểu RÚT THẺ: Idle mặc định; trigger Yellow/Red → giơ
        // thẻ → về Idle. Cấp 1 không kích trigger nào nên đứng yên ở Idle. ──
        private static void BuildCardController(string path, string idlePath,
                                                string yellowPath, string redPath)
        {
            AssetDatabase.DeleteAsset(path);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
            ac.AddParameter("Yellow", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Red", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            AnimatorState idle = sm.AddState("Idle");
            idle.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath);
            AnimatorState yellow = sm.AddState("Yellow");
            yellow.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(yellowPath);
            AnimatorState red = sm.AddState("Red");
            red.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(redPath);
            sm.defaultState = idle;

            AddTrigger(idle, yellow, "Yellow");
            AddTrigger(idle, red, "Red");
            AddReturn(yellow, idle);
            AddReturn(red, idle);
        }

        // ── Controller kiểu NÉM THEO CẤP: Idle mặc định; trigger Ball/Glove/Cup
        // → đòn ném tương ứng → về Idle. MatchView kích trigger theo cấp Dibu. ──
        private static void BuildDibuController(string path, string idlePath,
                                                string ballPath, string glovePath, string cupPath)
        {
            AssetDatabase.DeleteAsset(path);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
            ac.AddParameter("Ball", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Glove", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Cup", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            AnimatorState idle = sm.AddState("Idle");
            idle.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath);
            AnimatorState ball = sm.AddState("Ball");
            ball.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(ballPath);
            AnimatorState glove = sm.AddState("Glove");
            glove.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(glovePath);
            AnimatorState cup = sm.AddState("Cup");
            cup.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(cupPath);
            sm.defaultState = idle;

            AddTrigger(idle, ball, "Ball");
            AddTrigger(idle, glove, "Glove");
            AddTrigger(idle, cup, "Cup");
            AddReturn(ball, idle);
            AddReturn(glove, idle);
            AddReturn(cup, idle);
        }

        // ── Controller D10S: Idle mặc định; trigger Ball/Bottle/Mortar theo cấp
        // (MatchView kích theo cấp d10s lúc bắn) → đòn ném → về Idle. ──
        private static void BuildD10Controller(string path, string idlePath,
                                               string ballPath, string bottlePath, string mortarPath)
        {
            AssetDatabase.DeleteAsset(path);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
            ac.AddParameter("Ball", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Bottle", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Mortar", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            AnimatorState idle = sm.AddState("Idle");
            idle.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath);
            AnimatorState ball = sm.AddState("Ball");
            ball.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(ballPath);
            AnimatorState bottle = sm.AddState("Bottle");
            bottle.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(bottlePath);
            AnimatorState mortar = sm.AddState("Mortar");
            mortar.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(mortarPath);
            sm.defaultState = idle;

            AddTrigger(idle, ball, "Ball");
            AddTrigger(idle, bottle, "Bottle");
            AddTrigger(idle, mortar, "Mortar");
            AddReturn(ball, idle);
            AddReturn(bottle, idle);
            AddReturn(mortar, idle);
        }

        // ── Controller El Cinco: mỗi cấp có idle riêng là frame đầu action của nó.
        // MatchView đặt param Level khi mua/nâng, rồi trigger Punch/Kick/Stomp đưa
        // từ idle hiện tại vào đòn và trở về đúng idle cấp đó. ───────────────────
        private static void BuildCincoController(string path, string idleLv1Path,
                                                 string idleLv2Path, string idleLv3Path,
                                                 string punchPath, string kickPath, string stompPath)
        {
            AssetDatabase.DeleteAsset(path);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
            ac.AddParameter("Punch", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Kick", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Stomp", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Level", AnimatorControllerParameterType.Int);

            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            AnimatorState idleLv1 = sm.AddState("Idle Lv1");
            idleLv1.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleLv1Path);
            AnimatorState idleLv2 = sm.AddState("Idle Lv2");
            idleLv2.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleLv2Path);
            AnimatorState idleLv3 = sm.AddState("Idle Lv3");
            idleLv3.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleLv3Path);
            AnimatorState punch = sm.AddState("Punch");
            punch.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(punchPath);
            AnimatorState kick = sm.AddState("Kick");
            kick.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(kickPath);
            AnimatorState stomp = sm.AddState("Stomp");
            stomp.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(stompPath);
            sm.defaultState = idleLv1;

            AnimatorState[] idles = { idleLv1, idleLv2, idleLv3 };
            for (int i = 0; i < idles.Length; i++)
            {
                AddTrigger(idles[i], punch, "Punch");
                AddTrigger(idles[i], kick, "Kick");
                AddTrigger(idles[i], stomp, "Stomp");
                for (int j = 0; j < idles.Length; j++)
                    if (i != j) AddLevelTransition(idles[i], idles[j], j + 1, false);
            }
            foreach (AnimatorState attack in new[] { punch, kick, stomp })
                for (int level = 1; level <= idles.Length; level++)
                    AddLevelTransition(attack, idles[level - 1], level, true);
        }

        // ── Controller kiểu ĐI: đúng một state, lặp mãi, không trigger nào.
        // Quái không có trạng thái nào khác trong MVP — trúng đòn và bị làm chậm
        // đều đổi MÀU ở `MatchView`, không tốn frame. ──
        private static void BuildLoopController(string path, string clipPath)
        {
            AssetDatabase.DeleteAsset(path);
            var ac = AnimatorController.CreateAnimatorControllerAtPath(path);
            AnimatorStateMachine sm = ac.layers[0].stateMachine;
            AnimatorState walk = sm.AddState("Walk");
            walk.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            sm.defaultState = walk;
        }

        private static void AddTrigger(AnimatorState from, AnimatorState to, string trigger)
        {
            AnimatorStateTransition tr = from.AddTransition(to);
            tr.AddCondition(AnimatorConditionMode.If, 0, trigger);
            tr.hasExitTime = false;
            tr.duration = 0f;
        }

        private static void AddReturn(AnimatorState from, AnimatorState to)
        {
            AnimatorStateTransition tr = from.AddTransition(to);
            tr.hasExitTime = true;
            tr.exitTime = 1f;
            tr.duration = 0f;
        }

        private static void AddLevelTransition(AnimatorState from, AnimatorState to, int level,
                                               bool waitForClipEnd)
        {
            AnimatorStateTransition tr = from.AddTransition(to);
            tr.AddCondition(AnimatorConditionMode.Equals, level, "Level");
            tr.hasExitTime = waitForClipEnd;
            tr.exitTime = 1f;
            tr.duration = 0f;
        }

        private static void EnsureFolder(string dir)
        {
            if (AssetDatabase.IsValidFolder(dir)) return;
            string parent = System.IO.Path.GetDirectoryName(dir).Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(dir);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
