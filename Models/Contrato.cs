using System;
using System.ComponentModel.DataAnnotations;

namespace ConstrutoraApp.Models
{
    public class Contrato
    {
        public int Id { get; set; }

        [Required]
        public int ImovelId { get; set; }

        public Imovel? Imovel { get; set; }

        [Required]
        public string Comprador { get; set; }

        public string? CPF_CNPJ { get; set; }

        [DataType(DataType.Date)]
        public DateTime DataContrato { get; set; }

        public decimal ValorVenda { get; set; }

        public string? FormaPagamento { get; set; }

        public string? Observacoes { get; set; }
    }
}

