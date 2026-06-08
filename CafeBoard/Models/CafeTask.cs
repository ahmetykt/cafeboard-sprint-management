using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeBoard.Models
{
    public class CafeTask
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Status { get; set; }

        public string Priority { get; set; }

        // ZIRHLAMA: SQL'de boş veri olsa bile sistemin çökmesini engellemek için ? ekledik
        public string? TaskType { get; set; }
        public string? Sprint { get; set; }
        public int? StoryPoints { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Görev başlangıç tarihi (kullanıcı tarafından girilir)
        public DateTime? StartDate { get; set; }

        public DateTime? Deadline { get; set; }
        public bool IsDeleted { get; set; } = false;

        // İlerleme yüzdesi (0-100)
        public int ProgressPercent { get; set; } = 0;

        public int? DeveloperId { get; set; }
        [ForeignKey("DeveloperId")]
        public Developer? Developer { get; set; }
    }
}