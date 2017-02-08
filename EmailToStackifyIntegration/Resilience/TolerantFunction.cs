using System;
using System.Threading.Tasks;

namespace EmailToStackifyIntegration.Resilience
{
    public class TolerantFunction<TIn, TOut>
    {
        private static Random _random = new Random();
        private readonly TolerantFunctionConfig<TIn, TOut> _config;

        public TolerantFunction(TolerantFunctionConfig<TIn, TOut> config)
        {
            _config = config;
        }

        public async Task<TOut> Execute(TIn parameter)
        {
            var retryCount = 0;

            //Add a bit of randomness to the retry as there could be concurrent retries
            var retryInterval = _random.Next(_config.RetryMilliseconds / 2, _config.RetryMilliseconds);

            while (true)
            {
                try
                {
                    return _config.Func(parameter);
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
        
    }
}