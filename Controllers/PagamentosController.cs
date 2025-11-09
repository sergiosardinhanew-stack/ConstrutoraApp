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
    public class PagamentosController : Controller
    {
        private readonly AppDbContext _context;

        public PagamentosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pagamentos
        public async Task<IActionResult> Index(int? empreendimentoId, int? imovelId)
        {
            var pagamentosQuery = _context.Pagamentos
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Empreendimento)
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Imovel)
                .Where(p => p.Status == "Pago") // Mostrar apenas pagamentos pagos
                .AsQueryable();

            // Aplicar filtro de empreendimento se fornecido
            if (empreendimentoId.HasValue)
            {
                pagamentosQuery = pagamentosQuery.Where(p => p.Parcelamento != null && 
                                                             p.Parcelamento.Entrada != null && 
                                                             p.Parcelamento.Entrada.EmpreendimentoId == empreendimentoId.Value);
            }

            // Aplicar filtro de imóvel se fornecido
            if (imovelId.HasValue)
            {
                pagamentosQuery = pagamentosQuery.Where(p => p.Parcelamento != null && 
                                                             p.Parcelamento.Entrada != null && 
                                                             p.Parcelamento.Entrada.ImovelId == imovelId.Value);
            }

            var pagamentos = await pagamentosQuery
                .OrderByDescending(p => p.DataPagamento ?? p.DataVencimento)
                .ToListAsync();

            // Carregar empreendimentos e imóveis para os filtros
            var empreendimentos = await _context.Empreendimentos.ToListAsync();
            ViewBag.Empreendimentos = new SelectList(empreendimentos, "Id", "Nome", empreendimentoId);

            var imoveis = new List<Imovel>();
            if (empreendimentoId.HasValue)
            {
                imoveis = await _context.Imoveis
                    .Where(i => i.EmpreendimentoId == empreendimentoId.Value && i.Status == "Vendido")
                    .ToListAsync();
            }
            ViewBag.Imoveis = new SelectList(imoveis, "Id", "Numero", imovelId);

            ViewBag.EmpreendimentoId = empreendimentoId;
            ViewBag.ImovelId = imovelId;
            return View(pagamentos);
        }

        // GET: Pagamentos/GetPendentes
        [HttpGet]
        public async Task<IActionResult> GetPendentes(int? empreendimentoId, int? imovelId)
        {
            var pagamentosQuery = _context.Pagamentos
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Empreendimento)
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Imovel)
                .Where(p => p.Status == "Pendente")
                .AsQueryable();

            // Aplicar filtro de empreendimento se fornecido
            if (empreendimentoId.HasValue)
            {
                pagamentosQuery = pagamentosQuery.Where(p => p.Parcelamento != null && 
                                                             p.Parcelamento.Entrada != null && 
                                                             p.Parcelamento.Entrada.EmpreendimentoId == empreendimentoId.Value);
            }

            // Aplicar filtro de imóvel se fornecido
            if (imovelId.HasValue)
            {
                pagamentosQuery = pagamentosQuery.Where(p => p.Parcelamento != null && 
                                                             p.Parcelamento.Entrada != null && 
                                                             p.Parcelamento.Entrada.ImovelId == imovelId.Value);
            }

            var pagamentos = await pagamentosQuery
                .OrderBy(p => p.DataVencimento)
                .Select(p => new
                {
                    id = p.Id,
                    entradaId = p.Parcelamento != null && p.Parcelamento.Entrada != null ? p.Parcelamento.Entrada.Id : 0,
                    empreendimento = p.Parcelamento != null && p.Parcelamento.Entrada != null && p.Parcelamento.Entrada.Empreendimento != null ? p.Parcelamento.Entrada.Empreendimento.Nome : "",
                    imovel = p.Parcelamento != null && p.Parcelamento.Entrada != null && p.Parcelamento.Entrada.Imovel != null ? p.Parcelamento.Entrada.Imovel.Numero : "",
                    tipo = p.Parcelamento != null ? p.Parcelamento.TipoPagamento : "",
                    numeroParcela = p.NumeroParcela,
                    valorParcela = p.ValorParcela,
                    numeroParcelas = p.Parcelamento != null ? p.Parcelamento.NumeroParcelas : 1,
                    valorTotal = p.Parcelamento != null ? p.Parcelamento.ValorTotal : p.ValorParcela,
                    data = p.DataVencimento.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(pagamentos);
        }

        // POST: Pagamentos/MarcarComoPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarComoPago([FromBody] MarcarComoPagoModel model)
        {
            if (model == null)
            {
                return BadRequest("Dados inválidos");
            }

            var pagamento = await _context.Pagamentos
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada)
                .FirstOrDefaultAsync(p => p.Id == model.Id);
            
            if (pagamento == null)
            {
                return NotFound();
            }

            try
            {
                // Marcar pagamento como pago
                pagamento.Status = "Pago";
                
                // Se o valor pago for maior que 0, usar esse valor (pode ser diferente do valor original se houver ajuste)
                // Se for 0, manter o valor original da parcela (não atualizar o ValorParcela)
                // O ValorParcela já contém o valor correto da parcela quando foi criado
                if (model.ValorPago > 0)
                {
                    pagamento.ValorParcela = model.ValorPago;
                }
                // Caso contrário (valorPago == 0), manter o ValorParcela original que já está no banco
                // Isso acontece quando é parcelado e o valor já está correto no ValorParcela
                
                pagamento.DataPagamento = model.DataPagamento;

                _context.Update(pagamento);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Pagamento marcado como pago com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao atualizar: " + ex.Message });
            }
        }

        // GET: Pagamentos/Create - Não usado mais (pagamentos são criados automaticamente)
        public async Task<IActionResult> Create(int parcelamentoId)
        {
            var parcelamento = await _context.Parcelamentos
                .Include(p => p.Entrada!)
                    .ThenInclude(e => e!.Empreendimento)
                .Include(p => p.Entrada!)
                    .ThenInclude(e => e!.Imovel)
                .FirstOrDefaultAsync(p => p.Id == parcelamentoId);
            
            if (parcelamento == null)
            {
                return NotFound();
            }

            ViewBag.Parcelamento = parcelamento;
            return View(new Pagamento 
            { 
                ParcelamentoId = parcelamentoId,
                DataVencimento = DateTime.Today // Inicializar com a data de hoje
            });
        }

        // POST: Pagamentos/Create - Não usado mais (pagamentos são criados automaticamente)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParcelamentoId,NumeroParcela,ValorParcela,DataVencimento")] Pagamento pagamento)
        {
            // Definir status padrão como "Pendente" antes da validação
            pagamento.Status = "Pendente";
            
            // Remover erro de validação do Status se houver
            ModelState.Remove("Status");
            
            if (ModelState.IsValid)
            {
                var parcelamento = await _context.Parcelamentos
                    .Include(p => p.Entrada)
                    .FirstOrDefaultAsync(p => p.Id == pagamento.ParcelamentoId);
                
                if (parcelamento == null)
                {
                    return NotFound();
                }

                // Adicionar apenas o pagamento individual informado
                _context.Add(pagamento);
                await _context.SaveChangesAsync();
                
                if (parcelamento.Entrada != null)
                {
                    return RedirectToAction("Details", "Entradas", new { id = parcelamento.Entrada.Id });
                }
            }

            var parcelamentoView = await _context.Parcelamentos
                .Include(p => p.Entrada!)
                    .ThenInclude(e => e!.Empreendimento)
                .Include(p => p.Entrada!)
                    .ThenInclude(e => e!.Imovel)
                .FirstOrDefaultAsync(p => p.Id == pagamento.ParcelamentoId);
            
            ViewBag.Parcelamento = parcelamentoView;
            return View(pagamento);
        }

        // GET: Pagamentos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagamento = await _context.Pagamentos
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Empreendimento)
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Imovel)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (pagamento == null || pagamento.Parcelamento == null || pagamento.Parcelamento.Entrada == null)
            {
                return NotFound();
            }

            ViewBag.Entrada = pagamento.Parcelamento.Entrada;
            return View(pagamento);
        }

        // POST: Pagamentos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ParcelamentoId,NumeroParcela,ValorParcela,DataVencimento,Status,DataPagamento")] Pagamento pagamento)
        {
            if (id != pagamento.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pagamento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PagamentoExists(pagamento.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
                var parcelamento = await _context.Parcelamentos
                    .Include(p => p.Entrada)
                    .FirstOrDefaultAsync(p => p.Id == pagamento.ParcelamentoId);
                
                if (parcelamento?.Entrada != null)
                {
                    return RedirectToAction("Details", "Entradas", new { id = parcelamento.Entrada.Id });
                }
            }

            var pagamentoView = await _context.Pagamentos
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Empreendimento)
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Imovel)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (pagamentoView?.Parcelamento?.Entrada != null)
            {
                ViewBag.Entrada = pagamentoView.Parcelamento.Entrada;
            }
            
            return View(pagamento);
        }

        // GET: Pagamentos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagamento = await _context.Pagamentos
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Empreendimento)
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Imovel)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (pagamento == null)
            {
                return NotFound();
            }

            return View(pagamento);
        }

        // POST: Pagamentos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pagamento = await _context.Pagamentos
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (pagamento != null)
            {
                // Em vez de excluir o registro, apenas remover os dados de pagamento
                pagamento.Status = "Pendente";
                pagamento.DataPagamento = null;
                
                _context.Update(pagamento);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Pagamento cancelado com sucesso! O registro foi mantido como pendente.";
                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }

        // POST: Pagamentos/ExcluirPagamentosRealizados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirPagamentosRealizados()
        {
            try
            {
                // Em vez de excluir os registros, apenas remover os dados de pagamento
                var pagamentosPagos = await _context.Pagamentos
                    .Where(p => p.Status == "Pago")
                    .ToListAsync();
                
                var quantidade = pagamentosPagos.Count;
                
                if (quantidade > 0)
                {
                    foreach (var pagamento in pagamentosPagos)
                    {
                        pagamento.Status = "Pendente";
                        pagamento.DataPagamento = null;
                    }
                    
                    _context.UpdateRange(pagamentosPagos);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Foram cancelados {quantidade} pagamento(s) realizado(s). Os registros foram mantidos como pendentes.";
                }
                else
                {
                    TempData["Info"] = "Não há pagamentos realizados para cancelar.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao cancelar pagamentos realizados: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Pagamentos/GetPagamento/5
        [HttpGet]
        public async Task<IActionResult> GetPagamento(int id)
        {
            var pagamento = await _context.Pagamentos
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Empreendimento)
                .Include(p => p.Parcelamento!)
                    .ThenInclude(par => par!.Entrada!)
                        .ThenInclude(e => e!.Imovel)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pagamento == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = pagamento.Id,
                entradaId = pagamento.Parcelamento?.Entrada?.Id ?? 0,
                empreendimento = pagamento.Parcelamento?.Entrada?.Empreendimento?.Nome ?? "",
                imovel = pagamento.Parcelamento?.Entrada?.Imovel?.Numero ?? "",
                tipo = pagamento.Parcelamento?.TipoPagamento ?? "",
                numeroParcela = pagamento.NumeroParcela,
                valorParcela = pagamento.ValorParcela,
                status = pagamento.Status,
                dataVencimento = pagamento.DataVencimento.ToString("dd/MM/yyyy"),
                dataPagamento = pagamento.DataPagamento?.ToString("yyyy-MM-dd")
            });
        }

        // POST: Pagamentos/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusModel model)
        {
            if (model == null)
            {
                return BadRequest("Dados inválidos");
            }

            var pagamento = await _context.Pagamentos.FindAsync(model.Id);
            if (pagamento == null)
            {
                return NotFound();
            }

            pagamento.Status = model.Status;
            
            // Converter data de yyyy-MM-dd para DateTime
            if (!string.IsNullOrEmpty(model.DataPagamento))
            {
                if (DateTime.TryParseExact(model.DataPagamento, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dataConvertida))
                {
                    pagamento.DataPagamento = dataConvertida;
                }
            }
            else
            {
                pagamento.DataPagamento = null;
            }

            try
            {
                _context.Update(pagamento);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Status atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao atualizar: " + ex.Message });
            }
        }

        private bool PagamentoExists(int id)
        {
            return _context.Pagamentos.Any(e => e.Id == id);
        }
    }

    public class UpdateStatusModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DataPagamento { get; set; }
    }

    public class MarcarComoPagoModel
    {
        public int Id { get; set; }
        public decimal ValorPago { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}

