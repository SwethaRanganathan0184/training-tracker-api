using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TrainingTrackerAPI.Models;
using TrainingTrackerAPI.Models.Auth;

namespace TrainingTrackerAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<EmployeeCourse> EmployeeCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EmployeeCourse>()
                .HasOne(ec => ec.Employee)
                .WithMany()
                .HasForeignKey(ec => ec.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeCourse>()
                .HasOne(ec => ec.Course)
                .WithMany()
                .HasForeignKey(ec => ec.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}