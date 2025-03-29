namespace Phongkham.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string MessageContent { get; set; }
        public DateTime SentTime { get; set; }
        public bool IsRead { get; set; }
    }
}
