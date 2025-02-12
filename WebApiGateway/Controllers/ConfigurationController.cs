using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Tcp;
using WebApiGateway.Configuration;
using WebApiGateway.Helpers;
using WebApiGateway.Models;
using WebApiGateway.Session;

namespace WebApiGateway.Controllers
{
    [Authorize]    
    [ApiController]
    [Route("api/[controller]")]    
    public class ConfigurationController : ControllerBase
    {
        private readonly ILogger<ConfigurationController> _logger;
        private readonly Shared.PromptConfiguration.Configuration _configuration;
        public ConfigurationController(
            ILogger<ConfigurationController> logger,
            Shared.PromptConfiguration.Configuration configuration
            )
        {
            _logger = logger;
            _configuration = configuration;
        }
        [HttpPost("GetUnits", Name = "GetUnits")]
        public ActionResult<List<Shared.PromptConfiguration.Unit>> GetUnits()
        {            
            return Ok(_configuration.Units);
        }        
    }
}
