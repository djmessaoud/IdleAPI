using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdleAPI.Models
{
    [Index(nameof(Access_token), IsUnique = true)]
    [Index(nameof(User_id), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public long User_id { get; set; }
        public decimal Balance { get; set; }
        public string? Access_token { get; set; }
        public string role { get; set; }
    }
}
