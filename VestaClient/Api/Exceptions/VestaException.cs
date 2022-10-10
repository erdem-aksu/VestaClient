using System;
using VestaClient.Api.Responses;

namespace VestaClient.Api.Exceptions
{
    public class VestaException : Exception
    {
        public int? HttpStatusCode { get; internal set; }

        public string RequestUrl { get; internal set; }

        public string ResponseContent { get; internal set; }

        public ErrorResponse ErrorResponse { get; internal set; }

        public VestaException()
        {
        }

        public VestaException(string message)
            : base(message)
        {
        }

        public VestaException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal static T Create<T>(string message, string requestUrl, string responseContent,
            ErrorResponse errorResponse = null, int? httpStatusCode = null)
            where T : VestaException
        {
            var exception = (T) Activator.CreateInstance(typeof(T), message);
            exception.RequestUrl = requestUrl;
            exception.ResponseContent = responseContent;
            exception.HttpStatusCode = httpStatusCode;
            exception.ErrorResponse = errorResponse;
            return exception;
        }
    }
}