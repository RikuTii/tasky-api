using System.ComponentModel.DataAnnotations;

namespace TaskyAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int ReceiverId { get; set; }
        public virtual UserAccount? Receiver { get; set; }
        public required string Name { get; set; }
        public required string Data { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; }
    }
}
