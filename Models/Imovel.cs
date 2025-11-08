using System.ComponentModel.DataAnnotations;

namespace ConstrutoraApp.Models
{
    public class Imovel
    {
        public int Id { get; set; }

        [Required]
        public int EmpreendimentoId { get; set; }

        public Empreendimento? Empreendimento { get; set; }

        [Required]
        public string Tipo { get; set; } // Casa, Apartamento, Sala Comercial

        public string? Numero { get; set; }

        public decimal Metragem { get; set; }

        public int Quartos { get; set; }

        public int? Andar { get; set; }

        public decimal ValorVenda { get; set; }

        public string? Status { get; set; } // Disponível, Reservado, Vendido

        // Relação
        public Contrato? Contrato { get; set; }
    }
}

