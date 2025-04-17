using System.ComponentModel.DataAnnotations;

namespace WebsiteHotrohoctap.Models
{
    public class LessonContent
    {
        [Key]
        public int ContentID { get; set; }
        public int LessonID { get; set; }
        public string? ContentData { get; set; }
        public string? ContentType { get; set; }

        // Quan hệ với Lesson
        public Lesson Lesson { get; set; }
    }
}
