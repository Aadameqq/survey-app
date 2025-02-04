namespace Core.Ports;

public interface EmailSender
{
    public Task Send(string to, string subject, string body);
}
