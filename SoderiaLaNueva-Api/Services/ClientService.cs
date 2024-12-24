using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Client;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class ClientService(APIContext context)
    {
        private readonly APIContext _db = context;

        #region CRUD
        public async Task<GenericResponse<GetAllResponse>> GetAll(GetAllRequest rq)
        {
            var query = _db
                .Client
                .Include(x => x.Dealer)
                .AsQueryable();

            query = FilterQuery(query, rq);
            query = OrderQuery(query, rq.ColumnSort, rq.SortDirection);

            var response = new GenericResponse<GetAllResponse>
            {
                Data = new GetAllResponse
                {
                    TotalCount = await query.CountAsync(),
                    Clients = await query.Select(x => new GetAllResponse.Item
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Address = x.Address,
                        Debt = x.Debt,
                        Phone = x.Phone,
                        DeliveryDay = x.DeliveryDay,
                        DealerName = x.Dealer.FullName
                    })
                    .Skip((rq.Page - 1) * Pagination.DefaultPageSize)
                    .Take(Pagination.DefaultPageSize)
                    .ToListAsync()
            }
            };
            return response;
        }

        public async Task<GenericResponse<GetOneResponse>> GetOneById(GetOneRequest rq)
        {
            var response = new GenericResponse<GetOneResponse>();

            var client = await _db
                .Client
                .Include(x => x.Products)
                .Include(x => x.Subscriptions)
                .Select(x => new
                {
                    x.Id,
                    x.IsActive,
                    x.Name,
                    x.Address,
                    x.Phone,
                    x.Debt,
                    x.DeliveryDay,
                    x.DealerId,
                    x.HasInvoice,
                    x.InvoiceType,
                    x.TaxCondition,
                    x.CUIT,
                    x.Observations,
                    Products = x.Products.Select(x => x.Id).ToList(),
                    Subscriptions = x.Subscriptions.Select(x => x.Id).ToList()
                })
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (client is null)
                return response.SetError(Messages.Error.EntityNotFound("Cliente"));

            if (!client.IsActive)
                return response.SetError(Messages.Error.InactiveClient(client.Name));

            var salesHistory = await GetCartsTransfersHistory(client.Id);
            var productHistory = await GetProductHistory(client.Id);

            response.Data = new GetOneResponse
            {
                Id = client.Id,
                Name = client.Name,
                Address = client.Address,
                Phone = client.Phone,
                Debt = client.Debt,
                DeliveryDay = client.DeliveryDay,
                DealerId = client.DealerId,
                HasInvoice = client.HasInvoice,
                InvoiceType = client.InvoiceType,
                TaxCondition = client.TaxCondition,
                CUIT = client.CUIT,
                Observations = client.Observations,
                Products = client.Products,
                Subscriptions = client.Subscriptions,
                SalesHistory = salesHistory,
                ProductHistory = productHistory
            };

            return response;
        }

        public async Task<GenericResponse<CreateResponse>> Create(CreateRequest rq)
        {
            var response = new GenericResponse<CreateResponse>();

            // Create client
            Client client = new()
            {
                DealerId = rq.DealerId,
                Name = rq.Name,
                Address = rq.Address,
                Phone = rq.Phone,
                Observations = rq.Observations,
                DeliveryDay = rq.DeliveryDay,
                HasInvoice = rq.HasInvoice,
                InvoiceType = rq.InvoiceType,
                TaxCondition = rq.TaxCondition,
                CUIT = rq.CUIT
            };

            // Validate request
            if (!response.Attach(await ValidateFields(client)).Success)
                return response;

            if (rq.Products.Any(x => x.Quantity < 0))
                return response.SetError(Messages.Error.FieldGraterOrEqualThanZero("cantidad"));

            // Validate type
            if (!await _db.Product.AnyAsync(x => rq.Products.Select(x => x.ProductId).ToList().Contains(x.Id)))
                return response.SetError(Messages.Error.EntitiesNotFound("productos"));

            client.Products = rq.Products.Select(x => new ClientProduct()
            {
                ProductId = x.ProductId,
                Stock = x.Quantity
            }).ToList();

            // Save changes
            await _db.Client.AddAsync(client);
            try
            {
                await _db.Database.BeginTransactionAsync();
                await _db.SaveChangesAsync();
                await _db.Database.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _db.Database.RollbackTransactionAsync();
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityCreated("Cliente");
            return response;
        }

        public async Task<GenericResponse> Delete(DeleteRequest rq)
        {
            var response = new GenericResponse();

            // Retrieve client
            var client = await _db
                .Client
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Route)
                .Include(x => x.Products)
                .Include(x => x.Transfers)
                .Include(x => x.Subscriptions)
                .Include(x => x.SubscriptionRenewals)
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (client is null)
                return response.SetError(Messages.Error.EntityNotFound("Cliente"));

            client.IsActive = false;

            // Delete related information
            client.Carts.Where(x => x.Route.IsStatic).ToList().ForEach(x => x.DeletedAt = DateTime.UtcNow.AddHours(-3)); // Keep dynamics
            client.Products.ForEach(x => x.DeletedAt = DateTime.UtcNow.AddHours(-3));
            client.Subscriptions.ForEach(x => x.DeletedAt = DateTime.UtcNow.AddHours(-3));
            client.Transfers.ForEach(x => x.DeletedAt = DateTime.UtcNow.AddHours(-3));
            client.SubscriptionRenewals.ForEach(x => x.DeletedAt = DateTime.UtcNow.AddHours(-3));

            // Save changes
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityDeleted("Cliente");
            return response;
        }

        #endregion

        #region Private
        private async Task<List<GetOneResponse.CartsTransfersHistoryItem>> GetCartsTransfersHistory(int clientId)
        {
            var cartsTransfersHistory = new List<GetOneResponse.CartsTransfersHistoryItem>();

            var transfers = await _db
                .Transfer
                .Where(x => x.ClientId == clientId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new
                {
                    x.CreatedAt,
                    x.Amount
                })
                .ToListAsync();

            var carts = await _db.Cart
                .Where(x => x.ClientId == clientId && !x.Route.IsStatic && x.Status != CartStatuses.Pending)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Include(x => x.Products)
                    .ThenInclude(x => x.Type)
                .Include(x => x.PaymentMethods)
                    .ThenInclude(x => x.PaymentMethod)
                 .Select(x => new
                 {
                     x.CreatedAt,
                     Products = x.Products.Select(x => new GetOneResponse.SaleItem
                     {
                         Name = x.Type.Name,
                         Quantity = x.SoldQuantity + x.SubscriptionQuantity,
                         Price = x.SettedPrice
                     }).ToList(),
                     PaymentMethods = x.PaymentMethods.Select(x => new GetOneResponse.PaymentItem
                     {
                         Amount = x.Amount,
                         Name = x.PaymentMethod.Name
                     }).ToList()
                 })
                .ToListAsync();

            var subscriptions = await _db
                .SubscriptionRenewal
                .Include(x => x.Subscription)
                .Where(x => x.ClientId == clientId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new
                {
                    x.CreatedAt,
                    Subscription = new GetOneResponse.SaleItem
                    {
                        Name = x.Subscription.Name,
                        Price = x.Subscription.Price
                    }
                })
                .ToListAsync();

            foreach (var transfer in transfers)
            {
                cartsTransfersHistory.Add(new()
                {
                    Date = transfer.CreatedAt.ToString("dd/MM/yyyy"),
                    Type = CartsTransfersType.Transfer,
                    Payments = [new()
                    {
                        Amount = transfer.Amount,
                    }]
                });
            }

            foreach (var cart in carts)
            {
                cartsTransfersHistory.Add(new()
                {
                    Date = cart.CreatedAt.ToString("dd/MM/yyyy"),
                    Type = CartsTransfersType.Cart,
                    Items = cart.Products,
                    Payments = cart.PaymentMethods
                });
            }

            foreach (var subscription in subscriptions)
            {
                cartsTransfersHistory.Add(new()
                {
                    Date = subscription.CreatedAt.ToString("dd/MM/yyyy"),
                    Type = CartsTransfersType.Subscription,
                    Items = [subscription.Subscription]
                });
            }

            return [.. cartsTransfersHistory.OrderByDescending(x => DateTime.ParseExact(x.Date, "dd/MM/yyyy", null))];
        }
        
        private async Task<List<GetOneResponse.ProductHistoryItem>> GetProductHistory(int clientId)
        {
            var productsHistory = new List<GetOneResponse.ProductHistoryItem>();

            var products = await _db
                .CartProduct
                .Include(x => x.Cart)
                .Include(x => x.Type)
                .Where(x => x.Cart.ClientId == clientId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(30)
                .Select(x => new
                {
                    x.CreatedAt,
                    x.Type,
                    x.SoldQuantity,
                    x.SubscriptionQuantity,
                    x.ReturnedQuantity
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            foreach (var product in products)
            {
                if (product.SoldQuantity > 0)
                {
                    productsHistory.Add(new()
                    {
                        Date = product.CreatedAt.ToString("dd/MM/yyyy"),
                        Name = product.Type.Name,
                        Type = ActionType.Sold,
                        Quantity = product.SoldQuantity
                    });
                }

                if (product.SubscriptionQuantity > 0)
                {
                    productsHistory.Add(new()
                    {
                        Date = product.CreatedAt.ToString("dd/MM/yyyy"),
                        Name = product.Type.Name,
                        Type = ActionType.Subscription,
                        Quantity = product.SubscriptionQuantity
                    });
                }

                if (product.ReturnedQuantity > 0)
                {
                    productsHistory.Add(new()
                    {
                        Date = product.CreatedAt.ToString("dd/MM/yyyy"),
                        Name = product.Type.Name,
                        Type = ActionType.Returned,
                        Quantity = product.ReturnedQuantity
                    });
                }
            }
            return productsHistory;
        }
        #endregion

        #region Validations

        private async Task<GenericResponse<CreateResponse>> ValidateFields(Client entity)
        {
            var response = new GenericResponse<CreateResponse>();

            if (string.IsNullOrEmpty(entity.Name) || string.IsNullOrEmpty(entity.Address) || string.IsNullOrEmpty(entity.Phone))
                return response.SetError(Messages.Error.FieldsRequired());

            if (entity.HasInvoice && (string.IsNullOrEmpty(entity.InvoiceType) || string.IsNullOrEmpty(entity.TaxCondition) || string.IsNullOrEmpty(entity.CUIT)))
                return response.SetError(Messages.Error.FieldsRequired());

            // Check duplicate client. Same CUIT or same name and address
            if (await _db.Client.AnyAsync(x => !x.IsActive && ((!string.IsNullOrEmpty(x.CUIT) && !string.IsNullOrEmpty(entity.CUIT) && x.CUIT.ToLower() == entity.CUIT.ToLower())
            || (x.Name.ToLower() == entity.Name.ToLower() && x.Address.ToLower() == entity.Address.ToLower()))))
            {
                return response.SetError(Messages.Error.DuplicateEntity("Cliente"));
            }

            return response;
        }

        #endregion

        #region Helpers
        private static IQueryable<Client> FilterQuery(IQueryable<Client> query, GetAllRequest rq)
        {
            if (!string.IsNullOrEmpty(rq.Name))
                query = query.Where(x => x.Name.Contains(rq.Name));

            if (rq.DealerIds != null && rq.DealerIds.Count > 0)
                query = query.Where(x => rq.DealerIds.Contains(x.Dealer.Id));

            if (rq.Id != null)
                query = query.Where(x => x.Id == rq.Id);

            return query;
        }

        private static IQueryable<Client> OrderQuery(IQueryable<Client> query, string column, string direction)
        {
            return column switch
            {
                "name" => direction == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                "createdAt" => direction == "asc" ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt),
            };
        }
        #endregion
    }
}
