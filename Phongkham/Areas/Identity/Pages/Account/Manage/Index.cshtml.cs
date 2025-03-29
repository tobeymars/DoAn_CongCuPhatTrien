using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;
using Phongkham.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Phongkham.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDBcontext _context;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDBcontext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string LayoutPath { get; set; }
        public string Username { get; set; }
        public bool IsDentist { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ImageUrl { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile Image { get; set; }

            [Display(Name = "Chuyên môn")]
            public int? ChuyenMonId { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> ChuyenMonList { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            ImageUrl = user.ImageUrl;

            var roles = await _userManager.GetRolesAsync(user);
            IsDentist = roles.Contains("Dentist");

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                ChuyenMonId = await GetChuyenMonId(user.Id),
                ChuyenMonList = await _context.Chuyenmons
                    .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
                    .ToListAsync()
            };
        }

        private async Task<int?> GetChuyenMonId(string userId)
        {
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            var cTnhasi = await _context.cTnhasis.FirstOrDefaultAsync(x => x.UserId == userId);
            return cTnhasi?.chuyenmonId;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            LayoutPath = roles.Contains("Admin") ? "~/Views/Shared/_AdLayout.cshtml" :
                         roles.Contains("Dentist") ? "~/Views/Shared/_LayoutDentist.cshtml" :
                         "~/Views/Shared/_Layout.cshtml";

            return Page();
        }

        // Trong phương thức OnPostAsync của controller

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["LoaiTinTuc"] = _context.Loaitintucs.ToList();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.Image != null)
            {
                var filePath = Path.Combine("wwwroot/images", Input.Image.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.Image.CopyToAsync(stream);
                }
                user.ImageUrl = $"/images/{Input.Image.FileName}";
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set profile picture.";
                    return RedirectToPage();
                }
            }

            if (Input.ChuyenMonId.HasValue && IsDentist)
            {
                var cTnhasi = await _context.cTnhasis.FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (cTnhasi != null)
                {
                    cTnhasi.chuyenmonId = Input.ChuyenMonId.Value;
                    _context.cTnhasis.Update(cTnhasi);
                }
                else
                {
                    cTnhasi = new CTnhasi
                    {
                        chuyenmonId = Input.ChuyenMonId.Value,
                        UserId = user.Id
                    };
                    _context.cTnhasis.Add(cTnhasi);
                }
                await _context.SaveChangesAsync();
            }
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
