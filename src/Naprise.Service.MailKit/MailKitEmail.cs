using MailKit.Net.Smtp;
using MimeKit;
using Naprise.Service.MailKit;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Naprise.Tests")]
[assembly: InternalsVisibleTo("Naprise.DocGenerator")]

namespace Naprise
{
    public static class ServiceRegistryExtensions
    {
        public static ServiceRegistry AddMailKit(this ServiceRegistry registry)
        {
            registry.Add<MailKitEmail>();
            return registry;
        }
    }
}

namespace Naprise.Service.MailKit
{
    [NapriseNotificationService("Email via MailKit", "email", "smtp", "smtps", SupportText = true, SupportMarkdown = true, SupportHtml = true)]
    [NotificationServiceWebsite("https://genteure.github.io/naprise/services/mailkitemail")]
    [NotificationServiceApiDoc("https://genteure.github.io/naprise/services/mailkitemail")]
    public sealed class MailKitEmail : NotificationService
    {
        internal static readonly IReadOnlyDictionary<string, EmailPlatform> EmailPlatforms;

        static MailKitEmail()
        {
            EmailPlatforms = BuildEmailPlatformMap();
        }

        internal readonly bool useSsl;
        internal readonly string host;
        internal readonly int port;
        internal readonly string? username;
        internal readonly string? password;
        internal readonly string from;
        internal readonly string to;

        public MailKitEmail(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // Easy setup urls: email
            // Can only send to self
            // email://{user}:{pass}@{domain}

            // Full setup: stmp, smtps
            // smtp://{smtp_host}:{smtp_port}/{from}/{to}
            // smtps://{smtp_host}:{smtp_port}/{from}/{to}
            // smtp://{smtp_host}:{smtp_port}/{username}/{password}/{from}/{to}
            // smtps://{smtp_host}:{smtp_port}/{username}/{password}/{from}/{to}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            switch (url.Scheme)
            {
                case "email":
                    {
                        if (url.Port.HasValue)
                            throw new NapriseInvalidUrlException($"Port is not allowed in email urls");

                        if (EmailPlatforms.TryGetValue(url.Host, out var p))
                        {
                            this.useSsl = p.UseSsl;
                            this.host = p.Host;
                            this.port = p.Port;

                            var colen = url.UserInfo.IndexOf(':');
                            if (colen == -1)
                                throw new NapriseInvalidUrlException($"Username and password are required in email urls");

                            this.username = url.UserInfo.Substring(0, colen);
                            this.password = url.UserInfo.Substring(colen + 1);

                            this.from = $"{this.username}@{url.Host}"; ;
                            this.to = this.from;

                            if (p.UserNameWithDomain)
                                this.username = this.from; // use "user@example.com" as username instead of just "user"
                        }
                        else
                        {
                            throw new NapriseInvalidUrlException($"Domain \"{url.Host}\" is not supported (yet)");
                        }
                        break;
                    }
                case "smtp":
                case "smtps":
                    {
                        if (!url.Port.HasValue)
                            throw new NapriseInvalidUrlException($"Port is required in smtp urls");

                        this.useSsl = url.Scheme == "smtps";
                        this.host = url.Host;
                        this.port = url.Port.Value;

                        if (segment.Count == 2)
                        {
                            this.from = segment[0];
                            this.to = segment[1];
                        }
                        else if (segment.Count == 4)
                        {
                            this.username = segment[0];
                            this.password = segment[1];
                            this.from = segment[2];
                            this.to = segment[3];
                        }
                        else
                        {
                            throw new NapriseInvalidUrlException($"Invalid number of segments in smtp urls, expected 2 or 4, got {segment.Count}");
                        }
                        break;
                    }
                default:
                    throw new NapriseInvalidUrlException($"Unknown scheme: {url.Scheme}");
            }
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var body = new BodyBuilder
            {
                TextBody = message.PreferTextBody()
            };

            if (message.Markdown is not null || message.Html is not null)
                body.HtmlBody = message.PreferHtmlBody();

            var mime = new MimeMessage
            {
                Body = body.ToMessageBody(),
                Subject = message.GetTitleWithFallback()
            };

            mime.From.Add(new MailboxAddress(string.Empty, this.from));
            mime.To.Add(new MailboxAddress(string.Empty, this.to));

            using var client = new SmtpClient();
            await client.ConnectAsync(this.host, this.port, this.useSsl, cancellationToken).ConfigureAwait(false);

            if (this.username is not null && this.password is not null)
                await client.AuthenticateAsync(this.username, this.password, cancellationToken).ConfigureAwait(false);

            await client.SendAsync(mime, cancellationToken).ConfigureAwait(false);
            await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

            // TODO build the message body
            /*
            var payload = new Payload
            {
                // TODO fill payload
                // TODO check message.Type
            };

            var url = new Url($"{(true ? "https" : "http")}://{"localhost"}").AppendPathSegments("example");
            var content = JsonContent.Create(payload, options: null);

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url, content, cancellationToken);
            var respText = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(MailKit)}: {resp.StatusCode}") // TODO change class name
                {
                    Notifier = this,
                    Notification = message,
                    ResponseStatus = resp.StatusCode,
                    ResponseBody = respText,
                };
            }

            try
            {
                var jobj = JsonDocument.Parse(respText);
                // TODO parse response and check if it's successful
                var status = jobj.RootElement.GetProperty("status").GetString();
                if (status != "ok")
                {
                    var respMessage = jobj.RootElement.GetProperty("message").GetString();
                    throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(MailKit)}: \"{respMessage}\"")
                    {
                        Notifier = this,
                        Notification = message,
                        ResponseStatus = resp.StatusCode,
                        ResponseBody = respText,
                    };
                }
            }
            catch (Exception ex)
            {
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(MailKit)}", ex)
                {
                    Notifier = this,
                    Notification = message,
                    ResponseStatus = resp.StatusCode,
                    ResponseBody = respText,
                };
            }
            */
        }

        private class Payload
        {
            // TODO add payload
        }

        internal readonly struct EmailPlatform
        {
            public readonly string Name;
            public readonly string Host;
            public readonly int Port;
            public readonly bool UseSsl;
            public readonly bool UserNameWithDomain;

            public EmailPlatform(string name, string host, int port, bool useSsl, bool userNameWithDomain)
            {
                this.Name = name ?? throw new ArgumentNullException(nameof(name));
                this.Host = host ?? throw new ArgumentNullException(nameof(host));
                this.Port = port;
                this.UseSsl = useSsl;
                this.UserNameWithDomain = userNameWithDomain;
            }
        }

        private static Dictionary<string, EmailPlatform> BuildEmailPlatformMap()
        {
            var d = new Dictionary<string, EmailPlatform>();
            EmailPlatform p;
            // also see https://github.com/caronc/apprise/blob/master/apprise/plugins/NotifyEmail.py

            // https://support.google.com/mail/answer/7126229
            d.Add("gmail.com", new(name: "Gmail", host: "smtp.gmail.com", port: 465, useSsl: true, userNameWithDomain: true));

            // https://support.microsoft.com/en-us/office/pop-imap-and-smtp-settings-8361e398-8af4-4e97-b147-6c6c4ac95353
            p = new(name: "Outlook", host: "smtp.office365.com", port: 587, useSsl: false, userNameWithDomain: true);
            d.Add("outlook.com", p);
            d.Add("hotmail.com", p);
            d.Add("live.com", p);
            d.Add("outlook.jp", p);
            d.Add("hotmail.co.jp", p);
            d.Add("live.jp", p);

            // https://support.apple.com/en-us/HT202304
            p = new(name: "iCloud", host: "smtp.mail.me.com", port: 587, useSsl: false, userNameWithDomain: true);
            d.Add("icloud.com", p);

            // https://support.yahoo-net.jp/PccMail/s/article/H000007321
            p = new(name: "Yahoo メール", host: "smtp.mail.yahoo.co.jp", port: 465, useSsl: true, userNameWithDomain: true);
            d.Add("yahoo.co.jp", p);
            d.Add("ymail.ne.jp", p);

            // https://www.zoho.com/mail/help/zoho-smtp.html
            p = new(name: "Zoho Mail", host: "smtp.zoho.com", port: 465, useSsl: true, userNameWithDomain: true);
            d.Add("zoho.com", p);
            d.Add("zohomail.com", p);

            // https://service.mail.qq.com/cgi-bin/help?subtype=1&&id=28&&no=331
            p = new(name: "QQ Mail", host: "smtp.qq.com", port: 465, useSsl: true, userNameWithDomain: true);
            d.Add("qq.com", p);
            d.Add("vip.qq.com", p);
            d.Add("foxmail.com", p);

            // https://mail.163.com/mailhelp/client.htm#pop3_smtp_server
            // https://help.163.com/special/sp/vip163_client.html
            d.Add("163.com", new(name: "Netease Mail", host: "smtp.163.com", port: 465, useSsl: true, userNameWithDomain: true));
            d.Add("126.com", new(name: "Netease Mail", host: "smtp.126.com", port: 465, useSsl: true, userNameWithDomain: true));
            d.Add("yeah.net", new(name: "Netease Mail", host: "smtp.yeah.net", port: 465, useSsl: true, userNameWithDomain: true));
            d.Add("vip.163.com", new(name: "Netease Mail", host: "smtp.vip.163.com", port: 465, useSsl: true, userNameWithDomain: true));
            d.Add("vip.126.com", new(name: "Netease Mail", host: "smtp.vip.126.com", port: 465, useSsl: true, userNameWithDomain: true));
            d.Add("188.com", new(name: "Netease Mail", host: "smtp.188.com", port: 465, useSsl: true, userNameWithDomain: true));

            // https://help.mail.10086.cn/statichtml/9/Content/837.html
            d.Add("139.com", new(name: "139 Mail (China Mobile)", host: "smtp.139.com", port: 465, useSsl: true, userNameWithDomain: true));

            // https://help.189.cn/client/client.html
            d.Add("189.cn", new(name: "189 Mail (China Telecom)", host: "smtp.189.cn", port: 465, useSsl: true, userNameWithDomain: true));

            // no docs, but: https://mail.wo.cn/
            d.Add("wo.cn", new(name: "Wo Mail (China Unicom)", host: "smtp.wo.cn", port: 465, useSsl: true, userNameWithDomain: true));

            // https://mail.sohu.com/fe/#/help
            // https://vip.sohu.com/#/help
            d.Add("sohu.com", new(name: "Sohu Mail", host: "smtp.sohu.com", port: 465, useSsl: true, userNameWithDomain: true));
            // TLS certificate not confiured on port 465 as of 2022-12-09
            d.Add("vip.sohu.com", new(name: "Sohu Mail", host: "smtp.vip.sohu.com", port: 25, useSsl: false, userNameWithDomain: true));

            // https://help.sina.com.cn/comquestiondetail/view/160/
            d.Add("sina.com", new(name: "Sina Mail", host: "smtp.sina.com", port: 465, useSsl: true, userNameWithDomain: true));
            d.Add("sina.cn", new(name: "Sina Mail", host: "smtp.sina.cn", port: 465, useSsl: true, userNameWithDomain: true));
            d.Add("vip.sina.com", new(name: "Sina Mail", host: "smtp.vip.sina.com", port: 465, useSsl: true, userNameWithDomain: true));
            d.Add("vip.sina.cn", new(name: "Sina Mail", host: "smtp.vip.sina.cn", port: 465, useSsl: true, userNameWithDomain: true));

            // https://help.tom.com/freemail/3421485459.html?col_index=16
            p = new(name: "Tom Mail", host: "smtp.tom.com", port: 465, useSsl: true, userNameWithDomain: true);
            d.Add("tom.com", p);
            d.Add("vip.tom.com", p);
            d.Add("163.net", p);
            d.Add("163vip.com", p);

            // https://yandex.com/support/mail/mail-clients/others.html
            p = new(name: "Yandex Mail", host: "smtp.yandex.com", port: 465, useSsl: true, userNameWithDomain: true);
            d.Add("yandex.com", p);
            d.Add("yandex.net", p);
            d.Add("ya.ru", p);
            d.Add("yandex.ru", p);
            d.Add("yandex.by", p);
            d.Add("yandex.kz", p);
            d.Add("yandex.uz", p);
            d.Add("yandex.fr", p);
            d.Add("narod.ru", p);

            // https://help.mail.ru/mail/mailer/popsmtp
            p = new(name: "Mail.ru", host: "smtp.mail.ru", port: 465, useSsl: true, userNameWithDomain: true);
            d.Add("mail.ru", p);
            d.Add("inbox.ru", p);
            d.Add("list.ru", p);
            d.Add("bk.ru", p);


            // https://help.yahoo.com/kb/SLN4724.html
            // https://help.yahoo.com/kb/SLN2153.html
            p = new(name: "Yahoo Mail", host: "smtp.mail.yahoo.com", port: 465, useSsl: true, userNameWithDomain: true);
            d.Add("yahoo.com", p);
            d.Add("myyahoo.com", p);
            d.Add("ymail.com", p);
            d.Add("y7mail.com", p);
            d.Add("rocketmail.com", p);

            d.Add("yahoo.com.ar", p);
            d.Add("yahoo.com.au", p);
            d.Add("yahoo.com.br", p);
            d.Add("yahoo.com.co", p);
            d.Add("yahoo.com.hk", p);
            d.Add("yahoo.com.hr", p);
            d.Add("yahoo.com.mx", p);
            d.Add("yahoo.com.my", p);
            d.Add("yahoo.com.pe", p);
            d.Add("yahoo.com.ph", p);
            d.Add("yahoo.com.sg", p);
            d.Add("yahoo.com.tr", p);
            d.Add("yahoo.com.tw", p);
            d.Add("yahoo.com.ua", p);
            d.Add("yahoo.com.ve", p);
            d.Add("yahoo.com.vn", p);
            d.Add("yahoo.co.id", p);
            d.Add("yahoo.co.il", p);
            d.Add("yahoo.co.in", p);
            d.Add("yahoo.co.kr", p);
            d.Add("yahoo.co.nz", p);
            d.Add("yahoo.co.th", p);
            d.Add("yahoo.co.uk", p);
            d.Add("yahoo.co.za", p);
            d.Add("yahoo.at", p);
            d.Add("yahoo.be", p);
            d.Add("yahoo.bg", p);
            d.Add("yahoo.ca", p);
            d.Add("yahoo.cl", p);
            d.Add("yahoo.cz", p);
            d.Add("yahoo.de", p);
            d.Add("yahoo.dk", p);
            d.Add("yahoo.ee", p);
            d.Add("yahoo.es", p);
            d.Add("yahoo.fi", p);
            d.Add("yahoo.fr", p);
            d.Add("yahoo.gr", p);
            d.Add("yahoo.hu", p);
            d.Add("yahoo.ie", p);
            d.Add("yahoo.in", p);
            d.Add("yahoo.it", p);
            d.Add("yahoo.lv", p);
            d.Add("yahoo.nl", p);
            d.Add("yahoo.no", p);
            d.Add("yahoo.pl", p);
            d.Add("yahoo.pt", p);
            d.Add("yahoo.ro", p);
            d.Add("yahoo.se", p);
            d.Add("yahoo.sk", p);

            // https://www.fastmail.help/hc/en-us/articles/1500000278342-Server-names-and-ports
            // https://www.fastmail.com/about/ourdomains/
            p = new(name: "Fastmail", host: "smtp.fastmail.com", port: 465, useSsl: true, userNameWithDomain: true);
            d.Add("123mail.org", p);
            d.Add("150mail.com", p);
            d.Add("150ml.com", p);
            d.Add("16mail.com", p);
            d.Add("2-mail.com", p);
            d.Add("4email.net", p);
            d.Add("50mail.com", p);
            d.Add("airpost.net", p);
            d.Add("allmail.net", p);
            d.Add("cluemail.com", p);
            d.Add("elitemail.org", p);
            d.Add("emailcorner.net", p);
            d.Add("emailengine.net", p);
            d.Add("emailengine.org", p);
            d.Add("emailgroups.net", p);
            d.Add("emailplus.org", p);
            d.Add("emailuser.net", p);
            d.Add("eml.cc", p);
            d.Add("f-m.fm", p);
            d.Add("fast-email.com", p);
            d.Add("fast-mail.org", p);
            d.Add("fastem.com", p);
            d.Add("fastemailer.com", p);
            d.Add("fastest.cc", p);
            d.Add("fastimap.com", p);
            d.Add("fastmail.cn", p);
            d.Add("fastmail.co.uk", p);
            d.Add("fastmail.com", p);
            d.Add("fastmail.com.au", p);
            d.Add("fastmail.de", p);
            d.Add("fastmail.es", p);
            d.Add("fastmail.fm", p);
            d.Add("fastmail.fr", p);
            d.Add("fastmail.im", p);
            d.Add("fastmail.in", p);
            d.Add("fastmail.jp", p);
            d.Add("fastmail.mx", p);
            d.Add("fastmail.net", p);
            d.Add("fastmail.nl", p);
            d.Add("fastmail.org", p);
            d.Add("fastmail.se", p);
            d.Add("fastmail.to", p);
            d.Add("fastmail.tw", p);
            d.Add("fastmail.uk", p);
            d.Add("fastmailbox.net", p);
            d.Add("fastmessaging.com", p);
            d.Add("fea.st", p);
            d.Add("fmail.co.uk", p);
            d.Add("fmailbox.com", p);
            d.Add("fmgirl.com", p);
            d.Add("fmguy.com", p);
            d.Add("ftml.net", p);
            d.Add("hailmail.net", p);
            d.Add("imap-mail.com", p);
            d.Add("imap.cc", p);
            d.Add("imapmail.org", p);
            d.Add("inoutbox.com", p);
            d.Add("internet-e-mail.com", p);
            d.Add("internet-mail.org", p);
            d.Add("internetemails.net", p);
            d.Add("internetmailing.net", p);
            d.Add("jetemail.net", p);
            d.Add("justemail.net", p);
            d.Add("letterboxes.org", p);
            d.Add("mail-central.com", p);
            d.Add("mail-page.com", p);
            d.Add("mailas.com", p);
            d.Add("mailbolt.com", p);
            d.Add("mailc.net", p);
            d.Add("mailcan.com", p);
            d.Add("mailforce.net", p);
            d.Add("mailhaven.com", p);
            d.Add("mailingaddress.org", p);
            d.Add("mailite.com", p);
            d.Add("mailmight.com", p);
            d.Add("mailnew.com", p);
            d.Add("mailsent.net", p);
            d.Add("mailservice.ms", p);
            d.Add("mailup.net", p);
            d.Add("mailworks.org", p);
            d.Add("ml1.net", p);
            d.Add("mm.st", p);
            d.Add("myfastmail.com", p);
            d.Add("mymacmail.com", p);
            d.Add("nospammail.net", p);
            d.Add("ownmail.net", p);
            d.Add("petml.com", p);
            d.Add("postinbox.com", p);
            d.Add("postpro.net", p);
            d.Add("proinbox.com", p);
            d.Add("promessage.com", p);
            d.Add("realemail.net", p);
            d.Add("reallyfast.biz", p);
            d.Add("reallyfast.info", p);
            d.Add("rushpost.com", p);
            d.Add("sent.as", p);
            d.Add("sent.at", p);
            d.Add("sent.com", p);
            d.Add("speedpost.net", p);
            d.Add("speedymail.org", p);
            d.Add("ssl-mail.com", p);
            d.Add("swift-mail.com", p);
            d.Add("the-fastest.net", p);
            d.Add("the-quickest.com", p);
            d.Add("theinternetemail.com", p);
            d.Add("veryfast.biz", p);
            d.Add("veryspeedy.net", p);
            d.Add("warpmail.net", p);
            d.Add("xsmail.com", p);
            d.Add("yepmail.net", p);
            d.Add("your-mail.com", p);

            return d;
        }
    }
}
