using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Shared;
using Tcp;
using WebApiGateway.Configuration;
using WebApiGateway.Helpers;
using WebApiGateway.Models;
using WebApiGateway.Session;

namespace WebApiGateway.Helpers
{   
    public class WindowsNarratorSpeechGenerator : ISpeechGenerator
    {
        private readonly ILogger<WindowsNarratorSpeechGenerator> _logger;
        private readonly Client _tcpClient;
        private readonly ServerConfiguration _speechServerConfiguration;
        public WindowsNarratorSpeechGenerator(
            ILogger<WindowsNarratorSpeechGenerator> logger,
            Client client,
            ServerConfiguration serverConfiguration
            )
        {
            _logger = logger;
            _tcpClient = client;
            _speechServerConfiguration = serverConfiguration;            
        }                
        public async Task<Shared.TextToSpeechResponse> GenerateAsync(string text, string ssmlText, string outputFileName)
        {
            var responseFromSpeechServer = await _tcpClient.Send<Shared.TextToSpeechResponse>(new TcpRequest("TextToSpeech",
                JsonSerializer.Serialize(new Shared.Requests.TextToSpeechRequest
                {
                    Text = ssmlText,
                    Guid = outputFileName
                }
               )), _speechServerConfiguration.IpAddress, _speechServerConfiguration.Port);
            if (responseFromSpeechServer.response.IsSuccess)
                _logger.LogInformation("sent request to text to speech service for guid" + outputFileName);
            else
                _logger.LogError($"Error occurred while sending request to text to speech service for guid:{outputFileName}.{responseFromSpeechServer.response.Error}.");
            return responseFromSpeechServer.data!;
        }
    }
}
