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
    public class EmpreendimentosController : Controller
    {
        private readonly AppDbContext _context;

        public EmpreendimentosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Empreendimentos
        public async Task<IActionResult> Index()
        {
            var empreendimentos = await _context.Empreendimentos
                .Include(e => e.Imoveis)
                .ToListAsync();

            // Calcular imóveis por status para cada empreendimento
            var imoveisPorEmpreendimento = new Dictionary<int, Dictionary<string, int>>();
            foreach (var empreendimento in empreendimentos)
            {
                var imoveisPorStatus = empreendimento.Imoveis?
                    .GroupBy(i => i.Status ?? "Sem Status")
                    .Select(g => new { Status = g.Key, Quantidade = g.Count() })
                    .ToDictionary(x => x.Status, x => x.Quantidade) ?? new Dictionary<string, int>();
                
                imoveisPorEmpreendimento[empreendimento.Id] = imoveisPorStatus;
            }

            ViewBag.ImoveisPorEmpreendimento = imoveisPorEmpreendimento;
            return View(empreendimentos);
        }

        // GET: Empreendimentos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empreendimento = await _context.Empreendimentos
                .Include(e => e.Imoveis)
                .Include(e => e.Custos)
                .Include(e => e.Entradas)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empreendimento == null)
            {
                return NotFound();
            }

            // Calcular imóveis por status
            var imoveisPorStatus = empreendimento.Imoveis?
                .GroupBy(i => i.Status ?? "Sem Status")
                .Select(g => new { Status = g.Key, Quantidade = g.Count() })
                .ToDictionary(x => x.Status, x => x.Quantidade) ?? new Dictionary<string, int>();

            ViewBag.ImoveisPorStatus = imoveisPorStatus;
            ViewBag.TotalImoveis = empreendimento.Imoveis?.Count ?? 0;

            return View(empreendimento);
        }

        // GET: Empreendimentos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Empreendimentos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Tipo,Endereco,Descricao,DataInicio,DataEntrega,Status,ValorPrevisto,ValorRealizado")] Empreendimento empreendimento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(empreendimento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Se houver erros de validação, retornar a view com os erros
            return View(empreendimento);
        }

        // GET: Empreendimentos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empreendimento = await _context.Empreendimentos.FindAsync(id);
            if (empreendimento == null)
            {
                return NotFound();
            }
            return View(empreendimento);
        }

        // POST: Empreendimentos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Tipo,Endereco,Descricao,DataInicio,DataEntrega,Status,ValorPrevisto,ValorRealizado")] Empreendimento empreendimento)
        {
            if (id != empreendimento.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empreendimento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpreendimentoExists(empreendimento.Id))
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
            return View(empreendimento);
        }

        // GET: Empreendimentos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empreendimento = await _context.Empreendimentos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empreendimento == null)
            {
                return NotFound();
            }

            return View(empreendimento);
        }

        // POST: Empreendimentos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empreendimento = await _context.Empreendimentos.FindAsync(id);
            if (empreendimento != null)
            {
                _context.Empreendimentos.Remove(empreendimento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmpreendimentoExists(int id)
        {
            return _context.Empreendimentos.Any(e => e.Id == id);
        }
    }
}

