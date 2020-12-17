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

        [NotMapped]
        public string Name => BNetLib.Helpers.GameName.Get(Code);

        public Manifest() { }

        public static Manifest<T> Create(int seqn, string code, T content)
        {
            return new Manifest<T>()
            {
                Seqn = seqn,
                Code = code,
                Content = content
            };
        }
    }
}
