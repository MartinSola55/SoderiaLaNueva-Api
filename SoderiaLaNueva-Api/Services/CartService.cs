using Azure;
using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Cart;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class CartService(APIContext context, AuthService authService, TokenService tokenService)
    {
        private readonly APIContext _db = context;
        private readonly AuthService _auth = authService;
        private readonly Token _token = tokenService.GetToken();

        #region Basic methods
        public async Task<GenericResponse<GetOneResponse>> GetOne(GetOneRequest rq)
        {
            var response = new GenericResponse<GetOneResponse>();

            if (!_auth.IsAdmin() && await _db.Cart.AnyAsync(x => x.Id == rq.Id && x.Route.DealerId != _token.UserId))
                return response.SetError(Messages.Error.Unauthorized());

            var query = _db
                .Cart
                .Include(x => x.Client)
                .Include(x => x.Route)
                    .ThenInclude(x => x.Dealer)
                .Include(x => x.Products)
                    .ThenInclude(x => x.Type)
                .Include(x => x.PaymentMethods)
                    .ThenInclude(x => x.PaymentMethod)
                 .Where(x => x.Id == rq.Id)
                .AsQueryable();

            return new GenericResponse<GetOneResponse>()
            {
                Data = await query.Select(x => new GetOneResponse()
                {
                    Id = x.Id,
                    DeliveryDay = x.Route.DeliveryDay,
                    Dealer = x.Route.Dealer.FullName,
                    Client = x.Client.Name,
                    Products = x.Products.Select(y => new GetOneResponse.ProductItem()
                    {
                        Id = y.Id,
                        Name = y.Type.Name,
                        SoldQuantity = y.SoldQuantity,
                        ReturnedQuantity = y.ReturnedQuantity,
                    }).ToList(),
                }).FirstOrDefaultAsync()
            };
        }

        public async Task<GenericResponse<ConfirmResponse>> Confirm(ConfirmRequest rq)
        {
            var response = new GenericResponse<ConfirmResponse>();
            decimal totalDebt = 0;

            if (!response.Attach(await ValidateConfirmation(rq)).Success)
                return response;

            // Retrieve cart
            var cart = await _db
                .Cart
                .Include(x => x.Client)
                    .ThenInclude(x => x.Products)
                    .ThenInclude(x => x.Product)
                .FirstAsync(x => x.Id == rq.Id);

            if (cart.Status != CartStatuses.Pending)
                return response.SetError(Messages.Error.CartAlreadyConfirmed());

            // Paid products
            foreach (var product in rq.Products)
            {
                var clientProduct = cart.Client.Products.First(x => x.Product.TypeId == product.ProductTypeId);

                // Update client stock
                clientProduct.Stock += product.ReturnedQuantity - product.SoldQuantity;
                // Update debt
                totalDebt += clientProduct.Product.Price * product.SoldQuantity;

                // Add product to cart
                cart.Products.Add(new()
                {
                    ProductTypeId = product.ProductTypeId,
                    SoldQuantity = product.SoldQuantity,
                    ReturnedQuantity = product.ReturnedQuantity,
                    SettedPrice = clientProduct.Product.Price,
                });
            }

            // Retrieve available products
            var availableProducts = await _db
                .SubscriptionRenewalProduct
                .Where(x => x.SubscriptionRenewal.ClientId == cart.ClientId)
                .Where(x => x.CreatedAt.Month == DateTime.UtcNow.Month)
                .Where(x => x.CreatedAt.Year == DateTime.UtcNow.Year)
                .ToListAsync();

            if (availableProducts.Count == 0)
                return response.SetError(Messages.Error.EntitiesNotFound("productos del cliente"));

            // Subscription products
            foreach (var product in rq.SubscriptionProducts)
            {
                // Filter products by type
                var availableType = availableProducts
                    .Where(x => x.ProductTypeId == product.ProductTypeId)
                    .ToList();

                if (availableType.Count == 0)
                    return response.SetError(Messages.Error.EntitiesNotFound("productos del abono"));

                if (availableType.Sum(x => x.AvailableQuantity) < product.Quantity)
                    return response.SetError(Messages.Error.NotEnoughStock($"{availableType.First().Type.Name} (abono)"));

                // Update available stock for subscription products
                int quantity = product.Quantity;
                foreach (var availableProduct in availableType)
                {
                    if (availableProduct.AvailableQuantity >= quantity)
                    {
                        availableProduct.AvailableQuantity -= quantity;
                        break;
                    }
                    else
                    {
                        quantity -= availableProduct.AvailableQuantity;
                        availableProduct.AvailableQuantity = 0;
                    }
                }

                // Update client stock
                var clientProduct = cart.Client.Products.First(x => x.Product.TypeId == product.ProductTypeId);
                clientProduct.Stock += product.Quantity;

                // Update existing products or add new if the type not exists
                var cartProduct = cart.Products.FirstOrDefault(x => x.ProductTypeId == product.ProductTypeId);
                if (cartProduct == null)
                {
                    cart.Products.Add(new()
                    {
                        ProductTypeId = product.ProductTypeId,
                        SubscriptionQuantity = product.Quantity,
                        SettedPrice = clientProduct.Product.Price
                    });
                }
                else
                {
                    cartProduct.SubscriptionQuantity = product.Quantity;
                }
            }

            // Payment methods
            foreach (var method in rq.PaymentMethods)
            {
                totalDebt -= method.Amount;
                cart.PaymentMethods.Add(new()
                {
                    PaymentMethodId = method.Id,
                    Amount = method.Amount,
                });
            }

            // Update data
            cart.Client.Debt += totalDebt;
            cart.Status = CartStatuses.Confirmed;
            cart.UpdatedAt = DateTime.UtcNow;

            // Save changes
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.Operations.CartConfirmed();
            return response;
        }

        #endregion

        #region Cart Status
        public async Task<GenericResponse> UpdateStatus(UpdateStatusRequest rq)
        {
            var response = new GenericResponse();

            if (!_auth.IsAdmin() && await _db.Cart.AnyAsync(x => x.Id == rq.Id && x.Route.DealerId != _token.UserId))
                return response.SetError(Messages.Error.Unauthorized());

            if (!await _db.Cart.AnyAsync(x => x.Id == rq.Id))
                return response.SetError(Messages.Error.EntityNotFound("Bajada", true));

            var cart = await _db
                .Cart
                .FirstAsync(x => x.Id == rq.Id);

            cart.Status = rq.Status;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.Operations.CartStatusUpdated();
            return response;
        }

        public async Task<GenericResponse> RestoreStatus(RestoreStatusRequest rq)
        {
            var response = new GenericResponse();

            if (!_auth.IsAdmin() && await _db.Cart.AnyAsync(x => x.Id == rq.Id && x.Route.DealerId != _token.UserId))
                return response.SetError(Messages.Error.Unauthorized());

            if (!await _db.Cart.AnyAsync(x => x.Id == rq.Id))
                return response.SetError(Messages.Error.EntityNotFound("Bajada", true));

            var cart = await _db
                .Cart
                .FirstAsync(x => x.Id == rq.Id);

            cart.Status = CartStatuses.Pending;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.Operations.CartStatusRestored();
            return response;
        }
        #endregion

        #region Validations

        private async Task<GenericResponse<ConfirmResponse>> ValidateConfirmation(ConfirmRequest rq)
        {
            var response = new GenericResponse<ConfirmResponse>();

            if (!_auth.IsAdmin() && await _db.Cart.AnyAsync(x => x.Id == rq.Id && x.Route.DealerId != _token.UserId))
                return response.SetError(Messages.Error.Unauthorized());

            if (!await _db.Cart.AnyAsync(x => x.Id == rq.Id && !x.Route.IsStatic))
                return response.SetError(Messages.Error.EntityNotFound("Bajada", true));

            if (rq.PaymentMethods.Any(x => x.Amount < 0))
                return response.SetError(Messages.Error.FieldGraterOrEqualThanZero("monto"));

            if (rq.Products.Any(x => x.ReturnedQuantity < 0 || x.SoldQuantity < 0) || rq.SubscriptionProducts.Any(x => x.Quantity < 0))
                return response.SetError(Messages.Error.FieldGraterOrEqualThanZero("cantidad"));

            if (rq.Products.Any(x => x.ReturnedQuantity > int.MaxValue || x.SoldQuantity > int.MaxValue) || rq.SubscriptionProducts.Any(x => x.Quantity > int.MaxValue))
                return response.SetError(Messages.Error.FieldGraterThanMax("cantidad"));

            return response;
        }

        #endregion
    }
}
