using Shared.Enums;
using Shared.Enums;

namespace Repository.Entities
{
    public class User:Base,ITable
    {
        [TableColumnAttr]
        public string Username { get; set; }
        [TableColumnAttr]
        public string Password { get; set; }
        [TableColumnAttr]
        public string CurrentPromptId { get; set; } = "";
        [TableColumnAttr]
        public string CurrentUnitId { get; set; } = "";
        [TableColumnAttr]
        public UserTypeEnum UserType { get; set; }
        [TableColumnAttr]
        public AvatarGenderTypeEnum AvatarGenderType { get; set; }
        [TableColumnAttr]
        public List<Shared.UserAttribute> Attributes { get; set; } = new List<Shared.UserAttribute>();
    }
}
