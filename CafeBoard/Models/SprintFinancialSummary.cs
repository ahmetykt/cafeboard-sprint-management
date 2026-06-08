using System;
using System.ComponentModel.DataAnnotations;

namespace CafeBoard.Models
{
    public class SprintFinancialSummary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SprintName { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal TotalExpense { get; set; }

        public decimal NetProfit { get; set; }

        public int CompletedTaskCount { get; set; }

        public DateTime ClosedDate { get; set; }

    }
}