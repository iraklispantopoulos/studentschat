using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Shared;
using WebApiGateway.Session;

namespace WebApiGateway.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IConfiguration configuration,
            IUserRepository userRepository,
            ILogger<AuthController> logger
            )
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthModel model)
        {
            var passwordHasher = new PasswordHasher<object>();
            var user=await GetUser(model.Username, model.Password); 
            // Validate user (replace with your user validation logic)
            if (user!=null)
            {
                var token = await GenerateJwtToken(user.Id);
                return Ok(new { 
                    token, 
                    gender = user.AvatarGenderType,
                    type = user.UserType
                });
            }
            return Unauthorized();
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] AuthModel model)
        {
            _logger.LogInformation("Signup attempt for user {Username}", model.Username);   
            // Validate and save user to database (replace with your logic)
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Example: Save the user (pseudo-code)
            var user = await GetUser(model.Username, model.Password);
            if (user!=null)
            {
                return Conflict("User already exists.");
            }

            string hashedPassword = new PasswordHasher<object>().HashPassword(null, model.Password);
            var userId = await _userRepository.Save(new Repository.Entities.User()
            {
                Guid = Guid.NewGuid().ToString(),
                Username = model.Username,
                Password = hashedPassword,
                UserType=Shared.Enums.UserTypeEnum.Student
            });

            // Generate a token for the new user
            var token = await GenerateJwtToken(userId);         
            return Ok(new { 
                message = "Signup successful", 
                token,
                gender=Shared.Enums.AvatarGenderTypeEnum.Female,
                type= Shared.Enums.UserTypeEnum.Student
            });
        }
        
        [Authorize]
        [HttpPost("trylogin")]
        public IActionResult TryLogin()
        {            
            return Ok();
        }

        private async Task<string> GenerateJwtToken(int userId)
        {
            var key = Encoding.UTF8.GetBytes(_configuration.GetValue<string>("SecretKey")!); // Same as in Program.cs
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.UserData, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "chatbotforstudents", // Same as in Program.cs
                Audience = "chatbotforstudents", // Same as in Program.cs
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private async Task<Repository.Entities.User> GetUser(string username,string password)
        {
            var users = await _userRepository.GetAllWithCriteria(p => p.Username== username);
            return users.FirstOrDefault(p=> new PasswordHasher<object>().VerifyHashedPassword(null, p.Password, password) == PasswordVerificationResult.Success)!;
        }
    }

    public class AuthModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
