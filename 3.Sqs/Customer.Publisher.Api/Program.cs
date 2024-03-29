using Amazon.SQS;
using Customer.Publisher.Api.Database;
using Customer.Publisher.Api.Messaging;
using Customer.Publisher.Api.Repositories;
using Customer.Publisher.Api.Services;
using Customer.Publisher.Api.Validation;
using Dapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

var config = builder.Configuration;
config.AddEnvironmentVariables("CustomersApi_");

//builder.Services.AddControllers().AddFluentValidation(x =>
//{
//    x.RegisterValidatorsFromAssemblyContaining<Program>();
//    x.DisableDataAnnotationsValidation = true;
//});
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

SqlMapper.AddTypeHandler(new GuidTypeHandler());
SqlMapper.RemoveTypeMap(typeof(Guid));
SqlMapper.RemoveTypeMap(typeof(Guid?));

builder.Services.Configure<QueueSettings>(builder.Configuration.GetSection(QueueSettings.Key));

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(config.GetValue<string>("Database:ConnectionString")!));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<ICustomerService, CustomerService>();
builder.Services.AddSingleton<IGitHubService, GitHubService>();
builder.Services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
builder.Services.AddSingleton<ISqsMessenger, SqsMessenger>();

//Registers an HttpClient named "GitHub" in the dependency injection container. 
builder.Services.AddHttpClient("GitHub", httpClient =>
{
    //This action method will be called each time _httpClientFactory.CreateClient("GitHub") is invoked.
    
    // Set the base address for the HttpClient to the GitHub API base URL.
    httpClient.BaseAddress = new Uri(config.GetValue<string>("GitHub:ApiBaseUrl")!);

    // Add the "Accept" header with the value "application/vnd.github.v3+json" to specify the desired media type for the response.
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/vnd.github.v3+json");

    // Add a custom User-Agent header to the requests. It includes a string "Course-" followed by the machine name of the environment.
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.UserAgent, $"Course-{Environment.MachineName}");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<ValidationExceptionMiddleware>();
app.MapControllers();

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();