using Amazon.SQS.Model;
using Amazon.SQS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SQSPublisher
{
    internal class SQSPublisherUsingIAMRole
    {
        /// <summary>
        /// When you create an instance of AmazonSQSClient without passing in explicit credentials, 
        /// the SDK will attempt to use the default credentials provider chain, which includes environment variables,
        /// shared credentials file, and IAM roles associated with an EC2 instance (if applicable).
        /// </summary>
        /// <returns></returns>
        internal static async Task Publish()
        {
            using (var sqsClient = new AmazonSQSClient())
            {
                await SQSHelper.Publish(sqsClient);
            }
        }
    }
}
