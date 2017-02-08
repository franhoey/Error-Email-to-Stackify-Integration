using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using EmailToStackifyIntegration.ErrorReporter;
using EmailToStackifyIntegration.MailListener;
using StackifyLib;

namespace EmailToStackifyIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.GlobalApiKey = "masked";

            var errorReportStream = new Subject<ErrorDetail>();

            var emailListener = new ImapListener(errorReportStream);
            var reporter = new StackifyReporter(errorReportStream);

            emailListener.Start();
            reporter.Start();

            Console.WriteLine("Press any key to close");
            Console.ReadKey();

            emailListener.Stop();
            errorReportStream.OnCompleted();
        }
    }
}
