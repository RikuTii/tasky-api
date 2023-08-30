
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TaskyAPI.Models;

namespace TaskyAPI.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        [StringLength(50)]
        [Display(Name = "Name")]
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Locale { get; set; }
        [JsonIgnore]
        public int UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }

    }
}
