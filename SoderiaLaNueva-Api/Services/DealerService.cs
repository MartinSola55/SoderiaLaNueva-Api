using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Dealer;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class DealerService(APIContext context)
    {
        private readonly APIContext _db = context;

        #region Stats
        public async Task<GenericResponse<GetClientsNotVisitedResponse>> GetClientsNotVisited(GetClientsNotVisitedRequest rq)
        {
            var query = _db
                .Client
                .Include(x => x.Carts)
                .Where(x => x.IsActive)
                .Where(x => x.DealerId == rq.DealerId)
                .Where(x => x.Carts.Where(y => !y.Route.IsStatic && y.CreatedAt.Date >= rq.DateFrom.Date && y.CreatedAt.Date <= rq.DateTo.Date).All(y => y.Status != CartStatuses.Confirmed))
                .OrderBy(x => x.Name)
                .AsQueryable();

            var response = new GenericResponse<GetClientsNotVisitedResponse>
            {
                Data = new GetClientsNotVisitedResponse
                {
                    TotalClients = await _db.Client.Where(x => x.DealerId == rq.DealerId && x.IsActive).CountAsync(),
                    Clients = await query.Select(x => new GetClientsNotVisitedResponse.ClientItem
                    {
                        Name = x.Name,
                        Address = x.Address,
                    }).ToListAsync()
                }
            };

            return response;
        }

        public async Task<GenericResponse<GetClientsByDayResponse>> GetClientsByDay(GetClientsByDayRequest rq)
        {
            var query = _db
                .Client
                .Where(x => x.IsActive)
                .Where(x => x.DeliveryDay == rq.DeliveryDay && x.DealerId == rq.DealerId)
                .OrderBy(x => x.Name)
                .AsQueryable();

            var response = new GenericResponse<GetClientsByDayResponse>
            {
                Data = new GetClientsByDayResponse
                {
                    Clients = await query.Select(x => new GetClientsByDayResponse.ClientItem
                    {
                        Name = x.Name,
                        Debt = x.Debt
                    }).ToListAsync()
                }
            };

            return response;
        }

        public async Task<GenericResponse<GetSoldProductsResponse>> GetSoldProducts(GetSoldProductsRequest rq)
        {
            var products = _db
                .CartProduct
                .Include(x => x.Type)
                .Where(x => x.Cart.CreatedAt.Date >= rq.DateFrom.Date && x.Cart.CreatedAt.Date <= rq.DateTo.Date && x.Cart.Route.DealerId == rq.DealerId)
                .Select(x => new
                {
                    x.ProductTypeId,
                    x.Type.Name,
                    x.SoldQuantity,
                    x.SettedPrice
                });

            var productTypes = products
                .GroupBy(x => x.ProductTypeId)
                .Select(x => new
                {
                    ProductTypeId = x.Key,
                    x.First().Name,
                    SoldQuantity = x.Sum(y => y.SoldQuantity),
                    TotalAmount = x.Sum(y => y.SoldQuantity * y.SettedPrice)
                });

            var response = new GenericResponse<GetSoldProductsResponse>
            {
                Data = new GetSoldProductsResponse
                {
                    Products = await productTypes.Select(x => new GetSoldProductsResponse.ProductItem
                    {
                        Name = x.Name,
                        SoldQuantity = x.SoldQuantity,
                        TotalAmount = x.TotalAmount
                    }).ToListAsync()
                }
            };

            return response;
        }

        public async Task<GenericResponse<GetClientStockResponse>> GetClientStock(GetClientStockRequest rq)
        {
            var query = _db
                .Client
                .Include(x => x.Products)
                    .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.Type)
                .Where(x => x.IsActive && x.DealerId == rq.DealerId)
                .SelectMany(x => x.Products)
                .GroupBy(x => x.Product.Type)
                .Select(x => new
                {
                    x.Key.Name,
                    Stock = x.Sum(y => y.Stock)
                })
                .AsQueryable();

            var response = new GenericResponse<GetClientStockResponse>
            {
                Data = new GetClientStockResponse
                {
                    Products = await query.Select(x => new GetClientStockResponse.ProductItem
                    {
                        Name = x.Name,
                        Stock = x.Stock
                    }).ToListAsync()
                }
            };

            return response;
        }

        public async Task<GenericResponse<GetTotalCollectedResponse>> GetTotalCollected(GetTotalCollectedRequest rq)
        {
            var total = await _db
                .CartPaymentMethod
                .Where(x => x.Cart.Route.DealerId == rq.DealerId && x.CreatedAt.Month == rq.Date.Month && x.CreatedAt.Year == rq.Date.Year)
                .SumAsync(x => x.Amount);

            var response = new GenericResponse<GetTotalCollectedResponse>
            {
                Data = new GetTotalCollectedResponse
                {
                    Total = total,
                }
            };

            return response;
        }
        
        public async Task<GenericResponse<GetClientsDebtResponse>> GetClientsDebt(GetClientsDebtRequest rq)
        {
            var query = _db
                .Client
                .Where(x => x.IsActive && x.DealerId == rq.DealerId)
                .AsQueryable();

            var response = new GenericResponse<GetClientsDebtResponse>
            {
                Data = new GetClientsDebtResponse
                {
                    TotalDebt = await query.SumAsync(x => x.Debt),
                }
            };

            return response;
        }

        #endregion
    }
}
