using Shared.Enums;

namespace Repository.Entities
{
    public class Base
    {
        [TableColumnAttr]
        public string Guid { get; set; }
        [TableColumnAttr]
        public int Id { get; set; }
    }
}
