using Hassann_Khala.Application.DTOs.Product;

namespace Hassann_Khala.Application.Interfaces.IServices
{
    public interface IProductService
    {
        Task<ProductDto> CreateAsync(CreateProductRequest request);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllAsync();
    }
}
