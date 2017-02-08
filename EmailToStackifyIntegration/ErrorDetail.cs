using System;

namespace EmailToStackifyIntegration
{
    public class ErrorDetail
    {
        public string Application { get; set; }
        public string Environment { get; set; }
        public string Message { get; set; }
        public string Trace { get; set; }
        public DateTime ErrorTime { get; set; }
        
        public override string ToString()
        {
            return $"{Environment}:{Application}:{ErrorTime}:{Message}";
        }
    }
}