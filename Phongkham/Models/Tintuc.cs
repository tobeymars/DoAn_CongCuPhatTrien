    namespace Phongkham.Models
    {
        public class Tintuc
        {
            public int Id { get; set; }
            public string tieude { get; set; }
            public string Noidung { get; set; }
            public DateTime NgayDang { get; set; } = DateTime.Now;
            public string? ImageUrl { get; set; }
            public List<TintucImage>? Images { get; set; }
            public int LoaitintucId { get; set; }
            public Loaitintuc? Loaitintuc { get; set; }
            public string? Mp3Url { get; set; }

        }
    }