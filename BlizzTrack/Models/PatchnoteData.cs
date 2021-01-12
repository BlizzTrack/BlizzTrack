using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BlizzTrack.Models
{

    public class PatchNoteBuild
    {
        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }

    public class PatchNoteData
    {
        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public List<BNetLib.Models.Patchnotes.Overwatch.GenericUpdate> GenericUpdates { get; set; }

        public List<BNetLib.Models.Patchnotes.Overwatch.HeroUpdate> HeroUpdates { get; set; }

        public string Details { get; set; } = null;

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { 
                WriteIndented = true, 
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull 
            });
        }
    }
}
