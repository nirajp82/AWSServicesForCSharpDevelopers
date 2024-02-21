### How to deploy existing project as a AWS Lambda

#### 1: Modify Project file and add following settings to project file.
```csproj
<!--Start:: Following setting is copie to deply WebAPI project as AWS Lambda-->
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

- aws-lambda-tools-defaults.json
```
  "function-runtime": "dotnet8",
```
