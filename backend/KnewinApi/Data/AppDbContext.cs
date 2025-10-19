using Microsoft.EntityFrameworkCore;
using KnewinApi.Models; 
using System;
using System.Linq; // <-- Adicione este using para o .Where()

namespace KnewinApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Telefone> Telefones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- 1. CONFIGURAÇÃO DOS TIPOS DATETIME ---
            // O loop foreach só deve fazer isso
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var dateTimeProps = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

                foreach (var prop in dateTimeProps)
                {
                    prop.SetColumnType("timestamp with time zone");
                }
            }

            // --- 2. CONFIGURAÇÃO DAS RELAÇÕES (Fora do loop) ---
            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Fornecedores)    // Uma Empresa tem muitos Fornecedores
                .WithOne(f => f.Empresa)        // Um Fornecedor tem uma Empresa
                .HasForeignKey(f => f.EmpresaId); // A chave estrangeira está em Fornecedor

            modelBuilder.Entity<Fornecedor>()
                .HasMany(f => f.Telefones)      // Um Fornecedor tem muitos Telefones
                .WithOne(t => t.Fornecedor)     // Um Telefone pertence a um Fornecedor
                .HasForeignKey(t => t.FornecedorId); // A chave estrangeira está em Telefone


            // --- 3. DATA SEEDING (Fora do loop) ---
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

            // --- 4. CHAMADA BASE (No final de tudo) ---
            base.OnModelCreating(modelBuilder);
        }
    }
}