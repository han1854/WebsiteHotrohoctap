using System.ComponentModel.DataAnnotations;

namespace WebsiteHotrohoctap.Models
{
    public class LessonContent
    {
        [Key]
        public int ContentID { get; set; }
        [Required]
        public string ContentType { get; set; } = "";
        public string? ContentData { get; set; }
        [Required]
        public int LessonID { get; set; }
        public Lesson? Lesson { get; set; }
    }
}
