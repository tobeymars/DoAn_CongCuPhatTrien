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

namespace Phongkham.Areas.Patient.Controllers
{
    [Area("Patient")]
    [Authorize(Roles = SD.Role_Patient)]

    public class TintucsController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public TintucsController(ApplicationDBcontext context)
        {
            _context = context;
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
    }
}
