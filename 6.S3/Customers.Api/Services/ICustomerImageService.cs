using Amazon.S3;
using Amazon.S3.Model;

namespace Customers.Api.Services;

public interface ICustomerImageService
{
    Task<PutObjectResponse> UploadImageAsync(Guid id, IFormFile file);
    Task<GetObjectResponse> GetObjectAsync(Guid id);
    Task<DeleteObjectResponse> DeleteObjectAsync(Guid id);
}


public class CustomerImageService : ICustomerImageService
{
    const string _bucketName = "npawstraining";
    private readonly IAmazonS3 _s3;

    public CustomerImageService(IAmazonS3 s3)
    {
        _s3 = s3;
    }

    public async Task<GetObjectResponse> GetObjectAsync(Guid id)
    {
        var getObjectRequest = new GetObjectRequest 
        {
            BucketName = _bucketName,
            Key = $"image/{id}"
        };

        return await _s3.GetObjectAsync(getObjectRequest);
    }

    public async Task<PutObjectResponse> UploadImageAsync(Guid id, IFormFile file)
    {
        var putObjectRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = $"image/{id}",
            ContentType = file.ContentType,
            InputStream = file.OpenReadStream(),
            Metadata =
            {
                ["x-amz-meta-originalname"] = file.FileName,
                ["x-amz-meta-extension"] = Path.GetExtension(file.FileName),

            }
        };
        return await _s3.PutObjectAsync(putObjectRequest);
    }

    public async Task<DeleteObjectResponse> DeleteObjectAsync(Guid id)
    {
        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = $"image/{id}"
        };

        return await _s3.DeleteObjectAsync(deleteObjectRequest);
    }
}