using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class Cakham
    {
        public int Id { get; set; }
        public DateTime NgayDang { get; set; } = DateTime.Now;
        public string DentistId { get; set; }
        public ApplicationUser? Dentist { get; set; }
        public ICollection<CakhamKhungGio> CakhamKhungGios { get; set; }
    }
}