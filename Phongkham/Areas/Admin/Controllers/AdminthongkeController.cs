using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;

namespace Phongkham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class Adminthongkecontroller : Controller
    {
        private readonly ApplicationDBcontext _context;

        public Adminthongkecontroller(ApplicationDBcontext context)
        {
            _context = context;
        }

        public IActionResult Index(string monthYear)
        {
            // Nếu không có giá trị monthYear, sử dụng tháng và năm hiện tại
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;

            if (!string.IsNullOrEmpty(monthYear))
            {
                var dateParts = monthYear.Split('-');
                year = int.Parse(dateParts[0]);
                month = int.Parse(dateParts[1]);
            }
            // Gán giá trị tháng-năm vào ViewData
            ViewData["SelectedMonthYear"] = monthYear;
            // Lấy tất cả dịch vụ
            var allDichvu = _context.dichvus.ToList();

            // Lấy dữ liệu thu nhập theo tháng và năm từ bảng PhieuKhamDichvu
            var thuNhapTheoThang = _context.PhieuKhamDichvus
                .Include(pkdv => pkdv.Dichvu)
                .Include(pkdv => pkdv.PhieuKham)
                .Where(pkdv => pkdv.PhieuKham.NgayKham.Month == month && pkdv.PhieuKham.NgayKham.Year == year)
                .GroupBy(pkdv => pkdv.Dichvu.ten)
                .Select(g => new
                {
                    TenDichvu = g.Key,
                    TongTien = g.Sum(pkdv => pkdv.Dichvu.Price),
                    SoLan = g.Count() // Tính số lần xuất hiện của DichvuId
                })
                .ToList();

            // Tìm số lần sử dụng cao nhất
            var maxSoLan = thuNhapTheoThang.Any() ? thuNhapTheoThang.Max(t => t.SoLan) : 0;

            // Lọc ra tất cả dịch vụ có số lần sử dụng cao nhất
            var mostPopularServices = thuNhapTheoThang.Where(t => t.SoLan == maxSoLan).ToList();

            // Kết hợp danh sách tất cả dịch vụ với kết quả thu nhập
            var result = allDichvu.Select(dv => new
            {
                TenDichvu = dv.ten,
                TongTien = thuNhapTheoThang
                    .Where(t => t.TenDichvu == dv.ten)
                    .Select(t => t.TongTien)
                    .FirstOrDefault(),
                SoLan = thuNhapTheoThang
                    .Where(t => t.TenDichvu == dv.ten)
                    .Select(t => t.SoLan)
                    .FirstOrDefault(),
                GiaDichvu = dv.Price,
            }).ToList();
            var tongTatCaTien = thuNhapTheoThang.Sum(t => t.TongTien);
            return View(new
            {
                Result = result,
                MaxRevenueServices = mostPopularServices,
                TongTatCaTien = thuNhapTheoThang.Sum(t => t.TongTien)
            });
        }
        public IActionResult IndexNgay(string date)
        {
            // Nếu không có giá trị date, sử dụng ngày hiện tại
            var selectedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(date))
            {
                selectedDate = DateTime.Parse(date); // Chuyển đổi ngày từ chuỗi sang kiểu DateTime
            }

            // Gán giá trị ngày vào ViewData
            ViewData["SelectedDate"] = selectedDate.ToString("yyyy-MM-dd");

            // Lấy tất cả dịch vụ
            var allDichvu = _context.dichvus.ToList();

            // Lấy dữ liệu thu nhập theo ngày từ bảng PhieuKhamDichvu
            var thuNhapTheoNgay = _context.PhieuKhamDichvus
                .Include(pkdv => pkdv.Dichvu)
                .Include(pkdv => pkdv.PhieuKham)
                .Where(pkdv => pkdv.PhieuKham.NgayKham.Date == selectedDate.Date)
                .GroupBy(pkdv => pkdv.Dichvu.ten)
                .Select(g => new
                {
                    TenDichvu = g.Key,
                    TongTien = g.Sum(pkdv => pkdv.Dichvu.Price),
                    SoLan = g.Count() // Tính số lần xuất hiện của DichvuId
                })
                .ToList();

            // Kết hợp danh sách tất cả dịch vụ với kết quả thu nhập
            var result = allDichvu.Select(dv => new
            {
                TenDichvu = dv.ten,
                TongTien = thuNhapTheoNgay
                    .Where(t => t.TenDichvu == dv.ten)
                    .Select(t => t.TongTien)
                    .FirstOrDefault(),
                SoLan = thuNhapTheoNgay
                    .Where(t => t.TenDichvu == dv.ten)
                    .Select(t => t.SoLan)
                    .FirstOrDefault(),
                GiaDichvu = dv.Price,
            }).ToList();

            return View(result.Cast<dynamic>().ToList());
        }
        public IActionResult IndexTheoNam(int? year)
        {
            // Nếu không có giá trị year, sử dụng năm hiện tại
            int currentYear = DateTime.Now.Year;
            if (year == null)
            {
                year = currentYear;
            }

            // Gán giá trị năm vào ViewData
            ViewData["SelectedYear"] = year;

            // Lấy tất cả dịch vụ
            var allDichvu = _context.dichvus.ToList();

            // Lấy dữ liệu thu nhập theo tháng trong năm từ bảng PhieuKhamDichvu
            var thuNhapTheoThang = _context.PhieuKhamDichvus
                .Include(pkdv => pkdv.Dichvu)
                .Include(pkdv => pkdv.PhieuKham)
                .Where(pkdv => pkdv.PhieuKham.NgayKham.Year == year)
                .GroupBy(pkdv => pkdv.PhieuKham.NgayKham.Month)
                .Select(g => new
                {
                    Thang = g.Key,
                    TongTien = g.Sum(pkdv => pkdv.Dichvu.Price),
                    SoLan = g.Count() // Tính số lần xuất hiện của DichvuId
                })
                .ToList();

            // Lọc lại nếu năm hiện tại để chỉ lấy các tháng từ 1 đến tháng hiện tại
            if (year == currentYear)
            {
                thuNhapTheoThang = thuNhapTheoThang.Where(t => t.Thang <= DateTime.Now.Month).ToList();
            }

            // Tạo một danh sách đầy đủ 12 tháng và kết hợp với dữ liệu thu nhập
            var allMonths = Enumerable.Range(1, 12).Select(i => new { Month = i }).ToList();

            var result = allMonths.Select(month => new
            {
                Month = month.Month,
                TongTien = thuNhapTheoThang
         .Where(t => t.Thang == month.Month)
         .Select(t => t.TongTien)
         .FirstOrDefault(),  // Nếu không có dữ liệu thì mặc định là 0
                SoLan = thuNhapTheoThang
         .Where(t => t.Thang == month.Month)
         .Select(t => t.SoLan)
         .FirstOrDefault(),  // Nếu không có dữ liệu thì mặc định là 0
                             // Tìm tháng có doanh thu lớn nhất và thấp nhất
                MaxRevenue = thuNhapTheoThang.Any() ? thuNhapTheoThang.Max(t => t.TongTien) : 0, // Kiểm tra có dữ liệu hay không
                MinRevenue = thuNhapTheoThang.Any() ? thuNhapTheoThang.Min(t => t.TongTien) : 0 // Kiểm tra có dữ liệu hay không
            }).ToList();

            // Trả về kết quả dưới dạng dynamic
            return View(result.Cast<dynamic>().ToList());
        }
        public IActionResult PieChartThang1(string monthYear)
        {
            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;

            // Nếu người dùng có chọn tháng và năm thì phân tách và lấy tháng, năm
            if (!string.IsNullOrEmpty(monthYear))
            {
                var dateParts = monthYear.Split('-');
                year = int.Parse(dateParts[0]);
                month = int.Parse(dateParts[1]);
            }
            // Gán giá trị tháng-năm vào ViewData
            ViewData["SelectedMonthYear"] = monthYear;

            // Log hoặc kiểm tra giá trị của ViewData
            Console.WriteLine("Selected Month Year: " + ViewData["SelectedMonthYear"]);
            // Lấy tất cả các ngày trong tháng
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var allDaysInMonth = Enumerable.Range(1, daysInMonth)
                                           .Select(day => new DateTime(year, month, day))
                                           .ToList();

            // Lọc dữ liệu theo tháng và năm
            var thuNhapTheoDichvu = _context.PhieuKhamDichvus
                .Include(pkdv => pkdv.Dichvu)
                .Include(pkdv => pkdv.PhieuKham)
                .Where(pkdv => pkdv.PhieuKham.NgayKham.Month == month && pkdv.PhieuKham.NgayKham.Year == year)
                .GroupBy(pkdv => pkdv.Dichvu.ten)
                .Select(g => new
                {
                    TenDichvu = g.Key,
                    TongTien = g.Sum(pkdv => pkdv.Dichvu.Price)
                })
                .ToList();

            // Thu nhập theo ngày
            var thuNhapTheoNgay = _context.PhieuKhamDichvus
                .Include(pkdv => pkdv.Dichvu)
                .Include(pkdv => pkdv.PhieuKham)
                .Where(pkdv => pkdv.PhieuKham.NgayKham.Month == month && pkdv.PhieuKham.NgayKham.Year == year)
                .GroupBy(pkdv => pkdv.PhieuKham.NgayKham.Date)
                .Select(g => new
                {
                    NgayKham = g.Key,
                    TongTien = g.Sum(pkdv => pkdv.Dichvu.Price)
                })
                .ToList();

            // Bổ sung các ngày không có dữ liệu vào thuNhapTheoNgay với giá trị 0đ
            var thuNhapTheoNgayFull = allDaysInMonth.Select(day =>
            {
                var existingDay = thuNhapTheoNgay.FirstOrDefault(t => t.NgayKham.Date == day.Date);
                return new
                {
                    NgayKham = day.Date,
                    TongTien = existingDay?.TongTien ?? 0 // Nếu không có dữ liệu, gán 0
                };
            }).ToList();

            // Chia thành nửa đầu và nửa cuối tháng
            var halfMonth = daysInMonth / 2;
            var firstHalf = thuNhapTheoNgayFull.Where(x => x.NgayKham.Day <= halfMonth).ToList();
            var secondHalf = thuNhapTheoNgayFull.Where(x => x.NgayKham.Day > halfMonth).ToList();

            // Truyền dữ liệu vào ViewData dưới dạng mảng (list) đơn giản
            ViewData["ChartDataDichvu"] = thuNhapTheoDichvu.Select(item => new
            {
                TenDichvu = item.TenDichvu,
                TongTien = item.TongTien
            }).ToList();

            ViewData["ChartDataNgayFirstHalf"] = firstHalf.Select(item => new
            {
                NgayKham = item.NgayKham.ToString("yyyy-MM-dd"),
                TongTien = item.TongTien
            }).ToList();

            ViewData["ChartDataNgaySecondHalf"] = secondHalf.Select(item => new
            {
                NgayKham = item.NgayKham.ToString("yyyy-MM-dd"),
                TongTien = item.TongTien
            }).ToList();

            return View();
        }
    }
}
