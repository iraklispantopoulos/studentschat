using Microsoft.EntityFrameworkCore;
using Repository;
using Shared;
using Shared.Enums;
using WebApiGateway.Session;

namespace WebApiGateway.Helpers
{
    public class SpeechGeneratorFactory
    {
        private readonly IUserRepository _userRepository;        
        private readonly ILogger<SpeechGeneratorFactory> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly User _sessionUser;
        public SpeechGeneratorFactory(
            ILogger<SpeechGeneratorFactory> logger, 
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            User sessionUser
            )
        {
            _logger = logger;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _sessionUser = sessionUser;
        }
        public async Task<ISpeechGenerator> GetSpeechGenerator()
        {
            var serviceProvider = ServiceProviderWrapper.ServiceProvider;
            var user = await _userRepository.Get(_sessionUser.GetId());
            if (user == null)
            {
                throw new Exception("User not found");
            }
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServiceProvider = scope.ServiceProvider;
                //return scopedServiceProvider.GetService<GoogleTextToSpeech.SpeechGenerator>()!;
                return user.AvatarGenderType switch
                {
                    AvatarGenderTypeEnum.Male => scopedServiceProvider.GetService<WindowsNarratorSpeechGenerator>()!,
                    AvatarGenderTypeEnum.Female => scopedServiceProvider.GetService<GoogleTextToSpeech.SpeechGenerator>()!,
                    _ => throw new ArgumentOutOfRangeException(nameof(user.AvatarGenderType), user.AvatarGenderType, null)
                };
            }            
        }
    }
}
