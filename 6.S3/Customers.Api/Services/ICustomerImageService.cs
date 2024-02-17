using Amazon.S3.Model;

namespace Customers.Api.Services
{
    public interface ICustomerImageService
    {
        Task<GetObjectResponse> GetObjectAsync(Guid id);
        Task<PutObjectResponse> UploadImageAsync(Guid id, IFormFile file);
        Task<DeleteObjectResponse> DeleteObjectAsync(Guid id);
    }
}