namespace Core.Exceptions;

public class NoSuch<TTarget> : DomainException { }

public class NoSuch : NoSuch<object> { }
