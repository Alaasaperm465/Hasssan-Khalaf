using Hassann_Khala.Application.DTOs.Stock;
using Hassann_Khala.Application.Interfaces.IServices;
using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;
using System.Collections.Generic;

namespace Hassann_Khala.Application.Services
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepo;

        public StockService(IStockRepository stockRepo)
        {
            _stockRepo = stockRepo;
        }

        public async Task<IEnumerable<Stock>> GetStocksByClientAsync(int clientId)
        {
            var list = await _stockRepo.GetByClientAsync(clientId);
            return list ?? new List<Stock>();
        }

        public async Task<StockResponse> GetStockAsync(int clientId, int productId, int sectionId)
        {
            var stock = await _stockRepo.FindAsync(clientId, productId, sectionId);

            if (stock == null)
            {
                return new StockResponse
                {
                    ClientId = clientId,
                    ProductId = productId,
                    SectionId = sectionId,
                    Cartons = 0,
                    Pallets = 0
                };
            }

            return new StockResponse
            {
                ClientId = stock.ClientId,
                ProductId = stock.ProductId,
                SectionId = stock.SectionId,
                Cartons = stock.Cartons,
                Pallets = stock.Pallets
            };
        }

        public async Task<decimal> GetStockQuantityAsync(int clientId, int productId, int sectionId)
        {
            var stock = await _stockRepo.FindAsync(clientId, productId, sectionId);
            if (stock == null) return 0;
            return stock.Cartons;
        }
    }
}
