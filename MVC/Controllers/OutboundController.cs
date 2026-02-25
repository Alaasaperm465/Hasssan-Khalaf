using Microsoft.AspNetCore.Mvc;
using InfraStructure.Context;
using Hassann_Khala.Domain;
using Microsoft.EntityFrameworkCore;
using MVC.ViewModels.Outbound;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC.ViewModels.Ajax;

namespace MVC.Controllers
{
    public class OutboundController : Controller
    {
        private readonly DBContext _db;
        public OutboundController(DBContext db) => _db = db;

        public IActionResult Index()
        {
            var outbounds = _db.Outbounds.ToList();
            return View(outbounds);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new OutboundCreateVM
            {
                Clients = _db.Clients.OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList(),
                Products = _db.Products.OrderBy(p => p.Name).Select(p => new SelectListItem(p.Name, p.Id.ToString())).ToList(),
                Sections = _db.Sections.OrderBy(s => s.Name).Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList(),
                Delegates = Enumerable.Empty<SelectListItem>(),
                Details = Enumerable.Range(0, 6).Select(_ => new OutboundDetailVM()).ToList()
            };

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
                ModelState.AddModelError(string.Empty, "??? ????? ??? ???? ??? ?????.");
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
                        ModelState.AddModelError(string.Empty, $"?????? ???????? ?????? {l.ProductId} ?? ?????? {l.SectionId} ???? ?? ??????.");
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
                ModelState.AddModelError(nameof(vm.ClientId), "?????? ??? ?????.");
                return View(vm);
            }

            using var tx = _db.Database.BeginTransaction();
            try
            {
                var outbound = new Outbound { ClientId = vm.ClientId, CreatedAt = DateTime.UtcNow };
                if (vm.DelegateId.HasValue) outbound.DelegateId = vm.DelegateId.Value;

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

                _db.Outbounds.Add(outbound);
                _db.SaveChanges();
                tx.Commit();

                TempData["Success"] = "?? ????? ??? ??????.";
                return RedirectToAction("Index");
            }
            catch (System.Exception ex)
            {
                tx.Rollback();
                ModelState.AddModelError(string.Empty, "??? ??? ????? ?????.");
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Outbound/AddLineAjax")]
        public IActionResult AddLineAjax([FromBody] AddLineRequest req)
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
                return BadRequest(new { success = false, error = "?????? ???????? ???? ?? ??????" });
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
            catch (System.Exception)
            {
                tx.Rollback();
                return StatusCode(500, new { success = false, error = "Server error" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
    }
}