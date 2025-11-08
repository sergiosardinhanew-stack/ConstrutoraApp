using System;
using System.ComponentModel.DataAnnotations;

namespace ConstrutoraApp.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Tipo { get; set; } // Entrada / Saída

        [Required]
        [StringLength(100)]
        public string Descricao { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Informe um valor válido.")]
        public decimal Valor { get; set; }

        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string Empreendimento { get; set; }
    }
}
