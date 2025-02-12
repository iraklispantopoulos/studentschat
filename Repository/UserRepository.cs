using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Repositories;

namespace Repository
{
    public class UserRepository :BaseRepository<Repository.Entities.User>,IUserRepository
    {        
        public UserRepository(IDbContextFactory<AppDbContext> dbContextFactory, ILogger<UserRepository> logger) : base(dbContextFactory, logger) { }        
    }
}
