namespace Phongkham.Models
{
    public class Dungdv
    {
        public int Id { get; set; }
        public int PhieuKhamId { get; set; }
        public Phieukham phieukham { get; set; }
        // Thay đổi kiểu DentistId thành Guid
        public Guid DentistId { get; set; }
        public ApplicationUser Dentist { get; set; }
    }
}
