using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public interface IGameChildren
    {
        Task<List<Core.Models.GameChildren>> All();
        
        Task<Models.GameChildren> Get(string code);
        Task Add(Models.GameChildren child);
    }
    
    public class GameChildren : IGameChildren
    {
        private readonly DBContext _dbContext;

        public GameChildren(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Models.GameChildren>> All()
        {
            return await _dbContext.GameChildren
                .Include(x => x.Parent)
                .Include(x => x.GameConfig).ToListAsync();
        }

        public async Task<Core.Models.GameChildren> Get(string code)
        {
            return await _dbContext.GameChildren
                .Include(x => x.Parent)
                .Include(x => x.GameConfig)
                .FirstOrDefaultAsync(x => x.Code.ToLower() == code.ToLower() || x.Slug.ToLower() == code.ToLower());
        }

        public async Task Add(Models.GameChildren child)
        {
            await _dbContext.GameChildren.AddAsync(child);
            await _dbContext.SaveChangesAsync();
        }
    }
}