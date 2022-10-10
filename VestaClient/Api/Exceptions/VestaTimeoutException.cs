using System;

namespace VestaClient.Api.Exceptions
{
    public class VestaTimeoutException : VestaException
    {
        public VestaTimeoutException()
        {
            HttpStatusCode = 408;
        }

        public VestaTimeoutException(string message)
            : base(message)
        {
            HttpStatusCode = 408;
        }

        public VestaTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
            HttpStatusCode = 408;
        }
    }
}
