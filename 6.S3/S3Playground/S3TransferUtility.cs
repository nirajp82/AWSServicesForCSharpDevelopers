using Amazon.S3;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3Playground;

//The TransferUtility runs on top of the low-level API. For putting and getting objects into S3,
//I would recommend using this API. It is a simple interface for handling the most common uses of S3.
//The biggest benefit comes with putting objects.
//For example, TransferUtility detects if a file is large and switches into multipart upload mode.
//The multipart upload gives the benefit of better performance as the parts can be uploaded simultaneously as well,
//and if there is an error, only the individual part has to be retried.
//Here are examples showing the same operations above in the low-level API.
internal class S3TransferUtility
{
    const string bucketName = "npawstraining";
    const string fileName = "movies.csv";

    public async Task UploadAsync()
    {
        IAmazonS3 s3Client = new AmazonS3Client();
        using var transferUtility = new TransferUtility(s3Client);
        try
        {
            await transferUtility.UploadAsync(fileName, bucketName, fileName);
            Console.WriteLine("File uploaded successfully.");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("Error uploading file: {0}", e.Message);
        }
    }

    public async Task DownloadAsync()
    {
        IAmazonS3 s3Client = new AmazonS3Client();
        using var transferUtility = new TransferUtility(s3Client);
        try
        {
            await transferUtility.DownloadAsync(fileName, bucketName, fileName);
            Console.WriteLine("File downloaded successfully.");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("Error uploading file: {0}", e.Message);
        }
    }
}
