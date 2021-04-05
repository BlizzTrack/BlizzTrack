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
        public List<Icons> Logos { get; set; } = new();

        public string Website { get; set; }

        public string ServiceURL { get; set; }
        
        public GameChildren Owner { get; set; }

        public string GetName()
        {
            return string.IsNullOrEmpty(Name) ? BNetLib.Helpers.GameName.Get(Code) : Name;
        }
    }

    public class Icons
    {
        public string Type { get; set; }
        public string URL { get; set; }
        public string OriginalName { get; set; }

        public string GetName()
        {
            return !string.IsNullOrEmpty(OriginalName) ? OriginalName : System.IO.Path.GetFileName(URL);
        }
    }

    public class ConfigItems
    {
        public bool Encrypted { get; set; }
        public string EncryptedKey { get; set; }

        public ConfigItems()
        {
            EncryptedKey = string.Empty;
        }
        
        public ConfigItems(bool encrypted, string encryptedKey)
        {
            Encrypted = encrypted;
            EncryptedKey = encryptedKey;
        }
    }
}