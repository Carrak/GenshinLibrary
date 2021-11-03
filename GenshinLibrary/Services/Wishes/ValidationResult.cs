using GenshinLibrary.Services.Wishes.Filtering;

namespace GenshinLibrary.Services.Wishes
{
    public class ValidationResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }

        public ValidationResult(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }
    }
}
