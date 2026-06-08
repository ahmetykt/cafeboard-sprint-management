using System;
using System.ComponentModel.DataAnnotations;

namespace CafeBoard.Models
{
    public class TaskLog
    {
        [Key]
        public int LogId { get; set; }

        public int TaskId { get; set; }

        public CafeTask Task { get; set; }

        public string LogMessage { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}