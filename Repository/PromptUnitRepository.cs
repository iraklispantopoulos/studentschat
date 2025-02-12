using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Repositories;
namespace Repository
{
    public class PromptUnitRepository :BaseRepository<Repository.Entities.PromptUnit>, IPromptUnitRepository
    {       
        public PromptUnitRepository(IDbContextFactory<AppDbContext> dbContextFactory, ILogger<PromptUnitRepository> logger) : base(dbContextFactory, logger) { }        
    }
}
