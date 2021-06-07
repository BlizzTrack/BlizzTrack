using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface ICatalog
    {
        Task<Models.Catalog> Get(string hash);

        Task Put(Models.Catalog catalog);
    }

    public class Catalog : ICatalog
    {
        private readonly DBContext _dbContext;

        public Catalog(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Models.Catalog> Get(string hash)
        {
            return await _dbContext.Catalogs.FirstOrDefaultAsync(x => x.Hash == hash);
        }

        public async Task Put(Models.Catalog catalog)
        {
            await _dbContext.Catalogs.AddAsync(catalog);
            await _dbContext.SaveChangesAsync();
        }
    }
}
