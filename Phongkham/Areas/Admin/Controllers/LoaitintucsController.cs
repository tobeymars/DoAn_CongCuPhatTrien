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

namespace Phongkham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class LoaitintucsController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public LoaitintucsController(ApplicationDBcontext context)
        {
            _context = context;
        }

        // GET: Loaitintucs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Loaitintucs.ToListAsync());
        }

        // GET: Loaitintucs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaitintuc = await _context.Loaitintucs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loaitintuc == null)
            {
                return NotFound();
            }

            return View(loaitintuc);
        }

        // GET: Loaitintucs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Loaitintucs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?linkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Loaitintuc loaitintuc)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loaitintuc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loaitintuc);
        }

        // GET: Loaitintucs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaitintuc = await _context.Loaitintucs.FindAsync(id);
            if (loaitintuc == null)
            {
                return NotFound();
            }
            return View(loaitintuc);
        }

        // POST: Loaitintucs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?linkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Loaitintuc loaitintuc)
        {
            if (id != loaitintuc.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaitintuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaitintucExists(loaitintuc.Id))
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
            return View(loaitintuc);
        }

        // GET: Loaitintucs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaitintuc = await _context.Loaitintucs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loaitintuc == null)
            {
                return NotFound();
            }

            return View(loaitintuc);
        }

        // POST: Loaitintucs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loaitintuc = await _context.Loaitintucs.FindAsync(id);
            if (loaitintuc != null)
            {
                _context.Loaitintucs.Remove(loaitintuc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoaitintucExists(int id)
        {
            return _context.Loaitintucs.Any(e => e.Id == id);
        }
    }
}
