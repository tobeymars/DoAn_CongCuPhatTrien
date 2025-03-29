using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Phongkham.Models
{
    public class Phieukham
    {
        public int Id { get; set; }

        [Required]
        public DateTime NgayKham { get; set; }
        public string TimeSlot { get; set; }

        [Required]
        public string TenNhaSi { get; set; }

        [Required]
        public string TenBenhNhan { get; set; }

        public string GhiChu { get; set; }
        public ICollection<PhieuKhamDichvu> PhieuKhamDichvus { get; set; }
        public ICollection<Dungdv> dungdvs { get; set; }
        [Required]
        public bool TaiKham { get; set; }
        [Required]
        public bool TrangThaiTaiKham { get; set; }
        public string Thoigiantaikham { get; set; }
        // Thêm cột khoá ngoại có thể null
        public int? CTlichkhamId { get; set; }
        public CTlichkham CTlichkham { get; set; }

        public int? CtlichkhamVLId { get; set; }
        public CtlichkhamVL CtlichkhamVL { get; set; }
    }
}
