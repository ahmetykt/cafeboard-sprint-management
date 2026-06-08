using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CafeBoard.Models
{
    public class Developer
    {
        [Key]
        public int DeveloperId { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Role { get; set; } // Örn: Backend, Frontend, PM

        // Bir geliştiricinin birden fazla görevi olabilir (Bire-Çok İlişki)
        public List<CafeTask> Tasks { get; set; }
    }
}