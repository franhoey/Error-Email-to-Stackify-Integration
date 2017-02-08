using System;

namespace EmailToStackifyIntegration.ErrorReporter
{
    public class ErrorReport : ApplicationException
    {
        public override string StackTrace { get; }

        public ErrorReport(string message, string stackTrace) : base(message)
        {
            StackTrace = stackTrace;
        }
    }
}