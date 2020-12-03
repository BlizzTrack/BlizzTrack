﻿using BNetLib.Models;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface ICDNs
    {
        Task<Manifest<CDN[]>> Latest(string code);

        Task<Manifest<CDN[]>> Single(string code, int? seqn);

        Task<List<SeqnType>> Seqn(string code);
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
            return await _dbContext.CDN.AsNoTracking().OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).FirstOrDefaultAsync();
        }

        public async Task<List<SeqnType>> Seqn(string code)
        {
            var data = await _dbContext.CDN.AsNoTracking().Select(x => new { x.Seqn, x.Code, x.Indexed }).OrderByDescending(x => x.Seqn).Where(x => x.Code.ToLower() == code.ToLower()).ToListAsync();

            return data.Select(x => new SeqnType(x.Code, x.Seqn, x.Indexed)).ToList();
        }

        public async Task<Manifest<CDN[]>> Single(string code, int? seqn)
        {
            return await _dbContext.CDN.OrderByDescending(x => x.Seqn).Where(x => (seqn == null || x.Seqn == seqn) && x.Code.ToLower() == code.ToLower()).FirstOrDefaultAsync();
        }
    }
}