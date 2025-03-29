using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Phongkham.Models
{
    public class Chuyenmon
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [JsonIgnore]
        public List<CTnhasi>? cTnhasis { get; set; }
    }
}
