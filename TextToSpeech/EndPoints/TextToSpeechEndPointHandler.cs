using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TextToSpeech.Interfaces;

namespace TextToSpeech.EndPointHandlers
{
    public class TextToSpeechEndPoint
    {
        public string Text { get;}
        public string Guid { get; set; }
        public TextToSpeechEndPoint(string text, string guid)
        {
            Text = text;
            Guid = guid;
        }
    }
    internal class TextToSpeechEndPointHandler : IEndPointHandler<TextToSpeechEndPoint,string>
    {
        private readonly ILogger<TextToSpeechEndPointHandler> _logger;
        public TextToSpeechEndPointHandler(ILogger<TextToSpeechEndPointHandler> logger)
        {
            _logger = logger;
        }
        public async Task<string> Handle(TextToSpeechEndPoint data)
        {
            _logger.LogInformation($"Handling TextToSpeechEndPoint with text: {data.Text}");
            try
            {
                var filename = $"{data.Guid}.wav";
                await new SpeechGenerator().GenerateAndSaveAudioAsync(data.Text, filename);
                _logger.LogInformation($"TextToSpeechEndPoint handled successfully");
                return filename;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling TextToSpeechEndPoint");
                return "";
            }
        }
    }
}
