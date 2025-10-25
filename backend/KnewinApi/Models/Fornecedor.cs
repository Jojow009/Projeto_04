using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; 

namespace KnewinApi.Models
{
    public class Fornecedor
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string CpfCnpj { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime DataCadastro { get; set; }

        public string? RG { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? DataNascimento { get; set; }

        // Propriedade da Chave Estrangeira
        public int EmpresaId { get; set; } 
        
        // Propriedade de Navegação corrigida para evitar ciclo
        [JsonIgnore] 
        public Empresa? Empresa { get; set; }

        // Esta lista será serializada (aparecerá no JSON)
        public ICollection<Telefone> Telefones { get; set; } = new List<Telefone>();
    }
}