namespace Api.Auth;

public record AuthorizedUser(Guid UserId, Guid SessionId);
