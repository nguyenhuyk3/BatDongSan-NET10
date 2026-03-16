namespace VTTGROUP.Domain.Model
{
    public class ResultModel
    {
        public string Id { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public decimal GiaTri { get; set; }

        public object? Data { get; set; }
        public ResultModel() { }

        public ResultModel(string id, bool status, string message)
        {
            Id = id;
            Status = status;
            Message = message;
        }
        public ResultModel(decimal giaTri, bool status, string message)
        {
            GiaTri = giaTri;
            Status = status;
            Message = message;
        }
        public ResultModel(bool status, string message)
        {
            Status = status;
            Message = message;
        }

        public static ResultModel Success(string message = "Thành công")
            => new ResultModel(true, message);

        public static ResultModel SuccessWithId(string id, string message = "Thành công")
       => new ResultModel(id, true, message);
        public static ResultModel SuccessWithGiaTri(decimal id, string message = "Thành công")
      => new ResultModel(id, true, message);

        public static ResultModel SuccessWithData(object data, string message = "Thành công")
       => new ResultModel(true, message) { Data = data };

        public static ResultModel Fail(string message = "Thất bại")
            => new ResultModel(false, message);
    }
}
