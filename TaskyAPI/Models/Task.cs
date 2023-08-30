using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TaskyAPI.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
        public int CreatorId { get; set; }
        public virtual UserAccount Creator { get; set; }
        public int TaskListId { get; set; }
        public virtual TaskList TaskList { get; set; }
        public int? Status { get; set; }
        public int Ordering { get; set; }

    }
}
