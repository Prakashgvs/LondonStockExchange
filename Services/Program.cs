using Business.BusinessLogic;
using Core.Interfaces;
using Data.DataAccess;
using Data.Repository;
using Microsoft.OpenApi.Models;
using Services.Messaging;
using Services.Middlewares;
using LoggerFactory = Core.Logging.LoggerFactory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "London Stock Exchange API",
            Version = "v1"
        });

        options.AddSecurityDefinition("X-Broker-Id", new OpenApiSecurityScheme
        {
            Description = "Enter your Broker ID",
            Name = "X-Broker-Id",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "X-Broker-Id"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}

var loggerProvider = builder.Configuration.GetValue<string>("Logging:Provider") ?? "Serilog";
builder.Services.AddSingleton<IApplicationLogger>(LoggerFactory.CreateLogger(loggerProvider, builder.Configuration));

builder.Services.AddSingleton<ITradeQueue, MsmqTradeQueue>();
builder.Services.AddHostedService<TradeQueueConsumer>();

builder.Services.AddScoped<IDatabase, MsSqlDatabase>();

builder.Services.AddScoped<ITradeBusinessLogic, TradeBusinessLogic>();
builder.Services.AddScoped<IStockPriceBusinessLogic, StockPriceBusinessLogic>();

builder.Services.AddScoped<IBrokerRepository, BrokerRepository>();
builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<IStockPriceRepository, StockPriceRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "London Stock Exchange API v1");
    });
}

app.UseMiddleware<BrokerAuthorizationMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
