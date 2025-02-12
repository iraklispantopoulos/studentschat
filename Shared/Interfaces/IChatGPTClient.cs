
using System.Threading.Tasks;

namespace Shared
{
    public interface IChatGPTClient
    {
        Task<string> GetResponse(string userPrompt, string agentinstructions);
        Task<ResponsePrompt> GetSsmlResponse(string prompt, string userChatHistory, string agentChatHistory);
        Task<ResponsePrompt> GetSsmlResponse(string prompt, string agentInstructions);
    }
}