namespace BNetLib.Networking.Commands
{
    public class VersionCommand : AbstractCommand
    {
        private readonly string _code;

        public VersionCommand(string code)
        {
            _code = code;
        }

        public override string ToString()
        {
            return CreateCommand("products", _code, "versions");
        }
    }
}