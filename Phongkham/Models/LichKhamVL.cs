using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class LichKhamVL
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public string phone { get; set; }
        [EnumDataType(typeof(TrangThaiCaKham))]
        public TrangThaiCaKham TrangThai { get; set; }
        public int KhungGioId { get; set; }
        public KhungGio? KhungGio { get; set; }
        public List<CtlichkhamVL> ClichkhamVL { get; set; }
    }
}
