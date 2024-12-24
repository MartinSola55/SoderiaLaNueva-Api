using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
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
                    Period = $"{group.Key.Year}-{group.Key.Month.ToString().PadLeft(2, '0')}",
                    Profit = group.Sum(x => x.PaymentMethods.Sum(y => y.Amount)),
                })
                .OrderBy(entry => entry.Period)
                .ToListAsync();

            var annualProfits = new List<GetAnualSalesResponse.Item>();

            for (int month = 1; month <= 12; month++)
            {
                string monthPadded = month.ToString().PadLeft(2, '0');
                string period = $"{rq.Year}-{monthPadded}";

                var cartsEntry = cartsByMonth.FirstOrDefault(entry => entry.Period == period);

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
        #endregion
    }
}