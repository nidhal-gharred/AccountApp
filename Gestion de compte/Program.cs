using AccountApp.API.Middleware;
using AccountApp.Application.Interfaces;
using AccountApp.Application.Services;
using AccountApp.Domain.Interfaces;
using AccountApp.Infrastructure.Factories;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register factories (Infrastructure) referencing Domain only
builder.Services.AddScoped<ITransactionSourceFactory, CsvTransactionSourceFactory>();
builder.Services.AddScoped<ICurrencyConverterFactory, CsvCurrencyConverterFactory>();
// Register application service referencing Domain
builder.Services.AddScoped<IAccountService, AccountService>();
// Enable logging
builder.Services.AddLogging();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
