﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GameChildren = Core.Models.GameChildren;

namespace BlizzMeta.Pages
{
    public class ViewGameSeqnList : PageModel
    {
        private readonly IGameChildren _gameChildren;
        private readonly IVersions _versions;
        private readonly ICDNs _cdns;
        private readonly IBGDL _bgdl;

        public ViewGameSeqnList(IGameChildren gameChildren, IVersions versions, ICDNs cdns, IBGDL bgdl)
        {
            _gameChildren = gameChildren;
            _versions = versions;
            _cdns = cdns;
            _bgdl = bgdl;
        }
        
        [BindProperty(SupportsGet = true, Name = "code")]
        public string Code { get; set; }
        
        [BindProperty(SupportsGet = true, Name = "file")]
        public string File { get; set; }
        
        [BindProperty(SupportsGet = true, Name = "seqn")]
        public int? Seqn { get; set; }

        [BindProperty(SupportsGet = true, Name = "view")]
        public string View { get; set; } = "full";

        public GameChildren Children;

        public List<SeqnType> Seqns;

        public new object Content;
        
        public async Task<IActionResult> OnGetAsync()
        {
            Seqns = File.ToLower() switch
            {
                "versions" or "version" => await _versions.SeqnList(Code),
                "cdns" or "cdn" => await _cdns.Seqn(Code),
                "bgdl" => await _bgdl.SeqnList(Code),
                _ => null
            };
            if (Seqns == null) return NotFound();

            Seqn ??= Seqns.First().Seqn;
            
            Content = File.ToLower() switch
            {
                "versions" or "version" => await _versions.Single(Code, Seqn),
                "cdns" or "cdn" => await _cdns.Single(Code, Seqn),
                "bgdl" => await _bgdl.Single(Code, Seqn),
                _ => null
            };
            if (Content == null) return NotFound();
            
            Children = await _gameChildren.Get(Code);
            if (Children == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}