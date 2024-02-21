### How to deploy existing project as a AWS Lambda

#### 1: Modify Project file and add following settings to project file.
```csproj
<!--Start:: Following setting is copie to deply WebAPI project as AWS Lambda-->
<GenerateRuntimeConfigurationFiles>true</GenerateRuntimeConfigurationFiles>
<AWSProjectType>Lambda</AWSProjectType>
<!-- This property makes the build directory similar to a publish directory and helps the AWS .NET Lambda Mock Test Tool find project dependencies. -->
<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
<!-- Generate ready to run images during publishing to improve cold start time. -->
<PublishReadyToRun>true</PublishReadyToRun>
<!--End:: Following setting is copie to deply WebAPI project as AWS Lambda-->
```
### 2: 
