using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Core.Models
{
    public enum CatalogType
    {
        Config,
        Fragment,
        ProductConfig
    }

    public class Catalog
    {
        [Key]
        public string Hash { get; set; }

        public string Name { get; set; }

        public string ProperName { get; set; }

        public CatalogType Type { get; set; }

        public bool Activision { get; set; }

        public DateTime Indexed { get; set; }

        public JsonDocument Payload { get; set; }

        [Column(TypeName = "jsonb")]
        public List<CatalogInstall> Installs { get; set; }

        [Column(TypeName = "jsonb")]
        public Dictionary<string, string> Translations { get; set; }
    }

    public class CatalogInstall
    {
        public string Name { get; set; }

        public string Code { get; set; }
    }
}
