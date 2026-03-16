using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTTGROUP.Application.KyThanhToan.Dto
{
    public class KyThanhToanRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập mã dự án")]
        [DefaultValue("")]
        public string maDuAn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã khách hàng")]
        [DefaultValue("")]
        public string maKhachHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã hợp đồng")]
        [DefaultValue("")]
        public string maHopDong { get; set; } = string.Empty;
    }
}
