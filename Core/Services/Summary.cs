using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface ISummary
    {
        Task<Manifest<BNetLib.Models.Summary[]>> Latest();

        Task<Manifest<BNetLib.Models.Summary[]>> Previous();

        Task<List<Manifest<BNetLib.Models.Summary[]>>> Take(int amount);

        Task<Manifest<BNetLib.Models.Summary[]>> Single(int? seqn);

        Task<List<SeqnType>> Seqn();
    }

    public class Summary : ISummary
    {
        private readonly DBContext _dbContext;

        public Summary(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Manifest<BNetLib.Models.Summary[]>> Latest()
        {
            return await _dbContext.Summary.AsNoTracking().OrderByDescending(x => x.Seqn).FirstOrDefaultAsync();
        }

        public async Task<Manifest<BNetLib.Models.Summary[]>> Previous()
        {
            return await _dbContext.Summary.AsNoTracking().OrderByDescending(x => x.Seqn).Skip(1).Take(1).FirstOrDefaultAsync();
        }

        public async Task<List<SeqnType>> Seqn()
       {
           var data = await _dbContext.Summary.AsNoTracking().Select(x => new { x.Seqn, x.Indexed }).OrderByDescending(x => x.Seqn).ToListAsync();

           return data.Select(x => new SeqnType(null, x.Seqn, x.Indexed)).ToList();
       }
        
        public async Task<Manifest<BNetLib.Models.Summary[]>> Single(int? seqn)
        {
            return await _dbContext.Summary.AsNoTracking().OrderByDescending(x => x.Seqn).Where(x => (seqn == null || x.Seqn == seqn)).FirstOrDefaultAsync();
        }

        public async Task<List<Manifest<BNetLib.Models.Summary[]>>> Take(int amount)
        {
            return await _dbContext.Summary.AsNoTracking().OrderByDescending(x => x.Seqn).Take(amount).ToListAsync();
        }
    }
}
