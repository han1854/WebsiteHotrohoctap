using System.ComponentModel.DataAnnotations;

namespace WebsiteHotrohoctap.Models
{
    public class Lesson
    {
        public int LessonID { get; set; }
        [Required, StringLength(255)]
        public string LessonName { get; set; }
        public string? LessonDescription { get; set; }
        public int CourseID { get; set; }
        public Course? Course { get; set; }
        public List<Exam>? Exams { get; set; }
        public List<LessonContent>? LessonContents { get; set; }
    }
}
