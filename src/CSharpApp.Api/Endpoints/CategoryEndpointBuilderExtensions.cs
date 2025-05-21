using CSharpApp.Core.Dtos;

namespace CSharpApp.Api.Endpoints
{
    public static class CategoryEndpointBuilderExtensions
    {
        public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("api/v{version:apiVersion}/categories/", async (ICategoriesService categoriesService) =>
                {
                    var categories = await categoriesService.GetCategories();
                    return categories;
                })
                .WithName("GetCategories")
                .HasApiVersion(1.0);
            endpoints.MapGet("api/v{version:apiVersion}/categories/{id}", async (int id, ICategoriesService categoriesService) =>
                {
                    var category = await categoriesService.GetCategory(id);
                    return category;
                })
                .WithName("GetCategory")
                .HasApiVersion(1.0);
            endpoints.MapPost("api/v{version:apiVersion}/categories/", async (Category requestData, ICategoriesService categoriesService) =>
                {
                    var category = await categoriesService.CreateCategory(requestData);
                    if (category is null)
                    {
                        return Results.BadRequest("Could not create category!");
                    }
                    return Results.CreatedAtRoute("GetCategory", new { version = "1.0", id = category }, requestData);
                })
                .WithName("CreateCategory")
                .HasApiVersion(1.0);

            return endpoints;
        }
    }
}
