using Microsoft.AspNetCore.Identity;

namespace WebsiteHotrohoctap.Models
{
    public class Message
    {
        public int MessageID { get; set; }
        public string UserID { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        public User User { get; set; }
    }

}
