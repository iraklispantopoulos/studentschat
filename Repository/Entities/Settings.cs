using Shared.Enums;

namespace Repository.Entities
{
    public class Settings
    {
        [TableColumnAttr]
        public string Name { get; set; }
        [TableColumnAttr]
        public string ValueInJsonFormat { get; set; }
    }
}
