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
        public async Task<GenericResponse<GenericComboResponse>> GetSalesYears()
        {
            return await _statsService.GetSalesYears();
        }

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

        [HttpGet]
        public async Task<GenericResponse<GetDealerMonthlyStatsResponse>> GetDealerMonthlyStats([FromQuery] GetDealerMonthlyStatsRequest rq)
        {
            return await _statsService.GetDealerMonthlyStats(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<SoldProductsByRangeResponse>> SoldProductsByRange([FromQuery] SoldProductsByRangeRequest rq)
        {
            return await _statsService.SoldProductsByRange(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<ClientsDebtResponse>> ClientsDebt([FromQuery] ClientsDebtRequest rq)
        {
            return await _statsService.ClientsDebt(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<ClientsStockResponse>> ClientsStock([FromQuery] ClientsStockRequest rq)
        {
            return await _statsService.ClientsStock(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<NonVisitedClientsResponse>> NonVisitedClients([FromQuery] NonVisitedClientsRequest rq)
        {
            return await _statsService.NonVisitedClients(rq);
        }
        #endregion
    }
}
