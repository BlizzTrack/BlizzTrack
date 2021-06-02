using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public interface IGameCompanies
    {
        Task<List<GameCompany>> All();
    }
    
    public class GameCompanies : IGameCompanies
    {
        private readonly DBContext _dbContext;

        public GameCompanies(DBContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<List<GameCompany>> All()
        {
            return await _dbContext.GameCompanies
                .Include(x => x.Parents)
                .ThenInclude(x => x.Children)
                .ThenInclude(x => x.GameConfig)
                .AsSplitQuery()
                .ToListAsync();
        }
    }
}