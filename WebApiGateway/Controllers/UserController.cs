using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Tcp;
using WebApiGateway.Configuration;
using WebApiGateway.Helpers;
using WebApiGateway.Models;
using WebApiGateway.Session;

namespace WebApiGateway.Controllers
{
    [Authorize]    
    [ApiController]
    [Route("api/[controller]")]    
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly Session.User _sessionUser;
        private readonly IUserRepository _userRepository;
        public UserController(
            ILogger<UserController> logger,
            IUserRepository userRepository,
            Session.User sessionUser
            )
        {
            _logger = logger;            
            _sessionUser = sessionUser;            
            _userRepository = userRepository;
        }
        [HttpPost("SetAvatarGender", Name = "SetAvatarGender")]
        public async Task<ActionResult> SetAvatarGender(AvatarGender model)
        {
            await _userRepository.Update(_sessionUser.GetId(), p => p.AvatarGenderType = model.Type);
            _sessionUser.SetAvatarGenderType(model.Type);
            return Ok();            
        }
        [HttpPost("GetAllStudents", Name = "GetAllStudents")]
        public async Task<ActionResult<List<Models.User>>> GetAllStudents()
        {
            var students = await _userRepository.GetAllWithCriteria(p => p.UserType == Shared.Enums.UserTypeEnum.Student);
            return Ok(students
                        .Select(p => new Models.User()
                            {
                                Id = p.Id,
                                Name = p.Username,
                                AvatarGenderType = p.AvatarGenderType
                            })
                        .ToList()
                     );
        }
    }
}
