using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IBGDL
    {
        Task<Manifest<BNetLib.Ribbit.Models.BGDL[]>> Latest(string code);

        Task<Manifest<BNetLib.Ribbit.Models.BGDL[]>> Previous(string code);

        Task<List<Manifest<BNetLib.Ribbit.Models.BGDL[]>>> Take(string code, int amount);

        Task<Manifest<BNetLib.Ribbit.Models.BGDL[]>> Single(string code, int? seqn);

        Task<List<Manifest<BNetLib.Ribbit.Models.BGDL[]>>> MultiBySeqn(List<int> seqns);

        Task<List<SeqnType>> SeqnList(string code);
    }

    public class BGDL : IBGDL
    {
        private readonly DBContext _dbContext;

        public BGDL(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Manifest<BNetLib.Ribbit.Models.BGDL[]>> Latest(string code)
        {
            return await _dbContext.BGDL.AsNoTracking().Include(x => x.Config).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<Manifest<BNetLib.Ribbit.Models.BGDL[]>>> MultiBySeqn(List<int> seqns)
        {
            return await _dbContext.BGDL.AsNoTracking().Include(x => x.Config).Where(x => seqns.Contains(x.Seqn)).ToListAsync();
        }

        public async Task<Manifest<BNetLib.Ribbit.Models.BGDL[]>> Previous(string code)
        {
            return await _dbContext.BGDL.AsNoTracking().Include(x => x.Config).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).Skip(1).Take(1).FirstOrDefaultAsync();
        }

        public async Task<List<SeqnType>> SeqnList(string code)
        {
            var data = await _dbContext.BGDL.AsNoTracking().Select(x => new { x.Seqn, x.Code, x.Indexed }).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).ToListAsync();

            return data.Select(x => new SeqnType(x.Code, x.Seqn, x.Indexed)).ToList();
        }

        public async Task<Manifest<BNetLib.Ribbit.Models.BGDL[]>> Single(string code, int? seqn)
        {
            return await _dbContext.BGDL.AsNoTracking().Include(x => x.Config).OrderByDescending(x => x.Seqn).Where(x => (seqn == null || x.Seqn == seqn) && x.Code.ToLower() == code.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<Manifest<BNetLib.Ribbit.Models.BGDL[]>>> Take(string code, int amount)
        {
            return await _dbContext.BGDL.AsNoTracking().Include(x => x.Config).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).Take(amount).ToListAsync();
        }
    }
}