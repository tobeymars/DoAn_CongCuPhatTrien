using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;

namespace Phongkham.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = SD.Role_Staff)]
    public class TaikhamController : Controller
    {
        private readonly ApplicationDBcontext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public TaikhamController(ApplicationDBcontext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string reminder, int? id)
        {
            var taikhamList = await _context.phieukhams
                                             .Where(p => p.TaiKham) // Chỉ lấy các phiếu khám có trạng thái tái khám
                                             .ToListAsync();

            // Nếu có yêu cầu 'Lời nhắc hôm nay'
            if (reminder == "true")
            {
                var today = DateTime.Now.Date; // Lấy ngày hiện tại không có giờ
                taikhamList = taikhamList.Where(p => p.NgayKham.AddDays(7).Date == today).ToList();
            }

            // Đếm số lượng thông báo (số phiếu khám cần lời nhắc)
            // Chỉ lấy các phiếu khám mà TrangThaiTaiKham là false
            var reminderCount = taikhamList.Count(p => p.NgayKham.AddDays(7).Date == DateTime.Now.Date && !p.TrangThaiTaiKham);

            // Gửi số lượng thông báo vào ViewBag
            ViewBag.ReminderCount = reminderCount;

            // Nếu có yêu cầu cập nhật trạng thái (id là phiếu khám được chọn)
            if (id.HasValue)
            {
                var phieukham = await _context.phieukhams.FirstOrDefaultAsync(p => p.Id == id.Value);

                if (phieukham != null)
                {
                    // Cập nhật trạng thái tái khám thành true
                    phieukham.TrangThaiTaiKham = true;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.Update(phieukham);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại số lượng thông báo sau khi xác nhận
                    reminderCount = taikhamList.Count(p => p.NgayKham.AddDays(7).Date == DateTime.Now.Date && !p.TrangThaiTaiKham);

                    // Gửi lại số lượng thông báo vào ViewBag
                    ViewBag.ReminderCount = reminderCount;            
                }
            }
            // Lấy số điện thoại của bệnh nhân và nha sĩ từ các bảng liên quan
            var phoneNumbers = taikhamList.ToDictionary(phieukham => phieukham.Id, phieukham =>
            {
                // Kiểm tra nếu có CTlichkhamId (khóa ngoại)
                if (phieukham.CTlichkhamId.HasValue)
                {
                    var phoneFromCTlichkham = (from ct in _context.cTlichkhams
                                               join lk in _context.lichKhams
                                               on ct.LichkhamId equals lk.Id
                                               where ct.TenBenhNhan == phieukham.TenBenhNhan && ct.TenNhaSi == phieukham.TenNhaSi
                                               select lk.phone).FirstOrDefault();

                    if (phoneFromCTlichkham != null)
                    {
                        return phoneFromCTlichkham;
                    }
                }

                // Nếu không có số điện thoại từ CTlichkham, kiểm tra CtlichkhamVLId (khóa ngoại khác)
                if (phieukham.CtlichkhamVLId.HasValue)
                {
                    var phoneFromCtlichkhamVL = (from ctvl in _context.Ctlkvl
                                                 join lkv in _context.lichKhamvls
                                                 on ctvl.LichkhamVLId equals lkv.Id
                                                 where ctvl.TenBenhNhan == phieukham.TenBenhNhan && ctvl.TenNhaSi == phieukham.TenNhaSi
                                                 select lkv.phone).FirstOrDefault();

                    if (phoneFromCtlichkhamVL != null)
                    {
                        return phoneFromCtlichkhamVL;
                    }
                }

                // Trả về null hoặc số điện thoại mặc định nếu không tìm thấy
                return null;
            });

            ViewBag.PhoneNumbers = phoneNumbers;
            return View(taikhamList);
        }
        // GET: TaikhamController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
    }
}
