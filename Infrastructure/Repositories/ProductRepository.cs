using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Laptop", Price = 999.99m, Description = "Laptop de alto rendimiento", Stock = 10 },
        new Product { Id = 2, Name = "Mouse", Price = 29.99m, Description = "Mouse inalámbrico", Stock = 50 },
        new Product { Id = 3, Name = "Teclado", Price = 49.99m, Description = "Teclado mecánico", Stock = 30 }
    };

    private static int _nextId = 4;

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Product>>(_products);
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<Product> CreateAsync(Product product)
    {
        product.Id = _nextId++;
        _products.Add(product);
        return Task.FromResult(product);
    }

    public Task UpdateAsync(Product product)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existingProduct != null)
        {
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.Stock = product.Stock;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
        return Task.CompletedTask;
    }
}
