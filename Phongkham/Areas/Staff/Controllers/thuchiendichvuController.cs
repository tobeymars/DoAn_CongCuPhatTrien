using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using System.Threading.Tasks;

namespace Phongkham.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = SD.Role_Staff)]
    public class ThuchiendichvuController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public ThuchiendichvuController(ApplicationDBcontext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var phieuKhamDichvusGrouped = await _context.PhieuKhamDichvus
                .Include(p => p.PhieuKham)
                .Include(p => p.Dichvu)
                .GroupBy(p => p.PhieuKhamId)
                .ToListAsync();

            var dungdvs = await _context.Dungdvs
                .Include(d => d.Dentist)  // Bao gồm đối tượng Dentist
                .ToListAsync();

            // Kiểm tra xem thông tin của Dentist có đầy đủ không
            foreach (var item in dungdvs)
            {
                Console.WriteLine($"PhieuKhamId: {item.PhieuKhamId}, Dentist FullName: {item.Dentist?.FullName}");
            }

            var dentistLookup = dungdvs.ToDictionary(d => d.PhieuKhamId, d => d.Dentist);
            ViewBag.DentistLookup = dentistLookup;

            return View(phieuKhamDichvusGrouped);
        }

       public async Task<IActionResult> SaveDentist(int phieuKhamId, string dentistId)
        {
            try
            {
                // Chuyển dentistId từ string thành Guid
                if (!Guid.TryParse(dentistId, out Guid dentistGuid))
                {
                    return Json(new { success = false, message = "DentistId không hợp lệ." });
                }

                // Lấy đối tượng Dentist từ AspNetUsers (ApplicationUser) theo dentistGuid
                var dentist = await _context.Users.FirstOrDefaultAsync(u => u.Id == dentistGuid.ToString());

                if (dentist == null)
                {
                    return Json(new { success = false, message = "Dentist không tồn tại." });
                }

                // Lấy Phiếu Khám để lấy thông tin Ngày Khám và Khung Giờ
                var phieuKham = await _context.phieukhams
                    .Where(pk => pk.Id == phieuKhamId)
                    .FirstOrDefaultAsync();

                if (phieuKham == null)
                {
                    return Json(new { success = false, message = "Phiếu khám không tồn tại." });
                }

                var ngayKham = phieuKham.NgayKham;
                var phieuKhamTimeSlot = phieuKham.TimeSlot;

                // Tìm KhungGio có TimeSlot tương ứng
                var khungGio = await _context.KhungGios
                    .FirstOrDefaultAsync(k => k.TimeSlot == phieuKhamTimeSlot);

                if (khungGio == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy KhungGiới với TimeSlot tương ứng." });
                }

                // Gắn KhungGioId vào phieuKhamKhungGioId
                var phieuKhamKhungGioId = khungGio.Id; // ID của KhungGio tìm được

                // Lưu thông tin Dentist vào bảng Dungdv
                var dungdv = new Dungdv
                {
                    PhieuKhamId = phieuKhamId,
                    Dentist = dentist, // Gán đối tượng dentist vào trường Dentist thay vì gán giá trị cho DentistId
                };

                _context.Dungdvs.Add(dungdv);
                await _context.SaveChangesAsync();

                // Sau khi lưu Dentist, cập nhật trạng thái của CakhamKhungGio
                // Lấy tất cả các CaKham của nha sĩ đó
                var cakhamIds = await _context.Cakhams
                    .Where(c => c.DentistId == dentistId)
                    .Where(c => c.NgayDang.Date == ngayKham.Date) // Kiểm tra ngày khám trùng
                    .Select(c => c.Id)
                    .ToListAsync();

                var cakhamKhungGios = new List<CakhamKhungGio>();
                if (cakhamIds.Any())
                {
                    cakhamKhungGios = await _context.CakhamKhungGios
                        .Where(ckg => cakhamIds.Contains(ckg.CakhamId) && ckg.TrangThai == TrangThaiCaKham.Chưa_Đặt)
                        .ToListAsync();
                }

                foreach (var cakhamKhungGio in cakhamKhungGios)
                {
                    if (cakhamKhungGio.KhungGioId == phieuKhamKhungGioId)
                    {
                        cakhamKhungGio.TrangThai = TrangThaiCaKham.Đã_Đặt; // Cập nhật trạng thái từ Chưa Đặt -> Đã Đặt
                    }
                }
                // Lưu các thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                var errorMessage = $"Error: {ex.Message}. Inner Exception: {ex.InnerException?.Message}";
                return Json(new { success = false, message = errorMessage });
            }
        }


        // Action để lấy danh sách nha sĩ cho modal
        public async Task<IActionResult> GetDentistsForPhieuKham(int phieuKhamId)
        {
            // Lấy phiếu khám theo ID
            var phieuKham = await _context.phieukhams
                .Where(pk => pk.Id == phieuKhamId)
                .FirstOrDefaultAsync();

            if (phieuKham == null)
            {
                return Json(new List<int>()); // Trả về danh sách rỗng nếu không tìm thấy phiếu khám
            }

            // Lấy TimeSlot từ Phiếu Khám và ngày khám (ngaykham)
            var phieuKhamTimeSlot = phieuKham.TimeSlot;
            var phieuKhamNgayKham = phieuKham.NgayKham; // Giả sử NgayKham là trường ngày trong PhieuKham

            // Lấy danh sách các khung giờ có TimeSlot trùng với TimeSlot của Phiếu Khám và ngày khám bằng với ngày đặt trong CaKham
            var availableCakhamIds = await _context.CakhamKhungGios
                .Where(ckg => ckg.TrangThai == TrangThaiCaKham.Chưa_Đặt)  // Chỉ lấy các khung giờ chưa đặt
                .Where(ckg => ckg.KhungGio.TimeSlot == phieuKhamTimeSlot)  // So sánh TimeSlot
                .Where(ckg => ckg.Cakham.NgayDang.Date == phieuKhamNgayKham.Date)  // So sánh NgayDat trong Cakham với NgayKham trong PhieuKham
                .Select(ckg => ckg.CakhamId)  // Lấy CakhamId từ CakhamKhungGio
                .Distinct()
                .ToListAsync();

            // Lấy DentistId từ các Cakham có trong availableCakhamIds
            var availableDentistIds = await _context.Cakhams
                .Where(c => availableCakhamIds.Contains(c.Id))  // So sánh CakhamId
                .Select(c => c.DentistId)  // Lấy DentistId từ Cakham
                .Distinct()
                .ToListAsync();

            // Lấy tên nha sĩ
            var dentists = await _context.Users
                .Where(u => availableDentistIds.Contains(u.Id)) // Lọc các nha sĩ có trong danh sách availableDentistIds
                .Select(u => new { u.Id, u.FullName }) // Lấy Id và FullName của nha sĩ
                .ToListAsync();

            return Json(dentists); // Trả về danh sách nha sĩ với cả id và tên
        }


    }
}
