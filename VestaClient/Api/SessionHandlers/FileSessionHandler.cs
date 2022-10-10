using System.IO;
using VestaClient.Http;

namespace VestaClient.Api.SessionHandlers
{
    internal class FileSessionHandler : ISessionHandler
    {
        private VestaHttpClient HttpClient { get; }

        private string FilePath { get; }

        public FileSessionHandler(VestaHttpClient httpClient, string filePath)
        {
            HttpClient = httpClient;
            FilePath = filePath;
        }

        public void Load()
        {
            if (!File.Exists(FilePath)) return;

            using (var fs = File.OpenRead(FilePath))
            {
                HttpClient.LoadStateDataFromStream(fs);
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(FilePath)) return;

            using (var state = HttpClient.GetStateDataAsStream())
            {
                using (var fileStream = File.Create(FilePath))
                {
                    state.Seek(0, SeekOrigin.Begin);
                    state.CopyTo(fileStream);
                }
            }
        }
    }
}