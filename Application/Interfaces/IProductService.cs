using Application.DTOs;

namespace Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
    Task UpdateProductAsync(int id, UpdateProductDto productDto);
    Task DeleteProductAsync(int id);
}
