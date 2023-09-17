using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TaskyAPI.Models
{
    public class TaskListMeta
    {
        [Key]
        public int Id { get; set; }
        public int? TaskListId { get; set; }
        public int? UserAccountId { get; set; }
        public virtual UserAccount UserAccount { get; set; }
        public virtual TaskList TaskList { get; set; }
    }
}
