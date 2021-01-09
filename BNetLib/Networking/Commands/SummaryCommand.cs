namespace BNetLib.Networking.Commands
{
    public class SummaryCommand : AbstractCommand
    {
        public override string ToString()
        {
            return CreateCommand("summary");
        }
    }
}