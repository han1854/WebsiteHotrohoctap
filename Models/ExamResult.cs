using System.ComponentModel.DataAnnotations;

namespace WebsiteHotrohoctap.Models
{
    public class ExamResult
    {
        [Key]
        public int ResultID { get; set; } //primary key
        public int Score { get; set; }
        public string Code { get; set; } // Mã nguồn người dùng nộp
        public string Input { get; set; } // Input cho chương trình
        public string Output { get; set; } // Kết quả chạy mã
        public string Status { get; set; }
        public DateTime ExamDate { get; set; }
        public User User { get; set; }
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }
    }
}
