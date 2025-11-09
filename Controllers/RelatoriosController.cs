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
        public async Task<IActionResult> Index(string? dataInicial, string? dataFinal, string? tipo, string? status)
        {
            DateTime? dataInicialDt = null;
            DateTime? dataFinalDt = null;

            // Converter string para DateTime (aceita yyyy-MM-dd ou dd/MM/yyyy)
            if (!string.IsNullOrEmpty(dataInicial))
            {
                // Tentar formato brasileiro primeiro (dd/MM/yyyy)
                if (DateTime.TryParseExact(dataInicial, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataBr))
                {
                    dataInicialDt = dataBr;
                }
                // Tentar formato ISO (yyyy-MM-dd)
                else if (DateTime.TryParseExact(dataInicial, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dataIso))
                {
                    dataInicialDt = dataIso;
                }
                // Tentar parse genérico
                else if (DateTime.TryParse(dataInicial, out DateTime dataGen))
                {
                    dataInicialDt = dataGen;
                }
            }

            if (!string.IsNullOrEmpty(dataFinal))
            {
                // Tentar formato brasileiro primeiro (dd/MM/yyyy)
                if (DateTime.TryParseExact(dataFinal, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dataBr))
                {
                    dataFinalDt = dataBr;
                }
                // Tentar formato ISO (yyyy-MM-dd)
                else if (DateTime.TryParseExact(dataFinal, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime dataIso))
                {
                    dataFinalDt = dataIso;
                }
                // Tentar parse genérico
                else if (DateTime.TryParse(dataFinal, out DateTime dataGen))
                {
                    dataFinalDt = dataGen;
                }
            }

            // Se não houver filtros, usar data do dia atual
            if (!dataInicialDt.HasValue)
            {
                dataInicialDt = DateTime.Today;
            }
            if (!dataFinalDt.HasValue)
            {
                dataFinalDt = DateTime.Today;
            }

            var dataInicialFiltro = dataInicialDt.Value;
            var dataFinalFiltro = dataFinalDt.Value;

            var itensRelatorio = new List<RelatorioItem>();

            // Se tipo for "Saidas" ou vazio (Todos), incluir saídas (Custos) - realizadas e previstas
            if (string.IsNullOrEmpty(tipo) || tipo == "Saidas")
            {
                var custos = await _context.Custos
                    .Include(c => c.Empreendimento)
                    .Where(c => c.Data >= dataInicialFiltro.Date && c.Data <= dataFinalFiltro.Date.AddDays(1).AddTicks(-1))
                    .OrderBy(c => c.Data)
                    .ToListAsync();

                foreach (var custo in custos)
                {
                    var hoje = DateTime.Today;
                    var statusCusto = custo.Data <= hoje ? "Realizado" : "Previsto";

                    itensRelatorio.Add(new RelatorioItem
                    {
                        Data = custo.Data,
                        Descricao = custo.Descricao ?? string.Empty,
                        FornecedorOuImovel = custo.Fornecedor ?? string.Empty,
                        ValorTotal = custo.Valor,
                        Tipo = "Saida",
                        Status = statusCusto
                    });
                }
            }

            // Se tipo for "Entradas" ou vazio (Todos), incluir entradas (Pagamentos) - pagos e pendentes
            if (string.IsNullOrEmpty(tipo) || tipo == "Entradas")
            {
                // Pagamentos pagos (realizados)
                var pagamentosPagos = await _context.Pagamentos
                    .Include(p => p.Parcelamento!)
                        .ThenInclude(par => par!.Entrada!)
                            .ThenInclude(e => e!.Empreendimento)
                    .Include(p => p.Parcelamento!)
                        .ThenInclude(par => par!.Entrada!)
                            .ThenInclude(e => e!.Imovel)
                    .Where(p => p.Status == "Pago" && 
                               p.DataPagamento.HasValue &&
                               p.DataPagamento.Value.Date >= dataInicialFiltro.Date && 
                               p.DataPagamento.Value.Date <= dataFinalFiltro.Date)
                    .ToListAsync();

                foreach (var pagamento in pagamentosPagos)
                {
                    var descricao = pagamento.Parcelamento?.Entrada?.Descricao;
                    if (string.IsNullOrEmpty(descricao))
                    {
                        descricao = $"Pagamento - {pagamento.Parcelamento?.TipoPagamento} - Parcela {pagamento.NumeroParcela}";
                    }

                    var imovel = pagamento.Parcelamento?.Entrada?.Imovel?.Numero ?? string.Empty;

                    itensRelatorio.Add(new RelatorioItem
                    {
                        Data = pagamento.DataPagamento!.Value,
                        Descricao = descricao,
                        FornecedorOuImovel = imovel,
                        ValorTotal = pagamento.ValorParcela,
                        Tipo = "Entrada",
                        Status = "Pago"
                    });
                }

                // Pagamentos pendentes (previstos) - baseado na data de vencimento
                var pagamentosPendentes = await _context.Pagamentos
                    .Include(p => p.Parcelamento!)
                        .ThenInclude(par => par!.Entrada!)
                            .ThenInclude(e => e!.Empreendimento)
                    .Include(p => p.Parcelamento!)
                        .ThenInclude(par => par!.Entrada!)
                            .ThenInclude(e => e!.Imovel)
                    .Where(p => p.Status == "Pendente" && 
                               p.DataVencimento.Date >= dataInicialFiltro.Date && 
                               p.DataVencimento.Date <= dataFinalFiltro.Date)
                    .ToListAsync();

                foreach (var pagamento in pagamentosPendentes)
                {
                    var descricao = pagamento.Parcelamento?.Entrada?.Descricao;
                    if (string.IsNullOrEmpty(descricao))
                    {
                        descricao = $"Pagamento - {pagamento.Parcelamento?.TipoPagamento} - Parcela {pagamento.NumeroParcela}";
                    }

                    var imovel = pagamento.Parcelamento?.Entrada?.Imovel?.Numero ?? string.Empty;

                    itensRelatorio.Add(new RelatorioItem
                    {
                        Data = pagamento.DataVencimento,
                        Descricao = descricao,
                        FornecedorOuImovel = imovel,
                        ValorTotal = pagamento.ValorParcela,
                        Tipo = "Entrada",
                        Status = "Pendente"
                    });
                }
            }

            // Filtrar por status se fornecido
            if (!string.IsNullOrEmpty(status) && status != "Todos")
            {
                itensRelatorio = itensRelatorio.Where(i => i.Status == status).ToList();
            }

            // Ordenar por data
            itensRelatorio = itensRelatorio.OrderBy(i => i.Data).ToList();

            // Calcular valor total
            var valorTotal = itensRelatorio.Sum(i => i.ValorTotal);

            ViewBag.DataInicial = dataInicialFiltro.ToString("yyyy-MM-dd");
            ViewBag.DataFinal = dataFinalFiltro.ToString("yyyy-MM-dd");
            ViewBag.Tipo = tipo ?? "Todos";
            ViewBag.Status = status ?? "Todos";
            ViewBag.ValorTotal = valorTotal;
            ViewBag.ItensRelatorio = itensRelatorio;

            return View();
        }
    }
}

