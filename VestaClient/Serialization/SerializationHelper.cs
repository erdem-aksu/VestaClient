using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace VestaClient.Serialization
{
    internal class SerializationHelper
    {
        public static Stream SerializeToStream(object o)
        {
            var stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            stream.Position = 0;
            return stream;
        }

        public static T DeserializeFromStream<T>(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            return (T) formatter.Deserialize(stream);
        }

        public static string SerializeToString(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static T DeserializeFromString<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}