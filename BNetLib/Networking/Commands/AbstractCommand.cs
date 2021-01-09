namespace BNetLib.Networking.Commands
{
    public abstract class AbstractCommand
    {
        public abstract override string ToString();

        public static string CreateCommand(params string[] options)
        {
            return $"v1/{string.Join("/", options)}";
        }
    }
}