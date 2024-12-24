using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Models.DAO.User;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace SoderiaLaNueva_Api.Services
{
    public class UserService(APIContext context, UserManager<ApiUser> userManager, TokenService tokenService, AuthService authService)
    {
        private readonly APIContext _db = context;
        private readonly UserManager<ApiUser> _userManager = userManager;
        private readonly Token _token = tokenService.GetToken();
        private readonly AuthService _authService = authService;

        #region Methods
        public async Task<GenericResponse<GenericComboResponse>> GetComboRoles()
        {
            var response = new GenericResponse<GenericComboResponse>
            {
                Data = new GenericComboResponse
                {
                    Items = await _db.Roles
                    .Select(x => new GenericComboResponse.Item
                    {
                        Id = x.Id,
                        Description = x.Name
                    })
                    .ToListAsync()
                }
            };
            return response;
        }

        public async Task<GenericResponse<GetAllResponse>> GetAll(GetAllRequest rq)
        {
            var query = _db.User.AsQueryable();

            query = FilterQuery(query, rq);
            query = OrderQuery(query, rq.ColumnSort, rq.SortDirection);

            if (!_authService.IsAdmin())
                query = query.Where(x => x.Id == _token.UserId);

            if (rq.Roles != null && rq.Roles.Count > 0)
            {
                var roles = await _db.Roles
                    .Where(x => rq.Roles.Contains(x.Name))
                    .Select(x => x.Id)
                    .ToListAsync();

                query = query.Where(x => roles.Contains(x.Role.Id));
            }

            var response = new GenericResponse<GetAllResponse>
            {
                Data = new GetAllResponse
                {
                    TotalCount = await query.CountAsync(),
                    Users = await query.Select(x => new GetAllResponse.Item
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        Email = x.Email,
                        PhoneNumber = x.PhoneNumber,
                        CreatedAt = x.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                        Role = x.Role.Name
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
            var user = await _db
                .User
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

             if (user == null)
                return response.SetError(Messages.Error.EntityNotFound("Usuario"));

            response.Data = new GetOneResponse()
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                Role = user.Role.Name
            };

            return response;
        }

        public async Task<GenericResponse<CreateResponse>> Create(CreateRequest rq)
        {
            var response = new GenericResponse<CreateResponse>();

            // Create user and assign role
            ApiUser user = new()
            {
                Email = rq.Email,
                FullName = rq.FullName,
                UserName = rq.Email,
                EmailConfirmed = true,
                PhoneNumber = rq.PhoneNumber,
                RoleId = rq.RoleId
            };

            // Validate request
            if (!ValidateFields(user))
                return response.SetError(Messages.Error.FieldsRequired());

            if (string.IsNullOrEmpty(rq.Email) || !ValidateEmail(rq.Email))
                return response.SetError(Messages.Error.InvalidEmail());

            if (!ValidatePhone(rq.PhoneNumber))
                return response.SetError(Messages.Error.InvalidPhone());

            if (string.IsNullOrEmpty(rq.Password) || !ValidatePassword(rq.Password))
                return response.SetError(Messages.Error.InvalidPassword());

            // Validate role
            if (await _db.Role.FirstOrDefaultAsync(x => x.Id == rq.RoleId) == null)
                return response.SetError(Messages.Error.EntityNotFound("Rol"));

            if (await _userManager.FindByEmailAsync(rq.Email) != null)
                return response.SetError(Messages.Error.DuplicateEmail());

            // Save changes
            try
            {
                await _db.Database.BeginTransactionAsync();

                // Add user
                var result = _userManager.CreateAsync(user, rq.Password).GetAwaiter().GetResult();

                if (!result.Succeeded)
                {
                    await _db.Database.RollbackTransactionAsync();
                    return response.SetError(Messages.Error.SaveEntity("el usuario"));
                }

                // Save changes
                await _db.SaveChangesAsync();
             
                await _db.SaveChangesAsync();
                await _db.Database.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _db.Database.RollbackTransactionAsync();
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityCreated("Usuario");
            response.Data = new CreateResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = rq.RoleId,
            };
            return response;
        }

        public async Task<GenericResponse<UpdateResponse>> Update(UpdateRequest rq)
        {
            var response = new GenericResponse<UpdateResponse>();

            if (!_authService.IsAdmin() && rq.Id != _token.UserId)
                return response.SetError(Messages.Error.Unauthorized());

            var user = await _db
                .User
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == rq.Id);

            if (user == null)
                return response.SetError(Messages.Error.EntityNotFound("Usuario"));

            if (!ValidatePhone(rq.PhoneNumber))
                return response.SetError(Messages.Error.InvalidPhone());

            if (_authService.IsAdmin())
            {
                // Validate email
                if (!ValidateEmail(rq.Email))
                    return response.SetError(Messages.Error.InvalidEmail());

                // Validate role
                if (await _db.Role.FirstOrDefaultAsync(x => x.Id == rq.RoleId) == null)
                    return response.SetError(Messages.Error.EntityNotFound("Rol"));

                // Check if the new email is already in use by another user
                if (await _db.Users.AnyAsync(x => !string.IsNullOrEmpty(x.Email) && x.Email.ToLower() == rq.Email.ToLower() && x.Id != rq.Id))
                    return response.SetError(Messages.Error.DuplicateEmail());

                // Update user data
                user.FullName = rq.FullName;
            }
            // Update user data
            user.PhoneNumber = rq.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            if (!ValidateFields(user))
                return response.SetError(Messages.Error.FieldsRequired());

            // Save changes
            try
            {
                await _db.Database.BeginTransactionAsync();

                // Change email and username. Must be donde inside a transaction
                await _userManager.SetEmailAsync(user, rq.Email);
                await _userManager.SetUserNameAsync(user, rq.Email);

                await _db.SaveChangesAsync();
                await _db.Database.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _db.Database.RollbackTransactionAsync();
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityUpdated("Usuario");
            response.Data = new UpdateResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.Name
            };
            return response;
        }

        public async Task<GenericResponse> Delete(DeleteRequest rq)
        {
            var response = new GenericResponse();

            // Validate request
            if (string.IsNullOrEmpty(rq.Id))
                return response.SetError(Messages.Error.FieldsRequired());

            // Retrieve user
            var user = await _db.User.FirstOrDefaultAsync(x => x.Id == rq.Id);
            if (user == null)
                return response.SetError(Messages.Error.EntityNotFound("Usuario"));

            // Delete user
            user.DeletedAt = DateTime.UtcNow;

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

            response.Message = Messages.CRUD.EntityDeleted("Usuario");
            return response;
        }

        public async Task<GenericResponse> UpdatePassword(UpdatePasswordRequest rq)
        {
            var response = new GenericResponse();

            if (!_authService.IsAdmin() && rq.Id != _token.UserId)
                return response.SetError(Messages.Error.Unauthorized());

            // Validate request
            if (string.IsNullOrEmpty(rq.Password))
                return response.SetError(Messages.Error.FieldsRequired());

            if (!ValidatePassword(rq.Password))
                return response.SetError(Messages.Error.InvalidPassword());

            // Retrieve user
            var userId = string.IsNullOrEmpty(rq.Id) ? _token.UserId : rq.Id;
            var user = await _db
                .User
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return response.SetError(Messages.Error.EntityNotFound("Usuario"));

            try
            {
                await _db.Database.BeginTransactionAsync();

                // Update password. Must be done inside a transaction
                var remove = await _userManager.RemovePasswordAsync(user);
                var add = await _userManager.AddPasswordAsync(user, rq.Password);

                if (!remove.Succeeded || !add.Succeeded)
                    return response.SetError(Messages.Error.SaveEntity("la contraseña"));

                await _db.SaveChangesAsync();
                await _db.Database.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _db.Database.RollbackTransactionAsync();
                return response.SetError(Messages.Error.Exception());
            }

            response.Message = Messages.CRUD.EntityUpdated("Contraseña");
            return response;
        }

        #endregion

        #region Validations

        private static bool ValidateFields(ApiUser entity)
        {
            if (string.IsNullOrEmpty(entity.FullName) || string.IsNullOrEmpty(entity.Email) || string.IsNullOrEmpty(entity.PhoneNumber))
                return false;

            return true;
        }

        private static bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            string allowedChars = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit) || !password.Any(allowedChars.Contains))
                return false;

            return true;
        }

        private static bool ValidateEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

        private static bool ValidatePhone(string phone)
        {
            return new PhoneAttribute().IsValid(phone);
        }

        #endregion

        #region Helpers
        private static IQueryable<ApiUser> FilterQuery(IQueryable<ApiUser> query, GetAllRequest rq)
        {
            if (rq.DateFrom.HasValue && rq.DateTo.HasValue && rq.DateFrom <= rq.DateTo)
            {
                var dateFromUTC = DateTime.SpecifyKind(rq.DateFrom.Value, DateTimeKind.Utc).Date;
                var dateToUTC = DateTime.SpecifyKind(rq.DateTo.Value, DateTimeKind.Utc).Date;

                query = query.Where(x => x.CreatedAt.Date >= dateFromUTC && x.CreatedAt.Date <= dateToUTC);
            }

            return query;
        }

        private static IQueryable<ApiUser> OrderQuery(IQueryable<ApiUser> query, string column, string direction)
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
