using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlizzTrack.Models
{
    public class PatchnoteData
    {
        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public List<BNetLib.Models.Patchnotes.Overwatch.GenericUpdate> GenericUpdates { get; set; }

        public List<BNetLib.Models.Patchnotes.Overwatch.HeroUpdate> HeroUpdates { get; set; }

        public string Details { get; set; } = null;
    }
}
