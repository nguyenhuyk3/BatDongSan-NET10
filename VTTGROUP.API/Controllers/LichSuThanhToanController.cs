using Microsoft.AspNetCore.Mvc;
using VTTGROUP.Application.TienDoThanhToan.Dto;
using VTTGROUP.Application.TienDoThanhToan;
using VTTGROUP.Application.LichSuThanhToan;
using VTTGROUP.Application.LichSuThanhToan.Dto;
using System.Collections.Generic;
using VTTGROUP.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace VTTGROUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiKeyPolicy")]
    public class LichSuThanhToanController : ControllerBase
    {
        private readonly ILichSuThanhToanService _lsService;
        public LichSuThanhToanController(ILichSuThanhToanService lsService)
        {
            _lsService = lsService;
        }
        [HttpPost("lichsuthanhtoan")]
        public async Task<IActionResult> GetLichSuThanhToan([FromBody] LichSuThanhToanRequest request)
        {
            var listTD = await _lsService.GetLichSuThanhToanAsync(request.maDuAn, request.maKhachHang, request.maCanHo,request.maHopDong,request.maGiaiDoanTT);
            var response = new ApiResponse<List<SystemLichSuThanhToan>>
            {
                Data = listTD,
                Message = "Dữ liệu lịch sử thanh toán được trả về thành công.",
                Status = "success"
            };
            return Ok(response);
        }
    }
}
