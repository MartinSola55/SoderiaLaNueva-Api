﻿using Microsoft.EntityFrameworkCore;
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

        #region Combos
        public async Task<GenericResponse<GenericComboResponse>> GetPaymentMethodsCombo()
        {
            return new GenericResponse<GenericComboResponse>
            {
                Data = new GenericComboResponse
                {
                    Items = await _db.PaymentMethod
                    .Select(x => new GenericComboResponse.Item
                    {
                        Id = x.Id,
                        Description = x.Name
                    })
                    .OrderBy(x => x.Description)
                    .ToListAsync()
                }
            };
        }
        #endregion

        #region Basic methods
        public GenericResponse<GetFormDataResponse> GetFormData()
        {
            var response = new GenericResponse<GetFormDataResponse>
            {
                Data = new GetFormDataResponse
                {
                    CartStatuses = CartStatuses.GetCartStatuses(),
                    CartPaymentStatuses = PaymentStatuses.GetPaymentStatuses(),
                }
            };
            return response;
        }
        public async Task<GenericResponse<GetOneResponse>> GetOne(GetOneRequest rq)
        {
            var response = new GenericResponse<GetOneResponse>();

            if (!_auth.IsAdmin() && await _db.Cart.AnyAsync(x => x.Id == rq.Id && x.Route.DealerId != _token.UserId))
                return response.SetError(Messages.Error.Unauthorized());

            var query = _db
                .Cart
                .Include(x => x.Client)
                    .ThenInclude(x => x.SubscriptionRenewals)
                .Include(x => x.Client)
                    .ThenInclude(x => x.Products)
                .Include(x => x.Route)
                    .ThenInclude(x => x.Dealer)
                .Include(x => x.Products)
                    .ThenInclude(x => x.Type)
                .Include(x => x.PaymentMethods)
                    .ThenInclude(x => x.PaymentMethod)
                 .Where(x => x.Id == rq.Id)
                .AsQueryable();

            // Check if there's any product that doesn't exist anymore
            var cartProducts = await _db
                .CartProduct
                .Where(x => x.CartId == rq.Id)
                .Select(x => x.ProductTypeId)
                .ToListAsync();

            var clientProducts = await _db
                .ClientProduct
                .Where(x => x.ClientId == query.First().ClientId)
                .Select(x => x.Product.TypeId)
                .ToListAsync();

            if (!cartProducts.All(x => clientProducts.Contains(x)))
                return response.SetError(Messages.Error.CannotEditCart());

            return new GenericResponse<GetOneResponse>()
            {
                Data = await query.Select(x => new GetOneResponse()
                {
                    Id = x.Id,
                    RouteId = x.RouteId,
                    DeliveryDay = x.Route.DeliveryDay,
                    Dealer = x.Route.Dealer.FullName,
                    Client = x.Client.Name,
                    SubscriptionProducts = x.Client.SubscriptionRenewals
                        .SelectMany(z => z.RenewalProducts)
                        .GroupBy(z => new { z.ProductTypeId, z.Type.Name })
                        .Select(z => new GetOneResponse.SubscriptionProductItem
                        {
                            TypeId = z.Key.ProductTypeId,
                            Name = z.Key.Name,
                            Available = z.Sum(z => z.AvailableQuantity)
                        }).ToList(),
                    Products = x.Products
                    .Where(x => x.Type.Name != ProductTypes.Maquina)
                    .Select(y => new GetOneResponse.ProductItem()
                    {
                        Id = y.Id,
                        ProductTypeId = y.ProductTypeId,
                        Name = y.Type.Name,
                        Price = y.SettedPrice,
                        SoldQuantity = y.SoldQuantity,
                        ReturnedQuantity = y.ReturnedQuantity,
                        SubscriptionQuantity = y.SubscriptionQuantity,
                    }).ToList(),
                    ClientProducts = x.Client.Products
                    .Where(x => x.Product.Type.Name != ProductTypes.Maquina)
                    .Select(z => new GetOneResponse.ClientProductItem
                    {
                        ProductId= z.ProductId,
                        ProductTypeId= z.Product.TypeId,
                        Name = z.Product.Type.Name,
                        Price = z.Product.Price,
                        Stock = z.Stock
                    }).ToList(),
                    PaymentMethods = x.PaymentMethods.Select(y => new GetOneResponse.PaymentMethodItem
                    {
                        Id = y.Id,
                        PaymentMethodId = y.PaymentMethod.Id,
                        Amount = y.Amount,
                        Name = y.PaymentMethod.Name,
                    }).ToList()
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
                .Include(x => x.Products)
                .Include(x => x.PaymentMethods)
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
                .Include(x => x.Type)
                .Where(x => x.SubscriptionRenewal.ClientId == cart.ClientId)
                .Where(x => x.CreatedAt.Month == DateTime.UtcNow.Month)
                .Where(x => x.CreatedAt.Year == DateTime.UtcNow.Year)
                .ToListAsync();

            if (availableProducts.Count == 0 && rq.SubscriptionProducts.Count != 0)
                return response.SetError(Messages.Error.EntitiesNotFound("productos del cliente"));

            // Subscription products
            foreach (var product in rq.SubscriptionProducts)
            {
                // Filter products by type
                var availableTypes = availableProducts
                    .Where(x => x.ProductTypeId == product.ProductTypeId)
                    .ToList();

                if (availableTypes.Count == 0)
                    return response.SetError(Messages.Error.EntitiesNotFound("productos del abono"));

                if (availableTypes.Sum(x => x.AvailableQuantity) < product.Quantity)
                    return response.SetError(Messages.Error.NotEnoughStock($"{availableTypes.First().Type.Name} (abono)"));

                // Update available stock for subscription products
                int quantity = product.Quantity;
                foreach (var availableProduct in availableTypes)
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

                // Update existing products or add new if the type not exists
                var cartProduct = cart.Products.FirstOrDefault(x => x.ProductTypeId == product.ProductTypeId);

                if (cartProduct == null)
                {
                    cart.Products.Add(new()
                    {
                        ProductTypeId = product.ProductTypeId,
                        SubscriptionQuantity = product.Quantity,
                        ReturnedQuantity = product.Quantity,
                        SettedPrice = 0,
                    });
                }
                else
                {
                    cartProduct.SubscriptionQuantity = product.Quantity;
                    cartProduct.ReturnedQuantity += product.Quantity;
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
            response.Data = new ConfirmResponse
            {
                ClientDebt = cart.Client.Debt
            };

            return response;
        }

        public async Task<GenericResponse<UpdateResponse>> Update(UpdateRequest rq)
        {
            var response = new GenericResponse<UpdateResponse>();

            if (!response.Attach(await ValidateEdition(rq)).Success)
                return response;

            var cart = await _db
                .Cart
                .Include(x => x.Client)
                    .ThenInclude(x => x.Products)
                        .ThenInclude(x => x.Product)
                .Include(x => x.PaymentMethods)
                .Include(x => x.Products)
                .FirstAsync(x => x.Id == rq.Id);

            // Check if there are products to delete
            var productsToDelete = cart
                .Products
                .Where(x => !rq.Products.Any(y => y.ProductTypeId == x.ProductTypeId))
                .ToList();

            if (productsToDelete.Count > 0)
            {
                foreach (var product in productsToDelete)
                {
                    var clientProduct = cart
                        .Client
                        .Products
                        .FirstOrDefault(x => x.Product.TypeId == product.ProductTypeId);

                    if (clientProduct is null)
                        return response.SetError(Messages.Error.EntitiesNotFound("productos del cliente"));

                    // Update data
                    clientProduct.Stock -= product.SoldQuantity;
                    clientProduct.Stock += product.ReturnedQuantity;
                    cart.Client.Debt += product.SoldQuantity * product.SettedPrice;
                    product.DeletedAt = DateTime.UtcNow;
                }
            }

            // Check if there are methods to delete
            var methodsToDelete = cart
                .PaymentMethods
                .Where(x => !rq.PaymentMethods.Any(y => y.PaymentMethodId == x.PaymentMethodId))
                .ToList();

            if (methodsToDelete.Count > 0)
            {
                foreach (var method in methodsToDelete)
                {
                    cart.Client.Debt += method.Amount;
                    method.DeletedAt = DateTime.UtcNow;
                }
            }

            // Sold products
            if (rq.Products.Count > 0)
            {
                // Update or add new products
                foreach (var product in rq.Products)
                {
                    var clientProduct = cart
                        .Client
                        .Products
                        .First(x => x.Product.TypeId == product.ProductTypeId);

                    var cartProduct = cart
                        .Products
                        .FirstOrDefault(x => x.ProductTypeId == product.ProductTypeId);

                    // Restore previous data and update cart product
                    if (cartProduct is not null)
                    {
                        clientProduct.Stock += product.SoldQuantity - cartProduct.SoldQuantity;
                        clientProduct.Stock -= product.ReturnedQuantity - cartProduct.ReturnedQuantity;
                        cart.Client.Debt += (product.SoldQuantity - cartProduct.SoldQuantity) * cartProduct.SettedPrice;

                        cartProduct.SoldQuantity = product.SoldQuantity;
                        cartProduct.ReturnedQuantity = product.ReturnedQuantity;
                        cartProduct.SettedPrice = clientProduct.Product.Price;
                        cartProduct.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (cartProduct is null)
                    {
                        cart.Products.Add(new()
                        {
                            ProductTypeId = product.ProductTypeId,
                            SoldQuantity = product.SoldQuantity,
                            ReturnedQuantity = product.ReturnedQuantity,
                            SettedPrice = clientProduct.Product.Price,
                        });
                    }
                }
            }

            // Subscription products
            if (rq.SubscriptionProducts.Count > 0)
            {
                var availableProducts = await _db
                    .SubscriptionRenewalProduct
                    .Include(x => x.Type)
                    .Where(x => x.SubscriptionRenewal.ClientId == cart.ClientId)
                    .Where(x => x.CreatedAt.Month == cart.CreatedAt.Month)
                    .Where(x => x.CreatedAt.Year == cart.CreatedAt.Year)
                    .ToListAsync();

                // Update or add new products
                foreach (var product in rq.SubscriptionProducts)
                {
                    var clientProduct = cart
                        .Client
                        .Products
                        .First(x => x.Product.TypeId == product.ProductTypeId);

                    var cartProduct = cart
                        .Products
                        .FirstOrDefault(x => x.ProductTypeId == product.ProductTypeId);

                    var availableType = availableProducts
                        .Where(x => x.ProductTypeId == product.ProductTypeId)
                        .FirstOrDefault();

                    // Recalculate the available quantity in the first product of the type
                    if (availableType is not null)
                    {
                        if (availableProducts.Where(x => x.ProductTypeId == product.ProductTypeId).Sum(x => x.AvailableQuantity) < (product.Quantity - (cartProduct?.SubscriptionQuantity ?? 0)))
                            return response.SetError(Messages.Error.NotAvailableSubscription($"{availableType.Type.Name}"));

                        availableType.AvailableQuantity -= product.Quantity - (cartProduct?.SubscriptionQuantity ?? 0);
                    }

                    // Restore previous data and update cart product
                    if (cartProduct is not null)
                    {
                        clientProduct.Stock += (product.Quantity - cartProduct.SubscriptionQuantity);

                        cartProduct.SubscriptionQuantity = product.Quantity;
                        cartProduct.SettedPrice = clientProduct.Product.Price;
                        cartProduct.UpdatedAt = DateTime.UtcNow;
                    }

                    // Update data with new values
                    clientProduct.Stock += product.Quantity - (cartProduct?.SubscriptionQuantity ?? 0);
                }
            }

            // Payment methods
            if (rq.PaymentMethods.Count > 0)
            {
                // Update or add new methods
                foreach (var method in rq.PaymentMethods)
                {
                    var cartMethod = cart
                        .PaymentMethods
                        .FirstOrDefault(x => x.PaymentMethodId == method.PaymentMethodId);

                    if (cartMethod is not null)
                    {
                        cart.Client.Debt -= method.Amount - cartMethod.Amount;
                        cartMethod.Amount = method.Amount;
                        cartMethod.UpdatedAt = DateTime.UtcNow;
                    }
                    else if (cartMethod is null)
                    {
                        cart.PaymentMethods.Add(new()
                        {
                            PaymentMethodId = method.PaymentMethodId,
                            Amount = method.Amount,
                        });
                    }
                }
            }
            cart.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.Operations.CartUpdated();
            response.Data = new UpdateResponse
            {
                RouteId = cart.RouteId
            };

            return response;
        }

        public async Task<GenericResponse> Delete(DeleteRequest rq)
        {
            var response = new GenericResponse();

            if (!_auth.IsAdmin() && await _db.Cart.AnyAsync(x => x.Id == rq.Id && x.Route.DealerId != _token.UserId))
                return response.SetError(Messages.Error.Unauthorized());

            if (!await _db.Cart.AnyAsync(x => x.Id == rq.Id && !x.Route.IsStatic))
                return response.SetError(Messages.Error.EntityNotFound("Bajada", true));

            var cart = await _db
                .Cart
                .Include(x => x.Client)
                    .ThenInclude(x => x.Products)
                        .ThenInclude(x => x.Product)
                .Include(x => x.PaymentMethods)
                .Include(x => x.Products)
                .FirstAsync(x => x.Id == rq.Id);

            if (cart.Products.Count > 0)
            {
                var availableProducts = await _db
                    .SubscriptionRenewalProduct
                    .Where(x => x.SubscriptionRenewal.ClientId == cart.ClientId)
                    .Where(x => x.CreatedAt.Month == cart.CreatedAt.Month)
                    .Where(x => x.CreatedAt.Year == cart.CreatedAt.Year)
                    .ToListAsync();

                foreach (var product in cart.Products)
                {
                    var clientProduct = cart
                        .Client
                        .Products
                        .FirstOrDefault(x => x.Product.TypeId == product.ProductTypeId);

                    if (product.SubscriptionQuantity != 0)
                    {
                        // Subscription products
                        var availableType = availableProducts
                            .Where(x => x.ProductTypeId == product.ProductTypeId)
                            .FirstOrDefault();

                        if (availableType is null)
                            return response.SetError(Messages.Error.EntitiesNotFound("productos del abono"));
                        else
                            // Restore the available quantity in the first product of the type
                            availableType.AvailableQuantity += product.SubscriptionQuantity;
                    }

                    // Restore stock
                    if (clientProduct is not null)
                    {
                        clientProduct.Stock -= product.SoldQuantity;
                        clientProduct.Stock -= product.SubscriptionQuantity;
                        clientProduct.Stock += product.ReturnedQuantity;
                    }

                    cart.Client.Debt -= product.SoldQuantity * product.SettedPrice;
                    product.DeletedAt = DateTime.UtcNow;
                }
            }

            // Payment methods
            if (cart.PaymentMethods.Count > 0)
            {
                foreach (var method in cart.PaymentMethods)
                {
                    cart.Client.Debt += method.Amount;
                    method.DeletedAt = DateTime.UtcNow;
                }
            }

            cart.DeletedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.Operations.CartDeleted();
            return response;
        }
        #endregion

        #region Cart status
        public async Task<GenericResponse> UpdateStatus(UpdateStatusRequest rq)
        {
            var response = new GenericResponse();

            if (!_auth.IsAdmin() && await _db.Cart.AnyAsync(x => x.Id == rq.Id && x.Route.DealerId != _token.UserId))
                return response.SetError(Messages.Error.Unauthorized());

            if (!await _db.Cart.AnyAsync(x => x.Id == rq.Id))
                return response.SetError(Messages.Error.EntityNotFound("Bajada", true));

            if (!CartStatuses.Validate(rq.Status))
                return response.SetError(Messages.Error.InvalidField("estado"));

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

        private async Task<GenericResponse<UpdateResponse>> ValidateEdition(UpdateRequest rq)
        {
            var response = new GenericResponse<UpdateResponse>();

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
