using Microsoft.EntityFrameworkCore;
using VTTGROUP.Infrastructure.Database2.Entities;

namespace VTTGROUP.Infrastructure.Database2;

public partial class ErpBinhPhuCoDbContext : DbContext
{
    public ErpBinhPhuCoDbContext(DbContextOptions<ErpBinhPhuCoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<KT_PhieuDuTruChi> KT_PhieuDuTruChi { get; set; }

    public virtual DbSet<TC_PhieuPhaiThu> TC_PhieuPhaiThu { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AI");

        modelBuilder.Entity<KT_PhieuDuTruChi>(entity =>
        {
            entity.HasKey(e => e.maPhieuDuTruChi);

            entity.Property(e => e.maPhieuDuTruChi)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.KemTheo)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.chuoiHoadon).IsUnicode(false);
            entity.Property(e => e.diaChi)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.loaiChi)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.loaiDuTruChi)
                .HasDefaultValue((short)0)
                .HasComment("0: chi khác, 1: chi đơn hàng, 2: chi hợp đồng");
            entity.Property(e => e.maChiNhanh)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.maCongTrinh)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.maCongViecChungTu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.maDoiTuong)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.maLoaiChiPhi)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.maNganHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.maPhuongThucThanhToan)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.maTienTe)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.maTieuChiDuyetChi)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.ngayHachToan)
                .HasDefaultValueSql("('')")
                .HasColumnType("datetime");
            entity.Property(e => e.ngayLap)
                .HasDefaultValueSql("('')")
                .HasColumnType("datetime");
            entity.Property(e => e.nguoiDong)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.nguoiLap).HasMaxLength(50);
            entity.Property(e => e.nguoiTiepNhan)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.noiDung).HasDefaultValue("");
            entity.Property(e => e.soHopDong)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.soPhieuDuTruChi)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.soPhieuThanhToan)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.soTaiKhoan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.soTien).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.tenDoiTuong)
                .HasMaxLength(200)
                .HasDefaultValue("");
            entity.Property(e => e.tenTaiKhoan).HasMaxLength(250);
            entity.Property(e => e.trangThai).HasDefaultValue(false);
            entity.Property(e => e.tyGia)
                .HasDefaultValue(0m)
                .HasColumnType("numeric(10, 4)");
        });

        modelBuilder.Entity<TC_PhieuPhaiThu>(entity =>
        {
            entity.HasKey(e => e.maPhieu).HasName("PK__TC_Phieu__49A5B11FECA8AC64");

            entity.Property(e => e.maPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.diaChi).HasMaxLength(500);
            entity.Property(e => e.maCongTrinh)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.maCongViecChungTu).HasMaxLength(50);
            entity.Property(e => e.maDoiTuong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.maNhanVienDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ngayDong).HasColumnType("datetime");
            entity.Property(e => e.ngayHachToan).HasColumnType("datetime");
            entity.Property(e => e.ngayLap).HasColumnType("datetime");
            entity.Property(e => e.nguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.soHopDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.soPhieuThanhToan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.soTien).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.tenDoiTuong).HasMaxLength(250);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
