namespace VTTGROUP.Domain.Entities
{
    public class SystemLichSuThanhToan
    {
        public string? projectCode { get; set; }//Mã dự án
        public string? customerCode { get; set; }//Mã khách hàng
        public string? apartmentCode { get; set; }//Mã căn hộ
        public DateTime? paymentDate { get; set; }//Ngày thanh toán
        public double? paidAmount { get; set; }//Số tiền thanh toán
        public string? maChungTu { get; set; }//Mã chứng từ
    }
}