using System.Globalization;
using System.Threading;
using LaMuralla.Core.Json;
using Xunit;

namespace LaMuralla.Core.Tests
{
    public class JsonParserTests
    {
        [Fact]
        public void Doc_duoc_object_long_nhau()
        {
            JsonObject o = JsonParser.Parse("""{"a": {"b": [1, 2.5, -3]}}""").AsObject();
            Assert.Equal(2.5, o["a"].AsObject()["b"].AsArray()[1].AsNumber());
        }

        [Fact]
        public void Khoa_trung_thi_nem_chu_khong_im_lang_ghi_de()
        {
            // Bug thật ở vòng 3: enemies.json khai slowResistPercent hai lần.
            JsonException e = Assert.Throws<JsonException>(
                () => JsonParser.Parse("""{"x": 1, "x": 2}"""));
            Assert.Contains("khoá trùng", e.Message);
        }

        [Fact]
        public void Loi_chi_dung_duong_dan_chu_khong_noi_chung_chung()
        {
            JsonValue v = JsonParser.Parse("""{"towers": [{"levels": [{"cost": "sai"}]}]}""");
            JsonException e = Assert.Throws<JsonException>(
                () => v.AsObject()["towers"].AsArray()[0].AsObject()["levels"].AsArray()[0]
                       .AsObject()["cost"].AsNumber());
            Assert.Contains("towers[0].levels[0].cost", e.Message);
        }

        [Fact]
        public void So_thuc_doc_dung_ke_ca_khi_may_dat_locale_viet_nam()
        {
            // vi-VN dùng dấu phẩy thập phân → "0.69" có thể bị đọc thành 69.
            CultureInfo old = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                Assert.Equal(0.69, JsonParser.Parse("""{"base": 0.69}""").AsObject()["base"].AsNumber());
            }
            finally { Thread.CurrentThread.CurrentCulture = old; }
        }

        [Fact]
        public void Thieu_truong_bat_buoc_thi_nem_kem_ten_truong()
        {
            JsonObject o = JsonParser.Parse("""{"a": 1}""").AsObject();
            JsonException e = Assert.Throws<JsonException>(() => _ = o["khong_ton_tai"]);
            Assert.Contains("khong_ton_tai", e.Message);
        }
    }
}
