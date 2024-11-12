namespace Api.Controllers
{
    using Core.Application;
    using Core.Domain;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class GreetingsController : ControllerBase
    {
        private readonly GreetingInteractor _interactor;

        public GreetingsController(GreetingInteractor interactor)
        {
            _interactor = interactor;
        }

        [HttpGet]
        public async Task<Greeting[]> Get()
        {

            return _interactor.View();
        }
        [HttpPost]
        public async Task PostAsync([FromBody] string content)
        {
            _interactor.Create(content);
        }
    }
}