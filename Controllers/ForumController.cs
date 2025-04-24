using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebsiteHotrohoctap.Models;
using WebsiteHotrohoctap.Repositories;

namespace WebsiteHotrohoctap.Controllers
{
    public class ForumController : Controller
    {
        private readonly IMessageRepository _messageRepository;
    private readonly UserManager<User> _userManager;

    public ForumController(IMessageRepository messageRepository, UserManager<User> userManager)
    {
        _messageRepository = messageRepository;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var messages = await _messageRepository.GetMessagesAsync();
        return View(messages);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SendMessage(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return RedirectToAction("Index");
        }

        var user = await _userManager.GetUserAsync(User);
        var message = new Message
        {
            UserID = user.Id,
            Content = content,
            Timestamp = DateTime.Now
        };

        await _messageRepository.AddMessageAsync(message);
        return RedirectToAction("Index");
    }
}
}
