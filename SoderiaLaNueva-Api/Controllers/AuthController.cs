using Microsoft.AspNetCore.Mvc;
using SoderiaLaNueva_Api.Models.DAO.Auth;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;

namespace SoderiaLaNueva_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AuthController(AuthService authService) : ControllerBase
    {
        private readonly AuthService _authService = authService;

        [HttpPost]
        public async Task<GenericResponse<LoginResponse>> Login([FromBody] LoginRequest rq)
        {
            return await _authService.Login(rq);
        }
    

        [HttpPost]
        public async Task<GenericResponse> Logout()
        {
            return await _authService.Logout();
        }
    }
}
