namespace Api.Auth;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireAuthAttribute : Attribute
{
}
