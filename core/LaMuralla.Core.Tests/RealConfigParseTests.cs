using System.IO;
using LaMuralla.Core.Json;
using Xunit;

namespace LaMuralla.Core.Tests
{
    public class RealConfigParseTests
    {
        [Theory]
        [InlineData("towers.json")]
        [InlineData("enemies.json")]
        [InlineData("waves.json")]
        [InlineData("economy.json")]
        public void Doc_duoc_config_that(string file)
        {
            JsonValue v = JsonParser.Parse(File.ReadAllText(TestPaths.Config(file)));
            Assert.IsType<JsonObject>(v);
        }

        /// <summary>Mọi file map trong `config/maps/` phải đọc được — không chỉ map mặc định.
        /// Thêm map mới mà JSON hỏng thì đỏ ở đây, không phải lúc chạy game.</summary>
        [Fact]
        public void Doc_duoc_moi_file_map()
        {
            string dir = System.IO.Path.Combine(TestPaths.Root, "config", "maps");
            string[] files = Directory.GetFiles(dir, "*.json");
            Assert.NotEmpty(files);
            foreach (string f in files)
                Assert.IsType<JsonObject>(JsonParser.Parse(File.ReadAllText(f)));
        }

        [Fact]
        public void Doc_dung_tieng_viet_co_dau()
        {
            JsonObject t = JsonParser.Parse(File.ReadAllText(TestPaths.Config("towers.json"))).AsObject();
            JsonArray towers = t["towers"].AsArray();
            string names = "";
            foreach (JsonValue x in towers.Items) names += x.AsObject()["displayName"].AsString() + " ";
            Assert.Contains("El Árbitro", names);   // á có dấu — nơi encode hay vỡ
        }

        /// <summary>
        /// `spawns` là MẢNG `[{enemy, count, lane}]`, không phải object.
        ///
        /// Đổi ở vòng 27 để chở được `lane`: object không nhét được tuyến, và không
        /// diễn tả nổi "8 adepto tuyến trái + 6 adepto tuyến phải" vì khoá trùng nhau.
        /// Đây vẫn là chỗ `JsonUtility` của Unity dùng không được — lý do repo tự viết
        /// `JsonParser`.
        /// </summary>
        [Fact]
        public void Doc_dung_spawns_la_mang_co_lane()
        {
            JsonObject w = JsonParser.Parse(File.ReadAllText(TestPaths.Config("waves.json"))).AsObject();
            JsonObject g = w["waves"].AsArray()[0].AsObject()["spawns"].AsArray()[0].AsObject();
            Assert.Equal("adepto", g["enemy"].AsString());
            Assert.Equal(8, g["count"].AsInt());          // W1 (vòng 22: +25% quái)
            Assert.Equal("L1", g["lane"].AsString());
        }
    }
}
