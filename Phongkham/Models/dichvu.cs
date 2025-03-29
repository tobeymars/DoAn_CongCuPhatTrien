using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class dichvu
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ten { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<DichvuImage>? Images { get; set; }
        public bool IsDeleted { get; set; } // Lưu trạng thái xóa mềm

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public ICollection<PhieuKhamDichvu> PhieuKhamDichvus { get; set; }
    }
}
