using Newtonsoft.Json;

namespace BNetLib.Ribbit.Models
{
    public abstract record NGPD
    {
        public abstract string GetName();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}