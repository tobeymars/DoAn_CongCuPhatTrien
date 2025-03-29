using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;

namespace Phongkham.Controllers
{
    public class DLKVLController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public DLKVLController(ApplicationDBcontext context)
        {
            _context = context;
        }

        // GET: LichKham/Create
        public async Task<IActionResult> CreateAsync()
        {
            var khungGios = await _context.KhungGios.ToListAsync();
            ViewBag.KhungGios = khungGios; // Đảm bảo rằng khungGios không phải là null
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            return View();
        }

        // POST: LichKham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichKhamVL lichKhamVL)
        {
            // Kiểm tra xem ngày đặt có phải là trong tương lai
            if (lichKhamVL.NgayDat.Date < DateTime.Now.Date.AddDays(2))
            {
                ModelState.AddModelError("NgayDat", "Hãy đặt từ 2 ngày sau trở đi.VD:hôm nay 14/11 thì đặt 16/11 trở đi");
                ViewBag.KhungGios = await _context.KhungGios.ToListAsync(); // Lấy danh sách khung giờ
                return View(lichKhamVL);
            }

            // Kiểm tra xem khung giờ đã được đặt hay chưa
            var existingBooking = await _context.lichKhamvls
                .AnyAsync(l => l.KhungGioId == lichKhamVL.KhungGioId && l.NgayDat.Date == lichKhamVL.NgayDat.Date);

            if (existingBooking)
            {
                ModelState.AddModelError("KhungGioId", "Khung giờ đã được đặt cho ngày này.");
                ViewBag.KhungGios = await _context.KhungGios.ToListAsync(); // Lấy danh sách khung giờ
                return View(lichKhamVL);
            }

            // Kiểm tra số điện thoại đã tồn tại trong lichKhams và lichKhamvls
            var existingPhoneInLichKhams = await _context.lichKhams
                .FirstOrDefaultAsync(l => l.phone == lichKhamVL.phone);
            var existingPhoneInLichKhamVls = await _context.lichKhamvls
                .FirstOrDefaultAsync(l => l.phone == lichKhamVL.phone);

            if (existingPhoneInLichKhams != null || existingPhoneInLichKhamVls != null)
            {
                // Kiểm tra tên bệnh nhân có khớp không
                var existingPatientName = existingPhoneInLichKhams?.UserName ?? existingPhoneInLichKhamVls?.UserName;

                if (existingPatientName != null && !existingPatientName.Equals(lichKhamVL.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("phone", "Số điện thoại này đã dùng cho 1 bệnh nhân khác.");
                    ViewBag.KhungGios = await _context.KhungGios.ToListAsync(); // Lấy danh sách khung giờ
                    return View(lichKhamVL);
                }
            }

            // Lưu lịch khám
            _context.Add(lichKhamVL);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Search)); // Redirect về trang danh sách lịch khám
        }


        // GET: LichKham/Search
        public async Task<IActionResult> SearchAsync(string sortOrder)
        {
            var lichKhamsVL = _context.lichKhamvls
                .Include(lk => lk.KhungGio).ToList();
            switch (sortOrder)
            {
                case "patient_asc":
                    lichKhamsVL = lichKhamsVL.OrderBy(lk => lk.UserName).ToList();
                    break;
                case "patient_desc":
                    lichKhamsVL = lichKhamsVL.OrderByDescending(lk => lk.UserName).ToList();
                    break;
                case "date_asc":
                    lichKhamsVL = lichKhamsVL.OrderBy(lk => lk.NgayDat).ToList();
                    break;
                case "date_desc":
                    lichKhamsVL = lichKhamsVL.OrderByDescending(lk => lk.NgayDat).ToList();
                    break;
                case "timeSlot_asc":
                    lichKhamsVL = lichKhamsVL.OrderBy(lk => lk.KhungGio.TimeSlot).ToList();
                    break;
                case "timeSlot_desc":
                    lichKhamsVL = lichKhamsVL.OrderByDescending(lk => lk.KhungGio.TimeSlot).ToList();
                    break;
                    // Có thể thêm các trường sắp xếp khác tại đây
            }
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            ViewBag.CurrentSort = sortOrder;
            return View(lichKhamsVL);
        }

        // POST: LichKham/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                // Nếu không có số điện thoại, trả về view với thông báo lỗi hoặc một danh sách rỗng
                ModelState.AddModelError("phone", "Số điện thoại không được để trống.");
                return View(new List<LichKhamVL>()); // Trả về danh sách rỗng
            }

            // Tìm kiếm lịch khám theo số điện thoại
            var lichKhamList = await _context.lichKhamvls
                .Include(kg=>kg.KhungGio)
                .Include(lk => lk.ClichkhamVL)
                .Where(l => l.phone == phone)
                .ToListAsync();
            ViewBag.SearchPerformed = true;
            return View(lichKhamList); // Trả về danh sách lịch khám cho số điện thoại tương ứng
        }
        // GET: LichKham/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Tìm lịch khám theo Id
            var lichKhamVL = await _context.lichKhamvls.FindAsync(id);
            if (lichKhamVL == null)
            {
                return NotFound();
            }
            // Kiểm tra xem lịch khám đã được lên lịch hay chưa
            var hasScheduled = await _context.Ctlkvl
                                              .AnyAsync(c => c.LichkhamVLId == lichKhamVL.Id);

            if (hasScheduled)
            {
                // Nếu đã lên lịch, thông báo và không cho phép sửa
                ViewBag.Message = "Lịch khám đã được lên lịch, không thể sửa.";
                TempData["PhoneNumber"] = lichKhamVL.phone;
                return View("Search"); // Hoặc bạn có thể trả về một trang khác hoặc giữ lại trang hiện tại
            }
            // Lấy danh sách khung giờ để người dùng chọn
            ViewBag.KhungGios = await _context.KhungGios.ToListAsync();
            TempData["PhoneNumber"] = lichKhamVL.phone;
            return View(lichKhamVL);
        }


        // POST: LichKham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LichKhamVL lichKhamVL)
        {
            if (id != lichKhamVL.Id)
            {
                return NotFound();
            }

            // Tìm bản ghi lịch khám cần chỉnh sửa
            var existingLichKhamVL = await _context.lichKhamvls.FindAsync(id);
            if (existingLichKhamVL == null)
            {
                return NotFound();
            }

            // Giữ nguyên tên và số điện thoại (readonly)
            lichKhamVL.phone = existingLichKhamVL.phone;
            lichKhamVL.UserName = existingLichKhamVL.UserName;

            // Kiểm tra xem ngày đặt có phải là trong tương lai
            if (lichKhamVL.NgayDat.Date < DateTime.Now.Date.AddDays(2))
            {
                ModelState.AddModelError("NgayDat", "Hãy đặt từ 2 ngày sau trở đi.VD:hôm nay 14/11 thì đặt 16/11 trở đi");
                ViewBag.KhungGios = await _context.KhungGios.ToListAsync();
                return View(lichKhamVL);
            }

            // Kiểm tra xem khung giờ đã được đặt hay chưa
            var existingBooking = await _context.lichKhamvls
                .AnyAsync(l => l.KhungGioId == lichKhamVL.KhungGioId && l.NgayDat.Date == lichKhamVL.NgayDat.Date);

            if (existingBooking)
            {
                ModelState.AddModelError("KhungGioId", "Khung giờ đã được đặt cho ngày này.");
                ViewBag.KhungGios = await _context.KhungGios.ToListAsync();
                return View(lichKhamVL);
            }

            // Cập nhật các trường cần thay đổi
            existingLichKhamVL.NgayDat = lichKhamVL.NgayDat;
            existingLichKhamVL.KhungGioId = lichKhamVL.KhungGioId;

            _context.Update(existingLichKhamVL);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Search)); // Redirect về trang danh sách lịch khám
        }
    }
}
