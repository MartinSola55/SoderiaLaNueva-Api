using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Expense;
using SoderiaLaNueva_Api.Models.Constants;

namespace SoderiaLaNueva_Api.Controllers
{
    [Authorize(Policy = Policies.Admin)]
    public class ExpenseController(ExpenseService expenseService) : BaseController
    {
        private readonly ExpenseService _expenseService = expenseService;

        #region CRUD
        [HttpPost]
        public async Task<GenericResponse<GetAllResponse>> GetAll([FromBody] GetAllRequest rq)
        {
            return await _expenseService.GetAll(rq);
        }

        [HttpPost]
        public async Task<GenericResponse<CreateResponse>> Create([FromBody] CreateRequest rq)
        {
            return await _expenseService.Create(rq);
        }

        [HttpPost]
        public async Task<GenericResponse<UpdateResponse>> Update([FromBody] UpdateRequest rq)
        {
            return await _expenseService.Update(rq);
        }

        [HttpPost]
        public async Task<GenericResponse> Delete([FromBody] DeleteRequest rq)
        {
            return await _expenseService.Delete(rq);
        }
        #endregion

        #region Stats
        [HttpGet]
        public async Task<GenericResponse<GetExpensesByDateResponse>> GetExpensesByDate([FromQuery] GetExpensesByDateRequest rq)
        {
            return await _expenseService.GetExpensesByDate(rq);
        }
        #endregion
    }
}
