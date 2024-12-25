using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Expense;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class ExpenseService(APIContext context, TokenService tokenService)
    {
        private readonly APIContext _db = context;
        private readonly Token _token = tokenService.GetToken();

        #region Methods
        public async Task<GenericResponse<GetAllResponse>> GetAll(GetAllRequest rq)
        {
            var response = new GenericResponse<GetAllResponse>();

            var query = _db
                .Expense
                .Include(x => x.Dealer)
                .AsQueryable();

            query = FilterQuery(query, rq);
            query = OrderQuery(query, rq.ColumnSort, rq.SortDirection);

            response.Data = new GetAllResponse
            {
                TotalCount = await query.CountAsync(),
                Expenses = await query
                .Select(x => new GetAllResponse.Item
                {
                    Id = x.Id,
                    Description = x.Description,
                    DealerId = x.DealerId,
                    Amount = x.Amount,
                    CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .Skip((rq.Page - 1) * Pagination.DefaultPageSize)
                .Take(Pagination.DefaultPageSize)
                .ToListAsync()
            };

            return response;
        }

        public async Task<GenericResponse<CreateResponse>> Create(CreateRequest rq)
        {
            var response = new GenericResponse<CreateResponse>();

            if (rq.DealerId == null)
                return response.SetError(Messages.Error.EntityNotFound("Repartidor"));
            if (rq.Amount <= 0)
                return response.SetError(Messages.Error.FieldGraterThanZero("monto"));

            var expense = new Expense
            {
                DealerId = rq.DealerId,
                Description = rq.Description,
                Amount = rq.Amount
            };

            _db.Expense.Add(expense);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityCreated("Gasto");
            response.Data = new CreateResponse
            {
                Id = expense.Id,
                DealerId = expense.DealerId,
                Description = expense.Description,
                Amount = expense.Amount
            };
            return response;
        }

        public async Task<GenericResponse<UpdateResponse>> Update(UpdateRequest rq)
        {
            var response = new GenericResponse<UpdateResponse>();

            var expense = await _db
                .Expense
                .Include(x => x.Dealer)
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (expense == null)
                return response.SetError(Messages.Error.EntityNotFound("Gasto"));
            else if (rq.Amount <= 0)
                return response.SetError(Messages.Error.FieldGraterThanZero("monto"));


            if (expense.DealerId != rq.DealerId)
            {
                if (!await _db.User.AnyAsync(x => x.Id == rq.DealerId))
                    return response.SetError(Messages.Error.EntityNotFound("Repartidor"));

                expense.DealerId = rq.DealerId;
            }

            expense.Amount = rq.Amount;
            expense.Description = rq.Description;
            expense.UpdatedAt = DateTime.UtcNow;

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
                Id = expense.Id,
                DealerId = expense.DealerId,
                Description = expense.Description,
                Amount = expense.Amount
            };
            response.Message = Messages.CRUD.EntityUpdated("Gasto");
            return response;
        }

        public async Task<GenericResponse> Delete(DeleteRequest rq)
        {
            var response = new GenericResponse();

            var expense = await _db
                .Expense
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (expense == null)
                return response.SetError(Messages.Error.EntityNotFound("Gasto"));

            expense.DeletedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityDeleted("Gasto");
            return response;
        }
        #endregion

        #region Helpers
        private static IQueryable<Expense> FilterQuery(IQueryable<Expense> query, GetAllRequest rq)
        {
            if (rq.DateFrom <= rq.DateTo)
            {
                var dateFromUTC = DateTime.SpecifyKind(rq.DateFrom, DateTimeKind.Utc).Date;
                var dateToUTC = DateTime.SpecifyKind(rq.DateTo, DateTimeKind.Utc).Date;

                query = query.Where(x => x.CreatedAt.Date >= dateFromUTC && x.CreatedAt.Date <= dateToUTC);
            }

            return query;
        }

        private static IQueryable<Expense> OrderQuery(IQueryable<Expense> query, string column, string direction)
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