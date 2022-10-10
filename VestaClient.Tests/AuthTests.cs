using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace VestaClient.Tests
{
    public class AuthTests
    {
        private readonly ITestOutputHelper _output;

        private static string Uri => "https://49.12.36.129:8083/";
        
        private VestaClient VestaClient { get; }

        public AuthTests(ITestOutputHelper output)
        {
            VestaClient = new VestaClient(Uri, "admin", "Ea1021+2015", false,
                Path.Combine(Directory.GetCurrentDirectory(), "Sessions"));
            _output = output;
        }

        [Fact]
        public async Task Login()
        {
            await VestaClient.Auth.Login();
        }

        [Fact]
        public void AutoLogin()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Sessions");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            VestaClient.Auth.Login();
            VestaClient.Auth.Login();
        }
    }
}