using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Auth;

public class AuthorizedUserBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext.HttpContext.Items.TryGetValue("authorizedUser", out var value) &&
            value is AuthorizedUser authUser)
        {
            bindingContext.Result = ModelBindingResult.Success(authUser);
        }
        else
        {
            throw new InvalidOperationException($"{nameof(AuthorizedUser)} cannot be used without authorization.");
        }

        return Task.CompletedTask;
    }
}

public class AuthorizedUserBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(AuthorizedUser) ? new AuthorizedUserBinder() : null;
    }
}
