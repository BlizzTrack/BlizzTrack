using Newtonsoft.Json;

namespace BNetLib.Models
{
    public record CDN(string Name, string Path, string Hosts, string Servers, string ConfigPath) : NGPD
    {
        public CDN() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        public override string GetName()
        {
            return Helpers.RegionName.Get(Name);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public record Summary(string Product, int Seqn, string Flags) : NGPD
    {
        public Summary() : this(string.Empty, -1, string.Empty)
        {

        }

        public override string GetName()
        {
            return Helpers.GameName.Get(Product);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public record Versions(string Buildconfig, int Buildid, string Cdnconfig, string Keyring, string Region, string Versionsname, string Productconfig) : NGPD
    {
        public Versions() : this(string.Empty, -1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        public override string GetName()
        {
            return Helpers.RegionName.Get(Region);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public record BGDL(string Buildconfig, int Buildid, string Cdnconfig, string Keyring, string Region, string Versionsname, string Productconfig) : NGPD
    {
        public BGDL() : this(string.Empty, -1, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        public override string GetName()
        {
            return Helpers.RegionName.Get(Region);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public abstract record NGPD
    {
        public abstract string GetName();
        public override abstract string ToString();
    }
}