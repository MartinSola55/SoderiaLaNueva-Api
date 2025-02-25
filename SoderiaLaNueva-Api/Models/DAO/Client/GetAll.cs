namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class GetAllRequest : GenericGetAllRequest
    {
        public string? Name { get; set; } = null!;
        public int? Id { get; set; }
        public List<string>? DealerIds { get; set; } = null!;
        public List<int>? FilterClients { get; set; } = null!;
        public bool Unassigned { get; set; } = false;
    }

    public class GetAllResponse : GenericGetAllResponse
    {
        public List<Item> Clients { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public AddressItem Address { get; set; } = null!;
            public string Phone{ get; set; } = null!;
            public decimal Debt { get; set; }
            public string DealerName { get; set; } = null!;
            public int? DeliveryDay { get; set; }
        }
        public class AddressItem
        {
            public string? HouseNumber { get; set; }
            public string? Road { get; set; }
            public string? Neighbourhood { get; set; }
            public string? Suburb { get; set; }
            public string? CityDistrict { get; set; }
            public string? City { get; set; }
            public string? Town { get; set; }
            public string? Village { get; set; }
            public string? County { get; set; }
            public string? State { get; set; }
            public string? Country { get; set; }
            public string? Postcode { get; set; }
            public string Lat { get; set; } = null!;
            public string Lon { get; set; } = null!;
        }
    }
}