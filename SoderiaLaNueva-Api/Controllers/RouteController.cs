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
        public async Task<GenericResponse<GetAllDealerStaticResponse>> GetAllDealerStaticRoutes()
        {
            return await _routeService.GetAllDealerStaticRoutes();
        }

        [HttpGet]
        public async Task<GenericResponse<GetStaticRouteResponse>> GetStaticRoute([FromQuery] GetStaticRouteRequest rq)
        {
            return await _routeService.GetStaticRoute(rq);
        }

        [HttpGet]
        public async Task<GenericResponse<GetStaticRouteClientsResponse>> GetStaticRouteClients([FromQuery] GetStaticRouteClientsRequest rq)
        {
            return await _routeService.GetStaticRouteClients(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse<CreateStaticResponse>> CreateStaticRoute([FromBody] CreateStaticRequest rq)
        {
            return await _routeService.CreateStaticRoute(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse> Delete([FromBody] DeleteStaticRequest rq)
        {
            return await _routeService.DeleteStaticRoute(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse<GetClientsListResponse>> GetClientsList([FromBody] GetClientsListRequest rq)
        {
            return await _routeService.GetClientsList(rq);
        }

        #endregion

        #region Dynamic Routes
        [HttpGet]
        public async Task<GenericResponse<GetDynamicRouteResponse>> GetDynamicRoute([FromQuery] GetDynamicRouteRequest rq)
        {
            return await _routeService.GetDynamicRoute(rq);
        }

        [HttpPost]
        [Authorize(Policy = Policies.Admin)]
        public async Task<GenericResponse<GetDynamicAdminRoutesResponse>> GetDynamicAdminRoutes([FromBody] GetDynamicAdminRoutesRequest rq)
        {
            return await _routeService.GetDynamicAdminRoutes(rq);
        }
        
        [HttpPost]
        public async Task<GenericResponse<GetDynamicDealerRoutesResponse>> GetDynamicDealerRoutes([FromBody] GetDynamicDealerRoutesRequest rq)
        {
            return await _routeService.GetDynamicDealerRoutes(rq);
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

        [HttpPost]
        public async Task<GenericResponse> AddClient([FromBody] AddClientRequest rq)
        {
            return await _routeService.AddClient(rq);
        }


        #endregion
    }
}
