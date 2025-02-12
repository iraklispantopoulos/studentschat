using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Repository.Entities;
using Tcp;
using WebApiGateway.Configuration;
using WebApiGateway.Helpers;
using WebApiGateway.Models;
using WebApiGateway.Session;
using static System.Net.Mime.MediaTypeNames;

namespace WebApiGateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TextToSpeechController : ControllerBase
    {
        private readonly ILogger<TextToSpeechController> _logger;
        private readonly SpeechGeneratorFactory _speechGeneratorFactory;
        private readonly ServerConfiguration _speechServerConfiguration;
        private readonly AppConfig _appConfig;
        private readonly ChatFactory _chatFactory;
        private readonly WebApiGateway.Session.User _sessionUser;
        private readonly Shared.PromptConfiguration.Configuration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IChatHistoryRepository _chatHistoryRepository;
        public TextToSpeechController(
            ILogger<TextToSpeechController> logger,
            SpeechGeneratorFactory speechGeneratorFactory,
            ServerConfiguration serverConfiguration,
            AppConfig appConfig,
            ChatFactory chatFactory,
            Shared.PromptConfiguration.Configuration configuration,
            IUserRepository userRepository,
            IChatHistoryRepository chatHistoryRepository,
            WebApiGateway.Session.User sessionUser
            )
        {
            _logger = logger;
            _speechGeneratorFactory = speechGeneratorFactory;
            _speechServerConfiguration = serverConfiguration;
            _appConfig = appConfig;
            _chatFactory = chatFactory;
            _sessionUser = sessionUser;
            _configuration = configuration;
            _userRepository = userRepository;
            _chatHistoryRepository = chatHistoryRepository;
        }
        [HttpPost("Start", Name = "Start")]
        public async Task<ActionResult<AgentResponse>> Start(StartRequest startRequest)
        {
            _logger.LogInformation("Starting text to speech service");
            var startPrompt = await _chatFactory.GetChat(_sessionUser.GetId()).Start(!startRequest.NewConversation);
            await SaveResponse(false, startPrompt.ClearTextFormat);
            var fileName = Guid.NewGuid().ToString();
            return await GenerateResponse(startPrompt.ClearTextFormat, startPrompt.SsmlFormat, fileName);
        }
        [HttpPost("GetLastAgentResponse", Name = "GetLastAgentResponse")]
        public async Task<ActionResult<AgentResponse>> GetLastAgentResponse()
        {
            _logger.LogInformation("Getting last agent response");
            var lastAgentPrompt=await _chatFactory.GetChat(_sessionUser.GetId()).GetLastAgentPrompt();
            if(lastAgentPrompt== null)
            {
                return NotFound();
            }
            return Ok(lastAgentPrompt);
        }

        [HttpPost("ProcessResponse", Name = "ProcessResponse")]
        public async Task<ActionResult<AgentResponse>> ProcessResponse(Models.TextToSpeechRequest request)
        {
            _logger.LogInformation($"Generating agent response for text: {request.Text}");
            var fileName = Guid.NewGuid().ToString();
            var userId = _sessionUser.GetId();
            var agentResponse = await _chatFactory.GetChat(userId).ProcessUserResponse(request.Text);
            if (agentResponse.PromptId != null)
            {
                await _userRepository.Update(userId, p =>
                {
                    p.CurrentPromptId = agentResponse.PromptId;
                    p.CurrentUnitId = agentResponse.UnitId;
                });
            }
            await SaveResponse(true, request.Text);
            await SaveResponse(false, agentResponse.ClearTextFormat);
            _logger.LogInformation($"got response from chat: {agentResponse.ClearTextFormat}");
            return await GenerateResponse(agentResponse.ClearTextFormat, agentResponse.SsmlFormat, fileName);
        }
        private async Task<ActionResult<AgentResponse>> GenerateResponse(string clearText, string ssml, string fileNameNoExt)
        {
            var speechResult = await GenerateSpeech(clearText, ssml, fileNameNoExt);
            var lipGenerationResult = GenerateLipSyncData(fileNameNoExt, speechResult.Filename);
            if (string.IsNullOrEmpty(lipGenerationResult.filename))
            {
                return StatusCode(500, $"Error: {lipGenerationResult.error}");
            }
            else
                return Ok(new AgentResponse()
                {
                    SpeechFilename = speechResult.Filename,
                    LipSyncFilename = lipGenerationResult.filename
                });
        }
        private async Task<Shared.TextToSpeechResponse> GenerateSpeech(string text, string ssmlText, string outputFileName)
        {
            return await (await _speechGeneratorFactory.GetSpeechGenerator()).GenerateAsync(text, ssmlText, outputFileName);
        }
        private (string filename, string error) GenerateLipSyncData(string outputFileNameNoExtension, string speechAudioFileName)
        {
            string documentsPath = _appConfig.LipSyncOutputDir;
            string outputFileName = $"{outputFileNameNoExtension}.json";
            string audioFileName = speechAudioFileName;

            string outputFilePath = Path.Combine(documentsPath, outputFileName);
            string audioFilePath = Path.Combine(documentsPath, audioFileName);
            _logger.LogInformation($"Generating lip sync for audio file: {audioFilePath} with destination file {outputFileName}");
            var processInfo = new ProcessStartInfo
            {
                FileName = "C:\\Rhubarb-Lip-Sync-1.13.0\\rhubarb", // Assuming rhubarb is in PATH, otherwise provide the full path
                Arguments = $"-f json -r phonetic -o \"{outputFilePath}\" \"{audioFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false, // Required for redirection
                CreateNoWindow = true // Avoid showing a console window
            };

            try
            {
                using (var process = Process.Start(processInfo))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        return ("", error);
                    }

                    return (outputFileName, "");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating lip sync");
                return ("", ex.Message);
            }
        }
        private async Task SaveResponse(bool user, string message)
        {
            await _chatHistoryRepository.Save(new ChatHistory()
            {
                UserId = _sessionUser.GetId(),
                Date = DateTime.UtcNow,
                Guid = Guid.NewGuid().ToString(),
                PromptType = user ? Shared.Enums.PromptTypeEnum.User : Shared.Enums.PromptTypeEnum.Agent,
                Text = message
            });
        }
    }
}
