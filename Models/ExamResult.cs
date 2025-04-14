using System.ComponentModel.DataAnnotations;

namespace WebsiteHotrohoctap.Models
{
    public class ExamResult
    {
        [Key]
        public int ResultID { get; set; } //primary key
        public int Score { get; set; }
        public DateTime ExamDate { get; set; }
        public User User { get; set; }
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }
    }
}
