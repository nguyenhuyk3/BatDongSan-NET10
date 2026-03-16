using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VTTGROUP.Domain.Model.CanHo;
using VTTGROUP.Domain.Model.Hubs;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class BaseBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<BaseBackgroundService> _logger;

        public BaseBackgroundService(IServiceProvider provider, ILogger<BaseBackgroundService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var notifier = scope.ServiceProvider.GetRequiredService<INotificationSender>();

                var hasChanged = await UpdatePhieuDangKyAsync(db);
                if (hasChanged)
                {
                    await notifier.UpdateTTCHAsync();
                }

                var nowUtc = DateTime.UtcNow;
                var pending = await BuildDangKyCountdownsAsync(db, nowUtc);
                await notifier.DangKyCountdownsAsync(pending, nowUtc);


                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }

        #region Update Đăng ký nếu chưa xác nhận
        private async Task<bool> UpdatePhieuDangKyAsync(AppDbContext dbContext)
        {
            try
            {
                bool hasChanges = false;
                var now = DateTime.Now;

                // Lấy các phiếu chưa xác nhận và chưa hết hiệu lực
                var pendingDangKys = await dbContext.BhPhieuDangKiChonCans
                    .Where(p => (p.IsXacNhan ?? false) == false && (p.IsHetHieuLuc ?? false) == false)
                    .ToListAsync();

                foreach (var phieu in pendingDangKys)
                {
                    var gioHang = await dbContext.BhGioHangs
                        .FirstOrDefaultAsync(g => g.MaPhieu == phieu.MaGioHang);

                    if (gioHang == null) continue;

                    var config = await dbContext.DaDanhMucDuAnCauHinhChungs
                        .FirstOrDefaultAsync(c => c.MaDuAn == phieu.MaDuAn);

                    if (config == null) continue;

                    int allowedMinutes = gioHang.LoaiGioHang == true
                        ? config.ThoiGianChoBookGioHangChung ?? 0
                        : config.ThoiGianChoBookGioHangRieng ?? 0;

                    var duration = now - phieu.NgayLap;

                    if (duration.HasValue)
                    {
                        if (duration.Value.TotalMinutes > allowedMinutes)
                        {
                            phieu.IsHetHieuLuc = true;
                            hasChanges = true;

                            var sanPham = await dbContext.DaDanhMucSanPhams
                            .FirstOrDefaultAsync(sp => sp.MaSanPham == phieu.MaCanHo);
                            if (sanPham != null)
                                sanPham.HienTrangKd = "MB";

                            _logger.LogInformation($"Phiếu {phieu.MaPhieu} đã quá hạn ({duration.Value.TotalMinutes} phút > {allowedMinutes})");
                        }
                    }
                }
                if (hasChanges)
                {
                    await dbContext.SaveChangesAsync();
                }
                return hasChanges;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi cập nhật trạng thái hiệu lực phiếu đăng ký.");
                return false;
            }
        }

        private async Task<List<DangKyCountdownDto>> BuildDangKyCountdownsAsync(AppDbContext db, DateTime nowUtc)
        {
            var items = new List<DangKyCountdownDto>();

            var pendingDangKys = await db.BhPhieuDangKiChonCans
                .Where(p => (p.IsXacNhan ?? false) == false && (p.IsHetHieuLuc ?? false) == false)
                .Select(p => new {
                    p.MaPhieu,
                    p.MaCanHo,
                    p.MaDuAn,
                    p.NgayLap,
                    p.MaGioHang
                })
                .ToListAsync();

            // cache cấu hình & giỏ hàng để giảm query lặp
            var byGioHang = pendingDangKys.Select(x => x.MaGioHang).Distinct().ToList();
            var gioHangs = await db.BhGioHangs.Where(g => byGioHang.Contains(g.MaPhieu)).ToListAsync();
            var byDuAn = pendingDangKys.Select(x => x.MaDuAn).Distinct().ToList();
            var configs = await db.DaDanhMucDuAnCauHinhChungs.Where(c => byDuAn.Contains(c.MaDuAn)).ToListAsync();

            foreach (var p in pendingDangKys)
            {
                var gioHang = gioHangs.FirstOrDefault(g => g.MaPhieu == p.MaGioHang);
                var cfg = configs.FirstOrDefault(c => c.MaDuAn == p.MaDuAn);
                if (gioHang == null || cfg == null || p.NgayLap == default) continue;

                var allowedMinutes = (gioHang.LoaiGioHang == true)
                    ? (cfg.ThoiGianChoBookGioHangChung ?? 0)
                    : (cfg.ThoiGianChoBookGioHangRieng ?? 0);

                var expireAtLocal = p.NgayLap.Value.AddMinutes(allowedMinutes);
                var expireAtUtc = DateTime.SpecifyKind(expireAtLocal, DateTimeKind.Local).ToUniversalTime();
                var remaining = (int)Math.Max(0, (expireAtUtc - nowUtc).TotalSeconds);

                items.Add(new DangKyCountdownDto
                {
                    MaPhieu = p.MaPhieu!,
                    MaCanHo = p.MaCanHo!,
                    ExpireAtUtc = expireAtUtc,
                    RemainingSeconds = remaining
                });
            }
            return items;
        }
        #endregion


    }
}
