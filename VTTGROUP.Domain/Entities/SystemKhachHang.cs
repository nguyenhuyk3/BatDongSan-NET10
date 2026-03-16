namespace VTTGROUP.Domain.Entities
{
    public class SystemKhachHang
    {
        //public string? customerCode { get; set; }
        //public string? idCard { get; set; }
        //public string? fullName { get; set; }
        //public string? phoneNumber { get; set; }
        //public string? email { get; set; }
        //public string? address { get; set; }
        //public string? contractCode { get; set; }
        //public string? projectCode { get; set; }
        //public string? apartmentCode { get; set; }

        public string? MaKhachHang { get; set; }
        public string? TenKhachHang { get; set; }
        public string? LoaiIdCard { get; set; }
        public string? TenLoaiIdCard { get; set; }
        public string? SoIdCard { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? MaDuAn { get; set; }
        public string? MaCanHo { get; set; }
        public string? MaHopDong { get; set; }
        public string? HinhAnhMatBangTang { get; set; }
        public string? HinhAnhLayoutLoaiCan { get; set; }
    }
    public class ApiResponse<T>
    {
        public string Status { get; set; } = "success";
        public T Data { get; set; }
        public string Message { get; set; } = "Thành công";
    }
}
