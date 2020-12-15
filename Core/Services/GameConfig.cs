using Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IGameConfig
    {
        Task<Models.GameConfig> Get(string code);

        Task<List<Models.GameConfig>> In(string[] codes);
    }
    public class GameConfig : IGameConfig
    {
        private readonly DBContext _dbContext;
        public GameConfig(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Models.GameConfig> Get(string code)
        {
            return await _dbContext.GameConfigs.FirstOrDefaultAsync(x => x.Code.ToLower() == code.ToLower());
        }

        public async Task<List<Models.GameConfig>> In(string[] codes)
        {
            return await _dbContext.GameConfigs.Where(x => codes.Contains(x.Code)).ToListAsync();
        }
    }
}
