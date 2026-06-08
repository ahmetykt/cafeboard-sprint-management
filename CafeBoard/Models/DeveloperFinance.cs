using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafeBoard.Models
{
    public class DeveloperFinance
    {
        [Key]
        public int FinanceId { get; set; }

        public int DeveloperId { get; set; }

        [ForeignKey("DeveloperId")]
        public Developer Developer { get; set; }

        public decimal HourlyRate { get; set; }
        public decimal BaseSalary { get; set; }
    }
}