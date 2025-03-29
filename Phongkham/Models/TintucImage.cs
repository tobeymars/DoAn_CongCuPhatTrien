namespace Phongkham.Models
{
    public class TintucImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int TintucId { get; set; }
        public Tintuc? Tintuc { get; set; }
    }
}
