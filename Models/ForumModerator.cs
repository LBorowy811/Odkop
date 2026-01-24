namespace Odkop.Models
{
    // Tabela łącząca dla relacji wiele-do-wielu: User <-> Forum (moderatorzy)
    public class ForumModerator
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public int ForumId { get; set; }
        public Forum? Forum { get; set; }
    }
}
