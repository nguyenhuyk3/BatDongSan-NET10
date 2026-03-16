using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VTTGROUP.Application.KhachHang;
using VTTGROUP.Application.Menu;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Infrastructure.Services;

namespace VTTGROUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiKeyPolicy")]
    public class KhachHangController : ControllerBase
    {
        private readonly IKhachHangService _khService;
        public KhachHangController(IKhachHangService khService)
        {
            _khService = khService;
        }
        [HttpGet("khachhang")]
        public async Task<IActionResult> GetKhachHang()
        {
            var listKh = await _khService.GetKhachHangAsync(string.Empty, null, null, null);
            var response = new ApiResponse<List<SystemKhachHang>>
            {
                Data = listKh,
                Message = "Dữ liệu khách hàng được trả về thành công.",
                Status = "success"
            };
            return Ok(response);
        }
    }
}
