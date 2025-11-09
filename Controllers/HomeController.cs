using System.Diagnostics;
using ConstrutoraApp.Models;
using ConstrutoraApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ConstrutoraApp.Controllers
{
    [Authorize]
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
            var hoje = DateTime.Today;
            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
            var fimMes = inicioMes.AddMonths(1).AddDays(-1);

            // Entradas do Dia - Pagamentos pagos hoje
            var entradasDia = await _context.Pagamentos
                .Where(p => p.Status == "Pago" && 
                           p.DataPagamento.HasValue && 
                           p.DataPagamento.Value.Date == hoje)
                .SumAsync(p => (decimal?)p.ValorParcela) ?? 0;

            // Saídas do Dia - Custos de hoje
            var saidasDia = await _context.Custos
                .Where(c => c.Data.Date == hoje)
                .SumAsync(c => (decimal?)c.Valor) ?? 0;

            // Entradas do Mês - Pagamentos pagos no mês atual
            var entradasMes = await _context.Pagamentos
                .Where(p => p.Status == "Pago" && 
                           p.DataPagamento.HasValue && 
                           p.DataPagamento.Value.Date >= inicioMes && 
                           p.DataPagamento.Value.Date <= fimMes)
                .SumAsync(p => (decimal?)p.ValorParcela) ?? 0;

            // Saídas do Mês - Custos do mês atual
            var saidasMes = await _context.Custos
                .Where(c => c.Data.Date >= inicioMes && c.Data.Date <= fimMes)
                .SumAsync(c => (decimal?)c.Valor) ?? 0;

            ViewBag.EntradasDia = entradasDia;
            ViewBag.SaidasDia = saidasDia;
            ViewBag.EntradasMes = entradasMes;
            ViewBag.SaidasMes = saidasMes;

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
