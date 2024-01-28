using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace S3Playground;

internal class S3ClientUtility
{
    const string bucketName = "npawstraining";
    const string fileName = "movies.csv";

    public async Task UploadAsync()
    {
        IAmazonS3 s3Client = new AmazonS3Client();
        try
        {
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = fileName,
                //FilePath = fileName,
                InputStream = fileStream,
                ContentType = "text/csv"
            };
            var response = await s3Client.PutObjectAsync(request);
            Console.WriteLine("File uploaded successfully!");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
        }
    }

    public async Task DownloadAsync()
    {
        IAmazonS3 s3Client = new AmazonS3Client();
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            using var response = await s3Client.GetObjectAsync(request);
            using (var responseStream = response.ResponseStream)
            using (var fileStream = File.Create($"downloaded_{fileName}"))
            {
                await responseStream.CopyToAsync(fileStream);
            }
            Console.WriteLine($"File downloaded successfully!");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
        }
    }
}

