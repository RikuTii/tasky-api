using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

namespace TaskyAPI.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
        public int CreatorId { get; set; }
        public virtual UserAccount Creator { get; set; }
        public int TaskListId { get; set; }
        public virtual TaskList TaskList { get; set; }
        public int? Status { get; set; }
        public int Ordering { get; set; }
        public virtual ICollection<TaskMeta>? Meta { get; set; }
        public int? IsPast { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int? TimeTrack { get; set; }
        public long? TimeEstimate { get; set; }
        public long? TimeElapsed { get; set; }


    }
}
