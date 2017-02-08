using System.Text.RegularExpressions;
using MimeKit;
using MimeKit.Text;

namespace EmailToStackifyIntegration.MailListener
{
    public static class EmailMessageParser
    {
        private const string APPLICATION_REGEX_GROUP = "app";
        private const string ENVIRONMENT_REGEX_GROUP = "env";
        private const string SUBJECT_REGEX_PATTERN = @"Error\sin\s(?<" + APPLICATION_REGEX_GROUP + @">[^\(]+)\((?<" + ENVIRONMENT_REGEX_GROUP + @">[^\)]+)";
        private static Regex SubjectParser = new Regex(SUBJECT_REGEX_PATTERN, RegexOptions.Compiled);

        private const string MESSAGE_REGEX_GROUP = "grp";
        private const string ERROR_MESSAGE_REGEX = @"Exception\sThrown:\</b\>\s(?<" + MESSAGE_REGEX_GROUP + @">.*?)(\<br\>\s*)?\<b\>";
        private static Regex ErrorMessageParser = new Regex(ERROR_MESSAGE_REGEX, RegexOptions.Compiled | RegexOptions.Singleline);


        private const string TRACE_REGEX_GROUP = "trace";
        private const string TRACE_REGEX = @"Trace:\</b\>\<br\>\s*(?<" + TRACE_REGEX_GROUP + @">.*?)\<br\>";
        private static Regex TraceMessageParser = new Regex(TRACE_REGEX, RegexOptions.Compiled | RegexOptions.Singleline);

        public static ErrorDetail Parse(MimeMessage message)
        {
            var detail = new ErrorDetail()
            {
                ErrorTime = message.Date.DateTime
            };

            ParseSubject(detail, message);
            ParseBody(detail, message);

            return detail;
        }

        private static void ParseBody(ErrorDetail detail, MimeMessage message)
        {
            var body = message.GetTextBody(TextFormat.Html);

            var messageMatch = ErrorMessageParser.Match(body);
            if (messageMatch.Success)
                detail.Message = messageMatch.Groups[MESSAGE_REGEX_GROUP].Value;

            var traceMatch = TraceMessageParser.Match(body);
            if (traceMatch.Success)
                detail.Trace = traceMatch.Groups[TRACE_REGEX_GROUP].Value;
        }

        private static void ParseSubject(ErrorDetail detail, MimeMessage message)
        {
            var result = SubjectParser.Match(message.Subject);
            if (!result.Success)
                return;

            detail.Application = result.Groups[APPLICATION_REGEX_GROUP].Value;
            detail.Environment = result.Groups[ENVIRONMENT_REGEX_GROUP].Value;
        }
    }
}