using WebsiteHotrohoctap.Models;

namespace WebsiteHotrohoctap.Repositories
{
    public interface IExamResultRepository
    {
        Task<IEnumerable<ExamResult>> GetAllAsync();
        Task<ExamResult> GetByIdAsync(int id);
        Task AddAsync(ExamResult examResult);
        Task UpdateAsync(ExamResult examResult);
        Task DeleteAsync(int id);
       
    }
}
