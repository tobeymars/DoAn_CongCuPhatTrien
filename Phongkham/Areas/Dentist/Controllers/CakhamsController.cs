using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Phongkham.Controllers
{
    [Area("Dentist")]
    [Authorize(Roles = SD.Role_Dentist)]
    public class CakhamsController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public CakhamsController(ApplicationDBcontext context)
        {
            _context = context;
        }
        // GET: Cakhams
        public async Task<IActionResult> Index(DateTime? date)
        {
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            var today = DateTime.Today;
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID của nha sĩ đang đăng nhập

            // Lọc các ca khám theo DentistId và bao gồm KhungGio
            var khamsQuery = _context.Cakhams
                .Include(c => c.CakhamKhungGios)
                    .ThenInclude(ckg => ckg.KhungGio) // Bao gồm KhungGio
                .Where(c => c.DentistId == currentUserId); // Lọc theo DentistId

            // Lọc theo ngày nếu có
            if (date.HasValue)
            {
                khamsQuery = khamsQuery.Where(c => c.NgayDang.Date == date.Value.Date); // So sánh theo ngày (không có giờ)
            }

            // Lấy kết quả từ câu truy vấn
            var khams = await khamsQuery.ToListAsync();

            // Trả về giá trị ngày đã chọn cho View
            ViewData["SelectedDate"] = date?.ToString("yyyy-MM-dd");

            return View(khams); // Trả về danh sách ca khám
        }

        // GET: Cakhams/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var cakham = await _context.Cakhams
                .Include(c => c.Dentist) // Bao gồm thông tin bác sĩ
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cakham == null)
            {
                return NotFound();
            }

            // Kiểm tra điều kiện nếu ngày hiện tại đã qua ngày đặt lịch
            if (cakham.NgayDang <= DateTime.Now.Date)
            {
                TempData["ErrorMessage"] = "Không thể xóa ca khám đã đến ngày hẹn.";
                return RedirectToAction(nameof(Index));
            }

            return View(cakham); // Trả về view để xác nhận xóa
        }

        // POST: Cakhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cakham = await _context.Cakhams
                .Include(c => c.Dentist) // Bao gồm thông tin bác sĩ
                .FirstOrDefaultAsync(c => c.Id == id);

            // Kiểm tra ca khám có tồn tại không
            if (cakham == null)
            {
                return NotFound();
            }

            // Kiểm tra lần nữa trước khi xóa để đảm bảo điều kiện ngày hẹn chưa tới
            if (cakham.NgayDang > DateTime.Now.Date)
            {
                // Lấy danh sách các lịch khám liên quan đến bác sĩ này
                var lichKhams = await _context.lichKhams
                    .Where(l => l.Clichkham.Any(ct => ct.TenNhaSi == cakham.Dentist.FullName))
                    .ToListAsync();
                var lichkhamvls = await _context.lichKhamvls
                    .Where(l => l.ClichkhamVL.Any(ct => ct.TenNhaSi == cakham.Dentist.FullName))
                    .ToListAsync();
                // Cập nhật trạng thái cho các lịch khám liên quan
                foreach (var lichKham in lichKhams)
                {
                    lichKham.TrangThai = TrangThaiCaKham.Chưa_Đặt; // Cập nhật trạng thái
                }
                await _context.SaveChangesAsync();
                foreach (var lichkhamvl in lichkhamvls)
                {
                    lichkhamvl.TrangThai = TrangThaiCaKham.Chưa_Đặt; // Cập nhật trạng thái
                }
                await _context.SaveChangesAsync();
                // Lấy tên nha sĩ của ca khám
                var tenNhaSi = cakham.Dentist.FullName;

                // Lấy danh sách các lịch khám liên quan trong bảng ctlichkham
                var lichKhamCts = await _context.cTlichkhams
                    .Where(ct => ct.TenNhaSi == tenNhaSi)
                    .ToListAsync();

                // Lấy danh sách các lịch khám liên quan trong bảng ctlichkhamvl
                var lichKhamVlCts = await _context.Ctlkvl
                    .Where(ct => ct.TenNhaSi == tenNhaSi)
                    .ToListAsync();

                // Xóa các lịch khám liên quan trong bảng ctlichkham
                _context.cTlichkhams.RemoveRange(lichKhamCts);

                // Xóa các lịch khám liên quan trong bảng ctlichkhamvl
                _context.Ctlkvl.RemoveRange(lichKhamVlCts);
                // Xóa ca khám
                _context.Cakhams.Remove(cakham);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã xóa ca khám thành công."; // Thông báo thành công
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa ca khám đã đến ngày hẹn."; // Thông báo lỗi
            }

            return RedirectToAction(nameof(Index)); // Quay lại trang danh sách ca khám
        }



        // GET: Cakhams/Create
        public IActionResult Create()
        {
            ViewData["KhungGios"] = _context.KhungGios.ToList();
            return View();
        }

        // AJAX method to get booked KhungGioIds for a specific date and dentist
        public JsonResult GetBookedKhungGioIds(string selectedDate, string dentistId)
        {
            if (!DateTime.TryParse(selectedDate, out DateTime date))
            {
                return Json(new { bookedKhungGioIds = new List<int>() });
            }

            var bookedKhungGioIds = _context.CakhamKhungGios
                .Where(ck => ck.Cakham.NgayDang.Date == date.Date && ck.Cakham.DentistId == dentistId)
                .Select(ck => ck.KhungGioId)
                .ToList();

            return Json(new { bookedKhungGioIds });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int[] selectedKhungGioIds, DateTime selectedDate)
        {
            // Lấy ID của người dùng đang đăng nhập
            var dentistId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (selectedKhungGioIds == null || selectedKhungGioIds.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một khung giờ.");
            }

            if (selectedDate == DateTime.MinValue)
            {
                ModelState.AddModelError("", "Ngày khám không hợp lệ.");
            }
            else if (selectedDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Ngày khám không thể trong quá khứ.");
            }

            // Kiểm tra ca khám đã có trong Cakhams
            var existingCakham = await _context.Cakhams
                .Include(c => c.CakhamKhungGios)
                .FirstOrDefaultAsync(c => c.NgayDang.Date == selectedDate.Date && c.DentistId == dentistId);

            if (existingCakham != null)
            {
                // Nếu đã có ca khám, kiểm tra khung giờ
                var existingKhungGioIds = existingCakham.CakhamKhungGios.Select(kg => kg.KhungGioId).ToList();

                foreach (var khungGioId in existingKhungGioIds)
                {
                    if (selectedKhungGioIds.Contains(khungGioId))
                    {
                        ModelState.AddModelError("", $"Khung giờ {khungGioId} đã được đặt cho ngày {selectedDate.ToShortDateString()}.");
                    }
                }

                if (ModelState.IsValid)
                {
                    // Bổ sung khung giờ mới vào ca khám hiện tại
                    foreach (var khungGioId in selectedKhungGioIds)
                    {
                        if (!existingKhungGioIds.Contains(khungGioId))
                        {
                            var cakhamKhungGio = new CakhamKhungGio
                            {
                                CakhamId = existingCakham.Id,
                                KhungGioId = khungGioId,
                                TrangThai = TrangThaiCaKham.Chưa_Đặt
                            };

                            _context.CakhamKhungGios.Add(cakhamKhungGio);
                        }
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                // Nếu không có ca khám nào, tạo ca khám mới
                var newCakham = new Cakham
                {
                    DentistId = dentistId, // Đảm bảo rằng bạn đang lưu ID của nha sĩ đang đăng nhập
                    NgayDang = selectedDate,
                    CakhamKhungGios = new List<CakhamKhungGio>()
                };

                foreach (var khungGioId in selectedKhungGioIds)
                {
                    var cakhamKhungGio = new CakhamKhungGio
                    {
                        KhungGioId = khungGioId,
                        TrangThai = TrangThaiCaKham.Chưa_Đặt // Thiết lập giá trị mặc định
                    };

                    newCakham.CakhamKhungGios.Add(cakhamKhungGio);
                }

                _context.Cakhams.Add(newCakham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["KhungGios"] = _context.KhungGios.ToList();
            return View();
        }

        // GET: Cakhams/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cakham = await _context.Cakhams
                .Include(c => c.CakhamKhungGios)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cakham == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["KhungGios"] = await _context.KhungGios.ToListAsync();
            ViewData["DentistId"] = new SelectList(_context.Users, "Id", "FullName", currentUserId);
            ViewData["SelectedDate"] = cakham.NgayDang;  // Hiển thị ngày hiện tại, không cho phép thay đổi
            ViewBag.SelectedKhungGioIds = cakham.CakhamKhungGios.Select(k => k.KhungGioId).ToList();

            return View(cakham);
        }

        // POST: Cakhams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int[] selectedKhungGioIds, string dentistId)
        {
            var cakham = await _context.Cakhams.Include(c => c.CakhamKhungGios).FirstOrDefaultAsync(c => c.Id == id);

            if (cakham == null)
            {
                return NotFound();
            }

            if (selectedKhungGioIds == null || selectedKhungGioIds.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một khung giờ.");
            }

            if (ModelState.IsValid)
            {
                cakham.DentistId = dentistId;

                // Xóa các khung giờ hiện tại
                cakham.CakhamKhungGios.Clear();

                // Thêm lại các khung giờ mới đã chọn
                foreach (var khungGioId in selectedKhungGioIds)
                {
                    cakham.CakhamKhungGios.Add(new CakhamKhungGio { CakhamId = cakham.Id, KhungGioId = khungGioId });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang danh sách
            }

            // Nếu ModelState không hợp lệ, chuẩn bị lại dữ liệu để hiển thị
            ViewData["KhungGios"] = await _context.KhungGios.ToListAsync();
            ViewData["DentistId"] = new SelectList(_context.Users, "Id", "FullName", dentistId);
            ViewData["SelectedKhungGioIds"] = selectedKhungGioIds;

            return View(cakham); // Quay lại trang chỉnh sửa nếu có lỗi
        }
    }
}
