using Weather.Api.Models;
using Weather.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
var appName = builder.Environment.ApplicationName;

//Following code configures a secrets manager to load secrets into the application configuration.
//It filters secrets based on their names and transforms the keys of the secrets to a different format before adding them to the configuration.
builder.Configuration.AddSecretsManager(configurator: options =>
{
    options.SecretFilter = entry => entry.Name.StartsWith($"{env}_{appName}");

    options.KeyGenerator = (_, key) =>
    {
        //key: Development_Weather.Api_OpenWeatherMapApi__ApiKey
        key = key
            .Replace($"{env}_{appName}_", string.Empty)
            .Replace("__", ":");

        //Transforming secret keys to a format compatible with the application's configuration system.
        //Key: OpenWeatherMapApi:ApiKey
        return key;
    };

    //To handle rotation of secrets.
    options.PollingInterval = TimeSpan.FromHours(10);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<OpenWeatherApiSettings>(builder.Configuration.GetSection(OpenWeatherApiSettings.Key));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IWeatherService, WeatherService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
