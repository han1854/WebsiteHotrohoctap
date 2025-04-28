using Microsoft.EntityFrameworkCore;
using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Repositories
{
    public class EFExamContentRepository : IExamContentRepository
    {
        private readonly ApplicationDbContext _context;
        public EFExamContentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ExamContent>> GetAllAsync()
        {
            return await _context.ExamContents.Include(p => p.Exam).ToListAsync();
        }

        public async Task<ExamContent> GetByIdAsync(int id)
        {
            return await _context.ExamContents.Include(p => p.Exam).FirstOrDefaultAsync(p => p.ExamContentID == id);
        }

        public async Task AddAsync(ExamContent examcontent)
        {
            _context.ExamContents.Add(examcontent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExamContent examcontent)
        {
            _context.ExamContents.Update(examcontent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var examcontent = await _context.ExamContents.FindAsync(id);
            if (examcontent == null)
            {
                throw new ArgumentNullException(nameof(examcontent), "Không tìm thấy bản ghi cần xóa.");
            }

            _context.ExamContents.Remove(examcontent);
            await _context.SaveChangesAsync();
        }
    }
}
