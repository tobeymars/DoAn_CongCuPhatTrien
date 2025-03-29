using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using Phongkham.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Phongkham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminCakhams : Controller
    {
        private readonly ApplicationDBcontext _context;

        public AdminCakhams(ApplicationDBcontext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? date, int? chuyenMonId, string trangThai, string sortOrder)
        {
            var cakhams = await _context.Cakhams
                .Include(c => c.Dentist)
                .ThenInclude(d => d.cTnhasis)
                .ThenInclude(ct => ct.Chuyenmon)
                .Include(c => c.CakhamKhungGios)
                .ThenInclude(kg => kg.KhungGio)
                .ToListAsync();

            // Lọc theo ngày nếu có
            if (date.HasValue)
            {
                cakhams = cakhams.Where(c => c.NgayDang.Date == date.Value.Date).ToList();
            }

            // Lọc theo chuyên môn nếu có
            if (chuyenMonId.HasValue)
            {
                cakhams = cakhams.Where(c => c.Dentist.cTnhasis.Any(ct => ct.chuyenmonId == chuyenMonId.Value)).ToList();
            }

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(trangThai))
            {
                if (trangThai == TrangThaiCaKham.Đã_Đặt.ToString())
                {
                    // Lọc cakhams với trạng thái Đã Đặt và chỉ lấy các khung giờ đã đặt
                    cakhams = cakhams
                        .Where(c => c.CakhamKhungGios
                            .Any(kg => kg.TrangThai == TrangThaiCaKham.Đã_Đặt))
                        .ToList();

                    // Cập nhật danh sách cakhams để chỉ chứa thông tin cần thiết
                    foreach (var caKham in cakhams)
                    {
                        caKham.CakhamKhungGios = caKham.CakhamKhungGios
                            .Where(kg => kg.TrangThai == TrangThaiCaKham.Đã_Đặt)
                            .ToList();
                    }
                }
                else if (trangThai == TrangThaiCaKham.Chưa_Đặt.ToString())
                {
                    // Lọc cakhams với trạng thái Chưa Đặt và chỉ lấy các khung giờ chưa đặt
                    cakhams = cakhams
                        .Where(c => c.CakhamKhungGios
                            .Any(kg => kg.TrangThai == TrangThaiCaKham.Chưa_Đặt))
                        .ToList();

                    // Cập nhật danh sách cakhams để chỉ chứa thông tin cần thiết
                    foreach (var caKham in cakhams)
                    {
                        caKham.CakhamKhungGios = caKham.CakhamKhungGios
                            .Where(kg => kg.TrangThai == TrangThaiCaKham.Chưa_Đặt)
                            .ToList();
                    }
                }
                else
                {
                    // Lọc theo trạng thái khác nếu có
                    cakhams = cakhams
                        .Where(c => c.CakhamKhungGios
                            .Any(kg => kg.TrangThai.ToString() == trangThai))
                        .ToList();

                    // Cập nhật danh sách cakhams để chỉ chứa thông tin cần thiết
                    foreach (var caKham in cakhams)
                    {
                        caKham.CakhamKhungGios = caKham.CakhamKhungGios
                            .Where(kg => kg.TrangThai.ToString() == trangThai)
                            .ToList();
                    }
                }
            }

            // Sắp xếp theo yêu cầu
            switch (sortOrder)
            {
                case "date_asc":
                    cakhams = cakhams.OrderBy(c => c.NgayDang).ToList();
                    break;
                case "date_desc":
                    cakhams = cakhams.OrderByDescending(c => c.NgayDang).ToList();
                    break;
                case "dentist_asc":
                    cakhams = cakhams.OrderBy(c => c.Dentist.FullName).ToList();
                    break;
                case "dentist_desc":
                    cakhams = cakhams.OrderByDescending(c => c.Dentist.FullName).ToList();
                    break;
            }

            ViewBag.CurrentSort = sortOrder;
            ViewBag.ChuyenMons = await _context.Chuyenmons.ToListAsync();
            return View(cakhams);
        }
    }
}
