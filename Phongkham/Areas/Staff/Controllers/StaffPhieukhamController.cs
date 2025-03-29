using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;

namespace Phongkham.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = SD.Role_Staff)]
    public class StaffPhieukhamController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public StaffPhieukhamController(ApplicationDBcontext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string TenBenhNhan, DateTime? NgayKham)
        {
            var phieuKhamDichvusGrouped = _context.PhieuKhamDichvus
                .Include(p => p.PhieuKham)
                .Include(p => p.Dichvu)
                .AsQueryable();

            // Lọc theo tên bệnh nhân nếu có
            if (!string.IsNullOrEmpty(TenBenhNhan))
            {
                phieuKhamDichvusGrouped = phieuKhamDichvusGrouped.Where(p => p.PhieuKham.TenBenhNhan.Contains(TenBenhNhan));
            }

            // Lọc theo ngày khám nếu có
            if (NgayKham.HasValue)
            {
                phieuKhamDichvusGrouped = phieuKhamDichvusGrouped.Where(p => p.PhieuKham.NgayKham.Date == NgayKham.Value.Date);
            }

            // Nhóm theo PhieuKhamId và tính toán tổng tiền
            var phieuKhamDichvusGroupedResult = await phieuKhamDichvusGrouped
                .GroupBy(p => p.PhieuKhamId)
                .ToListAsync();

            var result = phieuKhamDichvusGroupedResult.Select(group => new
            {
                PhieuKham = group.FirstOrDefault().PhieuKham,
                Dichvus = group.Select(p => p.Dichvu).ToList(),
                TotalPrice = group.Sum(p => p.Dichvu.Price)
            }).ToList();

            return View(result);
        }
    }
}
