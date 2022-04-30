namespace GenshinLibrary.Services.Wishes
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }

        public Result(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        public Result(T value, bool isSuccess, string errorMessage) : base(isSuccess, errorMessage)
        {
            Value = value;
        }
    }
}
