using System;
using System.ComponentModel.DataAnnotations;

namespace ConstrutoraApp.Models
{
    public class Pagamento
    {
        public int Id { get; set; }

        [Required]
        public int ParcelamentoId { get; set; }

        public Parcelamento? Parcelamento { get; set; }

        [Required]
        public int NumeroParcela { get; set; } // Número da parcela (1, 2, 3, ..., 20)

        [Required]
        public decimal ValorParcela { get; set; } // Valor individual da parcela (ex: R$ 1.000,00)

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataVencimento { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty; // Pago, Pendente

        [DataType(DataType.Date)]
        public DateTime? DataPagamento { get; set; } // Data em que o pagamento foi realizado
    }
}

