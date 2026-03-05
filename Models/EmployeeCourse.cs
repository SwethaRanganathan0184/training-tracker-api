// EmployeeCourse.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainingTrackerAPI.Models
{
    public class EmployeeCourse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public bool IsCompleted { get; set; }
    }
}
