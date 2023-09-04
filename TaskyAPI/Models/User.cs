using System.Text.Json.Serialization;

namespace TaskyAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; } 
        public string Email { get; set; }

        [JsonIgnore]
        public string RefreshToken { get; set; }

        public virtual ICollection<UserAccount> Accounts { get; set; }


    }
}
