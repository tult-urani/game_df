using System.IO;

namespace LaMuralla.Core.Tests
{
    internal static class TestPaths
    {
        /// <summary>Gốc repo, dò ngược từ thư mục chạy test.
        /// Test đọc `config/*.json` THẬT, không đọc bản sao — bản sao sẽ mốc.</summary>
        internal static string Root { get; } = Find();

        internal static string Config(string name) => Path.Combine(Root, "config", name);

        /// <summary>File map. `config/path.json` cũ giờ là `config/maps/m00-la-muralla.json`.</summary>
        internal static string Map(string file) => Path.Combine(Root, "config", "maps", file);

        /// <summary>Map mặc định dùng trong test — map gốc đã cân xong.</summary>
        internal const string DefaultMap = "m00-la-muralla.json";

        private static string Find()
        {
            var d = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (d != null && !Directory.Exists(Path.Combine(d.FullName, "config")))
                d = d.Parent;
            return d?.FullName ?? throw new DirectoryNotFoundException("không tìm thấy thư mục config/");
        }
    }
}
