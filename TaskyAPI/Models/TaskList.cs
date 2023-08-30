using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace TaskyAPI.Models
{
    public class TaskList
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; }
        public int CreatorId { get; set; }
        public virtual UserAccount Creator { get; set; }
        public virtual ICollection<TaskListMeta>? TaskListMetas { get; set; }
        public virtual ICollection<Task>? Tasks { get; set; }
    }
}
