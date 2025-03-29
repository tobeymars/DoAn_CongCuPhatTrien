namespace Phongkham.Models
{
    public class KhungGio
    {
        public int Id { get; set; }
        public string TimeSlot { get; set; }
        public ICollection<CakhamKhungGio> CakhamKhungGios { get; set; }
    }
}
