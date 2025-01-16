using Microsoft.AspNetCore.Mvc;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Cart;

namespace SoderiaLaNueva_Api.Controllers
{
    public class CartController(CartService cartService) : BaseController
    {
        private readonly CartService _cartService = cartService;

        #region Combos

        [HttpGet]
        public async Task<GenericResponse<GenericComboResponse>> GetPaymentStatusesCombo()
        {
            return await _cartService.GetPaymentStatusesCombo();
        }
        #endregion

        #region Basic methods
        [HttpGet]
        public GenericResponse<GetFormDataResponse> GetFormData()
        {
            return _cartService.GetFormData();
        }

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
        public async Task<GenericResponse<UpdateResponse>> Update([FromBody] UpdateRequest rq)
        {
            return await _cartService.Update(rq);
        }

        [HttpPost]
        public async Task<GenericResponse> Delete([FromBody] DeleteRequest rq)
        {
            return await _cartService.Delete(rq);
        }

        #endregion

        #region Cart status
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
        #endregion
    }
}
