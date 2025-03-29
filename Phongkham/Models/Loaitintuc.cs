using System.ComponentModel.DataAnnotations;

namespace Phongkham.Models
{
    public class Loaitintuc
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        public List<Tintuc>? Tintucs { get; set; }
    }
}
