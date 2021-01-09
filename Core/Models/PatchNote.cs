using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class PatchNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public string Code { get; set; }

        public string Type { get; set; }

        public System.Text.Json.JsonDocument Body { get; set; }

        public static PatchNote Create(string body)
        {
            return new PatchNote()
            {
                Body = System.Text.Json.JsonDocument.Parse(body)
            };
        }
    }
}