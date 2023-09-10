using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TaskyAPI.Models
{
    public class TaskMeta
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public int FileId { get; set; }
        public virtual File File { get; set; }
    }
}
