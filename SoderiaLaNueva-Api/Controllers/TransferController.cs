using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Transfer;
using SoderiaLaNueva_Api.Models.Constants;

namespace SoderiaLaNueva_Api.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    public class TransferController(TransferService transferService) : BaseController
    {
        private readonly TransferService _transferService = transferService;

        #region CRUD
        [HttpPost]
        public async Task<GenericResponse<GetAllResponse>> GetAll([FromBody] GetAllRequest rq)
        {
            return await _transferService.GetAll(rq);
        }

        [HttpPost]
        public async Task<GenericResponse> Create([FromBody] CreateRequest rq)
        {
            return await _transferService.Create(rq);
        }

        [HttpPost]
        public async Task<GenericResponse<UpdateResponse>> Update([FromBody] UpdateRequest rq)
        {
            return await _transferService.Update(rq);
        }

        [HttpPost]
        public async Task<GenericResponse> Delete([FromBody] DeleteRequest rq)
        {
            return await _transferService.Delete(rq);
        }
        #endregion
    }
}
