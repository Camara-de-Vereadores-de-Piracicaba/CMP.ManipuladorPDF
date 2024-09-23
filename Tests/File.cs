using System.Reflection;

namespace Tests
{
    public class FileManager
    {
        public static MemoryStream GetFile(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream($"Tests.Resources.{name}");
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
