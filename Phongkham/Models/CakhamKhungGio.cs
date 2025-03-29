using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class CakhamKhungGio
    {
        public int CakhamId { get; set; }
        public Cakham Cakham { get; set; }
        [EnumDataType(typeof(TrangThaiCaKham))]
        public TrangThaiCaKham TrangThai { get; set; }
        public int KhungGioId { get; set; }
        public KhungGio KhungGio { get; set; }
    }

}
