using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.User;
using SoderiaLaNueva_Api.Models.Constants;

namespace SoderiaLaNueva_Api.Controllers
{
    public class UserController(UserService userService) : BaseController
    {
        private readonly UserService _userService = userService;

        [HttpGet]
        public async Task<GenericResponse<GenericComboResponse>> GetComboRoles()
        {
            return await _userService.GetComboRoles();
        }

        [HttpGet]
        public async Task<GenericResponse<GenericComboResponse>> GetComboDealers()
        {
            return await _userService.GetComboDealers();
        }

        [HttpPost]
        public async Task<GenericResponse> UpdatePassword([FromBody] UpdatePasswordRequest rq)
        {
            return await _userService.UpdatePassword(rq);
        }

        #region CRUD

        [HttpPost]
        public async Task<GenericResponse<GetAllResponse>> GetAll([FromBody] GetAllRequest rq)
        {
            return await _userService.GetAll(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetOneResponse>> GetOneById([FromQuery] GetOneRequest rq)
        {
            return await _userService.GetOneById(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse<CreateResponse>> Create([FromBody] CreateRequest rq)
        {
            return await _userService.Create(rq);
        }

        [HttpPost]
        public async Task<GenericResponse<UpdateResponse>> Update([FromBody] UpdateRequest rq)
        {
            return await _userService.Update(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse> Delete([FromBody] DeleteRequest rq)
        {
            return await _userService.Delete(rq);
        }

        #endregion
    }
}
