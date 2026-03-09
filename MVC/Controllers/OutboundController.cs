using Microsoft.AspNetCore.Mvc;
using InfraStructure.Context;
using Hassann_Khala.Domain;
using Microsoft.EntityFrameworkCore;
using MVC.ViewModels.Outbound;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC.ViewModels.Ajax;
using Microsoft.Extensions.Logging;

namespace MVC.Controllers
{
    public class OutboundController : Controller
    {
        private readonly DBContext _db;
        private readonly ILogger<OutboundController> _logger;
        public OutboundController(DBContext db, ILogger<OutboundController> logger) { _db = db; _logger = logger; }

        public IActionResult Index()
        {
            var outbounds = _db.Outbounds.Include(o => o.Client).ToList();
            return View(outbounds);
        }

        [HttpGet]
        public IActionResult Create(int? clientId)
        {
            var vm = new OutboundCreateVM
            {
                Clients = _db.Clients.OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList(),
                Products = _db.Products.OrderBy(p => p.Name).Select(p => new SelectListItem(p.Name, p.Id.ToString())).ToList(),
                Sections = _db.Sections.OrderBy(s => s.Name).Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList(),
                Delegates = Enumerable.Empty<SelectListItem>(),
                Details = Enumerable.Range(0, 6).Select(_ => new OutboundDetailVM()).ToList()
            };

            // If clientId provided, pre-load delegates for that client so the select shows names server-side
            if (clientId.HasValue && clientId.Value > 0)
            {
                vm.Delegates = _db.Delegates.Where(d => d.ClientId == clientId.Value).OrderBy(d => d.Name)
                    .Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
                vm.ClientId = clientId.Value;

                // For outbound we still want sections limited to those with stock for this client
                var sectionIds = _db.Stocks.Where(s => s.ClientId == clientId.Value).Select(s => s.SectionId).Distinct().ToList();
                vm.Sections = _db.Sections.Where(s => sectionIds.Contains(s.Id)).OrderBy(s => s.Name).Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(OutboundCreateVM vm)
        {
            vm.Clients = _db.Clients.OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            vm.Products = _db.Products.OrderBy(p => p.Name).Select(p => new SelectListItem(p.Name, p.Id.ToString())).ToList();
            vm.Sections = _db.Sections.OrderBy(s => s.Name).Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();
            vm.Delegates = vm.ClientId > 0 ? _db.Delegates.Where(d => d.ClientId == vm.ClientId).OrderBy(d => d.Name).Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList() : Enumerable.Empty<SelectListItem>();

            if (vm.Details == null || vm.Details.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "??? ?? ????? ????? ??? ??? ???? ??? ?????.");
            }

            // validate stock availability for each detail
            if (vm.Details != null && vm.Details.Count > 0)
            {
                foreach (var l in vm.Details.Where(d => d != null))
                {
                    if (l.ProductId <= 0) continue; // skip empty lines
                    var stock = _db.Stocks.FirstOrDefault(s => s.ClientId == vm.ClientId && s.ProductId == l.ProductId && s.SectionId == l.SectionId);
                    var availableCartons = stock?.Cartons ?? 0;
                    var availablePallets = stock?.Pallets ?? 0;
                    if (l.Cartons > availableCartons || l.Pallets > availablePallets)
                    {
                        ModelState.AddModelError(string.Empty, $"?????? ???????? ?????? {l.ProductId} ?? ?????? {l.SectionId} ??? ?????.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var client = _db.Clients.FirstOrDefault(c => c.Id == vm.ClientId);
            if (client == null)
            {
                ModelState.AddModelError(nameof(vm.ClientId), "Selected client does not exist.");
                return View(vm);
            }

            using var tx = _db.Database.BeginTransaction();
            try
            {
                var outbound = new Outbound { ClientId = vm.ClientId, CreatedAt = DateTime.UtcNow };
                if (vm.DelegateId.HasValue) outbound.DelegateId = vm.DelegateId.Value;

                // save additional entry and notes
                outbound.AdditionalEntry = vm.AdditionalEntry;
                outbound.Notes = vm.Notes;

                // try set user info if available
                try
                {
                    outbound.UserId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : (int?)null;
                }
                catch { /* ignore */ }

                foreach (var l in vm.Details)
                {
                    var detail = new OutboundDetail
                    {
                        ProductId = l.ProductId,
                        SectionId = l.SectionId,
                        Cartons = l.Cartons,
                        Pallets = l.Pallets,
                        Quantity = l.Cartons + (l.Pallets * 100m)
                    };
                    outbound.Details.Add(detail);

                    var stock = _db.Stocks.FirstOrDefault(s => s.ClientId == vm.ClientId && s.ProductId == l.ProductId && s.SectionId == l.SectionId);
                    if (stock != null)
                    {
                        stock.Cartons -= l.Cartons;
                        stock.Pallets -= l.Pallets;
                        _db.Stocks.Update(stock);
                    }

                    var prodStock = _db.ProductStocks.FirstOrDefault(ps => ps.ClientId == vm.ClientId && ps.ProductId == l.ProductId);
                    if (prodStock != null)
                    {
                        prodStock.Cartons -= l.Cartons;
                        prodStock.Pallets -= l.Pallets;
                        _db.ProductStocks.Update(prodStock);
                    }

                    var secStock = _db.SectionStocks.FirstOrDefault(ss => ss.ClientId == vm.ClientId && ss.SectionId == l.SectionId);
                    if (secStock != null)
                    {
                        secStock.Cartons -= l.Cartons;
                        secStock.Pallets -= l.Pallets;
                        _db.SectionStocks.Update(secStock);
                    }
                }

                // apply AdditionalEntry effect to stocks if provided (Option B): treat as cartons to subtract from client totals
                if (vm.AdditionalEntry.HasValue && vm.AdditionalEntry.Value != 0)
                {
                    var add = vm.AdditionalEntry.Value;
                    // distribute across product/section totals: here we subtract from client's total ProductStock if exists (simple implementation)
                    var clientProdTotals = _db.ProductStocks.Where(ps => ps.ClientId == vm.ClientId).ToList();
                    if (clientProdTotals.Any())
                    {
                        // subtract from the first product stock as a simple heuristic
                        var ps = clientProdTotals.First();
                        ps.Cartons -= add;
                        if (ps.Cartons < 0) ps.Cartons = 0;
                        _db.ProductStocks.Update(ps);
                    }
                }

                _db.Outbounds.Add(outbound);
                _db.SaveChanges();
                tx.Commit();

                _logger.LogInformation("Created outbound {OutboundId} for client {ClientId}", outbound.Id, vm.ClientId);

                TempData["Success"] = "?? ??? ????? ?????.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Failed to create outbound for client {ClientId}", vm.ClientId);
                ModelState.AddModelError(string.Empty, "??? ?? ??? ?????.");
                return View(vm);
            }
        }

        [HttpPost]
        [Route("Outbound/AddLineAjax")]
        public IActionResult AddLineAjax([FromBody] AddLineRequest? req)
        {
            // support form-encoded fallback
            if (req == null && Request.HasFormContentType)
            {
                var f = Request.Form;
                req = new AddLineRequest
                {
                    OutboundId = int.TryParse(f["outboundId"], out var oid) ? oid : (int?)null,
                    ClientId = int.TryParse(f["clientId"], out var cid) ? cid : 0,
                    ProductId = int.TryParse(f["productId"], out var pid) ? pid : 0,
                    SectionId = int.TryParse(f["sectionId"], out var sid) ? sid : 0,
                    Cartons = int.TryParse(f["cartons"], out var c) ? c : 0,
                    Pallets = int.TryParse(f["pallets"], out var p) ? p : 0
                };
            }

            try
            {
                if (req == null) return BadRequest(new { success = false, error = "Request body required" });
                if (req.ClientId <= 0) return BadRequest(new { success = false, error = "ClientId required" });
                if (req.ProductId <= 0) return BadRequest(new { success = false, error = "ProductId required" });
                if (req.SectionId <= 0) return BadRequest(new { success = false, error = "SectionId required" });
                if (req.Cartons < 0 || req.Pallets < 0) return BadRequest(new { success = false, error = "Quantities must be non-negative" });

                var client = _db.Clients.FirstOrDefault(c => c.Id == req.ClientId);
                var product = _db.Products.FirstOrDefault(p => p.Id == req.ProductId && p.IsActive);
                var section = _db.Sections.FirstOrDefault(s => s.Id == req.SectionId);

                if (client == null || product == null || section == null)
                {
                    return BadRequest(new { success = false, error = "Invalid client/product/section" });
                }

                // check availability
                var stockRecord = _db.Stocks.FirstOrDefault(s => s.ClientId == req.ClientId && s.ProductId == req.ProductId && s.SectionId == req.SectionId);
                var availableCartons = stockRecord?.Cartons ?? 0;
                var availablePallets = stockRecord?.Pallets ?? 0;
                if (req.Cartons > availableCartons || req.Pallets > availablePallets)
                {
                    return BadRequest(new { success = false, error = "Insufficient stock" });
                }

                // If outboundId provided, append detail
                if (req.OutboundId.HasValue && req.OutboundId.Value > 0)
                {
                    var outbound = _db.Outbounds.Include(o => o.Details).FirstOrDefault(o => o.Id == req.OutboundId.Value);
                    if (outbound == null) return NotFound(new { success = false, error = "Outbound not found" });
                    if (outbound.ClientId != req.ClientId) return BadRequest(new { success = false, error = "Client mismatch" });

                    var detail = new OutboundDetail
                    {
                        OutboundId = outbound.Id,
                        ProductId = req.ProductId,
                        SectionId = req.SectionId,
                        Cartons = req.Cartons,
                        Pallets = req.Pallets,
                        Quantity = req.Cartons + (req.Pallets * 100m)
                    };
                    outbound.Details.Add(detail);

                    // reduce stock
                    if (stockRecord != null)
                    {
                        stockRecord.Cartons -= req.Cartons; stockRecord.Pallets -= req.Pallets; _db.Stocks.Update(stockRecord);
                    }

                    var prodStock = _db.ProductStocks.FirstOrDefault(ps => ps.ClientId == outbound.ClientId && ps.ProductId == req.ProductId);
                    if (prodStock != null) { prodStock.Cartons -= req.Cartons; prodStock.Pallets -= req.Pallets; _db.ProductStocks.Update(prodStock); }

                    var secStock = _db.SectionStocks.FirstOrDefault(ss => ss.ClientId == outbound.ClientId && ss.SectionId == req.SectionId);
                    if (secStock != null) { secStock.Cartons -= req.Cartons; secStock.Pallets -= req.Pallets; _db.SectionStocks.Update(secStock); }

                    _db.SaveChanges();

                    // find inserted detail id (last added)
                    var insertedDetail = outbound.Details.LastOrDefault();
                    return Ok(new { success = true, id = outbound.Id, detailId = insertedDetail?.Id });
                }

                // create new outbound with single line
                using var tx = _db.Database.BeginTransaction();
                try
                {
                    var outbound = new Outbound { ClientId = req.ClientId, CreatedAt = DateTime.UtcNow };
                    var detail = new OutboundDetail { ProductId = req.ProductId, SectionId = req.SectionId, Cartons = req.Cartons, Pallets = req.Pallets, Quantity = req.Cartons + (req.Pallets * 100m) };
                    outbound.Details.Add(detail);

                    _db.Outbounds.Add(outbound);

                    if (stockRecord != null) { stockRecord.Cartons -= req.Cartons; stockRecord.Pallets -= req.Pallets; _db.Stocks.Update(stockRecord); }

                    var prodStock2 = _db.ProductStocks.FirstOrDefault(ps => ps.ClientId == req.ClientId && ps.ProductId == req.ProductId);
                    if (prodStock2 != null) { prodStock2.Cartons -= req.Cartons; prodStock2.Pallets -= req.Pallets; _db.ProductStocks.Update(prodStock2); }

                    var secStock2 = _db.SectionStocks.FirstOrDefault(ss => ss.ClientId == req.ClientId && ss.SectionId == req.SectionId);
                    if (secStock2 != null) { secStock2.Cartons -= req.Cartons; secStock2.Pallets -= req.Pallets; _db.SectionStocks.Update(secStock2); }

                    _db.SaveChanges();
                    tx.Commit();

                    // get detail id
                    var savedOutbound = _db.Outbounds.Include(o => o.Details).FirstOrDefault(o => o.Id == outbound.Id);
                    var savedDetailId = savedOutbound?.Details?.FirstOrDefault()?.Id;

                    return Ok(new { success = true, id = outbound.Id, detailId = savedDetailId });
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    var inner = ex.GetBaseException()?.Message;
                    return StatusCode(500, new { success = false, error = ex.Message, detail = inner, stack = ex.StackTrace });
                }
            }
            catch (Exception ex)
            {
                var inner = ex.GetBaseException()?.Message;
                return StatusCode(500, new { success = false, error = ex.Message, detail = inner, stack = ex.StackTrace });
            }
        }

        [HttpPost]
        [Route("Outbound/DeleteLineAjax")]
        public IActionResult DeleteLineAjax([FromBody] MVC.ViewModels.Ajax.DeleteLineRequest req)
        {
            if (req == null) return BadRequest(new { success = false, error = "Request body required" });
            if (req.DetailId <= 0) return BadRequest(new { success = false, error = "DetailId required" });

            var detail = _db.OutboundDetails.FirstOrDefault(d => d.Id == req.DetailId);
            if (detail == null) return NotFound(new { success = false, error = "Detail not found" });

            var outbound = _db.Outbounds.Include(o => o.Details).FirstOrDefault(o => o.Id == detail.OutboundId);
            if (outbound == null) return NotFound(new { success = false, error = "Outbound not found" });

            // remove detail
            _db.OutboundDetails.Remove(detail);

            // revert stocks (add back quantities)
            var stock = _db.Stocks.FirstOrDefault(s => s.ClientId == outbound.ClientId && s.ProductId == detail.ProductId && s.SectionId == detail.SectionId);
            if (stock != null)
            {
                stock.Cartons += detail.Cartons;
                stock.Pallets += detail.Pallets;
                _db.Stocks.Update(stock);
            }

            var prodStock = _db.ProductStocks.FirstOrDefault(ps => ps.ClientId == outbound.ClientId && ps.ProductId == detail.ProductId);
            if (prodStock != null)
            {
                prodStock.Cartons += detail.Cartons;
                prodStock.Pallets += detail.Pallets;
                _db.ProductStocks.Update(prodStock);
            }

            var secStock = _db.SectionStocks.FirstOrDefault(ss => ss.ClientId == outbound.ClientId && ss.SectionId == detail.SectionId);
            if (secStock != null)
            {
                secStock.Cartons += detail.Cartons;
                secStock.Pallets += detail.Pallets;
                _db.SectionStocks.Update(secStock);
            }

            _db.SaveChanges();
            return Ok(new { success = true, id = detail.Id });
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
        public IActionResult Details(int id)
        {
            var outbound = _db.Outbounds.Include(o => o.Details).Include(o => o.Client).FirstOrDefault(o => o.Id == id);
            if (outbound == null) return NotFound();

            var vm = new OutboundDetailsVM
            {
                Id = outbound.Id,
                ClientName = outbound.Client?.Name ?? outbound.ClientId.ToString(),
                CreatedAt = outbound.CreatedAt,
                Details = outbound.Details.Select(d => new MVC.ViewModels.Inbound.InboundDetailVM
                {
                    Id = d.Id,
                    InboundId = d.OutboundId,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name,
                    SectionId = d.SectionId,
                    SectionName = d.Section?.Name,
                    Cartons = d.Cartons,
                    Pallets = d.Pallets,
                    Quantity = d.Quantity
                }).ToList()
            };

            return View(vm);
        }
        [HttpGet]
        public IActionResult GetAvailableStock(int clientId, int productId, int sectionId)
        {
            if (clientId <= 0 || productId <= 0 || sectionId <= 0)
                return Json(new { cartons = 0, pallets = 0 });

            var stock = _db.Stocks
                .FirstOrDefault(s =>
                    s.ClientId == clientId &&
                    s.ProductId == productId &&
                    s.SectionId == sectionId);

            return Json(new
            {
                cartons = stock?.Cartons ?? 0,
                pallets = stock?.Pallets ?? 0
            });
        }
    }
}