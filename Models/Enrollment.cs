namespace WebsiteHotrohoctap.Models
{
    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public DateOnly EnrollmentDate { get; set; }
        public string CompletionStatus { get; set; }
        public User User { get; set; }
        public int CourseID { get; set; }
        public Course Course { get; set; }
    }
}
