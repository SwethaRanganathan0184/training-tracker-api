using Microsoft.AspNetCore.Identity;

namespace TrainingTrackerAPI.Models.Auth
{
    public class ApplicationUser : IdentityUser
    {
        // Link login account to employee (null for admin)
        public int? EmployeeId { get; set; }
    }
}