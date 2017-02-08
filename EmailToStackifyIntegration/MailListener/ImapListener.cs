using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmailToStackifyIntegration.Resilience;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

namespace EmailToStackifyIntegration.MailListener
{
    public class ImapListener
    {
        public const int DEFAULT_CHECK_INTERVAL_MILLISECONDS = 1000;

        private Timer _timer;

        private static Dictionary<string, MailBoxFolderStatus> _mailFolderStatuses =
            new Dictionary<string, MailBoxFolderStatus>();

        private static readonly string[] MailfolderNames = new[] {"Inbox/stuff", "Junk E-Mail"};
        private readonly IObserver<ErrorDetail> _reportStream;
        
        private bool currentlyChecking = false;
        
        public TimeSpan CheckInterval { get; }

        public ImapListener(IObserver<ErrorDetail> reportStream)
            : this(reportStream, TimeSpan.FromMilliseconds(DEFAULT_CHECK_INTERVAL_MILLISECONDS))
        {
            
        }

        public ImapListener(IObserver<ErrorDetail> reportStream, TimeSpan checkInterval)
        {
            _reportStream = reportStream;
            CheckInterval = checkInterval;
        }

        public void Start()
        {
            _timer = new Timer (async (o) => await CheckEmails(), new object(), CheckInterval, CheckInterval);
        }

        public void Stop()
        {
            _timer.Dispose();
            _timer = null;
        }

        private async Task CheckEmails()
        {
            if (currentlyChecking)
                return;

            currentlyChecking = true;

            try
            {
                using (var client = new ImapClient())
                {
                    // Accept all SSL certificates
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connect("mail.emac.ltd.uk", 993, true);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Authenticate(Settings.EMAIL_ADDRESS, Settings.PASSWORD);

                    foreach (var mailfolderName in MailfolderNames)
                    {
                        await CheckMailfolder(client.GetFolder(mailfolderName));
                    }


                    client.Disconnect(true);
                }
            }
            finally
            {
                currentlyChecking = false;
            }
            
        }

        private async Task CheckMailfolder(IMailFolder mailFolder)
        {
            if (!mailFolder.IsOpen)
                mailFolder.Open(FolderAccess.ReadOnly);

            var folderStatus = GetFolderStatus(mailFolder.FullName);
            
            var ids = await SearchMail(mailFolder, folderStatus);

            
            var newIds = ids
                .Where(id => id > folderStatus.LastReceived)
                .ToList();
            
            foreach (var uid in newIds)
            {
                var message = await GetMessage(mailFolder, uid);

                folderStatus.LastReceived = uid;

                ReportMessage(message);
            }

            folderStatus.LastChecked = DateTime.Now;
        }

        private async Task<IList<UniqueId>> SearchMail(IMailFolder mailFolder, MailBoxFolderStatus folderStatus)
        {
            var tollerantSearch = new TolerantFunction<SearchQuery, IList<UniqueId>>(new TolerantFunctionConfig<SearchQuery, IList<UniqueId>>()
            {
                Func = searchQuery => mailFolder.Search(searchQuery)
            });

            var query = SearchQuery.DeliveredAfter(folderStatus.LastChecked);
            return await tollerantSearch.Execute(query);
        }

        private async Task<MimeMessage> GetMessage(IMailFolder mailFolder, UniqueId uid)
        {
            var tollerantGetMessage = new TolerantFunction<UniqueId, MimeMessage>(new TolerantFunctionConfig<UniqueId, MimeMessage>()
            {
                Func = id => mailFolder.GetMessage(id)
            });

            return await tollerantGetMessage.Execute(uid);
        }

        private void ReportMessage(MimeMessage message)
        {
            var errorDetail = EmailMessageParser.Parse(message);
            _reportStream.OnNext(errorDetail);
        }

        private MailBoxFolderStatus GetFolderStatus(string name)
        {
            if (!_mailFolderStatuses.ContainsKey(name))
                _mailFolderStatuses[name] = new MailBoxFolderStatus();
            

            return _mailFolderStatuses[name];
        }
        
    }
}