using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class PhieuKhamDichvu
    {
        public int PhieuKhamId { get; set; }
        public Phieukham PhieuKham { get; set; }  // Liên kết tới PhieuKham

        public int DichvuId { get; set; }
        public dichvu Dichvu { get; set; }
    }
}
