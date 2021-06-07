namespace BNetLib.Ribbit.Models
{
    public record BGDL(string Buildconfig, int Buildid, string Cdnconfig, string Keyring, string Region, string Versionsname, string Productconfig) : NGPD
    {
        public BGDL() : this(string.Empty, -1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        public override string GetName()
        {
            return Helpers.RegionName.Get(Region);
        }
    }
}