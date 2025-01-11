namespace Core.Exceptions;

public class InvalidCredentials<TTarget> : DomainException
{
}

public class InvalidCredentials : InvalidCredentials<object>
{
}
