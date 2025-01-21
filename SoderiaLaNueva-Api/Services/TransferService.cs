using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.Transfer;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class TransferService(APIContext context)
    {
        private readonly APIContext _db = context;

        #region CRUD
        public async Task<GenericResponse<GetAllResponse>> GetAll(GetAllRequest rq)
        {
            var response = new GenericResponse<GetAllResponse>();

            var query = _db
                .Transfer
                .Include(x => x.Client)
                    .ThenInclude(x => x.Dealer)
                .AsQueryable();

            query = FilterQuery(query, rq);
            query = OrderQuery(query, rq.ColumnSort, rq.SortDirection);

            response.Data = new GetAllResponse
            {
                TotalCount = await query.CountAsync(),
                Transfers = await query
                .Select(x => new GetAllResponse.Item
                {
                    Id = x.Id,
                    ClientName = x.Client.Name,
                    DealerName = x.Client.Dealer.FullName,
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

            if (rq.Amount <= 0)
                return response.SetError(Messages.Error.FieldGraterThanZero("monto"));

            var client = await _db
                .Client
                .Include(x => x.Dealer)
                .FirstOrDefaultAsync(x => x.Id == rq.ClientId);

            if (client == null)
                return response.SetError(Messages.Error.EntityNotFound("Cliente"));

            var transfer = new Transfer
            {
                ClientId = rq.ClientId,
                Amount = rq.Amount,
            };
            client.Debt -= transfer.Amount;

            _db.Transfer.Add(transfer);

            // Save changes
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityCreated("Transferencia", true);
            response.Data = new CreateResponse
            {
                Id = transfer.Id,
                ClientName = client.Name,
                Address = client.Address,
                Phone = client.Phone,
                Amount = transfer.Amount,
                DealerName = client.Dealer?.FullName ?? "",
            };
            return response;
        }

        public async Task<GenericResponse<UpdateResponse>> Update(UpdateRequest rq)
        {
            var response = new GenericResponse<UpdateResponse>();

            var transfer = await _db
                .Transfer
                .Include(x => x.Client)
                    .ThenInclude(x => x.Dealer)
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (transfer == null)
                return response.SetError(Messages.Error.EntityNotFound("Transferencia"));
            else if (rq.Amount <= 0)
                return response.SetError(Messages.Error.FieldGraterThanZero("monto"));

            // Update client debt
            transfer.Client.Debt += transfer.Amount - rq.Amount;

            // Update transfer
            transfer.Amount = rq.Amount;
            transfer.UpdatedAt = DateTime.UtcNow.AddHours(-3);

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
                Id = transfer.Id,
                ClientName = transfer.Client.Name,
                DealerName = transfer.Client.Dealer.FullName,
                Amount = transfer.Amount
            };
            response.Message = Messages.CRUD.EntityUpdated("Transferencia");
            return response;
        }

        public async Task<GenericResponse> Delete(DeleteRequest rq)
        {
            var response = new GenericResponse();

            var transfer = await _db
                .Transfer
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (transfer == null)
                return response.SetError(Messages.Error.EntityNotFound("Transferencia"));

            // Update client debt
            transfer.Client.Debt += transfer.Amount;
            // Delete transfer
            transfer.DeletedAt = DateTime.UtcNow.AddHours(-3);

            // Save changes
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityDeleted("Transferencia");
            return response;
        }
        #endregion

        #region Helpers
        private static IQueryable<Transfer> FilterQuery(IQueryable<Transfer> query, GetAllRequest rq)
        {
            if (rq.DateFrom <= rq.DateTo)
            {
                var dateFromUTC = DateTime.SpecifyKind(rq.DateFrom, DateTimeKind.Utc).Date;
                var dateToUTC = DateTime.SpecifyKind(rq.DateTo, DateTimeKind.Utc).Date;

                query = query.Where(x => x.CreatedAt.Date >= dateFromUTC && x.CreatedAt.Date <= dateToUTC);
            }

            return query;
        }

        private static IQueryable<Transfer> OrderQuery(IQueryable<Transfer> query, string column, string direction)
        {
            return column switch
            {
                "amount" => direction == "asc" ? query.OrderBy(x => x.Amount) : query.OrderByDescending(x => x.Amount),
                "clientName" => direction == "asc" ? query.OrderBy(x => x.Client.Name) : query.OrderByDescending(x => x.Client.Name),
                "createdAt" => direction == "asc" ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt),
            };
        }
        #endregion
    }
}