namespace WebsiteHotrohoctap.Models
{
    public class ExamContent
    {
        public int ExamContentID { get; set; }
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public int ExamID { get; set; }
        public Exam? Exam { get; set; }

    }
}
