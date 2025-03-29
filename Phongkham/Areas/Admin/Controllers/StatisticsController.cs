using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Phongkham.Data;
using Phongkham.Models;
using System.Linq;
namespace Phongkham.Areas.Admin.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public StatisticsController(ApplicationDBcontext context)
        {
            _context = context;
        }

        public IActionResult ServiceRevenueChart()
        {
            var serviceRevenueData = _context.dichvus
                .GroupJoin(
                    _context.PhieuKhamDichvus,
                    dv => dv.Id,
                    pkdv => pkdv.DichvuId,
                    (dv, pkdvs) => new
                    {
                        ServiceName = dv.ten,
                        Revenue = pkdvs.Any() ? pkdvs.Count() * dv.Price : 0
                    })
                .ToList();

            // Truyền dữ liệu dưới dạng JSON vào ViewData
            ViewData["ServiceRevenueData"] = JsonConvert.SerializeObject(serviceRevenueData);

            return View();
        }
    }
}
