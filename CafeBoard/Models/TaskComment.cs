using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeBoard.Models
{
    public class TaskComment
    {
        [Key]
        public int CommentId { get; set; }
        public int TaskId { get; set; }
        public int? DeveloperId { get; set; }

        [Required]
        public string CommentText { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("DeveloperId")]
        public Developer Developer { get; set; }
    }
}