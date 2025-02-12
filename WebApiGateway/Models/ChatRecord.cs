namespace WebApiGateway.Models
{
    public class ChatRecord
    {
        public string Message { get; set; }
        public Shared.Enums.PromptTypeEnum PromptType { get; set; }
        public DateTime Date { get; set; }
    }
}
