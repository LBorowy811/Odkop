using System.ComponentModel.DataAnnotations;

namespace Odkop.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Nazwa użytkownika musi mieć od 3 do 50 znaków")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format email")]
        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        public DateTime? LastLoginAt { get; set; }

        [StringLength(500)]
        public string? About { get; set; }

        public string? AvatarUrl { get; set; }

        public int PostCount { get; set; } = 0;

        public ICollection<ForumModerator> ModeratedForums { get; set; } = new List<ForumModerator>();
    }
}
