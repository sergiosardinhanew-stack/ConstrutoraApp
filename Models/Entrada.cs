using System;
using System.ComponentModel.DataAnnotations;

namespace ConstrutoraApp.Models
{
    public class Entrada
    {
        public int Id { get; set; }

        [Required]
        public int EmpreendimentoId { get; set; }

        public Empreendimento? Empreendimento { get; set; }

        public int? ImovelId { get; set; }

        public Imovel? Imovel { get; set; }

        [Required]
        public string Tipo { get; set; } // Entrada, Parcela, Anual, Investimento, Outros

        public string? Descricao { get; set; }

        public decimal Valor { get; set; }

        [DataType(DataType.Date)]
        public DateTime Data { get; set; }
    }
}

