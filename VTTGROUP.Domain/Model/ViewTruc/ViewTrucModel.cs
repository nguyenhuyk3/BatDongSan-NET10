namespace VTTGROUP.Domain.Model.ViewTruc
{
    public class ViewTrucPagingDto
    {
        public int STT { get; set; }
        public string MaTruc { get; set; } = string.Empty;
        public string TenTruc { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaBlock { get; set; } = string.Empty;
        public string TenBlock { get; set; } = string.Empty;
        public string MaLoaiGoc { get; set; } = string.Empty;
        public string TenLoaiGoc { get; set; } = string.Empty;
        public string MaLoaiView { get; set; } = string.Empty;
        public string TenView { get; set; } = string.Empty;
        public string MaViTri { get; set; } = string.Empty;
        public string TenViTri { get; set; } = string.Empty;
        public string MaViewMatKhoi { get; set; } = string.Empty;
        public string TenMatKhoi { get; set; } = string.Empty;      
        public float HeSoTruc { get; set; } = 0;
        public int ThuTuHienThi { get; set; } = 1;
        public int TotalCount { get; set; }
        public bool IsSelected { get; set; }
    }
    public class ViewTrucModel
    {
        public string? MaTruc { get; set; }
        public string? TenTruc { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public string? MaBlock { get; set; }
        public string? TenBlock { get; set; }
        public decimal HeSoTruc { get; set; }
        public int ThuTuHienThi { get; set; }
        public string MaLoaiGoc { get; set; } = string.Empty;
        public string TenLoaiGoc { get; set; } = string.Empty;
        public string MaViTri { get; set; } = string.Empty;
        public string TenViTri { get; set; } = string.Empty;
        public string MaView { get; set; } = string.Empty;
        public string TenView { get; set; } = string.Empty;
        public string MaMatKhoi { get; set; } = string.Empty;
        public string TenMatKhoi { get; set; } = string.Empty;
        public string MaHuong { get; set; } = string.Empty;
        public string TenHuong { get; set; } = string.Empty;
        public decimal? HeSoViTri { get; set; }
        public decimal? HeSoView { get; set; }
        public decimal? HeSoGoc { get; set; }
        public decimal? HeSoMatKhoi { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
    }

    public class ViewTrucImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaBlock { get; set; }
        public string? MaTruc { get; set; }
        public string? TenTruc { get; set; }
        public string? MaLoaiGoc { get; set; }
        public string? MaView { get; set; }
        public string MaViTri { get; set; } = string.Empty;
        public string MaMatKhoi { get; set; } = string.Empty;
        public string MaHuong { get; set; } = string.Empty;
        public decimal? HeSoTruc { get; set; }
        public int? STTTang { get; set; }
        public int RowIndex { get; set; } // dòng Excel (tính cả header)
    }
}
