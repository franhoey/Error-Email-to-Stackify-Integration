using System;

namespace EmailToStackifyIntegration.Resilience
{
    public class OpperationFailedException : ApplicationException
    {
        public OpperationFailedException(string message, Exception innerException, object data) : base(message, innerException)
        {
            Data = data;
        }

        public object Data { get; set; }

    }
}