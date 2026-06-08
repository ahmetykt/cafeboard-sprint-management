using System;
using System.ComponentModel.DataAnnotations;

namespace CafeBoard.Models
{
    public class Sprint
    {
        [Key]
        public int SprintId { get; set; }
        public string SprintName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}