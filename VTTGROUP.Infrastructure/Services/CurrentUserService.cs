using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VTTGROUP.Domain.Model.NhanVien;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public interface ICurrentUserService
    {
        int? UserID { get; }
        string? Username { get; }
        string? MaNhanVien { get; }
        string? Role { get; }
        bool IsLoaded { get; }
        void LoadFromToken(string token);
        Task EnsureUserLoadedFromJSAsync(IJSRuntime js);
        Task<NguoiLapModel> GetThongTinNguoiLapAsync(string? maNhanVien = null, CancellationToken ct = default);
    }
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        public CurrentUserService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }
        private bool _isLoaded = false;
        public int? UserID { get; private set; }
        public string? Username { get; private set; }
        public string? MaNhanVien { get; private set; }
        public string? Role { get; private set; }
        public bool IsLoaded => _isLoaded;
        public void LoadFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            UserID = Convert.ToInt32(jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            Username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            MaNhanVien = jwt.Claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            Role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            _isLoaded = true;
        }

        //public async Task<NguoiLapModel> GetThongTinNguoiLapAsync(string? maNhanVien = null)
        //{
        //    var targetMaNV = maNhanVien ?? MaNhanVien;

        //    if (!_isLoaded || string.IsNullOrEmpty(targetMaNV))
        //        return new NguoiLapModel();

        //    using var _context = _factory.CreateDbContext();

        //    var entity = await (
        //              from nv in _context.TblNhanviens
        //              join chucvu in _context.TblNhanvienChucvus on nv.MaChucVu equals chucvu.MaChucVu
        //              where nv.MaNhanVien == targetMaNV
        //              select new NguoiLapModel
        //              {
        //                  MaNhanVien = targetMaNV,
        //                  HoVaTen = nv.HoVaTen,
        //                  ChucVu = chucvu.TenChucVu ?? string.Empty,
        //                  LoaiUser = "NV",
        //                  SoLuongBookCanHo = 0,
        //              }).FirstOrDefaultAsync();
        //    if (entity == null)
        //    {
        //        entity = await (
        //              from nv in _context.DmSanGiaoDiches
        //              where nv.MaSanGiaoDich == targetMaNV
        //              select new NguoiLapModel
        //              {
        //                  MaNhanVien = targetMaNV,
        //                  HoVaTen = nv.TenSanGiaoDich ?? string.Empty,
        //                  LoaiUser = "SGG",
        //                  MaSanGiaoDich = nv.MaSanGiaoDich,
        //                  TenSanGiaoDich = nv.TenSanGiaoDich ?? string.Empty,
        //                  SoLuongBookCanHo = nv.SoLuongBookCanHo ?? 0,
        //              }).FirstOrDefaultAsync();

        //    }

        //    if (entity == null)
        //    {
        //        entity = await (
        //              from nv in _context.TblNhanviens
        //              join san in _context.DmSanGiaoDiches on nv.MaSanGiaoDich equals san.MaSanGiaoDich
        //              where nv.MaNhanVien == targetMaNV
        //              select new NguoiLapModel
        //              {
        //                  MaNhanVien = targetMaNV,
        //                  HoVaTen = nv.HoVaTen,
        //                  LoaiUser = "SGG",
        //                  MaSanGiaoDich = nv.MaSanGiaoDich ?? string.Empty,
        //                  TenSanGiaoDich = san.TenSanGiaoDich ?? string.Empty,
        //                  SoLuongBookCanHo = san.SoLuongBookCanHo ?? 0,
        //              }).FirstOrDefaultAsync();
        //    }

        //    return entity;
        //}

        public async Task<NguoiLapModel> GetThongTinNguoiLapAsync(
    string? maNhanVien = null,
    CancellationToken ct = default)
        {
            var target = (maNhanVien ?? MaNhanVien)?.Trim();

            if (!_isLoaded || string.IsNullOrWhiteSpace(target))
                return new NguoiLapModel();

            await using var db = await _factory.CreateDbContextAsync(ct);

            // 1) Ưu tiên: Nhân viên (NV) + Chức vụ (LEFT JOIN để không mất NV nếu MaChucVu null)
            var nv = await (
                from n in db.TblNhanviens.AsNoTracking()
                join cv0 in db.TblNhanvienChucvus.AsNoTracking()
                    on n.MaChucVu equals cv0.MaChucVu into cvGroup
                from cv in cvGroup.DefaultIfEmpty()
                where n.MaNhanVien == target && ((n.MaSanGiaoDich ?? string.Empty) == string.Empty)
                select new NguoiLapModel
                {
                    MaNhanVien = target,
                    HoVaTen = n.HoVaTen,
                    ChucVu = cv != null ? (cv.TenChucVu ?? string.Empty) : string.Empty,
                    LoaiUser = "NV",
                    SoLuongBookCanHo = 0
                }
            ).FirstOrDefaultAsync(ct);

            if (nv is not null)
                return nv;

            // 2) Nếu không phải NV, thử coi target là Mã sàn giao dịch (SGG)
            var sgg = await db.DmSanGiaoDiches.AsNoTracking()
                .Where(x => x.MaSanGiaoDich == target)
                .Select(x => new NguoiLapModel
                {
                    MaNhanVien = target,
                    HoVaTen = x.TenSanGiaoDich ?? string.Empty,
                    LoaiUser = "SGG",
                    MaSanGiaoDich = x.MaSanGiaoDich,
                    TenSanGiaoDich = x.TenSanGiaoDich ?? string.Empty,
                  //  SoLuongBookCanHo = x.SoLuongBookCanHo ?? 0
                })
                .FirstOrDefaultAsync(ct);

            if (sgg is not null)
                return sgg;

            // 3) Cuối cùng: NV thuộc sàn (NV -> SGG)
            var nvThuocSan = await (
                from n in db.TblNhanviens.AsNoTracking()
                join s in db.DmSanGiaoDiches.AsNoTracking()
                    on n.MaSanGiaoDich equals s.MaSanGiaoDich
                where n.MaNhanVien == target
                select new NguoiLapModel
                {
                    MaNhanVien = target,
                    HoVaTen = n.HoVaTen,
                    LoaiUser = "SGG",
                    MaSanGiaoDich = n.MaSanGiaoDich ?? string.Empty,
                    TenSanGiaoDich = s.TenSanGiaoDich ?? string.Empty,
                   // SoLuongBookCanHo = s.SoLuongBookCanHo ?? 0
                }
            ).FirstOrDefaultAsync(ct);

            return nvThuocSan ?? new NguoiLapModel();
        }


        public async Task EnsureUserLoadedFromJSAsync(IJSRuntime js)
        {
            if (_isLoaded) return;

            var token = await js.InvokeAsync<string>("blazorGetCookie", "accessToken");
            if (!string.IsNullOrEmpty(token))
            {
                LoadFromToken(token);
            }
            //Console.WriteLine($"🟢 Token parsed. name = {MaNhanVien}, role = {Role}");
        }
    }
}
