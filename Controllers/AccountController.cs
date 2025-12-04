using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Odkop.Data;
using Odkop.Models;
using Odkop.Models.ViewModels;

namespace Odkop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // REJESTRACJA GET
        public IActionResult Register()
        {
            return View();
        }

        // REJESTRACJA POST
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // sprawdzenie czy istnieje
            var exists = _db.Users.Any(u => u.Username.ToLower() == model.Username.ToLower());
            if (exists)
            {
                ModelState.AddModelError("", "Użytkownik o tej nazwie już istnieje");
                return View(model);
            }

            // dodaj usera
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            // zaloguj od razu
            HttpContext.Session.SetString("User", user.Username);

            return RedirectToAction("Index", "Forum");
        }

        // LOGIN GET
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN POST
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.FirstOrDefault(u =>
                u.Username.ToLower() == model.Username.ToLower() &&
                u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Nieprawidłowy login lub hasło");
                return View(model);
            }

            HttpContext.Session.SetString("User", user.Username);

            return RedirectToAction("Index", "Forum");
        }

        // WYLOGOWANIE
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User");
            return RedirectToAction("Login");
        }
    }
}
