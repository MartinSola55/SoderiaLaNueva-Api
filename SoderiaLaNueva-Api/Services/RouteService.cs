using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Cart;
using SoderiaLaNueva_Api.Models.DAO.Route;
using System;
using System.Data;
using System.Globalization;
using System.Security.Cryptography.Xml;

namespace SoderiaLaNueva_Api.Services
{
    public class RouteService(APIContext context, AuthService authService, TokenService tokenService)
    {
        private readonly APIContext _db = context;
        private readonly AuthService _auth = authService;
        private readonly Token _token = tokenService.GetToken();


        #region Combos
        public async Task<GenericResponse<GetClientsListResponse>> GetClientsList(GetClientsListRequest rq)
        {
            var query = _db
            .Client
            .Include(x => x.Carts)
                .ThenInclude(x => x.Route)
            .Where(x => !x.Carts.Select(x => x.RouteId).Contains(rq.Id))
            .AsQueryable();

            var response = new GenericResponse<GetClientsListResponse>
            {
                Data = new GetClientsListResponse
                {
                    TotalCount = await query.CountAsync(),
                    Items = await query.Select(x => new GetClientsListResponse.ClientItem
                    {
                        ClientId = x.Id,
                        Name = x.Name,
                        Address = new GetClientsListResponse.AddressItem
                        {
                            HouseNumber = x.Address.HouseNumber,
                            Road = x.Address.Road,
                        }
                    })
                    .Skip((rq.Page - 1) * Pagination.DefaultPageSize)
                    .Take(Pagination.DefaultPageSize)
                    .ToListAsync()
                }
            };
            return response;
        }
        #endregion

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

        public async Task<GenericResponse<GetAllDealerStaticResponse>> GetAllDealerStaticRoutes()
        {
            var query = _db
                .Route
                .Where(x => x.IsStatic && x.DealerId == _token.UserId)
                .Include(x => x.Carts)
                .Include(x => x.Dealer)
                .AsQueryable();

            query = query.OrderBy(x => x.DeliveryDay);

            var response = new GenericResponse<GetAllDealerStaticResponse>
            {
                Data = new GetAllDealerStaticResponse
                {
                    Routes = await query.Select(x => new GetAllDealerStaticResponse.Item
                    {
                        Id = x.Id,
                        Dealer = x.Dealer.FullName,
                        TotalCarts = x.Carts.Count,
                        DeliveryDay = x.DeliveryDay,
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
                .Where(x => x.Id == rq.Id)
                .AsQueryable();

            response.Data = await query.Select(x => new GetStaticRouteResponse
            {
                Id = x.Id,
                Dealer = x.Dealer.FullName,
                DeliveryDay = x.DeliveryDay,
                Carts = x.Carts.Select(y => new GetStaticRouteResponse.CartItem
                {
                    Id = y.Id,
                    ClientId = y.ClientId,
                    Name = y.Client.Name,
                    Debt = y.Client.Debt,
                    Address = new GetStaticRouteResponse.AddressItem
                    {
                        HouseNumber = y.Client.Address.HouseNumber,
                        Road = y.Client.Address.Road,
                        Neighbourhood = y.Client.Address.Neighbourhood,
                        Suburb = y.Client.Address.Suburb,
                        CityDistrict = y.Client.Address.CityDistrict,
                        City = y.Client.Address.City,
                        Town = y.Client.Address.Town,
                        Village = y.Client.Address.Village,
                        County = y.Client.Address.County,
                        State = y.Client.Address.State,
                        Country = y.Client.Address.Country,
                        Postcode = y.Client.Address.Postcode,
                        Lat = y.Client.Address.Lat,
                        Lon = y.Client.Address.Lon,
                    },
                    Phone = y.Client.Phone,
                    CreatedAt = y.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    UpdatedAt = y.UpdatedAt.HasValue ? y.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "",
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

        public async Task<GenericResponse<GetStaticRouteClientsResponse>> GetStaticRouteClients(GetStaticRouteClientsRequest rq)
        {
            var response = new GenericResponse<GetStaticRouteClientsResponse>();

            if (!await _db.Route.AnyAsync(x => x.Id == rq.Id))
                return response.SetError(Messages.Error.EntityNotFound("Ruta", true));

            if (!_auth.IsAdmin() && !await _db.Route.AnyAsync(x => x.Id == rq.Id && x.DealerId == _token.UserId))
                return response.SetError(Messages.Error.Unauthorized());

            var query = _db
                .Route
                .Include(x => x.Dealer)
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Client)
                .Where(x => x.Id == rq.Id)
                .AsQueryable();

            response.Data = await query.Select(x => new GetStaticRouteClientsResponse
            {
                Id = x.Id,
                Dealer = x.Dealer.FullName,
                DeliveryDay = x.DeliveryDay,
                Clients = x.Carts.Select(y => new GetStaticRouteClientsResponse.ClientItem
                {
                    ClientId = y.ClientId,
                    Name = y.Client.Name,
                    Address = new GetStaticRouteClientsResponse.AddressItem
                    {
                        HouseNumber = y.Client.Address.HouseNumber,
                        Road = y.Client.Address.Road,
                        Neighbourhood = y.Client.Address.Neighbourhood,
                        Suburb = y.Client.Address.Suburb,
                        CityDistrict = y.Client.Address.CityDistrict,
                        City = y.Client.Address.City,
                        Town = y.Client.Address.Town,
                        Village = y.Client.Address.Village,
                        County = y.Client.Address.County,
                        State = y.Client.Address.State,
                        Country = y.Client.Address.Country,
                        Postcode = y.Client.Address.Postcode,
                        Lat = y.Client.Address.Lat,
                        Lon = y.Client.Address.Lon,
                    }
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
        public async Task<GenericResponse<GetDynamicRouteResponse>> GetDynamicRoute(GetDynamicRouteRequest rq)
        {
            var response = new GenericResponse<GetDynamicRouteResponse>();

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
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Client)
                        .ThenInclude(x => x.Carts)
                .Include(x => x.Dealer)
                .Where(x => x.Id == rq.Id)
                .AsQueryable();

            // Get transfers and expenses
            var routeDealerId = await _db
                .Route
                .Where(x => x.Id == rq.Id)
                .Select(x => x.DealerId)
                .FirstAsync();

            var routeClients = await _db
                .Cart
                .Where(x => x.RouteId == rq.Id)
                .Select(x => x.ClientId)
                .ToListAsync();

            var transfersAmount = await _db
                .Transfer
                .Where(x => routeClients.Contains(x.ClientId))
                .Where(x => x.CreatedAt.Date == DateTime.UtcNow.Date)
                .SumAsync(x => x.Amount);

            var spentAmount = await _db
                .Expense
                .Where(x => x.CreatedAt.Date == DateTime.UtcNow.Date)
                .Where(x => x.DealerId == routeDealerId)
                .SumAsync(x => x.Amount);

            var productTypes = await _db
                .ProductType
                .Where(x => x.Name != ProductTypes.Maquina)
                .Select(x => new GetDynamicRouteResponse.ProductTypeItem
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToListAsync();

            // Get route data
            response.Data = await query.Select(x => new GetDynamicRouteResponse
            {
                Id = x.Id,
                Dealer = x.Dealer.FullName,
                DeliveryDay = x.DeliveryDay,
                TransfersAmount = transfersAmount,
                SpentAmount = spentAmount,
                IsClosed = x.IsClosed,
                ProductTypes = productTypes,
                Carts = x.Carts.Select(y => new GetDynamicRouteResponse.CartItem
                {
                    Id = y.Id,
                    Status = y.Status,
                    CreatedAt = y.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    UpdatedAt = y.UpdatedAt.HasValue ? y.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty,
                    PaymentMethods = y.PaymentMethods.Select(pm => new GetDynamicRouteResponse.CartItem.PaymentItem
                    {
                        Name = pm.PaymentMethod.Name,
                        Amount = pm.Amount
                    }).ToList(),
                    Products = y.Products
                    .Where(y => y.Type.Name != ProductTypes.Maquina)
                    .Select(p => new GetDynamicRouteResponse.CartItem.ProductItem
                    {
                        ProductTypeId = p.ProductTypeId,
                        ProductId = p.ProductTypeId,
                        Name = p.Type.Name,
                        Price = p.SettedPrice,
                        SoldQuantity = p.SoldQuantity,
                        ReturnedQuantity = p.ReturnedQuantity,
                        SubscriptionQuantity = p.SubscriptionQuantity
                    }).ToList(),
                    Client = new GetDynamicRouteResponse.CartItem.ClientItem
                    {
                        Id = y.ClientId,
                        Name = y.Client.Name,
                        Address = new GetDynamicRouteResponse.CartItem.AddressItem
                        {
                            HouseNumber = y.Client.Address.HouseNumber,
                            Road = y.Client.Address.Road,
                            Neighbourhood = y.Client.Address.Neighbourhood,
                            Suburb = y.Client.Address.Suburb,
                            CityDistrict = y.Client.Address.CityDistrict,
                            City = y.Client.Address.City,
                            Town = y.Client.Address.Town,
                            Village = y.Client.Address.Village,
                            County = y.Client.Address.County,
                            State = y.Client.Address.State,
                            Country = y.Client.Address.Country,
                            Postcode = y.Client.Address.Postcode,
                            Lat = y.Client.Address.Lat,
                            Lon = y.Client.Address.Lon,
                        },
                        Phone = y.Client.Phone,
                        Debt = y.Client.Debt,
                        Observations = y.Client.Observations,
                        LastProducts = y.Client
                        .Carts
                        .OrderByDescending(z => z.CreatedAt)
                        .Take(10)
                        .SelectMany(z => z.Products)
                        .Where(z => z.Type.Name != ProductTypes.Maquina)
                        .Select(z => new GetDynamicRouteResponse.CartItem.LastProductItem
                        {
                            Date = z.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                            Name = z.Type.Name,
                            ReturnedQuantity = z.ReturnedQuantity,
                            SoldQuantity = z.SoldQuantity
                        }).ToList(),
                        Products = y.Client.Products
                        .Where(y => y.Product.Type.Name != ProductTypes.Maquina)
                        .Select(z => new GetDynamicRouteResponse.CartItem.ClientProductItem
                        {
                            ProductId = z.ProductId,
                            ProductTypeId = z.Product.TypeId,
                            Name = z.Product.Type.Name,
                            Price = z.Product.Price,
                            Stock = z.Stock
                        }).ToList(),
                        SubscriptionProducts = y.Client.SubscriptionRenewals
                        .SelectMany(z => z.RenewalProducts)
                        .GroupBy(z => new { z.ProductTypeId, z.Type.Name })
                        .Select(z => new GetDynamicRouteResponse.CartItem.ClientSubscriptionProductItem
                        {
                            TypeId = z.Key.ProductTypeId,
                            Name = z.Key.Name,
                            Available = z.Sum(z => z.AvailableQuantity)
                        }).ToList(),
                    }
                }).ToList()
            }).FirstAsync();

            return response;
        }

        public async Task<GenericResponse<GetDynamicAdminRoutesResponse>> GetDynamicAdminRoutes(GetDynamicAdminRoutesRequest rq)
        {
            var response = new GenericResponse<GetDynamicAdminRoutesResponse>();

            var query = _db
                .Route
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Products)
                .Include(x => x.Dealer)
                .Where(x => !x.IsStatic)
                .Where(x => x.CreatedAt.Date == rq.Date.Date)
                .OrderBy(x => x.Dealer.FullName)
                    .ThenByDescending(x => x.CreatedAt)
                .AsQueryable();

            var productTypes = await _db
                .ProductType
                .Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToListAsync();

            var routes = await query.Select(x => new
            {
                x.Id,
                Dealer = x.Dealer.FullName,
                TotalCarts = x.Carts.Count,
                CompletedCarts = x.Carts.Count(y => y.Status != CartStatuses.Pending),
                TotalCollected = x.Carts.SelectMany(y => y.Products).Sum(y => y.SoldQuantity * y.SettedPrice),
                CreatedAt = x.CreatedAt.Date,
                ClientIds = x.Carts.Select(x => x.ClientId),
                Products = x.Carts.SelectMany(x => x.Products)
            }).ToListAsync();

            var clientIdList = routes.SelectMany(y => y.ClientIds).ToList();
            var transfersTotalCollected = await _db
                .Transfer
                .Where(x => x.CreatedAt.Date == rq.Date.Date)
                .Where(x => clientIdList.Contains(x.ClientId))
                .Select(x => new
                {
                    x.ClientId,
                    x.Amount,
                    CreatedAt = x.CreatedAt.Date
                })
                .ToListAsync();

            var productsSold = routes
              .SelectMany(route => route.Products)
              .GroupBy(product => product.ProductTypeId)
              .Select(group => new
              {
                  ProductTypeId = group.Key,
                  TotalQuantity = group.Sum(p => p.SoldQuantity + p.SubscriptionQuantity)
              })
              .ToList();

            response.Data = new GetDynamicAdminRoutesResponse
            {
                Routes = routes.Select(x => new GetDynamicAdminRoutesResponse.RouteItem
                {
                    Id = x.Id,
                    Dealer = x.Dealer,
                    TotalCarts = x.TotalCarts,
                    CompletedCarts = x.CompletedCarts,
                    TotalCollected = x.TotalCollected + transfersTotalCollected
                        .Where(t => t.CreatedAt == x.CreatedAt && x.ClientIds.Contains(t.ClientId))
                        .Sum(t => t.Amount),
                    SoldProducts = productTypes.Select(x => new GetDynamicAdminRoutesResponse.RouteItem.SoldProductItem
                    {
                        Name = x.Name,
                        Amount = productsSold.FirstOrDefault(y => y.ProductTypeId == x.Id)?.TotalQuantity ?? 0
                    }).ToList(),
                }).ToList()
            };

            return response;
        }

        public async Task<GenericResponse<GetDynamicDealerRoutesResponse>> GetDynamicDealerRoutes(GetDynamicDealerRoutesRequest rq)
        {
            var response = new GenericResponse<GetDynamicDealerRoutesResponse>();

            var query = _db
                .Route
                .Include(x => x.Carts)
                    .ThenInclude(x => x.Products)
                .Include(x => x.Dealer)
                .Where(x => !x.IsStatic)
                .Where(x => x.DealerId == _token.UserId && x.DeliveryDay == rq.DeliveryDay)
                .OrderByDescending(x => x.CreatedAt)
                .AsQueryable();

            var productTypes = await _db
                .ProductType
                .Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToListAsync();

            var routes = await query.Select(x => new
            {
                x.Id,
                Dealer = x.Dealer.FullName,
                TotalCarts = x.Carts.Count,
                CompletedCarts = x.Carts.Count(y => y.Status != CartStatuses.Pending),
                TotalCollected = x.Carts.SelectMany(y => y.Products).Sum(y => y.SoldQuantity * y.SettedPrice),
                CreatedAt = x.CreatedAt.Date,
                ClientIds = x.Carts.Select(x => x.ClientId),
                Products = x.Carts.SelectMany(x => x.Products)
            }).ToListAsync();

            var clientIdList = routes.SelectMany(y => y.ClientIds).ToList();
            var daysList = routes.Select(x => x.CreatedAt).ToList();

            var transfersTotalCollected = await _db
                .Transfer
                .Where(x => daysList.Contains(x.CreatedAt.Date))
                .Where(x => clientIdList.Contains(x.ClientId))
                .Select(x => new
                {
                    x.ClientId,
                    x.Amount,
                    CreatedAt = x.CreatedAt.Date
                })
                .ToListAsync();

            response.Data = new GetDynamicDealerRoutesResponse
            {
                Routes = routes.Select(x => new GetDynamicDealerRoutesResponse.RouteItem
                {
                    Id = x.Id,
                    Dealer = x.Dealer,
                    TotalCarts = x.TotalCarts,
                    CompletedCarts = x.CompletedCarts,
                    TotalCollected = x.TotalCollected + transfersTotalCollected
                        .Where(t => t.CreatedAt == x.CreatedAt && x.ClientIds.Contains(t.ClientId))
                        .Sum(t => t.Amount),
                    CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy"),
                }).ToList()
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

            response.Message = Messages.Operations.RouteOpened();
            response.Data = new OpenNewResponse
            {
                Id = route.Id
            };

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
                    .ThenInclude(x => x.Client)
                .FirstOrDefaultAsync(x => x.Id == rq.RouteId && x.IsStatic);

            if (route is null)
                return response.SetError(Messages.Error.EntityNotFound("Ruta", true));

            var newClients = await _db
                .Client
                .Include(x => x.Address)
                .Where(x => rq.Clients.Contains(x.Id) && !route.Carts.Select(y => y.ClientId).Contains(x.Id))
                .ToListAsync();

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

            cartsToRemove.ForEach(x =>
            {
                x.Client.DealerId = null;
                x.Client.DeliveryDay = null;
                x.DeletedAt = DateTime.UtcNow;
            });
            newClients.ForEach(x =>
            {
                x.DealerId = route.DealerId;
                x.DeliveryDay = route.DeliveryDay;
            });

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

        public async Task<GenericResponse> AddClient(AddClientRequest rq)
        {
            var response = new GenericResponse();

            if (!response.Attach(await ValidateAddClient(rq)).Success)
                return response;

            decimal totalDebt = 0;

            var cart = new Cart
            {
                ClientId = rq.ClientId,
                RouteId = rq.RouteId,
                Products = new List<CartProduct>(),
                PaymentMethods = new List<CartPaymentMethod>(),
                Status = CartStatuses.Confirmed
            };

            _db.Cart.Add(cart);

            var client = await _db.Client
                .Include(x => x.Products)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(X => X.Id == rq.ClientId);

            if (client is null)
                return response.SetError(Messages.Error.EntityNotFound("Cliente", true));

            // Paid products
            foreach (var product in rq.Products)
            {
                var clientProduct = client.Products.First(x => x.ProductId == product.ProductTypeId);

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
                    SubscriptionQuantity = 0,
                    SettedPrice = clientProduct.Product.Price,
                });
            }

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

        #region Validations

        private async Task<GenericResponse> ValidateAddClient(AddClientRequest rq)
        {
            var response = new GenericResponse();

            if (!await _db.Route.AnyAsync(x => x.Id == rq.RouteId && !x.IsStatic))
                return response.SetError(Messages.Error.EntityNotFound("Bajada", true));

            if (rq.PaymentMethods.Any(x => x.Amount < 0))
                return response.SetError(Messages.Error.FieldGraterOrEqualThanZero("monto"));

            if (rq.Products.Any(x => x.ReturnedQuantity < 0 || x.SoldQuantity < 0))
                return response.SetError(Messages.Error.FieldGraterOrEqualThanZero("cantidad"));

            if (rq.Products.Any(x => x.ReturnedQuantity > int.MaxValue || x.SoldQuantity > int.MaxValue))
                return response.SetError(Messages.Error.FieldGraterThanMax("cantidad"));

            return response;
        }

        #endregion
    }
}
