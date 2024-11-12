namespace Api.Controllers
{
    using Core.Application;
    using Core.Domain;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class GreetingsController : ControllerBase
    {
        private readonly GreetingInteractor interactor;

        public GreetingsController(GreetingInteractor interactor)
        {
            this.interactor = interactor;
        }

        [HttpGet]
        public async Task<Greeting[]> Get()
        {
            return interactor.View();
        }
        [HttpPost]
        public async Task PostAsync([FromBody] string content)
        {
            interactor.Create(content);
        }
    }
}
