using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string OptionsJson { get; set; } = "[]";

        [NotMapped]
        public List<string> Options
        {
            get => string.IsNullOrEmpty(OptionsJson)
                ? new List<string>()
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(OptionsJson);
            set => OptionsJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        public string? StarterCode { get; set; }
        public string? SampleInput { get; set; }
        public string? SampleOutput { get; set; }
        public string? Status { get; set; }
        public string? Language { get; set; }
    }
}
