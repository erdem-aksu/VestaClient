using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace VestaClient.Tests
{
    public class MailTests
    {
        private readonly ITestOutputHelper _output;

        private static string Uri => "https://116.203.221.31:8083/";
        
        private VestaClient VestaClient { get; }

        public MailTests(ITestOutputHelper output)
        {
            VestaClient = new VestaClient(Uri, "admin", "Ea1021+2015", false,
                Path.Combine(Directory.GetCurrentDirectory(), "Sessions"));
            _output = output;
        }

        [Fact]
        public async Task CreateMailDomain()
        {
            await VestaClient.Mail.CreateMailDomain("asdfasdasfdfqwerqewr");
        }

        [Fact]
        public async Task CreateMailAddress()
        {
            await VestaClient.Mail.CreateMailAddress("mail.direkyukle.com", "asdfasd", "123456");
        }

    }
}