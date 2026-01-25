using System.ComponentModel.DataAnnotations;

namespace Odkop.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa kategorii jest wymagana")]
        [StringLength(100, ErrorMessage = "Nazwa kategorii nie może być dłuższa niż 100 znaków")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public ICollection<Forum> Forums { get; set; } = new List<Forum>();
    }
}
