using Microsoft.AspNetCore.Mvc;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Dealer;

namespace SoderiaLaNueva_Api.Controllers
{
    public class DealerController(DealerService dealerService) : BaseController
    {
        private readonly DealerService _dealerService = dealerService;

        #region Methods
        [HttpGet]
        public async Task<GenericResponse<GetOneResponse>> GetOne([FromQuery] GetOneRequest rq)
        {
            return await _dealerService.GetOne(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetAllResponse>> GetAll([FromQuery] GetAllRequest rq)
        {
            return await _dealerService.GetAll(rq);
        }

        #endregion

        #region Stats
        [HttpGet]
        public async Task<GenericResponse<GetClientsNotVisitedResponse>> GetClientsNotVisited([FromQuery] GetClientsNotVisitedRequest rq)
        {
            return await _dealerService.GetClientsNotVisited(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetClientsByDayResponse>> GetClientsByDay([FromQuery] GetClientsByDayRequest rq)
        {
            return await _dealerService.GetClientsByDay(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetSoldProductsResponse>> GetSoldProducts([FromQuery] GetSoldProductsRequest rq)
        {
            return await _dealerService.GetSoldProducts(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetClientStockResponse>> GetClientStock([FromQuery] GetClientStockRequest rq)
        {
            return await _dealerService.GetClientStock(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetTotalCollectedResponse>> GetTotalCollected([FromQuery] GetTotalCollectedRequest rq)
        {
            return await _dealerService.GetTotalCollected(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetClientsDebtResponse>> GetClientsDebt([FromQuery] GetClientsDebtRequest rq)
        {
            return await _dealerService.GetClientsDebt(rq);
        }
        #endregion
    }
}
