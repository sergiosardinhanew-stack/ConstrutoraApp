using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ConstrutoraApp.Data;
using ConstrutoraApp.Models;

namespace ConstrutoraApp.Controllers
{
    [Authorize]
    public class ContratosController : Controller
    {
        private readonly AppDbContext _context;

        public ContratosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Contratos
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Contratos.Include(c => c.Imovel);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Contratos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .Include(c => c.Imovel)
                .ThenInclude(i => i.Empreendimento)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        // GET: Contratos/Create
        public IActionResult Create()
        {
            var empreendimentos = _context.Empreendimentos.ToList();
            if (!empreendimentos.Any())
            {
                TempData["Warning"] = "É necessário cadastrar pelo menos um empreendimento antes de criar um contrato.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpreendimentoId"] = new SelectList(empreendimentos, "Id", "Nome", null);
            ViewData["ImovelId"] = new SelectList(new List<Imovel>(), "Id", "Numero", null);
            return View();
        }

        // GET: Contratos/GetImoveisByEmpreendimento
        [HttpGet]
        public async Task<IActionResult> GetImoveisByEmpreendimento(int empreendimentoId)
        {
            var imoveis = await _context.Imoveis
                .Where(i => i.EmpreendimentoId == empreendimentoId)
                .Select(i => new { i.Id, i.Numero, i.Tipo, i.Status })
                .ToListAsync();

            return Json(imoveis);
        }

        // POST: Contratos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ImovelId,Comprador,CPF_CNPJ,DataContrato,ValorVenda,FormaPagamento,Observacoes")] Contrato contrato)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contrato);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Recarregar dados para a view em caso de erro
            var empreendimentos = _context.Empreendimentos.ToList();
            ViewData["EmpreendimentoId"] = new SelectList(empreendimentos, "Id", "Nome", null);
            
            // Se houver ImovelId, carregar os imóveis do empreendimento correspondente
            int? empreendimentoIdParaView = null;
            if (contrato.ImovelId > 0)
            {
                var imovel = await _context.Imoveis.FindAsync(contrato.ImovelId);
                if (imovel != null)
                {
                    empreendimentoIdParaView = imovel.EmpreendimentoId;
                    var imoveis = await _context.Imoveis
                        .Where(i => i.EmpreendimentoId == imovel.EmpreendimentoId)
                        .ToListAsync();
                    ViewData["ImovelId"] = new SelectList(imoveis, "Id", "Numero", contrato.ImovelId);
                }
                else
                {
                    ViewData["ImovelId"] = new SelectList(new List<Imovel>(), "Id", "Numero", null);
                }
            }
            else
            {
                ViewData["ImovelId"] = new SelectList(new List<Imovel>(), "Id", "Numero", null);
            }
            
            // Passar o EmpreendimentoId para a view (para o JavaScript)
            if (empreendimentoIdParaView.HasValue)
            {
                ViewData["EmpreendimentoIdSelecionado"] = empreendimentoIdParaView.Value;
            }
            
            return View(contrato);
        }

        // GET: Contratos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato == null)
            {
                return NotFound();
            }
            ViewData["ImovelId"] = new SelectList(_context.Imoveis, "Id", "Numero", contrato.ImovelId);
            return View(contrato);
        }

        // POST: Contratos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ImovelId,Comprador,CPF_CNPJ,DataContrato,ValorVenda,FormaPagamento,Observacoes")] Contrato contrato)
        {
            if (id != contrato.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contrato);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContratoExists(contrato.Id))
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
            ViewData["ImovelId"] = new SelectList(_context.Imoveis, "Id", "Numero", contrato.ImovelId);
            return View(contrato);
        }

        // GET: Contratos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .Include(c => c.Imovel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato != null)
            {
                _context.Contratos.Remove(contrato);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContratoExists(int id)
        {
            return _context.Contratos.Any(e => e.Id == id);
        }
    }
}

