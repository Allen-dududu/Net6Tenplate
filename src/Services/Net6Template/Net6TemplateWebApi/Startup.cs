using Microsoft.eShopOnContainers.BuildingBlocks.EventBus;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBusRabbitMQ;
using Net6TemplateWebApi.Controllers;
using Net6TemplateWebApi.Infrastructure.Filters;
using Net6TemplateWebApi.Infrastructure.Middlewares;
using Net6TemplateWebApi.Infrastructure.Serilog;
using Net6.Template.Repertory;
using Polly;
using Polly.Extensions.Http;
using RabbitMQ.Client;
using System.IdentityModel.Tokens.Jwt;
using Net6.Common.Util;

namespace Net6TemplateWebApi.template.Api;
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public virtual IServiceProvider ConfigureServices(IServiceCollection services)
    {

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            });

        var noOpPolicy = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();

        services.AddHttpClient("demo")
            // Select a policy based on the request: retry for Get requests, noOp for other http verbs.
            .AddPolicyHandler(request => request.Method == HttpMethod.Get ? retryPolicy : noOpPolicy)
            .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30)
                    ));

        services.AddHttpClient<ITodoRepertory>(client =>
        {
            client.BaseAddress = new Uri("https://avatars.githubusercontent.com/");
        });

        services.AddSingleton<ITodoRepertory, TodoRepertory>();

        //RegisterAppInsights(services);

        services.AddControllers(options =>
        {
            options.Filters.Add(typeof(ValidateModelStateFilter));

        }) // Added for functional tests
           // 当controller 在其他dll中
            .AddApplicationPart(typeof(WeatherForecastController).Assembly)
            // net 不在默认使用序列化工具，需要指定。
            .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Net6TemplateWebApiOnContainers - Basket HTTP API",
                Version = "v1",
                Description = "The Net6TemplateWebApi Service HTTP API"
            });

            //options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            //{
            //    Type = SecuritySchemeType.OAuth2,
            //    Flows = new OpenApiOAuthFlows()
            //    {
            //        Implicit = new OpenApiOAuthFlow()
            //        {
            //            AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
            //            TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityUrlExternal")}/connect/token"),
            //            Scopes = new Dictionary<string, string>()
            //            {
            //                { "basket", "Basket API" }
            //            }
            //        }
            //    }
            //});

            options.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        ConfigureAuthService(services);

        services.AddCustomHealthCheck(Configuration);

        /// 1
        services.Configure<Net6TemplateWebApiSettings>(Configuration.GetSection(Net6TemplateWebApiSettings.Net6TemplateWebApiSettingsName));

        /// 2
        //services.AddOptions<Net6TemplateWebApiSettings>()
        //      .Bind(Configuration.GetSection("ConnectionString"))
        //      .ValidateDataAnnotations();

        //By connecting here we are making sure that our service
        //cannot start until redis is ready. This might slow down startup,
        //but given that there is a delay on resolving the ip address
        //and then creating the connection it seems reasonable to move
        //that cost to startup instead of having the first request pay the
        //penalty.
        //services.AddSingleton<ConnectionMultiplexer>(sp =>
        //{
        //    var settings = sp.GetRequiredService<IOptions<BasketSettings>>().Value;
        //    var configuration = ConfigurationOptions.Parse(settings.ConnectionString, true);

        //    return ConnectionMultiplexer.Connect(configuration);
        //});


        //services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
        //{
        //    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

        //    var factory = new ConnectionFactory()
        //    {
        //        HostName = Configuration["EventBusConnection"],
        //        DispatchConsumersAsync = true
        //    };

        //    if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
        //    {
        //        factory.UserName = Configuration["EventBusUserName"];
        //    }

        //    if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
        //    {
        //        factory.Password = Configuration["EventBusPassword"];
        //    }

        //    var retryCount = 5;
        //    if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
        //    {
        //        retryCount = int.Parse(Configuration["EventBusRetryCount"]);
        //    }

        //    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
        //});


        //RegisterEventBus(services);


        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        services.AddOptions();

        var container = new ContainerBuilder();
        container.Populate(services);

        var serviceProvider = new AutofacServiceProvider(container.Build());

        // 把 serviceProvidor 设置成静态变量, 从全局访问ServiceProvider
        services.AddGlobalContext(services.BuildServiceProvider(), Configuration);

        return serviceProvider;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        //loggerFactory.AddAzureWebAppDiagnostics();
        //loggerFactory.AddApplicationInsights(app.ApplicationServices, LogLevel.Trace);

        var pathBase = Configuration["PATH_BASE"];
        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = LogHelper.EnrichFromRequest;
            opts.GetLevel = LogHelper.ExcludeHealthChecks;
        });

        app.UseMiddleware<ErrorHandlerMiddleware>();
        app.UseSwagger()
            .UseSwaggerUI(setup =>
            {
                setup.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Basket.API V1");
                setup.OAuthClientId("basketswaggerui");
                setup.OAuthAppName("Basket Swagger UI");
            });

        app.UseRouting();
        app.UseCors("CorsPolicy");
        ConfigureAuth(app);

        app.UseStaticFiles();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
            endpoints.MapControllers();
            endpoints.MapGet("/_proto/", async ctx =>
            {
                ctx.Response.ContentType = "text/plain";
                using var fs = new FileStream(Path.Combine(env.ContentRootPath, "Proto", "basket.proto"), FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                while (!sr.EndOfStream)
                {
                    var line = await sr.ReadLineAsync();
                    if (line != "/* >>" || line != "<< */")
                    {
                        await ctx.Response.WriteAsync(line);
                    }
                }
            });
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });

        //ConfigureEventBus(app);
    }

    //private void RegisterAppInsights(IServiceCollection services)
    //{
    //    services.AddApplicationInsightsTelemetry(Configuration);
    //    services.AddApplicationInsightsKubernetesEnricher();
    //}

    private void ConfigureAuthService(IServiceCollection services)
    {
        // prevent from mapping "sub" claim to nameidentifier.
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        var identityUrl = Configuration.GetValue<string>("IdentityUrl");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(options =>
        {
            options.Authority = identityUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = "basket";
        });
    }

    protected virtual void ConfigureAuth(IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    private void RegisterEventBus(IServiceCollection services)
    {

        services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
        {
            var subscriptionClientName = Configuration["SubscriptionClientName"];
            var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
            var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
            var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
            var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

            var retryCount = 5;
            if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
            {
                retryCount = int.Parse(Configuration["EventBusRetryCount"]);
            }

            return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
        });


        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        //services.AddTransient<ProductPriceChangedIntegrationEventHandler>();
        //services.AddTransient<OrderStartedIntegrationEventHandler>();
    }

    private void ConfigureEventBus(IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        //eventBus.Subscribe<ProductPriceChangedIntegrationEvent, ProductPriceChangedIntegrationEventHandler>();
        //eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();
    }
}

public static class CustomExtensionMethods
{
    public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

        //hcBuilder
        //    .AddRedis(
        //        configuration["ConnectionString"],
        //        name: "redis-check",
        //        tags: new string[] { "redis" });

        //if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        //{
        //    hcBuilder
        //        .AddAzureServiceBusTopic(
        //            configuration["EventBusConnection"],
        //            topicName: "eshop_event_bus",
        //            name: "basket-servicebus-check",
        //            tags: new string[] { "servicebus" });
        //}
        //else
        //{
        //    hcBuilder
        //        .AddRabbitMQ(
        //            $"amqp://{configuration["EventBusConnection"]}",
        //            name: "basket-rabbitmqbus-check",
        //            tags: new string[] { "rabbitmqbus" });
        //}

        return services;
    }
}