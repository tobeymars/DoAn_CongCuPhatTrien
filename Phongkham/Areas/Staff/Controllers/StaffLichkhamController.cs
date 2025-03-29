using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vonage;
using Vonage.Request;
using Vonage.Messaging;
namespace Phongkham.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = SD.Role_Staff)]
    public class StaffLichkhamController : Controller
    {
        private readonly ApplicationDBcontext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string vonageApiKey = "e01e2454";
        private readonly string vonageApiSecret = "CRBONLi9wLbx7mkD";
        public StaffLichkhamController(ApplicationDBcontext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Lichkham
        public async Task<IActionResult> Index(DateTime? date, string trangThai, string sortOrder, string type = "CT")
        {
            // Lấy dữ liệu từ bảng cTlichkhams
            var lichKhamsQuery = _context.cTlichkhams
                .Include(c => c.LichKham)
                .ThenInclude(c => c.KhungGio)
                .Select(x => new
                {
                    x.Id,
                    FullName = x.TenBenhNhan,
                    x.LichKham.NgayDat,
                    x.TenNhaSi,
                    x.PhongKham,
                    TimeSlot = x.LichKham.KhungGio != null ? x.LichKham.KhungGio.TimeSlot : "N/A",
                    HasIdentity = x.LichKham.Patient != null
                });

            // Lấy dữ liệu từ bảng Ctlkvl
            var lichKhamsVLQuery = _context.Ctlkvl
                .Include(c => c.LichKhamVL)
                .ThenInclude(c => c.KhungGio)
                .Select(x => new
                {
                    x.Id,
                    FullName = x.TenBenhNhan,
                    x.LichKhamVL.NgayDat,
                    x.TenNhaSi,
                    x.PhongKham,
                    TimeSlot = x.LichKhamVL.KhungGio != null ? x.LichKhamVL.KhungGio.TimeSlot : "N/A",
                    HasIdentity = false // Là khách, không có tài khoản
                });

            // Hợp nhất dữ liệu từ hai bảng
            var combinedQuery = lichKhamsQuery.Union(lichKhamsVLQuery);

            // Thực hiện truy vấn và lấy danh sách lịch khám
            var lichKhams = await combinedQuery.ToListAsync();

            // Sắp xếp theo thứ tự
            switch (sortOrder)
            {
                case "patient_asc":
                    lichKhams = lichKhams.OrderBy(lk => lk.FullName).ToList();
                    break;
                case "patient_desc":
                    lichKhams = lichKhams.OrderByDescending(lk => lk.FullName).ToList();
                    break;
                case "dentist_asc":
                    lichKhams = lichKhams.OrderBy(lk => lk.TenNhaSi).ToList();
                    break;
                case "dentist_desc":
                    lichKhams = lichKhams.OrderByDescending(lk => lk.TenNhaSi).ToList();
                    break;
                case "room_asc":
                    lichKhams = lichKhams.OrderBy(lk => lk.PhongKham).ToList();
                    break;
                case "room_desc":
                    lichKhams = lichKhams.OrderByDescending(lk => lk.PhongKham).ToList();
                    break;
                case "date_asc":
                    lichKhams = lichKhams.OrderBy(lk => lk.NgayDat).ToList();
                    break;
                case "date_desc":
                    lichKhams = lichKhams.OrderByDescending(lk => lk.NgayDat).ToList();
                    break;
                case "timeSlot_asc":
                    lichKhams = lichKhams.OrderBy(lk => lk.TimeSlot).ToList();
                    break;
                case "timeSlot_desc":
                    lichKhams = lichKhams.OrderByDescending(lk => lk.TimeSlot).ToList();
                    break;
            }

            ViewBag.CurrentSort = sortOrder;
            return View(lichKhams);
        }

        // GET: Admin/Lichkham/Create
        public IActionResult Create()
        {
            // Khởi tạo đối tượng cho ViewModel
            object model = new CTlichkham(); // Đặt mặc định là CTlichkham hoặc một ViewModel chung nếu cần

            // Lấy dữ liệu từ bảng lichKhams
            var lichKhamList = _context.lichKhams
                .Select(l => new { l.UserName, l.phone })
                .Distinct()
                .ToList();

            // Lấy dữ liệu từ bảng LichKhamVL
            var lichKhamVLList = _context.lichKhamvls
                .Select(l => new { l.UserName, l.phone })
                .Distinct()
                .ToList();

            // Kết hợp dữ liệu từ hai bảng và loại bỏ các phần tử trùng lặp
            var combinedBenhNhanList = lichKhamList
                .Concat(lichKhamVLList)
                .Distinct()
                .ToList();

            ViewBag.BenhNhanList = combinedBenhNhanList;

            ViewBag.PhongKhamList = new List<string>
    {
        "Tầng 1 - Phòng 01",
        "Tầng 1 - Phòng 02",
        "Tầng 1 - Phòng 03",
        "Tầng 1 - Phòng 04",
        "Tầng 2 - Phòng 01",
        "Tầng 2 - Phòng 02",
        "Tầng 2 - Phòng 03",
        "Tầng 2 - Phòng 04",
    };

            return View(model);
        }
        [HttpGet]
        public IActionResult SearchBenhNhanByPhone(string phone)
        {
            // Kiểm tra nếu phone rỗng
            if (string.IsNullOrEmpty(phone))
            {
                return Json(new List<object>());
            }

            // Lấy danh sách bệnh nhân từ bảng lichKhams
            var lichKhamList = _context.lichKhams
                .Where(l => l.phone.Contains(phone))
                .Select(l => new { id = l.phone, userName = l.UserName, phone = l.phone })
                .ToList();

            // Lấy danh sách bệnh nhân từ bảng lichKhamVLs
            var lichKhamVLList = _context.lichKhamvls
                .Where(l => l.phone.Contains(phone))
                .Select(l => new { id = l.phone, userName = l.UserName, phone = l.phone })
                .ToList();

            // Kết hợp dữ liệu từ hai bảng và loại bỏ các phần tử trùng lặp
            var combinedBenhNhanList = lichKhamList
                .Concat(lichKhamVLList)
                .GroupBy(x => new { x.userName, x.phone })
                .Select(g => g.First())
                .ToList();

            return Json(combinedBenhNhanList);
        }

        [HttpGet]
        public JsonResult GetKhungGioByDate(string username, DateTime ngayDat)
        {
            // Lấy dữ liệu từ cả hai bảng lichKhams và lichKhamvls
            var khungGioList = _context.lichKhams
                .Where(l => l.UserName == username && l.NgayDat.Date == ngayDat.Date && l.TrangThai != TrangThaiCaKham.Đã_Đặt)
                .Select(l => new { l.KhungGioId, l.KhungGio })
                .Union(
                    _context.lichKhamvls
                    .Where(l => l.UserName == username && l.NgayDat.Date == ngayDat.Date && l.TrangThai != TrangThaiCaKham.Đã_Đặt)
                    .Select(l => new { l.KhungGioId, l.KhungGio })
                )
                .Distinct()
                .ToList();

            if (!khungGioList.Any())
            {
                return Json(new { message = "No time slots available" });
            }

            return Json(khungGioList);
        }

        [HttpGet]
        public JsonResult GetNhaSi(int khungGioId, DateTime ngayDat)
        {
            var nhaSiList = _context.Cakhams
                .Where(c => c.CakhamKhungGios.Any(k => k.KhungGioId == khungGioId && k.TrangThai != TrangThaiCaKham.Đã_Đặt) && c.NgayDang.Date == ngayDat.Date)
                .Select(c => new { c.DentistId, c.Dentist.FullName })
                .Distinct()
                .ToList();
            return Json(nhaSiList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection formCollection)
        {
            string tenBenhNhan = formCollection["TenBenhNhan"];
            string tenNhaSi = formCollection["TenNhaSi"];
            string phongKham = formCollection["PhongKham"];
            DateTime ngayDat = DateTime.Parse(formCollection["NgayDat"]);
            int khungGioId = int.Parse(formCollection["KhungGioId"]);

            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên bệnh nhân nằm trong bảng nào
                var lichKham = await _context.lichKhams
                    .FirstOrDefaultAsync(l => l.UserName == tenBenhNhan && l.NgayDat.Date == ngayDat.Date && l.KhungGioId == khungGioId);

                var lichKhamVL = await _context.lichKhamvls
                    .FirstOrDefaultAsync(l => l.UserName == tenBenhNhan && l.NgayDat.Date == ngayDat.Date && l.KhungGioId == khungGioId);

                if (lichKham == null && lichKhamVL == null)
                {
                    ModelState.AddModelError("", "Khung giờ không tồn tại.");
                    return View(new CTlichkham()); // Truyền vào model mặc định để tránh lỗi
                }
                string soDienThoai = null;
                if (lichKham != null)
                {
                    // Lấy số điện thoại từ bảng lichKham
                    soDienThoai = lichKham.phone;
                    soDienThoai = "+84" + soDienThoai.Substring(1);
                }
                else if (lichKhamVL != null)
                {
                    // Lấy số điện thoại từ bảng lichKhamVL
                    soDienThoai = lichKhamVL.phone;
                    soDienThoai = "+84" + soDienThoai.Substring(1);
                }

                // Tạo đối tượng CTlichkham hoặc CtlichkhamVL tùy thuộc vào bảng chứa tên bệnh nhân
                if (lichKham != null)
                {
                    var ctlichkham = new CTlichkham
                    {
                        TenBenhNhan = tenBenhNhan,
                        TenNhaSi = tenNhaSi,
                        PhongKham = phongKham,
                        LichkhamId = lichKham.Id
                    };
                    _context.Add(ctlichkham);
                    var message = $"Xin chao, ban co lich kham ngày {ngayDat.ToString("dd/MM/yyyy")} luc {khungGioId}.";
                    await SendSmsNotification(soDienThoai, message);
                }
                else if (lichKhamVL != null)
                {
                    var ctlichkhamVL = new CtlichkhamVL
                    {
                        TenBenhNhan = tenBenhNhan,
                        TenNhaSi = tenNhaSi,
                        PhongKham = phongKham,
                        LichkhamVLId = lichKhamVL.Id
                    };
                    _context.Add(ctlichkhamVL);
                    var message = $"Xin chao, ban co lich kham ngày {ngayDat.ToString("dd/MM/yyyy")} luc {khungGioId}.";
                    await SendSmsNotification(soDienThoai, message);
                }

                // Cập nhật trạng thái của ca khung giờ
                var caKhamKhungGio = await _context.CakhamKhungGios
                    .FirstOrDefaultAsync(ckg => ckg.Cakham.Dentist.FullName == tenNhaSi && ckg.Cakham.NgayDang == ngayDat && ckg.KhungGioId == khungGioId);

                if (caKhamKhungGio != null)
                {
                    caKhamKhungGio.TrangThai = TrangThaiCaKham.Đã_Đặt;
                    _context.Update(caKhamKhungGio);
                }

                // Cập nhật trạng thái của lichKham hoặc lichKhamVL
                if (lichKham != null)
                {
                    lichKham.TrangThai = TrangThaiCaKham.Đã_Đặt;
                }
                else if (lichKhamVL != null)
                {
                    lichKhamVL.TrangThai = TrangThaiCaKham.Đã_Đặt;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, trả về view với model mặc định
            return View(new CTlichkham());
        }
        private async Task SendSmsNotification(string to, string message)
        {
            Console.WriteLine("Số điện thoại gửi: " + to);
            var credentials = Credentials.FromApiKeyAndSecret(vonageApiKey, vonageApiSecret);
            var vonageClient = new VonageClient(credentials);

            var request = new SendSmsRequest
            {
                To = to,
                From = "VonageAPI", // Thay bằng số điện thoại gửi của bạn hoặc tên bạn muốn hiển thị
                Text = message
            };

            var response = await vonageClient.SmsClient.SendAnSmsAsync(request);

            if (response.Messages[0].Status == "0")
            {
                Console.WriteLine("Tin nhắn đã được gửi thành công.");
            }
            else
            {
                Console.WriteLine($"Lỗi: {response.Messages[0].ErrorText}");
            }
        }
    }
}
