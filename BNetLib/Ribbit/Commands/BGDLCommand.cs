namespace BNetLib.Ribbit.Commands
{
    public class BGDLCommand : AbstractCommand
    {
        private readonly string _code;

        public BGDLCommand(string code)
        {
            _code = code;
        }

        public override string ToString()
        {
            return CreateCommand("products", _code, "bgdl");
        }
    }
}