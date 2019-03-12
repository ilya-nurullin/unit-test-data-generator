using System.IO;
using log4net;

namespace AutoUnitTestingCore
{
    public interface IInputProvider
    {
        string GetInput();
    }

    public class StringInputProvider : IInputProvider
    {
        public StringInputProvider(string input)
        {
            Input = input;
        }

        public string Input { get; }

        public string GetInput()
        {
            return Input;
        }
    }

    public class FileInputProvider : IInputProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FileInputProvider));

        public FileInputProvider(string filePath)
        {
            Input = File.OpenText(filePath).ReadToEnd();
            log.Debug($"File content (the start on next line):\n{Input}");
        }

        public string Input { get; }

        public string GetInput()
        {
            return Input;
        }
    }
}