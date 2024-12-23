using Microsoft.EntityFrameworkCore;
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
            return new GenericResponse<GenericComboResponse>
            {
                Data = new GenericComboResponse
                {
                    Items = await _db.Product
                    .Select(x => new GenericComboResponse.Item
                    {
                        Id = x.Id,
                        Description = $"{x.Name} - {Formatting.FormatCurrency(x.Price)}"
                    })
                    .OrderBy(x => x.Description)
                    .ToListAsync()
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
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

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

            // Create product and assign role
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

            // Duplicate product
            if (await _db.Product.AnyAsync(x => x.Name.ToLower() == rq.Name.ToLower() && x.TypeId == rq.TypeId))
                return response.SetError(Messages.Error.DuplicateEntity("producto y tipo"));

            // Save changes
            await _db.Product.AddAsync(product);
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
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

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
            if (await _db.Product.AnyAsync(x => x.Name.ToLower() == rq.Name.ToLower() && x.TypeId == rq.TypeId && x.Id != rq.Id))
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
            var product = await _db.Product.FirstOrDefaultAsync(x => x.Id == rq.Id);
            if (product == null)
                return response.SetError(Messages.Error.EntityNotFound("Producto"));

            // Delete product
            product.DeletedAt = DateTime.UtcNow;

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

            response.Message = Messages.CRUD.EntityDeleted("Producto");
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

            return query;
        }

        private static IQueryable<Product> OrderQuery(IQueryable<Product> query, string column, string direction)
        {
            return column switch
            {
                "createdAt" => direction == "asc" ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt),
            };
        }
        #endregion
    }
}
