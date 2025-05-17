using System.ComponentModel.DataAnnotations;

namespace WebsiteHotrohoctap.Models
{
    public class ExamContent
    {
        public int ExamContentID { get; set; }

        [Required]
        public string QuestionType { get; set; }

        [Required]
        [StringLength(1000)]
        public string? QuestionText { get; set; }

        public string? CorrectAnswer { get; set; }

        [Required]
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }

        // --------- Thêm cho dạng Code ----------
        public string? Language { get; set; }
        public string? StarterCode { get; set; }
        public string? SampleInput { get; set; }
        public string? ExpectedOutput { get; set; }

    }
}
