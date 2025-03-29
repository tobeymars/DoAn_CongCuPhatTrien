namespace Phongkham.Models
{
    public class DichvuImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int DichvuId { get; set; }
        public dichvu? dichvu { get; set; }
    }
}
