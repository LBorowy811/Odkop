using Microsoft.AspNetCore.Mvc;
using Odkop.Models;
using Odkop.Models.ViewModels;
using System.Text.Json;

namespace Odkop.Controllers
{
    public class AccountController : Controller
    {
        private readonly string usersFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");

        //wczytanie listy uzytkownikow z jsona, potem na baze zmienic trzeba
        private List<User> LoadUsers()
        {
            if (!System.IO.File.Exists(usersFile))
            {
                // Tworzymy pusty plik, jeśli go nie ma
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(usersFile)!);
                System.IO.File.WriteAllText(usersFile, "[]");
                return new List<User>();
            }

            var json = System.IO.File.ReadAllText(usersFile);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        //zapis do jsona, trzeba potem na baze zmienic
        private void SaveUsers(List<User> users)
        {
            System.IO.File.WriteAllText(usersFile, JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true }));
        }

        // Rejestracja - GET
        public IActionResult Register()
        {
            return View();
        }

        // Rejestracja - POST
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var users = LoadUsers();

            //sprawdzenie czy uzytkownik istnieje
            if (users.Any(u => u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("", "Użytkownik o tej nazwie już istnieje");
                return View(model);
            }

            //dodanie uzytkownika
            users.Add(new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password
            });

            SaveUsers(users);

            HttpContext.Session.SetString("User", model.Username);

            return RedirectToAction("Index", "Forum");
        }

        // Logowanie - GET
        public IActionResult Login()
        {
            return View();
        }

        // Logowanie - POST
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var users = LoadUsers();

            var user = users.FirstOrDefault(u =>
                u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == model.Password);

            if (user != null)
            {
                HttpContext.Session.SetString("User", user.Username);
                return RedirectToAction("Index", "Forum");
            }

            ModelState.AddModelError("", "Nieprawidłowy login lub hasło");
            return View(model);
        }

        //wylogowywanie
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User");
            return RedirectToAction("Login");
        }
    }
}
