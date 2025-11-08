using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConstrutoraApp.Data;
using ConstrutoraApp.Models;

namespace ConstrutoraApp.Controllers
{
    public class RelatoriosController : Controller
    {
        private readonly AppDbContext _context;

        public RelatoriosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Relatorios
        public async Task<IActionResult> Index()
        {
            var totalEmpreendimentos = await _context.Empreendimentos.CountAsync();
            var totalImoveis = await _context.Imoveis.CountAsync();
            var totalContratos = await _context.Contratos.CountAsync();
            var totalEntradas = await _context.Entradas.SumAsync(e => (decimal?)e.Valor) ?? 0;
            var totalCustos = await _context.Custos.SumAsync(c => (decimal?)c.Valor) ?? 0;
            var saldo = totalEntradas - totalCustos;
            var totalVendas = await _context.Contratos.SumAsync(c => (decimal?)c.ValorVenda) ?? 0;

            ViewBag.TotalEmpreendimentos = totalEmpreendimentos;
            ViewBag.TotalImoveis = totalImoveis;
            ViewBag.TotalContratos = totalContratos;
            ViewBag.TotalEntradas = totalEntradas;
            ViewBag.TotalCustos = totalCustos;
            ViewBag.Saldo = saldo;
            ViewBag.TotalVendas = totalVendas;

            // Empreendimentos com mais custos
            var empreendimentosComCustos = await _context.Empreendimentos
                .Include(e => e.Custos)
                .Include(e => e.Entradas)
                .ToListAsync();

            var empreendimentosResumo = empreendimentosComCustos
                .Select(e => new
                {
                    Empreendimento = e,
                    TotalCustos = e.Custos?.Sum(c => c.Valor) ?? 0,
                    TotalEntradas = e.Entradas?.Sum(en => en.Valor) ?? 0
                })
                .OrderByDescending(x => x.TotalCustos)
                .Take(10)
                .ToList();

            ViewBag.EmpreendimentosComCustos = empreendimentosResumo;

            return View();
        }
    }
}

