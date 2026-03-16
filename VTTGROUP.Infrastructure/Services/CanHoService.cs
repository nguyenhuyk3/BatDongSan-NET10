using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.Json;
using VTTGROUP.Domain.Helpers;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.CanHo;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class CanHoService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        // private readonly AppDbContext _context;
        private readonly ILogger<CanHoService> _logger;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly IDbConnection _connection;
        private readonly ICurrentUserService _currentUser;
        public CanHoService(IDbContextFactory<AppDbContext> factory, ILogger<CanHoService> logger, IConfiguration config, IMemoryCache cache, ICurrentUserService currentUser)
        {
            // _context = context;
            _factory = factory;
            _logger = logger;
            _config = config;
            _cache = cache;
            _connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            _currentUser = currentUser;
        }

        public async Task<List<DaDanhMucDuAn>> GetByDuAnAsync()
        {
            var entity = new List<DaDanhMucDuAn>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucDuAns.ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucDuAn>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách dự án");
            }
            return entity;
        }

        public async Task<List<HienTrangKinhDoanDto>> GetHienTrangCanHoAsync(string maDuAn)
        {
            var entity = new List<HienTrangKinhDoanDto>();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@MaDuAn", maDuAn);
                var result = await _connection.QueryAsync<HienTrangKinhDoanDto>(
                    "Proc_SoDoCanHo_HienTrangKinhDoanh",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách hiện trạng kinh doanh căn hộ");
            }
            return entity;
        }

        public async Task<List<SoDoCanHoModel>> GetSoDoCanHoAsync(string maDuAn, string maBlock)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@MaDuAn", maDuAn);
                parameters.Add("@MaBlock", maBlock);

                var result = await _connection.QueryAsync<SoDoCanHoModel>(
                    "Proc_SoDoCanHo_ViewTheoTrucMat",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new List<SoDoCanHoModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new List<SoDoCanHoModel>();
            }
        }

        public async Task<DataTable> GetSoDoCanHo1Async(string maDuAn, string? maBlock = null)
        {
            try
            {
                var param = new DynamicParameters();
                param.Add("@maDuAn", maDuAn);
                param.Add("@maBlock", !string.IsNullOrEmpty(maBlock) ? maBlock : null);

                var result = await _connection.QueryAsync(
                    sql: "Proc_SoDoCanHo_Dong",
                    param: param,
                    commandType: CommandType.StoredProcedure
                );

                // Convert to DataTable for flexible dynamic columns
                return result.ToDataTable(); // Có thể dùng extension `.ToDataTable()` nếu đã định nghĩa
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new DataTable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new DataTable();
            }
        }

        public async Task<List<CanHoModel>> GetSoDoCanHoAsyncBK(string? maDuAn, string? maBlock)
        {
            var canHos = new List<CanHoModel>();
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("Proc_SoDoSanPham", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@maDuAn", maDuAn);
                    cmd.Parameters.AddWithValue("@maBlock", maBlock);

                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            canHos.Add(new CanHoModel
                            {
                                MaCanHo = reader["MaCanHo"]?.ToString(),
                                TenCanHo = reader["TenCanHo"]?.ToString(),
                                Tang = new Domain.Model.Tang.TangModel
                                {
                                    MaTang = reader["MaTang"]?.ToString(),
                                    TenTang = reader["TenTang"]?.ToString(),

                                },
                                LoaiView = reader["LoaiView"]?.ToString()
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetSoDoCanHoAsync] Lỗi khi lấy toàn bộ danh sách can hộ theo dự án và block");
            }
            return canHos;
        }
        public async Task<List<SoDoCanHoModel>> GetSoDoCanHoByGioHangAsync(string maDuAn, string maBlock)
        {
            try
            {
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                string MaNhanVien = NguoiLap.MaNhanVien;
                var parameters = new DynamicParameters();
                parameters.Add("@MaDuAn", maDuAn);
                parameters.Add("@MaBlock", maBlock);
                parameters.Add("@MaNhanVien", MaNhanVien);

                var result = await _connection.QueryAsync<SoDoCanHoModel>(
                    "Proc_GioHang_ViewSoDoCanHoDangMoBan",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoByGioHangAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new List<SoDoCanHoModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoByGioHangAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new List<SoDoCanHoModel>();
            }
        }

        //   public async Task<List<UploadedFileModel>> GetThongTinThietKeAsync(
        //string maDuAn,
        //string maSanPham,
        //CancellationToken ct = default)
        //   {
        //       if (string.IsNullOrWhiteSpace(maDuAn) || string.IsNullOrWhiteSpace(maSanPham))
        //           return [];

        //       var duAnKey = maDuAn.Trim();
        //       var spKey = maSanPham.Trim();

        //       try
        //       {
        //           await using var db = await _factory.CreateDbContextAsync(ct);

        //           // 1) Lấy MaLoaiLayout từ DA_DanhMucSanPham
        //           var loaiLayout = await db.DaDanhMucSanPhams.AsNoTracking()
        //               .Where(x => (x.MaDuAn ?? "").Trim() == duAnKey
        //                        && (x.MaSanPham ?? "").Trim() == spKey)
        //               .Select(x => x.MaLoaiLayout)
        //               .FirstOrDefaultAsync(ct);

        //           if (string.IsNullOrWhiteSpace(loaiLayout))
        //               return [];

        //           var loaiKey = loaiLayout.Trim();

        //           // 2) Lấy danh sách HinhAnh từ DA_DanhMucLoaiThietKe (NHIỀU DÒNG => ToListAsync)
        //           var hinhAnhRawList = await db.DaDanhMucLoaiThietKes.AsNoTracking()
        //               .Where(x => (x.MaDuAn ?? "").Trim() == duAnKey
        //                        && (x.MaLoaiThietKe ?? "").Trim() == loaiKey)
        //               .Select(x => x.HinhAnh)
        //               .ToListAsync(ct);

        //           if (hinhAnhRawList.Count == 0)
        //               return [];

        //           // 3) Flatten tất cả ảnh thành 1 list path
        //           var paths = hinhAnhRawList
        //               .Where(s => !string.IsNullOrWhiteSpace(s))
        //               .SelectMany(ParseImageList)
        //               .Select(p => (p ?? "").Trim().Replace("\\", "/"))
        //               .Where(p => !string.IsNullOrWhiteSpace(p))
        //               .Distinct(StringComparer.OrdinalIgnoreCase)
        //               .ToList();

        //           // 4) Map ra UploadedFileModel
        //           var result = paths.Select(p => new UploadedFileModel
        //           {
        //               // Id không có thì để 0 / null tùy model của baby
        //               FileName = SafeFileName(p),
        //               FileNameSave = p
        //           }).ToList();

        //           return result;
        //       }
        //       catch (OperationCanceledException)
        //       {
        //           throw;
        //       }
        //       catch (Exception ex)
        //       {
        //           _logger.LogError(ex,
        //               "[GetThongTinThietKeAsync] Lỗi lấy thiết kế. MaDuAn={MaDuAn}, MaSanPham={MaSanPham}",
        //               duAnKey, spKey);

        //           return [];
        //       }
        //   }

        public async Task<List<UploadedFileModel>> GetThongTinThietKeAsync(
    string maDuAn,
    string maSanPham,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn) || string.IsNullOrWhiteSpace(maSanPham))
                return [];

            var duAnKey = maDuAn.Trim();
            var spKey = maSanPham.Trim();

            try
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                // 1) Lấy MaLoaiLayout từ DA_DanhMucSanPham
                var loaiLayout = await db.DaDanhMucSanPhams.AsNoTracking()
                    .Where(x => (x.MaDuAn ?? "").Trim() == duAnKey
                             && (x.MaSanPham ?? "").Trim() == spKey)
                    .Select(x => x.MaLoaiLayout)
                    .FirstOrDefaultAsync(ct);

                if (string.IsNullOrWhiteSpace(loaiLayout))
                    return [];

                var loaiKey = loaiLayout.Trim();

                // 2) Lấy cả HinhAnh + FullDomain
                var rawList = await db.DaDanhMucLoaiThietKes.AsNoTracking()
                    .Where(x => (x.MaDuAn ?? "").Trim() == duAnKey
                             && (x.MaLoaiThietKe ?? "").Trim() == loaiKey)
                    .Select(x => new
                    {
                        x.HinhAnh,
                        x.FullDomain
                    })
                    .ToListAsync(ct);

                if (rawList.Count == 0)
                    return [];

                // 3) Ưu tiên FullDomain, fallback HinhAnh
                var urls = new List<string>();

                foreach (var r in rawList)
                {
                    // FullDomain có thể là 1 link hoặc danh sách link
                    var fulls = (r.FullDomain ?? "")
                        .Trim()
                        .Replace("\\", "/");

                    if (!string.IsNullOrWhiteSpace(fulls))
                    {
                        urls.AddRange(
                            ParseImageList(fulls)
                                .Select(p => (p ?? "").Trim().Replace("\\", "/"))
                                .Where(p => !string.IsNullOrWhiteSpace(p))
                        );
                        continue;
                    }

                    // fallback: HinhAnh
                    var rels = (r.HinhAnh ?? "")
                        .Trim()
                        .Replace("\\", "/");

                    if (!string.IsNullOrWhiteSpace(rels))
                    {
                        urls.AddRange(
                            ParseImageList(rels)
                                .Select(p => (p ?? "").Trim().Replace("\\", "/"))
                                .Where(p => !string.IsNullOrWhiteSpace(p))
                        );
                    }
                }

                // 4) Distinct + map
                var result = urls
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(u => new UploadedFileModel
                    {
                        FileName = SafeFileName(u),
                        FileNameSave = u,     // ✅ giờ FileNameSave chính là FullDomain nếu DB có
                        FullDomain = u,       // ✅ set luôn cho rõ ràng                     
                        ContentType = IsVideoUrl(u) ? "video" : "image"
                    })
                    .ToList();

                return result;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[GetThongTinThietKeAsync] Lỗi lấy thiết kế. MaDuAn={MaDuAn}, MaSanPham={MaSanPham}",
                    duAnKey, spKey);

                return [];
            }
        }

        private static bool IsVideoUrl(string url)
        {
            var ext = Path.GetExtension(url)?.ToLowerInvariant();
            return ext is ".mp4" or ".webm" or ".ogg" or ".mov";
        }


        /// <summary>
        /// Parse HinhAnh raw: hỗ trợ JSON array hoặc chuỗi phân tách ; , | newline
        /// </summary>
        private static IEnumerable<string> ParseImageList(string raw)
        {
            raw = raw.Trim();

            // Case 1: JSON array ["a","b"]
            if (raw.StartsWith("[") && raw.EndsWith("]"))
            {
                try
                {
                    var arr = JsonSerializer.Deserialize<List<string>>(raw);
                    if (arr is not null)
                        return arr.Where(x => !string.IsNullOrWhiteSpace(x));
                }
                catch
                {
                    // fallback qua split bình thường
                }
            }

            // Case 2: split theo nhiều kiểu delimiter
            var parts = raw.Split(
                new[] { ";", ",", "|", "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return parts;
        }

        private static string SafeFileName(string path)
        {
            try
            {
                return Path.GetFileName(path);
            }
            catch
            {
                return path;
            }
        }

        public async Task<string> GetLoaiUserCanHoAsync()
        {
            var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
            return NguoiLap.LoaiUser ?? string.Empty;
        }

        public async Task<SoDoCanHoModel?> GetViewMatKhoiHuongAsync(
     string maDuAn,
     string maSanPham,
     CancellationToken ct = default)
        {
            maDuAn = (maDuAn ?? string.Empty).Trim();
            maSanPham = (maSanPham ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(maDuAn) || string.IsNullOrWhiteSpace(maSanPham))
                return null;

            try
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                var query =
                    from sp in db.DaDanhMucSanPhams.AsNoTracking()
                    where sp.MaDuAn == maDuAn && sp.MaSanPham == maSanPham

                    join vt0 in db.DaDanhMucViewTrucs.AsNoTracking()
                        on new { sp.MaDuAn, sp.MaBlock, sp.MaTruc }
                        equals new { vt0.MaDuAn, vt0.MaBlock, vt0.MaTruc }
                        into vtJoin
                    from vt in vtJoin.DefaultIfEmpty()

                    join mk0 in db.DaDanhMucViewMatKhois.AsNoTracking()
                        on vt.MaViewMatKhoi equals mk0.MaMatKhoi
                        into mkJoin
                    from mk in mkJoin.DefaultIfEmpty()

                    join h0 in db.DaDanhMucHuongs.AsNoTracking()
                        on new { MaDuAn = sp.MaDuAn, MaHuong = (string?)vt.MaHuong }
                        equals new { h0.MaDuAn, MaHuong = (string?)h0.MaHuong }
                        into hJoin
                    from h in hJoin.DefaultIfEmpty()

                    select new SoDoCanHoModel
                    {
                        MaDuAn = sp.MaDuAn,
                        MaSanPham = sp.MaSanPham,

                        MaView = vt != null ? vt.MaViewMatKhoi : null,
                        TenView = mk != null ? mk.TenMatKhoi : null,

                        MaHuong = vt != null ? vt.MaHuong : null,
                        TenHuong = h != null ? h.TenHuong : null
                    };

                return await query.FirstOrDefaultAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // tôn trọng cancel
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetViewMatKhoiHuongAsync failed. MaDuAn={MaDuAn}, MaSanPham={MaSanPham}",
                    maDuAn, maSanPham);

                // tuỳ chiến lược: trả null hay throw business exception.
                // Senior recommendation: throw để upstream xử lý đúng + không che lỗi data.
                return new SoDoCanHoModel
                {
                    MaDuAn = maDuAn,
                    MaSanPham = maSanPham,
                    MaView = string.Empty,
                    TenView = string.Empty,
                    MaHuong = string.Empty,
                    TenHuong = string.Empty
                };
            }
        }

        #region Kiểm tra ẩn hiện nút đăng ký căn hộ ở giỏ hàng
        //   public async Task<bool> CanShowDangKyAsync(
        //string maDuAn,
        //string maCanHo,
        //string? maGioHang,
        //CancellationToken ct = default)
        //   {
        //       if (string.IsNullOrWhiteSpace(maDuAn) || string.IsNullOrWhiteSpace(maCanHo))
        //           return false;

        //       var duAnKey = maDuAn.Trim();
        //       var canHoKey = maCanHo.Trim();
        //       var gioHangKey = (maGioHang ?? "").Trim();

        //       try
        //       {
        //           await using var db = await _factory.CreateDbContextAsync(ct);

        //           var hienTrang = await db.DaDanhMucSanPhams.AsNoTracking()
        //               .Where(x => x.MaDuAn == duAnKey && x.MaSanPham == canHoKey)
        //               .Select(x => x.HienTrangKd)
        //               .FirstOrDefaultAsync(ct);

        //           if (hienTrang is null) return false;

        //           if (string.Equals((hienTrang ?? "ConTrong").Trim(), "DaBan", StringComparison.OrdinalIgnoreCase))
        //               return false;

        //           var hasActivePdk = await db.BhPhieuDangKiChonCans.AsNoTracking()
        //               .AnyAsync(x =>
        //                   x.MaDuAn == duAnKey &&
        //                   x.MaCanHo == canHoKey &&
        //                   (string.IsNullOrEmpty(gioHangKey) || x.MaGioHang == gioHangKey) &&
        //                   (x.IsHetHieuLuc != true) &&
        //                   (x.IsXacNhan == null || x.IsXacNhan == false),
        //                   ct);

        //           return !hasActivePdk;
        //       }
        //       catch (OperationCanceledException) { throw; }
        //       catch (Exception ex)
        //       {
        //           _logger.LogError(ex,
        //               "[CanShowDangKyAsync] Failed. MaDuAn={MaDuAn}, MaCanHo={MaCanHo}, MaGioHang={MaGioHang}",
        //               duAnKey, canHoKey, gioHangKey);

        //           return false; // an toàn: lỗi thì ẩn nút
        //       }
        //   }
        #endregion
    }
}
