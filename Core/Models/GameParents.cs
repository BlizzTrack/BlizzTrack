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

        public List<string> ChildrenOverride { get; set; }


        [Column(TypeName = "jsonb")]
        public List<Icons> Logos { get; set; } = new List<Icons>();
    }
}
