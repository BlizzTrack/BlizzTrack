using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IGameParents
    {
        Task Add(Models.GameParents config);

        Task<List<Models.GameParents>> All();

        Task<Models.GameParents> Get(string code);

        Task<Models.GameParents> GetBySlug(string slug);

        Task Update(Models.GameParents config);
    }

    public class GameParents : IGameParents
    {
        private readonly DBContext _dbContext;

        public GameParents(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Models.GameParents config)
        {
            _dbContext.GameParents.Add(config);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Models.GameParents>> All()
        {
            return await _dbContext.GameParents.ToListAsync();
        }

        public async Task<Models.GameParents> Get(string code)
        {
            code = code.ToLower();
            return await _dbContext.GameParents.FirstOrDefaultAsync(x => code.StartsWith(x.Code) || code == x.Code);
        }

        public async Task<Models.GameParents> GetBySlug(string slug)
        {
            slug = slug.ToLower();
            return await _dbContext.GameParents.FirstOrDefaultAsync(x => x.Slug == slug);
        }

        public async Task Update(Models.GameParents config)
        {
            _dbContext.GameParents.Update(config);
            await _dbContext.SaveChangesAsync();
        }
    }
}