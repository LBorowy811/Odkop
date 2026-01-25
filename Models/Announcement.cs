using System.ComponentModel.DataAnnotations;

namespace Odkop.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [StringLength(200, ErrorMessage = "Tytuł nie może być dłuższy niż 200 znaków")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Treść jest wymagana")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public int? AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }
}
