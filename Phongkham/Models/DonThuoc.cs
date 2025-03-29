using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class DonThuoc
    {
        public int Id { get; set; }
        [Required]
        public string TenBenhNhan { get; set; }
        [Required]
        public string TenNhaSi { get; set; }
        public DateTime NgayLap { get; set; } = DateTime.Now; // Ngày lập đơn
        public string TimeSlot { get; set; }
        public string GhiChu { get; set; }
        public bool IsDeleted { get; set; } // Lưu trạng thái xóa mềm
        public virtual ICollection<ChiTietDonThuoc> ChiTietDonThuocs { get; set; }
        // Thêm cột khoá ngoại có thể null
        public int? CTlichkhamId { get; set; }
        public CTlichkham CTlichkham { get; set; }

        public int? CtlichkhamVLId { get; set; }
        public CtlichkhamVL CtlichkhamVL { get; set; }
    }

}
