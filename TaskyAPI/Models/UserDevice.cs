using System.ComponentModel.DataAnnotations;

namespace TaskyAPI.Models
{
    public class UserDevice
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int AccountId { get; set; }
        public virtual UserAccount Account { get; set; }
        public DateTime LastActive { get; set; }
        public string? AuthToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? FcmToken { get; set; }
    }
}
