namespace BNetLib.Ribbit.Commands
{
    public class CDNCommand : AbstractCommand
    {
        private readonly string _code;

        public CDNCommand(string code)
        {
            _code = code;
        }

        public override string ToString()
        {
            return CreateCommand("products", _code, "cdns");
        }
    }
}