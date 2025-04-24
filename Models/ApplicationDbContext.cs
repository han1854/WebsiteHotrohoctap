using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebsiteHotrohoctap.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonContent> LessonContents { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<ExamContent> ExamContents { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<Message> Messages { get; set; }

    }
}
