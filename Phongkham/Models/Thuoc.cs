using Phongkham.Data;
using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class Thuoc
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên Thuốc không được để trống.")]
        public string TenThuoc { get; set; }
        public string MoTa { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là một số dương.")]
        public int SoLuong { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public bool IsDeleted { get; set; } // Lưu trạng thái xóa mềm
        public virtual ICollection<ChiTietDonThuoc> ChiTietDonThuocs { get; set; } 
    }
}
