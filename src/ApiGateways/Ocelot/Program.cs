using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Configuration.AddConfiguration(GetConfiguration(args));
builder.Services.AddOcelot(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.

await app.UseOcelot();

app.UseAuthorization();

app.MapControllers();

app.Run();


IConfiguration GetConfiguration(string[] arg)
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(arg).AddJsonFile("ocelot.json");
        

    return builder.Build();
}