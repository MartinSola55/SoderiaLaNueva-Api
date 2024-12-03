namespace SoderiaLaNueva_Api.Models.DAO
{
    public class GenericGetAllRequest
    {
        public bool Paginate { get; set; } = false;
        public int Page { get; set; } = 1;
        public string ColumnSort { get; set; } = "createdAt";
        public string SortDirection { get; set; } = "desc";
        public List<int>? SubsidiariesSelected { get; set; }

    }

    public class GenericGetAllResponse
    {
        public int TotalCount { get; set; } = 0;

    }
}
