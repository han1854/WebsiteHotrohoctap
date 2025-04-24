using Microsoft.EntityFrameworkCore;
using WebsiteHotrohoctap.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebsiteHotrohoctap.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public MessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Message>> GetMessagesAsync()
        {
            return await _context.Messages
                .Include(m => m.User)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }
}