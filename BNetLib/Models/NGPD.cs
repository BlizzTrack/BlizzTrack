using Newtonsoft.Json;

namespace BNetLib.Models
{
    public record CDN(string Name, string Path, string Hosts, string Servers, string ConfigPath) : NGPD
    {
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