using Microsoft.EntityFrameworkCore;
using ConstrutoraApp.Models;

namespace ConstrutoraApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Empreendimento> Empreendimentos { get; set; }
        public DbSet<Imovel> Imoveis { get; set; }
        public DbSet<Custo> Custos { get; set; }
        public DbSet<Entrada> Entradas { get; set; }
        public DbSet<Parcelamento> Parcelamentos { get; set; }
        public DbSet<Pagamento> Pagamentos { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Empreendimento>()
                .HasMany(e => e.Imoveis)
                .WithOne(i => i.Empreendimento)
                .HasForeignKey(i => i.EmpreendimentoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Imovel>()
                .HasOne(i => i.Contrato)
                .WithOne(c => c.Imovel)
                .HasForeignKey<Contrato>(c => c.ImovelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Empreendimento>()
                .HasMany(e => e.Custos)
                .WithOne(c => c.Empreendimento)
                .HasForeignKey(c => c.EmpreendimentoId);

            modelBuilder.Entity<Empreendimento>()
                .HasMany(e => e.Entradas)
                .WithOne(c => c.Empreendimento)
                .HasForeignKey(c => c.EmpreendimentoId);

            modelBuilder.Entity<Entrada>()
                .HasOne(e => e.Imovel)
                .WithMany()
                .HasForeignKey(e => e.ImovelId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Entrada>()
                .HasMany(e => e.Parcelamentos)
                .WithOne(p => p.Entrada)
                .HasForeignKey(p => p.EntradaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Parcelamento>()
                .HasMany(p => p.Pagamentos)
                .WithOne(pg => pg.Parcelamento)
                .HasForeignKey(pg => pg.ParcelamentoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
