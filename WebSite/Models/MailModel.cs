using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Linq;

namespace WebSite.Models
{
    public class MailModel
    {
        public IEnumerable<EmailAddress> Receivers { get; set; } = Enumerable.Empty<EmailAddress>();
        public string Subject { get; set; }
        public string PlainTextContent { get; set; }
        public string HtmlContent { get; set; }
        public IEnumerable<EmailAddress> CCs { get; set; } = Enumerable.Empty<EmailAddress>();
        public IEnumerable<Attachment> Attachments { get; set; } = Enumerable.Empty<Attachment>();
    }

}