using System.ComponentModel.DataAnnotations;

namespace Odkop.Models
{
    public class Forum
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa forum jest wymagana")]
        [StringLength(100, ErrorMessage = "Nazwa forum nie może być dłuższa niż 100 znaków")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Uprawnienia dla użytkowników anonimowych
        public bool AllowAnonymousView { get; set; } = true;
        public bool AllowAnonymousPost { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public ICollection<Topic> Topics { get; set; } = new List<Topic>();
        public ICollection<ForumModerator> Moderators { get; set; } = new List<ForumModerator>();
    }
}
