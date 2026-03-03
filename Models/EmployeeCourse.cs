namespace TrainingTrackerAPI.Models
{
    public class EmployeeCourse
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }   // navigation

        public int CourseId { get; set; }
        public Course Course { get; set; }       // navigation

        public bool IsCompleted { get; set; }
    }
}