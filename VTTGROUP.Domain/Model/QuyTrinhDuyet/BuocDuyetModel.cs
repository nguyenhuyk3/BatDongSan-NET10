namespace VTTGROUP.Domain.Model.QuyTrinhDuyet
{
    public class BuocDuyetModel
    {
        public bool? IsCreate { get; set; }
        public string? MaBuocDuyet { get; set; }
        public string? TenBuocDuyet { get; set; }
        public string? TenHienThi { get; set; }
        public string? GhiChu { get; set; }
    }
    public class BuocDuyetPagingDto
    {
        public int STT { get; set; }
        public string? MaBuocDuyet { get; set; }
        public string? TenBuocDuyet { get; set; }
        public string? TenHienThi { get; set; }
        public string? GhiChu { get; set; }
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
    }
}
