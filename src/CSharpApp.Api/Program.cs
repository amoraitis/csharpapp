using CSharpApp.Core.Dtos;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Logging.ClearProviders().AddSerilog(logger);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDefaultConfiguration();
builder.Services.AddHttpConfiguration();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

var versionedEndpointRouteBuilder = app.NewVersionedApi();

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/products/", async (IProductsService productsService) =>
    {
        var products = await productsService.GetProducts();
        return products;
    })
    .WithName("GetProducts")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapGet(@"api/v{version:apiVersion}/products/{id}", async (int id, IProductsService productsService) =>
    {
        var products = await productsService.GetProduct(id);
        return products;
    })
    .WithName("GetProduct")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapPost("api/v{version:apiVersion}/products/", async (Product requestData, IProductsService productsService) =>
    {
        var product = await productsService.CreateProduct(requestData);

        if (product is null)
        {
            return Results.BadRequest("Could not create product!");
        }

        return Results.CreatedAtRoute("GetProduct", new { version = "1.0", id = product }, requestData);
    })
    .WithName("CreateProduct")
    .HasApiVersion(1.0);

app.Run();