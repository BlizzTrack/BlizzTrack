using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface ICatalog
    {
        Task<Models.Catalog> Get(string hash);
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
    }
}
