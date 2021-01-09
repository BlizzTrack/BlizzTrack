using System.Collections.Generic;
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

        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "jsonb")]
        public List<Icons> Logos { get; set; } = new List<Icons>();

        public string Website { get; set; }

        public string ServiceURL { get; set; }

        public string GetName()
        {
            if (string.IsNullOrEmpty(Name)) return BNetLib.Helpers.GameName.Get(Code);

            return Name;
        }
    }

    public class Icons
    {
        public string Type { get; set; }
        public string URL { get; set; }
        public string OriginalName { get; set; }

        public string GetName()
        {
            if (!string.IsNullOrEmpty(OriginalName)) return OriginalName;

            return System.IO.Path.GetFileName(URL);
        }
    }

    public class ConfigItems
    {
        public bool Encrypted { get; set; } = false;
        public string EncryptedKey { get; set; } = string.Empty;
    }
}