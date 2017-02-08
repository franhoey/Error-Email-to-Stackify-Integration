using System;
using MailKit;

namespace EmailToStackifyIntegration.MailListener
{
    public class MailBoxFolderStatus
    {
        public DateTime LastChecked { get; set; } = DateTime.Today.AddDays(-1);
        public UniqueId LastReceived { get; set; } = UniqueId.MinValue;
    }
}