using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Core.Models
{
    public class Assets
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string url { get; set; }
        
        [Column(TypeName = "jsonb")]
        public JsonDocument  Metadata { get; set; }
    }
}