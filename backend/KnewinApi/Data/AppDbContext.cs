using Microsoft.EntityFrameworkCore;
using KnewinApi.Models; // Garante que ele pode ver seus Modelos (Empresa, Fornecedor, etc)
using System; // Necessário para o DateTime

namespace KnewinApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Mapeia suas classes para tabelas no banco de dados
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Telefone> Telefones { get; set; }

        // Este método é chamado pelo Entity Framework quando ele está criando o modelo
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- 1. CONFIGURAÇÃO DAS RELAÇÕES ---

            // Configura a relação 1-para-N (Empresa -> Fornecedores)
            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Fornecedores)    // Uma Empresa tem muitos Fornecedores
                .WithOne(f => f.Empresa)        // Um Fornecedor tem uma Empresa
                .HasForeignKey(f => f.EmpresaId); // A chave estrangeira está em Fornecedor

            // Configura a relação 1-para-N (Fornecedor -> Telefones)
            modelBuilder.Entity<Fornecedor>()
                .HasMany(f => f.Telefones)      // Um Fornecedor tem muitos Telefones
                .WithOne(t => t.Fornecedor)     // Um Telefone pertence a um Fornecedor
                .HasForeignKey(t => t.FornecedorId); // A chave estrangeira está em Telefone

            
            // --- 2. DATA SEEDING (EMPRESAS PRÉ-CADASTRADAS) ---
            // Adiciona empresas no banco de dados assim que a migração é aplicada
            
            modelBuilder.Entity<Empresa>().HasData(
                new Empresa
                {
                    Id = 1, // O ID precisa ser definido manualmente no Seed
                    UF = "PR", // Paraná, para testar a regra de negócio
                    NomeFantasia = "Empresa Teste do Paraná",
                    CNPJ = "00000000000100"
                },
                new Empresa
                {
                    Id = 2,
                    UF = "SC", // Outro estado (Santa Catarina)
                    NomeFantasia = "Empresa Teste de SC",
                    CNPJ = "00000000000200"
                }
            );

            // Chama o método base no final
            base.OnModelCreating(modelBuilder);
        }
    }
}