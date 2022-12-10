using System.IO;
using System.Text;

namespace LobotJR.Data.Import
{
    /// <summary>
    /// Implementation of the default System.IO File methods
    /// </summary>
    public class FileSystem : IFileSystem
    {
        public void AppendAllText(string path, string contents)
        {
            File.AppendAllText(path, contents);
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void Move(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        public string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path, UTF8Encoding.Default);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, UTF8Encoding.Default);
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
    }
}
