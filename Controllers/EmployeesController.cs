using Microsoft.AspNetCore.Mvc;
using TrainingTrackerAPI.Data;
using TrainingTrackerAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace TrainingTrackerAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/employees
        [HttpPost]
        public IActionResult AddEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            _context.SaveChanges();
            return Ok(employee);
        }

        // GET: api/employees
        [HttpGet]
        public IActionResult GetEmployees()
        {
            return Ok(_context.Employees.ToList());
        }
    }
}