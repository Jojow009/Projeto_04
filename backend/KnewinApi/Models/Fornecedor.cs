using System;
using System.Collections.Generic;

namespace KnewinApi.Models
{
    public class Fornecedor
    {
        public int Id { get; set; } // Chave Primária
        public string Nome { get; set; }
        
        // --- CAMPOS EXIGIDOS PELO TESTE ---
        public string CpfCnpj { get; set; }
        public DateTime DataCadastro { get; set; }
        
        // Campos para Pessoa Física (podem ser nulos)
        public string? RG { get; set; } // '?' permite ser nulo
        public DateTime? DataNascimento { get; set; } // '?' permite ser nulo
        // --- FIM DOS CAMPOS ---

        // Relação com Empresa (Chave Estrangeira)
        public int EmpresaId { get; set; }
        
        // **** CORREÇÃO AQUI ****
        // Adicionado '?' para indicar que a propriedade de navegação é opcional
        // Isso corrige o erro "The Empresa field is required."
        public Empresa? Empresa { get; set; } 

        // Propriedade de navegação: Um Fornecedor tem muitos Telefones
        public ICollection<Telefone> Telefones { get; set; } = new List<Telefone>();
    }
}