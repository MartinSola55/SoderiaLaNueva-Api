using Microsoft.AspNetCore.Mvc;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Cart;

namespace SoderiaLaNueva_Api.Controllers
{
    public class CartController(CartService cartService) : BaseController
    {
        private readonly CartService _cartService = cartService;

        [HttpGet]
        public async Task<GenericResponse<GetOneResponse>> GetOne([FromQuery] GetOneRequest rq)
        {
            return await _cartService.GetOne(rq);
        }

        [HttpPost]
        public async Task<GenericResponse<ConfirmResponse>> Confirm([FromBody] ConfirmRequest rq)
        {
            return await _cartService.Confirm(rq);
        }

        [HttpPost]
        public async Task<GenericResponse> UpdateStatus([FromBody] UpdateStatusRequest rq)
        {
            return await _cartService.UpdateStatus(rq);
        }

        [HttpPost]
        public async Task<GenericResponse> RestoreStatus([FromBody] RestoreStatusRequest rq)
        {
            return await _cartService.RestoreStatus(rq);
        }
    }
}
