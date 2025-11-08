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
    public class EntradasController : Controller
    {
        private readonly AppDbContext _context;

        public EntradasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Entradas
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Entradas.Include(e => e.Empreendimento);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Entradas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entrada = await _context.Entradas
                .Include(e => e.Empreendimento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entrada == null)
            {
                return NotFound();
            }

            return View(entrada);
        }

        // GET: Entradas/Create
        public IActionResult Create()
        {
            var empreendimentos = _context.Empreendimentos.ToList();
            if (!empreendimentos.Any())
            {
                TempData["Warning"] = "É necessário cadastrar pelo menos um empreendimento antes de criar uma entrada.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpreendimentoId"] = new SelectList(empreendimentos, "Id", "Nome", null);
            ViewData["ImovelId"] = new SelectList(new List<Imovel>(), "Id", "Numero", null);
            return View();
        }

        // GET: Entradas/GetImoveisByEmpreendimento
        [HttpGet]
        public async Task<IActionResult> GetImoveisByEmpreendimento(int empreendimentoId)
        {
            var imoveis = await _context.Imoveis
                .Where(i => i.EmpreendimentoId == empreendimentoId)
                .Select(i => new { i.Id, i.Numero, i.Tipo, i.Status })
                .ToListAsync();

            return Json(imoveis);
        }

        // POST: Entradas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpreendimentoId,ImovelId,Tipo,Descricao,Valor,Data")] Entrada entrada)
        {
            if (ModelState.IsValid)
            {
                _context.Add(entrada);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Recarregar dados para a view em caso de erro
            var empreendimentos = _context.Empreendimentos.ToList();
            ViewData["EmpreendimentoId"] = new SelectList(empreendimentos, "Id", "Nome", entrada.EmpreendimentoId);
            
            // Se houver EmpreendimentoId, carregar os imóveis do empreendimento
            if (entrada.EmpreendimentoId > 0)
            {
                var imoveis = await _context.Imoveis
                    .Where(i => i.EmpreendimentoId == entrada.EmpreendimentoId)
                    .ToListAsync();
                ViewData["ImovelId"] = new SelectList(imoveis, "Id", "Numero", entrada.ImovelId);
            }
            else
            {
                ViewData["ImovelId"] = new SelectList(new List<Imovel>(), "Id", "Numero", null);
            }
            
            return View(entrada);
        }

        // GET: Entradas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entrada = await _context.Entradas.FindAsync(id);
            if (entrada == null)
            {
                return NotFound();
            }
            
            ViewData["EmpreendimentoId"] = new SelectList(_context.Empreendimentos, "Id", "Nome", entrada.EmpreendimentoId);
            
            // Carregar imóveis do empreendimento
            if (entrada.EmpreendimentoId > 0)
            {
                var imoveis = await _context.Imoveis
                    .Where(i => i.EmpreendimentoId == entrada.EmpreendimentoId)
                    .ToListAsync();
                ViewData["ImovelId"] = new SelectList(imoveis, "Id", "Numero", entrada.ImovelId);
            }
            else
            {
                ViewData["ImovelId"] = new SelectList(new List<Imovel>(), "Id", "Numero", null);
            }
            
            return View(entrada);
        }

        // POST: Entradas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpreendimentoId,ImovelId,Tipo,Descricao,Valor,Data")] Entrada entrada)
        {
            if (id != entrada.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entrada);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntradaExists(entrada.Id))
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
            
            // Recarregar dados para a view em caso de erro
            var empreendimentos = _context.Empreendimentos.ToList();
            ViewData["EmpreendimentoId"] = new SelectList(empreendimentos, "Id", "Nome", entrada.EmpreendimentoId);
            
            // Se houver EmpreendimentoId, carregar os imóveis do empreendimento
            if (entrada.EmpreendimentoId > 0)
            {
                var imoveis = await _context.Imoveis
                    .Where(i => i.EmpreendimentoId == entrada.EmpreendimentoId)
                    .ToListAsync();
                ViewData["ImovelId"] = new SelectList(imoveis, "Id", "Numero", entrada.ImovelId);
            }
            else
            {
                ViewData["ImovelId"] = new SelectList(new List<Imovel>(), "Id", "Numero", null);
            }
            
            return View(entrada);
        }

        // GET: Entradas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entrada = await _context.Entradas
                .Include(e => e.Empreendimento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entrada == null)
            {
                return NotFound();
            }

            return View(entrada);
        }

        // POST: Entradas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entrada = await _context.Entradas.FindAsync(id);
            if (entrada != null)
            {
                _context.Entradas.Remove(entrada);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EntradaExists(int id)
        {
            return _context.Entradas.Any(e => e.Id == id);
        }
    }
}

