﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Client;
using SoderiaLaNueva_Api.Models.Constants;

namespace SoderiaLaNueva_Api.Controllers
{
    public class ClientController(ClientService clientService) : BaseController
    {
        private readonly ClientService _clientService = clientService;

        #region CRUD
        [HttpGet]
        public GenericResponse<GetFormDataResponse> GetFormData()
        {
            return _clientService.GetFormData();
        }

        [HttpPost]
        public async Task<GenericResponse<GetAllResponse>> GetAll([FromBody] GetAllRequest rq)
        {
            return await _clientService.GetAll(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetOneResponse>> GetOneById([FromQuery] GetOneRequest rq)
        {
            return await _clientService.GetOneById(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse<CreateResponse>> Create([FromBody] CreateRequest rq)
        {
            return await _clientService.Create(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse> Delete([FromBody] DeleteRequest rq)
        {
            return await _clientService.Delete(rq);
        }

        #endregion

        #region Update methods
        [HttpPost]
        public async Task<GenericResponse<UpdateClientDataResponse>> UpdateClientData([FromBody] UpdateClientDataRequest rq)
        {
            return await _clientService.UpdateClientData(rq);
        }

        [HttpPost]
        public async Task<GenericResponse<UpdateClientProductsResponse>> UpdateClientProducts([FromBody] UpdateClientProductsRequest rq)
        {
            return await _clientService.UpdateClientProducts(rq);
        }

        [HttpPost]
        public async Task<GenericResponse> UpdateClientSubscriptions([FromBody] UpdateClientSubscriptionsRequest rq)
        {
            return await _clientService.UpdateClientSubscriptions(rq);
        }
        #endregion
    }
}
