using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ConstrutoraApp.Data;
using ConstrutoraApp.Models;

namespace ConstrutoraApp.Controllers
{
    public class ImoveisController : Controller
    {
        private readonly AppDbContext _context;

        public ImoveisController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Imoveis
        public async Task<IActionResult> Index(string statusFiltro)
        {
            var imoveisQuery = _context.Imoveis.Include(i => i.Empreendimento).AsQueryable();

            // Aplicar filtro de status se fornecido
            if (!string.IsNullOrEmpty(statusFiltro))
            {
                imoveisQuery = imoveisQuery.Where(i => i.Status == statusFiltro);
            }

            var imoveis = await imoveisQuery.ToListAsync();

            // Calcular valor recebido para cada imóvel em uma única consulta
            var idsImoveis = imoveis.Select(i => i.Id).ToList();
            var valoresRecebidos = await _context.Entradas
                .Where(e => e.ImovelId.HasValue && idsImoveis.Contains(e.ImovelId.Value))
                .GroupBy(e => e.ImovelId.Value)
                .Select(g => new { ImovelId = g.Key, ValorRecebido = g.Sum(e => e.Valor) })
                .ToDictionaryAsync(x => x.ImovelId, x => x.ValorRecebido);

            // Criar dicionário completo com todos os imóveis (incluindo os que não têm entradas)
            var valoresCompletos = new Dictionary<int, decimal>();
            foreach (var imovel in imoveis)
            {
                valoresCompletos[imovel.Id] = valoresRecebidos.ContainsKey(imovel.Id) 
                    ? valoresRecebidos[imovel.Id] 
                    : 0;
            }

            ViewBag.ValoresRecebidos = valoresCompletos;
            ViewBag.StatusFiltro = statusFiltro;
            return View(imoveis);
        }

        // GET: Imoveis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imovel = await _context.Imoveis
                .Include(i => i.Empreendimento)
                .Include(i => i.Contrato)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (imovel == null)
            {
                return NotFound();
            }

            // Calcular valor recebido para este imóvel
            var valorRecebido = await _context.Entradas
                .Where(e => e.ImovelId == imovel.Id)
                .SumAsync(e => (decimal?)e.Valor) ?? 0;

            ViewBag.ValorRecebido = valorRecebido;

            return View(imovel);
        }

        // GET: Imoveis/Create
        public IActionResult Create()
        {
            ViewData["EmpreendimentoId"] = new SelectList(_context.Empreendimentos, "Id", "Nome");
            return View();
        }

        // POST: Imoveis/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpreendimentoId,Tipo,Numero,Metragem,Quartos,Andar,ValorVenda,Status")] Imovel imovel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(imovel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpreendimentoId"] = new SelectList(_context.Empreendimentos, "Id", "Nome", imovel.EmpreendimentoId);
            return View(imovel);
        }

        // GET: Imoveis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imovel = await _context.Imoveis.FindAsync(id);
            if (imovel == null)
            {
                return NotFound();
            }
            ViewData["EmpreendimentoId"] = new SelectList(_context.Empreendimentos, "Id", "Nome", imovel.EmpreendimentoId);
            return View(imovel);
        }

        // POST: Imoveis/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpreendimentoId,Tipo,Numero,Metragem,Quartos,Andar,ValorVenda,Status")] Imovel imovel)
        {
            if (id != imovel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(imovel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImovelExists(imovel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpreendimentoId"] = new SelectList(_context.Empreendimentos, "Id", "Nome", imovel.EmpreendimentoId);
            return View(imovel);
        }

        // GET: Imoveis/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imovel = await _context.Imoveis
                .Include(i => i.Empreendimento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (imovel == null)
            {
                return NotFound();
            }

            return View(imovel);
        }

        // POST: Imoveis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var imovel = await _context.Imoveis.FindAsync(id);
            if (imovel != null)
            {
                _context.Imoveis.Remove(imovel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ImovelExists(int id)
        {
            return _context.Imoveis.Any(e => e.Id == id);
        }
    }
}

