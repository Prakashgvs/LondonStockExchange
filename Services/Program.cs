using Business.BusinessLogic;
using Core.Interfaces;
using Data.DataAccess;
using Data.Repository;
using Services.Messaging;
using Services.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<ITradeQueue, MsmqTradeQueue>();
builder.Services.AddHostedService<TradeQueueConsumer>();

builder.Services.AddScoped<IDatabase, MsSqlDatabase>();

builder.Services.AddScoped<ITradeBusinessLogic, TradeBusinessLogic>();
builder.Services.AddScoped<IStockPriceBusinessLogic, StockPriceBusinessLogic>();

builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<IStockPriceRepository, StockPriceRepository>();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
