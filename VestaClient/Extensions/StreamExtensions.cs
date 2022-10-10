using System.IO;

namespace VestaClient.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadAllBytes(this Stream sourceStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                sourceStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}