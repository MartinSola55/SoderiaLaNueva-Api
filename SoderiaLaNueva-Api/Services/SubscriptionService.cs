using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Helpers;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Subscription;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class SubscriptionService(APIContext context)
    {
        private readonly APIContext _db = context;

        #region Combos
        public async Task<GenericResponse<GenericComboResponse>> GetComboSubscriptions()
        {
            var response = new GenericResponse<GenericComboResponse>
            {
                Data = new GenericComboResponse
                {
                    Items = await _db.Subscription
                    .Select(x => new GenericComboResponse.Item
                    {
                        Id = x.Id.ToString(),
                        Description = $"{x.Name} - {Formatting.FormatCurrency(x.Price)}"
                    })
                    .OrderBy(x => x.Description)
                    .ToListAsync()
                }
            };
            return response;
        }
        #endregion

        #region CRUD
        public async Task<GenericResponse<GetAllResponse>> GetAll(GetAllRequest rq)
        {
            var response = new GenericResponse<GetAllResponse>();

            var query = _db
                .Subscription
                .Include(x => x.Products)
                    .ThenInclude(x => x.ProductType)
                .AsQueryable();

            query = FilterQuery(query, rq);
            query = OrderQuery(query, rq.ColumnSort, rq.SortDirection);

            response.Data = new GetAllResponse
            {
                TotalCount = await query.CountAsync(),
                Subscriptions = await query
                .Select(x => new GetAllResponse.Item
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    SubscriptionProductItems = x.Products.Select(x => new GetAllResponse.SubscriptionProductItem
                    {
                        Id = x.Id,
                        Name = x.ProductType.Name,
                        Quantity = x.Quantity,
                    }).ToList(),
                })
                .Skip((rq.Page - 1) * Pagination.DefaultPageSize)
                .Take(Pagination.DefaultPageSize)
                .ToListAsync()
            };

            return response;
        }

        public async Task<GenericResponse<GetOneResponse>> GetOneById(GetOneRequest rq)
        {
            var response = new GenericResponse<GetOneResponse>();
            
            var subscription = await _db
                .Subscription
                .Include(x => x.Products)
                    .ThenInclude(x => x.ProductType)
                .Select(x => new GetOneResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    SubscriptionProducts = x.Products.Select(p => new GetOneResponse.SubscriptionProductItem
                    {
                        Id = p.ProductType.Id,
                        Name = p.ProductType.Name,
                        Quantity = p.Quantity,
                    }).ToList(),
                })
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (subscription == null)
                return response.SetError(Messages.Error.EntityNotFound("Abono"));

            var otherProducts = await _db
                .ProductType
                .Where(x => !subscription.SubscriptionProducts.Select(x => x.Id).Contains(x.Id))
                .Select(x => new GetOneResponse.SubscriptionProductItem
                {
                    Id = x.Id,
                    Name = x.Name,
                    Quantity = 0
                }).ToListAsync();

            subscription.SubscriptionProducts = subscription.SubscriptionProducts.Concat(otherProducts).ToList();

            response.Data = subscription;

            return response;
        }

        public async Task<GenericResponse<CreateResponse>> Create(CreateRequest rq)
        {
            var response = new GenericResponse<CreateResponse>();

            var subscription = new Subscription
            {
                Name = rq.Name,
                Price = rq.Price,
                Products = rq.SubscriptionProducts.Select(x => new SubscriptionProduct
                {
                    ProductTypeId = x.ProductTypeId,
                    Quantity = x.Quantity,
                }).ToList(),
            };

            // Validate request
            if (!response.Attach(await ValidateFields<CreateResponse>(subscription)).Success)
                return response;

            _db.Subscription.Add(subscription);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityCreated("Abono");
            response.Data = new CreateResponse
            {
                Id = subscription.Id,
                Name = subscription.Name,
                Price = subscription.Price,
                ProductTypesIds = subscription.Products.Select(x => x.ProductTypeId).ToList(),
            };
            return response;
        }

        public async Task<GenericResponse<UpdateResponse>> Update(UpdateRequest rq)
        {
            var response = new GenericResponse<UpdateResponse>();

            var subscription = await _db
                .Subscription
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (subscription == null)
                return response.SetError(Messages.Error.EntityNotFound("Abono"));

            subscription.Name = rq.Name;
            subscription.Price = rq.Price;
            subscription.UpdatedAt = DateTime.UtcNow.AddHours(-3);

            foreach (var rqProduct in rq.SubscriptionProducts)
            {
                var existingProduct = subscription.Products.FirstOrDefault(x => x.ProductTypeId == rqProduct.ProductTypeId);

                if (existingProduct != null && rqProduct.Quantity == 0)
                {
                    subscription.Products.Remove(existingProduct);
                }
                else if (existingProduct != null && rqProduct.Quantity != 0)
                {
                    existingProduct.Quantity = rqProduct.Quantity;
                }
                else if (existingProduct == null && rqProduct.Quantity != 0)
                {
                    subscription.Products.Add(new SubscriptionProduct
                    {
                        ProductTypeId = rqProduct.ProductTypeId,
                        Quantity = rqProduct.Quantity
                    });
                }
            }

            // Validate request
            if (!response.Attach(await ValidateFields<UpdateResponse>(subscription)).Success)
                return response;

            // Save changes
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Data = new()
            {
                Id = subscription.Id,
                Name = subscription.Name,
                Price = subscription.Price,
                ProductTypesIds = subscription.Products.Select(x => x.ProductTypeId).ToList(),
            };
            response.Message = Messages.CRUD.EntityUpdated("Abono");
            return response;
        }

        public async Task<GenericResponse> Delete(DeleteRequest rq)
        {
            var response = new GenericResponse();

            var subscription = await _db
                .Subscription
                .Include(x => x.Products)
                .Include(x => x.ClientSubscriptions)
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (subscription == null)
                return response.SetError(Messages.Error.EntityNotFound("Abono"));

            subscription.DeletedAt = DateTime.UtcNow.AddHours(-3);
            subscription.ClientSubscriptions.ForEach(x => x.DeletedAt = DateTime.UtcNow.AddHours(-3));
            subscription.Products.ForEach(x => x.DeletedAt = DateTime.UtcNow.AddHours(-3));

            // Save changes
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityDeleted("Abono");
            return response;
        }
        #endregion

        #region Renew
        public async Task<GenericResponse> RenewAll()
        {
            var response = new GenericResponse();
            var today = DateTime.UtcNow.AddHours(-3);

            // Get all renewed subscriptions
            var renewedSubs = await _db
                .SubscriptionRenewal
                .Where(x => x.CreatedAt.Month == today.Month && x.CreatedAt.Year == today.Year)
                .Select(x => new { x.ClientId, x.SubscriptionId })
                .ToListAsync();

            // Get all subscriptions to renew
            var subscriptionsToRenew = await _db
                .ClientSubscription
                .Include(x => x.Client)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Products)
                .Where(x => !renewedSubs.Any(y => y.ClientId == x.ClientId && y.SubscriptionId == x.SubscriptionId))
                .Select(x => new
                {
                    x.Id,
                    x.Client,
                    x.SubscriptionId,
                    x.Subscription.Price,
                    Products = x.Subscription.Products.Select(p => new
                    {
                        p.ProductTypeId,
                        p.Quantity,
                    }).ToList()
                })
                .ToListAsync();

            // Renew subscriptions and products
            foreach (var clientSub in subscriptionsToRenew)
            {
                var newRenewal = new SubscriptionRenewal
                {
                    ClientId = clientSub.Client.Id,
                    SubscriptionId = clientSub.SubscriptionId,
                    SettedPrice = clientSub.Price,
                    RenewalProducts = clientSub.Products.Select(x => new SubscriptionRenewalProduct
                    {
                        ProductTypeId = x.ProductTypeId,
                        AvailableQuantity = x.Quantity,
                    }).ToList()
                };

                // Update client debt
                clientSub.Client.Debt += clientSub.Price;

                _db.SubscriptionRenewal.Add(newRenewal);
            }

            // Save changes
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.Operations.SubscriptionRenewed();

            return response;
        }

        public async Task<GenericResponse> RenewByRoute(RenewByRouteRequest rq)
        {
            var response = new GenericResponse();
            var today = DateTime.UtcNow.AddHours(-3);

            if (!await _db.Route.AnyAsync(x => x.Id == rq.RouteId && x.IsStatic))
                return response.SetError(Messages.Error.EntityNotFound("Planilla"));

            // Get all clients from the route
            var clients = await _db
                .Route
                .Where(x => x.Id == rq.RouteId)
                .SelectMany(x => x.Carts)
                .Select(x => x.Client.Id)
                .ToListAsync();

            // Get all renewed subscriptions
            var renewedSubs = await _db
                .SubscriptionRenewal
                .Where(x => x.CreatedAt.Month == today.Month && x.CreatedAt.Year == today.Year)
                .Where(x => clients.Contains(x.ClientId))
                .Select(x => new { x.ClientId, x.SubscriptionId })
                .ToListAsync();

            // Get all subscriptions to renew
            var subscriptionsToRenew = await _db
                .ClientSubscription
                .Include(x => x.Client)
                .Include(x => x.Subscription)
                    .ThenInclude(x => x.Products)
                .Where(x => !renewedSubs.Any(y => y.ClientId == x.ClientId && y.SubscriptionId == x.SubscriptionId))
                .Where(x => clients.Contains(x.ClientId))
                .Select(x => new
                {
                    x.Id,
                    x.Client,
                    x.SubscriptionId,
                    x.Subscription.Price,
                    Products = x.Subscription.Products.Select(p => new
                    {
                        p.ProductTypeId,
                        p.Quantity,
                    }).ToList()
                })
                .ToListAsync();

            // Renew subscriptions and products
            foreach (var clientSub in subscriptionsToRenew)
            {
                var newRenewal = new SubscriptionRenewal
                {
                    ClientId = clientSub.Client.Id,
                    SubscriptionId = clientSub.SubscriptionId,
                    SettedPrice = clientSub.Price,
                    RenewalProducts = clientSub.Products.Select(x => new SubscriptionRenewalProduct
                    {
                        ProductTypeId = x.ProductTypeId,
                        AvailableQuantity = x.Quantity,
                    }).ToList()
                };

                // Update client debt
                clientSub.Client.Debt += clientSub.Price;

                _db.SubscriptionRenewal.Add(newRenewal);
            }

            // Save changes
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.Operations.SubscriptionRenewed();

            return response;
        }
        #endregion

        #region Search
        public async Task<GenericResponse<GetClientListResponse>> GetClientList(GetClientListRequest rq)
        {
            var query = _db
                .Client
                .Include(x => x.Subscriptions)
                .Include(x => x.Dealer)
                .Where(x => x.Subscriptions.Any(p => p.Id == rq.SubscriptionId))
                .OrderBy(x => x.Name)
                .AsQueryable();

            return new GenericResponse<GetClientListResponse>
            {
                Data = new GetClientListResponse
                {
                    Clients = await query
                    .Select(x => new GetClientListResponse.ClientItem
                    {
                        Name = x.Name,
                        Address = x.Address,
                        DealerName = x.Dealer.FullName,
                        DeliveryDay = x.DeliveryDay
                    })
                    .ToListAsync()
                }
            };
        }
        #endregion

        #region Validations
        private async Task<GenericResponse<T>> ValidateFields<T>(Subscription entity)
        {
            var response = new GenericResponse<T>();

            if (string.IsNullOrEmpty(entity.Name) || entity.Products.Count == 0)
                return response.SetError(Messages.Error.FieldsRequired());

            if (entity.Price < 0)
                return response.SetError(Messages.Error.FieldGraterOrEqualThanZero("precio"));

            if (entity.Products.Any(x => x.Quantity < 0))
                return response.SetError(Messages.Error.FieldGraterThanZero("cantidad"));

            if (!await _db.ProductType.AnyAsync(x => entity.Products.Select(x => x.ProductTypeId).Contains(x.Id)))
                return response.SetError(Messages.Error.EntitiesNotFound("tipos de producto"));

            return response;
        }
        #endregion

        #region Helpers
        private static IQueryable<Subscription> FilterQuery(IQueryable<Subscription> query, GetAllRequest rq)
        {
            if (!string.IsNullOrEmpty(rq.Name))
                query = query.Where(x => x.Name.Contains(rq.Name));

            return query;
        }

        private static IQueryable<Subscription> OrderQuery(IQueryable<Subscription> query, string column, string direction)
        {
            return column switch
            {
                "name" => direction == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                "price" => direction == "asc" ? query.OrderBy(x => x.Price) : query.OrderByDescending(x => x.Price),
                _ => query.OrderByDescending(x => x.CreatedAt),
            };
        }
        #endregion
    }
}