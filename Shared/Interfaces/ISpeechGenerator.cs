
using System.Threading.Tasks;

namespace Shared
{
    public interface ISpeechGenerator
    {
        Task<TextToSpeechResponse> GenerateAsync(string text, string ssmlText, string outputFileName);
    }
}