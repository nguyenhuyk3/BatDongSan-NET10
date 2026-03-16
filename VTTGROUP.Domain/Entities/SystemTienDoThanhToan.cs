namespace VTTGROUP.Domain.Entities
{
    public class SystemTienDoThanhToan
    {
        public string? projectCode { get; set; }//Mã dự án
        public string? customerCode { get; set; }//Mã khách hàng
        public string? apartmentCode { get; set; }//Mã căn hộ
        public string? apartmentName { get; set; }//Tên căn hộ
        public string? contractCode { get; set; }//Mã hợp đồng
        public double? contractAmount { get; set; }//giá trị hợp đồng
        public string? paymentCode { get; set; }//Mã thanh toán
        public string? paymentNameVi { get; set; }//Nội dung thanh toán tiếng việt
        public string? paymentNameEn { get; set; }//Nội dung thanh toán tiếng Anh
        public double? paymentAmount { get; set; }//Số tiền thanh toán
        public double? paidAmount { get; set; }//Số tiền đã thanh toán
        public double? interestAmount { get; set; }//Số tiền lãi suất
        public DateTime? paymentDate { get; set; }//Ngày thanh toán
        public int? paymentStatusCode { get; set; }//Trạng thái thanh toán
        public string? paymentStatusName { get; set; }//Tên trạng thái thanh toán
        public string? paymentStatusNameEn { get; set; }//Tên trạng thái thanh toán Tiếng Anh
        public string? paymentDueDate { get; set; }//Hạn thanh toán
        public string? StatusColor { get; set; }//Mã màu trạng thái
        public string? StatusBgColor { get; set; }//Mã màu nền
        public string? StatusTextColor { get; set; }//Mã màu chữ trạng thái
        public string? TextNumberColor { get; set; }//Mã màu số
        public bool? isShowDetail { get; set; }// Hiển thị xem chi tiết
    }
}
