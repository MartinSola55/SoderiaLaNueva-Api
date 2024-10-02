namespace SoderiaLaNueva_Api.Models.DAO
{
    public class GenericResponse<T>
    {
        public T? Data { get; set; }
        public string Message { get; set; } = null!;
        public ErrorResponse? Error { get; set; }
        public bool Success => Error == null;

        public class ErrorResponse
        {
            public string Message { get; set; } = null!;
            public int Code { get; set; }
        }

        public GenericResponse<T> SetError(string error)
        {
            Error = new ErrorResponse
            {
                Message = error,
                Code = 400
            };
            return this;
        }
    }

    public class GenericResponse
    {
        public object Data { get; set; } = new();
        public string Message { get; set; } = null!;
        public ErrorResponse? Error { get; set; }
        public bool Success => Error == null;

        public class ErrorResponse
        {
            public string Message { get; set; } = null!;
            public int Code { get; set; }
        }

        public GenericResponse SetError(string error)
        {
            Error = new ErrorResponse
            {
                Message = error,
                Code = 400
            };
            return this;
        }
    }
}
