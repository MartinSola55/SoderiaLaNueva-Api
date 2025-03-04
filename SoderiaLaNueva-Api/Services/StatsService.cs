using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Stats;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class StatsService(APIContext context)
    {
        private readonly APIContext _db = context;

        #region Methods
        public async Task<GenericResponse<GenericComboResponse>> GetSalesYears()
        {
            var years = await _db
                .Route
                .Select(route => route.CreatedAt.Year)
                .Distinct()
                .OrderByDescending(year => year)
                .ToListAsync();

            return new GenericResponse<GenericComboResponse>
            {
                Data = new GenericComboResponse
                {
                    Items = years.Select(year => new GenericComboResponse.Item
                    {
                        Id = year,
                        Description = year.ToString(),
                    }).ToList()
                }
            };
        }

        public async Task<GenericResponse<GetAnualSalesResponse>> GetAnualSales(GetAnualSalesRequest rq)
        {
            var response = new GenericResponse<GetAnualSalesResponse>();

            // Group by year and month and sum the amount of all payment methods
            var cartsByMonth = await _db
                .Cart
                .Include(x => x.Route)
                .Include(x => x.PaymentMethods)
                .Where(x => !x.Route.IsStatic && x.CreatedAt.Year == rq.Year)
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .Select(group => new
                {
                    group.Key.Year,
                    group.Key.Month,
                    Profit = group.Sum(x => x.PaymentMethods.Sum(y => y.Amount)),
                })
                .OrderBy(entry => entry.Year)
                .ThenBy(entry => entry.Month)
                .ToListAsync();

            var result = cartsByMonth
                .Select(x => new
                {
                    Period = $"{x.Year}-{x.Month.ToString().PadLeft(2, '0')}",
                    x.Profit
                })
                .ToList();

            var annualProfits = new List<GetAnualSalesResponse.Item>();

            for (int month = 1; month <= 12; month++)
            {
                string monthPadded = month.ToString().PadLeft(2, '0');
                string period = $"{rq.Year}-{monthPadded}";

                var cartsEntry = result.FirstOrDefault(entry => entry.Period == period);

                decimal sold = (cartsEntry?.Profit ?? 0);

                annualProfits.Add(new GetAnualSalesResponse.Item
                {
                    Period = period,
                    Total = sold,
                });
            }

            response.Data = new GetAnualSalesResponse
            {
                Total = annualProfits.Sum(x => x.Total),
                Daily = annualProfits,
            };

            return response;
        }

        public async Task<GenericResponse<GetMonthlySalesResponse>> GetMonthlySales(GetMonthlySalesRequest rq)
        {
            var response = new GenericResponse<GetMonthlySalesResponse>();

            // Group by day and sum the amount of all payment methods
            var cartsByDay = await _db
                .Cart
                .Include(x => x.Route)
                .Include(x => x.PaymentMethods)
                .Where(x => !x.Route.IsStatic && x.CreatedAt.Year == rq.Year && x.CreatedAt.Month == rq.Month)
                .GroupBy(x => x.CreatedAt.Day)
                .Select(group => new
                {
                    Day = group.Key,
                    Profit = group.Sum(x => x.PaymentMethods.Sum(y => y.Amount)),
                })
                .OrderBy(entry => entry.Day)
                .ToListAsync();

            var total = cartsByDay.Sum(x => x.Profit);
            var monthlyProfits = new List<GetMonthlySalesResponse.Item>();

            for (int day = 1; day <= DateTime.DaysInMonth(rq.Year, rq.Month); day++)
            {
                var cart = cartsByDay.FirstOrDefault(s => s.Day == day);

                decimal sold = (cart?.Profit ?? 0);

                string monthPadded = rq.Month.ToString().PadLeft(2, '0');
                string dayPadded = day.ToString().PadLeft(2, '0');
                string period = $"{rq.Year}-{monthPadded}-{dayPadded}";

                monthlyProfits.Add(new GetMonthlySalesResponse.Item
                {
                    Period = period,
                    Total = sold,
                });
            }

            response.Data = new GetMonthlySalesResponse
            {
                Total = total,
                Daily = monthlyProfits,
            };

            return response;
        }

        public async Task<GenericResponse<GetSoldProductsByMonthResponse>> GetSoldProductsByMonth(GetSoldProductsByMonthRequest rq)
        {
            var products = await _db
                .CartProduct
                .Include(x => x.Type)
                .Where(x => x.Cart.CreatedAt.Year == rq.Year && x.Cart.CreatedAt.Month == rq.Month)
                .GroupBy(x => x.Type)
                .Select(x => new
                {
                    x.Key.Id,
                    x.Key.Name,
                    Quantity = x.Sum(x => x.SoldQuantity) + x.Sum(x => x.SubscriptionQuantity),
                })
                .ToListAsync();

            var types = await _db
                .ProductType
                .Where(x => x.Name != ProductTypes.Maquina)
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();

            foreach (int type in types.Select(x => x.Id))
            {
                if (!products.Any(x => x.Id == type))
                {
                    products.Add(new
                    {
                        Id = type,
                        types.First(x => x.Id == type).Name,
                        Quantity = 0
                    });
                }
            }

            return new GenericResponse<GetSoldProductsByMonthResponse>
            {
                Data = new GetSoldProductsByMonthResponse
                {
                    Products = products.Select(x => new GetSoldProductsByMonthResponse.Item
                    {
                        Name = x.Name,
                        Quantity = x.Quantity,
                    })
                    .OrderBy(x => x.Name)
                    .ToList(),
                },
            };
        }

        public async Task<GenericResponse<GetProductSalesResponse>> GetProductSales(GetProductSalesRequest rq)
        {
            var response = new GenericResponse<GetProductSalesResponse>();

            if (!await _db.Product.AnyAsync(x => x.Id == rq.ProductId))
                return response.SetError(Messages.Error.EntityNotFound("Producto"));

            var productType = await _db
                .Product
                .Where(x => x.Id == rq.ProductId)
                .Select(x => x.TypeId)
                .FirstOrDefaultAsync();

            var clientStock = await _db
                .ClientProduct
                .Include(x => x.Product)
                .Where(x => x.Product.TypeId == productType && x.CreatedAt.Year == rq.Year)
                .SumAsync(x => x.Stock);

            var salesByMonth = await _db
                .CartProduct
                .Where(x => x.ProductTypeId == productType && x.CreatedAt.Year == rq.Year)
                .GroupBy(x => x.CreatedAt.Month)
                .Select(x => new
                {
                    Month = x.Key,
                    Total = x.Sum(x => x.SoldQuantity) + x.Sum(x => x.SubscriptionQuantity),
                    Amount = x.Sum(x => x.SoldQuantity * x.SettedPrice),
                })
                .ToListAsync();

            int[] sales = new int[12];

            foreach (var sale in salesByMonth)
            {
                sales[sale.Month - 1] = sale.Total;
            }

            response.Data = new GetProductSalesResponse()
            {
                Sales = sales.ToList(),
                ClientStock = clientStock,
                TotalSold = salesByMonth.Sum(x => x.Amount),
            };

            return response;
        }

        public async Task<GenericResponse<GetBalanceByDayResponse>> GetBalanceByDay(GetBalanceByDayRequest rq)
        {
            var cartPaymentMethods = new GetBalanceByDayResponse.Item()
            {
                Name = "Bajadas",
                Total = await _db
                    .CartPaymentMethod
                    .Where(x => x.CreatedAt.Date == rq.Date.Date)
                    .SumAsync(x => x.Amount),
            };

            var transfers = new GetBalanceByDayResponse.Item()
            {
                Name = "Transferencias",
                Total = await _db
                    .Transfer
                    .Where(x => x.CreatedAt.Date == rq.Date.Date)
                    .SumAsync(x => x.Amount),
            };

            var expenses = new GetBalanceByDayResponse.Item()
            {
                Name = "Gastos",
                Total = await _db
                    .Expense
                    .Where(x => x.CreatedAt.Date == rq.Date.Date)
                    .SumAsync(x => x.Amount)
            };

            return new GenericResponse<GetBalanceByDayResponse>()
            {
                Data = new GetBalanceByDayResponse()
                {
                    Items = new List<GetBalanceByDayResponse.Item>()
                    {
                        cartPaymentMethods,
                        transfers,
                        expenses,
                    },
                },
            };
        }

        public async Task<GenericResponse<GetDealerMonthlyStatsResponse>> GetDealerMonthlyStats(GetDealerMonthlyStatsRequest rq)
        {
            var routes = await _db
                .Route
                .Include(x => x.Carts)
                    .ThenInclude(x => x.PaymentMethods)
                .Where(x => !x.IsStatic && x.DealerId == rq.DealerId && x.CreatedAt.Month == DateTime.UtcNow.Month)
                .AsNoTracking()
                .ToListAsync();

            var completedCarts = routes
                .SelectMany(x => x.Carts)
                .Where(x => x.Status == CartStatuses.Confirmed)
                .ToList();

            var incompleteCarts = routes
                .SelectMany(x => x.Carts)
                .Where(x => !completedCarts.Select(x => x.Id).Contains(x.Id))
                .ToList();

            var totalAmt = routes
                .SelectMany(x => x.Carts)
                .SelectMany(x => x.PaymentMethods)
                .Sum(x => x.Amount);

            return new GenericResponse<GetDealerMonthlyStatsResponse>()
            {
                Data = new GetDealerMonthlyStatsResponse()
                {
                    TotalAmount = totalAmt,
                    CompletedCarts = completedCarts.Count,
                    IncompleteCarts = incompleteCarts.Count
                },
            };
        }

        public async Task<GenericResponse<SoldProductsByRangeResponse>> SoldProductsByRange(SoldProductsByRangeRequest rq)
        {
            var routes = await _db
                .Route
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Products)
                        .ThenInclude(x => x.Type)
                .Where(x => !x.IsStatic && x.DealerId == rq.DealerId && x.CreatedAt >= rq.DateFrom && x.CreatedAt <= rq.DateTo)
                .AsNoTracking()
                .ToListAsync();

            var products = routes
                .SelectMany(x => x.Carts)
                .SelectMany(x => x.Products)
                .GroupBy(x => new { x.ProductTypeId, x.Type.Name })
                .Select(g => new
                {
                    ProductTypeId = g.Key,
                    g.Key.Name,
                    Total = g.Sum(p => (p.SoldQuantity + p.SubscriptionQuantity) * p.SettedPrice),
                    Amount = g.Sum(p => p.SoldQuantity + p.SubscriptionQuantity)
                })
                .ToList();

            return new GenericResponse<SoldProductsByRangeResponse>()
            {
                Data = new SoldProductsByRangeResponse
                {
                    Products = products.Select(x => new SoldProductsByRangeResponse.ProductItem
                    {
                        Name = x.Name,
                        Total = x.Total,
                        Amount = x.Amount
                    }).ToList()
                }
            };
        }

        public async Task<GenericResponse<ClientsDebtResponse>> ClientsDebt(ClientsDebtRequest rq)
        {
            var clients = await _db
                .Route
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Client)
                .Where(x => x.IsStatic && x.DealerId == rq.DealerId && x.DeliveryDay == rq.DeliveryDay)
                .SelectMany(x => x.Carts)
                .Select(x => x.Client)
                .ToListAsync();


            return new GenericResponse<ClientsDebtResponse>()
            {
                Data = new ClientsDebtResponse
                {
                    Clients = clients.Select(x => new ClientsDebtResponse.ClientItem
                    {
                        Name = x.Name,
                        Debt = x.Debt
                    }).ToList()
                }
            };
        }
        #endregion
    }
}