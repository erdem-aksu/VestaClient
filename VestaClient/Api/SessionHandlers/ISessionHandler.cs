using VestaClient.Http;

namespace VestaClient.Api.SessionHandlers
{
    public interface ISessionHandler
    {
        void Load();

        void Save();
    }
}
