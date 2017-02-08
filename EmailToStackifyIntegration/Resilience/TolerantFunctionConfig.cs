using System;

namespace EmailToStackifyIntegration.Resilience
{
    public class TolerantFunctionConfig<TIn, TOut>
    {
        public int MaxRetry { get; set; } = 20;
        public int RetryMilliseconds { get; set; } = 30;

        public Func<TIn, TOut> Func { get; set; }
    }
}