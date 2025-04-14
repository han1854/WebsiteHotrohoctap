using Microsoft.EntityFrameworkCore;
using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Repositories
{
    public class EFLessonContentRepository : ILessonContentRepository
    {
        private readonly ApplicationDbContext _context;
        public EFLessonContentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<LessonContent>> GetAllAsync()
        {
            return await _context.LessonContents.Include(p => p.Lesson).ToListAsync();
        }

        public async Task<LessonContent> GetByIdAsync(int id)
        {
            return await _context.LessonContents.Include(p => p.Lesson).FirstOrDefaultAsync(p => p.ContentID == id);
        }

        public async Task AddAsync(LessonContent lessoncontent)
        {
            _context.LessonContents.Add(lessoncontent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LessonContent lessoncontent)
        {
            _context.LessonContents.Update(lessoncontent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var lessoncontent = await _context.LessonContents.FindAsync(id);
            _context.LessonContents.Remove(lessoncontent);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LessonContent>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.LessonContents
                                 .Where(lc => lc.LessonID == lessonId)
                                 .ToListAsync();
        }


    }
}
