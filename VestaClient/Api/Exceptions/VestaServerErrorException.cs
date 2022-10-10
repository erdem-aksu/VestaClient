using System;

namespace VestaClient.Api.Exceptions
{
    public class VestaServerErrorException : VestaException
    {
        public VestaServerErrorException()
        {

        }

        public VestaServerErrorException(string message)
            : base(message)
        {

        }

        public VestaServerErrorException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
