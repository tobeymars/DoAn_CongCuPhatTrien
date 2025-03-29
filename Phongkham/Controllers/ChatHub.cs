using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Phongkham.Data;
using Phongkham.Models;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Phongkham.Controllers
{
    public class ChatHub : Hub
    {
        // Dictionary để lưu trữ UserId và ConnectionId của người dùng
        private static readonly ConcurrentDictionary<string, string> UserConnections = new ConcurrentDictionary<string, string>();
        private readonly ApplicationDBcontext _context;
        public ChatHub(ApplicationDBcontext context)
        {
            _context = context;
        }
        // Khi một người dùng kết nối
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier; // Sử dụng UserId thay vì ConnectionId
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }
            if (Context.User.IsInRole("Admin"))
            {
                // Nếu là admin, thêm vào nhóm "AdminGroup"
                await Groups.AddToGroupAsync(Context.ConnectionId, "AdminGroup");
            }
            else
            {
                // Thêm người dùng vào danh sách với UserId
                UserConnections[userId] = Context.ConnectionId; // Lưu ConnectionId của UserId
            }

            await base.OnConnectedAsync();
        }

        // Phương thức để người dùng gửi tin nhắn tới admin
        public async Task SendMessageToAdmin(string message)
        {
            var userId = Context.UserIdentifier; // Lấy UserId của người gửi
            var Message = new Message
            {
                SenderId = userId,
                ReceiverId = "AdminGroup",
                MessageContent = message,
                SentTime = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(Message);
            await _context.SaveChangesAsync();
            await Clients.Group("AdminGroup").SendAsync("ReceiveMessageFromUser", userId, message);
        }

        public async Task SendMessageToUser(string userId, string message)
        {
            // Lưu tin nhắn vào cơ sở dữ liệu trước, bất kể người dùng có trực tuyến hay không
            var Message = new Message
            {
                SenderId = "Admin",
                ReceiverId = userId,
                MessageContent = message,
                SentTime = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(Message);
            await _context.SaveChangesAsync();

            // Kiểm tra nếu UserId tồn tại trong danh sách kết nối (người dùng đang trực tuyến)
            if (UserConnections.TryGetValue(userId, out var connectionId))
            {
                // Gửi tin nhắn ngay lập tức nếu người dùng đang trực tuyến
                await Clients.Client(connectionId).SendAsync("ReceiveMessageFromAdmin", "Admin", message);
            }
        }
    }
}
