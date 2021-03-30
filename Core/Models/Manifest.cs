using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public record Manifest<T> where T : System.Collections.IEnumerable
    {
        public int Seqn { get; set; }

        public string Code { get; set; }

        [Column(TypeName = "jsonb")]
        public T Content { get; set; }

        public DateTime Indexed { get; set; }

        public string Raw { get; set; }

        public int Parent { get; set; }
        
        public string ConfigId { get; set; }
        
        [ForeignKey("ConfigId ")]
        public GameConfig Config { get; set; }

        [NotMapped]
        public string Name => BNetLib.Helpers.GameName.Get(Code);

        public Manifest() { }

        public static Manifest<T> Create(int seqn, string code, T content)
        {
            return new()
            {
                Seqn = seqn,
                Code = code,
                Content = content
            };
        }
    }
}