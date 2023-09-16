using System.Globalization;

namespace ProductRepositoryAsync
{
    /// <summary>
    /// Represents a product storage service and provides a set of methods for managing the list of products.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly string productCollectionName;
        private readonly IDatabase database;

        public ProductRepository(string productCollectionName, IDatabase database)
        {
            this.productCollectionName = productCollectionName;
            this.database = database;
        }

        public async Task<int> AddProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException("Name and Category should not be empty or whitespace only.", nameof(product));
            }

            if (string.IsNullOrWhiteSpace(product.Category))
            {
                throw new ArgumentException("Category", nameof(product));
            }

            if (product.UnitPrice < 0)
            {
                throw new ArgumentException("UnitPrice should be greater or equal to zero.", nameof(product));
            }

            if (product.UnitsInStock < 0)
            {
                throw new ArgumentException("UnitsInStock should be greater or equal to zero.", nameof(product));
            }

            OperationResult collectionExistResult = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

            if (collectionExistResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (collectionExistResult != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            if (!collectionExists)
            {
                OperationResult createCollectionResult = await this.database.CreateCollectionAsync(this.productCollectionName);

                if (createCollectionResult == OperationResult.ConnectionIssue)
                {
                    throw new DatabaseConnectionException("database connection is lost.");
                }
                else if (createCollectionResult != OperationResult.Success)
                {
                    throw new RepositoryException("a database error occurred.");
                }
            }

            OperationResult generateIdResult = await this.database.GenerateIdAsync(this.productCollectionName, out int productId);

            if (generateIdResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (generateIdResult != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            IDictionary<string, string> data = new Dictionary<string, string>
    {
        { "name", product.Name },
        { "category", product.Category },
        { "price", product.UnitPrice.ToString(CultureInfo.InvariantCulture) },
        { "in-stock", product.UnitsInStock.ToString(CultureInfo.InvariantCulture) },
        { "discontinued", product.Discontinued.ToString() },
    };

            OperationResult insertResult = await this.database.InsertCollectionElementAsync(this.productCollectionName, productId, data);

            if (insertResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException();
            }
            else if (insertResult != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            return productId;
        }

        public async Task<Product> GetProductAsync(int productId)
        {
            _ = new Product
            {
                Id = productId,
                Name = string.Empty,
                Category = string.Empty,
            };
            OperationResult result = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

            if (result == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (result != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            if (!collectionExists)
            {
                throw new CollectionNotFoundException("collection is not found.");
            }

            result = await this.database.IsCollectionElementExistAsync(this.productCollectionName, productId, out bool collectionElementExists);

            if (result == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (result != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            if (!collectionElementExists)
            {
                throw new ProductNotFoundException();
            }

            result = await this.database.GetCollectionElementAsync(this.productCollectionName, productId, out IDictionary<string, string> data);

            if (result == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (result != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            return new Product
            {
                Id = productId,
                Name = data["name"],
                Category = data["category"],
                UnitPrice = decimal.Parse(data["price"], CultureInfo.InvariantCulture),
                UnitsInStock = int.Parse(data["in-stock"], CultureInfo.InvariantCulture),
                Discontinued = bool.Parse(data["discontinued"]),
            };
        }

        public async Task RemoveProductAsync(int productId)
        {
            OperationResult collectionExistResult = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

            if (collectionExistResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (collectionExistResult != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            if (!collectionExists)
            {
                throw new CollectionNotFoundException("collection is not found.");
            }

            OperationResult elementExistResult = await this.database.IsCollectionElementExistAsync(this.productCollectionName, productId, out bool collectionElementExists);

            if (elementExistResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (elementExistResult != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            if (!collectionElementExists)
            {
                throw new ProductNotFoundException("product is not found.");
            }

            OperationResult deleteResult = await this.database.DeleteCollectionElementAsync(this.productCollectionName, productId);

            if (deleteResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (deleteResult != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name) || string.IsNullOrWhiteSpace(product.Category))
            {
                throw new ArgumentException("Name and Category should not be empty or whitespace only.", nameof(product));
            }

            if (product.UnitPrice < 0)
            {
                throw new ArgumentException("UnitPrice should be greater or equal to zero.", nameof(product));
            }

            if (product.UnitsInStock < 0)
            {
                throw new ArgumentException("UnitsInStock should be greater or equal to zero.", nameof(product));
            }

            OperationResult collectionExistResult = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool collectionExists);

            if (collectionExistResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (collectionExistResult != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            if (!collectionExists)
            {
                throw new CollectionNotFoundException("collection is not found.");
            }

            OperationResult productExistResult = await this.database.IsCollectionElementExistAsync(this.productCollectionName, product.Id, out bool productExists);

            if (productExistResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (productExistResult != OperationResult.Success)
            {
                throw new RepositoryException("a database error occurred.");
            }

            if (!productExists)
            {
                throw new ProductNotFoundException("product is not found.");
            }

            IDictionary<string, string> data = new Dictionary<string, string>
    {
        { "name", product.Name },
        { "category", product.Category },
        { "price", product.UnitPrice.ToString(CultureInfo.InvariantCulture) },
        { "in-stock", product.UnitsInStock.ToString(CultureInfo.InvariantCulture) },
        { "discontinued", product.Discontinued.ToString() },
    };

            OperationResult updateResult = await this.database.UpdateCollectionElementAsync(this.productCollectionName, product.Id, data);

            if (updateResult == OperationResult.ConnectionIssue)
            {
                throw new DatabaseConnectionException("database connection is lost.");
            }
            else if (updateResult != OperationResult.Success)
            {
                throw new RepositoryException();
            }
        }
    }
}
