using Core.Domain;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.smtp;

public class ActivationCodeEmailSenderImpl(
    EmailSender emailSender,
    IOptions<AccountOptions> accountOptions
) : ActivationCodeEmailSender
{
    public Task Send(Account account, string activationCode)
    {
        return emailSender.Send(
            account.Email,
            "Account activation",
            GetBody(account, activationCode)
        );
    }

    private string GetBody(Account account, string activationCode)
    {
        var html = File.ReadAllText(
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Smtp",
                "templates",
                "account-activation.html"
            )
        );

        html = html.Replace("{userName}", account.UserName);
        html = html.Replace("{link}", $"{accountOptions.Value.ActivationUrl}/{activationCode}");

        return html;
    }
}
