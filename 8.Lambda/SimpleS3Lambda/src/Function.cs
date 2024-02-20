using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleS3Lambda
{
    public class Function
    {
        IAmazonS3 S3Client { get; set; }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            S3Client = new AmazonS3Client();
        }

        ///// <summary>
        ///// Constructs an instance with a preconfigured S3 client. This can be used for testing outside of the Lambda environment.
        ///// </summary>
        ///// <param name="s3Client"></param>
        //public Function(IAmazonS3 s3Client)
        //{
        //    this.S3Client = s3Client;
        //}

        /// <summary>
        /// This method is responsible for handling S3 events triggered by changes in the S3 bucket. 
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// 
        //  It processes each event record and resizes images that have not been resized yet.
        /// </summary>
        public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            // Retrieve the list of S3 event records from the event, or create an empty list if null.
            var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();

            // Iterate through each S3 event record.
            foreach (var record in eventRecords)
            {
                // Retrieve the S3 event details from the record.
                var s3Event = record.S3;

                // If the S3 event details are null, skip to the next record.
                if (s3Event == null)
                {
                    continue;
                }

                try
                {
                    // Check if the object has already been resized.
                    var response = await this.S3Client.GetObjectMetadataAsync(s3Event.Bucket.Name, s3Event.Object.Key);
                    if (response.Metadata["x-amz-meta-resized"] == true.ToString())
                    {
                        // Log a message indicating the object has already been resized and skip to the next record.
                        context.Logger.LogInformation($"Item with key {s3Event.Object.Key} is already resized");
                        continue;
                    }

                    // Retrieve the content stream of the original object from S3.
                    await using var itemStream = await S3Client.GetObjectStreamAsync(s3Event.Bucket.Name,
                        s3Event.Object.Key, new Dictionary<string, object> { });

                    // Create a memory stream to store the resized image.
                    using var outStream = new MemoryStream();

                    var fileNameAndExtension = GetFileNameWithExtension(s3Event.Object.Key, context);

                    // Load the image from the stream, resize it, and save the resized image to the output stream.
                    using (var image = await Image.LoadAsync(itemStream))
                    {
                        // Resize the image to 500x500 pixels using Lanczos3 resampling algorithm.
                        image.Mutate(x => x.Resize(500, 500, KnownResamplers.Lanczos3));

                        // Retrieve the original name of the image from the metadata.
                        var originalName = response.Metadata["x-amz-meta-originalname"] ?? $"{fileNameAndExtension.Item1}.{fileNameAndExtension.Item2}";

                        // Save the resized image to the output stream with the original image format.
                        await image.SaveAsync(outStream, image.DetectEncoder(originalName));
                    }

                    // Put the resized image object back into the S3 bucket with updated metadata.
                    await S3Client.PutObjectAsync(new PutObjectRequest
                    {
                        BucketName = s3Event.Bucket.Name,
                        Key = s3Event.Object.Key,
                        Metadata = {
                            // Preserve the original name and extension in the metadata.
                            ["x-amz-meta-originalname"] = response.Metadata["x-amz-meta-originalname"] ?? fileNameAndExtension.Item1,
                            ["x-amz-meta-extension"] = response.Metadata["x-amz-meta-extension"]  ?? fileNameAndExtension.Item2,
                            // Set the flag indicating the image has been resized to true.
                            ["x-amz-meta-resized"] = true.ToString()
                        },
                        // Set the content type of the resized image to the original content type.
                        ContentType = response.Headers.ContentType,
                        // Set the input stream to the resized image stream.
                        InputStream = outStream
                    });

                    // Log a message indicating the successful resize operation.
                    context.Logger.LogInformation($"Resized image with key: {s3Event.Object.Key}");
                }
                catch (Exception e)
                {
                    // Log an error message if an exception occurs during processing.
                    context.Logger.LogError($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                    context.Logger.LogError(e.Message);
                    context.Logger.LogError(e.StackTrace);
                    throw; // Re-throw the exception to ensure it's propagated correctly.
                }
            }
        }

        private Tuple<string, string> GetFileNameWithExtension(string objectKey, ILambdaContext context)
        {
            string[] keyParts = objectKey.Split('/');
            string filenameWithExtension = keyParts[keyParts.Length - 1]; // Get the last part of the key
            //context.Logger.LogInformation($"FilenameWithExtension: {filenameWithExtension}");
            string[] filenameParts = filenameWithExtension.Split('.');
            string filename = filenameParts[0].ToLowerInvariant(); // Filename without extension
            string extension = filenameParts.Length > 1 ? filenameParts[1].ToLowerInvariant() : ""; // Extension if available
            context.Logger.LogInformation($"FilenameWithExtension: filename:{filename}, extension:{extension}");
            return new Tuple<string, string>(filename, extension);
        }
    }
}