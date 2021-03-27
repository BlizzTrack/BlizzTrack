using System.Threading.Tasks;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public interface IGameChildren
    {
        Task<Models.GameChildren> Get(string code);
    }
    
    public class GameChildren : IGameChildren
    {
        private readonly DBContext _dbContext;

        public GameChildren(DBContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<Core.Models.GameChildren> Get(string code)
        {
            return await _dbContext.GameChildren
                .Include(x => x.Parent)
                .Include(x => x.GameConfig)
                .FirstOrDefaultAsync(x => x.Code.ToLower() == code.ToLower() || x.Slug.ToLower() == code.ToLower());
        }
    }
}