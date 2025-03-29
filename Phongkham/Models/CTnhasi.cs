namespace Phongkham.Models
{
    public class CTnhasi
    {
        public int Id {  get; set; }
        public int chuyenmonId { get; set; }
        public Chuyenmon Chuyenmon { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
