using Microsoft.AspNetCore.Mvc;
using SoderiaLaNueva_Api.Services;

namespace SoderiaLaNueva_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HomeController(HomeService homeService) : ControllerBase
    {
        private readonly HomeService _homeService = homeService;
        
    }
}
