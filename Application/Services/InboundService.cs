using Hassann_Khala.Application.DTOs.Inbound;
using Hassann_Khala.Application.Interfaces.IServices;
using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;

namespace Hassann_Khala.Application.Services
{
    public class InboundService : IInboundService
    {
        private readonly IRepository<Inbound> _inboundRepo;
        private readonly IClientRepository _clientRepo;
        private readonly IProductRepository _productRepo;
        private readonly ISectionRepository _sectionRepo;
        private readonly IStockRepository _stockRepo;
        private readonly IProductStockRepository _productStockRepo;
        private readonly ISectionStockRepository _sectionStockRepo;
        private readonly IUnitOfWork _uow;

        public InboundService(
            IRepository<Inbound> inboundRepo,
            IClientRepository clientRepo,
            IProductRepository productRepo,
            ISectionRepository sectionRepo,
            IStockRepository stockRepo,
            IProductStockRepository productStockRepo,
            ISectionStockRepository sectionStockRepo,
            IUnitOfWork uow)
        {
            _inboundRepo = inboundRepo;
            _clientRepo = clientRepo;
            _productRepo = productRepo;
            _sectionRepo = sectionRepo;
            _stockRepo = stockRepo;
            _productStockRepo = productStockRepo;
            _sectionStockRepo = sectionStockRepo;
            _uow = uow;
        }

        public async Task<int> CreateInboundAsync(CreateInboundRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ClientName)) throw new ArgumentException("ClientName is required");
            if (request.Lines == null || request.Lines.Count == 0) throw new ArgumentException("Inbound must contain at least one line");

            var clientName = request.ClientName.Trim();

            int inboundId = 0;
            await _uow.ExecuteInTransactionAsync(async () =>
            {
                var client = await _clientRepo.FindByNameAsync(clientName);
                if (client == null)
                {
                    client = new Client { Name = clientName };
                    await _clientRepo.AddAsync(client);
                    await _uow.SaveChangesAsync();
                }

                var inbound = new Inbound { ClientId = client.Id, CreatedAt = DateTime.UtcNow };

                var pendingStock = new Dictionary<string, Stock>();

                foreach (var line in request.Lines)
                {
                    if (line == null) throw new ArgumentException("Line cannot be null");
                    if (string.IsNullOrWhiteSpace(line.ProductName)) throw new ArgumentException("ProductName is required");
                    if (string.IsNullOrWhiteSpace(line.SectionName)) throw new ArgumentException("SectionName is required");
                    if (line.Cartons < 0 || line.Pallets < 0) throw new ArgumentException("Cartons and pallets must be non-negative");

                    var product = await _productRepo.GetByNameAsync(line.ProductName.Trim());
                    if (product == null) throw new ArgumentException($"Product not found: {line.ProductName}");

                    var section = await _sectionRepo.GetByNameAsync(line.SectionName.Trim());
                    if (section == null) throw new ArgumentException($"Section not found: {line.SectionName}");

                    var quantity = (decimal)line.Cartons + ((decimal)line.Pallets * 100m);

                    var detail = new InboundDetail
                    {
                        ProductId = product.Id,
                        SectionId = section.Id,
                        Cartons = line.Cartons,
                        Pallets = line.Pallets,
                        Quantity = quantity
                    };

                    inbound.Details.Add(detail);

                    var key = $"{product.Id}:{section.Id}";

                    if (!pendingStock.TryGetValue(key, out var stock))
                    {
                        stock = await _stockRepo.FindAsync(client.Id, product.Id, section.Id);
                    }

                    if (stock == null)
                    {
                        stock = new Stock
                        {
                            ClientId = client.Id,
                            ProductId = product.Id,
                            SectionId = section.Id,
                            Cartons = line.Cartons,
                            Pallets = line.Pallets
                        };
                        await _stockRepo.AddAsync(stock);
                        pendingStock[key] = stock;
                    }
                    else
                    {
                        stock.Cartons += line.Cartons;
                        stock.Pallets += line.Pallets;
                        _stockRepo.Update(stock);
                        pendingStock[key] = stock;
                    }

                    // ProductStock
                    var prodStock = await _productStockRepo.FindAsync(client.Id, product.Id);
                    if (prodStock == null)
                    {
                        prodStock = new ProductStock
                        {
                            ClientId = client.Id,
                            ProductId = product.Id,
                            Cartons = line.Cartons,
                            Pallets = line.Pallets
                        };
                        await _productStockRepo.AddAsync(prodStock);
                    }
                    else
                    {
                        prodStock.Cartons += line.Cartons;
                        prodStock.Pallets += line.Pallets;
                        _productStockRepo.Update(prodStock);
                    }

                    // SectionStock
                    var secStock = await _sectionStockRepo.FindAsync(client.Id, section.Id);
                    if (secStock == null)
                    {
                        secStock = new SectionStock
                        {
                            ClientId = client.Id,
                            SectionId = section.Id,
                            Cartons = line.Cartons,
                            Pallets = line.Pallets
                        };
                        await _sectionStockRepo.AddAsync(secStock);
                    }
                    else
                    {
                        secStock.Cartons += line.Cartons;
                        secStock.Pallets += line.Pallets;
                        _sectionStockRepo.Update(secStock);
                    }
                }

                await _inboundRepo.AddAsync(inbound);
                await _uow.SaveChangesAsync();
                inboundId = inbound.Id;
            });

            return inboundId;
        }

        public Task<IEnumerable<Inbound>> GetAllInboundsAsync()
        {
            return _inboundRepo.GetAllAsync();
        }

        public Task<IEnumerable<Inbound>> GetDailyInboundReportAsync()
        {
            return _inboundRepo.GetDailyReportAsync();
        }

        public async Task<IEnumerable<Inbound>> GetInboundReportFromToAsync(DateTime startDate, DateTime endDate)
        {
            var start = startDate.Date.ToUniversalTime();
            var end = endDate.Date.AddDays(1).ToUniversalTime();

            return await _inboundRepo.GetByDateRangeAsync(start, end);
        }

        public async Task UpdateInboundAsync(int id, UpdateInboundRequest request)
        {
            throw new NotImplementedException("Update not implemented in this iteration");
        }

        //Task<IEnumerable<Inbound>> IInboundService.GetAllInboundsAsync()
        //{
        //    throw new NotImplementedException();
        //}

        Task<IEnumerable<Inbound>> IInboundService.GetDailyInboundReportAsync()
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Inbound>> IInboundService.GetInboundReportFromToAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }
    }
}