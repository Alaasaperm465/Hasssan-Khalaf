using System.Linq;
using Hassann_Khala.Application.DTOs.Product;
using Hassann_Khala.Application.Interfaces.IServices;
using Hassann_Khala.Application.Interfaces.IRepositories;
using Hassann_Khala.Domain;

namespace Hassann_Khala.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly Hassann_Khala.Application.Interfaces.IRepositories.IProductRepository _repo;
        private readonly Hassann_Khala.Domain.Interfaces.IUnitOfWork _uow;

        public ProductService(Hassann_Khala.Application.Interfaces.IRepositories.IProductRepository repo, Hassann_Khala.Domain.Interfaces.IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<ProductDto> CreateAsync(CreateProductRequest request)
        {
            var product = new Product { Name = request.Name };
            await _repo.AddAsync(product);
            await _uow.SaveChangesAsync();
            return new ProductDto { Id = product.Id, Name = product.Name };
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return entities.Select(p => new ProductDto { Id = p.Id, Name = p.Name }).ToList();
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return new ProductDto { Id = entity.Id, Name = entity.Name };
        }
    }
}
