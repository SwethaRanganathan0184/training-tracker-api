using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TrainingTrackerAPI.Models.Auth;
using TrainingTrackerAPI.DTOs.Auth;
using TrainingTrackerAPI.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TrainingTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!new[] { "Admin", "Employee" }.Contains(request.Role))
                return BadRequest("Invalid role");

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest("User already exists");

            int? linkedEmployeeId = null;

            // If registering as employee, find their record by email
            if (request.Role == "Employee")
            {
                var employee = _context.Employees
                    .FirstOrDefault(e => e.Email.ToLower() == request.Email.ToLower());

                if (employee == null)
                    return BadRequest("No employee record found for this email. Contact your admin.");

                linkedEmployeeId = employee.Id;
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmployeeId = linkedEmployeeId
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, request.Role);

            return Ok("User registered successfully");
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return Ok(new
            {
                token,
                role = roles.FirstOrDefault(),
                employeeId = user.EmployeeId
            });
        }

        // ---------------- JWT GENERATION ----------------
        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("employeeId", user.EmployeeId?.ToString() ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:DurationInMinutes"]!)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}