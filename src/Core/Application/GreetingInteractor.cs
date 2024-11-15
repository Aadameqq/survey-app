using Core.Domain;
using Core.Infrastructure;

namespace Core.Application;

public class GreetingInteractor
{
    private PostgresContext ctx;

    public GreetingInteractor(PostgresContext ctx)
    {
        this.ctx = ctx;
    }

    public void Create(string greetingContent)
    {
        var greeting = new Greeting
        {
            Content = greetingContent,
        };

        ctx.Add(greeting);
        ctx.SaveChanges();
    }

    public Greeting[] View()
    {
        var greetings = ctx.Greetings.ToArray();
        return greetings;
    }
}
