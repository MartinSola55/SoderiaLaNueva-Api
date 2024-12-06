using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SoderiaLaNueva_Api.Models.DAO;
using SoderiaLaNueva_Api.Services;
using SoderiaLaNueva_Api.Models.DAO.Route;
using SoderiaLaNueva_Api.Models.Constants;

namespace SoderiaLaNueva_Api.Controllers
{
    public class RouteController(RouteService routeService) : BaseController
    {
        private readonly RouteService _routeService = routeService;

        #region Static Routes
        [HttpGet]
        public async Task<GenericResponse<GetAllStaticResponse>> GetAllStaticRoutes([FromQuery] GetAllStaticRequest rq)
        {
            return await _routeService.GetAllStaticRoutes(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetStaticRouteResponse>> GetStaticRoute([FromQuery] GetStaticRouteRequest rq)
        {
            return await _routeService.GetStaticRoute(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse<CreateStaticResponse>> CreateStaticRoute([FromBody] CreateStaticRequest rq)
        {
            return await _routeService.CreateStaticRoute(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse> DeleteStaticRoute([FromBody] DeleteStaticRequest rq)
        {
            return await _routeService.DeleteStaticRoute(rq);
        }

        #endregion

        #region Dynamic Routes
        [HttpGet]
        public async Task<GenericResponse<GetDynamicRouteResponse>> GetDynamicRoute([FromQuery] GetDynamicRouteRequest rq)
        {
            return await _routeService.GetDynamicRoute(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetDynamicRoutesResponse>> GetDynamicRoutes([FromQuery] GetDynamicRoutesRequest rq)
        {
            return await _routeService.GetDynamicRoutes(rq);
        }

        [HttpPost]
        public async Task<GenericResponse<OpenNewResponse>> OpenNew([FromBody] OpenNewRequest rq)
        {
            return await _routeService.OpenNew(rq);
        }

        [HttpPost]
        public async Task<GenericResponse> Close([FromBody] CloseRequest rq)
        {
            return await _routeService.Close(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse> UpdateClients([FromBody] UpdateClientsRequest rq)
        {
            return await _routeService.UpdateClients(rq);
        }

        #endregion
    }
}
