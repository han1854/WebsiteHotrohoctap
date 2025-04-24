using WebsiteHotrohoctap.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebsiteHotrohoctap.Repositories
{
    public interface IMessageRepository
    {
        Task AddMessageAsync(Message message);
        Task<List<Message>> GetMessagesAsync();
    }
}
