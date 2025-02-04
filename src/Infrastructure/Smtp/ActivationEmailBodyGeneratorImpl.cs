using Core.Domain;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.smtp;

public class ActivationEmailBodyGeneratorImpl(IOptions<SmtpOptions> smtpOptions)
    : ActivationEmailBodyGenerator
{
    public string Generate(User user, string verificationCode)
    {
        var html = File.ReadAllText(
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Smtp",
                "templates",
                "email-verification.html"
            )
        );

        html = html.Replace("{userName}", user.UserName);
        html = html.Replace("{code}", verificationCode);

        return html;
    }
}
