using System.ComponentModel.DataAnnotations;

namespace WebsiteHotrohoctap.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        [Required, StringLength(255)]
        public string CourseName { get; set; }
        public string? Description { get; set; }
        public List<Lesson>? Lessons { get; set; }
    }
}
