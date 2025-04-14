namespace WebsiteHotrohoctap.Models
{
    public class Exam
    {
        public int ExamID { get; set; }
        public string ExamName { get; set; }
        public string Description { get; set; }
        public int TotalMarks { get; set; }
        public int LessonID { get; set; }
        public Lesson? Lesson { get; set; }
        public List<ExamContent>? ExamContents { get; set; }
        public List<ExamResult>? examResults { get; set; }
    }
}
