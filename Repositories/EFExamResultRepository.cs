using Microsoft.EntityFrameworkCore;
using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Repositories
{
    public class EFExamResultRepository : IExamResultRepository
    {
        private readonly ApplicationDbContext _context;

        public EFExamResultRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExamResult>> GetAllAsync()
        {
            return await _context.ExamResults
                                 .Include(p => p.Exam)
                                 .Include(p => p.User)
                                 .ToListAsync();
        }

        public async Task<ExamResult> GetByIdAsync(int id)
        {
            return await _context.ExamResults
                                 .Include(p => p.Exam)
                                 .Include(p => p.User)
                                 .FirstOrDefaultAsync(p => p.ResultID == id);
        }


        public async Task AddAsync(ExamResult examresult)
        {
            _context.ExamResults.Add(examresult);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExamResult examresult)
        {
            _context.ExamResults.Update(examresult);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var examresult = await _context.ExamResults.FindAsync(id);
            _context.ExamResults.Remove(examresult);
            await _context.SaveChangesAsync();
        }
    }
}
