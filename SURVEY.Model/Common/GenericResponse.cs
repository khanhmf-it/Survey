namespace SURVEY.Model.Common
{
    public class GenericResponse<T>
    {
        public bool Success { get; set; }

        public int? Status { get; set; }

        public string Message { get; set; }

        public string Error { get; set; }

        public T Data { get; set; }
        public GenericResponse()
        {
        }

        public GenericResponse(int? status, T data, string message = null)
        {
            Status = status;
            Data = data;
            Message = message;
        }

        public GenericResponse(int? status, string error = null, string message = null)
        {
            Error = error;
            Status = status;
            Message = message;
        }

        public static GenericResponse ResultWithData(T data, string message = null)
        {
            return new GenericResponse
            {
                Data = data,
                Message = message,
                Success = true
            };
        }

        public static GenericResponse ResultWithError(int? status = null, string error = null, string message = null, object data = null)
        {
            return new GenericResponse
            {
                Status = status,
                Error = error,
                Message = message,
                Success = false
            };
        }
    }
    public class GenericResponse
    {
        public bool Success { get; set; }

        public int? Status { get; set; }

        public string Message { get; set; }

        public string Error { get; set; }

        public object Data { get; set; }

        public GenericResponse()
        {
        }

        public GenericResponse(int? status, object data, string message = null)
        {
            Status = status;
            Data = data;
            Message = message;
        }

        public GenericResponse(int? status, string error = null, string message = null)
        {
            Error = error;
            Status = status;
            Message = message;
        }

        public GenericResponse(int? status, string error = null, string message = null, object data = null)
        {
            Error = error;
            Status = status;
            Message = message;
            Data = data;
        }

        public static GenericResponse ResultWithData(object data, string message = null)
        {
            return new GenericResponse
            {
                Data = data,
                Message = message,
                Success = true
            };
        }

        public static GenericResponse ResultWithError(int? status = null, string error = null, string message = null, object data = null)
        {
            return new GenericResponse
            {
                Status = status,
                Error = error,
                Message = message,
                Success = false
            };
        }
    }
}
