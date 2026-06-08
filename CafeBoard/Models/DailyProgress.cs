using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeBoard.Models
{
    public class DailyProgress
    {
        [Key]
        public int ProgressId { get; set; }

        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public CafeTask Task { get; set; }

        public int DeveloperId { get; set; }

        [ForeignKey("DeveloperId")]
        public Developer Developer { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now.Date;

        // 0-100 arası ilerleme yüzdesi
        [Range(0, 100)]
        public int ProgressPercent { get; set; }

        // O gün yapılan iş açıklaması
        public string? Notes { get; set; }

        // O gün kaç saat çalışıldı
        public decimal HoursWorked { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
