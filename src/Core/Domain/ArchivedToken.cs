namespace Core.Domain;

public class ArchivedToken
{
    private Guid id = Guid.NewGuid();

    public ArchivedToken(string token, Guid sessionId)
    {
        Token = token;
        SessionId = sessionId;
    }

    public string Token { get; private set; }

    public Guid SessionId { get; private set; }
}
