using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VTTGROUP.Application.TienDoThanhToan.Dto;
using VTTGROUP.Application.TienDoThanhToan;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Application.DanhMucTrangThai;
using VTTGROUP.Application.DanhMucTrangThai.Dto;

namespace VTTGROUP.API.Controllers
{
    //public class DanhMucTrangThaiController : Controller
    //{
    //    public IActionResult Index()
    //    {
    //        return View();
    //    }
    //}
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiKeyPolicy")]
    public class DanhMucTrangThaiController : ControllerBase
    {
        private readonly IDanhMucTrangThaiService _tdService;
        public DanhMucTrangThaiController(IDanhMucTrangThaiService tdService)
        {
            _tdService = tdService;
        }
        [HttpPost("danhmuctrangthai")]
        public async Task<IActionResult> GetDanhMucTrangThaiAsync([FromBody] DanhMucTrangThaiRequest request)
        {
            int pageIndex = request.pageIndex ?? 1;
            int numOfPage = request.numOfPage ?? 10;
            var listTD = await _tdService.GetDanhMucTrangThaiAsync(pageIndex, numOfPage);
            var response = new ApiResponse<List<SysDanhMucTrangThai>>
            {
                Data = listTD,
                Message = "Dữ liệu danh mục trạng thái trả về thành công.",
                Status = "success"
            };
            return Ok(response);
        }
    }
}
