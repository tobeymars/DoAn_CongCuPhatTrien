using Microsoft.AspNetCore.Mvc;
using Phongkham.Models;
using Phongkham.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Phongkham.Areas.Dentist.Controllers
{
    [Area("Dentist")]
    [Authorize(Roles = SD.Role_Dentist)]
    public class ImageController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public ImageController(ApplicationDBcontext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userImages = await _context.UserImages.ToListAsync();
            return View(userImages);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserImage userImage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userImage);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userImage = await _context.UserImages.FindAsync(id);
            if (userImage == null)
            {
                return NotFound();
            }
            return View(userImage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserImage userImage)
        {
            if (id != userImage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserImageExists(userImage.Id))
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
            return View(userImage);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userImage = await _context.UserImages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userImage == null)
            {
                return NotFound();
            }

            return View(userImage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userImage = await _context.UserImages.FindAsync(id);
            _context.UserImages.Remove(userImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserImageExists(int id)
        {
            return _context.UserImages.Any(e => e.Id == id);
        }
    }
}
