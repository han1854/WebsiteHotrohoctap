using Microsoft.EntityFrameworkCore;
using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Repositories
{
    public class EFLessonRepository : ILessonRepository
    {
        private readonly ApplicationDbContext _context;
        public EFLessonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lesson>> GetAllAsync()
        {
            return await _context.Lessons.Include(p => p.Course).ToListAsync();

        }

        public async Task<Lesson> GetByIdAsync(int id)
        {
            return await _context.Lessons.Include(p => p.Course).FirstOrDefaultAsync(p => p.LessonID == id);
        }

        public async Task AddAsync(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Lesson lesson)
        {
            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Lesson>> GetLessonsByCourseIdAsync(int courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseID == courseId)
                .ToListAsync();
        }

    }
}
