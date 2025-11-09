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
            var entradas = await _context.Entradas
                .Include(e => e.Empreendimento)
                .Include(e => e.Imovel)
                .Include(e => e.Parcelamentos!)
                    .ThenInclude(p => p.Pagamentos)
                .ToListAsync();

            return View(entradas);
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
                .Include(e => e.Imovel)
                .Include(e => e.Parcelamentos!)
                    .ThenInclude(p => p.Pagamentos)
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
                .Where(i => i.EmpreendimentoId == empreendimentoId && i.Status == "Vendido")
                .Select(i => new { i.Id, i.Numero, i.Tipo, i.Status })
                .ToListAsync();

            return Json(imoveis);
        }

        // POST: Entradas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpreendimentoId,ImovelId,Descricao")] Entrada entrada, string TipoPagamento, string NumeroParcelas, string ValorTotal)
        {
            // Capturar NumeroParcelas diretamente do formulário
            if (string.IsNullOrEmpty(NumeroParcelas))
            {
                NumeroParcelas = Request.Form["NumeroParcelas"].ToString();
            }
            
            int? numeroParcelasParaCriar = null;
            if (!string.IsNullOrEmpty(NumeroParcelas) && int.TryParse(NumeroParcelas, out int numParcelas) && numParcelas > 0)
            {
                numeroParcelasParaCriar = numParcelas;
            }

            // Capturar ValorTotal diretamente do formulário
            if (string.IsNullOrEmpty(ValorTotal))
            {
                ValorTotal = Request.Form["ValorTotal"].ToString();
            }
            
            decimal? valorTotalParaCriar = null;
            if (!string.IsNullOrEmpty(ValorTotal))
            {
                var valorTotalStr = ValorTotal.Replace(",", ".").Replace(" ", "");
                if (decimal.TryParse(valorTotalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal valorTotalDecimal))
                {
                    valorTotalParaCriar = valorTotalDecimal;
                }
            }

            // Validar se já existe uma entrada para o mesmo Empreendimento e Imóvel
            if (entrada.ImovelId.HasValue)
            {
                var entradaExistente = await _context.Entradas
                    .FirstOrDefaultAsync(e => e.EmpreendimentoId == entrada.EmpreendimentoId && 
                                             e.ImovelId == entrada.ImovelId);
                if (entradaExistente != null)
                {
                    ModelState.AddModelError("ImovelId", "Já existe uma entrada cadastrada para este Empreendimento e Imóvel.");
                }
            }

            // Validar TipoPagamento
            if (string.IsNullOrEmpty(TipoPagamento))
            {
                ModelState.AddModelError("TipoPagamento", "O tipo de pagamento é obrigatório.");
            }

            // Validar ValorTotal
            if (!valorTotalParaCriar.HasValue || valorTotalParaCriar.Value <= 0)
            {
                ModelState.AddModelError("ValorTotal", "O valor total deve ser maior que zero.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Criar a entrada
                    _context.Add(entrada);
                    await _context.SaveChangesAsync();

                    // 2. Criar o parcelamento (resumo)
                    var parcelamento = new Parcelamento
                    {
                        EntradaId = entrada.Id,
                        TipoPagamento = TipoPagamento,
                        NumeroParcelas = numeroParcelasParaCriar ?? 1,
                        ValorTotal = valorTotalParaCriar.Value
                    };
                    
                    _context.Add(parcelamento);
                    await _context.SaveChangesAsync();

                    // 3. Criar os pagamentos individuais (parcelas)
                    int numeroParcelas = parcelamento.NumeroParcelas;
                    decimal valorTotal = parcelamento.ValorTotal;
                    
                    // Calcular valor base da parcela
                    decimal valorParcelaBase = Math.Round(valorTotal / numeroParcelas, 2, MidpointRounding.ToEven);
                    decimal valorTotalParcelasBase = valorParcelaBase * (numeroParcelas - 1);
                    decimal valorUltimaParcela = Math.Round(valorTotal - valorTotalParcelasBase, 2, MidpointRounding.ToEven);
                    
                    DateTime dataBase = DateTime.Today;
                    
                    var pagamentosParaAdicionar = new List<Pagamento>();
                    
                    for (int i = 0; i < numeroParcelas; i++)
                    {
                        DateTime dataVencimento;
                        
                        // Calcular data de vencimento baseado no tipo
                        switch (TipoPagamento)
                        {
                            case "Mensal":
                            case "Entrada":
                                dataVencimento = dataBase.AddMonths(i + 1);
                                break;
                            case "Anual":
                                dataVencimento = dataBase.AddMonths((i + 1) * 12);
                                break;
                            case "Financiamento":
                                dataVencimento = (i == 0) ? dataBase : dataBase.AddMonths(i);
                                break;
                            default:
                                dataVencimento = dataBase.AddMonths(i + 1);
                                break;
                        }
                        
                        decimal valorParcela = (i == numeroParcelas - 1) ? valorUltimaParcela : valorParcelaBase;
                        valorParcela = Math.Round(valorParcela, 2, MidpointRounding.ToEven);
                        
                        var novoPagamento = new Pagamento
                        {
                            ParcelamentoId = parcelamento.Id,
                            NumeroParcela = i + 1,
                            ValorParcela = valorParcela,
                            DataVencimento = dataVencimento,
                            Status = "Pendente"
                        };
                        
                        pagamentosParaAdicionar.Add(novoPagamento);
                    }
                    
                    // Adicionar todas as parcelas de uma vez
                    _context.Pagamentos.AddRange(pagamentosParaAdicionar);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Details), new { id = entrada.Id });
                }
                catch
                {
                    throw;
                }
            }
            
            // Recarregar dados para a view em caso de erro
            var empreendimentos = _context.Empreendimentos.ToList();
            ViewData["EmpreendimentoId"] = new SelectList(empreendimentos, "Id", "Nome", entrada.EmpreendimentoId);
            
            // Se houver EmpreendimentoId, carregar os imóveis vendidos do empreendimento
            if (entrada.EmpreendimentoId > 0)
            {
                var imoveis = await _context.Imoveis
                    .Where(i => i.EmpreendimentoId == entrada.EmpreendimentoId && i.Status == "Vendido")
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

            var entrada = await _context.Entradas
                .Include(e => e.Parcelamentos!)
                    .ThenInclude(p => p.Pagamentos)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (entrada == null)
            {
                return NotFound();
            }
            
            ViewData["EmpreendimentoId"] = new SelectList(_context.Empreendimentos, "Id", "Nome", entrada.EmpreendimentoId);
            
            // Carregar imóveis vendidos do empreendimento
            if (entrada.EmpreendimentoId > 0)
            {
                var imoveis = await _context.Imoveis
                    .Where(i => i.EmpreendimentoId == entrada.EmpreendimentoId && i.Status == "Vendido")
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpreendimentoId,ImovelId,Descricao")] Entrada entrada)
        {
            if (id != entrada.Id)
            {
                return NotFound();
            }

            // Validar se já existe uma entrada para o mesmo Empreendimento e Imóvel (excluindo a entrada atual)
            if (entrada.ImovelId.HasValue)
            {
                var entradaExistente = await _context.Entradas
                    .FirstOrDefaultAsync(e => e.Id != entrada.Id &&
                                             e.EmpreendimentoId == entrada.EmpreendimentoId && 
                                             e.ImovelId == entrada.ImovelId);
                if (entradaExistente != null)
                {
                    ModelState.AddModelError("ImovelId", "Já existe uma entrada cadastrada para este Empreendimento e Imóvel.");
                }
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
                return RedirectToAction(nameof(Details), new { id = entrada.Id });
            }
            
            // Recarregar dados para a view em caso de erro
            var empreendimentos = _context.Empreendimentos.ToList();
            ViewData["EmpreendimentoId"] = new SelectList(empreendimentos, "Id", "Nome", entrada.EmpreendimentoId);
            
            // Se houver EmpreendimentoId, carregar os imóveis vendidos do empreendimento
            if (entrada.EmpreendimentoId > 0)
            {
                var imoveis = await _context.Imoveis
                    .Where(i => i.EmpreendimentoId == entrada.EmpreendimentoId && i.Status == "Vendido")
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
                .Include(e => e.Imovel)
                .Include(e => e.Parcelamentos!)
                    .ThenInclude(p => p.Pagamentos)
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

        // GET: Entradas/GetForEdit/5
        [HttpGet]
        public async Task<IActionResult> GetForEdit(int id)
        {
            var entrada = await _context.Entradas
                .Include(e => e.Empreendimento)
                .Include(e => e.Imovel)
                .Include(e => e.Parcelamentos!)
                    .ThenInclude(p => p.Pagamentos)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            if (entrada == null)
            {
                return NotFound();
            }

            var empreendimentos = await _context.Empreendimentos.ToListAsync();
            var imoveis = new List<Imovel>();
            if (entrada.EmpreendimentoId > 0)
            {
                imoveis = await _context.Imoveis
                    .Where(i => i.EmpreendimentoId == entrada.EmpreendimentoId && i.Status == "Vendido")
                    .ToListAsync();
            }

            // Coletar todos os pagamentos de todos os parcelamentos
            var pagamentos = new List<object>();
            if (entrada.Parcelamentos != null)
            {
                foreach (var parcelamento in entrada.Parcelamentos)
                {
                    if (parcelamento.Pagamentos != null)
                    {
                        foreach (var pagamento in parcelamento.Pagamentos)
                        {
                            pagamentos.Add(new
                            {
                                id = pagamento.Id,
                                parcelamentoId = pagamento.ParcelamentoId,
                                numeroParcela = pagamento.NumeroParcela,
                                valorParcela = pagamento.ValorParcela,
                                status = pagamento.Status,
                                dataVencimento = pagamento.DataVencimento.ToString("dd/MM/yyyy")
                            });
                        }
                    }
                }
            }

            return Json(new
            {
                entrada = new
                {
                    id = entrada.Id,
                    empreendimentoId = entrada.EmpreendimentoId,
                    imovelId = entrada.ImovelId,
                    descricao = entrada.Descricao
                },
                empreendimentos = empreendimentos.Select(e => new { id = e.Id, nome = e.Nome }),
                imoveis = imoveis.Select(i => new { id = i.Id, numero = i.Numero ?? "" }),
                pagamentos = pagamentos
            });
        }

        // POST: Entradas/UpdateWithPagamentos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWithPagamentos([FromBody] UpdateEntradaModel model)
        {
            if (model == null || model.Entrada == null)
            {
                return BadRequest("Dados inválidos");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Atualizar a entrada
                var entrada = await _context.Entradas
                    .Include(e => e.Parcelamentos!)
                        .ThenInclude(p => p.Pagamentos)
                    .FirstOrDefaultAsync(e => e.Id == model.Entrada.Id);

                if (entrada == null)
                {
                    return NotFound();
                }

                // Validar se já existe uma entrada para o mesmo Empreendimento e Imóvel (excluindo a entrada atual)
                if (model.Entrada.ImovelId.HasValue)
                {
                    var entradaExistente = await _context.Entradas
                        .FirstOrDefaultAsync(e => e.Id != model.Entrada.Id &&
                                                 e.EmpreendimentoId == model.Entrada.EmpreendimentoId && 
                                                 e.ImovelId == model.Entrada.ImovelId);
                    if (entradaExistente != null)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "Já existe uma entrada cadastrada para este Empreendimento e Imóvel." });
                    }
                }

                entrada.EmpreendimentoId = model.Entrada.EmpreendimentoId;
                entrada.ImovelId = model.Entrada.ImovelId;
                entrada.Descricao = model.Entrada.Descricao;

                // Atualizar ou criar pagamentos
                if (model.Pagamentos != null && model.Pagamentos.Any())
                {
                    // Coletar todos os pagamentos de todos os parcelamentos
                    var todosPagamentos = new List<Pagamento>();
                    if (entrada.Parcelamentos != null)
                    {
                        foreach (var parcelamento in entrada.Parcelamentos)
                        {
                            if (parcelamento.Pagamentos != null)
                            {
                                todosPagamentos.AddRange(parcelamento.Pagamentos);
                            }
                        }
                    }

                    var pagamentosIds = model.Pagamentos.Where(p => p.Id > 0).Select(p => p.Id).ToList();
                    
                    // Remover pagamentos que não estão mais na lista
                    var pagamentosParaRemover = todosPagamentos.Where(p => !pagamentosIds.Contains(p.Id)).ToList();
                    if (pagamentosParaRemover.Any())
                    {
                        _context.Pagamentos.RemoveRange(pagamentosParaRemover);
                    }

                    // Atualizar ou criar pagamentos
                    foreach (var pagamentoModel in model.Pagamentos)
                    {
                        if (pagamentoModel.Id > 0)
                        {
                            // Atualizar pagamento existente
                            var pagamento = await _context.Pagamentos.FindAsync(pagamentoModel.Id);
                            if (pagamento != null)
                            {
                                pagamento.Status = pagamentoModel.Status;
                                pagamento.ValorParcela = pagamentoModel.ValorParcela;
                                
                                // Converter data de dd/MM/yyyy para DateTime
                                if (DateTime.TryParseExact(pagamentoModel.DataVencimento, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataConvertida))
                                {
                                    pagamento.DataVencimento = dataConvertida;
                                }
                                
                                _context.Update(pagamento);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Entrada e pagamentos atualizados com sucesso!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Erro ao atualizar: " + ex.Message });
            }
        }

        // GET: Entradas/AdicionarParcelamento/5
        public async Task<IActionResult> AdicionarParcelamento(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entrada = await _context.Entradas
                .Include(e => e.Empreendimento)
                .Include(e => e.Imovel)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrada == null)
            {
                return NotFound();
            }

            ViewBag.Entrada = entrada;
            return View();
        }

        // POST: Entradas/AdicionarParcelamento/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarParcelamento(int id, string TipoPagamento, string NumeroParcelas, string ValorTotal)
        {
            var entrada = await _context.Entradas.FindAsync(id);
            if (entrada == null)
            {
                return NotFound();
            }

            // Capturar NumeroParcelas
            int? numeroParcelasParaCriar = null;
            if (!string.IsNullOrEmpty(NumeroParcelas) && int.TryParse(NumeroParcelas, out int numParcelas) && numParcelas > 0)
            {
                numeroParcelasParaCriar = numParcelas;
            }

            // Capturar ValorTotal
            decimal? valorTotalParaCriar = null;
            if (!string.IsNullOrEmpty(ValorTotal))
            {
                var valorTotalStr = ValorTotal.Replace(",", ".").Replace(" ", "");
                if (decimal.TryParse(valorTotalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal valorTotalDecimal))
                {
                    valorTotalParaCriar = valorTotalDecimal;
                }
            }

            // Validar TipoPagamento
            if (string.IsNullOrEmpty(TipoPagamento))
            {
                ModelState.AddModelError("TipoPagamento", "O tipo de pagamento é obrigatório.");
            }

            // Validar ValorTotal
            if (!valorTotalParaCriar.HasValue || valorTotalParaCriar.Value <= 0)
            {
                ModelState.AddModelError("ValorTotal", "O valor total deve ser maior que zero.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Criar o parcelamento (resumo)
                    var parcelamento = new Parcelamento
                    {
                        EntradaId = entrada.Id,
                        TipoPagamento = TipoPagamento,
                        NumeroParcelas = numeroParcelasParaCriar ?? 1,
                        ValorTotal = valorTotalParaCriar.Value
                    };
                    
                    _context.Add(parcelamento);
                    await _context.SaveChangesAsync();

                    // 2. Criar os pagamentos individuais (parcelas)
                    int numeroParcelas = parcelamento.NumeroParcelas;
                    decimal valorTotal = parcelamento.ValorTotal;
                    
                    // Calcular valor base da parcela
                    decimal valorParcelaBase = Math.Round(valorTotal / numeroParcelas, 2, MidpointRounding.ToEven);
                    decimal valorTotalParcelasBase = valorParcelaBase * (numeroParcelas - 1);
                    decimal valorUltimaParcela = Math.Round(valorTotal - valorTotalParcelasBase, 2, MidpointRounding.ToEven);
                    
                    DateTime dataBase = DateTime.Today;
                    
                    var pagamentosParaAdicionar = new List<Pagamento>();
                    
                    for (int i = 0; i < numeroParcelas; i++)
                    {
                        DateTime dataVencimento;
                        
                        // Calcular data de vencimento baseado no tipo
                        switch (TipoPagamento)
                        {
                            case "Mensal":
                            case "Entrada":
                                dataVencimento = dataBase.AddMonths(i + 1);
                                break;
                            case "Anual":
                                dataVencimento = dataBase.AddMonths((i + 1) * 12);
                                break;
                            case "Financiamento":
                                dataVencimento = (i == 0) ? dataBase : dataBase.AddMonths(i);
                                break;
                            default:
                                dataVencimento = dataBase.AddMonths(i + 1);
                                break;
                        }
                        
                        decimal valorParcela = (i == numeroParcelas - 1) ? valorUltimaParcela : valorParcelaBase;
                        valorParcela = Math.Round(valorParcela, 2, MidpointRounding.ToEven);
                        
                        var novoPagamento = new Pagamento
                        {
                            ParcelamentoId = parcelamento.Id,
                            NumeroParcela = i + 1,
                            ValorParcela = valorParcela,
                            DataVencimento = dataVencimento,
                            Status = "Pendente"
                        };
                        
                        pagamentosParaAdicionar.Add(novoPagamento);
                    }
                    
                    // Adicionar todas as parcelas de uma vez
                    _context.Pagamentos.AddRange(pagamentosParaAdicionar);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Parcelamento adicionado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = entrada.Id });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Erro ao adicionar parcelamento: {ex.Message}";
                }
            }

            // Recarregar dados para a view em caso de erro
            var entradaView = await _context.Entradas
                .Include(e => e.Empreendimento)
                .Include(e => e.Imovel)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            ViewBag.Entrada = entradaView;
            return View();
        }

        // POST: Entradas/ExcluirParcelamento/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirParcelamento(int id, int entradaId)
        {
            try
            {
                var parcelamento = await _context.Parcelamentos
                    .Include(p => p.Pagamentos)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (parcelamento == null)
                {
                    TempData["Error"] = "Parcelamento não encontrado.";
                    return RedirectToAction(nameof(Details), new { id = entradaId });
                }

                // Remover todos os pagamentos associados ao parcelamento
                if (parcelamento.Pagamentos != null && parcelamento.Pagamentos.Any())
                {
                    _context.Pagamentos.RemoveRange(parcelamento.Pagamentos);
                }

                // Remover o parcelamento
                _context.Parcelamentos.Remove(parcelamento);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Parcelamento e seus pagamentos foram excluídos com sucesso!";
                return RedirectToAction(nameof(Details), new { id = entradaId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao excluir parcelamento: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = entradaId });
            }
        }

        // POST: Entradas/LimparPagamentosAntigos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LimparPagamentosAntigos()
        {
            try
            {
                // Remover todos os pagamentos antigos (criados antes da nova lógica)
                var pagamentosAntigos = await _context.Pagamentos.ToListAsync();
                _context.Pagamentos.RemoveRange(pagamentosAntigos);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Foram removidos {pagamentosAntigos.Count} pagamentos antigos da base de dados.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao limpar pagamentos antigos: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool EntradaExists(int id)
        {
            return _context.Entradas.Any(e => e.Id == id);
        }
    }

    public class UpdateEntradaModel
    {
        public EntradaModel? Entrada { get; set; }
        public List<PagamentoModel>? Pagamentos { get; set; }
    }

    public class EntradaModel
    {
        public int Id { get; set; }
        public int EmpreendimentoId { get; set; }
        public int? ImovelId { get; set; }
        public string? Descricao { get; set; }
    }

    public class PagamentoModel
    {
        public int Id { get; set; }
        public int ParcelamentoId { get; set; }
        public int NumeroParcela { get; set; }
        public decimal ValorParcela { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DataVencimento { get; set; } = string.Empty;
    }
}

