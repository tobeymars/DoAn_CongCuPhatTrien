using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;

namespace Phongkham.Areas.Admin.Controllers {
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ChuyenmonsController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public ChuyenmonsController(ApplicationDBcontext context)
        {
            _context = context;
        }

        // GET: Chuyenmons
        public async Task<IActionResult> Index(string sortOrder)
        {
            // Lấy danh sách chuyên môn
            var chuyenmons = await _context.Chuyenmons.ToListAsync();

            // Lấy số lượng nha sĩ theo chuyên môn
            var nhaSiCounts = await _context.cTnhasis
                .GroupBy(n => n.chuyenmonId)
                .Select(g => new { ChuyenMonId = g.Key, Count = g.Count() })
                .ToListAsync();
            switch (sortOrder)
            {
                case "Name_asc":
                    chuyenmons = chuyenmons.OrderBy(lk => lk.Name).ToList();
                    break;
                case "Name_desc":
                    chuyenmons = chuyenmons.OrderByDescending(lk => lk.Name).ToList();
                    break;
                case "count_asc":
                    nhaSiCounts = nhaSiCounts.OrderBy(lk => lk.Count).ToList();
                    break;
                case "count_desc":
                    nhaSiCounts = nhaSiCounts.OrderByDescending(lk => lk.Count).ToList();
                    break;
                    // Có thể thêm các trường sắp xếp khác tại đây
            }
            ViewBag.CurrentSort = sortOrder;
            // Chuyển đổi danh sách kết quả thành Dictionary
            ViewBag.NhaSiCounts = nhaSiCounts.ToDictionary(x => x.ChuyenMonId, x => x.Count);
            // Trả về danh sách chuyên môn
            return View(chuyenmons);
        }

        // GET: Chuyenmons/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chuyenmons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Chuyenmon chuyenmon)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem chuyên môn đã tồn tại chưa
                var existingChuyenmon = await _context.Chuyenmons
                    .FirstOrDefaultAsync(cm => cm.Name == chuyenmon.Name);

                if (existingChuyenmon != null)
                {
                    // Thông báo lỗi nếu chuyên môn đã tồn tại
                    ModelState.AddModelError("Name", "Chuyên môn này đã tồn tại.");
                    return View(chuyenmon);
                }

                _context.Add(chuyenmon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chuyenmon);
        }

        // GET: Chuyenmons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuyenmon = await _context.Chuyenmons.FindAsync(id);
            if (chuyenmon == null)
            {
                return NotFound();
            }
            return View(chuyenmon);
        }

        // POST: Chuyenmons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?linkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Chuyenmon chuyenmon)
        {
            if (id != chuyenmon.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {// Kiểm tra xem chuyên môn đã tồn tại chưa
                var existingChuyenmon = await _context.Chuyenmons
                    .FirstOrDefaultAsync(cm => cm.Name == chuyenmon.Name);

                if (existingChuyenmon != null)
                {
                    // Thông báo lỗi nếu chuyên môn đã tồn tại
                    ModelState.AddModelError("Name", "Chuyên môn này đã tồn tại.");
                    return View(chuyenmon);
                }
                try
                {
                    _context.Update(chuyenmon);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChuyenmonExists(chuyenmon.Id))
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
            return View(chuyenmon);
        }

        // GET: Chuyenmons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chuyenmon = await _context.Chuyenmons
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chuyenmon == null)
            {
                return NotFound();
            }

            return View(chuyenmon);
        }

        // POST: Chuyenmons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chuyenmon = await _context.Chuyenmons.FindAsync(id);
            if (chuyenmon != null)
            {
                _context.Chuyenmons.Remove(chuyenmon);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChuyenmonExists(int id)
        {
            return _context.Chuyenmons.Any(e => e.Id == id);
        }
    }
}
