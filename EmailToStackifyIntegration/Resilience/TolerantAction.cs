using System;
using System.Threading.Tasks;

namespace EmailToStackifyIntegration.Resilience
{
    public class TolerantAction<TIn>
    {
        private static Random _random = new Random();
        private readonly Config _config;

        public TolerantAction(Config config)
        {
            _config = config;
        }

        public async Task Execute(TIn parameter)
        {
            var retryCount = 0;

            //Add a bit of randomness to the retry as there could be concurrent retries
            var retryInterval = _random.Next(_config.RetryMilliseconds / 2, _config.RetryMilliseconds);

            while (true)
            {
                try
                {
                    _config.Action(parameter);
                    break;
                }
                catch (Exception ex)
                {
                    //If first fail, try again straight off
                    if (retryCount != 0)
                    {
                        var pauseTime = (retryCount * retryCount) * retryInterval;

                        await Task.Delay(pauseTime);
                    }
                    retryCount++;

                    if (retryCount >= _config.MaxRetry)
                    {
                        throw new OpperationFailedException(ex.Message, ex, parameter);
                    }
                }
            }
        }
        
        public class Config
        {
            public int MaxRetry { get; set; } = 20;
            public int RetryMilliseconds { get; set; } = 30;

            public Action<TIn> Action { get; set; }
        }
    }
}