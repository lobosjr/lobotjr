using Newtonsoft.Json;
using System;
using System.IO;

namespace LobotJR.Data
{
    public class FileDataAccess<T> : IDataAccess<T>
    {
        public bool Exists(string source)
        {
            return File.Exists(source);
        }

        public T ReadData(string source)
        {
            var content = File.ReadAllText(source);
            try
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (JsonSerializationException e)
            {
                Console.WriteLine($"Exception when deserializing data source \"{source}\" into type \"{typeof(T).Name}\": {e}");
            }
            return default(T);
        }

        public void WriteData(string source, T content)
        {
            var directoryName = Path.GetDirectoryName(source);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            File.WriteAllText(source, JsonConvert.SerializeObject(content));
        }
    }
}
