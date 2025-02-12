using Shared.Enums;

namespace WebApiGateway.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AvatarGenderTypeEnum AvatarGenderType { get; set; }
    }
}
