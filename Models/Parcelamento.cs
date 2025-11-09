using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConstrutoraApp.Models
{
    public class Parcelamento
    {
        public int Id { get; set; }

        [Required]
        public int EntradaId { get; set; }

        public Entrada? Entrada { get; set; }

        [Required]
        public string TipoPagamento { get; set; } = string.Empty; // Entrada, Mensal, Anual, Investimento, Financiamento, Outros

        [Required]
        public int NumeroParcelas { get; set; }

        [Required]
        public decimal ValorTotal { get; set; }

        // Relação com Pagamentos (cada parcela individual)
        public ICollection<Pagamento>? Pagamentos { get; set; }
    }
}

