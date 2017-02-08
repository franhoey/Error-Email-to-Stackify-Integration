using System;

namespace EmailToStackifyIntegration.Resilience
{
    public class TolerantActionConfig<TIn>
    {
        public int MaxRetry { get; set; } = 20;
        public int RetryMilliseconds { get; set; } = 30;

        public Action<TIn> Action { get; set; }
    }
}