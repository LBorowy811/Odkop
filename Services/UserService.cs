using Odkop.Models;
using System.Text.Json;


namespace Odkop.Services
{
    public class UserService
    {
        private readonly string file = Path.Combine(Directory.GetCurrentDirectory(), "Data", "users.json");

        private List<User> Load()
        {
            if (!File.Exists(file)) return new List<User>();
            return JsonSerializer.Deserialize<List<User>>(File.ReadAllText(file)) ?? new();
        }

        private void Save(List<User> users)
        {
            File.WriteAllText(file, JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true }));
        }

        public bool Register(User user)
        {
            var users = Load();
            if (users.Any(u => u.Username == user.Username)) return false;
            users.Add(user);
            Save(users);
            return true;
        }

        public bool Validate(string username, string password)
        {
            return Load().Any(u => u.Username == username && u.Password == password);
        }
    }
}
