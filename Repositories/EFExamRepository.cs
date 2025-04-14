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

        public async Task<IEnumerable<Exam>> GetAllAsync()
        {
            return await _context.Exams.Include(p => p.Lesson).ToListAsync();

        }

        public async Task<Exam> GetByIdAsync(int id)
        {
            return await _context.Exams.Include(p => p.Lesson).FirstOrDefaultAsync(p => p.ExamID == id);
        }

        public async Task AddAsync(Exam exam)
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
