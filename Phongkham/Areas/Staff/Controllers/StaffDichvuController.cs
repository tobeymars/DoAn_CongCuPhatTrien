using Microsoft.AspNetCore.Mvc;
using Phongkham.Models;
using Phongkham.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Phongkham.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = SD.Role_Staff)]
    public class StaffDichvuController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public StaffDichvuController(ApplicationDBcontext context)
        {
            _context = context;
        }

        // Hiển thị danh sách dịch vụ
        public async Task<IActionResult> Index()
        {
            var dichvus = await _context.dichvus.ToListAsync();
            return View(dichvus);
        }

        // Trang tạo dịch vụ mới (GET)
        public IActionResult Create()
        {
            return View(new dichvu());
        }

        // POST: Dichvu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(dichvu dichvu, List<IFormFile> images, IFormFile mainImage)
        {
            if (!ModelState.IsValid)
            {
                // Lưu ảnh chính vào thuộc tính ImageUrl
                if (mainImage != null)
                {
                    var mainImageUrl = await SaveImage(mainImage);
                    dichvu.ImageUrl = mainImageUrl; // Lưu ảnh chính vào bảng dichvu
                }

                // Lưu các ảnh phụ vào bảng DichvuImage
                if (images != null && images.Count > 0)
                {
                    dichvu.Images = new List<DichvuImage>();

                    foreach (var image in images)
                    {
                        var imageUrl = await SaveImage(image);
                        dichvu.Images.Add(new DichvuImage { Url = imageUrl }); // Lưu ảnh vào bảng DichvuImage
                    }
                }

                _context.Add(dichvu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dichvu);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối của ảnh
        }
        // Trang chỉnh sửa dịch vụ (GET)
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var dichvu = await _context.dichvus.FindAsync(id);
            if (dichvu == null)
            {
                return NotFound();
            }
            return View(dichvu);
        }

        // Xử lý chỉnh sửa dịch vụ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, dichvu dichvu, IFormFile imageUrl)
        {
            if (id != dichvu.Id)
            {
                return NotFound();
            }
            try
            {
                _context.Update(dichvu);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DichVuExists(dichvu.Id))
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

        // Action GET: Hiển thị form xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            var dichvu = await _context.dichvus.FindAsync(id);
            if (dichvu == null)
            {
                return NotFound();
            }
            return View(dichvu); // Trả về view cho việc xác nhận xóa
        }

        // Action POST: Xác nhận xóa và thực hiện xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dichvu = await _context.dichvus.FindAsync(id);
            if (dichvu == null)
            {
                return NotFound();
            }

            _context.dichvus.Remove(dichvu);  // Xóa dịch vụ
            await _context.SaveChangesAsync(); // Lưu thay đổi

            return RedirectToAction(nameof(Index)); // Quay lại danh sách
        }




        // Kiểm tra xem dịch vụ có tồn tại không
        private bool DichVuExists(int id)
        {
            return _context.dichvus.Any(e => e.Id == id);
        }
    }
}
