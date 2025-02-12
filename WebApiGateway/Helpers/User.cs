using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Shared;
using WebApiGateway.Controllers;
using Shared.Enums;
using Namotion.Reflection;
using Repository;

namespace WebApiGateway.Session
{    
    public class User
    {
        private readonly ILogger<User> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        public User(ILogger<User> logger, IHttpContextAccessor httpContextAccessor,IUserRepository userRepository)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
        }
        public int GetId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.UserData)!;
            var userId = userIdClaim?.Value;
            return int.Parse(userId!);
        }
        public async Task<AvatarGenderTypeEnum> GetAvatarGenderType()
        {
            AvatarGenderTypeEnum? avatarGenderType = _httpContextAccessor.HttpContext?.Session.TryGetPropertyValue<AvatarGenderTypeEnum>("avatar-gender");
            if (!avatarGenderType.HasValue)
            {
                var user = await _userRepository.Get(GetId());
                avatarGenderType = user?.AvatarGenderType;
                if (!avatarGenderType.HasValue)
                {
                    avatarGenderType = AvatarGenderTypeEnum.Male;
                }else
                    _httpContextAccessor.HttpContext?.Session.SetString("avatar-gender", avatarGenderType.ToString()!);
            }         
            return avatarGenderType!.Value;
        }
        public void SetAvatarGenderType(AvatarGenderTypeEnum avatarGenderType)
        {
            _httpContextAccessor.HttpContext?.Session.SetString("avatar-gender", "Admin");
        }
    }    
}
