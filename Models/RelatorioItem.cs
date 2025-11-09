using System;

namespace ConstrutoraApp.Models
{
    public class RelatorioItem
    {
        public DateTime Data { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string FornecedorOuImovel { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Saida" ou "Entrada"
        public string Status { get; set; } = string.Empty; // "Pago", "Pendente", "Realizado"
    }
}

