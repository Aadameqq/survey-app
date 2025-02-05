using Core.Domain;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.smtp;

public class ActivationEmailBodyGeneratorImpl(IOptions<SmtpOptions> smtpOptions)
    : ActivationEmailBodyGenerator
{
    public string Generate(Account account, string verificationCode)
    {
        var html = File.ReadAllText(
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Smtp",
                "templates",
                "email-verification.html"
            )
        );

        html = html.Replace("{userName}", account.UserName);
        html = html.Replace("{code}", verificationCode);

        return html;
    }
}
