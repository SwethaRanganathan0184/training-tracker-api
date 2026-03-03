using Microsoft.AspNetCore.Mvc;
using TrainingTrackerAPI.Data;
using TrainingTrackerAPI.Models;
using TrainingTrackerAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace TrainingTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/employee-courses")]
    public class EmployeeCoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeCoursesController(AppDbContext context)
        {
            _context = context;
        }

        // ADMIN: Assign course to employee
        [Authorize(Roles = "Admin")]
        [HttpPost("assign")]
        public IActionResult AssignCourse([FromBody] AssignCourseDto dto)
        {
            var exists = _context.EmployeeCourses
                .Any(ec => ec.EmployeeId == dto.EmployeeId && ec.CourseId == dto.CourseId);

            if (exists)
                return BadRequest("Course already assigned to this employee.");

            var ec = new EmployeeCourse
            {
                EmployeeId = dto.EmployeeId,
                CourseId = dto.CourseId,
                IsCompleted = false
            };

            _context.EmployeeCourses.Add(ec);
            _context.SaveChanges();
            return Ok(ec);
        }

        // ADMIN: Mark course as completed for an employee
        [Authorize(Roles = "Admin")]
        [HttpPut("complete/{id}")]
        public IActionResult CompleteCourse(int id)
        {
            var record = _context.EmployeeCourses.Find(id);
            if (record == null) return NotFound();

            record.IsCompleted = true;
            _context.SaveChanges();
            return Ok(record);
        }

        // ADMIN: Get completion percentage for any employee
        [Authorize(Roles = "Admin")]
        [HttpGet("completion/{employeeId}")]
        public IActionResult GetCompletionPercentage(int employeeId)
        {
            var totalAssigned = _context.EmployeeCourses
                .Count(ec => ec.EmployeeId == employeeId);

            if (totalAssigned == 0)
                return Ok(0);

            var completed = _context.EmployeeCourses
                .Count(ec => ec.EmployeeId == employeeId && ec.IsCompleted);

            return Ok((completed * 100) / totalAssigned);
        }

        // ADMIN: Get total score for any employee
        [Authorize(Roles = "Admin")]
        [HttpGet("score/{employeeId}")]
        public IActionResult GetScore(int employeeId)
        {
            var score = _context.EmployeeCourses
                .Where(ec => ec.EmployeeId == employeeId && ec.IsCompleted)
                .Include(ec => ec.Course)
                .Sum(ec => ec.Course.Weight);

            return Ok(score);
        }

        // EMPLOYEE: Get own total score
        [Authorize(Roles = "Employee")]
        [HttpGet("score/me")]
        public IActionResult GetMyScore()
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;
            if (string.IsNullOrEmpty(employeeIdClaim))
                return Unauthorized();

            int employeeId = int.Parse(employeeIdClaim);

            var score = _context.EmployeeCourses
                .Where(ec => ec.EmployeeId == employeeId && ec.IsCompleted)
                .Include(ec => ec.Course)
                .Sum(ec => ec.Course.Weight);

            return Ok(score);
        }

        // EMPLOYEE: Get own assigned courses with course details
        [Authorize(Roles = "Employee")]
        [HttpGet("my-courses")]
        public IActionResult GetMyCourses()
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim))
                return Unauthorized();

            int employeeId = int.Parse(employeeIdClaim);

            var courses = _context.EmployeeCourses
                .Where(ec => ec.EmployeeId == employeeId)
                .Include(ec => ec.Course)
                .Select(ec => new
                {
                    ec.Id,
                    ec.CourseId,
                    CourseName = ec.Course.CourseName,
                    Weight = ec.Course.Weight,
                    ec.IsCompleted
                })
                .ToList();

            return Ok(courses);
        }

        // EMPLOYEE: Get own completion percentage
        [Authorize(Roles = "Employee")]
        [HttpGet("completion/me")]
        public IActionResult GetMyCompletionPercentage()
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim))
                return Unauthorized();

            int employeeId = int.Parse(employeeIdClaim);

            var totalAssigned = _context.EmployeeCourses
                .Count(ec => ec.EmployeeId == employeeId);

            if (totalAssigned == 0)
                return Ok(0);

            var completed = _context.EmployeeCourses
                .Count(ec => ec.EmployeeId == employeeId && ec.IsCompleted);

            return Ok((completed * 100) / totalAssigned);
        }

        // EMPLOYEE: Mark own course as completed
        [Authorize(Roles = "Employee")]
        [HttpPut("complete/me/{employeeCourseId}")]
        public IActionResult MarkMyCourseComplete(int employeeCourseId)
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;

            if (string.IsNullOrEmpty(employeeIdClaim))
                return Unauthorized();

            int employeeId = int.Parse(employeeIdClaim);

            var record = _context.EmployeeCourses
                .FirstOrDefault(ec => ec.Id == employeeCourseId && ec.EmployeeId == employeeId);

            if (record == null)
                return NotFound("Course not found or does not belong to you.");

            record.IsCompleted = true;
            _context.SaveChanges();
            return Ok(record);
        }
    }
}