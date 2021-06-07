using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface ISummary
    {
        Task<Manifest<BNetLib.Ribbit.Models.Summary[]>> Latest();

        Task<Manifest<BNetLib.Ribbit.Models.Summary[]>> Previous();

        Task<List<Manifest<BNetLib.Ribbit.Models.Summary[]>>> Take(int amount);

        Task<Manifest<BNetLib.Ribbit.Models.Summary[]>> Single(int? seqn);

        Task<List<SeqnType>> SeqnList();
    }

    public class Summary : ISummary
    {
        private readonly DBContext _dbContext;

        public Summary(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Manifest<BNetLib.Ribbit.Models.Summary[]>> Latest()
        {
            return await _dbContext.Summary.AsNoTracking()
                .OrderByDescending(x => x.Seqn)
                .FirstOrDefaultAsync();
        }

        public async Task<Manifest<BNetLib.Ribbit.Models.Summary[]>> Previous()
        {
            return await _dbContext.Summary.AsNoTracking()
                .OrderByDescending(x => x.Seqn)
                .Skip(1).FirstOrDefaultAsync();
        }

        public async Task<List<SeqnType>> SeqnList()
        {
            var data = await _dbContext.Summary.AsNoTracking()
                .Select(x => new { x.Seqn, x.Indexed })
                .OrderByDescending(x => x.Seqn)
                .ToListAsync();

            return data.Select(x => new SeqnType(null, x.Seqn, x.Indexed)).ToList();
        }

        public async Task<Manifest<BNetLib.Ribbit.Models.Summary[]>> Single(int? seqn)
        {
            return await _dbContext.Summary.AsNoTracking()
                .OrderByDescending(x => x.Seqn)
                .Where(x => seqn == null || x.Seqn == seqn)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Manifest<BNetLib.Ribbit.Models.Summary[]>>> Take(int amount)
        {
            return await _dbContext.Summary.AsNoTracking()
                .OrderByDescending(x => x.Seqn)
                .Take(amount).ToListAsync();
        }
    }
}