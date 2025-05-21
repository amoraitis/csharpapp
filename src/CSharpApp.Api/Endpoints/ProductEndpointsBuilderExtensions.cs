using CSharpApp.Core.Dtos;

namespace CSharpApp.Api.Endpoints
{
    public static class ProductEndpointsBuilderExtensions
    {
        public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("api/v{version:apiVersion}/products/", async (IProductsService productsService) =>
                {
                    var products = await productsService.GetProducts();
                    return products;
                })
                .WithName("GetProducts")
                .HasApiVersion(1.0);

            endpoints.MapGet("api/v{version:apiVersion}/products/{id}", async (int id, IProductsService productsService) =>
                {
                    var product = await productsService.GetProduct(id);
                    return product;
                })
                .WithName("GetProduct")
                .HasApiVersion(1.0);

            endpoints.MapPost("api/v{version:apiVersion}/products/", async (Product requestData, IProductsService productsService) =>
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

            return endpoints;
        }
    }
}
