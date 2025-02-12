using Google.Cloud.TextToSpeech.V1;
using Grpc.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Protobuf;
using Shared;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Shared.Enums;

namespace GoogleTextToSpeech;
public class SpeechGenerator : ISpeechGenerator
{
    private static GoogleCredential? _credentials=null;
    private readonly ILogger _logger;
    private readonly string _outputDirectoryPath = "";
    private readonly string _jsonKeyPath = "";
    private readonly AvatarGenderTypeEnum _avatarGenderType;
    private SsmlVoiceGender Gender
    {
        get {
            return _avatarGenderType == AvatarGenderTypeEnum.Female ? SsmlVoiceGender.Female : SsmlVoiceGender.Male;
        }
    } 
    public SpeechGenerator(ILogger<SpeechGenerator> logger,string outputDirectoryPath,string jsonKeyPath,AvatarGenderTypeEnum? avatarGenderType)
    {
        _logger = logger;
        _outputDirectoryPath = outputDirectoryPath;
        _jsonKeyPath = jsonKeyPath;
        _avatarGenderType = avatarGenderType??AvatarGenderTypeEnum.Male;
    }
    public Task<Shared.TextToSpeechResponse> GenerateAsync(string text, string ssmlText, string outputFileName)
    {
        try
        {
            if(_credentials == null)
            {
                _logger.LogInformation("Loading credentials");
                _credentials = GoogleCredential.FromFile(_jsonKeyPath)
                                              .CreateScoped(TextToSpeechClient.DefaultScopes);
            }            

            // Create the Text-to-Speech client using the _credentials
            var client = new TextToSpeechClientBuilder
            {
                ChannelCredentials = _credentials.ToChannelCredentials()
            }.Build();

            // Build the synthesis input
            var input = new SynthesisInput
            {
                Text = text
            };

            // Configure the voice request
            var voice = new VoiceSelectionParams
            {
                LanguageCode = "el-GR",
                SsmlGender = Gender                
            };

            // Set audio configuration
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // Perform the text-to-speech request
            var response = client.SynthesizeSpeech(input, voice, audioConfig);
            var mp3Filename = $"{outputFileName}.mp3";
            var wavFilename = $"{outputFileName}.wav";
            // Save the audio to a outputMp3File
            var outputMp3File = Path.Combine(_outputDirectoryPath, mp3Filename);
            var outputWavFile = Path.Combine(_outputDirectoryPath, wavFilename);
            System.IO.File.WriteAllBytes(outputMp3File, response.ToByteArray());
            ConvertMp3ToWav(outputMp3File, outputWavFile);
            _logger.LogInformation($"Audio content written to file '{outputFileName}'");
            return Task.FromResult(new TextToSpeechResponse()
            {
                Filename = wavFilename
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating speech");
            return Task.FromResult(new TextToSpeechResponse()
            {
                Filename = ""
            });
        }
    }
    public void ConvertMp3ToWav(string mp3FilePath, string wavFilePath)
    {
        using var mp3Reader = new Mp3FileReader(mp3FilePath);
        using var waveStream = WaveFormatConversionStream.CreatePcmStream(mp3Reader);
        WaveFileWriter.CreateWaveFile(wavFilePath, waveStream);
    }
}
