using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Phongkham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ApplicationUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDBcontext _context;

        public ApplicationUserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDBcontext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index(string role, string sortOrder)
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => userRoles[u.Id].Contains(role)).ToList();
            }
            switch (sortOrder)
            {
                case "Name_asc":
                    users = users.OrderBy(lk => lk.FullName).ToList();
                    break;
                case "Name_desc":
                    users = users.OrderByDescending(lk => lk.FullName).ToList();
                    break;
            }
            ViewBag.CurrentSort = sortOrder;
            ViewBag.UserRoles = userRoles;
            ViewBag.SelectedRole = role;

            return View(users);
        }
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserRoles = roles;

            return View(user);
        }
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách vai trò và chuyên môn
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var specialties = await _context.Chuyenmons.ToListAsync(); // Lấy chuyên môn từ DbContext

            // Tạo ViewBag cho vai trò và chuyên môn
            ViewBag.Roles = new SelectList(roles, user.SelectedRole); // Chọn vai trò hiện tại
            ViewBag.Specialties = new SelectList(specialties, "Id", "Name"); // Chọn chuyên môn hiện tại

            // Nếu người dùng có vai trò Dentist, lấy ChuyenMonId hiện tại
            if (user.SelectedRole == "Dentist")
            {
                var ctnhasi = await _context.cTnhasis.SingleOrDefaultAsync(c => c.UserId == user.Id);
                if (ctnhasi != null)
                {
                    ViewBag.SelectedChuyenMonId = ctnhasi.chuyenmonId; // Lưu giá trị ChuyenMonId vào ViewBag
                }
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Email,FullName,SelectedRole")] ApplicationUser user, int? chuyenMonId)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Không cập nhật UserName
            user.UserName = existingUser.UserName;

            // Chỉ cập nhật email và full name
            existingUser.Email = user.Email;
            existingUser.FullName = user.FullName;

            var result = await _userManager.UpdateAsync(existingUser);
            if (result.Succeeded)
            {
                var existingRoles = await _userManager.GetRolesAsync(existingUser);
                await _userManager.RemoveFromRolesAsync(existingUser, existingRoles);
                await _userManager.AddToRoleAsync(existingUser, user.SelectedRole);

                // Lấy vai trò mới của người dùng
                var newRoles = await _userManager.GetRolesAsync(existingUser);

                // Nếu người dùng từ vai trò Dentist sang vai trò khác
                if (existingRoles.Contains("Dentist") && !newRoles.Contains("Dentist"))
                {
                    // Xóa bản ghi tương ứng trong bảng CTnhasi
                    var ctnhasi = await _context.cTnhasis.SingleOrDefaultAsync(c => c.UserId == user.Id);
                    if (ctnhasi != null)
                    {
                        _context.cTnhasis.Remove(ctnhasi);
                        await _context.SaveChangesAsync();
                    }
                }
                // Nếu ngược lại, người dùng đổi từ vai trò khác sang Dentist
                else if (!existingRoles.Contains("Dentist") && newRoles.Contains("Dentist"))
                {
                    // Kiểm tra chuyên môn
                    if (chuyenMonId == null || !await _context.Chuyenmons.AnyAsync(c => c.Id == chuyenMonId))
                    {
                        ModelState.AddModelError(string.Empty, "Chuyên môn không hợp lệ.");
                        ViewBag.Roles = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList(), user.SelectedRole);
                        ViewBag.Specialties = new SelectList(await _context.Chuyenmons.ToListAsync(), "Id", "Name");
                        return View(user);
                    }

                    // Thêm bản ghi mới vào bảng CTnhasi
                    var ctnhasi = new CTnhasi
                    {
                        UserId = user.Id,
                        chuyenmonId = chuyenMonId.Value // Lấy giá trị từ biến chuyenMonId
                    };

                    _context.cTnhasis.Add(ctnhasi);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Cập nhật lại ViewBag cho vai trò và chuyên môn
            ViewBag.Roles = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList(), user.SelectedRole);
            ViewBag.Specialties = new SelectList(await _context.Chuyenmons.ToListAsync(), "Id", "Name");

            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Khóa tài khoản người dùng
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue; // Khóa vĩnh viễn

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(user);
        }

    }
}
