using System;
using System.Threading.Tasks;
using EmailToStackifyIntegration.Resilience;
using StackifyLib;
using StackifyLib.Models;

namespace EmailToStackifyIntegration.ErrorReporter
{
    public class StackifyReporter
    {
        private readonly IObservable<ErrorDetail> _reportStream;
        private IDisposable _subscriptionToken;
        private static readonly DateTime EpocStart = new DateTime(1970, 1, 1);
        private readonly TolerantAction<LogMsg> _tolerantPostToStackify ;

        public StackifyReporter(IObservable<ErrorDetail> reportStream)
        {
            _reportStream = reportStream;
            _tolerantPostToStackify = new TolerantAction<LogMsg>(new TolerantAction<LogMsg>.Config()
            {
                Action = PostToStackify
            });
        }

        public void Start()
        {
            _subscriptionToken = _reportStream.Subscribe(async (e) => await ReportError(e));
        }

        public void Stop()
        {
            _subscriptionToken.Dispose();
        }

        private async Task ReportError(ErrorDetail error)
        {
            var epoc = error.ErrorTime.ToUniversalTime().Subtract(EpocStart);
            
            var msg = new LogMsg
            {
                Ex = StackifyError.New(new ErrorReport(error.Message, error.Trace)),
                EpochMs = (long)epoc.TotalMilliseconds,
                AppDetails = new LogMsgGroup() { AppName = error.Application, Env = error.Environment },
                Msg = error.Message,
                Level = "ERROR"
            };

            await _tolerantPostToStackify.Execute(msg);
        }

        private void PostToStackify(LogMsg msg)
        {
            Logger.QueueLogObject(msg);
        }
    }
}