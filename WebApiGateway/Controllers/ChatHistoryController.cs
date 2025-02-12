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
    public class ChatHistoryController : ControllerBase
    {
        private readonly ILogger<ConfigurationController> _logger;
        private readonly IChatHistoryRepository _chatHistoryRepository;
        public ChatHistoryController(
            ILogger<ConfigurationController> logger,
            IChatHistoryRepository chatHistoryRepository
            )
        {
            _logger = logger;
            _chatHistoryRepository = chatHistoryRepository;
        }
        [HttpPost("GetChat", Name = "GetChat")]
        public async Task<ActionResult<ChatSession>> GetChat(int userId, DateTime date,int page)
        {
            var pageSize = 10;
            var startOfDay = date.Date;
            var endOfDay = date.Date.AddDays(1).AddTicks(-1);
            var chatHistory = await _chatHistoryRepository.GetAllWithCriteria(p => p.UserId == userId && p.Date >= startOfDay && p.Date <= endOfDay,new Repository.Repositories.PageInfo()
            {
                Page = page,
                PageSize = pageSize
            });
            return Ok(new ChatSession()
            {
                Records = chatHistory
                            .Select(p => new ChatRecord()
                            {
                                Date = p.Date,
                                Message = p.Text,
                                PromptType = p.PromptType
                            })
                            .ToList()
            });
        }
    }
}
