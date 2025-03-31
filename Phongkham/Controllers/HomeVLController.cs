using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Phongkham.Areas.Patient.Controllers;
using Phongkham.Data;
using Phongkham.Models;
//aaaa
namespace Phongkham.Controllers
{
    public class HomeVLController : Controller
    {
        private readonly ApplicationDBcontext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeVLController( ApplicationDBcontext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách loại tin tức
            var loaiTinTuc = await _context.Loaitintucs.ToListAsync();

            // Lấy danh sách tin tức
            var tinTuc = await _context.Tintucs
                .OrderByDescending(t => t.NgayDang)
                .Take(3) // Lấy 3 tin tức mới nhất
                .ToListAsync();

            // Kiểm tra nếu không có tin tức
            ViewData["TinTuc"] = tinTuc ?? new List<Tintuc>(); // Truyền danh sách rỗng nếu không có tin tức

            // Lấy danh sách tất cả nha sĩ
            var dentists = await _context.cTnhasis
                .Include(c => c.Chuyenmon) // Bao gồm thông tin chuyên môn
                .Include(c => c.User) // Bao gồm thông tin người dùng
                .Select(d => new
                {
                    d.Id,
                    PhoneNumber = d.User.PhoneNumber,
                    FullName = d.User.FullName,
                    Email = d.User.UserName,
                    ImageUrl = d.User.ImageUrl,
                    Specialization = d.Chuyenmon.Name
                })
                .ToListAsync();

            // Chuyển danh sách nha sĩ sang JSON
            ViewData["Dentists"] = JsonConvert.SerializeObject(dentists);
            var services = await _context.dichvus
        .Include(s => s.Images)  // Bao gồm danh sách hình ảnh
        .Where(s => !s.IsDeleted)  // Lọc dịch vụ chưa xóa
        .ToListAsync(); // Lấy tất cả dịch vụ từ bảng Dichvu

            // Truyền dữ liệu dịch vụ vào View
            ViewData["Services"] = services;
            // Truyền dữ liệu tới View
            ViewData["LoaiTinTuc"] = loaiTinTuc;

            return View(); // Trả về View
        }
        public async Task<IActionResult> IndexTTVL(int? loaiTinTucId)
        {
            var loaiTinTuc = _context.Loaitintucs.ToList();
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
        public async Task<IActionResult> DetailVL(int id)
        {
            var loaiTinTuc = _context.Loaitintucs.ToList();
            var tintuc = await _context.Tintucs.Include(t => t.Loaitintuc).FirstOrDefaultAsync(t => t.Id == id);
            if (tintuc == null)
            {
                return NotFound();
            }
            ViewData["LoaiTinTuc"] = loaiTinTuc;
            return View(tintuc);
        }
        public IActionResult DetailDCVL()
        {
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            return View();
        }
        public async Task<IActionResult> ChatClient()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserId = user.Id;
            return View();
        }
        public async Task<IActionResult> ChatAdmin()
        {
            return View();
        }
        public IActionResult DichVU()
        {
            var services = _context.dichvus
                                   .Where(d => !d.IsDeleted) // Chỉ lấy dịch vụ chưa bị xóa
                                   .ToList();

            return View(services);
        }
    }
}
