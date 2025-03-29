using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Phongkham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminLichdatController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public AdminLichdatController(ApplicationDBcontext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? date, string trangThai, string sortOrder)
        {
            // Lấy dữ liệu từ bảng lichKhams
            var lichKhamsQuery = _context.lichKhams
                .Include(lk => lk.Patient)
                .Include(lk => lk.KhungGio)
                .Select(lk => new
                {
                    lk.Id,
                    FullName = lk.Patient != null ? lk.Patient.FullName : "Khách",
                    lk.NgayDat,
                    lk.phone,
                    lk.TrangThai,
                    TimeSlot = lk.KhungGio != null ? lk.KhungGio.TimeSlot : "N/A",
                    HasIdentity = lk.Patient != null
                });

            // Lấy dữ liệu từ bảng lichKhamvls
            var lichKhamsVLQuery = _context.lichKhamvls
                .Include(lk => lk.KhungGio)
                .Select(lk => new
                {
                    lk.Id,
                    FullName = lk.UserName,  // Sử dụng tên người dùng cho khách
                    lk.NgayDat,
                    lk.phone,
                    lk.TrangThai,
                    TimeSlot = lk.KhungGio != null ? lk.KhungGio.TimeSlot : "N/A",
                    HasIdentity = false  // Vì là khách nên không có tài khoản
                });

            // Hợp nhất dữ liệu từ hai bảng
            var combinedQuery = lichKhamsQuery.Union(lichKhamsVLQuery);

            // Lọc theo ngày nếu có
            if (date.HasValue)
            {
                combinedQuery = combinedQuery.Where(lk => lk.NgayDat.Date == date.Value.Date);
            }

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(trangThai))
            {
                if (Enum.TryParse(typeof(TrangThaiCaKham), trangThai, true, out var trangThaiEnum))
                {
                    combinedQuery = combinedQuery.Where(lk => lk.TrangThai == (TrangThaiCaKham)trangThaiEnum);
                }
            }

            // Sắp xếp theo thứ tự
            switch (sortOrder)
            {
                case "date_asc":
                    combinedQuery = combinedQuery.OrderBy(lk => lk.NgayDat);
                    break;
                case "date_desc":
                    combinedQuery = combinedQuery.OrderByDescending(lk => lk.NgayDat);
                    break;
                case "patient_asc":
                    combinedQuery = combinedQuery.OrderBy(lk => lk.FullName);
                    break;
                case "patient_desc":
                    combinedQuery = combinedQuery.OrderByDescending(lk => lk.FullName);
                    break;
                case "timeSlot_asc":
                    combinedQuery = combinedQuery.OrderBy(lk => lk.TimeSlot);
                    break;
                case "timeSlot_desc":
                    combinedQuery = combinedQuery.OrderByDescending(lk => lk.TimeSlot);
                    break;
            }

            // Thực hiện truy vấn và lấy danh sách lịch khám
            var lichKhams = await combinedQuery.ToListAsync();
            ViewBag.CurrentSort = sortOrder;

            return View(lichKhams);
        }

    }
}
