namespace BNetLib.Ribbit.Models
{
    public record Summary(string Product, int Seqn, string Flags) : NGPD
    {
        public Summary() : this(string.Empty, -1, string.Empty)
        {

        }

        public override string GetName()
        {
            return Helpers.GameName.Get(Product);
        }
    }
}