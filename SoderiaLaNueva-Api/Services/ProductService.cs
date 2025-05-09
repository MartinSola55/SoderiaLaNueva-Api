﻿using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Helpers;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Product;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class ProductService(APIContext context, AuthService authService)
    {
        private readonly APIContext _db = context;
        private readonly AuthService _authService = authService;

        #region Combos
        public async Task<GenericResponse<GenericComboResponse>> GetComboProductTypes()
        {
            var response = new GenericResponse<GenericComboResponse>
            {
                Data = new GenericComboResponse
                {
                    Items = await _db.ProductType
                    .Select(x => new GenericComboResponse.Item
                    {
                        Id = x.Id,
                        Description = x.Name
                    })
                    .OrderBy(x => x.Description)
                    .ToListAsync()
                }
            };
            return response;
        }

        public async Task<GenericResponse<GenericComboResponse>> GetComboProducts()
        {
            var items = await _db.Product
                .Where(x => x.DeletedAt == null)
                .Select(x => new GenericComboResponse.Item
                {
                    Id = x.Id,
                    Description = $"{x.Name} - {Formatting.FormatCurrency(x.Price)}"
                })
                .ToListAsync();

            return new GenericResponse<GenericComboResponse>
            {
                Data = new GenericComboResponse
                {
                    Items = items
                    .OrderBy(x => x.Description)
                    .ToList()
                }
            };
        }
        #endregion

        #region CRUD
        public async Task<GenericResponse<GetAllResponse>> GetAll(GetAllRequest rq)
        {
            var query = _db
                .Product
                .Include(x => x.Type)
                .AsQueryable();

            query = FilterQuery(query, rq);
            query = OrderQuery(query, rq.ColumnSort, rq.SortDirection);

            if (rq.TypeIds != null && rq.TypeIds.Count > 0)
            {
                var types = await _db.ProductType
                    .Where(x => rq.TypeIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync();

                query = query.Where(x => types.Contains(x.Type.Id));
            }

            var response = new GenericResponse<GetAllResponse>
            {
                Data = new GetAllResponse
                {
                    TotalCount = await query.CountAsync(),
                    Products = await query.Select(x => new GetAllResponse.Item
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Price = x.Price,
                        Type = x.Type.Name,
                        CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        IsActive = x.DeletedAt == null,
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
            var product = await _db
                .Product
                .FirstOrDefaultAsync(x => x.DeletedAt == null && x.Id == rq.Id);

             if (product == null)
                return response.SetError(Messages.Error.EntityNotFound("Producto"));

            response.Data = new GetOneResponse()
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                TypeId = product.TypeId,
                CreatedAt = product.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            };

            return response;
        }

        public async Task<GenericResponse<CreateResponse>> Create(CreateRequest rq)
        {
            var response = new GenericResponse<CreateResponse>();

            // Create product
            Product product = new()
            {
                Name = rq.Name,
                Price = rq.Price,
                TypeId = rq.TypeId
            };

            // Validate request
            if (!ValidateFields(product))
                return response.SetError(Messages.Error.FieldsRequired());

            // Validate type
            if (!await _db.ProductType.AnyAsync(x => x.Id == rq.TypeId))
                return response.SetError(Messages.Error.EntityNotFound("Tipo de producto"));

            // Duplicated product?
            var duplicatedProduct = await _db
                .Product
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Name.ToLower() == rq.Name.ToLower() && x.Price == rq.Price && x.TypeId == rq.TypeId);

            if (duplicatedProduct != null)
                return response.SetError($"{Messages.Error.DuplicateEntity("producto y tipo")} ({(duplicatedProduct.DeletedAt == null ? "Activo" : "Inactivo")})");

            // Save changes
            _db.Product.Add(product);

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

            response.Message = Messages.CRUD.EntityCreated("Producto");
            response.Data = new CreateResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                TypeId = product.TypeId,
            };
            return response;
        }

        public async Task<GenericResponse<UpdateResponse>> Update(UpdateRequest rq)
        {
            var response = new GenericResponse<UpdateResponse>();

            if (!_authService.IsAdmin())
                return response.SetError(Messages.Error.Unauthorized());

            var product = await _db
                .Product
                .Include(x => x.Type)
                .FirstOrDefaultAsync(x => x.DeletedAt == null && x.Id == rq.Id);

            if (product == null)
                return response.SetError(Messages.Error.EntityNotFound("Producto"));

            // Update product data
            product.Name = rq.Name;
            product.Price = rq.Price;
            product.TypeId = rq.TypeId;
            product.UpdatedAt = DateTime.UtcNow;

            if (!ValidateFields(product))
                return response.SetError(Messages.Error.FieldsRequired());

            // Validate type
            if (await _db.ProductType.FirstOrDefaultAsync(x => x.Id == rq.TypeId) == null)
                return response.SetError(Messages.Error.EntityNotFound("Tipo de producto"));

            // Check if the name and type is duplicated
            if (await _db.Product.AnyAsync(x => x.DeletedAt == null && x.Name.ToLower() == rq.Name.ToLower() && x.TypeId == rq.TypeId && x.Id != rq.Id))
                return response.SetError(Messages.Error.DuplicateEntity("producto"));

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

            response.Message = Messages.CRUD.EntityUpdated("Producto");
            response.Data = new UpdateResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                TypeId = product.TypeId,
            };
            return response;
        }

        public async Task<GenericResponse> Delete(DeleteRequest rq)
        {
            var response = new GenericResponse();

            // Retrieve product
            var product = await _db
                .Product
                .FirstOrDefaultAsync(x => x.Id == rq.Id && x.DeletedAt == null);

            if (product == null)
                return response.SetError(Messages.Error.EntityNotFound("Producto"));

            var clients = await _db
                .Client
                .Include(x => x.Products)
                .Include(x => x.Subscriptions)
                    .ThenInclude(x => x.Subscription)
                        .ThenInclude(x => x.Products)
                .Where(x => x.Products.Any(p => p.ProductId == rq.Id))
                .ToListAsync();

            // Delete product
            product.DeletedAt = DateTime.UtcNow;

            // Delete client products and subscriptions
            foreach (var client in clients)
            {
                client.Products
                    .Where(x => x.ProductId == rq.Id)
                    .ToList()
                    .ForEach(x => x.DeletedAt = DateTime.UtcNow);

                client.Subscriptions
                    .Where(x => x.Subscription.Products.Any(p => p.ProductTypeId == product.TypeId))
                    .ToList()
                    .ForEach(x => x.DeletedAt = DateTime.UtcNow);
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

            response.Message = Messages.CRUD.EntityDeactivated("Producto");
            return response;
        }

        public async Task<GenericResponse> Activate(ActivateRequest rq)
        {
            var response = new GenericResponse();

            // Retrieve product
            var product = await _db
                .Product
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (product == null)
                return response.SetError(Messages.Error.EntityNotFound("Producto"));

            // Restore product
            product.DeletedAt = null;

            // Save changes
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityActivated("Producto");
            return response;
        }

        #endregion

        #region Search
        public async Task<GenericResponse<GetClientListResponse>> GetClientList(GetClientListRequest rq)
        {
            var clients = await _db
                .Client
                .Include(x => x.Products)
                .Include(x => x.Dealer)
                .Where(x => x.Products.Any(p => p.ProductId == rq.ProductId))
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new GetClientListResponse.ClientItem
                {
                    Name = x.Name,
                    Address = new GetClientListResponse.AddressItem
                    {
                        HouseNumber = x.Address.HouseNumber,
                        Road = x.Address.Road,
                        Neighbourhood = x.Address.Neighbourhood,
                        Suburb = x.Address.Suburb,
                        CityDistrict = x.Address.CityDistrict,
                        City = x.Address.City,
                        Town = x.Address.Town,
                        Village = x.Address.Village,
                        County = x.Address.County,
                        State = x.Address.State,
                        Country = x.Address.Country,
                        Postcode = x.Address.Postcode,
                        Lat = x.Address.Lat,
                        Lon = x.Address.Lon,
                    },
                    DealerName = x.Dealer.FullName,
                    DeliveryDay = x.DeliveryDay
                }).ToListAsync();

            return new GenericResponse<GetClientListResponse>
            {
                Data = new GetClientListResponse
                {
                    Clients = clients
                }
            };
        }

        public async Task<GenericResponse<GetSoldProductsByDateResponse>> GetSoldProductsByDate(GetSoldProductsByDateRequest rq)
        {
            var response = new GenericResponse<GetSoldProductsByDateResponse>();
            var cartProducts = await _db
                .CartProduct
                .Include(x => x.Type)
                .Where(x => x.Cart.CreatedAt.Date == rq.Date.Date)
                .Select(x => new
                {
                    x.ProductTypeId,
                    x.Type.Name,
                    x.SoldQuantity,
                    x.SubscriptionQuantity,
                    x.ReturnedQuantity
                })
                .ToListAsync();

            var productTypes = await _db
                .ProductType
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();

            var soldProducts = new List<GetSoldProductsByDateResponse.ProductItem>();

            foreach (var type in productTypes)
            {
                var productsByType = cartProducts.Where(x => x.ProductTypeId == type.Id).Sum(x => x.SoldQuantity);
                var subsProductsByType = cartProducts.Where(x => x.ProductTypeId == type.Id).Sum(x => x.SubscriptionQuantity);
                var returnedProductsByType = cartProducts.Where(x => x.ProductTypeId == type.Id).Sum(x => x.ReturnedQuantity);

                var soldProduct = new GetSoldProductsByDateResponse.ProductItem()
                {
                    Name = type.Name,
                    Sold = productsByType,
                    Returned = returnedProductsByType,
                };
                soldProduct.Sold += subsProductsByType;

                soldProducts.Add(soldProduct);
            }

            response.Data = new GetSoldProductsByDateResponse
            {
                Products = soldProducts
            };

            return response;
        }
        #endregion

        #region Validations
        private static bool ValidateFields(Product entity)
        {
            if (string.IsNullOrEmpty(entity.Name) || entity.Price < 0 || entity.Price > int.MaxValue)
                return false;

            return true;
        }

        #endregion

        #region Helpers
        private static IQueryable<Product> FilterQuery(IQueryable<Product> query, GetAllRequest rq)
        {
            if (rq.DateFrom.HasValue && rq.DateTo.HasValue && rq.DateFrom <= rq.DateTo)
            {
                var dateFromUTC = DateTime.SpecifyKind(rq.DateFrom.Value, DateTimeKind.Utc).Date;
                var dateToUTC = DateTime.SpecifyKind(rq.DateTo.Value, DateTimeKind.Utc).Date;

                query = query.Where(x => x.CreatedAt.Date >= dateFromUTC && x.CreatedAt.Date <= dateToUTC);
            }

            if (rq.Statuses.Count > 0)
            {
                if (rq.Statuses.Contains("active") && !rq.Statuses.Contains("inactive"))
                {
                    query = query.Where(x => x.DeletedAt == null);
                }
                else if (rq.Statuses.Contains("inactive") && !rq.Statuses.Contains("active"))
                {
                    query = query.Where(x => x.DeletedAt != null);
                }
            }
            else
            {
                query = query.Where(x => x.DeletedAt == null);
            }

            return query;
        }

        private static IQueryable<Product> OrderQuery(IQueryable<Product> query, string column, string direction)
        {
            return column switch
            {
                "createdAt" => direction == "asc" ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
                "name" => direction == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                _ => query.OrderBy(x => x.Name),
            };
        }
        #endregion
    }
}
