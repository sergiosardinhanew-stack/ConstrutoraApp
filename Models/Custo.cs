using System;
using System.ComponentModel.DataAnnotations;

namespace ConstrutoraApp.Models
{
    public class Custo
    {
        public int Id { get; set; }

        [Required]
        public int EmpreendimentoId { get; set; }

        public Empreendimento? Empreendimento { get; set; }

        [Required]
        public string Tipo { get; set; } // Material, Mão de Obra, Imposto, etc.

        [Required]
        public string Descricao { get; set; }

        public decimal Valor { get; set; }

        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        public string? Fornecedor { get; set; }

        public string? FormaPagamento { get; set; } // PIX, Débito, Crédito, Boleto

        public int? NumeroParcelas { get; set; } // Para Crédito, até 12

        public decimal? ValorParcela { get; set; } // Para Crédito

        [DataType(DataType.Date)]
        public DateTime? DataVencimentoParcela { get; set; } // Para Crédito

        [DataType(DataType.Date)]
        public DateTime? DataVencimentoBoleto { get; set; } // Para Boleto

        public string? Status { get; set; } // Pago, Pendente
    }
}

