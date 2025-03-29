using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class ChiTietDonThuoc
    {
        public int Id { get; set; }
        public int DonThuocId { get; set; }
        public virtual DonThuoc DonThuoc { get; set; }
        [Required]
        public int ThuocId { get; set; }
        public virtual Thuoc Thuoc { get; set; }
        [Required]
        public string LieuLuong { get; set; }
    }
}
