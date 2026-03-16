using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Xceed.Words.NET;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Licensing;
using System.Security.AccessControl;
using Org.BouncyCastle.Utilities;

namespace VTTGROUP.Infrastructure.Services.Email
{
    public interface IEmailTemplateService
    {
        byte[] ReplacePlaceholders(string templatePath, IDictionary<string, string> data);
        byte[] ReplacePlaceholders(byte[] templateBytes, IDictionary<string, string> data);
        string Render(string template, IDictionary<string, object?> data, bool htmlEncode = false, CultureInfo? culture = null);
        string Render(string template, object data, bool htmlEncode = false, CultureInfo? culture = null);
        Task<string> RenderFromPathAsync(string filePath, IDictionary<string, object?> data, bool htmlEncode = false, CultureInfo? culture = null);
        Task<string> RenderFromPathAsync(string filePath, object data, bool htmlEncode = false, CultureInfo? culture = null);
        Task<(byte[] Bytes, string FileName)?> DownloadFileFromUrlAsync(string tenFileDinhKemLuu, string webRootPath);
        byte[] TryDocxToPdfAsync(byte[] Bytes);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private static bool _licensed;
        public EmailTemplateService()
        {
            if (!_licensed)
            {
                SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t3VVhhQlJDfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTH5bd0ZjXn1bcX1XTmlVWkd2");
                _licensed = true;
            }
        }

        private static readonly Regex PlaceholderRegex =
            new(@"\{\$\s*([A-Za-z0-9_\.]+)(?::([^|\}]+))?(?:\|([^}]*))?\}",
                RegexOptions.Compiled);

        #region Replace cột trong file

        // Từ file path
        public byte[] ReplacePlaceholders(string templatePath, IDictionary<string, string> data)
        {
            var bytes = System.IO.File.ReadAllBytes(templatePath);
            return ReplacePlaceholders(bytes, data);
        }

        // Từ byte[] (dùng khi file lấy từ DB/URL)
        public byte[] ReplacePlaceholders(byte[] templateBytes, IDictionary<string, string> data)
        {
            using var inMs = new MemoryStream(templateBytes, writable: false);
            using var doc = DocX.Load(inMs);

            foreach (var kv in data)
                doc.ReplaceText($"<<{kv.Key}>>", kv.Value ?? string.Empty, false, RegexOptions.None);

            using var outMs = new MemoryStream();
            doc.SaveAs(outMs);
            return outMs.ToArray();
        }

        #endregion
        public string Render(string template, IDictionary<string, object?> data, bool htmlEncode = false, CultureInfo? culture = null)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;
            culture ??= new CultureInfo("vi-VN");

            // dict không phân biệt hoa/thường
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in data) dict[kv.Key] = kv.Value;

            return PlaceholderRegex.Replace(template, m =>
            {
                var key = m.Groups[1].Value.Trim();
                var fmt = m.Groups[2].Success ? m.Groups[2].Value.Trim() : null;
                var def = m.Groups[3].Success ? m.Groups[3].Value : null;

                if (!dict.TryGetValue(key, out var raw) || raw is null || IsEmpty(raw))
                    return Encode(def ?? string.Empty, htmlEncode);

                var rendered = RenderValue(raw, fmt, culture);
                return Encode(rendered, htmlEncode);
            });
        }

        public string Render(string template, object data, bool htmlEncode = false, CultureInfo? culture = null)
            => Render(template, ToDictionary(data), htmlEncode, culture);

        public async Task<string> RenderFromPathAsync(string filePath, IDictionary<string, object?> data, bool htmlEncode = false, CultureInfo? culture = null)
        {
            var content = await System.IO.File.ReadAllTextAsync(filePath);
            return Render(content, data, htmlEncode, culture);
        }

        public Task<string> RenderFromPathAsync(string filePath, object data, bool htmlEncode = false, CultureInfo? culture = null)
            => RenderFromPathAsync(filePath, ToDictionary(data), htmlEncode, culture);

        // helpers
        private static string RenderValue(object value, string? format, CultureInfo culture)
        {
            if (value is DateTime dt)
                return string.IsNullOrWhiteSpace(format) ? dt.ToString(culture) : dt.ToString(format, culture);

            if (value is DateTimeOffset dto)
                return string.IsNullOrWhiteSpace(format) ? dto.ToString(culture) : dto.ToString(format, culture);

            if (value is IFormattable f && !string.IsNullOrWhiteSpace(format))
                return f.ToString(format, culture);

            if (!string.IsNullOrWhiteSpace(format) && value is string s)
            {
                if (DateTime.TryParse(s, culture, DateTimeStyles.None, out var pdt)) return pdt.ToString(format, culture);
                if (decimal.TryParse(s, NumberStyles.Any, culture, out var pdec)) return pdec.ToString(format, culture);
            }

            return Convert.ToString(value, culture) ?? string.Empty;
        }

        private static bool IsEmpty(object o) => o is string s && string.IsNullOrWhiteSpace(s);
        private static string Encode(string s, bool htmlEncode) => htmlEncode ? HtmlEncoder.Default.Encode(s) : s;

        private static IDictionary<string, object?> ToDictionary(object anon)
            => anon.GetType().GetProperties()
                   .ToDictionary(p => p.Name, p => p.GetValue(anon), StringComparer.OrdinalIgnoreCase);

        public async Task<(byte[] Bytes, string FileName)?> DownloadFileFromUrlAsync(string tenFileDinhKemLuu, string webRootPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenFileDinhKemLuu))
                    return null;

                var relPath = tenFileDinhKemLuu.Replace('\\', '/').TrimStart('/');
                if (!relPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                    return null;

                var fullPath = Path.Combine(webRootPath, relPath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(fullPath))
                    return null;

                var bytes = await File.ReadAllBytesAsync(fullPath);
                return (bytes, Path.GetFileName(fullPath));
            }
            catch
            {
                return null;
            }
        }
        public byte[] TryDocxToPdfAsync(byte[] docxBytes)
        {
            using var input = new MemoryStream(docxBytes, writable: false);
            using var doc = new WordDocument(input, FormatType.Docx);
            using var renderer = new DocIORenderer();
            renderer.Settings.EmbedFonts = true; // nếu cần ổn định font
            using PdfDocument pdf = renderer.ConvertToPDF(doc);
            using var output = new MemoryStream();
            pdf.Save(output);
            pdf.Close(true);
            doc.Close();
            return output.ToArray();
        }
        //public async Task<byte[]?> TryDocxToPdfAsync(byte[] docxBytes)
        //{
        //    if (!File.Exists(_sofficePath))
        //        throw new FileNotFoundException("Không tìm thấy soffice.exe", _sofficePath);

        //    var workDir = Path.Combine(Path.GetTempPath(), "docx2pdf_" + Guid.NewGuid().ToString("N"));
        //    Directory.CreateDirectory(workDir);

        //    var inputPath = Path.Combine(workDir, "input.docx");
        //    var outputPath = Path.Combine(workDir, "input.pdf");
        //    await File.WriteAllBytesAsync(inputPath, docxBytes);

        //    // Thư mục profile riêng cho lần chạy này
        //    var profileDir = Path.Combine(workDir, "profile");
        //    Directory.CreateDirectory(profileDir);
        //    var profileUri = new Uri(profileDir + Path.DirectorySeparatorChar).AbsoluteUri;

        //    var psi = new ProcessStartInfo
        //    {
        //        FileName = _sofficePath,
        //        WorkingDirectory = workDir,
        //        Arguments = $"--headless --nologo --nofirststartwizard -env:UserInstallation=\"{profileUri}\" --convert-to pdf \"{inputPath}\" --outdir \"{workDir}\"",
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    // Ép TMP/TEMP về thư mục tạm để tránh lỗi quyền
        //    psi.Environment["TMP"] = workDir;
        //    psi.Environment["TEMP"] = workDir;
        //    psi.Environment["HOME"] = workDir;
        //    psi.Environment["APPDATA"] = workDir;
        //    psi.Environment["USERPROFILE"] = workDir;

        //    try
        //    {
        //        using var proc = Process.Start(psi)!;
        //        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        //        await proc.WaitForExitAsync(cts.Token);

        //        if (proc.ExitCode != 0 || !File.Exists(outputPath))
        //            return null;

        //        return await File.ReadAllBytesAsync(outputPath);
        //    }
        //    finally
        //    {
        //        try { Directory.Delete(workDir, true); } catch { /* ignore */ }
        //    }
        //}
    }

}
