namespace BNetLib.Ribbit.Commands
{
    public abstract class AbstractCommand
    {
        public abstract override string ToString();

        protected static string CreateCommand(params string[] options)
        {
            return $"v1/{string.Join("/", options)}";
        }
    }
}