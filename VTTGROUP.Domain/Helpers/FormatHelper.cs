using System.Globalization;
namespace VTTGROUP.Domain.Helpers
{
    public static class FormatHelper
    {
        /// <summary>
        /// Định dạng số có dấu phẩy ngăn cách (vd: 1,234.56)
        /// </summary>
        /// 
        private static readonly string[] ChuSo = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
        private static readonly string[] DonViNho = { "", "nghìn", "triệu", "tỷ" };

        public static string Format<T>(T? value, string format = "#,##0.##") where T : struct
        {
            if (value == null) return "0";

            return value switch
            {
                decimal d => d.ToString(format, CultureInfo.InvariantCulture),
                double db => db.ToString(format, CultureInfo.InvariantCulture),
                int i => i.ToString(format, CultureInfo.InvariantCulture),
                long l => l.ToString(format, CultureInfo.InvariantCulture),
                _ => value?.ToString() ?? "0"
            };
        }

        /// <summary>
        /// Parse chuỗi thành đúng kiểu mong muốn (decimal, double, int...)
        /// </summary>
        public static T Parse<T>(string? input) where T : struct
        {
            var raw = input?.Replace(",", "") ?? "0";

            object result = typeof(T) switch
            {
                Type t when t == typeof(decimal) &&
                           decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) => d,

                Type t when t == typeof(double) &&
                           double.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var dbl) => dbl,

                Type t when t == typeof(int) &&
                           int.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var i) => i,

                Type t when t == typeof(long) &&
                           long.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var l) => l,

                _ => default(T)
            };

            return (T)result!;
        }

        public static string DocSoTienBangChu(decimal number)
        {
            if (number == 0)
                return "Không đồng";

            string s = ((long)number).ToString();
            int len = s.Length;
            int soDonVi = 0;
            string ketQua = "";
            bool daDocPhanTram = false;

            while (s.Length > 0)
            {
                string segment;
                if (s.Length >= 3)
                {
                    segment = s.Substring(s.Length - 3, 3);
                    s = s.Substring(0, s.Length - 3);
                }
                else
                {
                    segment = s;
                    s = "";
                }

                string doc3So = DocBaChuSo(segment);
                if (doc3So != "")
                {
                    ketQua = doc3So + " " + DonViNho[soDonVi] + " " + ketQua;
                    daDocPhanTram = true;
                }
                else if (soDonVi == 3 && daDocPhanTram) // đọc tỷ nhưng đoạn này là 000 thì vẫn phải đọc
                {
                    ketQua = DonViNho[soDonVi] + " " + ketQua;
                }

                soDonVi++;
            }

            ketQua = ketQua.Trim();
            // Viết hoa chữ cái đầu và thêm "đồng chẵn"
            return char.ToUpper(ketQua[0]) + ketQua.Substring(1) + " đồng chẵn";
        }

        private static string DocBaChuSo(string baSo)
        {
            while (baSo.Length < 3)
                baSo = "0" + baSo;

            int tram = int.Parse(baSo[0].ToString());
            int chuc = int.Parse(baSo[1].ToString());
            int donvi = int.Parse(baSo[2].ToString());

            string result = "";

            if (tram != 0)
            {
                result += ChuSo[tram] + " trăm";
                if (chuc == 0 && donvi != 0)
                    result += " linh";
            }

            if (chuc != 0 && chuc != 1)
            {
                result += " " + ChuSo[chuc] + " mươi";
                if (donvi == 1)
                    result += " mốt";
                else if (donvi == 5)
                    result += " lăm";
                else if (donvi != 0)
                    result += " " + ChuSo[donvi];
            }
            else if (chuc == 1)
            {
                result += " mười";
                if (donvi == 1)
                    result += " một";
                else if (donvi == 5)
                    result += " lăm";
                else if (donvi != 0)
                    result += " " + ChuSo[donvi];
            }
            else if (chuc == 0)
            {
                if (donvi != 0)
                    result += " " + ChuSo[donvi];
            }

            return result.Trim();
        }
    }
}
