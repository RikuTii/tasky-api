using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TaskyAPI.Models
{
    public class TaskListMeta
    {
        public int Id { get; set; }
        [Key]
        [ForeignKey("TaskList")]
        public int? TaskListId { get; set; }
        public int? UserAccountId { get; set; }
        public virtual UserAccount UserAccount { get; set; }
        public virtual TaskList TaskList { get; set; }
    }
}
