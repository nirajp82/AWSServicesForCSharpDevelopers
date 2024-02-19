using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SimpleLambda;

//cd C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda
//Deploy: dotnet lambda deploy-function SimpleLambda
//Run: dotnet lambda invoke-function SimpleLambda --payload '{ ""Name"": ""ShreeRajShyamaji""}'
//Debug: dotnet lambda-test-tool-6.0
public class Functions
{
    [LambdaFunction(Role = "MyLambda-role-2iccehky", MemorySize = 256, Timeout = 30)]
    public string FunctionHandler(LambdaRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation($"Pranam from AWS Serverless Application -> Empty Serverless Application. Request Data: {request.Name}");
        return $"Pranam, {request.Name}!!!";
    }

    /*
        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <remarks>
        /// This uses the <see href="https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.Annotations/README.md">Lambda Annotations</see> 
        /// programming model to bridge the gap between the Lambda programming model and a more idiomatic .NET model.
        /// 
        /// This automatically handles reading parameters from an APIGatewayProxyRequest
        /// as well as syncing the function definitions to serverless.template each time you build.
        /// 
        /// If you do not wish to use this model and need to manipulate the API Gateway 
        /// objects directly, see the accompanying Readme.md for instructions.
        /// </remarks>
        /// <param name="context">Information about the invocation, function, and execution environment</param>
        /// <returns>The response as an implicit <see cref="APIGatewayProxyResponse"/></returns>
        [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole", MemorySize = 256, Timeout = 30)]
        [RestApi(LambdaHttpMethod.Get, "/")]
        public IHttpResult Get(ILambdaContext context)
        {
            context.Logger.LogInformation("Handling the 'Get' Request");

            return HttpResults.Ok("Hello AWS Serverless");
        }
    */
}

public class LambdaRequest
{
    public string Name { get; set; } = default!;
}

/*
     Deploy: dotnet lambda deploy-function SimpleLambda
     Output:
            Updating code for existing function SimpleLambda
            Updating function url config: https://z3wrv5feb7v2rfckz2wfbyhclm0gipkz.lambda-url.us-east-1.on.aws/
            PS C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda> dotnet lambda deploy-function SimpleLambda
            Amazon Lambda Tools for .NET Core applications (5.10.0)
            Project Home: https://github.com/aws/aws-extensions-for-dotnet-cli, https://github.com/aws/aws-lambda-dotnet

            Executing publish command
            Deleted previous publish folder
            ... invoking 'dotnet publish', working folder 'C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda\bin\Release\net6.0\publish'
            ... dotnet publish "C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda" --output "C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda\bin\Release\net6.0\publish" --configuration "Release" --framework "net6.0" /p:GenerateRuntimeConfigurationFiles=true --runtime linux-x64 --self-contained False
            ... publish: MSBuild version 17.8.3+195e7f5a3 for .NET
            ... publish:   Determining projects to restore...
            ... publish:   All projects are up-to-date for restore.
            ... publish:   SimpleLambda -> C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda\bin\Release\net6.0\linux-x64\SimpleLambda.dll
            ... publish:   SimpleLambda -> C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda\bin\Release\net6.0\publish\
            Zipping publish folder C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda\bin\Release\net6.0\publish to C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda\bin\Release\net6.0\SimpleLambda.zip
            ... zipping: Amazon.Lambda.Annotations.dll
            ... zipping: Amazon.Lambda.APIGatewayEvents.dll
            ... zipping: Amazon.Lambda.Core.dll
            ... zipping: Amazon.Lambda.Serialization.SystemTextJson.dll
            ... zipping: Microsoft.Extensions.DependencyInjection.Abstractions.dll
            ... zipping: Microsoft.Extensions.DependencyInjection.dll
            ... zipping: SimpleLambda.deps.json
            ... zipping: SimpleLambda.dll
            ... zipping: SimpleLambda.pdb
            ... zipping: SimpleLambda.runtimeconfig.json
            Created publish archive (C:\Projects\AWSServicesForCSharpDevelopers\8.Lambda\SimpleLambda\bin\Release\net6.0\SimpleLambda.zip).
            Updating code for existing function SimpleLambda
            Updating function url config: https://z3wrv5feb7v2rfckz2wfbyhclm0gipkz.lambda-url.us-east-1.on.aws/
 */

/*
    Invoke:  dotnet lambda invoke-function SimpleLambda --payload '{ ""Name"": ""ShreeRajShyamaji""}'
    Output:
            Amazon Lambda Tools for .NET Core applications (5.10.0)
            Project Home: https://github.com/aws/aws-extensions-for-dotnet-cli, https://github.com/aws/aws-lambda-dotnet

            Payload:
            "Pranam, ShreeRajShyamaji!!!"

            Log Tail:
            START RequestId: b32078a1-b16a-4637-a205-295a9940c12d Version: $LATEST
            2024-02-18T21:38:06.914Z        b32078a1-b16a-4637-a205-295a9940c12d    info   
                        Pranam from AWS Serverless Application -> Empty Serverless Application. Request Data: ShreeRajShyamaji
            END RequestId: b32078a1-b16a-4637-a205-295a9940c12d
            REPORT RequestId: b32078a1-b16a-4637-a205-295a9940c12d  Duration: 61.46 ms      
                        Billed Duration: 62 ms  Memory Size: 256 MB     Max Memory Used: 61 MB  
 */