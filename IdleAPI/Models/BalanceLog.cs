using System.ComponentModel.DataAnnotations;

namespace IdleAPI.Models
{
    public class BalanceLog
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int User_Id { get; set; }
        [Required]
        public decimal value { get; set; }
        [Required]
        public DateTime change_time { get; set; }
    }
}
