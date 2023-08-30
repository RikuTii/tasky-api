using System.Text.Json.Serialization;

namespace TaskyAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; } 
        public string Email { get; set; }

        [JsonIgnore]
        public virtual UserAccount Account { get; set; } 
        public int? UserAccountId { get; set; }

        [JsonIgnore]
        public string RefreshToken { get; set; }

    }
}
