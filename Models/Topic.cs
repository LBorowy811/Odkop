using System.ComponentModel.DataAnnotations;

namespace Odkop.Models
{
    public class Topic
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany")]
        [StringLength(100, ErrorMessage = "Tytuł nie może być dłuższy niż 100 znaków")]
        public string Title { get; set; } = string.Empty;

        public int? AuthorId { get; set; }
        public User? Author { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public int ForumId { get; set; }
        public Forum? Forum { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;

        public DateTime? LastPostAt { get; set; }

        public int ViewCount { get; set; } = 0;

        public int PostCount { get; set; } = 0;

        public bool IsPinned { get; set; } = false;

        public bool IsLocked { get; set; } = false;

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
