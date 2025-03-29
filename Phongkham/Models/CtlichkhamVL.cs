using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Phongkham.Models
{
    public class CtlichkhamVL
    {
        public int Id { get; set; }
        [Required]
        public int LichkhamVLId { get; set; }
        public LichKhamVL? LichKhamVL { get; set; }
        public string TenNhaSi { get; set; }
        public string TenBenhNhan { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn địa điểm khám")]
        public string PhongKham { get; set; }
    }
}
