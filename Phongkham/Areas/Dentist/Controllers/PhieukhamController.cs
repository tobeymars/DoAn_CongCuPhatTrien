using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Phongkham.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PdfSharp.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Newtonsoft.Json;
using Hangfire;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using Elfie.Serialization;

namespace Phongkham.Controllers
{
    [Area("Dentist")]
    [Authorize(Roles = SD.Role_Dentist)]
    public class PhieukhamController : Controller
    {
        private readonly ApplicationDBcontext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IEmailSender _emailSender;
        public PhieukhamController(ApplicationDBcontext context, UserManager<ApplicationUser> userManager, ICompositeViewEngine viewEngine, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _viewEngine = viewEngine; // Gán _viewEngine
            _emailSender = emailSender;
        }
        // Tạo phiếu khám (GET)
        public IActionResult Create(string tenBenhNhan, string tenNhaSi, string khunggio, string source, int lichKhamId)
        {
            // Truyền các thông tin cần thiết vào view
            ViewData["TenBenhNhan"] = tenBenhNhan;
            ViewData["TenNhaSi"] = tenNhaSi;
            ViewData["KhungGio"] = khunggio;
            ViewData["Source"] = source;  // Thêm vào để truyền thông tin source
            ViewData["LichKhamId"] = lichKhamId;  // Thêm vào để truyền thông tin lichKhamId
            // Lấy danh sách dịch vụ từ cơ sở dữ liệu
            ViewData["DichVuList"] = _context.dichvus.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonThuoc donThuoc, Phieukham phieukham, List<int> DichvuIds, string ThuocIds, string LieuLuongs, bool TaiKham, string Thoigiantaikham, string source, int? lichKhamId) // Nhận danh sách dịch vụ đã chọn
        {
            // Lấy thông tin người dùng hiện tại
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                // Gán thêm thông tin nha sĩ và các thông tin cần thiết
                phieukham.TenNhaSi = currentUser.FullName;
                phieukham.NgayKham = DateTime.Now;
                donThuoc.TimeSlot = Request.Form["KhungGio"];
                phieukham.TimeSlot = Request.Form["KhungGio"];

                // Gán giá trị mặc định cho cột TrangThaiTaiKham (false)
                phieukham.TrangThaiTaiKham = false;  // Giá trị mặc định là không tái khám
                                                     // Lưu phiếu khám vào cơ sở dữ liệu
                if (TaiKham)
                {
                    phieukham.Thoigiantaikham = Thoigiantaikham; // Cập nhật thời gian tái khám, 1 tháng sau ngày khám
                }
                else
                {
                    phieukham.Thoigiantaikham = "Không có"; // Nếu không tái khám thì lưu "Không có"
                }
                if (source == "CTlichkham")
                {
                    phieukham.CTlichkhamId = lichKhamId;
                    donThuoc.CTlichkhamId = lichKhamId;
                }
                else if (source == "CtlichkhamVL")
                {
                    phieukham.CtlichkhamVLId = lichKhamId;
                    donThuoc.CtlichkhamVLId = lichKhamId;
                }
                _context.Add(phieukham);
                _context.Add(donThuoc);
                await _context.SaveChangesAsync();  // Lưu phiếu khám vào cơ sở dữ liệu để có ID

                // Lưu các dịch vụ đã chọn vào bảng liên kết PhieuKhamDichvu
                foreach (var dichvuId in DichvuIds)
                {
                    var phieuKhamDichvu = new PhieuKhamDichvu
                    {
                       
                        PhieuKhamId = phieukham.Id,
                        DichvuId = dichvuId
                    };
                    _context.PhieuKhamDichvus.Add(phieuKhamDichvu);
                }
                List<int> thuocIds = JsonConvert.DeserializeObject<List<int>>(ThuocIds);
                List<int> lieuLuongs = JsonConvert.DeserializeObject<List<int>>(LieuLuongs);
                // Lưu chi tiết đơn thuốc
                for (int i = 0; i < thuocIds.Count; i++)
                {
                    var chiTietDonThuoc = new ChiTietDonThuoc
                    {
                        DonThuocId = donThuoc.Id,  // Giả sử bạn đã có ID của đơn thuốc
                        ThuocId = thuocIds[i],  // ID của thuốc
                        LieuLuong = lieuLuongs[i].ToString()  // Liều lượng tương ứng
                    };

                    _context.ChiTietDonThuocs.Add(chiTietDonThuoc);  // Thêm vào cơ sở dữ liệu
                }

                await _context.SaveChangesAsync(); // Lưu bảng liên kết

                if (TaiKham)
                {
                    phieukham.TaiKham = true;
                    await _context.SaveChangesAsync();

                    var benhNhan = await _context.Users
                         .FirstOrDefaultAsync(u => u.FullName == phieukham.TenBenhNhan);

                    if (benhNhan != null)
                    {
                        var email = benhNhan.Email;
                        var subject = "Thông báo tái khám";
                        var htmlMessage = $@"
                        <p style='color: black; margin-bottom: 10px;'>Chào {benhNhan.FullName},</p>
                        <p style='color: black; margin-bottom: 10px;'>Chúng tôi rất vui mừng thông báo rằng bạn đã hoàn thành lần khám trước tại phòng khám của chúng tôi. Cảm ơn bạn đã tin tưởng và lựa chọn dịch vụ chăm sóc sức khỏe của chúng tôi. Chúng tôi luôn cam kết mang đến những dịch vụ tốt nhất để bảo vệ sức khỏe của bạn.</p>
                        <p style='color: black; margin-bottom: 10px;'>Để tiếp tục quá trình điều trị và đảm bảo sức khỏe của bạn, chúng tôi xin thông báo bạn sẽ cần tái khám tại phòng khám của chúng tôi.</p>
                        <p style='color: black; margin-bottom: 10px;'><strong>* Bạn có lịch tái khám vào {phieukham.Thoigiantaikham} sau. Xin vui lòng liên hệ với chúng tôi để phòng khám có thể sắp xếp nha sĩ và thời gian thích hợp cho bạn.</strong></p>
                        <p style='color: black; margin-bottom: 10px;'>Sự hài lòng và sức khỏe của bạn luôn là ưu tiên hàng đầu của chúng tôi. Nếu bạn có bất kỳ câu hỏi hay yêu cầu nào, xin đừng ngần ngại liên hệ với chúng tôi.</p>
                        <p style='color: black; margin-bottom: 10px;'>
                        Trân trọng,<br/>
                        Đội ngũ hỗ trợ Mediplus<br/>
                        Email: <a href='mailto:noheart135246@gmail.com' style='color: blue;'>noheart135246@gmail.com</a><br/>
                        Hotline: <strong>0123456789</strong><br/>
                        Địa chỉ: <strong>123 Đường ABC, Quận 1, TP.HCM</strong><br/>
                        </p>";
                        // Gửi email liền
                        //await _emailSender.SendEmailAsync(email, subject, htmlMessage);

                        // Lên lịch gửi email sau 1 tuần
                        // Tính toán thời gian gửi email vào 8 giờ sáng của ngày tiếp theo

                        // Lên lịch gửi thông báo tái khám sau 7 ngày vào 8 giờ sáng của ngày gửi sau 7 ngày
                        var sendTime = phieukham.NgayKham.Date.AddDays(7).AddHours(6);
                        BackgroundJob.Schedule(() => _emailSender.SendEmailAsync(email, subject, htmlMessage), sendTime);
                        // Email hỏi thăm sức khỏe
                        var followUpSubject = "Hỏi thăm sức khỏe sau khi khám";
                        var followUpMessage = $@"
                        <p style='color: black; margin-bottom: 10px;'>Chào {benhNhan.FullName},</p>
                        <p style='color: black; margin-bottom: 10px;'>Chúng tôi hy vọng bạn đang cảm thấy tốt hơn sau khi hoàn tất lần khám trước tại phòng khám của chúng tôi. Chúng tôi luôn quan tâm đến sự hồi phục của bạn và rất mong được biết tình trạng sức khỏe hiện tại của bạn.</p>
                        <p style='color: black; margin-bottom: 10px;'>Nếu có bất kỳ vấn đề gì, xin đừng ngần ngại liên hệ với chúng tôi. Chúng tôi sẵn sàng hỗ trợ bạn mọi lúc.</p>
                        <p style='color: black; margin-bottom: 10px;'>Chúng tôi rất trân trọng sự tin tưởng của bạn và sẽ luôn đồng hành cùng bạn trong suốt quá trình chăm sóc sức khỏe.</p>
                        <p style='color: black; margin-bottom: 10px;'>
                        Trân trọng,<br/>
                        Đội ngũ hỗ trợ Mediplus<br/>
                        Email: <a href='mailto:noheart135246@gmail.com' style='color: blue;'>noheart135246@gmail.com</a><br/>
                        Hotline: <strong>0123456789</strong><br/>
                        Địa chỉ: <strong>123 Đường ABC, Quận 1, TP.HCM</strong><br/>
                        </p>";
                        // Lên lịch gửi email hỏi thăm sức khỏe sau 1 ngày vào 6 giờ sáng của ngày hôm sau
                        var followUpSendTime = phieukham.NgayKham.Date.AddDays(1).AddHours(6);
                        BackgroundJob.Schedule(() => _emailSender.SendEmailAsync(email, followUpSubject, followUpMessage), followUpSendTime);
                    }
                }                
                return RedirectToAction("ListPhieuKham", "Phieukham"); // Hoặc chuyển đến bất kỳ trang nào bạn muốn sau khi tạo thành công
            }

            // Nếu có lỗi, truyền lại thông tin vào view
            ViewData["DichVuList"] = _context.dichvus.ToList();
            return View(phieukham);
        }
        [HttpGet]
        public IActionResult SearchThuoc(string term)
        {
            // Lấy danh sách thuốc dựa trên term
            var thuocList = _context.Thuocs
                .Where(t => t.TenThuoc.Contains(term) && !t.IsDeleted)
                .Select(t => new { id = t.Id, tenThuoc = t.TenThuoc, soLuong = t.SoLuong })
                .ToList();

            return Json(thuocList);
        }

        // Chi tiết phiếu khám (GET)
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            // Lấy thông tin phiếu khám
            var phieuKham = await _context.phieukhams
                                           .Include(p => p.PhieuKhamDichvus)  // Bao gồm dịch vụ liên quan
                                           .ThenInclude(pkd => pkd.Dichvu)   // Bao gồm thông tin dịch vụ
                                           .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuKham == null)
            {
                return NotFound();
            }

            // Tìm đơn thuốc dựa trên TenBenhNhan, NgayKham, và TimeSlot
            var donThuoc = await _context.DonThuocs
                                            .Include(dt => dt.ChiTietDonThuocs)
                                            .ThenInclude(ctdt => ctdt.Thuoc)
                                         .FirstOrDefaultAsync(d => d.TenBenhNhan == phieuKham.TenBenhNhan &&
                                                                   d.NgayLap.Date == phieuKham.NgayKham.Date &&
                                                                   d.TimeSlot == phieuKham.TimeSlot &&
                                                                   !d.IsDeleted);  // Kiểm tra nếu đơn thuốc chưa bị xóa
            string donThuocInfo = string.Empty;
            if (donThuoc != null)
            {
                foreach (var ctdt in donThuoc.ChiTietDonThuocs)
                {
                    donThuocInfo += $"{ctdt.Thuoc.TenThuoc} - Liều lượng: {ctdt.LieuLuong}\n";
                }
            }
            // Truyền cả thông tin phiếu khám và đơn thuốc vào View
            ViewBag.PhieuKham = phieuKham;
            ViewBag.DonThuocInfo = donThuocInfo;

            return View();
        }

        // Hiển thị danh sách các phiếu khám (GET)
        public async Task<IActionResult> ListPhieuKham(DateTime? ngayKham, string timeSlot)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("Người dùng không tồn tại hoặc không đăng nhập.");
            }

            // Lấy danh sách phiếu khám của nha sĩ hiện tại và các dịch vụ liên quan, lọc theo ngày và giờ
            var phieuKhamsQuery = _context.phieukhams
                .Include(p => p.PhieuKhamDichvus) // Bao gồm các dịch vụ qua bảng trung gian PhieuKhamDichvu
                .ThenInclude(pkd => pkd.Dichvu)  // Bao gồm các thông tin dịch vụ từ bảng Dichvu
                .Where(p => p.TenNhaSi == currentUser.FullName); // Lọc theo tên nha sĩ

            // Nếu có ngày khám (ngayKham) và giờ khám (timeSlot), lọc theo các điều kiện này
            if (ngayKham.HasValue)
            {
                phieuKhamsQuery = phieuKhamsQuery.Where(p => p.NgayKham.Date == ngayKham.Value.Date);
            }

            if (!string.IsNullOrEmpty(timeSlot))
            {
                phieuKhamsQuery = phieuKhamsQuery.Where(p => p.TimeSlot == timeSlot);
            }

            var phieuKhams = await phieuKhamsQuery.ToListAsync();

            // Lấy danh sách đơn thuốc của nha sĩ hiện tại và các thuốc liên quan, lọc theo ngày và giờ nếu cần
            var donThuocsQuery = _context.DonThuocs
                .Include(dt => dt.ChiTietDonThuocs)
                .ThenInclude(ctdt => ctdt.Thuoc)
                .Where(dt => dt.TenNhaSi == currentUser.FullName);

            // Nếu có ngày khám (ngayKham) và giờ khám (timeSlot), lọc theo các điều kiện này
            if (ngayKham.HasValue)
            {
                donThuocsQuery = donThuocsQuery.Where(dt => dt.NgayLap.Date == ngayKham.Value.Date);
            }

            if (!string.IsNullOrEmpty(timeSlot))
            {
                donThuocsQuery = donThuocsQuery.Where(dt => dt.TimeSlot == timeSlot);
            }

            var donThuocs = await donThuocsQuery.ToListAsync();

            // Trả về danh sách phiếu khám và đơn thuốc trong một dynamic object
            return View(new { PhieuKhams = phieuKhams, DonThuocs = donThuocs });
        }
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            // Lấy phiếu khám từ cơ sở dữ liệu
            var phieuKham = await _context.phieukhams
                                            .Include(p => p.PhieuKhamDichvus)
                                            .ThenInclude(pkd => pkd.Dichvu)
                                            .FirstOrDefaultAsync(p => p.Id == id);

            if (phieuKham == null)
            {
                return NotFound();
            }

            // Truyền danh sách dịch vụ vào ViewData
            ViewData["DichVuList"] = await _context.dichvus.ToListAsync();

            // Lấy danh sách các dịch vụ đã chọn
            var selectedDichVuIds = phieuKham.PhieuKhamDichvus.Select(pkd => pkd.DichvuId).ToList();
            ViewData["SelectedDichVuIds"] = selectedDichVuIds;
            
            // Kiểm tra nếu phiếu khám có liên kết với đơn thuốc
            var donThuoc = await _context.DonThuocs
                                 .Include(dt => dt.ChiTietDonThuocs)
                                 .ThenInclude(ct => ct.Thuoc)
                                 .FirstOrDefaultAsync(dt => dt.TenBenhNhan == phieuKham.TenBenhNhan &&
                                                             dt.TimeSlot == phieuKham.TimeSlot &&
                                                             dt.CTlichkhamId == phieuKham.CTlichkhamId &&
                                                             dt.CtlichkhamVLId == phieuKham.CtlichkhamVLId);
            ViewData["ThuocList"] = donThuoc;
            // Truyền danh sách thuốc và liều lượng vào ViewData dưới dạng danh sách
            if (donThuoc != null)
            {
                var thuocList = donThuoc.ChiTietDonThuocs.Select(ct => new
                {
                    ThuocId = ct.ThuocId,
                    LieuLuong = ct.LieuLuong
                }).ToList();

                ViewData["ThuocList2"] = thuocList;

                var thuocIds = thuocList.Select(t => t.ThuocId).ToList(); // Đảm bảo là List<int>
                var lieuLuongs = thuocList.Select(t => t.LieuLuong).ToList(); // Đảm bảo là List<string>

                ViewData["ThuocIds"] = thuocIds;
                ViewData["LieuLuongs"] = lieuLuongs;
            }

            return View(phieuKham);
        }
        // Xử lý sửa phiếu khám (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Phieukham phieukham, List<int> SelectedDichVuIds, string ThuocIds, string LieuLuongs)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                phieukham.TenNhaSi = currentUser.FullName; // Gán giá trị TenNhaSi từ người dùng hiện tại
            }

            phieukham.TimeSlot = Request.Form["KhungGio"]; // Lấy khung giờ từ form

            // Lấy phiếu khám từ CSDL
            var existingPhieuKham = await _context.phieukhams
                                                  .Include(p => p.PhieuKhamDichvus)  // Bao gồm dịch vụ của phiếu khám
                                                  .FirstOrDefaultAsync(p => p.Id == phieukham.Id);

            if (existingPhieuKham == null)
            {
                return NotFound();
            }

            // Cập nhật dịch vụ
            if (SelectedDichVuIds != null)
            {
                // Xóa các dịch vụ cũ trước khi thêm lại dịch vụ mới
                _context.PhieuKhamDichvus.RemoveRange(existingPhieuKham.PhieuKhamDichvus);

                foreach (var dichvuId in SelectedDichVuIds)
                {
                    existingPhieuKham.PhieuKhamDichvus.Add(new PhieuKhamDichvu
                    {
                        PhieuKhamId = phieukham.Id,
                        DichvuId = dichvuId
                    });
                }
            }

            if (phieukham.TaiKham != existingPhieuKham.TaiKham)
            {
                existingPhieuKham.TaiKham = phieukham.TaiKham;

                // Kiểm tra nếu có tái khám (TaiKham == true), thì cập nhật thời gian tái khám
                if (phieukham.TaiKham)
                {
                    existingPhieuKham.Thoigiantaikham = phieukham.Thoigiantaikham; // Cập nhật thời gian tái khám
                }
                else
                {
                    existingPhieuKham.Thoigiantaikham = "Không có"; // Nếu không tái khám thì lưu "Không có"
                }
            }

            // Cập nhật ghi chú
            existingPhieuKham.GhiChu = phieukham.GhiChu;

            // Cập nhật trạng thái tái khám nếu không có giá trị từ form
            if (phieukham.TrangThaiTaiKham == null)
            {
                phieukham.TrangThaiTaiKham = false; // Gán mặc định là không tái khám nếu không có sự thay đổi
            }
            existingPhieuKham.TrangThaiTaiKham = phieukham.TrangThaiTaiKham; // Gán lại trạng thái tái khám

            // Xử lý đơn thuốc
            var donThuoc = await _context.DonThuocs
                             .Include(dt => dt.ChiTietDonThuocs) // Đảm bảo bao gồm ChiTietDonThuocs
                             .FirstOrDefaultAsync(dt => dt.TenBenhNhan == phieukham.TenBenhNhan && dt.TimeSlot == phieukham.TimeSlot);

if (donThuoc == null)
{
    // Nếu không tìm thấy đơn thuốc, tạo mới
    donThuoc = new DonThuoc
    {
        TenBenhNhan = phieukham.TenBenhNhan,
        TenNhaSi = phieukham.TenNhaSi,
        TimeSlot = phieukham.TimeSlot,
        GhiChu = phieukham.GhiChu
    };
    _context.DonThuocs.Add(donThuoc);
    await _context.SaveChangesAsync(); // Lưu đơn thuốc để có ID
}
else
{
    // Xóa các chi tiết đơn thuốc cũ nếu có
    if (donThuoc.ChiTietDonThuocs != null && donThuoc.ChiTietDonThuocs.Count > 0)
    {
        _context.ChiTietDonThuocs.RemoveRange(donThuoc.ChiTietDonThuocs);
        await _context.SaveChangesAsync(); // Lưu các thay đổi để xóa chi tiết cũ
    }
}

// Nếu không có thuốc hoặc liều lượng, không làm gì cả
if (string.IsNullOrEmpty(ThuocIds) || string.IsNullOrEmpty(LieuLuongs))
{
    await _context.SaveChangesAsync(); // Lưu các thay đổi trước khi quay lại
    return RedirectToAction(nameof(ListPhieuKham)); // Quay lại trang danh sách phiếu khám
}

// Deserialize danh sách thuốc và liều lượng
List<int> thuocIds = JsonConvert.DeserializeObject<List<int>>(ThuocIds);
List<string> lieuLuongs = JsonConvert.DeserializeObject<List<string>>(LieuLuongs);

// Lưu các chi tiết đơn thuốc mới
for (int i = 0; i < thuocIds.Count; i++)
{
    var chiTietDonThuoc = new ChiTietDonThuoc
    {
        DonThuocId = donThuoc.Id,
        ThuocId = thuocIds[i],
        LieuLuong = lieuLuongs[i]  // Liều lượng tương ứng
    };
    _context.ChiTietDonThuocs.Add(chiTietDonThuoc);
}

// Lưu tất cả các thay đổi vào cơ sở dữ liệu
await _context.SaveChangesAsync();

            // Cập nhật phiếu khám và lưu lại
            _context.Update(existingPhieuKham);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ListPhieuKham)); // Chuyển hướng sau khi sửa thành công
        }

        public async Task<IActionResult> ExportPdf(int id)
        {
            var phieuKham = await _context.phieukhams
                                           .Include(p => p.PhieuKhamDichvus)
                                           .ThenInclude(pkd => pkd.Dichvu)
                                           .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuKham == null)
            {
                return NotFound();
            }

            var donThuoc = await _context.DonThuocs
                                         .Include(dt => dt.ChiTietDonThuocs)
                                         .ThenInclude(ctdt => ctdt.Thuoc)
                                         .FirstOrDefaultAsync(d => d.TenBenhNhan == phieuKham.TenBenhNhan &&
                                                                   d.NgayLap.Date == phieuKham.NgayKham.Date &&
                                                                   d.TimeSlot == phieuKham.TimeSlot &&
                                                                   !d.IsDeleted);

            var dichvuList = phieuKham.PhieuKhamDichvus.Select(pkd => pkd.Dichvu).ToList();
            var thuocList = donThuoc?.ChiTietDonThuocs
      .Select(ct => new { Thuoc = ct.Thuoc, LieuLuong = ct.LieuLuong })
      .Cast<dynamic>() // Chuyển kiểu về dynamic
      .ToList() ?? new List<dynamic>();


            // Tính tổng tiền
            decimal tongTienKhám = 100000; // Tiền khám mặc định
            decimal tongTienDichVu = dichvuList.Sum(d => d.Price);
            decimal tongTienThuoc = thuocList.Sum(t =>
            {
                decimal lieuLuongDecimal = 0m;
                if (!string.IsNullOrEmpty(t.LieuLuong) && decimal.TryParse(t.LieuLuong, out lieuLuongDecimal))
                {
                    return t.Thuoc.Price * lieuLuongDecimal;  // Tính tổng tiền thuốc
                }
                return 0m;  // Trả về 0 nếu LieuLuong không hợp lệ
            });

            decimal tongTien = tongTienKhám + tongTienDichVu + tongTienThuoc;

            // Định dạng VNĐ
            var cultureInfo = new CultureInfo("vi-VN");  // Văn hóa VN

            // Tạo nội dung HTML
            string htmlContent = $@"
<html>
<head>
    <style>
        body {{
            font-family: 'Arial', sans-serif;
            color: #333;
            margin: 0;
            padding: 0;
        }}
        .header {{
            text-align: center;
            padding: 20px;
            border-bottom: 2px solid #1e88e5;
        }}
        .header img {{
            height: 70px;
        }}
        .header h1 {{
            margin: 10px 0;
            font-size: 24px;
            color: #1e88e5;
        }}
        .header p {{
            font-size: 14px;
            color: #555;
        }}
        .container {{
            padding: 20px 40px;
            margin: 0 auto;
            max-width: 700px;
        }}
        .section {{
            margin-bottom: 10px;
        }}
        .section-title {{
            font-weight: bold;
            font-size: 16px;
            color: #555;
            margin-bottom: 5px;
        }}
        .section-content {{
            font-size: 14px;
            color: #333;
            line-height: 1.5;
        }}
        ul {{
            padding-left: 20px;
        }}
        li {{
            font-size: 14px;
            margin-bottom: 10px;
        }}

        .price {{
             text-align: right; /* Đảm bảo giá nằm ở bên phải */
        }}
        .signature {{
            margin-top: 30px;
            text-align: right;
            font-size: 14px;
            font-style: italic;
        }}
        .signature p {{
            margin-bottom: 50px;
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #aaa;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <img src='https://png.pngtree.com/png-clipart/20220109/original/pngtree-white-teeth-doll-png-image_7035897.png' alt='Clinic Logo'>
        <h1>Phòng Khám MediaPlus</h1>
        <p>Địa chỉ: 123 Đường ABC, Quận 1, TP. HCM | Hotline: 0987 654 321</p>
    </div>
    <div class='container'>
        <h2 style='text-align:center;'>PHIẾU KHÁM BỆNH</h2>
        <div class='section'>
            <p class='section-title'>Ngày khám:</p>
            <p class='section-content'>{phieuKham.NgayKham:dd/MM/yyyy}</p>
        </div>
        <div class='section'>
            <p class='section-title'>Tên bệnh nhân:</p>
            <p class='section-content'>{phieuKham.TenBenhNhan}</p>
        </div>
        <div class='section'>
            <p class='section-title'>Chuẩn đoán:</p>
            <p class='section-content'>{phieuKham.GhiChu}</p>
        </div>
        <div class='section'>
            <p class='section-title'>Dịch vụ đã chọn:</p>
            <ul class='section-content'>";

            foreach (var dichvu in dichvuList)
            {
                htmlContent += $"<li>{dichvu.ten}<div class='price'> Giá: {dichvu.Price.ToString("C0", cultureInfo)}</div></li>";
            }

            htmlContent += @"
            </ul>
        </div>
        <div class='section'>
            <p class='section-title'>Thuốc đã kê:</p>
            <ul class='section-content'>";

            foreach (var thuoc in thuocList)
            {
                decimal thuocPrice = thuoc.Thuoc.Price * decimal.Parse(thuoc.LieuLuong);
                htmlContent += $"<li>{thuoc.Thuoc.TenThuoc} - Liều lượng: {thuoc.LieuLuong} <div class='price'> Giá: {thuocPrice.ToString("C0", cultureInfo)}</div></li>";
            }

            htmlContent += $@"
            </ul>
        </div>
        <div class='section'>
            <p class='section-title'>Tổng tiền:<span class='price'>{tongTien.ToString("C0", cultureInfo)}</span></p>
        </div>
    </div>
    <div class='signature'>
        <p>Ký tên,</p>
        <p>__________________________</p>
    </div>
    <div class='footer'>
        © 2024 Phòng Khám XYZ - Tất cả quyền được bảo lưu
    </div>
</body>
</html>";

            PdfDocument pdf = PdfGenerator.GeneratePdf(htmlContent, PdfSharp.PageSize.A4);

            using (MemoryStream stream = new MemoryStream())
            {
                pdf.Save(stream, false);
                return File(stream.ToArray(), "application/pdf", "PhieuKham_" + phieuKham.TenBenhNhan + ".pdf");
            }
        }

        // Helper method để render View thành HTML string
        private async Task<string> RenderViewAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} không được tìm thấy.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                // Đảm bảo không sử dụng layout
                viewContext.ViewData["Layout"] = null;

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
