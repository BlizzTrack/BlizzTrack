using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class GameConfig
    {
        [Key]
        public string Code { get; set; }

        [Column(TypeName = "jsonb")]
        public ConfigItems Config { get; set; }
    }

    public class ConfigItems
    {
        public bool Encrypted { get; set; } = false;
    }
}
