using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using System.Text.RegularExpressions;

namespace Phongkham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class TintucsController : Controller
    {

        private readonly ApplicationDBcontext _context;
        private readonly TextToSpeechService _textToSpeechService;

        public TintucsController(ApplicationDBcontext context, TextToSpeechService textToSpeechService)
        {
            _context = context;
            _textToSpeechService = textToSpeechService;
        }

        // GET: Tintucs
        public async Task<IActionResult> Index()
        {
            var applicationDBcontext = _context.Tintucs.Include(t => t.Loaitintuc);
            return View(await applicationDBcontext.ToListAsync());
        }

        // GET: Tintucs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tintuc = await _context.Tintucs
                .Include(t => t.Loaitintuc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tintuc == null)
            {
                return NotFound();
            }

            return View(tintuc);
        }

        // GET: Tintucs/Create
        public IActionResult Create()
        {
            ViewData["LoaitintucId"] = new SelectList(_context.Loaitintucs, "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,tieude,Noidung,NgayDang,LoaitintucId")] Tintuc tintuc, IFormFile ImageUrl, IFormFile Mp3File)
        {
            if (!ModelState.IsValid)
            {
                // Lưu hình ảnh nếu có
                if (ImageUrl != null)
                {
                    tintuc.ImageUrl = await SaveImage(ImageUrl);
                }

                // Chuyển văn bản thành MP3 nếu có nội dung
                if (!string.IsNullOrEmpty(tintuc.Noidung) && Mp3File != null)
                {
                    try
                    {
                        // Lưu file MP3 vào thư mục wwwroot/mp3
                        var fileName = Guid.NewGuid().ToString() + ".mp3";
                        var mp3Path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/mp3", fileName);

                        using (var stream = new FileStream(mp3Path, FileMode.Create))
                        {
                            await Mp3File.CopyToAsync(stream);
                        }

                        // Lưu đường dẫn MP3 vào cơ sở dữ liệu
                        tintuc.Mp3Url = $"/mp3/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Có lỗi khi lưu file MP3: " + ex.Message);
                        ViewData["LoaitintucId"] = new SelectList(_context.Loaitintucs, "Id", "Name", tintuc.LoaitintucId);
                        return View(tintuc);
                    }
                }

                // Lưu tin tức vào cơ sở dữ liệu
                _context.Add(tintuc);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["LoaitintucId"] = new SelectList(_context.Loaitintucs, "Id", "Name", tintuc.LoaitintucId);
            return View(tintuc);
        }


        private async Task<string> DownloadAndSaveMp3Async(string mp3Url, string fileName)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(mp3Url);
                if (response.IsSuccessStatusCode)
                {
                    var mp3Data = await response.Content.ReadAsByteArrayAsync();
                    var mp3Path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/mp3", fileName);
                    await System.IO.File.WriteAllBytesAsync(mp3Path, mp3Data);
                    return $"/mp3/{fileName}"; // Đường dẫn tương đối tới file MP3 đã lưu
                }
                else
                {
                    throw new Exception("Failed to download MP3 file.");
                }
            }
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName); //

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }
        // GET: Tintucs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tintuc = await _context.Tintucs.FindAsync(id);
            if (tintuc == null)
            {
                return NotFound();
            }
            ViewData["LoaitintucId"] = new SelectList(_context.Loaitintucs, "Id", "Name", tintuc.LoaitintucId);
            return View(tintuc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,tieude,Noidung,NgayDang,ImageUrl,LoaitintucId,Mp3Url")] Tintuc tintuc, IFormFile imageUrl, IFormFile mp3Url)
        {
            if (id != tintuc.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageUrl != null)
                    {
                        tintuc.ImageUrl = await SaveImage(imageUrl);
                    }             

                    _context.Update(tintuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TintucExists(tintuc.Id))
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
            ViewData["LoaitintucId"] = new SelectList(_context.Loaitintucs, "Id", "Name", tintuc.LoaitintucId);
            return View(tintuc);
        }

        // GET: Tintucs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tintuc = await _context.Tintucs
                .Include(t => t.Loaitintuc)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tintuc == null)
            {
                return NotFound();
            }

            return View(tintuc);
        }

        // POST: Tintucs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tintuc = await _context.Tintucs.FindAsync(id);
            if (tintuc != null)
            {
                _context.Tintucs.Remove(tintuc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TintucExists(int id)
        {
            return _context.Tintucs.Any(e => e.Id == id);
        }
    }
}
