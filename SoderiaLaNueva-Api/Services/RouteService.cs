using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Route;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class RouteService(APIContext context, AuthService authService, TokenService tokenService)
    {
        private readonly APIContext _db = context;
        private readonly AuthService _auth = authService;
        private readonly Token _token = tokenService.GetToken();

        #region Static Methods
        public async Task<GenericResponse<GetAllStaticResponse>> GetAllStaticRoutes(GetAllStaticRequest rq)
        {
            var query = _db
                .Route
                .Include(x => x.Carts)
                .Include(x => x.Dealer)
                .AsQueryable();

            query = FilterQuery(query, rq);

            var response = new GenericResponse<GetAllStaticResponse>
            {
                Data = new GetAllStaticResponse
                {
                    Routes = await query.Select(x => new GetAllStaticResponse.Item
                    {
                        Id = x.Id,
                        Dealer = x.Dealer.FullName,
                        TotalCarts = x.Carts.Count
                    }).ToListAsync()
                }
            };

            return response;
        }

        public async Task<GenericResponse<GetStaticRouteResponse>> GetStaticRoute(GetStaticRouteRequest rq)
        {
            var response = new GenericResponse<GetStaticRouteResponse>();

            if (!await _db.Route.AnyAsync(x => x.Id == rq.Id))
                return response.SetError(Messages.Error.EntityNotFound("Ruta", true));

            if (!_auth.IsAdmin() && !await _db.Route.AnyAsync(x => x.Id == rq.Id && x.DealerId == _token.UserId))
                return response.SetError(Messages.Error.Unauthorized());

            var query = _db
                .Route
                .Include(x => x.Dealer)
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Client)
                    .ThenInclude(x => x.Carts)
                    .ThenInclude(x => x.Products)
                    .ThenInclude(x => x.Type)
                .AsQueryable();

            response.Data = await query.Select(x => new GetStaticRouteResponse
            {
                Id = x.Id,
                Dealer = x.Dealer.FullName,
                DeliveryDay = x.DeliveryDay,
                Carts = x.Carts.Select(y => new GetStaticRouteResponse.CartItem
                {
                    ClientId = y.ClientId,
                    Name = y.Client.Name,
                    Debt = y.Client.Debt,
                    Address = y.Client.Address,
                    Phone = y.Client.Phone,
                    CreatedAt = y.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    LastProducts = y.Client
                        .Carts
                        .OrderByDescending(z => z.CreatedAt)
                        .Take(10)
                        .SelectMany(z => z.Products)
                        .Select(z => new GetStaticRouteResponse.ProductItem
                        {
                            Date = z.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                            Name = z.Type.Name,
                            ReturnedQuantity = z.ReturnedQuantity,
                            SoldQuantity = z.SoldQuantity
                        }).ToList()
                }).ToList()
            }).FirstOrDefaultAsync();
            return response;
        }

        public async Task<GenericResponse<CreateStaticResponse>> CreateStaticRoute(CreateStaticRequest rq)
        {
            var response = new GenericResponse<CreateStaticResponse>();

            if (!await _db.User.AnyAsync(x => x.Id == rq.DealerId))
                return response.SetError(Messages.Error.EntityNotFound("Usuario"));

            if (await _db.User.AnyAsync(x => x.Id == rq.DealerId && x.Role.Name != Roles.Dealer))
                return response.SetError(Messages.Error.UserNotDealer());

            if (await _db.Route.AnyAsync(x => x.IsStatic && x.DealerId == rq.DealerId && x.DeliveryDay == rq.DeliveryDay))
                return response.SetError(Messages.Error.DuplicateRoute());

            // Create route
            var route = new Models.Route
            {
                DealerId = rq.DealerId,
                DeliveryDay = rq.DeliveryDay,
                IsStatic = true,
            };

            _db.Add(route);

            // Save changes
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

            response.Message = Messages.CRUD.EntityCreated("Ruta", true);
            return response;
        }

        public async Task<GenericResponse> DeleteStaticRoute(DeleteStaticRequest rq)
        {
            var response = new GenericResponse();

            // Retrieve route
            var route = await _db
                .Route
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Client)
                .FirstOrDefaultAsync(x => x.Id == rq.Id && x.IsStatic);

            if (route is null)
                return response.SetError(Messages.Error.EntityNotFound("Ruta", true));

            // Delete route
            route.DeletedAt = DateTime.UtcNow;
            route.Carts.ForEach(x => x.DeletedAt = DateTime.UtcNow);
            route.Carts.ForEach(x => x.Client.DealerId = null);

            // Save changes
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

            response.Message = Messages.CRUD.EntityDeleted("Ruta", true);
            return response;
        }

        #endregion

        #region Dynamic Methods
        public async Task<GenericResponse<GetDynamicsResponse>> GetDynamicRoute(GetDynamicsRequest rq)
        {
            var response = new GenericResponse<GetDynamicsResponse>();

            if (!_auth.IsAdmin() && !await _db.Route.AnyAsync(x => x.Id == rq.Id && x.DealerId == _token.UserId && !x.IsStatic))
                return response.SetError(Messages.Error.Unauthorized());

            if (!await _db.Route.AnyAsync(x => x.Id == rq.Id && !x.IsStatic))
                return response.SetError(Messages.Error.EntityNotFound("Planilla", true));

            var query = _db
                .Route
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Products)
                    .ThenInclude(x => x.Type)
                .Include(x => x.Carts)
                    .ThenInclude(x => x.PaymentMethods)
                    .ThenInclude(x => x.PaymentMethod)
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Client)
                    .ThenInclude(x => x.Products)
                    .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.Type)
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Client)
                    .ThenInclude(x => x.SubscriptionRenewals)
                    .ThenInclude(x => x.RenewalProducts)
                    .ThenInclude(x => x.Type)
                .Include(x => x.Dealer)
                .Where(x => !x.IsStatic && x.Id == rq.Id)
                .AsQueryable();

            response.Data = await query.Select(x => new GetDynamicsResponse
            {
                Id = x.Id,
                Dealer = x.Dealer.FullName,
                DeliveryDay = x.DeliveryDay,
                Carts = x.Carts.Select(y => new GetDynamicsResponse.CartItem
                {
                    Id = y.Id,
                    Status = y.Status,
                    CreatedAt = y.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    UpdatedAt = y.UpdatedAt.HasValue ? y.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                    PaymentMethods = y.PaymentMethods.Select(pm => new GetDynamicsResponse.CartItem.PaymentItem
                    {
                        Name = pm.PaymentMethod.Name,
                        Amount = pm.Amount
                    }).ToList(),
                    Products = y.Products.Select(p => new GetDynamicsResponse.CartItem.ProductItem
                    {
                        Name = p.Type.Name,
                        Price = p.SettedPrice,
                        SoldQuantity = p.SoldQuantity,
                        ReturnedQuantity = p.ReturnedQuantity,
                        SubscriptionQuantity = p.SubscriptionQuantity
                    }).ToList(),
                    Client = new GetDynamicsResponse.CartItem.ClientItem
                    {
                        Id = y.ClientId,
                        Name = y.Client.Name,
                        Address = y.Client.Address,
                        Phone = y.Client.Phone,
                        Debt = y.Client.Debt,
                        Observations = y.Client.Observations,
                        Products = y.Client.Products.Select(z => new GetDynamicsResponse.CartItem.ClientProductItem
                        {
                            ProductId = z.ProductId,
                            Name = z.Product.Type.Name,
                            Price = z.Product.Price,
                            Stock = z.Stock
                        }).ToList(),
                        SubscriptionProducts = y.Client.SubscriptionRenewals.SelectMany(z => z.RenewalProducts).Select(z => new GetDynamicsResponse.CartItem.ClientSubsctiptionProductItem
                        {
                            TypeId = z.ProductTypeId,
                            Name = z.Type.Name,
                            Available = z.AvailableQuantity
                        }).ToList()
                    }
                }).ToList()
            }).FirstAsync();

            return response;
        }

        public async Task<GenericResponse<GetDynamicRoutesResponse>> GetDynamicRoutes(GetDynamicRoutesRequest rq)
        {
            var query = _db
                .Route
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Products)
                .Include(x => x.Dealer)
                .Where(x => !x.IsStatic && x.CreatedAt.Date == rq.Date.Date)
                .AsQueryable();

            if (!_auth.IsAdmin())
            {
                query = query.Where(x => x.DealerId == _token.UserId);
            }

            var response = new GenericResponse<GetDynamicRoutesResponse>
            {
                Data = new GetDynamicRoutesResponse
                {
                    Routes = await query.Select(x => new GetDynamicRoutesResponse.RouteItem
                    {
                        Id = x.Id,
                        Dealer = x.Dealer.FullName,
                        TotalCarts = x.Carts.Count,
                        CompletedCarts = x.Carts.Count(y => y.Status != CartStatuses.Pending),
                        TotalCollected = x.Carts.SelectMany(y => y.Products).Sum(y => y.SoldQuantity * y.SettedPrice),
                        SoldProducts = x.Carts
                        .SelectMany(y => y.Products.Select(z => new GetDynamicRoutesResponse.RouteItem.ProductItem
                        {
                            Name = z.Type.Name,
                            Quantity = z.SoldQuantity,
                        })).ToList()
                    })
                    .OrderBy(x => x.Dealer)
                    .ToListAsync()
                }
            };

            return response;
        }

        public async Task<GenericResponse<OpenNewResponse>> OpenNew(OpenNewRequest rq)
        {
            var response = new GenericResponse<OpenNewResponse>();

            if (!_auth.IsAdmin() && !await _db.Route.AnyAsync(x => x.Id == rq.RouteId && x.DealerId == _token.UserId && x.IsStatic))
                return response.SetError(Messages.Error.Unauthorized());

            var staticRoute = await _db
                .Route
                .Include(x => x.Carts)
                .Select(x => new
                {
                    x.Id,
                    x.DealerId,
                    x.DeliveryDay,
                    Carts = x.Carts.Select(x => new { x.ClientId }),
                    x.IsStatic,
                }).FirstOrDefaultAsync(x => x.Id == rq.RouteId && x.IsStatic);

            if (staticRoute is null)
                return response.SetError(Messages.Error.EntityNotFound("Ruta", true));

            // Create route
            var route = new Models.Route
            {
                DealerId = staticRoute.DealerId,
                DeliveryDay = staticRoute.DeliveryDay,
                Carts = staticRoute.Carts.Select(x => new Cart { ClientId = x.ClientId }).ToList()
            };

            _db.Add(route);

            // Save changes
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

            response.Message = Messages.CRUD.EntityCreated("Ruta", true);
            return response;
        }

        public async Task<GenericResponse> Close(CloseRequest rq)
        {
            var response = new GenericResponse();

            if (!_auth.IsAdmin() && !await _db.Route.AnyAsync(x => x.Id == rq.RouteId && x.DealerId == _token.UserId && !x.IsStatic))
                return response.SetError(Messages.Error.Unauthorized());

            if (!await _db.Route.AnyAsync(x => x.Id == rq.RouteId && !x.IsStatic))
                return response.SetError(Messages.Error.EntityNotFound("Ruta", true));

            // Retrieve route
            var route = await _db
                .Route
                .Include(x => x.Carts)
                .FirstOrDefaultAsync(x => x.Id == rq.RouteId && !x.IsStatic);

            if (route is null)
                return response.SetError(Messages.Error.EntityNotFound("Ruta", true));

            // Close route
            route.IsClosed = true;
            route.UpdatedAt = DateTime.UtcNow;

            foreach (var cart in route.Carts.Where(x => x.Status == CartStatuses.Pending))
            {
                cart.DeletedAt = DateTime.UtcNow;
            }

            // Save changes
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

            response.Message = Messages.Operations.RouteClosed();
            return response;
        }

        public async Task<GenericResponse> UpdateClients(UpdateClientsRequest rq)
        {
            var response = new GenericResponse();

            var route = await _db
                .Route
                .Include(x => x.Carts)
                .FirstOrDefaultAsync(x => x.Id == rq.RouteId && x.IsStatic);

            if (route is null)
                return response.SetError(Messages.Error.EntityNotFound("Ruta", true));

            var cartsToRemove = route
                .Carts
                .Where(x => !rq.Clients.Contains(x.ClientId))
                .ToList();

            var cartsToAdd = rq
                .Clients
                .Where(x => !route.Carts.Any(y => y.ClientId == x))
                .Select(x => new Cart { ClientId = x })
                .ToList();

            // Update route
            route.UpdatedAt = DateTime.UtcNow;
            route.Carts.RemoveAll(x => cartsToRemove.Contains(x));
            route.Carts.AddRange(cartsToAdd);

            // Save changes
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

            response.Message = Messages.Operations.ClientsUpdated();
            return response;
        }

        #endregion

        #region Helpers
        private IQueryable<Models.Route> FilterQuery(IQueryable<Models.Route> query, GetAllStaticRequest rq)
        {
            query = query
                .Where(x => x.IsStatic)
                .Where(x => x.DeliveryDay == rq.DeliveryDay);
            
            if (!_auth.IsAdmin())
            {
                query = query.Where(x => x.DealerId == _token.UserId);
            }

            query = query.OrderBy(x => x.Dealer.FullName);
            return query;
        }
        #endregion
    }
}
