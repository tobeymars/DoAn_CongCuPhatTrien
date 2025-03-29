using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;

namespace Phongkham.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = SD.Role_Staff)]
    public class StaffThuocController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public StaffThuocController(ApplicationDBcontext context)
        {
            _context = context;
        }

        // GET: Thuoc
        public async Task<IActionResult> Index()
        {
            var thuocs = await _context.Thuocs.ToListAsync(); // Lấy tất cả thuốc
            return View(thuocs);
        }

        // GET: Thuoc/Create
        public IActionResult Create()
        {
            return View(new Thuoc());
        }

        // POST: Thuoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Thuoc thuoc)
        {
            // Kiểm tra tên thuốc có bị trùng không
            bool isExists = await _context.Thuocs.AnyAsync(t => t.TenThuoc == thuoc.TenThuoc);
            if (isExists)
            {
                ModelState.AddModelError("TenThuoc", "Tên thuốc đã tồn tại. Vui lòng chọn tên khác !");
                return View(thuoc);
            }
            if (!ModelState.IsValid)
            {     
                thuoc.IsDeleted = false; // Ensure the new record is not marked as deleted
                _context.Add(thuoc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(thuoc);
        }

        // GET: Thuoc/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var thuoc = await _context.Thuocs.FindAsync(id);
            if (thuoc == null)
            {
                return NotFound();
            }
            // Kiểm tra nếu thuốc đã bị xóa
            if (thuoc.IsDeleted)
            {
                TempData["ErrorMessage"] = "Thuốc này đã bị xóa và không thể chỉnh sửa !";
                return RedirectToAction(nameof(Index)); // Chuyển hướng về trang danh sách
            }

            return View(thuoc);
        }

        // POST: Thuoc/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Thuoc thuoc)
        {
            if (id != thuoc.Id)
            {
                return BadRequest();
            }
            bool isExists = await _context.Thuocs.AnyAsync(t => t.TenThuoc == thuoc.TenThuoc && t.Id != thuoc.Id);
            if (isExists)
            {
                ModelState.AddModelError("TenThuoc", "Tên thuốc đã tồn tại. Vui lòng chọn tên khác !");
                return View(thuoc);
            }
            if (!ModelState.IsValid)
            {
                try
                {
                    thuoc.IsDeleted = false;
                    _context.Update(thuoc);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã sửa thông tin thuốc thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThuocTonTai(thuoc.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(thuoc);
        }

        // GET: Thuoc/Delete
        public async Task<IActionResult> Delete(int id)
        {
            var thuoc = await _context.Thuocs.FindAsync(id);
            if (thuoc == null || thuoc.IsDeleted)
            {
                return NotFound();
            }
            return View(thuoc);
        }

        // POST: Thuoc/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thuoc = await _context.Thuocs.FindAsync(id);
            if (thuoc != null)
            {
                thuoc.IsDeleted = true; // Mark as deleted
                _context.Thuocs.Update(thuoc);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa thuốc thành công";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Thuoc/Restore
        public async Task<IActionResult> Restore(int id)
        {
            var thuoc = await _context.Thuocs.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id);
            if (thuoc == null)
            {
                return NotFound();
            }

            return View(thuoc);
        }

        // POST: Thuoc/RestoreConfirmed
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            var thuoc = await _context.Thuocs.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id);
            if (thuoc == null)
            {
                return NotFound();
            }

            thuoc.IsDeleted = false; // Đánh dấu là không bị xóa nữa
            _context.Thuocs.Update(thuoc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã khôi phục thuốc thành công";

            return RedirectToAction(nameof(Index));
        }

        private bool ThuocTonTai(int id)
        {
            return _context.Thuocs.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}
