namespace WebsiteHotrohoctap.Models.ViewModels
{
    public class ExamViewModel
    {
        public int ExamID { get; set; }
        public int LessonID { get; set; }
        public string ExamName { get; set; }
        public string ProblemDescription { get; set; }
        public string SampleInput { get; set; }
        public string SampleOutput { get; set; }
        public int TotalMarks { get; set; }
    }
}