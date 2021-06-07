namespace BNetLib.Ribbit.Models
{
    public record CDN(string Name, string Path, string Hosts, string Servers, string ConfigPath) : NGPD
    {
        public CDN() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) { }
        
        public override string GetName()
        {
            return Helpers.RegionName.Get(Name);
        }
    }
}