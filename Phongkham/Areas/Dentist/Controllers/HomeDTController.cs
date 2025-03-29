using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;

using Phongkham.Models;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Phongkham.Areas.Dentist.Controllers
{
    [Area("Dentist")]
    [Authorize(Roles = SD.Role_Dentist)]
    public class HomeDTController : Controller
    {
        // GET: HomeDTController
        private readonly ApplicationDBcontext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeDTController(ApplicationDBcontext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string monthYear)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Người dùng không tồn tại hoặc không đăng nhập.");
            }

            var dentistId = currentUser.Id;

            // Nếu không có giá trị monthYear, sử dụng tháng và năm hiện tại
            if (string.IsNullOrEmpty(monthYear) || !DateTime.TryParseExact(monthYear, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                monthYear = DateTime.Now.ToString("yyyy-MM"); // Sử dụng tháng năm hiện tại nếu không hợp lệ
            }

            var selectedMonth = int.Parse(monthYear.Substring(5, 2));  // Lấy tháng
            var selectedYear = int.Parse(monthYear.Substring(0, 4));   // Lấy năm

            // Lấy danh sách lịch khám cho nha sĩ hiện tại từ cả 2 bảng CTlichkham và CtlichkhamVL
            var lichKhams = await _context.cTlichkhams
                .Join(_context.lichKhams, ct => ct.LichkhamId, lk => lk.Id, (ct, lk) => new { ct, lk })
                .Where(joined => joined.ct.TenNhaSi == currentUser.FullName)
                .Select(joined => new
                {
                    NgayDat = joined.lk.NgayDat,
                    KhungGio = joined.lk.KhungGio.TimeSlot,
                })
                .ToListAsync();

            var lichKhamVLs = await _context.Ctlkvl
                .Join(_context.lichKhamvls, ctvl => ctvl.LichkhamVLId, lkv => lkv.Id, (ctvl, lkv) => new { ctvl, lkv })
                .Where(joined => joined.ctvl.TenNhaSi == currentUser.FullName)
                .Select(joined => new
                {
                    NgayDat = joined.lkv.NgayDat,
                    KhungGio = joined.lkv.KhungGio.TimeSlot,
                })
                .ToListAsync();

            // Kết hợp dữ liệu từ hai bảng
            var allLichKhams = lichKhams.Concat(lichKhamVLs).ToList();

            // Lọc các lịch theo tháng và năm người dùng chọn
            var filteredLichKhams = allLichKhams
                .Where(lk => lk.NgayDat.Month == selectedMonth && lk.NgayDat.Year == selectedYear)
                .ToList();

            // Đếm số ngày có lịch
            var daysWithAppointments = filteredLichKhams
                .GroupBy(lk => lk.NgayDat.Date)
                .Select(group => new { day = group.Key, count = group.Count() })
                .ToList();
            int totalDaysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);  // Tổng số ngày trong tháng
            var allDaysInMonth = Enumerable.Range(1, totalDaysInMonth)
                                             .Select(day => new DateTime(selectedYear, selectedMonth, day))
                                             .ToList();
            var totalDaysWithAppointments = daysWithAppointments.Count;
            var totalDaysWithoutAppointments = totalDaysInMonth - totalDaysWithAppointments;
            var timeSlotsWithAppointments = filteredLichKhams
                .Select(lk => lk.KhungGio)  // Chỉ lấy khung giờ
                .ToList();
            var totalHours = timeSlotsWithAppointments.Count; int totalHoursInMonth = totalDaysInMonth * 24;
            var hoursByDay = allDaysInMonth.Select(day =>
            {
                var appointmentsForDay = filteredLichKhams.Where(lk => lk.NgayDat.Date == day).ToList();
                int hoursWorked = appointmentsForDay.Count();
                return new
                {
                    Day = day,
                    HoursWorked = hoursWorked
                };
            }).ToList();
            var firstHalf = hoursByDay.Where(x => x.Day.Day <= 15).ToList();
            var secondHalf = hoursByDay.Where(x => x.Day.Day > 15).ToList();
            // Truyền dữ liệu vào ViewBag
            ViewBag.FirstHalf = firstHalf;
            ViewBag.SecondHalf = secondHalf;
            ViewBag.TotalDaysInMonth = totalDaysInMonth;
            ViewBag.TotalHoursInMonth = totalHoursInMonth;
            ViewBag.TotalDaysWithAppointments = totalDaysWithAppointments;
            ViewBag.TotalDaysWithoutAppointments = totalDaysWithoutAppointments;
            ViewBag.TotalHours = totalHours;
            ViewBag.SelectedMonthYear = monthYear;

            return View();
        }

        public async Task<IActionResult> IndexTT(int? loaiTinTucId)
        {
            var loaiTinTuc = _context.Loaitintucs.ToList();
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Người dùng không tồn tại hoặc không đăng nhập.");
            }

            // Lấy tin tức thuộc loại tin tức được chọn (nếu có)
            var tinTucQuery = _context.Tintucs.AsQueryable();
            if (loaiTinTucId.HasValue)
            {
                tinTucQuery = tinTucQuery.Where(tt => tt.LoaitintucId == loaiTinTucId);
            }
            var tinTuc = await tinTucQuery.ToListAsync();

            ViewData["SelectedLoaiTinTucId"] = loaiTinTucId;
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View(tinTuc);
        }

        public async Task<IActionResult> IndexLK(DateTime? ngayKham)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Người dùng không tồn tại hoặc không đăng nhập.");
            }
            ViewData["CurrentUserFullName"] = currentUser.FullName;

            // Lấy dữ liệu từ bảng phieukhams
            var phieuKhams = await _context.phieukhams
                .Select(phieu => new
                {
                    TenBenhNhan = phieu.TenBenhNhan,
                    TenNhaSi = phieu.TenNhaSi,
                    NgayKham = phieu.NgayKham.ToString("dd/MM/yyyy"),
                    KhungGio = phieu.TimeSlot
                })
                .ToListAsync();

            // Lấy dữ liệu từ bảng ctlichkham với điều kiện lọc theo ngày nếu có
            var lichKhamsCTQuery = _context.cTlichkhams
                .Where(ct => ct.TenNhaSi == currentUser.FullName);

            if (ngayKham.HasValue)
            {
                lichKhamsCTQuery = lichKhamsCTQuery.Where(ct => ct.LichKham.NgayDat.Date == ngayKham.Value.Date);
            }

            var lichKhamsCT = await lichKhamsCTQuery
                .Select(ct => new
                {
                    TenBenhNhan = ct.TenBenhNhan,
                    PhongKham = ct.PhongKham,
                    LichKhamPhone = ct.LichKham.phone,
                    LichKhamNgay = ct.LichKham.NgayDat.ToString("dd/MM/yyyy"),
                    LichKhamGio = ct.LichKham.KhungGio.TimeSlot,
                    KhungGioId = ct.LichKham.KhungGioId,
                    Source = "CTlichkham",
                    LichKhamId = ct.Id
                })
                .ToListAsync();

            // Lấy dữ liệu từ bảng ctlichkhamvl với điều kiện lọc theo ngày nếu có
            var lichKhamsVLQuery = _context.Ctlkvl
                .Where(vl => vl.TenNhaSi == currentUser.FullName);

            if (ngayKham.HasValue)
            {
                lichKhamsVLQuery = lichKhamsVLQuery.Where(vl => vl.LichKhamVL.NgayDat.Date == ngayKham.Value.Date);
            }

            var lichKhamsVL = await lichKhamsVLQuery
                .Select(vl => new
                {
                    TenBenhNhan = vl.TenBenhNhan,
                    PhongKham = vl.PhongKham,
                    LichKhamPhone = vl.LichKhamVL.phone,
                    LichKhamNgay = vl.LichKhamVL.NgayDat.ToString("dd/MM/yyyy"),
                    LichKhamGio = vl.LichKhamVL.KhungGio.TimeSlot,
                    KhungGioId = vl.LichKhamVL.KhungGioId,
                    Source = "CtlichkhamVL",
                    LichKhamId = vl.Id
                })
                .ToListAsync();

            // Kết hợp cả hai danh sách vào một
            var lichKhams = lichKhamsCT.Concat(lichKhamsVL).ToList();

            // Gửi dữ liệu đến View
            ViewData["LichKhams"] = lichKhams;
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            ViewData["PhieuKhams"] = phieuKhams;
            ViewData["SelectedDate"] = ngayKham?.ToString("yyyy-MM-dd");
            return View();
        }


        // GET: HomeDTController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HomeDTController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HomeDTController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeDTController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HomeDTController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeDTController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeDTController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
