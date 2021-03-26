using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class GameParents
    {
        [Key]
        [Required]
        public string Code { get; set; }

        [Required]
        public string Name { get; set; }

        public string Slug { get; set; }

        public string Website { get; set; }

        public List<string> ChildrenOverride { get; set; }

        public List<string> PatchNoteAreas { get; set; }

        public string PatchNoteTool { get; set; }

        public string PatchNoteCode { get; set; }

        public string ManifestID { get; set; }

        public bool? Visible { get; set; }

        [Column(TypeName = "jsonb")]
        public List<Icons> Logos { get; set; } = new();
        
        public string About { get; set; }
        
        public ICollection<GameChildren> Children { get; set; }

        public GameCompany Owner { get; set; }
    }
}