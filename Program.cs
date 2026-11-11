using CrudApi.Data;
using CrudApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddProblemDetails();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
}
app.UseExceptionHandler();

app.MapGet("/", () => Results.Ok(new { name = "CRUD API", status = "running" }));
var products = app.MapGroup("/api/products");

products.MapGet("/", async (AppDbContext db, CancellationToken token) => Results.Ok(await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync(token)));
products.MapGet("/{id:int}", async (int id, AppDbContext db, CancellationToken token) =>
{
    var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, token);
    return product is null ? Results.NotFound() : Results.Ok(product);
});
products.MapPost("/", async (CreateProductRequest request, AppDbContext db, CancellationToken token) =>
{
    var error = Validate(request.Name, request.Price);
    if (error is not null) return Results.ValidationProblem(new Dictionary<string, string[]> { [error.Value.Key] = new[] { error.Value.Message } });
    var product = new Product { Name = request.Name!.Trim(), Price = request.Price };
    db.Products.Add(product);
    await db.SaveChangesAsync(token);
    return Results.Created($"/api/products/{product.Id}", product);
});
products.MapPut("/{id:int}", async (int id, UpdateProductRequest request, AppDbContext db, CancellationToken token) =>
{
    var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id, token);
    if (product is null) return Results.NotFound();
    var error = Validate(request.Name, request.Price);
    if (error is not null) return Results.ValidationProblem(new Dictionary<string, string[]> { [error.Value.Key] = new[] { error.Value.Message } });
    product.Name = request.Name!.Trim();
    product.Price = request.Price;
    await db.SaveChangesAsync(token);
    return Results.Ok(product);
});
products.MapDelete("/{id:int}", async (int id, AppDbContext db, CancellationToken token) =>
{
    var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id, token);
    if (product is null) return Results.NotFound();
    db.Products.Remove(product);
    await db.SaveChangesAsync(token);
    return Results.NoContent();
});

app.Run();

static (string Key, string Message)? Validate(string? name, decimal price)
{
    if (string.IsNullOrWhiteSpace(name)) return ("name", "Name is required.");
    if (name.Trim().Length > 200) return ("name", "Name must be 200 characters or fewer.");
    if (price < 0) return ("price", "Price must be zero or greater.");
    return null;
}

public sealed record CreateProductRequest(string? Name, decimal Price);
public sealed record UpdateProductRequest(string? Name, decimal Price);
