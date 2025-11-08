using System.Diagnostics;
using ConstrutoraApp.Models;
using ConstrutoraApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConstrutoraApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalEmpreendimentos = await _context.Empreendimentos.CountAsync();
            
            var totalEntradas = await _context.Entradas.SumAsync(e => (decimal?)e.Valor) ?? 0;
            var totalCustos = await _context.Custos.SumAsync(c => (decimal?)c.Valor) ?? 0;
            var saldo = totalEntradas - totalCustos;
            
            var totalVendas = await _context.Contratos.SumAsync(c => (decimal?)c.ValorVenda) ?? 0;

            ViewBag.TotalEmpreendimentos = totalEmpreendimentos;
            ViewBag.Saldo = saldo;
            ViewBag.TotalVendas = totalVendas;
            ViewBag.TotalEntradas = totalEntradas;
            ViewBag.TotalCustos = totalCustos;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
