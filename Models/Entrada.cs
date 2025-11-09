using System;
using System.Collections.Generic;
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

        public string? Descricao { get; set; }

        // Relação com Parcelamentos (resumo do parcelamento)
        public ICollection<Parcelamento>? Parcelamentos { get; set; }
    }
}

