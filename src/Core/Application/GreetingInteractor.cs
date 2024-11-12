using Core.Domain;
using Core.Infrastructure;

namespace Core.Application;

public class GreetingInteractor
{
    private PostgresContext _ctx;

    public GreetingInteractor(PostgresContext ctx)
    {
        _ctx = ctx;
    }

    public void Create(string greetingContent)
    {
        var greeting = new Greeting
        {
            Content = greetingContent,
        };

        _ctx.Add(greeting);
        _ctx.SaveChanges();
    }

    public Greeting[] View()
    {
        var greetings = _ctx.Greetings.ToArray();
        return greetings;
    }
}
