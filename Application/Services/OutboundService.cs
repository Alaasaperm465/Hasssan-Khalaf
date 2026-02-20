using Hassann_Khala.Application.DTOs.Outbound;
using Hassann_Khala.Application.Interfaces.IServices;
using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;

namespace Hassann_Khala.Application.Services
{
    public class OutboundService : IOutboundService
    {
        private readonly IRepository<Outbound> _outboundRepo;
        private readonly IStockRepository _stockRepo;
        private readonly IUnitOfWork _uow;

        public OutboundService(IRepository<Outbound> outboundRepo, IStockRepository stockRepo, IUnitOfWork uow)
        {
            _outboundRepo = outboundRepo;
            _stockRepo = stockRepo;
            _uow = uow;
        }

        public async Task<int> CreateOutboundAsync(CreateOutboundRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Lines == null || request.Lines.Count == 0)
                throw new ArgumentException("Outbound must contain at least one line");

            // Validate inputs
            foreach (var line in request.Lines)
            {
                if (line.Cartons < 0 || line.Pallets < 0) throw new ArgumentException("Cartons and pallets must be non-negative");
            }

            var outbound = new Outbound
            {
                ClientId = request.ClientId,
                CreatedAt = DateTime.UtcNow
            };

            // Execute validation and mutation inside a transaction
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                // Validate all lines first
                foreach (var line in request.Lines)
                {
                    var stock = await _stockRepo.FindAsync(request.ClientId, line.ProductId, line.SectionId);
                    if (stock == null)
                    {
                        throw new InvalidOperationException($"Stock not found for product {line.ProductId} in section {line.SectionId} for client {request.ClientId}");
                    }

                    if (stock.Cartons < line.Cartons || stock.Pallets < line.Pallets)
                    {
                        throw new InvalidOperationException($"Insufficient stock for product {line.ProductId} in section {line.SectionId}");
                    }
                }

                // All validations passed, perform updates and save details
                foreach (var line in request.Lines)
                {
                    var detail = new OutboundDetail
                    {
                        ProductId = line.ProductId,
                        SectionId = line.SectionId,
                        Cartons = line.Cartons,
                        Pallets = line.Pallets,
                        Quantity = (decimal)line.Cartons + (decimal)line.Pallets * 100m
                    };

                    outbound.Details.Add(detail);

                    var stock = await _stockRepo.FindAsync(request.ClientId, line.ProductId, line.SectionId);
                    // stock must exist and have sufficient qty (validated above)
                    stock!.Cartons -= line.Cartons;
                    stock!.Pallets -= line.Pallets;

                    if (stock.Cartons < 0 || stock.Pallets < 0) throw new InvalidOperationException("Stock cannot be negative");

                    _stockRepo.Update(stock);
                }

                await _outboundRepo.AddAsync(outbound);
                await _uow.SaveChangesAsync();
            });

            return outbound.Id;
        }
    }
}