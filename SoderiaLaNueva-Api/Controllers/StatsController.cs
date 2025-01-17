using Microsoft.AspNetCore.Mvc;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Stats;

namespace SoderiaLaNueva_Api.Controllers
{
    public class StatsController(StatsService statsService) : BaseController
    {
        private readonly StatsService _statsService = statsService;

        #region Methods
        [HttpGet]
        public async Task<GenericResponse<GetAnualSalesResponse>> GetAnualSales([FromQuery] GetAnualSalesRequest rq)
        {
            return await _statsService.GetAnualSales(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetMonthlySalesResponse>> GetMonthlySales([FromQuery] GetMonthlySalesRequest rq)
        {
            return await _statsService.GetMonthlySales(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetSoldProductsByMonthResponse>> GetSoldProductsByMonth([FromQuery] GetSoldProductsByMonthRequest rq)
        {
            return await _statsService.GetSoldProductsByMonth(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetProductSalesResponse>> GetProductSales([FromQuery] GetProductSalesRequest rq)
        {
            return await _statsService.GetProductSales(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetBalanceByDayResponse>> GetBalanceByDay([FromQuery] GetBalanceByDayRequest rq)
        {
            return await _statsService.GetBalanceByDay(rq);
        }

        #endregion
    }
}
