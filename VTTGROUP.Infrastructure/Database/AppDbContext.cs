using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VTTGROUP.Infrastructure.Database;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BhChinhSachThanhToan> BhChinhSachThanhToans { get; set; }

    public virtual DbSet<BhChinhSachThanhToanChiTiet> BhChinhSachThanhToanChiTiets { get; set; }

    public virtual DbSet<BhChinhSachThanhToanChiTietHopDong> BhChinhSachThanhToanChiTietHopDongs { get; set; }

    public virtual DbSet<BhChinhSachThanhToanChiTietHopDongCanTruDatCoc> BhChinhSachThanhToanChiTietHopDongCanTruDatCocs { get; set; }

    public virtual DbSet<BhGioHang> BhGioHangs { get; set; }

    public virtual DbSet<BhGioHangCanHo> BhGioHangCanHos { get; set; }

    public virtual DbSet<BhGioHangLichSuLgh> BhGioHangLichSuLghs { get; set; }

    public virtual DbSet<BhHinhThucThanhLy> BhHinhThucThanhLies { get; set; }

    public virtual DbSet<BhKeHoachBanHang> BhKeHoachBanHangs { get; set; }

    public virtual DbSet<BhKeHoachBanHangDotMoBan> BhKeHoachBanHangDotMoBans { get; set; }

    public virtual DbSet<BhKeHoachBanHangDotMoBanCanHo> BhKeHoachBanHangDotMoBanCanHos { get; set; }

    public virtual DbSet<BhKeHoachBanHangDotMoBanGiaBan> BhKeHoachBanHangDotMoBanGiaBans { get; set; }

    public virtual DbSet<BhPhieuDangKiChonCan> BhPhieuDangKiChonCans { get; set; }

    public virtual DbSet<BhPhieuDangKiChonCanCsbh> BhPhieuDangKiChonCanCsbhs { get; set; }

    public virtual DbSet<BhPhieuDangKiChonCanLichSuThayDoiKhachHang> BhPhieuDangKiChonCanLichSuThayDoiKhachHangs { get; set; }

    public virtual DbSet<BhPhieuDatCoc> BhPhieuDatCocs { get; set; }

    public virtual DbSet<BhPhieuDatCocTienDoThanhToan> BhPhieuDatCocTienDoThanhToans { get; set; }

    public virtual DbSet<BhPhieuDuyetGiaChinhSachThanhToan> BhPhieuDuyetGiaChinhSachThanhToans { get; set; }

    public virtual DbSet<BhPhieuDuyetGium> BhPhieuDuyetGia { get; set; }

    public virtual DbSet<BhPhieuGiuCho> BhPhieuGiuChos { get; set; }

    public virtual DbSet<BhPhieuGiuChoStt> BhPhieuGiuChoStts { get; set; }

    public virtual DbSet<BhPhieuThanhLyDatCoc> BhPhieuThanhLyDatCocs { get; set; }

    public virtual DbSet<BhPhieuThanhLyDatCocChiTiet> BhPhieuThanhLyDatCocChiTiets { get; set; }

    public virtual DbSet<DaChinhSachBanHang> DaChinhSachBanHangs { get; set; }

    public virtual DbSet<DaChinhSachBanHangChiTiet> DaChinhSachBanHangChiTiets { get; set; }

    public virtual DbSet<DaDanhMucBlock> DaDanhMucBlocks { get; set; }

    public virtual DbSet<DaDanhMucDotMoBan> DaDanhMucDotMoBans { get; set; }

    public virtual DbSet<DaDanhMucDuAn> DaDanhMucDuAns { get; set; }

    public virtual DbSet<DaDanhMucDuAnCauHinhChung> DaDanhMucDuAnCauHinhChungs { get; set; }

    public virtual DbSet<DaDanhMucHuong> DaDanhMucHuongs { get; set; }

    public virtual DbSet<DaDanhMucLoaiCanHo> DaDanhMucLoaiCanHos { get; set; }

    public virtual DbSet<DaDanhMucLoaiGoc> DaDanhMucLoaiGocs { get; set; }

    public virtual DbSet<DaDanhMucLoaiSanPham> DaDanhMucLoaiSanPhams { get; set; }

    public virtual DbSet<DaDanhMucLoaiThietKe> DaDanhMucLoaiThietKes { get; set; }

    public virtual DbSet<DaDanhMucLoaiThietKeHinhAnh> DaDanhMucLoaiThietKeHinhAnhs { get; set; }

    public virtual DbSet<DaDanhMucSanPham> DaDanhMucSanPhams { get; set; }

    public virtual DbSet<DaDanhMucTang> DaDanhMucTangs { get; set; }

    public virtual DbSet<DaDanhMucTienDoKyThanhToan> DaDanhMucTienDoKyThanhToans { get; set; }

    public virtual DbSet<DaDanhMucViTri> DaDanhMucViTris { get; set; }

    public virtual DbSet<DaDanhMucView> DaDanhMucViews { get; set; }

    public virtual DbSet<DaDanhMucViewMatKhoi> DaDanhMucViewMatKhois { get; set; }

    public virtual DbSet<DaDanhMucViewTruc> DaDanhMucViewTrucs { get; set; }

    public virtual DbSet<DaLoaiDienTich> DaLoaiDienTiches { get; set; }

    public virtual DbSet<DmSanGiaoDich> DmSanGiaoDiches { get; set; }

    public virtual DbSet<DmSanGiaoDichDuAn> DmSanGiaoDichDuAns { get; set; }

    public virtual DbSet<HtBuocDuyet> HtBuocDuyets { get; set; }

    public virtual DbSet<HtDanhMucHinhThucThanhToan> HtDanhMucHinhThucThanhToans { get; set; }

    public virtual DbSet<HtDanhMucTinhTrangSanPham> HtDanhMucTinhTrangSanPhams { get; set; }

    public virtual DbSet<HtDmhinhThucKhuyenMai> HtDmhinhThucKhuyenMais { get; set; }

    public virtual DbSet<HtDmloaiDieuKienKhuyenMai> HtDmloaiDieuKienKhuyenMais { get; set; }

    public virtual DbSet<HtDmnguoiDuyet> HtDmnguoiDuyets { get; set; }

    public virtual DbSet<HtDmquocGium> HtDmquocGia { get; set; }

    public virtual DbSet<HtDmtrangThaiThanhToan> HtDmtrangThaiThanhToans { get; set; }

    public virtual DbSet<HtEmailHistory> HtEmailHistories { get; set; }

    public virtual DbSet<HtFileDinhKem> HtFileDinhKems { get; set; }

    public virtual DbSet<HtGhiNhanLog> HtGhiNhanLogs { get; set; }

    public virtual DbSet<HtHienTrangKinhDoanh> HtHienTrangKinhDoanhs { get; set; }

    public virtual DbSet<HtHistoryLog> HtHistoryLogs { get; set; }

    public virtual DbSet<HtLoaiMauIn> HtLoaiMauIns { get; set; }

    public virtual DbSet<HtMauIn> HtMauIns { get; set; }

    public virtual DbSet<HtQuyTrinhDuyet> HtQuyTrinhDuyets { get; set; }

    public virtual DbSet<HtQuyTrinhDuyetBuocDuyet> HtQuyTrinhDuyetBuocDuyets { get; set; }

    public virtual DbSet<HtQuyTrinhDuyetDuAn> HtQuyTrinhDuyetDuAns { get; set; }

    public virtual DbSet<HtSendEmail> HtSendEmails { get; set; }

    public virtual DbSet<HtSendEmailAttachment> HtSendEmailAttachments { get; set; }

    public virtual DbSet<HtTemplate> HtTemplates { get; set; }

    public virtual DbSet<HtThongTinCongTy> HtThongTinCongTies { get; set; }

    public virtual DbSet<HtTrangThaiDuyet> HtTrangThaiDuyets { get; set; }

    public virtual DbSet<KdChuyenNhuong> KdChuyenNhuongs { get; set; }

    public virtual DbSet<KdChuyenNhuongKhachHang> KdChuyenNhuongKhachHangs { get; set; }

    public virtual DbSet<KdHopDong> KdHopDongs { get; set; }

    public virtual DbSet<KdHopDongKhachHang> KdHopDongKhachHangs { get; set; }

    public virtual DbSet<KdHopDongTienDoThanhToan> KdHopDongTienDoThanhToans { get; set; }

    public virtual DbSet<KdPhieuDeNghiHoanTienBooking> KdPhieuDeNghiHoanTienBookings { get; set; }

    public virtual DbSet<KdPhieuDeNghiHoanTienBookingSoPhieuBooking> KdPhieuDeNghiHoanTienBookingSoPhieuBookings { get; set; }

    public virtual DbSet<KdPhieuTongHopBooking> KdPhieuTongHopBookings { get; set; }

    public virtual DbSet<KdPhieuTongHopBookingPhieuBooking> KdPhieuTongHopBookingPhieuBookings { get; set; }

    public virtual DbSet<KdPhuLucHopDong> KdPhuLucHopDongs { get; set; }

    public virtual DbSet<KdPhuLucHopDongTienDoThanhToan> KdPhuLucHopDongTienDoThanhToans { get; set; }

    public virtual DbSet<KdThanhLyHopDong> KdThanhLyHopDongs { get; set; }

    public virtual DbSet<KhDmdoiTuongKhachHang> KhDmdoiTuongKhachHangs { get; set; }

    public virtual DbSet<KhDmkhachHang> KhDmkhachHangs { get; set; }

    public virtual DbSet<KhDmkhachHangChiTiet> KhDmkhachHangChiTiets { get; set; }

    public virtual DbSet<KhDmkhachHangHinhAnhDinhKem> KhDmkhachHangHinhAnhDinhKems { get; set; }

    public virtual DbSet<KhDmkhachHangNguon> KhDmkhachHangNguons { get; set; }

    public virtual DbSet<KhDmkhachHangTam> KhDmkhachHangTams { get; set; }

    public virtual DbSet<KhDmloaiCard> KhDmloaiCards { get; set; }

    public virtual DbSet<KhDmnguonKhachHang> KhDmnguonKhachHangs { get; set; }

    public virtual DbSet<KtPhieuCongNoPhaiThu> KtPhieuCongNoPhaiThus { get; set; }

    public virtual DbSet<KtPhieuCongNoPhaiTra> KtPhieuCongNoPhaiTras { get; set; }

    public virtual DbSet<KtPhieuXacNhanThanhToan> KtPhieuXacNhanThanhToans { get; set; }

    public virtual DbSet<KtPhieuXacNhanThanhToanChiTiet> KtPhieuXacNhanThanhToanChiTiets { get; set; }

    public virtual DbSet<KtPhieuXacNhanThanhToanPhieuChuyenDoi> KtPhieuXacNhanThanhToanPhieuChuyenDois { get; set; }

    public virtual DbSet<KtTinhLaiQuaHanNhap> KtTinhLaiQuaHanNhaps { get; set; }

    public virtual DbSet<KvDmkhuVuc> KvDmkhuVucs { get; set; }

    public virtual DbSet<TblCongviec> TblCongviecs { get; set; }

    public virtual DbSet<TblCongviecvavuviec> TblCongviecvavuviecs { get; set; }

    public virtual DbSet<TblDuan> TblDuans { get; set; }

    public virtual DbSet<TblDuanFiledinhkem> TblDuanFiledinhkems { get; set; }

    public virtual DbSet<TblDuanLichsuhoatdong> TblDuanLichsuhoatdongs { get; set; }

    public virtual DbSet<TblDuanNhanvien> TblDuanNhanviens { get; set; }

    public virtual DbSet<TblDuanThaoluan> TblDuanThaoluans { get; set; }

    public virtual DbSet<TblDuanThaoluanFiledinhkem> TblDuanThaoluanFiledinhkems { get; set; }

    public virtual DbSet<TblDuanTiendoduan> TblDuanTiendoduans { get; set; }

    public virtual DbSet<TblDuanTrangthai> TblDuanTrangthais { get; set; }

    public virtual DbSet<TblNganhang> TblNganhangs { get; set; }

    public virtual DbSet<TblNhanvien> TblNhanviens { get; set; }

    public virtual DbSet<TblNhanvienChucvu> TblNhanvienChucvus { get; set; }

    public virtual DbSet<TblNhanvienDantoc> TblNhanvienDantocs { get; set; }

    public virtual DbSet<TblNhanvienHochieu> TblNhanvienHochieus { get; set; }

    public virtual DbSet<TblNhanvienNganhang> TblNhanvienNganhangs { get; set; }

    public virtual DbSet<TblNhanvienNganhnghe> TblNhanvienNganhnghes { get; set; }

    public virtual DbSet<TblNhanvienNguoiphuthuoc> TblNhanvienNguoiphuthuocs { get; set; }

    public virtual DbSet<TblNhanvienNguoiphuthuocMoiquanhe> TblNhanvienNguoiphuthuocMoiquanhes { get; set; }

    public virtual DbSet<TblNhanvienPhongban> TblNhanvienPhongbans { get; set; }

    public virtual DbSet<TblNhanvienTongiao> TblNhanvienTongiaos { get; set; }

    public virtual DbSet<TblNhanvienTrinhdo> TblNhanvienTrinhdos { get; set; }

    public virtual DbSet<TblNhomuser> TblNhomusers { get; set; }

    public virtual DbSet<TblRefreshtoken> TblRefreshtokens { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserthuocnhom> TblUserthuocnhoms { get; set; }

    public virtual DbSet<TblVuviec> TblVuviecs { get; set; }

    public virtual DbSet<TblVuvieccuacongviec> TblVuvieccuacongviecs { get; set; }

    public virtual DbSet<TcDmtienDoThiCongDuAn> TcDmtienDoThiCongDuAns { get; set; }

    public virtual DbSet<TcTienDoThiCongDuAn> TcTienDoThiCongDuAns { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AI");

        modelBuilder.Entity<BhChinhSachThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaCstt).HasName("PK__DA_DMChi__90380CE295565779");

            entity.ToTable("BH_ChinhSachThanhToan");

            entity.Property(e => e.MaCstt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenCstt)
                .HasMaxLength(250)
                .HasColumnName("TenCSTT");
            entity.Property(e => e.TyLeChietKhau).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<BhChinhSachThanhToanChiTiet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_DA_DanhMucChinhSachThanhToanChiTiet");

            entity.ToTable("BH_ChinhSachThanhToanChiTiet");

            entity.Property(e => e.DotTt)
                .HasComment("Tự tăng theo từng chính sách")
                .HasColumnName("DotTT");
            entity.Property(e => e.IsCongNoDc).HasColumnName("IsCongNoDC");
            entity.Property(e => e.MaCstt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
            entity.Property(e => e.MaKyTt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaKyTT");
            entity.Property(e => e.NoiDungTt).HasColumnName("NoiDungTT");
            entity.Property(e => e.TyLeTtdatCoc)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeTTDatCoc");
            entity.Property(e => e.TyLeTtdatCocVat)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("TyLeTTDatCocVAT");
        });

        modelBuilder.Entity<BhChinhSachThanhToanChiTietHopDong>(entity =>
        {
            entity.ToTable("BH_ChinhSachThanhToanChiTiet_HopDong");

            entity.Property(e => e.DotTt)
                .HasComment("Tự tăng theo từng chính sách")
                .HasColumnName("DotTT");
            entity.Property(e => e.IsCongNo).HasComment("DA_DanhMucTienDoKyThanhToan");
            entity.Property(e => e.MaCstt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
            entity.Property(e => e.MaKyTt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaKyTT");
            entity.Property(e => e.NoiDungTt).HasColumnName("NoiDungTT");
            entity.Property(e => e.TyLeTt)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeTT");
            entity.Property(e => e.TyLeTtvat)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("TyLeTTVAT");
        });

        modelBuilder.Entity<BhChinhSachThanhToanChiTietHopDongCanTruDatCoc>(entity =>
        {
            entity.HasKey(e => new { e.MaCstt, e.DotTthopDong, e.DotTtdatCoc });

            entity.ToTable("BH_ChinhSachThanhToanChiTiet_HopDong_CanTruDatCoc");

            entity.Property(e => e.MaCstt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
            entity.Property(e => e.DotTthopDong).HasColumnName("DotTTHopDong");
            entity.Property(e => e.DotTtdatCoc).HasColumnName("DotTTDatCoc");
        });

        modelBuilder.Entity<BhGioHang>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("BH_GioHang");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.LoaiGioHang).HasComment("1: giỏ hàng riêng, 0: giỏ hàng chung");
            entity.Property(e => e.MaDotMoBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuDuyetGia)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuKh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuKH");
            entity.Property(e => e.MaSanGiaoDich)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSoGioHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayDong).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiDong)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BhGioHangCanHo>(entity =>
        {
            entity.ToTable("BH_GioHang_CanHo");

            entity.Property(e => e.DienTichCanHo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DienTichPhanBo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanSauPhanBo).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.HeSoCanHo).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.MaCanHo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuGioHang)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BhGioHangLichSuLgh>(entity =>
        {
            entity.ToTable("BH_GioHang_LichSuLGH");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSanGiaoDichCu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSanGiaoDichMoi)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
        });

        modelBuilder.Entity<BhHinhThucThanhLy>(entity =>
        {
            entity.HasKey(e => e.MaHttl);

            entity.ToTable("BH_HinhThucThanhLy");

            entity.Property(e => e.MaHttl)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaHTTL");
            entity.Property(e => e.TenHttl)
                .HasMaxLength(500)
                .HasColumnName("TenHTTL");
        });

        modelBuilder.Entity<BhKeHoachBanHang>(entity =>
        {
            entity.HasKey(e => e.MaPhieuKh);

            entity.ToTable("BH_KeHoachBanHang");

            entity.Property(e => e.MaPhieuKh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuKH");
            entity.Property(e => e.DoanhThuDuKien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DonGiaTb)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DonGiaTB");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap).HasMaxLength(100);
        });

        modelBuilder.Entity<BhKeHoachBanHangDotMoBan>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuKh, e.MaDotMoBan }).HasName("PK__BH_KeHoa__64869D1FCF63CD0A");

            entity.ToTable("BH_KeHoachBanHang_DotMoBan");

            entity.Property(e => e.MaPhieuKh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuKH");
            entity.Property(e => e.MaDotMoBan)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BhKeHoachBanHangDotMoBanCanHo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BH_KeHoa__3214EC07178DAB7B");

            entity.ToTable("BH_KeHoachBanHang_DotMoBanCanHo");

            entity.Property(e => e.DienTichCanHo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.HeSoCanHo).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.MaCanHo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDotMoBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuKh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuKH");
        });

        modelBuilder.Entity<BhKeHoachBanHangDotMoBanGiaBan>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuKh, e.MaDotMoBan }).HasName("PK__BH_KeHoa__64869D1F36F6D83E");

            entity.ToTable("BH_KeHoachBanHang_DotMoBanGiaBan");

            entity.Property(e => e.MaPhieuKh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuKH");
            entity.Property(e => e.MaDotMoBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DonGiaTbdot)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DonGiaTBDot");
            entity.Property(e => e.IsXacNhan).HasDefaultValue(false);
        });

        modelBuilder.Entity<BhPhieuDangKiChonCan>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("BH_PhieuDangKiChonCan");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DienTichSanVuon).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichThongThuy).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichTimTuong).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DotMoBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanChinhThuc).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanChinhThucTruocThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanTheoCstt)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("GiaBanTheoCSTT");
            entity.Property(e => e.GiaBanTruocThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.MaCanHo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaChinhSachTt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaChinhSachTT");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaGioHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHangTam)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuBooking)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NhanVienMoiGioi).HasMaxLength(250);
            entity.Property(e => e.PhuongThucTinhChietKhauKm)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("PhuongThucTinhChietKhauKM");
            entity.Property(e => e.SanGiaoDich)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BhPhieuDangKiChonCanCsbh>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuDangKy, e.MaCsbh, e.LoaiCs });

            entity.ToTable("BH_PhieuDangKiChonCan_CSBH");

            entity.Property(e => e.MaPhieuDangKy)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaCsbh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSBH");
            entity.Property(e => e.LoaiCs)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasComment("CSKM: Chính sách khuyến mãi (CS bán hàng); CSTT: Chính sách thanh toán")
                .HasColumnName("LoaiCS");
            entity.Property(e => e.GiaTriCk)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("GiaTriCK");
            entity.Property(e => e.ThanhTienKmgiaBanChinhThucDuocChon)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("ThanhTienKMGiaBanChinhThucDuocChon");
            entity.Property(e => e.ThanhTienKmgiaBanDuocChon)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("ThanhTienKMGiaBanDuocChon");
        });

        modelBuilder.Entity<BhPhieuDangKiChonCanLichSuThayDoiKhachHang>(entity =>
        {
            entity.ToTable("BH_PhieuDangKiChonCan_LichSuThayDoiKhachHang");

            entity.Property(e => e.MaKhachHangTam)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuBooking)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuDangKy)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
            entity.Property(e => e.NguoiCapNhat)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BhPhieuDatCoc>(entity =>
        {
            entity.HasKey(e => e.MaPhieuDc).HasName("PK__PH_Phieu__27792E6CEF2C7085");

            entity.ToTable("BH_PhieuDatCoc");

            entity.Property(e => e.MaPhieuDc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuDC");
            entity.Property(e => e.DienTichLotLong).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichSanVuon).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichTimTuong).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DonGiaDat).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanSauThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanTienThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanTruocThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaCanHoSauThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaCanHoTruocThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaDat).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaTriCk)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("GiaTriCK");
            entity.Property(e => e.IdkhachHangCt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IDKhachHangCT");
            entity.Property(e => e.MaCanHo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaChinhSachTt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaChinhSachTT");
            entity.Property(e => e.MaDotMoBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaMauIn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuDangKy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuDatCocKyLai)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayKi).HasColumnType("datetime");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayXacNhan).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NguoiXacNhan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoPhieuDc)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("SoPhieuDC");
            entity.Property(e => e.TienQuyBaoTri).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TyLeCk)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeCK");
            entity.Property(e => e.TyLeQuyBaoTri).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TyLeThueVat)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeThueVAT");
        });

        modelBuilder.Entity<BhPhieuDatCocTienDoThanhToan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PH_Phieu__5BC72032E0D9A759");

            entity.ToTable("BH_PhieuDatCoc_TienDoThanhToan");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DotTt).HasColumnName("DotTT");
            entity.Property(e => e.MaCstt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
            entity.Property(e => e.MaKyTt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaKyTT");
            entity.Property(e => e.MaPhieuDc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuDC");
            entity.Property(e => e.NgayThanhToan).HasColumnType("datetime");
            entity.Property(e => e.NoiDungTt).HasColumnName("NoiDungTT");
            entity.Property(e => e.SoTienCanTruDaTt)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("SoTienCanTruDaTT");
            entity.Property(e => e.SoTienChuyenDoiBooking).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SoTienPhaiThanhToan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SoTienThanhToan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TyLeTt)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeTT");
            entity.Property(e => e.TyLeTtvat)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeTTVAT");
        });

        modelBuilder.Entity<BhPhieuDuyetGiaChinhSachThanhToan>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuDuyetGia, e.MaCstt }).HasName("PK_BH_PhieuDuyetGia_ChinhSachThanhToan_1");

            entity.ToTable("BH_PhieuDuyetGia_ChinhSachThanhToan");

            entity.Property(e => e.MaPhieuDuyetGia)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaCstt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
        });

        modelBuilder.Entity<BhPhieuDuyetGium>(entity =>
        {
            entity.HasKey(e => e.MaPhieu).HasName("PK__BH_Phieu__2660BFE07B0B0214");

            entity.ToTable("BH_PhieuDuyetGia");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GiaBanKeHoach).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaBanThucTe).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaDotMoBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuKh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuKH");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap).HasMaxLength(100);
            entity.Property(e => e.TyLeChuyenDoi).HasColumnType("numeric(18, 2)");
        });

        modelBuilder.Entity<BhPhieuGiuCho>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("BH_PhieuGiuCho");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DotMoBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsxacNhan).HasColumnName("ISXacNhan");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHangTam)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiThietKe)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.MaMatKhoi)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuTh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuTH");
            entity.Property(e => e.MaSanMoiGioi)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoPhieu)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasComment("tự điền theo quy tắc: Mã dự án +'-'+Mã Đợt +'-' +'4 stt tăng của đợt'");
            entity.Property(e => e.SoTienGiuCho).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SoTtbooking).HasColumnName("SoTTBooking");
            entity.Property(e => e.TenNhanVienMg)
                .HasMaxLength(250)
                .HasColumnName("TenNhanVienMG");
        });

        modelBuilder.Entity<BhPhieuGiuChoStt>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuGiuCho, e.MaDotBanHang });

            entity.ToTable("BH_PhieuGiuCho_STT");

            entity.Property(e => e.MaPhieuGiuCho)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDotBanHang)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.SoTtbooking).HasColumnName("SoTTBooking");
        });

        modelBuilder.Entity<BhPhieuThanhLyDatCoc>(entity =>
        {
            entity.HasKey(e => e.MaPhieuThanhLy).HasName("PK__BH_Phieu__B3C949152CC3FB8F");

            entity.ToTable("BH_PhieuThanhLyDatCoc");

            entity.Property(e => e.MaPhieuThanhLy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.HinhThucThanhLy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LyDoThanhLy).HasMaxLength(255);
            entity.Property(e => e.MaPhieuDangKy)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuDatCoc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NgayThanhLy).HasColumnType("datetime");
            entity.Property(e => e.NguoiThanhLy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhiPhat).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SoPhieuThanhLy).HasMaxLength(250);
            entity.Property(e => e.TienHoanLai).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<BhPhieuThanhLyDatCocChiTiet>(entity =>
        {
            entity.ToTable("BH_PhieuThanhLyDatCoc_ChiTiet");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DotTt).HasColumnName("DotTT");
            entity.Property(e => e.MaCstt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
            entity.Property(e => e.MaKyTt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaKyTT");
            entity.Property(e => e.MaPhieuTl)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuTL");
            entity.Property(e => e.NoiDungTt).HasColumnName("NoiDungTT");
            entity.Property(e => e.SoTienConLai).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SoTienDaTt)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("SoTienDaTT");
            entity.Property(e => e.SoTienThanhToan).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<DaChinhSachBanHang>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("DA_ChinhSachBanHang");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DotBanHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap).HasMaxLength(100);
        });

        modelBuilder.Entity<DaChinhSachBanHangChiTiet>(entity =>
        {
            entity.ToTable("DA_ChinhSachBanHang_ChiTiet");

            entity.Property(e => e.DenNgay).HasColumnType("datetime");
            entity.Property(e => e.GiaTriKm)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("GiaTriKM");
            entity.Property(e => e.MaCsbh)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MaCSBH");
            entity.Property(e => e.MaHinhThucKm)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MaHinhThucKM");
            entity.Property(e => e.MaLoaiDieuKienKm)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaLoaiDieuKienKM");
            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoLuongKm).HasColumnName("SoLuongKM");
            entity.Property(e => e.TenCsbh)
                .HasMaxLength(500)
                .HasColumnName("TenCSBH");
            entity.Property(e => e.TuNgay).HasColumnType("datetime");
        });

        modelBuilder.Entity<DaDanhMucBlock>(entity =>
        {
            entity.HasKey(e => new { e.MaBlock, e.MaDuAn });

            entity.ToTable("DA_DanhMucBlock");

            entity.HasIndex(e => new { e.MaDuAn, e.MaBlock }, "IX_DA_DanhMucBlock_MaDuAn_MaBlock");

            entity.Property(e => e.MaBlock)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenBlock).HasMaxLength(250);
        });

        modelBuilder.Entity<DaDanhMucDotMoBan>(entity =>
        {
            entity.HasKey(e => e.MaDotMoBan).HasName("PK__DA_DanhM__C8875F4F97D3FB01");

            entity.ToTable("DA_DanhMucDotMoBan");

            entity.Property(e => e.MaDotMoBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaMau)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenDotMoBan).HasMaxLength(255);
        });

        modelBuilder.Entity<DaDanhMucDuAn>(entity =>
        {
            entity.HasKey(e => e.MaDuAn);

            entity.ToTable("DA_DanhMucDuAn");

            entity.HasIndex(e => e.MaDuAn, "IX_DA_DanhMucDuAn_MaDuAn");

            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenDuAn).HasMaxLength(200);
            entity.Property(e => e.TongDienTichDuAn).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasComment("1: Đang mở bán, 2: Sắp mở bán, 3: Đóng dự án");
        });

        modelBuilder.Entity<DaDanhMucDuAnCauHinhChung>(entity =>
        {
            entity.ToTable("DA_DanhMucDuAn_CauHinhChung");

            entity.Property(e => e.ChenhLechGiaTran).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.DonGiaDat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DonGiaTb)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DonGiaTB");
            entity.Property(e => e.IsKichHoatGh).HasColumnName("IsKichHoatGH");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhuongThucTinhChietKhauKm)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasComment("1: Chiết khấu thẳng; 2: Chiết khấu theo thứ tự")
                .HasColumnName("PhuongThucTinhChietKhauKM");
            entity.Property(e => e.SaiSoDoanhThuChoPhepKhbh)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("SaiSoDoanhThuChoPhepKHBH");
            entity.Property(e => e.SoLuongUserSanGd).HasColumnName("SoLuongUserSanGD");
            entity.Property(e => e.SoTienGiuCho).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ThoiGianChoBookGioHangChung).HasComment("Số phút");
            entity.Property(e => e.ThoiGianChoBookGioHangRieng).HasComment("Số phút");
            entity.Property(e => e.TyLeLaiQuaHan).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TyLeQuyBaoTri).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TyLeThueVat)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeThueVAT");
        });

        modelBuilder.Entity<DaDanhMucHuong>(entity =>
        {
            entity.HasKey(e => new { e.MaDuAn, e.MaHuong });

            entity.ToTable("DA_DanhMucHuong");

            entity.Property(e => e.MaDuAn)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaHuong)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.HeSo).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.TenHuong).HasMaxLength(500);
        });

        modelBuilder.Entity<DaDanhMucLoaiCanHo>(entity =>
        {
            entity.HasKey(e => new { e.MaLoaiCanHo, e.MaDuAn });

            entity.ToTable("DA_DanhMucLoaiCanHo");

            entity.HasIndex(e => new { e.MaDuAn, e.MaLoaiCanHo }, "IX_DA_DanhMucLoaiCanHo_MaDuAn");

            entity.Property(e => e.MaLoaiCanHo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DienTich).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichLotLong).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichSanVuon).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.HeSoDienTich).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.HinhAnh)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiThietKe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenLoaiCanHo).HasMaxLength(250);
        });

        modelBuilder.Entity<DaDanhMucLoaiGoc>(entity =>
        {
            entity.HasKey(e => new { e.MaLoaiGoc, e.MaDuAn });

            entity.ToTable("DA_DanhMucLoaiGoc");

            entity.Property(e => e.MaLoaiGoc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HeSoGoc).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.TenLoaiGoc).HasMaxLength(250);
        });

        modelBuilder.Entity<DaDanhMucLoaiSanPham>(entity =>
        {
            entity.HasKey(e => e.MaLoaiSanPham);

            entity.ToTable("DA_DanhMucLoaiSanPham");

            entity.Property(e => e.MaLoaiSanPham)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TenLoaiSanPham).HasMaxLength(150);
        });

        modelBuilder.Entity<DaDanhMucLoaiThietKe>(entity =>
        {
            entity.HasKey(e => new { e.MaLoaiThietKe, e.MaDuAn });

            entity.ToTable("DA_DanhMucLoaiThietKe");

            entity.HasIndex(e => new { e.MaDuAn, e.MaLoaiThietKe }, "IX_DA_DanhMucLoaiThietKe_MaDuAn");

            entity.Property(e => e.MaLoaiThietKe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenLoaiThietKe).HasMaxLength(500);
        });

        modelBuilder.Entity<DaDanhMucLoaiThietKeHinhAnh>(entity =>
        {
            entity.ToTable("DA_DanhMucLoaiThietKe_HinhAnh");

            entity.Property(e => e.DuongDanLuuFileAnh).HasMaxLength(500);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiThietKe)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DaDanhMucSanPham>(entity =>
        {
            entity.HasKey(e => new { e.MaSanPham, e.MaDuAn });

            entity.ToTable("DA_DanhMucSanPham");

            entity.Property(e => e.MaSanPham)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DienTichSanVuon).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichThongThuy).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichTimTuong).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.HeSoCanHo).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.HienTrangKd)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("HienTrangKD");
            entity.Property(e => e.LoaiSanPham)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaBlock)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiCan)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiDienTich)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiLayout)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaTang)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaTruc)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenSanPham).HasMaxLength(250);
        });

        modelBuilder.Entity<DaDanhMucTang>(entity =>
        {
            entity.HasKey(e => new { e.MaTang, e.MaBlock, e.MaDuAn });

            entity.ToTable("DA_DanhMucTang");

            entity.HasIndex(e => new { e.MaDuAn, e.MaBlock, e.MaTang }, "IX_DA_DanhMucTang_MaDuAn_MaBlock_MaTang");

            entity.Property(e => e.MaTang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaBlock)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HeSoTang).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.Stttang).HasColumnName("STTTang");
            entity.Property(e => e.TenTang).HasMaxLength(250);
        });

        modelBuilder.Entity<DaDanhMucTienDoKyThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaKyTt).HasName("PK_HT_DanhMucTienDoKyThanhToan");

            entity.ToTable("DA_DanhMucTienDoKyThanhToan");

            entity.Property(e => e.MaKyTt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("MaKyTT");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayDuKien).HasColumnType("datetime");
            entity.Property(e => e.TenKyTt)
                .HasMaxLength(250)
                .HasColumnName("TenKyTT");
            entity.Property(e => e.ThuTuHt).HasColumnName("ThuTuHT");
        });

        modelBuilder.Entity<DaDanhMucViTri>(entity =>
        {
            entity.HasKey(e => new { e.MaViTri, e.MaDuAn });

            entity.ToTable("DA_DanhMucViTri");

            entity.Property(e => e.MaViTri)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HeSoViTri).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.TenViTri).HasMaxLength(250);
        });

        modelBuilder.Entity<DaDanhMucView>(entity =>
        {
            entity.HasKey(e => new { e.MaView, e.MaDuAn });

            entity.ToTable("DA_DanhMucView");

            entity.Property(e => e.MaView)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HeSoView).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.TenView).HasMaxLength(250);
        });

        modelBuilder.Entity<DaDanhMucViewMatKhoi>(entity =>
        {
            entity.HasKey(e => new { e.MaDuAn, e.MaMatKhoi }).HasName("PK_DA_DanhMucViewMatKhoi_1");

            entity.ToTable("DA_DanhMucViewMatKhoi");

            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaMatKhoi)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HeSoMatKhoi).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.TenMatKhoi).HasMaxLength(250);
        });

        modelBuilder.Entity<DaDanhMucViewTruc>(entity =>
        {
            entity.HasKey(e => new { e.MaTruc, e.MaDuAn, e.MaBlock });

            entity.ToTable("DA_DanhMucViewTruc");

            entity.HasIndex(e => new { e.MaDuAn, e.MaBlock, e.MaTruc }, "IX_DA_DanhMucViewTruc_MaDuAn_MaBlock_MaTruc");

            entity.Property(e => e.MaTruc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaBlock)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HeSoTruc).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.MaHuong)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiGoc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiView)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaViTri)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaViewMatKhoi)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenTruc).HasMaxLength(250);
        });

        modelBuilder.Entity<DaLoaiDienTich>(entity =>
        {
            entity.HasKey(e => new { e.MaDuAn, e.MaLoaiDt });

            entity.ToTable("DA_LoaiDienTich");

            entity.HasIndex(e => new { e.MaDuAn, e.MaLoaiDt }, "IX_DA_LoaiDienTich_MaDuAn");

            entity.Property(e => e.MaDuAn)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiDt)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MaLoaiDT");
            entity.Property(e => e.HeSo).HasColumnType("decimal(8, 4)");
            entity.Property(e => e.TenLoaiDt)
                .HasMaxLength(200)
                .HasColumnName("TenLoaiDT");
        });

        modelBuilder.Entity<DmSanGiaoDich>(entity =>
        {
            entity.HasKey(e => e.MaSanGiaoDich);

            entity.ToTable("DM_SanGiaoDich");

            entity.Property(e => e.MaSanGiaoDich)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DienThoai)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenSanGiaoDich).HasMaxLength(200);
            entity.Property(e => e.TrangThai).HasComment("1: Đang mở, 0 đang đóng");
        });

        modelBuilder.Entity<DmSanGiaoDichDuAn>(entity =>
        {
            entity.HasKey(e => new { e.MaSan, e.MaDuAn });

            entity.ToTable("DM_SanGiaoDich_DuAn");

            entity.Property(e => e.MaSan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HtBuocDuyet>(entity =>
        {
            entity.HasKey(e => e.MaBuocDuyet).HasName("PK_HT_BuocDuyet_1");

            entity.ToTable("HT_BuocDuyet");

            entity.Property(e => e.MaBuocDuyet)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.IdbuocDuyet).HasColumnName("IDBuocDuyet");
            entity.Property(e => e.TenBuocDuyet).HasMaxLength(500);
            entity.Property(e => e.TenHienThi).HasMaxLength(250);
        });

        modelBuilder.Entity<HtDanhMucHinhThucThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaHttt);

            entity.ToTable("HT_DanhMucHinhThucThanhToan");

            entity.Property(e => e.MaHttt)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("MaHTTT");
            entity.Property(e => e.TenHttt)
                .HasMaxLength(250)
                .HasColumnName("TenHTTT");
        });

        modelBuilder.Entity<HtDanhMucTinhTrangSanPham>(entity =>
        {
            entity.HasKey(e => e.MaTinhTrang);

            entity.ToTable("HT_DanhMucTinhTrangSanPham");

            entity.Property(e => e.MaTinhTrang)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.MaMau)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenTinhTrang).HasMaxLength(50);
        });

        modelBuilder.Entity<HtDmhinhThucKhuyenMai>(entity =>
        {
            entity.HasKey(e => e.MaHinhThucKm);

            entity.ToTable("HT_DMHinhThucKhuyenMai");

            entity.Property(e => e.MaHinhThucKm)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MaHinhThucKM");
            entity.Property(e => e.TenHinhThucKm)
                .HasMaxLength(500)
                .HasColumnName("TenHinhThucKM");
        });

        modelBuilder.Entity<HtDmloaiDieuKienKhuyenMai>(entity =>
        {
            entity.HasKey(e => e.MaLoaiDieuKienKm);

            entity.ToTable("HT_DMLoaiDieuKienKhuyenMai");

            entity.Property(e => e.MaLoaiDieuKienKm)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MaLoaiDieuKienKM");
            entity.Property(e => e.TenLoaiDieuKienKm)
                .HasMaxLength(500)
                .HasColumnName("TenLoaiDieuKienKM");
        });

        modelBuilder.Entity<HtDmnguoiDuyet>(entity =>
        {
            entity.ToTable("HT_DMNguoiDuyet");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaCongViec)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayDuyet).HasColumnType("datetime");
        });

        modelBuilder.Entity<HtDmquocGium>(entity =>
        {
            entity.HasKey(e => e.MaQuocGia);

            entity.ToTable("HT_DMQuocGia");

            entity.Property(e => e.MaQuocGia)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HtDmtrangThaiThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai);

            entity.ToTable("HT_DMTrangThaiThanhToan");

            entity.Property(e => e.MaTrangThai).ValueGeneratedNever();
            entity.Property(e => e.MaMau)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaMauChu)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaMauNen)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaMauSo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThaiEn).HasMaxLength(500);
            entity.Property(e => e.TenTrangThaiVi).HasMaxLength(500);
        });

        modelBuilder.Entity<HtEmailHistory>(entity =>
        {
            entity.ToTable("HT_EmailHistory");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.NgayGui).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TieuDe).HasMaxLength(500);
        });

        modelBuilder.Entity<HtFileDinhKem>(entity =>
        {
            entity.ToTable("HT_FileDinhKem");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AcTion)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Controller)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FileSize).HasMaxLength(15);
            entity.Property(e => e.FileType).HasMaxLength(250);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.TaiLieuUrl).HasColumnName("TaiLieuURL");
            entity.Property(e => e.TenNhanVien).HasMaxLength(100);
            entity.Property(e => e.ThumbnailUrl).HasColumnName("ThumbnailURL");
        });

        modelBuilder.Entity<HtGhiNhanLog>(entity =>
        {
            entity.ToTable("HT_GhiNhanLog");

            entity.Property(e => e.Id).HasColumnName("ID");
        });

        modelBuilder.Entity<HtHienTrangKinhDoanh>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai);

            entity.ToTable("HT_HienTrangKinhDoanh");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FontWeghtMaCanHo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaMauChu)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaMauGoc)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(100);
        });

        modelBuilder.Entity<HtHistoryLog>(entity =>
        {
            entity.ToTable("HT_History_logs");

            entity.Property(e => e.GiaTri)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
            entity.Property(e => e.NguoiCapNhat)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenBang)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.TenTruong)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HtLoaiMauIn>(entity =>
        {
            entity.HasKey(e => e.MaLoaiMauIn);

            entity.ToTable("HT_LoaiMauIn");

            entity.Property(e => e.MaLoaiMauIn)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HtMauIn>(entity =>
        {
            entity.HasKey(e => e.MaMauIn);

            entity.ToTable("HT_MauIn");

            entity.Property(e => e.MaMauIn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LoaiMauIn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HtQuyTrinhDuyet>(entity =>
        {
            entity.ToTable("HT_QuyTrinhDuyet");

            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.MaCongViec)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenQuyTrinh).HasMaxLength(500);
        });

        modelBuilder.Entity<HtQuyTrinhDuyetBuocDuyet>(entity =>
        {
            entity.ToTable("HT_QuyTrinhDuyet_BuocDuyet");

            entity.Property(e => e.MaBuocDuyet)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NguoiDuyet)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HtQuyTrinhDuyetDuAn>(entity =>
        {
            entity.ToTable("HT_QuyTrinhDuyet_DuAn");

            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HtSendEmail>(entity =>
        {
            entity.ToTable("HT_SendEmail");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IdEmail)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayGui).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TieuDe).HasMaxLength(500);
        });

        modelBuilder.Entity<HtSendEmailAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HT_SendE__3214EC0751CF9974");

            entity.ToTable("HT_SendEmailAttachments");

            entity.Property(e => e.EmailId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FileName).HasMaxLength(255);
        });

        modelBuilder.Entity<HtTemplate>(entity =>
        {
            entity.HasKey(e => e.MaTemplate);

            entity.ToTable("HT_Template");

            entity.Property(e => e.MaTemplate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoTemplate)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.TieuDe).HasMaxLength(250);
        });

        modelBuilder.Entity<HtThongTinCongTy>(entity =>
        {
            entity.HasKey(e => e.MaCongTy);

            entity.ToTable("HT_ThongTinCongTy");

            entity.Property(e => e.MaCongTy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ChiNhanhNganHang).HasMaxLength(500);
            entity.Property(e => e.ChucVuNguoiDaiDien).HasMaxLength(50);
            entity.Property(e => e.CmndNgayCapNguoiDd)
                .HasColumnType("datetime")
                .HasColumnName("CmndNgayCapNguoiDD");
            entity.Property(e => e.CmndNoiCapNguoiDd)
                .HasMaxLength(50)
                .HasColumnName("CmndNoiCapNguoiDD");
            entity.Property(e => e.CmndSoNguoiDaiDien).HasMaxLength(50);
            entity.Property(e => e.DaiDienCongTy).HasMaxLength(250);
            entity.Property(e => e.DiaChiCongTy).HasMaxLength(500);
            entity.Property(e => e.DienThoai).HasMaxLength(50);
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .HasColumnName("email");
            entity.Property(e => e.Fax).HasMaxLength(50);
            entity.Property(e => e.MaSoThue).HasMaxLength(50);
            entity.Property(e => e.TaiKhoan).HasMaxLength(500);
            entity.Property(e => e.TenChiNhanh).HasMaxLength(500);
            entity.Property(e => e.TenCongTy).HasMaxLength(500);
            entity.Property(e => e.TenNganHang).HasMaxLength(500);
            entity.Property(e => e.TenTaiKhoan).HasMaxLength(50);
        });

        modelBuilder.Entity<HtTrangThaiDuyet>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai);

            entity.ToTable("HT_TrangThaiDuyet");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Icon).HasMaxLength(150);
            entity.Property(e => e.Mau)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenTrangThai).HasMaxLength(50);
        });

        modelBuilder.Entity<KdChuyenNhuong>(entity =>
        {
            entity.HasKey(e => e.MaChuyenNhuong).HasName("PK__KD_Chuye__B48B1EDA1094623D");

            entity.ToTable("KD_ChuyenNhuong");

            entity.Property(e => e.MaChuyenNhuong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GiaTriCanHo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaTriDaThanhToan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaHopDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSanPham)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayChuyenNhuong).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhiBaoTri).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PhiBaoTriDaTt)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("PhiBaoTriDaTT");
        });

        modelBuilder.Entity<KdChuyenNhuongKhachHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KD_Chuye__3214EC07D26F3A7C");

            entity.ToTable("KD_ChuyenNhuong_KhachHang");

            entity.Property(e => e.IdlanDieuChinhKh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IDLanDieuChinhKH");
            entity.Property(e => e.MaChuyenNhuong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Stt).HasColumnName("STT");
        });

        modelBuilder.Entity<KdHopDong>(entity =>
        {
            entity.HasKey(e => e.MaHopDong);

            entity.ToTable("KD_HopDong");

            entity.Property(e => e.MaHopDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DienTichLotLong).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichSanVuon).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DienTichTimTuong).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DonGiaDat).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanSauThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanTienThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanTruocThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaCanHoSauThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaCanHoTruocThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaDat).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaTriCk)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("GiaTriCK");
            entity.Property(e => e.IdlanDieuChinhKh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IDLanDieuChinhKH");
            entity.Property(e => e.MaCanHo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaChinhSachThanhToan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDatCoc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaMauIn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayKy).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NgayXacNhan).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NguoiXacNhan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoHopDong).HasMaxLength(250);
            entity.Property(e => e.TienQuyBaoTri).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TyLeCk)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeCK");
            entity.Property(e => e.TyLeQuyBaoTri).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TyLeThueVat)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeThueVAT");
        });

        modelBuilder.Entity<KdHopDongKhachHang>(entity =>
        {
            entity.HasKey(e => new { e.MaHopDong, e.MaKhachHang });

            entity.ToTable("KD_HopDong_KhachHang");

            entity.Property(e => e.MaHopDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IdlanDieuChinhKh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IDLanDieuChinhKH");
            entity.Property(e => e.IskhdaiDien).HasColumnName("ISKHDaiDien");
        });

        modelBuilder.Entity<KdHopDongTienDoThanhToan>(entity =>
        {
            entity.ToTable("KD_HopDong_TienDoThanhToan");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DotTt).HasColumnName("DotTT");
            entity.Property(e => e.KyThanhToan)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaCstt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
            entity.Property(e => e.MaHopDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NoiDungTt)
                .HasMaxLength(500)
                .HasColumnName("NoiDungTT");
            entity.Property(e => e.SoTienCanTruDaTt)
                .HasColumnType("numeric(18, 0)")
                .HasColumnName("SoTienCanTruDaTT");
            entity.Property(e => e.SoTienPhaiThanhToan).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.SoTienTt)
                .HasColumnType("numeric(18, 0)")
                .HasColumnName("SoTienTT");
            entity.Property(e => e.TyLeTt)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeTT");
            entity.Property(e => e.TyLeVat)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TyLeVAT");
        });

        modelBuilder.Entity<KdPhieuDeNghiHoanTienBooking>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("KD_PhieuDeNghiHoanTienBooking");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSanGiaoDich)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<KdPhieuDeNghiHoanTienBookingSoPhieuBooking>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuHoanTien, e.MaPhieuBooking, e.MaPhieuTongHopThu });

            entity.ToTable("KD_PhieuDeNghiHoanTienBooking_SoPhieuBooking");

            entity.Property(e => e.MaPhieuHoanTien)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuBooking)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuTongHopThu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<KdPhieuTongHopBooking>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("KD_PhieuTongHopBooking");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSanGiaoDich)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NgayThu).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<KdPhieuTongHopBookingPhieuBooking>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieuTh, e.MaBooking });

            entity.ToTable("KD_PhieuTongHopBooking_PhieuBooking");

            entity.Property(e => e.MaPhieuTh)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("MaPhieuTH");
            entity.Property(e => e.MaBooking)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<KdPhuLucHopDong>(entity =>
        {
            entity.HasKey(e => e.MaPhuLuc);

            entity.ToTable("KD_PhuLucHopDong");

            entity.Property(e => e.MaPhuLuc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanSauCk)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("GiaBanSauCK");
            entity.Property(e => e.GiaBanSauThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaBanTruocThue).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GiaTriCk)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("GiaTriCK");
            entity.Property(e => e.MaCstt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
            entity.Property(e => e.MaHopDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaMauIn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaQuiTrinhDuyet)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayKyPl)
                .HasColumnType("datetime")
                .HasColumnName("NgayKyPL");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoPhuLuc).HasMaxLength(250);
            entity.Property(e => e.TrangThaiDuyet)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<KdPhuLucHopDongTienDoThanhToan>(entity =>
        {
            entity.ToTable("KD_PhuLucHopDong_TienDoThanhToan");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DotTt).HasColumnName("DotTT");
            entity.Property(e => e.GiaiDoanTt)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("GiaiDoanTT");
            entity.Property(e => e.KyThanhToan)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaCstt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaCSTT");
            entity.Property(e => e.MaPhuLuc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayTt)
                .HasColumnType("datetime")
                .HasColumnName("NgayTT");
            entity.Property(e => e.NoiDungTt)
                .HasMaxLength(500)
                .HasColumnName("NoiDungTT");
            entity.Property(e => e.SoTienTt)
                .HasColumnType("numeric(18, 0)")
                .HasColumnName("SoTienTT");
            entity.Property(e => e.TyLeTt).HasColumnName("TyLeTT");
        });

        modelBuilder.Entity<KdThanhLyHopDong>(entity =>
        {
            entity.HasKey(e => e.MaPhieuTl);

            entity.ToTable("KD_ThanhLyHopDong");

            entity.Property(e => e.MaPhieuTl)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaPhieuTL");
            entity.Property(e => e.HinhThucThanhLy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaHopDong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NgayThanhLy).HasColumnType("datetime");
            entity.Property(e => e.NguoiThanhLy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoPhieuTl)
                .HasMaxLength(250)
                .HasColumnName("SoPhieuTL");
            entity.Property(e => e.SoTienDaThu).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SoTienHoanTra).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SoTienPhiBaoTriDaThu).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SoTienViPhamHopDong).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TyLeViPham).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<KhDmdoiTuongKhachHang>(entity =>
        {
            entity.HasKey(e => e.MaDoiTuongKhachHang).HasName("PK__KH_DMDoi__0CEBC3440B38C10B");

            entity.ToTable("KH_DMDoiTuongKhachHang");

            entity.Property(e => e.MaDoiTuongKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenDoiTuongKhachHang).HasMaxLength(100);
        });

        modelBuilder.Entity<KhDmkhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang);

            entity.ToTable("KH_DMKhachHang");

            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDoiTuongKhachHang)
                .HasMaxLength(50)
                .HasComment("C: cá nhân,T: tổ chức");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.TenKhachHang).HasMaxLength(250);
        });

        modelBuilder.Entity<KhDmkhachHangChiTiet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KH_DMKha__3214EC2735396354");

            entity.ToTable("KH_DMKhachHangChiTiet");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ChucVuNguoiDaiDien).HasMaxLength(250);
            entity.Property(e => e.DiaChiLienLac).HasMaxLength(200);
            entity.Property(e => e.DiaChiThuongTru).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.IdCard)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IdlanDieuChinh)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("IDLanDieuChinh");
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiIdCard)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapIdCard).HasColumnType("datetime");
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.NguoiDaiDien).HasMaxLength(100);
            entity.Property(e => e.NguoiLienHe).HasMaxLength(250);
            entity.Property(e => e.NoiCapIdCard).HasMaxLength(100);
            entity.Property(e => e.QuocTich).HasMaxLength(50);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(11)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoaiDaiDien)
                .HasMaxLength(11)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoaiNguoiLienHe)
                .HasMaxLength(11)
                .IsUnicode(false);
        });

        modelBuilder.Entity<KhDmkhachHangHinhAnhDinhKem>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("KH_DMKhachHang_HinhAnhDinhKem");

            entity.Property(e => e.MaHinhAnh)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<KhDmkhachHangNguon>(entity =>
        {
            entity.HasKey(e => new { e.MaKhachHang, e.MaKhachHangTam });

            entity.ToTable("KH_DMKhachHang_Nguon");

            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHangTam)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.KhDmkhachHangNguons)
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KH_DMKhachHang_Nguon_KH_DMKhachHang");

            entity.HasOne(d => d.MaKhachHangTamNavigation).WithMany(p => p.KhDmkhachHangNguons)
                .HasForeignKey(d => d.MaKhachHangTam)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KH_DMKhachHang_Nguon_KH_DMKhachHangTam");
        });

        modelBuilder.Entity<KhDmkhachHangTam>(entity =>
        {
            entity.HasKey(e => e.MaKhachHangTam);

            entity.ToTable("KH_DMKhachHangTam");

            entity.Property(e => e.MaKhachHangTam)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ChucVuNguoiDaiDien).HasMaxLength(150);
            entity.Property(e => e.DiaChiHienNay).HasMaxLength(500);
            entity.Property(e => e.DiaChiThuongTru).HasMaxLength(500);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.IdCard).HasMaxLength(100);
            entity.Property(e => e.MaDoiTuongKhachHang)
                .HasMaxLength(50)
                .HasComment("C: cá nhân,T: tổ chức");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiIdCard)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNguonKhach)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSanGd)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaSanGD");
            entity.Property(e => e.NgayCapIdCard).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.NguoiDaiDien).HasMaxLength(100);
            entity.Property(e => e.NguoiLienHe).HasMaxLength(150);
            entity.Property(e => e.NoiCapIdCard).HasMaxLength(500);
            entity.Property(e => e.QuocTich)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai).HasMaxLength(11);
            entity.Property(e => e.SoDienThoaiNguoiDaiDien)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TenKhachHang).HasMaxLength(250);

            entity.HasOne(d => d.MaLoaiIdCardNavigation).WithMany(p => p.KhDmkhachHangTams)
                .HasForeignKey(d => d.MaLoaiIdCard)
                .HasConstraintName("FK_KH_DMKhachHangTam_KH_DMLoaiCard");

            entity.HasOne(d => d.MaSanGdNavigation).WithMany(p => p.KhDmkhachHangTams)
                .HasForeignKey(d => d.MaSanGd)
                .HasConstraintName("FK_KH_DMKhachHangTam_DM_SanGiaoDich");

            entity.HasOne(d => d.QuocTichNavigation).WithMany(p => p.KhDmkhachHangTams)
                .HasForeignKey(d => d.QuocTich)
                .HasConstraintName("FK_KH_DMKhachHangTam_HT_DMQuocGia");
        });

        modelBuilder.Entity<KhDmloaiCard>(entity =>
        {
            entity.HasKey(e => e.MaLoaiIdCard).HasName("PK__DA_DanhM__EC802C68F7565D96");

            entity.ToTable("KH_DMLoaiCard");

            entity.Property(e => e.MaLoaiIdCard)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenLoaiIdCard).HasMaxLength(100);
        });

        modelBuilder.Entity<KhDmnguonKhachHang>(entity =>
        {
            entity.HasKey(e => e.MaNguonKhach);

            entity.ToTable("KH_DMNguonKhachHang");

            entity.Property(e => e.MaNguonKhach)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNguonCha)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenNguonKhach).HasMaxLength(250);
        });

        modelBuilder.Entity<KtPhieuCongNoPhaiThu>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("KT_PhieuCongNoPhaiThu");

            entity.HasIndex(e => e.MaPhieu, "UX_KT_PCNPT_MaPhieu").IsUnique();

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HanThanhToan).HasColumnType("datetime");
            entity.Property(e => e.IdChungTu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaChungTu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaCongViec)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDoiTuong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NoiDung).HasMaxLength(500);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TenKhachHang).HasMaxLength(500);
        });

        modelBuilder.Entity<KtPhieuCongNoPhaiTra>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("KT_PhieuCongNoPhaiTra");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HanThanhToan).HasColumnType("datetime");
            entity.Property(e => e.IdChungTu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaChungTu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaCongViec)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDoiTuong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NoiDung).HasMaxLength(500);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TenKhachHang).HasMaxLength(500);
        });

        modelBuilder.Entity<KtPhieuXacNhanThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("KT_PhieuXacNhanThanhToan");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.HinhThucThanhToan)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LoaiPhieu)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasComment("T: Thu; C: Chi");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayHachToan).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoChungTu).HasMaxLength(500);
            entity.Property(e => e.TenKhachHang).HasMaxLength(500);
        });

        modelBuilder.Entity<KtPhieuXacNhanThanhToanChiTiet>(entity =>
        {
            entity.ToTable("KT_PhieuXacNhanThanhToan_ChiTiet");

            entity.Property(e => e.IdChungTu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuCongNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<KtPhieuXacNhanThanhToanPhieuChuyenDoi>(entity =>
        {
            entity.HasKey(e => new { e.MaPhieu, e.MaPhieuNguon });

            entity.ToTable("KT_PhieuXacNhanThanhToan_PhieuChuyenDoi");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaPhieuNguon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoTienChuyenDoi).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<KtTinhLaiQuaHanNhap>(entity =>
        {
            entity.ToTable("KT_TinhLaiQuaHan_Nhap");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DaDong).HasColumnName("daDong");
            entity.Property(e => e.GiamTru).HasColumnName("giamTru");
            entity.Property(e => e.LaiSuatQuaHan).HasColumnName("laiSuatQuaHan");
            entity.Property(e => e.MaCanHo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maCanHo");
            entity.Property(e => e.MaGiaiDoanTt)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("maGiaiDoanTT");
            entity.Property(e => e.NgayBatDau).HasColumnName("ngayBatDau");
            entity.Property(e => e.NgayKetThuc).HasColumnName("ngayKetThuc");
            entity.Property(e => e.SoNgayQuaHan).HasColumnName("soNgayQuaHan");
            entity.Property(e => e.SoTienTinhLai).HasColumnName("soTienTinhLai");
            entity.Property(e => e.TienLai).HasColumnName("tienLai");
        });

        modelBuilder.Entity<KvDmkhuVuc>(entity =>
        {
            entity.HasKey(e => e.MaKhuVuc);

            entity.ToTable("KV_DMKhuVuc");

            entity.Property(e => e.MaKhuVuc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenKhuVuc).HasMaxLength(250);
        });

        modelBuilder.Entity<TblCongviec>(entity =>
        {
            entity.HasKey(e => e.MaCongViec).HasName("PK_TBL_CongViec");

            entity.ToTable("TBL_CONGVIEC");

            entity.Property(e => e.MaCongViec)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.DoUuTien).HasDefaultValue(0);
            entity.Property(e => e.GhiChu).HasMaxLength(100);
            entity.Property(e => e.HienThiTrenMenu).HasDefaultValue(true);
            entity.Property(e => e.MaCha)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenAction).HasMaxLength(50);
            entity.Property(e => e.TenCongViec).HasMaxLength(100);
            entity.Property(e => e.TenController).HasMaxLength(50);
            entity.Property(e => e.TienTo)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblCongviecvavuviec>(entity =>
        {
            entity.HasKey(e => new { e.MaCongViec, e.MaVuViec, e.MaNhomUser });

            entity.ToTable("TBL_CONGVIECVAVUVIEC");

            entity.Property(e => e.MaCongViec)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.MaVuViec)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaNhomUser)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.MaCongViecNavigation).WithMany(p => p.TblCongviecvavuviecs)
                .HasForeignKey(d => d.MaCongViec)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBL_CONGVIECVAVUVIEC_TBL_CONGVIEC");

            entity.HasOne(d => d.MaNhomUserNavigation).WithMany(p => p.TblCongviecvavuviecs)
                .HasForeignKey(d => d.MaNhomUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBL_CONGVIECVAVUVIEC_TBL_NHOMUSER");

            entity.HasOne(d => d.MaVuViecNavigation).WithMany(p => p.TblCongviecvavuviecs)
                .HasForeignKey(d => d.MaVuViec)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBL_CONGVIECVAVUVIEC_TBL_VUVIEC");
        });

        modelBuilder.Entity<TblDuan>(entity =>
        {
            entity.HasKey(e => e.MaDuAn);

            entity.ToTable("TBL_DUAN");

            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.IsThoiGian).HasComment("Khi thời gian thực hiện dự án thay đổi thì thời gian công việc thay đổi theo.Khi bật cài đặt này thì, ví dụ: Thời gian thực hiện dự án là 10/03/2020 - 20/03/2020, công việc X thuộc dự án A có thời gian thực hiện là 11/03/2020 - 15/03/2020. Khi dự án A được tịnh tiến 3 ngày, tức thời gian thực hiện là 13/03/2020 – 20/03/2020, thì thời gian công việc X cũng tịnh tiến thêm 3 ngày, tức là 14/03/2020 - 15/03/2020.");
            entity.Property(e => e.IsXemCheoCongViec).HasComment("Không cho phép người thực hiện công việc xem chéo các công việc khác.Khi bật cài đặt này thì, ví dụ: Dự án A gồm 2 công việc B và C, người thực hiện công việc B sẽ không được xem công việc C nếu người đó không phải là người thực hiện công việc C");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaTienDo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblDuanFiledinhkem>(entity =>
        {
            entity.ToTable("TBL_DUAN_FILEDINHKEM");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SizeFile)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TypeFile)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblDuanLichsuhoatdong>(entity =>
        {
            entity.ToTable("TBL_DUAN_LICHSUHOATDONG");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapNhat).HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblDuanNhanvien>(entity =>
        {
            entity.ToTable("TBL_DUAN_NHANVIEN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Loai).HasComment("0: Người Quản Trị, 1: Người Thực Hiện");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblDuanThaoluan>(entity =>
        {
            entity.ToTable("TBL_DUAN_THAOLUAN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblDuanThaoluanFiledinhkem>(entity =>
        {
            entity.ToTable("TBL_DUAN_THAOLUAN_FILEDINHKEM");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdthaoLuan).HasColumnName("IDThaoLuan");
            entity.Property(e => e.SizeFile)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TypeFile)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblDuanTiendoduan>(entity =>
        {
            entity.ToTable("TBL_DUAN_TIENDODUAN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaTienDo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenTienDo).HasMaxLength(200);
        });

        modelBuilder.Entity<TblDuanTrangthai>(entity =>
        {
            entity.HasKey(e => e.MaTrangThai);

            entity.ToTable("TBL_DUAN_TRANGTHAI");

            entity.Property(e => e.MaTrangThai)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblNganhang>(entity =>
        {
            entity.ToTable("TBL_NGANHANG");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GhiChu).HasMaxLength(250);
            entity.Property(e => e.MaNganHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenNganHang).HasMaxLength(250);
        });

        modelBuilder.Entity<TblNhanvien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TBL_NHANSU_NHANVIEN");

            entity.ToTable("TBL_NHANVIEN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EmailCongTy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasComment("0 = Nam\n\n1 = Nữ\n\n2 = Khác");
            entity.Property(e => e.HoVaTen).HasMaxLength(200);
            entity.Property(e => e.MaCanCuoc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaChamCong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaChucVu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDanToc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDiaChiTamTru)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaLoaiHoChieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNganhNghe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaPhongBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaQuocTich)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSanGiaoDich)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaSoThueCaNhan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaThuongTru)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaTonGia)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaTrinhDoHocVan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaTrinhDoPhoThong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayCapCc)
                .HasColumnType("datetime")
                .HasColumnName("NgayCapCC");
            entity.Property(e => e.NgayCapHoChieu).HasColumnType("datetime");
            entity.Property(e => e.NgayHetHanHoChieu).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NoiCapCc).HasColumnName("NoiCapCC");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai2)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SoHoChieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TinhTrangHonNhan).HasComment("0 = Độc thân \n\n1 = Kết hôn 3 = Ly hôn");
            entity.Property(e => e.UrlCccdmatSau).HasColumnName("UrlCCCDMatSau");
            entity.Property(e => e.UrlCccdmatTruoc).HasColumnName("UrlCCCDMatTruoc");
        });

        modelBuilder.Entity<TblNhanvienChucvu>(entity =>
        {
            entity.ToTable("TBL_NHANVIEN_CHUCVU");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GhiChu).HasMaxLength(250);
            entity.Property(e => e.MaChucVu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenChucVu).HasMaxLength(250);
        });

        modelBuilder.Entity<TblNhanvienDantoc>(entity =>
        {
            entity.ToTable("TBL_NHANVIEN_DANTOC");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GhiChu).HasMaxLength(250);
            entity.Property(e => e.MaDanToc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenDanToc).HasMaxLength(250);
        });

        modelBuilder.Entity<TblNhanvienHochieu>(entity =>
        {
            entity.ToTable("TBL_NHANVIEN_HOCHIEU");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaLoaiHoChieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenLoaiHoChieu).HasMaxLength(250);
        });

        modelBuilder.Entity<TblNhanvienNganhang>(entity =>
        {
            entity.HasKey(e => new { e.MaNhanVien, e.SoTaiKhoanNh }).HasName("PK_TBL_NHANVIEN_NGANHANG_1");

            entity.ToTable("TBL_NHANVIEN_NGANHANG");

            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SoTaiKhoanNh)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("SoTaiKhoanNH");
            entity.Property(e => e.DiaChiNganHang).HasMaxLength(250);
            entity.Property(e => e.MaChiNhanh).HasMaxLength(250);
            entity.Property(e => e.MaNganHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenTaiKhoanNh)
                .HasMaxLength(250)
                .HasColumnName("TenTaiKhoanNH");
        });

        modelBuilder.Entity<TblNhanvienNganhnghe>(entity =>
        {
            entity.ToTable("TBL_NHANVIEN_NGANHNGHE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaNganhNghe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenNganhNghe).HasMaxLength(250);
        });

        modelBuilder.Entity<TblNhanvienNguoiphuthuoc>(entity =>
        {
            entity.ToTable("TBL_NHANVIEN_NGUOIPHUTHUOC");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.HoVaTen).HasMaxLength(200);
            entity.Property(e => e.MaMoiQuanHe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayBatDauGiamTru).HasColumnType("datetime");
            entity.Property(e => e.NgayCap).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThucGiamTru).HasColumnType("datetime");
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.SoCmtcc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SoCMTCC");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblNhanvienNguoiphuthuocMoiquanhe>(entity =>
        {
            entity.ToTable("TBL_NHANVIEN_NGUOIPHUTHUOC_MOIQUANHE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GhiChu).HasMaxLength(200);
            entity.Property(e => e.MaMoiQuanHe)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenMoiQuanHe).HasMaxLength(100);
        });

        modelBuilder.Entity<TblNhanvienPhongban>(entity =>
        {
            entity.ToTable("TBL_NHANVIEN_PHONGBAN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GhiChu).HasMaxLength(250);
            entity.Property(e => e.MaPhongBan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenPhongBan).HasMaxLength(250);
        });

        modelBuilder.Entity<TblNhanvienTongiao>(entity =>
        {
            entity.ToTable("TBL_NHANVIEN_TONGIAO");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GhiChu).HasMaxLength(250);
            entity.Property(e => e.MaTonGia)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenTonGia).HasMaxLength(250);
        });

        modelBuilder.Entity<TblNhanvienTrinhdo>(entity =>
        {
            entity.ToTable("TBL_NHANVIEN_TRINHDO");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GhiChu).HasMaxLength(250);
            entity.Property(e => e.MaTrinhDo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenTrinhDo).HasMaxLength(250);
        });

        modelBuilder.Entity<TblNhomuser>(entity =>
        {
            entity.HasKey(e => e.MaNhomUser);

            entity.ToTable("TBL_NHOMUSER");

            entity.Property(e => e.MaNhomUser)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.GhiChu).HasMaxLength(100);
            entity.Property(e => e.TenNhomUser).HasMaxLength(100);
        });

        modelBuilder.Entity<TblRefreshtoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TBL_RefreshToken");

            entity.ToTable("TBL_REFRESHTOKEN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayHetHan).HasColumnType("datetime");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ToKen).HasMaxLength(240);

            entity.HasOne(d => d.User).WithMany(p => p.TblRefreshtokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_TBL_REFRESHTOKEN_TBL_USER");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.ToTable("TBL_USER");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LoaiUser)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasComment("'NV': Nhân viên, 'SGD': Sàn giao dịch");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.NgayHetHangToken).HasColumnType("datetime");
            entity.Property(e => e.NgayLap).HasColumnType("datetime");
            entity.Property(e => e.NguoiLap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(250)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblUserthuocnhom>(entity =>
        {
            entity.HasKey(e => new { e.MaNhomUser, e.UserId });

            entity.ToTable("TBL_USERTHUOCNHOM");

            entity.Property(e => e.MaNhomUser)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MaNv)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MaNV");

            entity.HasOne(d => d.MaNhomUserNavigation).WithMany(p => p.TblUserthuocnhoms)
                .HasForeignKey(d => d.MaNhomUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBL_USERTHUOCNHOM_TBL_NHOMUSER");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserthuocnhoms)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TBL_USERTHUOCNHOM_TBL_USER");
        });

        modelBuilder.Entity<TblVuviec>(entity =>
        {
            entity.HasKey(e => e.MaVuViec);

            entity.ToTable("TBL_VUVIEC");

            entity.Property(e => e.MaVuViec)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.GhiChu)
                .HasMaxLength(100)
                .HasColumnName("ghiChu");
            entity.Property(e => e.TenVuViec).HasMaxLength(50);
        });

        modelBuilder.Entity<TblVuvieccuacongviec>(entity =>
        {
            entity.HasKey(e => e.VuViecCuaCongViecId).HasName("PK_TBL_VuViecCuaCongViec");

            entity.ToTable("TBL_VUVIECCUACONGVIEC");

            entity.Property(e => e.MaCongViec)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.MaVuViec)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.MaCongViecNavigation).WithMany(p => p.TblVuvieccuacongviecs)
                .HasForeignKey(d => d.MaCongViec)
                .HasConstraintName("FK_TBL_VUVIECCUACONGVIEC_TBL_CONGVIEC");

            entity.HasOne(d => d.MaVuViecNavigation).WithMany(p => p.TblVuvieccuacongviecs)
                .HasForeignKey(d => d.MaVuViec)
                .HasConstraintName("FK_TBL_VUVIECCUACONGVIEC_TBL_VUVIEC");
        });

        modelBuilder.Entity<TcDmtienDoThiCongDuAn>(entity =>
        {
            entity.HasKey(e => e.MaGiaiDoan);

            entity.ToTable("TC_DMTienDoThiCongDuAn");

            entity.Property(e => e.MaGiaiDoan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhuVuc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayHoanThanh).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.TenGiaiDoan).HasMaxLength(250);
        });

        modelBuilder.Entity<TcTienDoThiCongDuAn>(entity =>
        {
            entity.HasKey(e => e.MaPhieu);

            entity.ToTable("TC_TienDoThiCongDuAn");

            entity.Property(e => e.MaPhieu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaDuAn)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaGiaiDoan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKhuVuc)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayLap).HasColumnType("datetime");

            entity.HasOne(d => d.MaDuAnNavigation).WithMany(p => p.TcTienDoThiCongDuAns)
                .HasForeignKey(d => d.MaDuAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TC_TienDoThiCongDuAn_DA_DanhMucDuAn");

            entity.HasOne(d => d.MaGiaiDoanNavigation).WithMany(p => p.TcTienDoThiCongDuAns)
                .HasForeignKey(d => d.MaGiaiDoan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TC_TienDoThiCongDuAn_TC_DMTienDoThiCongDuAn");

            entity.HasOne(d => d.MaKhuVucNavigation).WithMany(p => p.TcTienDoThiCongDuAns)
                .HasForeignKey(d => d.MaKhuVuc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TC_TienDoThiCongDuAn_KV_DMKhuVuc");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
