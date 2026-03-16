using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTTGROUP.Domain.Entities
{
    public class SystemLaiPhatQuaHan
    {
        public string? apartmentCode { get; set; }//Mã căn hộ
        public string? paymentCode { get; set; }//Mã giai đoạn thanh toán
        public DateTime? startDate { get; set; }//Ngày bắt đầu
        public DateTime? endDate { get; set; }//Ngày kết thúc
        public double? interestPrincipal { get; set; }//Tiền tính lãi
        public int? overdueDate { get; set; }//Số ngày quá hạn
        public double? dailyPenaltyRate { get; set; }//Lãi suất quá hạn 
        public double? interestAmount { get; set; }//Tiền lãi
        public double? paidAmount { get; set; }//Số tiền đã đóng
        public double? discountAmount { get; set; }//Giảm trừ
        public double? amountDue { get; set; }//Số tiền còn lại phải đóng
    }
}
