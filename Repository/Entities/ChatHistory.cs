using Shared.Enums;

namespace Repository.Entities
{
    public class ChatHistory:Base,ITable
    {
        [TableColumnAttr]
        public DateTime Date { get; set; }
        [TableColumnAttr]
        public PromptTypeEnum PromptType { get; set; }
        [TableColumnAttr]
        public string Text { get; set; }
        [TableColumnAttr]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
