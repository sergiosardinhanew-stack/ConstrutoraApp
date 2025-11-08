using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConstrutoraApp.Models
{
    public class Empreendimento
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        public string Tipo { get; set; } // Casa, Edifício, Comercial

        public string Endereco { get; set; }

        public string Descricao { get; set; }

        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataEntrega { get; set; }

        public string Status { get; set; } // Em construção, Concluído, Vendido

        public decimal? ValorPrevisto { get; set; }

        public decimal? ValorRealizado { get; set; }

        // Relações
        public ICollection<Imovel>? Imoveis { get; set; }

        public ICollection<Custo>? Custos { get; set; }

        public ICollection<Entrada>? Entradas { get; set; }
    }
}

