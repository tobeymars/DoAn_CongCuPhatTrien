using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Phongkham.Data;

namespace Phongkham.Controllers
{
    // Nguyen minh nhut
    public class ChatController : Controller
    {
        private readonly ApplicationDBcontext _context;

        public ChatController(ApplicationDBcontext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserList()
        {
            // Lấy tất cả các SenderId trừ "Admin" và loại bỏ các giá trị trùng lặp
            var userIds = await _context.Messages
                .Where(m => m.SenderId != "Admin") // Loại bỏ các tin nhắn có SenderId là Admin
                .Select(m => m.SenderId) // Chỉ lấy SenderId của người dùng
                .Distinct() // Đảm bảo không trùng lặp
                .ToListAsync();

            return Json(userIds);
        }



        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string userId)
        {
            // Kiểm tra nếu userId được truyền vào hợp lệ
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            // Truy vấn tin nhắn giữa Admin và người dùng cụ thể
            var messages = await _context.Messages
                                         .Where(m =>
                                             (m.SenderId == "Admin" && m.ReceiverId == userId) ||
                                             (m.SenderId == userId && m.ReceiverId == "AdminGroup"))
                                         .OrderBy(m => m.SentTime)
                                         .ToListAsync();

            return Json(messages);
        }


        [HttpGet]
        public async Task<int> GetUnreadMessageCount(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return 0; // Nếu không có userId, trả về 0
            }

            var unreadMessagesCount = await _context.Messages
                .Where(m => m.ReceiverId == userId && m.SenderId == "Admin" && m.IsRead == false)
                .CountAsync();

            return unreadMessagesCount;
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllMessagesAsRead(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID không hợp lệ");
            }

            // Lấy tất cả tin nhắn chưa đọc của người dùng
            var unreadMessages = await _context.Messages
                .Where(m => m.ReceiverId == userId && m.SenderId == "Admin" && m.IsRead == false)
                .ToListAsync();

            // Cập nhật tất cả các tin nhắn thành đã đọc
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                _context.Messages.Update(message);
            }

            // Lưu thay đổi
            await _context.SaveChangesAsync();

            return Ok(); // Trả về thành công
        }


    }
}
