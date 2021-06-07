using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BNetLib.Ribbit.Models;

namespace Core.Services
{
    public interface ICDNs
    {
        Task<Manifest<CDN[]>> Latest(string code);

        Task<Manifest<CDN[]>> Previous(string code);

        Task<List<Manifest<CDN[]>>> Take(string code, int amount);

        Task<Manifest<CDN[]>> Single(string code, int? seqn);

        Task<List<SeqnType>> Seqn(string code);
        
        Task<List<Manifest<CDN[]>>> MultiBySeqn(List<int> toList);
    }

    public class CDNs : ICDNs
    {
        private readonly DBContext _dbContext;

        public CDNs(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Manifest<CDN[]>> Latest(string code)
        {
            return await _dbContext.CDN.AsNoTracking().Include(x => x.Config).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<Manifest<CDN[]>> Previous(string code)
        {
            return await _dbContext.CDN.AsNoTracking().Include(x => x.Config).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).Skip(1).Take(1).FirstOrDefaultAsync();
        }

        public async Task<List<SeqnType>> Seqn(string code)
        {
            var data = await _dbContext.CDN.AsNoTracking().Select(x => new { x.Seqn, x.Code, x.Indexed }).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).ToListAsync();

            return data.Select(x => new SeqnType(x.Code, x.Seqn, x.Indexed)).ToList();
        }

        public async Task<List<Manifest<CDN[]>>> MultiBySeqn(List<int> seqns)
        {
            return await _dbContext.CDN.AsNoTracking().Include(x => x.Config).Where(x => seqns.Contains(x.Seqn)).ToListAsync();
        }

        public async Task<Manifest<CDN[]>> Single(string code, int? seqn)
        {
            return await _dbContext.CDN.AsNoTracking().Include(x => x.Config).OrderByDescending(x => x.Seqn).Where(x => (seqn == null || x.Seqn == seqn) && x.Code.ToLower() == code.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<Manifest<CDN[]>>> Take(string code, int amount)
        {
            return await _dbContext.CDN.AsNoTracking().Include(x => x.Config).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).Take(amount).ToListAsync();
        }
    }
}