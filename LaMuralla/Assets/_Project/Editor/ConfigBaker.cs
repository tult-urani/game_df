using System;
using System.IO;
using LaMuralla.Core.Config;
using LaMuralla.Unity;
using UnityEditor;
using UnityEngine;

namespace LaMuralla.EditorTools
{
    /// <summary>
    /// `config/*.json` (gốc repo) → `Assets/_Project/Resources/BakedConfig.asset`.
    ///
    /// 🔴 KHÔNG FORK. Baker ĐỌC config/ lúc edit-time; nó không bao giờ ghi ngược.
    /// Nguồn chân lý vẫn là `config/`, dùng chung với tools/*.py. Asset sinh ra là
    /// sản phẩm phái sinh — xoá đi bake lại được.
    ///
    /// Validate TRƯỚC khi ghi: config sai thì hỏng build ngay tại máy người viết,
    /// chứ không phải crash trên iPhone người chơi. Đó là toàn bộ lý do bake thay
    /// vì nhét thẳng JSON vào StreamingAssets.
    /// </summary>
    public static class ConfigBaker
    {
        private const string AssetPath = "Assets/_Project/Resources/BakedConfig.asset";

        [MenuItem("La Muralla/Bake Config")]
        public static void Bake()
        {
            try
            {
                BakeOrThrow();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConfigBaker] BAKE HỎNG — {ex.Message}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                throw;
            }
        }

        private static void BakeOrThrow()
        {
            string dir = ConfigDir();
            string towers = Read(dir, "towers.json");
            string enemies = Read(dir, "enemies.json");
            string waves = Read(dir, "waves.json");
            string economy = Read(dir, "economy.json");
            // 🔵 Bake MỌI map trong config/maps/, không chỉ map mặc định. Validate
            // TỪNG map: thêm một map sai số thì hỏng ngay ở máy người viết, chứ không
            // phải lúc người chơi bấm vào nó.
            string mapDir = System.IO.Path.Combine(dir, "maps");
            string[] mapFiles = Directory.GetFiles(mapDir, "*.json");
            System.Array.Sort(mapFiles, System.StringComparer.Ordinal);
            if (mapFiles.Length == 0)
                throw new IOException($"không có map nào trong {mapDir}");

            var entries = new System.Collections.Generic.List<BakedConfig.MapEntry>();
            GameConfig? first = null;
            string path = "";

            foreach (string f in mapFiles)
            {
                string json = File.ReadAllText(f);
                GameConfig one = ConfigMapper.Map(towers, enemies, waves, economy, json);
                ConfigValidator.Validate(one);   // ném = không ghi asset nào

                entries.Add(new BakedConfig.MapEntry
                {
                    id = one.MapId,
                    displayName = one.MapDisplayName,
                    order = entries.Count,
                    json = json,
                });
                if (first == null) { first = one; path = json; }
            }

            GameConfig cfg = first!;

            Directory.CreateDirectory(Path.GetDirectoryName(AssetPath)!);
            BakedConfig asset = AssetDatabase.LoadAssetAtPath<BakedConfig>(AssetPath);
            bool isNew = asset == null;
            if (isNew) asset = ScriptableObject.CreateInstance<BakedConfig>();

            asset.towersJson = towers;
            asset.enemiesJson = enemies;
            asset.wavesJson = waves;
            asset.economyJson = economy;
            asset.pathJson = path;
            asset.maps = entries.ToArray();
            asset.bakedAtUtc = DateTime.UtcNow.ToString("o");

            if (isNew) AssetDatabase.CreateAsset(asset, AssetPath);
            else EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            Debug.Log($"[ConfigBaker] OK — {cfg.Towers.Count} tướng, {cfg.Enemies.Count} quân, " +
                      $"{cfg.Waves.Count} wave, {cfg.Path.Slots.Count} ô · " +
                      $"{entries.Count} map ({string.Join(", ", entries.ConvertAll(x => x.id))}) → {AssetPath}");
        }

        /// <summary>`config/` ở gốc repo, tức trên `Assets/` hai bậc:
        /// {repo}/LaMuralla/Assets → {repo}/config</summary>
        private static string ConfigDir()
        {
            string repo = Directory.GetParent(Application.dataPath)!.Parent!.FullName;
            string dir = Path.Combine(repo, "config");
            if (!Directory.Exists(dir))
                throw new DirectoryNotFoundException(
                    $"không thấy config/ ở `{dir}`. Baker đọc config gốc của repo, " +
                    "không đọc bản sao — nếu cấu trúc thư mục đổi thì sửa hàm này.");
            return dir;
        }

        private static string Read(string dir, string name)
        {
            string f = Path.Combine(dir, name);
            if (!File.Exists(f)) throw new FileNotFoundException($"thiếu config `{name}`", f);
            return File.ReadAllText(f);
        }
    }
}
