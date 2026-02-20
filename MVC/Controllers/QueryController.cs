using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfraStructure.Context;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
//using Application.DTOs;
using MVC.ViewModels;
using System.Collections.Generic;
using System.Text;

namespace MVC.Controllers
{
    public class QueryController : Controller
    {
        private readonly DBContext _db;
        private readonly ILogger<QueryController> _logger;

        public QueryController(DBContext db, ILogger<QueryController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ClientTotal()
        {
            ViewBag.Clients = await _db.Clients
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            return View("ClientTotal");
        }

        // JSON endpoint: detailed breakdown for a section
        [HttpGet]
        public async Task<IActionResult> SectionDetails(int sectionId)
        {
            if (sectionId <= 0)
            {
                return BadRequest(new { message = "Invalid sectionId" });
            }

            var sectionExists = await _db.Sections.AnyAsync(s => s.Id == sectionId);
            if (!sectionExists) return NotFound(new { message = "Section not found" });

            try
            {
                var inbound = await (from d in _db.InboundDetails
                                     join i in _db.Inbounds on d.InboundId equals i.Id
                                     where d.SectionId == sectionId
                                     group d by new { i.ClientId, d.ProductId } into g
                                     select new
                                     {
                                         ClientId = g.Key.ClientId,
                                         ProductId = g.Key.ProductId,
                                         Cartons = g.Sum(x => (int?)x.Cartons) ?? 0,
                                         Pallets = g.Sum(x => (int?)x.Pallets) ?? 0
                                     }).ToListAsync();

                var outbound = await (from d in _db.OutboundDetails
                                      join o in _db.Outbounds on d.OutboundId equals o.Id
                                      where d.SectionId == sectionId
                                      group d by new { o.ClientId, d.ProductId } into g
                                      select new
                                      {
                                          ClientId = g.Key.ClientId,
                                          ProductId = g.Key.ProductId,
                                          Cartons = g.Sum(x => (int?)x.Cartons) ?? 0,
                                          Pallets = g.Sum(x => (int?)x.Pallets) ?? 0
                                      }).ToListAsync();

                var map = new Dictionary<int, Dictionary<int, (int cartons, int pallets)>>();

                foreach (var i in inbound)
                {
                    if (!map.ContainsKey(i.ClientId)) map[i.ClientId] = new Dictionary<int, (int, int)>();
                    map[i.ClientId][i.ProductId] = (i.Cartons, i.Pallets);
                }

                foreach (var o in outbound)
                {
                    if (!map.ContainsKey(o.ClientId)) map[o.ClientId] = new Dictionary<int, (int, int)>();
                    var prodMap = map[o.ClientId];
                    if (!prodMap.ContainsKey(o.ProductId)) prodMap[o.ProductId] = (0, 0);
                    var current = prodMap[o.ProductId];
                    prodMap[o.ProductId] = (current.cartons - o.Cartons, current.pallets - o.Pallets);
                }

                var result = new List<SectionClientDetailDto>();
                var clientIds = map.Keys.ToList();
                var clients = await _db.Clients.Where(c => clientIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name);
                var productIds = map.Values.SelectMany(d => d.Keys).Distinct().ToList();
                var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name);

                foreach (var kv in map)
                {
                    var clientId = kv.Key;
                    var prodMap = kv.Value;
                    var clientName = clients.ContainsKey(clientId) ? clients[clientId] : "-";
                    var clientDetail = new SectionClientDetailDto
                    {
                        ClientId = clientId,
                        ClientName = clientName,
                        TotalCartons = prodMap.Values.Sum(x => x.cartons),
                        TotalPallets = prodMap.Values.Sum(x => x.pallets),
                        Products = prodMap.Select(p => new ProductDetailDto
                        {
                            ProductId = p.Key,
                            ProductName = products.ContainsKey(p.Key) ? products[p.Key] : "-",
                            Cartons = p.Value.cartons,
                            Pallets = p.Value.pallets
                        }).OrderByDescending(p => p.Cartons).ToList()
                    };
                    result.Add(clientDetail);
                }

                // Compute overall totals for the section (use sums of client totals)
                var totalCartons = result.Sum(r => r.TotalCartons);
                var totalPallets = result.Sum(r => r.TotalPallets);

                var payload = new
                {
                    Clients = result,
                    Totals = new { TotalCartons = totalCartons, TotalPallets = totalPallets }
                };

                return Json(payload, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error while generating section details for sectionId={SectionId}", sectionId);
                return StatusCode(500, new { message = "An error occurred while processing the request" });
            }
        }

        // CSV export for section details
        [HttpGet]
        public async Task<IActionResult> SectionDetailsCsv(int sectionId)
        {
            if (sectionId <= 0) return BadRequest("Invalid sectionId");
            var sectionExists = await _db.Sections.AnyAsync(s => s.Id == sectionId);
            if (!sectionExists) return NotFound();

            // Reuse SectionDetails logic to build simple CSV
            var detailsResp = await SectionDetails(sectionId) as JsonResult;
            if (detailsResp == null)
            {
                return StatusCode(500);
            }

            // Extract payload
            var json = System.Text.Json.JsonSerializer.Serialize(detailsResp.Value);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            // CSV header
            var sb = new StringBuilder();
            sb.AppendLine("ClientId,ClientName,TotalCartons,TotalPallets,ProductId,ProductName,Cartons,Pallets");

            if (root.TryGetProperty("clients", out var clientsProp))
            {
                foreach (var c in clientsProp.EnumerateArray())
                {
                    var clientId = c.GetProperty("clientId").GetInt32();
                    var clientName = c.GetProperty("clientName").GetString() ?? string.Empty;
                    var clientTotalCartons = c.GetProperty("totalCartons").GetInt32();
                    var clientTotalPallets = c.GetProperty("totalPallets").GetInt32();

                    if (c.TryGetProperty("products", out var productsProp))
                    {
                        foreach (var p in productsProp.EnumerateArray())
                        {
                            var prodId = p.GetProperty("productId").GetInt32();
                            var prodName = p.GetProperty("productName").GetString() ?? string.Empty;
                            var cartons = p.GetProperty("cartons").GetInt32();
                            var pallets = p.GetProperty("pallets").GetInt32();

                            sb.AppendLine($"{clientId},\"{EscapeCsv(clientName)}\",{clientTotalCartons},{clientTotalPallets},{prodId},\"{EscapeCsv(prodName)}\",{cartons},{pallets}");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"{clientId},\"{EscapeCsv(clientName)}\",{clientTotalCartons},{clientTotalPallets},,,,,");
                    }
                }
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"section_{sectionId}_details.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        private static string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("\"", "\"\"");
        }


        // existing MVC views and POST methods...
        // 1) Total stock for a section
        [HttpGet]
        public async Task<IActionResult> SectionTotal()
        {
            ViewBag.Sections = await _db.Sections
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            return View("SectionTotal");
        }
        [HttpPost]
        public async Task<IActionResult> SectionTotal(int sectionId)
        {
            ViewBag.Sections = await _db.Sections
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            var exists = await _db.Sections.AnyAsync(s => s.Id == sectionId);
            if (!exists) return NotFound();

            var inboundCartons = await _db.InboundDetails
                .Where(d => d.SectionId == sectionId)
                .SumAsync(d => (int?)d.Cartons) ?? 0;

            var inboundPallets = await _db.InboundDetails
                .Where(d => d.SectionId == sectionId)
                .SumAsync(d => (int?)d.Pallets) ?? 0;

            var outboundCartons = await _db.OutboundDetails
                .Where(d => d.SectionId == sectionId)
                .SumAsync(d => (int?)d.Cartons) ?? 0;

            var outboundPallets = await _db.OutboundDetails
                .Where(d => d.SectionId == sectionId)
                .SumAsync(d => (int?)d.Pallets) ?? 0;

            var model = new SectionStockDTO
            {
                SectionId = sectionId,
                TotalCartons = inboundCartons - outboundCartons,
                TotalPallets = inboundPallets - outboundPallets
            };

            return View("SectionTotal", model);
        }

        // 2) Client in Section
        [HttpGet]
        public async Task<IActionResult> ClientInSection()
        {
            ViewBag.Sections = await _db.Sections
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            ViewBag.Clients = await _db.Clients
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            return View("ClientInSection");
        }
        [HttpPost]
        public async Task<IActionResult> ClientInSection(int sectionId, int clientId)
        {
            ViewBag.Sections = await _db.Sections
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            ViewBag.Clients = await _db.Clients
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            var sectionExists = await _db.Sections.AnyAsync(s => s.Id == sectionId);
            var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId);

            if (!sectionExists || !clientExists) return NotFound();

            var inboundCartons = await (from d in _db.InboundDetails
                                        join i in _db.Inbounds on d.InboundId equals i.Id
                                        where d.SectionId == sectionId && i.ClientId == clientId
                                        select (int?)d.Cartons).SumAsync() ?? 0;

            var inboundPallets = await (from d in _db.InboundDetails
                                        join i in _db.Inbounds on d.InboundId equals i.Id
                                        where d.SectionId == sectionId && i.ClientId == clientId
                                        select (int?)d.Pallets).SumAsync() ?? 0;

            var outboundCartons = await (from d in _db.OutboundDetails
                                         join o in _db.Outbounds on d.OutboundId equals o.Id
                                         where d.SectionId == sectionId && o.ClientId == clientId
                                         select (int?)d.Cartons).SumAsync() ?? 0;

            var outboundPallets = await (from d in _db.OutboundDetails
                                         join o in _db.Outbounds on d.OutboundId equals o.Id
                                         where d.SectionId == sectionId && o.ClientId == clientId
                                         select (int?)d.Pallets).SumAsync() ?? 0;

            var model = new ClientSectionDTO
            {
                ClientId = clientId,
                SectionId = sectionId,
                TotalCartons = inboundCartons - outboundCartons,
                TotalPallets = inboundPallets - outboundPallets
            };

            return View("ClientInSection", model);
        }

        // 3) Client + Product in Section
        [HttpGet]
        public async Task<IActionResult> ClientProductInSection()
        {
            ViewBag.Sections = await _db.Sections
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            ViewBag.Clients = await _db.Clients
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            ViewBag.Products = await _db.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();

            return View("ClientProductInSection");
        }
        [HttpPost]
        public async Task<IActionResult> ClientProductInSection(int sectionId, int clientId, int productId)
        {
            ViewBag.Sections = await _db.Sections
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            ViewBag.Clients = await _db.Clients
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            ViewBag.Products = await _db.Products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();

            var exists = await _db.Sections.AnyAsync(s => s.Id == sectionId)
                && await _db.Clients.AnyAsync(c => c.Id == clientId)
                && await _db.Products.AnyAsync(p => p.Id == productId);

            if (!exists) return NotFound();

            var inboundCartons = await (from d in _db.InboundDetails
                                        join i in _db.Inbounds on d.InboundId equals i.Id
                                        where d.SectionId == sectionId
                                           && i.ClientId == clientId
                                           && d.ProductId == productId
                                        select (int?)d.Cartons).SumAsync() ?? 0;

            var inboundPallets = await (from d in _db.InboundDetails
                                        join i in _db.Inbounds on d.InboundId equals i.Id
                                        where d.SectionId == sectionId
                                           && i.ClientId == clientId
                                           && d.ProductId == productId
                                        select (int?)d.Pallets).SumAsync() ?? 0;

            var outboundCartons = await (from d in _db.OutboundDetails
                                         join o in _db.Outbounds on d.OutboundId equals o.Id
                                         where d.SectionId == sectionId
                                           && o.ClientId == clientId
                                           && d.ProductId == productId
                                         select (int?)d.Cartons).SumAsync() ?? 0;

            var outboundPallets = await (from d in _db.OutboundDetails
                                         join o in _db.Outbounds on d.OutboundId equals o.Id
                                         where d.SectionId == sectionId
                                           && o.ClientId == clientId
                                           && d.ProductId == productId
                                         select (int?)d.Pallets).SumAsync() ?? 0;

            var model = new ClientProductSectionViewModel
            {
                ClientId = clientId,
                ProductId = productId,
                SectionId = sectionId,
                Cartons = inboundCartons - outboundCartons,
                Pallets = inboundPallets - outboundPallets
            };

            return View("ClientProductInSection", model);
        }

        //  Client Totals CSV export
        [HttpGet]
        public async Task<IActionResult> ClientTotalCsv(int clientId)
        {
            if (clientId <= 0) return BadRequest("Invalid clientId");
            var clientExists = await _db.Clients.AnyAsync(c => c.Id == clientId);
            if (!clientExists) return NotFound();

            try
            {
                // compute totals across all sections
                var inbound = await (from d in _db.InboundDetails
                                     join i in _db.Inbounds on d.InboundId equals i.Id
                                     where i.ClientId == clientId
                                     group d by new { d.ProductId } into g
                                     select new
                                     {
                                         ProductId = g.Key.ProductId,
                                         Cartons = g.Sum(x => (int?)x.Cartons) ?? 0,
                                         Pallets = g.Sum(x => (int?)x.Pallets) ?? 0
                                     }).ToListAsync();

                var outbound = await (from d in _db.OutboundDetails
                                      join o in _db.Outbounds on d.OutboundId equals o.Id
                                      where o.ClientId == clientId
                                      group d by new { d.ProductId } into g
                                      select new
                                      {
                                          ProductId = g.Key.ProductId,
                                          Cartons = g.Sum(x => (int?)x.Cartons) ?? 0,
                                          Pallets = g.Sum(x => (int?)x.Pallets) ?? 0
                                      }).ToListAsync();

                var prodMap = new Dictionary<int, (int cartons, int pallets)>();
                foreach (var i in inbound)
                {
                    prodMap[i.ProductId] = (i.Cartons, i.Pallets);
                }
                foreach (var o in outbound)
                {
                    if (!prodMap.ContainsKey(o.ProductId)) prodMap[o.ProductId] = (0, 0);
                    var cur = prodMap[o.ProductId];
                    prodMap[o.ProductId] = (cur.cartons - o.Cartons, cur.pallets - o.Pallets);
                }

                var productIds = prodMap.Keys.ToList();
                var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Name);

                var sb = new StringBuilder();
                sb.AppendLine("ProductId,ProductName,Cartons,Pallets");
                foreach (var kv in prodMap)
                {
                    var pid = kv.Key;
                    var cartons = kv.Value.cartons;
                    var pallets = kv.Value.pallets;
                    var pname = products.ContainsKey(pid) ? products[pid] : "-";
                    sb.AppendLine($"{pid},\"{EscapeCsv(pname)}\",{cartons},{pallets}");
                }

                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var fileName = $"client_{clientId}_totals.csv";
                return File(bytes, "text/csv; charset=utf-8", fileName);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error generating client CSV for clientId={ClientId}", clientId);
                return StatusCode(500, "An error occurred");
            }
        }

        // DTOs for JSON endpoint
        public class SectionClientDetailDto
        {
            public int ClientId { get; set; }
            public string ClientName { get; set; } = string.Empty;
            public int TotalCartons { get; set; }
            public int TotalPallets { get; set; }
            public List<ProductDetailDto> Products { get; set; } = new List<ProductDetailDto>();
        }

        public class ProductDetailDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int Cartons { get; set; }
            public int Pallets { get; set; }
        }
    }
}
