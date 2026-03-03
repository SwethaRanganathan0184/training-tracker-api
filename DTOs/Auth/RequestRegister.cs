namespace TrainingTrackerAPI.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Only required for employee registration
        public int? EmployeeId { get; set; }

        // "Admin" or "Employee"
        public string Role { get; set; } = string.Empty;
    }
}