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
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

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
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            // Zaktualizuj ostatnie logowanie
            user.LastLoginAt = DateTime.Now;
            _db.SaveChanges();

            return RedirectToAction("Index", "Forum");
        }

        // WYLOGOWANIE
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // PROFIL
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login");

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        // EDYCJA PROFILU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string? about)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login");

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            user.About = about;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Profil został zaktualizowany.";
            return View(user);
        }

        // WIDOK PROFILU INNEGO UŻYTKOWNIKA
        public async Task<IActionResult> ViewProfile(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // UPLOAD AVATARA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login");

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            if (avatar == null || avatar.Length == 0)
            {
                TempData["Error"] = "Wybierz plik obrazu.";
                return RedirectToAction("Profile");
            }

            // Sprawdź rozmiar (max 2MB)
            if (avatar.Length > 2 * 1024 * 1024)
            {
                TempData["Error"] = "Plik jest za duży. Maksymalny rozmiar to 2MB.";
                return RedirectToAction("Profile");
            }

            // Sprawdź rozszerzenie
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(avatar.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Niedozwolony format pliku. Dozwolone: JPG, PNG, GIF, WEBP.";
                return RedirectToAction("Profile");
            }

            // Usuń stary avatar jeśli istnieje
            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            // Utwórz folder jeśli nie istnieje
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            Directory.CreateDirectory(uploadsFolder);

            // Zapisz nowy avatar
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            user.AvatarUrl = $"/uploads/avatars/{fileName}";
            await _db.SaveChangesAsync();

            TempData["Success"] = "Zdjęcie profilowe zostało zaktualizowane.";
            return RedirectToAction("Profile");
        }

        // USUWANIE AVATARA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvatar()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login");

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            if (!string.IsNullOrEmpty(user.AvatarUrl))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                user.AvatarUrl = null;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Zdjęcie profilowe zostało usunięte.";
            }

            return RedirectToAction("Profile");
        }
    }
}
