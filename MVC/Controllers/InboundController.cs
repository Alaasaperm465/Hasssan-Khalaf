using AutoMapper;
using Hassann_Khala.Application.DTOs.Inbound;
using Hassann_Khala.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC.ViewModels.Inbound;
using MVC.ViewModels.Ajax;
using InfraStructure.Context;
using Hassann_Khala.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MVC.Controllers
{
    public class InboundController : Controller
    {
        private readonly IInboundService _inboundService;
        private readonly Hassann_Khala.Application.Interfaces.IServices.IClientService _clientService;
        private readonly Hassann_Khala.Application.Interfaces.IServices.IProductService _productService;
        private readonly Hassann_Khala.Application.Interfaces.IServices.ISectionService _sectionService;
        private readonly IMapper _mapper;
        private readonly DBContext _db;

        public InboundController(
            IInboundService inboundService,
            Hassann_Khala.Application.Interfaces.IServices.IClientService clientService,
            Hassann_Khala.Application.Interfaces.IServices.IProductService productService,
            Hassann_Khala.Application.Interfaces.IServices.ISectionService sectionService,
            IMapper mapper,
            DBContext db)
        {
            _inboundService = inboundService;
            _clientService = clientService;
            _productService = productService; // keep service for single product lookups used on POST
            _sectionService = sectionService;
            _mapper = mapper;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var inbounds = await _inboundService.GetAllInboundsAsync();
            var model = _mapper.Map<IEnumerable<InboundListVM>>(inbounds);
            return View(model);
        }

        // Accept optional clientId to filter sections server-side
        public async Task<IActionResult> Create(int? clientId)
        {
            var clients = await _clientService.GetAllAsync();
            // load domain products to get their Type
            var products = await _db.Products.AsNoTracking().ToListAsync();
            var sections = await _sectionService.GetAllAsync();

            // if clientId provided, filter products by client's allowed types
            IEnumerable<Product> filteredProducts = products;
            if (clientId.HasValue && clientId.Value > 0)
            {
                var client = await _db.Clients.FindAsync(clientId.Value);
                if (client != null)
                {
                    var mask = client.AllowedProductTypes;
                    var allowedTypeInts = new List<int>();
                    foreach (var val in Enum.GetValues(typeof(ProductType)).Cast<ProductType>())
                    {
                        var bit = (int)val;
                        if ((mask & bit) == bit) allowedTypeInts.Add(bit);
                    }
                    if (allowedTypeInts.Any()) filteredProducts = products.Where(p => allowedTypeInts.Contains((int)p.Type));
                }
            }

            IEnumerable<SelectListItem> sectionsItems;
            if (clientId.HasValue && clientId.Value > 0)
            {
                var sectionIds = await _db.Stocks.Where(s => s.ClientId == clientId.Value).Select(s => s.SectionId).Distinct().ToListAsync();
                sectionsItems = sections.Where(s => sectionIds.Contains(s.Id)).Select(s => new SelectListItem(s.Name, s.Id.ToString()));
            }
            else
            {
                sectionsItems = sections.Select(s => new SelectListItem(s.Name, s.Id.ToString()));
            }

            var vm = new InboundCreateVM
            {
                Clients = clients.Select(c => new SelectListItem(c.Name, c.Id.ToString())),
                Delegates = Enumerable.Empty<SelectListItem>(),
                Products = filteredProducts.Select(p => new SelectListItem(p.Name, p.Id.ToString())),
                Sections = sectionsItems,
                Details = Enumerable.Range(0, 6).Select(_ => new InboundDetailVM()).ToList()
            };

            // if clientId provided, set it in vm so view knows
            if (clientId.HasValue) vm.ClientId = clientId.Value;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InboundCreateVM vm)
        {
            // existing server-side non-AJAX handling
            async Task PopulateLookups()
            {
                var clients = await _clientService.GetAllAsync();
                var products = await _db.Products.AsNoTracking().ToListAsync();
                var sections = await _sectionService.GetAllAsync();

                vm.Clients = clients.Select(c => new SelectListItem(c.Name, c.Id.ToString()));
                vm.Sections = sections.Select(s => new SelectListItem(s.Name, s.Id.ToString()));

                // if client selected, filter products by allowed types
                if (vm.ClientId > 0)
                {
                    var client = await _db.Clients.FindAsync(vm.ClientId);
                    if (client != null)
                    {
                        var mask = client.AllowedProductTypes;
                        var allowedTypeInts = new List<int>();
                        foreach (var val in Enum.GetValues(typeof(ProductType)).Cast<ProductType>())
                        {
                            var bit = (int)val;
                            if ((mask & bit) == bit) allowedTypeInts.Add(bit);
                        }
                        if (allowedTypeInts.Any()) vm.Products = products.Where(p => allowedTypeInts.Contains((int)p.Type)).Select(p => new SelectListItem(p.Name, p.Id.ToString()));
                        else vm.Products = products.Select(p => new SelectListItem(p.Name, p.Id.ToString()));
                    }
                    else
                    {
                        vm.Products = products.Select(p => new SelectListItem(p.Name, p.Id.ToString()));
                    }
                }
                else
                {
                    vm.Products = products.Select(p => new SelectListItem(p.Name, p.Id.ToString()));
                }
                
                
                // also repopulate delegates for selected client
                if (vm.ClientId > 0)
                {
                    vm.Delegates = _db.Delegates.Where(d => d.ClientId == vm.ClientId).OrderBy(d => d.Name).Select(d => new SelectListItem(d.Name, d.Id.ToString()));
                }
                else
                {
                    vm.Delegates = Enumerable.Empty<SelectListItem>();
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateLookups();
                return View(vm);
            }

            var clientDto = await _clientService.GetByIdAsync(vm.ClientId);
            if (clientDto == null)
            {
                ModelState.AddModelError(nameof(vm.ClientId), "Selected client does not exist");
                await PopulateLookups();
                return View(vm);
            }

            var lines = new List<InboundLineRequest>();
            var allSections = (await _sectionService.GetAllAsync()).ToList();

            foreach (var line in vm.Details)
            {
                var productDto = await _productService.GetByIdAsync(line.ProductId);
                var sectionDto = allSections.FirstOrDefault(s => s.Id == line.SectionId);

                if (productDto == null)
                {
                    ModelState.AddModelError(string.Empty, $"Product not found for id {line.ProductId}");
                    break;
                }
                if (sectionDto == null)
                {
                    ModelState.AddModelError(string.Empty, $"Section not found for id {line.SectionId}");
                    break;
                }
                if (line.Cartons < 0 || line.Pallets < 0)
                {
                    ModelState.AddModelError(string.Empty, "Cartons and Pallets must be non-negative");
                    break;
                }

                lines.Add(new InboundLineRequest
                {
                    ProductName = productDto.Name,
                    SectionName = sectionDto.Name,
                    Cartons = line.Cartons,
                    Pallets = line.Pallets
                });
            }

            if (!ModelState.IsValid)
            {
                await PopulateLookups();
                return View(vm);
            }

            var request = new CreateInboundRequest
            {
                ClientName = clientDto.Name,
                Lines = lines
            };

            try
            {
                var id = await _inboundService.CreateInboundAsync(request);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateLookups();
                return View(vm);
            }

        }

        // New AJAX endpoint to handle JSON POST and return JSON result
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Inbound/CreateAjax")]
        public async Task<IActionResult> CreateAjax([FromBody] CreateInboundRequest request)
        {
            if (request == null) return BadRequest(new { success = false, error = "Request body is required" });

            try
            {
                var id = await _inboundService.CreateInboundAsync(request);
                return Ok(new { success = true, id = id, client = request.ClientName });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "Server error" });
            }
        }

        [HttpPost]
        [Route("Inbound/AddLineAjax")]
        public async Task<IActionResult> AddLineAjax([FromBody] AddLineRequest? req)
        {
            // support form-encoded fallback
            if (req == null && Request.HasFormContentType)
            {
                var f = Request.Form;
                req = new AddLineRequest
                {
                    InboundId = int.TryParse(f["inboundId"], out var iid) ? iid : (int?)null,
                    ClientId = int.TryParse(f["clientId"], out var cid) ? cid : 0,
                    ProductId = int.TryParse(f["productId"], out var pid) ? pid : 0,
                    SectionId = int.TryParse(f["sectionId"], out var sid) ? sid : 0,
                    Cartons = int.TryParse(f["cartons"], out var c) ? c : 0,
                    Pallets = int.TryParse(f["pallets"], out var p) ? p : 0
                };
            }

            if (req == null) return BadRequest(new { success = false, error = "Request body required" });
            if (req.ClientId <= 0) return BadRequest(new { success = false, error = "ClientId required" });
            if (req.ProductId <= 0) return BadRequest(new { success = false, error = "ProductId required" });
            if (req.SectionId <= 0) return BadRequest(new { success = false, error = "SectionId required" });
            if (req.Cartons < 0 || req.Pallets < 0) return BadRequest(new { success = false, error = "Quantities must be non-negative" });

            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == req.ProductId && p.IsActive);
            var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == req.SectionId);
            var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == req.ClientId);

            if (product == null || section == null || client == null)
            {
                return BadRequest(new { success = false, error = "Invalid client/product/section" });
            }

            // If inboundId provided -> add detail to existing inbound
            if (req.InboundId.HasValue && req.InboundId.Value > 0)
            {
                using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    var inbound = await _db.Inbounds.Include(i => i.Details).FirstOrDefaultAsync(i => i.Id == req.InboundId.Value);
                    if (inbound == null) return NotFound(new { success = false, error = "Inbound not found" });
                    if (inbound.ClientId != req.ClientId) return BadRequest(new { success = false, error = "Client mismatch" });

                    var detail = new InboundDetail
                    {
                        InboundId = inbound.Id,
                        ProductId = req.ProductId,
                        SectionId = req.SectionId,
                        Cartons = req.Cartons,
                        Pallets = req.Pallets,
                        Quantity = req.Cartons + (req.Pallets * 100m)
                    };

                    inbound.Details.Add(detail);

                    // update stocks
                    var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.ClientId == inbound.ClientId && s.ProductId == req.ProductId && s.SectionId == req.SectionId);
                    if (stock == null)
                    {
                        stock = new Stock { ClientId = inbound.ClientId, ProductId = req.ProductId, SectionId = req.SectionId, Cartons = req.Cartons, Pallets = req.Pallets };
                        await _db.Stocks.AddAsync(stock);
                    }
                    else
                    {
                        stock.Cartons += req.Cartons;
                        stock.Pallets += req.Pallets;
                        _db.Stocks.Update(stock);
                    }

                    var prodStock = await _db.ProductStocks.FirstOrDefaultAsync(ps => ps.ClientId == inbound.ClientId && ps.ProductId == req.ProductId);
                    if (prodStock == null)
                    {
                        prodStock = new ProductStock { ClientId = inbound.ClientId, ProductId = req.ProductId, Cartons = req.Cartons, Pallets = req.Pallets };
                        await _db.ProductStocks.AddAsync(prodStock);
                    }
                    else
                    {
                        prodStock.Cartons += req.Cartons;
                        prodStock.Pallets += req.Pallets;
                        _db.ProductStocks.Update(prodStock);
                    }

                    var secStock = await _db.SectionStocks.FirstOrDefaultAsync(ss => ss.ClientId == inbound.ClientId && ss.SectionId == req.SectionId);
                    if (secStock == null)
                    {
                        secStock = new SectionStock { ClientId = inbound.ClientId, SectionId = req.SectionId, Cartons = req.Cartons, Pallets = req.Pallets };
                        await _db.SectionStocks.AddAsync(secStock);
                    }
                    else
                    {
                        secStock.Cartons += req.Cartons;
                        secStock.Pallets += req.Pallets;
                        _db.SectionStocks.Update(secStock);
                    }

                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();

                    // ensure detail id is loaded
                    var savedDetailId = detail.Id;
                    return Ok(new { success = true, id = inbound.Id, detailId = savedDetailId });
                }
                catch (System.Exception ex)
                {
                    await tx.RollbackAsync();
                    return StatusCode(500, new { success = false, error = ex.Message });
                }
            }

            // Otherwise create new inbound with this single line
            using var tx2 = await _db.Database.BeginTransactionAsync();
            try
            {
                var inbound = new Inbound { ClientId = client.Id, CreatedAt = DateTime.UtcNow };
                var detail = new InboundDetail
                {
                    ProductId = req.ProductId,
                    SectionId = req.SectionId,
                    Cartons = req.Cartons,
                    Pallets = req.Pallets,
                    Quantity = req.Cartons + (req.Pallets * 100m)
                };
                inbound.Details.Add(detail);

                await _db.Inbounds.AddAsync(inbound);

                // update stocks
                var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.ClientId == client.Id && s.ProductId == req.ProductId && s.SectionId == req.SectionId);
                if (stock == null)
                {
                    stock = new Stock { ClientId = client.Id, ProductId = req.ProductId, SectionId = req.SectionId, Cartons = req.Cartons, Pallets = req.Pallets };
                    await _db.Stocks.AddAsync(stock);
                }
                else
                {
                    stock.Cartons += req.Cartons;
                    stock.Pallets += req.Pallets;
                    _db.Stocks.Update(stock);
                }

                var prodStock = await _db.ProductStocks.FirstOrDefaultAsync(ps => ps.ClientId == client.Id && ps.ProductId == req.ProductId);
                if (prodStock == null)
                {
                    prodStock = new ProductStock { ClientId = client.Id, ProductId = req.ProductId, Cartons = req.Cartons, Pallets = req.Pallets };
                    await _db.ProductStocks.AddAsync(prodStock);
                }
                else
                {
                    prodStock.Cartons += req.Cartons;
                    prodStock.Pallets += req.Pallets;
                    _db.ProductStocks.Update(prodStock);
                }

                var secStock = await _db.SectionStocks.FirstOrDefaultAsync(ss => ss.ClientId == client.Id && ss.SectionId == req.SectionId);
                if (secStock == null)
                {
                    secStock = new SectionStock { ClientId = client.Id, SectionId = req.SectionId, Cartons = req.Cartons, Pallets = req.Pallets };
                    await _db.SectionStocks.AddAsync(secStock);
                }
                else
                {
                    secStock.Cartons += req.Cartons;
                    secStock.Pallets += req.Pallets;
                    _db.SectionStocks.Update(secStock);
                }

                await _db.SaveChangesAsync();
                await tx2.CommitAsync();

                var savedInboundId = inbound.Id;
                var savedDetailId = detail.Id;
                return Ok(new { success = true, id = savedInboundId, detailId = savedDetailId });
            }
            catch (System.Exception ex)
            {
                await tx2.RollbackAsync();
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [Route("Inbound/DeleteLineAjax")]
        public async Task<IActionResult> DeleteLineAjax([FromBody] DeleteLineRequest req)
        {
            if (req == null) return BadRequest(new { success = false, error = "Request body required" });
            if (req.DetailId <= 0) return BadRequest(new { success = false, error = "DetailId required" });

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var detail = await _db.InboundDetails.FirstOrDefaultAsync(d => d.Id == req.DetailId);
                if (detail == null) return NotFound(new { success = false, error = "Detail not found" });

                var inbound = await _db.Inbounds.FirstOrDefaultAsync(i => i.Id == detail.InboundId);
                if (inbound == null) return NotFound(new { success = false, error = "Inbound not found" });

                // remove detail
                _db.InboundDetails.Remove(detail);

                // revert stocks (subtract the added quantities)
                var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.ClientId == inbound.ClientId && s.ProductId == detail.ProductId && s.SectionId == detail.SectionId);
                if (stock != null)
                {
                    stock.Cartons -= detail.Cartons;
                    stock.Pallets -= detail.Pallets;
                    if (stock.Cartons < 0) stock.Cartons = 0;
                    if (stock.Pallets < 0) stock.Pallets = 0;
                    _db.Stocks.Update(stock);
                }

                var prodStock = await _db.ProductStocks.FirstOrDefaultAsync(ps => ps.ClientId == inbound.ClientId && ps.ProductId == detail.ProductId);
                if (prodStock != null)
                {
                    prodStock.Cartons -= detail.Cartons;
                    prodStock.Pallets -= detail.Pallets;
                    if (prodStock.Cartons < 0) prodStock.Cartons = 0;
                    if (prodStock.Pallets < 0) prodStock.Pallets = 0;
                    _db.ProductStocks.Update(prodStock);
                }

                var secStock = await _db.SectionStocks.FirstOrDefaultAsync(ss => ss.ClientId == inbound.ClientId && ss.SectionId == detail.SectionId);
                if (secStock != null)
                {
                    secStock.Cartons -= detail.Cartons;
                    secStock.Pallets -= detail.Pallets;
                    if (secStock.Cartons < 0) secStock.Cartons = 0;
                    if (secStock.Pallets < 0) secStock.Pallets = 0;
                    _db.SectionStocks.Update(secStock);
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(new { success = true, id = req.DetailId });
            }
            catch (System.Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var inbounds = await _inboundService.GetAllInboundsAsync();
            var inbound = inbounds.FirstOrDefault(i => i.Id == id);
            if (inbound == null) return NotFound();

            var vm = _mapper.Map<InboundDetailsVM>(inbound);
            return View(vm);
        }

        [HttpGet]
        public IActionResult GetDelegatesForClient(int clientId)
        {
            var list = _db.Delegates.Where(d => d.ClientId == clientId).OrderBy(d => d.Name)
                .Select(d => new { value = d.Id, text = d.Name })
                .ToList();
            return Json(list);
        }

        [HttpGet]
        public IActionResult GetClientSections(int clientId)
        {
            if (clientId <= 0) return BadRequest(new { success = false, error = "clientId required" });

            var sections = _db.Stocks
                .Where(s => s.ClientId == clientId)
                .Include(s => s.Section)
                .Select(s => new { value = s.SectionId, text = s.Section != null ? s.Section.Name : string.Empty })
                .Distinct()
                .ToList();

            return Json(sections);
        }

    }
}