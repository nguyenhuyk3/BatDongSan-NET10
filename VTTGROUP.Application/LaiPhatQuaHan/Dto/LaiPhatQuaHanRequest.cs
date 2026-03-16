using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTTGROUP.Application.LaiPhatQuaHan.Dto
{
    public class LaiPhatQuaHanRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập mã dự án")]
        [DefaultValue("")]
        public string maDuAn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã khách hàng")]
        [DefaultValue("")]
        public string maKhachHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã căn hộ")]
        [DefaultValue("")]
        public string maCanHo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã giai đoạn thanh toán")]
        [DefaultValue("")]
        public string maGiaiDoanTT { get; set; } = string.Empty;
    }
}
