using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdleAPI.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public long User_Id { get; set; }
        [Required]
        public  bool isStarted { get; set; }
        [Required]
        public bool isComplete { get; set; }
        [Required]
        public decimal value { get; set; }
        [Required]
        public float progress { get; set; }
        public DateTime start_time { get; set; }
    }
}
