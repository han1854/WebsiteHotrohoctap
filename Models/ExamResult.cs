using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebsiteHotrohoctap.Models
{
    public class ExamResult
    {
        [Key]
        public int ResultID { get; set; } // PK

        [Required]
        public string UserId { get; set; } // FK tới User

        [ForeignKey("UserId")]
        public User User { get; set; }

        public int ExamID { get; set; } // FK tới Exam
        public Exam Exam { get; set; }

        public int Score { get; set; } // Tổng điểm

        public DateTime ExamDate { get; set; }

        // Nếu làm bài trắc nghiệm:
        public string? Answer { get; set; } // Đáp án người dùng chọn (optional)

        // Nếu làm bài viết code:
        public string? Code { get; set; } // Mã nguồn người dùng nộp
        public string? Input { get; set; } // Input đầu vào cho code
        public string? Output { get; set; } // Output chạy ra

        public string? Status { get; set; } // Pass/Fail hoặc Correct/Wrong
    }
}
