using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Subscription;
using SoderiaLaNueva_Api.Models.Constants;

namespace SoderiaLaNueva_Api.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    public class SubscriptionController(SubscriptionService subscriptionService) : BaseController
    {
        private readonly SubscriptionService _subscriptionService = subscriptionService;

        [HttpGet]
        public async Task<GenericResponse<GetFormDataResponse>> GetFormData()
        {
            return await _subscriptionService.GetFormData();
        }

        #region CRUD
        [HttpPost]
        public async Task<GenericResponse<GetAllResponse>> GetAll([FromBody] GetAllRequest rq)
        {
            return await _subscriptionService.GetAll(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetOneResponse>> GetOneById([FromQuery] GetOneRequest rq)
        {
            return await _subscriptionService.GetOneById(rq);
        }

        [HttpPost]
        public async Task<GenericResponse<CreateResponse>> Create([FromBody] CreateRequest rq)
        {
            return await _subscriptionService.Create(rq);
        }

        [HttpPost]
        public async Task<GenericResponse<UpdateResponse>> Update([FromBody] UpdateRequest rq)
        {
            return await _subscriptionService.Update(rq);
        }

        [HttpPost]
        public async Task<GenericResponse> Delete([FromBody] DeleteRequest rq)
        {
            return await _subscriptionService.Delete(rq);
        }
        #endregion
    }
}
