using System.Globalization;

namespace ConstrutoraApp.Helpers
{
    public static class DecimalExtensions
    {
        private static readonly CultureInfo CulturaBrasileira = new CultureInfo("pt-BR");

        /// <summary>
        /// Formata um valor decimal como moeda brasileira (R$ X.XXX,XX)
        /// </summary>
        public static string ToMoedaBrasileira(this decimal valor)
        {
            return valor.ToString("N2", CulturaBrasileira);
        }

        /// <summary>
        /// Formata um valor decimal nullable como moeda brasileira (R$ X.XXX,XX)
        /// </summary>
        public static string ToMoedaBrasileira(this decimal? valor)
        {
            return valor?.ToString("N2", CulturaBrasileira) ?? "0,00";
        }
    }
}

