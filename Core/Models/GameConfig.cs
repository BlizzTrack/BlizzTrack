using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string EncryptedKey { get; set; } = string.Empty;
    }
}
