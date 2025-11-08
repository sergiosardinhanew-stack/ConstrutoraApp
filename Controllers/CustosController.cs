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
    public class CustosController : Controller
    {
        private readonly AppDbContext _context;

        public CustosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Custos
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Custos.Include(c => c.Empreendimento);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Custos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var custo = await _context.Custos
                .Include(c => c.Empreendimento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (custo == null)
            {
                return NotFound();
            }

            return View(custo);
        }

        // GET: Custos/Create
        public IActionResult Create()
        {
            var empreendimentos = _context.Empreendimentos.ToList();
            if (!empreendimentos.Any())
            {
                TempData["Warning"] = "É necessário cadastrar pelo menos um empreendimento antes de criar um custo.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpreendimentoId"] = new SelectList(empreendimentos, "Id", "Nome", null);
            return View();
        }

        // POST: Custos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpreendimentoId,Tipo,Descricao,Valor,Data,Fornecedor,FormaPagamento,NumeroParcelas,ValorParcela,DataVencimentoParcela,DataVencimentoBoleto,Status")] Custo custo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(custo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpreendimentoId"] = new SelectList(_context.Empreendimentos, "Id", "Nome", custo.EmpreendimentoId);
            return View(custo);
        }

        // GET: Custos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var custo = await _context.Custos.FindAsync(id);
            if (custo == null)
            {
                return NotFound();
            }
            ViewData["EmpreendimentoId"] = new SelectList(_context.Empreendimentos, "Id", "Nome", custo.EmpreendimentoId);
            return View(custo);
        }

        // POST: Custos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpreendimentoId,Tipo,Descricao,Valor,Data,Fornecedor,FormaPagamento,NumeroParcelas,ValorParcela,DataVencimentoParcela,DataVencimentoBoleto,Status")] Custo custo)
        {
            if (id != custo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(custo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustoExists(custo.Id))
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
            ViewData["EmpreendimentoId"] = new SelectList(_context.Empreendimentos, "Id", "Nome", custo.EmpreendimentoId);
            return View(custo);
        }

        // GET: Custos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var custo = await _context.Custos
                .Include(c => c.Empreendimento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (custo == null)
            {
                return NotFound();
            }

            return View(custo);
        }

        // POST: Custos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var custo = await _context.Custos.FindAsync(id);
            if (custo != null)
            {
                _context.Custos.Remove(custo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustoExists(int id)
        {
            return _context.Custos.Any(e => e.Id == id);
        }
    }
}

