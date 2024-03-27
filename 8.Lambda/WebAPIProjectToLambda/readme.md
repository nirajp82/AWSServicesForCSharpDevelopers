### How to deploy existing project as a AWS Lambda

#### 1: Modify Project file and add following settings to project file.
```csproj
<!--Start:: Copy following settings to deploy WebAPI project as AWS Lambda-->
<GenerateRuntimeConfigurationFiles>true</GenerateRuntimeConfigurationFiles>
<AWSProjectType>Lambda</AWSProjectType>
<!-- This property makes the build directory similar to a publish directory
      and helps the AWS .NET Lambda Mock Test Tool find project dependencies. -->
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
<!-- Generate ready to run images during publishing to improve cold start time. -->
<PublishReadyToRun>true</PublishReadyToRun>
<!--End:: Following setting is copie to deply WebAPI project as AWS Lambda-->
```
![image](https://github.com/nirajp82/AWSServicesForCSharpDevelopers/assets/61636643/83a3fb64-157c-49cc-a272-0fc83da46c30)

### 2: Make sure selected target framework is supported by AWS Lambda
- WebAPIProjectToLambda.csproj
```csproj
<Project Sdk="Microsoft.NET.Sdk.Web">
	<PropertyGroup>
		<TargetFramework>net8.0</TargetFramework>
    ...
```
![image](https://github.com/nirajp82/AWSServicesForCSharpDevelopers/assets/61636643/a2e6dff1-53ea-450c-b392-9a01c974387f)

### 3: Add `aws-lambda-tools-defaults.json` to the project.  The aws-lambda-tools-defaults.json file is used to configure default settings for deploying AWS Lambda functions using the AWS Lambda Tools for .NET.
- aws-lambda-tools-defaults.json
``` json
  {
  "Information": [
    "This file provides default values for the deployment wizard inside Visual Studio and the AWS Lambda commands added to the .NET Core CLI.",
    "To learn more about the Lambda commands with the .NET Core CLI execute the following command at the command line in the project root directory.",
    "dotnet lambda help",
    "All the command line options for the Lambda command can be specified in this file."
  ],
  "profile": "",
  "region": "",
  "configuration": "Release",
  "function-architecture": "x86_64",
  "function-runtime": "dotnet8",
  "function-memory-size": 256,
  "function-timeout": 30,
  "function-handler": "WebAPIProjectToLambda",
  "function-url-enable": true
}
```
**Note**: Make sre fun `function-handler` contains the name of the assembly and `function-runtime` has correct version supported by lambda. 

### 4: Add `Amazon.Lambda.AspNetCoreServer.Hosting` nuget package to the project.

### 5: Add AWS Lambda integration for HTTP API Gateway events.
- Program.cs
  ```cs
	builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
  ```
### 6: Deploy function to the aws
```dotnet lambda deploy-function WebAPIProjectToLambda```

### 7: Test the function in browser
https://{{AWSLambdaURL}}/WeatherForecast
