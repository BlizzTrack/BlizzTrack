using System;

namespace Core.Services
{
    public record SeqnType(string Code, int Seqn, DateTime Indexed)
    {
        public string Name() => BNetLib.Helpers.GameName.Get(Code ?? "");
    }
}