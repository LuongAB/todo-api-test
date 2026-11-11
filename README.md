# CRUD API

A small ASP.NET Core 10 Web API for managing products, using Entity Framework Core with SQL Server storage.

API specification: [`openapi.yaml`](openapi.yaml).

## Run

```bash
dotnet run --urls http://localhost:5050
```

The `CrudApiDb` database is created automatically on first startup. The default connection targets a local SQL Server instance using Windows integrated authentication.

## Endpoints

- `GET /` - API status and endpoint list
- `GET /api/products` - list products
- `GET /api/products/{id}` - get one product
- `POST /api/products` - create a product
- `PUT /api/products/{id}` - update a product
- `DELETE /api/products/{id}` - delete a product

Example request body:

```json
{
  "name": "Keyboard",
  "price": 49.99
}
```

## Verify manually

```bash
curl http://localhost:5050/api/products
curl -X POST http://localhost:5050/api/products -H "Content-Type: application/json" -d '{"name":"Keyboard","price":49.99}'
curl -X PUT http://localhost:5050/api/products/1 -H "Content-Type: application/json" -d '{"name":"Mechanical Keyboard","price":79.99}'
curl -X DELETE http://localhost:5050/api/products/1
```

## GitHub Actions CI/CD

The workflow in `.github/workflows/dotnet.yml`:

- Builds the API on pull requests and pushes to `main`.
- Publishes a Release build on pushes to `main` or manual workflow runs.
- Uploads a 14-day `crud-api-<commit-sha>` deployment artifact.

The package is ready to copy to a hosting environment. The workflow does not deploy to a specific server because no hosting provider or deployment target has been configured.
