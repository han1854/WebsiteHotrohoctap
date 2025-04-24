using Microsoft.EntityFrameworkCore;
using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Repositories
{
    public class EFExamRepository : IExamRepository
    {
        private readonly ApplicationDbContext _context;

        public EFExamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Exam>> GetAllAsync()
        {
            return await _context.Exams
                .Include(e => e.Lesson)
                .ToListAsync();
        }

        public async Task<Exam> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Lesson) // Include nếu cần liên kết với Lesson
                .FirstOrDefaultAsync(e => e.ExamID == id);
        }

        public async Task AddExamAsync(Exam exam)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Exam exam)
        {
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
        }
    }
}
