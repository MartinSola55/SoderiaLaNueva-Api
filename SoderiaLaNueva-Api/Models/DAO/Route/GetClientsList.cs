﻿
namespace SoderiaLaNueva_Api.Models.DAO.Route
{
    public class GetClientsListRequest : GenericGetAllRequest
    {
        public int Id { get; set; }
    }

    public class GetClientsListResponse : GenericGetAllResponse
    {
        public List<ClientItem> Items { get; set; } = [];

        public class ClientItem
        {
            public string ClientId { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string Address { get; set; } = null!;
        }
    }
}
