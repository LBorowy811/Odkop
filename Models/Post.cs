using System.ComponentModel.DataAnnotations;

namespace Odkop.Models
{
    public class Post
    {
        public int Id { get; set; }

        public int TopicId { get; set; }
        public Topic? Topic { get; set; }

        public int? AuthorId { get; set; }
        public User? Author { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Treść jest wymagana")]
        public string Content { get; set; } = string.Empty;

        public DateTime Created { get; set; } = DateTime.Now;

        public DateTime? EditedAt { get; set; }

        public int? EditedById { get; set; }
        public User? EditedBy { get; set; }
    }
}
