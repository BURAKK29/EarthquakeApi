using EarthaquakeApplication.Entities;
using EarthaquakeApplication.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EarthquakeApi.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        //UserManager:kayıt, giriş, güncelleme, silme, şifre işlemleri, kullanıcı bilgisi çekme vs. işlerini yapar
        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> MyProfile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User); // sadece giriş yapan kullanıcı
            return View(user);
        }
    }
}