
using System.ComponentModel.DataAnnotations;

namespace IdleAPI.Models
{
    public class Config
    {
        [Key]
        [Required]
        public string Key { get; set; }
        [Required]
        public string Value { get; set; }
    }
}
