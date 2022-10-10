using System;

namespace VestaClient.Api.Exceptions
{
    public class VestaRateLimitExceededException : VestaException
    {
        public VestaRateLimitExceededException()
        {
            HttpStatusCode = 429;
        }

        public VestaRateLimitExceededException(string message)
            : base(message)
        {
            HttpStatusCode = 429;
        }

        public VestaRateLimitExceededException(string message, Exception inner)
            : base(message, inner)
        {
            HttpStatusCode = 429;
        }
    }
}
