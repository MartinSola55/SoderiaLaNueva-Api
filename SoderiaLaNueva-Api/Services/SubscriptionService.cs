using Azure;
using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Subscription;
using SoderiaLaNueva_Api.Helpers;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class SubscriptionService(APIContext context)
    {
        private readonly APIContext _db = context;
        #region Combos
        public async Task<GenericResponse<GenericComboResponse>> GetComboSubscriptions()
        {

            var items = await _db.Subscription
                .Select(x => new GenericComboResponse.Item
                {
                    Id = x.Id.ToString(),
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
            var response = new GenericResponse<GetAllResponse>();

            var query = _db
                .Subscription
                .Include(x => x.Products)
                    .ThenInclude(x => x.ProductType)
                .AsQueryable();

            if (rq.DateFrom.HasValue && rq.DateTo.HasValue)
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
                    SubscriptionProductItems = x.Products.Select(x => new GetAllResponse.Item.SubscriptionProductItem
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
                        Id = p.ProductType.Id.ToString(),
                        Quantity = p.Quantity,
                    }).ToList(),
                })
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (subscription == null)
                return response.SetError(Messages.Error.EntityNotFound("Abono"));

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
                Products = rq.SubscriptionProducts.Where(x => x.Quantity > 0).Select(x => new SubscriptionProduct
                {
                    ProductTypeId = x.ProductTypeId,
                    Quantity = x.Quantity,
                }).ToList(),
            };

            // Validate request
            if (!ValidateFields(subscription))
                return response.SetError(Messages.Error.FieldsRequired());

            // Validate quantities
            var error = await ValidateQuantities(subscription);

            if (error != "")
                return response.SetError(error);

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
            subscription.UpdatedAt = DateTime.UtcNow;

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
            if (!ValidateFields(subscription))
                return response.SetError(Messages.Error.FieldsRequired());

            // Validate quantities
            var error = await ValidateQuantities(subscription);

            if (error != "")
                return response.SetError(error);
            
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
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (subscription == null)
                return response.SetError(Messages.Error.EntityNotFound("Abono"));

            subscription.DeletedAt = DateTime.UtcNow;

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


        #region 
        private static bool ValidateFields(Subscription entity)
        {
            if (string.IsNullOrEmpty(entity.Name) || entity.Price < 0 || entity.Products.Count == 0)
                return false;

            return true;
        }

        private async Task<string> ValidateQuantities(Subscription entity)
        {
            if (entity.Products.Any(x => x.Quantity < 0))
                return Messages.Error.FieldGraterOrEqualThanZero("cantidad");

            var productTypes = await _db
                .ProductType
                .Where(x => entity.Products.Select(x => x.ProductTypeId).Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (productTypes.Count != entity.Products.Count)
                return Messages.Error.EntityNotFound("Tipo de producto");

            return "";
        }

        #endregion

        #region Helpers
        private static IQueryable<Subscription> FilterQuery(IQueryable<Subscription> query, GetAllRequest rq)
        {
            if (rq.DateFrom <= rq.DateTo)
            {
                var dateFromUTC = DateTime.SpecifyKind(rq.DateFrom.Value, DateTimeKind.Utc).Date;
                var dateToUTC = DateTime.SpecifyKind(rq.DateTo.Value, DateTimeKind.Utc).Date;

                query = query.Where(x => x.CreatedAt.Date >= dateFromUTC && x.CreatedAt.Date <= dateToUTC);
            }

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