using Microsoft.AspNetCore.Mvc;
using TrainingTrackerAPI.Data;
using TrainingTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace TrainingTrackerAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddCourse(Course course)
        {
            _context.Courses.Add(course);
            _context.SaveChanges();
            return Ok(course);
        }

        [HttpGet]
        public IActionResult GetCourses()
        {
            return Ok(_context.Courses.ToList());
        }
    }
}