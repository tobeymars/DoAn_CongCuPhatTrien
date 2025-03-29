    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Phongkham.Data;
    using Phongkham.Models;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    namespace Phongkham.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = SD.Role_Patient)]
        public class DanglichController : Controller
        {
            private readonly ApplicationDBcontext _context;
            private readonly UserManager<ApplicationUser> _userManager;

            public DanglichController(ApplicationDBcontext context, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            var lichKhams = await _context.lichKhams
                .Where(l => l.PatientId == user.Id)
                .Include(l => l.KhungGio) // Nếu bạn muốn hiển thị thông tin khung giờ
                .ToListAsync();

            return View(lichKhams);
        }

        public async Task<IActionResult> Create()
            {
                // Lấy thông tin người dùng đang đăng nhập
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account"); // Hoặc trang phù hợp
                }

                ViewBag.Phone = user.PhoneNumber; // Gửi số điện thoại vào ViewBag để hiển thị trong form

                // Lấy danh sách khung giờ để người dùng chọn
                var khungGios = await _context.KhungGios.ToListAsync();
                ViewBag.KhungGios = khungGios; // Đảm bảo rằng khungGios không phải là null
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> Create(lichKham lichKham)
        {
            var user = await _userManager.GetUserAsync(User);
            // Kiểm tra xem ngày đặt có phải là trong tương lai
            if (lichKham.NgayDat.Date < DateTime.Now.Date.AddDays(2))
                    {
                        ModelState.AddModelError("NgayDat", "Hãy đặt từ 2 ngày sau trở đi.VD:hôm nay 14/11 thì đặt 16/11 trở đi");
                ViewBag.Phone = user.PhoneNumber;
                ViewBag.KhungGios = await _context.KhungGios.ToListAsync(); // Lấy danh sách khung giờ
                return View(lichKham);
                    }

                    // Kiểm tra xem khung giờ đã được đặt hay chưa
                    var existingBooking = await _context.lichKhams
                        .AnyAsync(l => l.KhungGioId == lichKham.KhungGioId && l.NgayDat.Date == lichKham.NgayDat.Date);

                    if (existingBooking)
                    {
                        ModelState.AddModelError("KhungGioId", "Khung giờ đã được đặt cho ngày này.");
                ViewBag.Phone = user.PhoneNumber;
                ViewBag.KhungGios = await _context.KhungGios.ToListAsync(); // Lấy danh sách khung giờ
                return View(lichKham);
                    }

                    lichKham.UserName = user.FullName; // Lấy tên người dùng
                    lichKham.PatientId = user.Id; // Lấy ID người dùng
             // Nếu người dùng có số điện thoại, thì gán vào lichKham.phone, nếu không có, người dùng sẽ nhập vào form       
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                lichKham.phone = user.PhoneNumber;
            }
            else
            {
                lichKham.phone = string.IsNullOrEmpty(lichKham.phone) ? "" : lichKham.phone;
                user.PhoneNumber = lichKham.phone;

                // Kiểm tra số điện thoại đã tồn tại trong lichKhams và lichKhamVls
                var existingPhone = await _context.lichKhams
                    .AnyAsync(l => l.phone == lichKham.phone) || await _context.lichKhamvls
                    .AnyAsync(l => l.phone == lichKham.phone);

                if (existingPhone)
                {
                    ModelState.AddModelError("phone", "Số điện thoại này đã dùng cho 1 bệnh nhân khác.");
                    ViewBag.KhungGios = await _context.KhungGios.ToListAsync(); // Lấy danh sách khung giờ
                    return View(lichKham);
                }
                else
                {
                    _context.Update(user);
                }
            }

            // Thêm lichKham vào cơ sở dữ liệu
            _context.lichKhams.Add(lichKham);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Danglich"); // Chuyển hướng đến trang chính sau khi thành công
            }
        public async Task<IActionResult> Edit(int id)
        {
            var lichKham = await _context.lichKhams.FindAsync(id);
            if (lichKham == null)
            {
                return NotFound();
            }            
            var user = await _userManager.GetUserAsync(User);
            if (user == null || lichKham.PatientId != user.Id)
            {
                return Unauthorized();
            }
            var hasScheduled = await _context.cTlichkhams
                                              .AnyAsync(c => c.LichkhamId == lichKham.Id);

            if (hasScheduled)
            {
                // Nếu đã lên lịch, thông báo và không cho phép sửa
                TempData["ErrorMessage"] = "Lịch khám đã được lên lịch, không thể sửa.";
                return RedirectToAction("Index"); // Hoặc bạn có thể trả về một trang khác hoặc giữ lại trang hiện tại
            }
            if (lichKham.NgayDat <= DateTime.Now.AddDays(1))
            {
                // Hiển thị thông báo lỗi nếu đã qua ngày
                TempData["ErrorMessage"] = "Không thể sửa lịch khám đã đến ngày.";
                return RedirectToAction("Index", "Danglich");
            }
            ViewBag.Phone = user.PhoneNumber; // Gửi số điện thoại vào ViewBag
            ViewBag.KhungGios = await _context.KhungGios.ToListAsync(); // Lấy danh sách khung giờ
            return View(lichKham);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, lichKham lichKham)
        {            
            if (id != lichKham.Id)
            {
                return NotFound();
            }
                // Kiểm tra xem ngày đặt có phải là trong tương lai
                if (lichKham.NgayDat.Date < DateTime.Now.Date.AddDays(2))
                {
                    ModelState.AddModelError("NgayDat", "Hãy đặt từ 2 ngày sau trở đi.VD:hôm nay 14/11 thì đặt 16/11 trở đi");
                ViewBag.KhungGios = await _context.KhungGios.ToListAsync();
                return View(lichKham);
                }
                // Kiểm tra xem khung giờ đã được đặt hay chưa
                var existingBooking = await _context.lichKhams
                    .AnyAsync(l => l.KhungGioId == lichKham.KhungGioId
                                && l.NgayDat.Date == lichKham.NgayDat.Date
                                && l.Id != lichKham.Id); // Đảm bảo không kiểm tra lịch khám hiện tại

                if (existingBooking)
                {
                    ModelState.AddModelError("KhungGioId", "Khung giờ đã được đặt cho ngày này.");
                ViewBag.KhungGios = await _context.KhungGios.ToListAsync();
                return View(lichKham);
                }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Gán lại PatientId và các thông tin cần thiết khác
            lichKham.PatientId = user.Id;
            lichKham.UserName = user.FullName; // Cập nhật UserName nếu cần

            try
                {
                    _context.Update(lichKham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!lichKhamExists(lichKham.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            return RedirectToAction(nameof(Index));
        }
        private bool lichKhamExists(int id)
        {
            return _context.lichKhams.Any(e => e.Id == id);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var lichKham = await _context.lichKhams
                .Include(l => l.KhungGio) // Bao gồm thông tin về khung giờ
                .FirstOrDefaultAsync(l => l.Id == id);
            var hasScheduled = await _context.cTlichkhams
                                             .AnyAsync(c => c.LichkhamId == lichKham.Id);

            if (hasScheduled)
            {
                // Nếu đã lên lịch, thông báo và không cho phép sửa
                TempData["ErrorMessage"] = "Lịch khám đã được lên lịch, không thể xóa.";
                return RedirectToAction("Index"); // Hoặc bạn có thể trả về một trang khác hoặc giữ lại trang hiện tại
            }
            // Kiểm tra nếu lịch khám đã qua ngày
            if (lichKham.NgayDat <= DateTime.Now.AddDays(1))
            {
                // Hiển thị thông báo lỗi nếu đã qua ngày
                TempData["ErrorMessage"] = "Không thể xóa lịch khám đã đến ngày.";
                return RedirectToAction("Index", "Danglich");
            }
            if (lichKham == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || lichKham.PatientId != user.Id)
            {
                return Unauthorized();
            }

            return View(lichKham);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichKham = await _context.lichKhams
                .Include(l => l.KhungGio) // Bao gồm thông tin về khung giờ
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lichKham == null)
            {
                return NotFound();
            }       

            // Cập nhật trạng thái trong CakhamKhungGio về trạng thái "Chưa Đặt"
            var khungGio = await _context.CakhamKhungGios
                .FirstOrDefaultAsync(kg => kg.KhungGioId == lichKham.KhungGioId && kg.Cakham.NgayDang==lichKham.NgayDat); // Giả sử bạn có Id cho KhungGio trong lichKham

            if (khungGio != null)
            {
                khungGio.TrangThai = TrangThaiCaKham.Chưa_Đặt; // Giả sử bạn có trạng thái Chưa Đặt trong enum TrangThaiCaKham
            }
            await _context.SaveChangesAsync();
            _context.lichKhams.Remove(lichKham);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
