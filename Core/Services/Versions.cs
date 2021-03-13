﻿using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IVersions
    {
        Task<Manifest<BNetLib.Models.Versions[]>> Latest(string code);

        Task<Manifest<BNetLib.Models.Versions[]>> Previous(string code);

        Task<List<Manifest<BNetLib.Models.Versions[]>>> Take(string code, int amount);

        Task<Manifest<BNetLib.Models.Versions[]>> Single(string code, int? seqn);

        Task<List<Manifest<BNetLib.Models.Versions[]>>> MultiBySeqn(List<int> seqns);

        Task<List<SeqnType>> Seqn(string code);
    }

    public class Versions : IVersions
    {
        private readonly DBContext _dbContext;

        public Versions(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Manifest<BNetLib.Models.Versions[]>> Latest(string code)
        {
            return await _dbContext.Versions.AsNoTracking().OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<Manifest<BNetLib.Models.Versions[]>>> MultiBySeqn(List<int> seqns)
        {
            return await _dbContext.Versions.AsNoTracking().Where(x => seqns.Contains(x.Seqn)).ToListAsync();
        }

        public async Task<Manifest<BNetLib.Models.Versions[]>> Previous(string code)
        {
            return await _dbContext.Versions.AsNoTracking().OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).Skip(1).Take(1).FirstOrDefaultAsync();
        }

        public async Task<List<SeqnType>> Seqn(string code)
        {
            var data = await _dbContext.Versions.AsNoTracking().Select(x => new { x.Seqn, x.Code, x.Indexed }).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).ToListAsync();

            return data.Select(x => new SeqnType(x.Code, x.Seqn, x.Indexed)).ToList();
        }

        public async Task<Manifest<BNetLib.Models.Versions[]>> Single(string code, int? seqn)
        {
            return await _dbContext.Versions.AsNoTracking().OrderByDescending(x => x.Seqn).Where(x => (seqn == null || x.Seqn == seqn) && x.Code.ToLower() == code.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<Manifest<BNetLib.Models.Versions[]>>> Take(string code, int amount)
        {
            return await _dbContext.Versions.AsNoTracking().OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).Take(amount).ToListAsync();
        }
    }
}