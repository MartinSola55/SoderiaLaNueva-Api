using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Product;
using SoderiaLaNueva_Api.Models.Constants;

namespace SoderiaLaNueva_Api.Controllers
{
    public class ProductController(ProductService productService) : BaseController
    {
        private readonly ProductService _productService = productService;

        [HttpGet]
        public async Task<GenericResponse<GetFormDataResponse>> GetFormData()
        {
            return await _productService.GetFormData();
        }

        #region CRUD

        [HttpPost]
        public async Task<GenericResponse<GetAllResponse>> GetAll([FromBody] GetAllRequest rq)
        {
            return await _productService.GetAll(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetOneResponse>> GetOneById([FromQuery] GetOneRequest rq)
        {
            return await _productService.GetOneById(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse<CreateResponse>> Create([FromBody] CreateRequest rq)
        {
            return await _productService.Create(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse<UpdateResponse>> Update([FromBody] UpdateRequest rq)
        {
            return await _productService.Update(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse> Delete([FromBody] DeleteRequest rq)
        {
            return await _productService.Delete(rq);
        }

        #endregion
    }
}
