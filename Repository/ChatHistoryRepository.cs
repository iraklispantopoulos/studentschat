using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Repositories;

namespace Repository
{
    public class ChatHistoryRepository :BaseRepository<Repository.Entities.ChatHistory>,IChatHistoryRepository
    {       
        public ChatHistoryRepository(IDbContextFactory<AppDbContext> dbContextFactory, ILogger<ChatHistoryRepository> logger) : base(dbContextFactory, logger) { }        
    }
}
