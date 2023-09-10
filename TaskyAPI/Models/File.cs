using System.ComponentModel.DataAnnotations;

namespace TaskyAPI.Models
{
    public class File
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; }
        public string Type { get; set; }

    }
}
