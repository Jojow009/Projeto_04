using System.Collections.Generic;

namespace KnewinApi.Models
{
    public class Empresa
    {
        public int Id { get; set; } 
        
        // Propriedades exigidas pelo teste
        public string UF { get; set; }
        public string NomeFantasia { get; set; }
        public string CNPJ { get; set; }

        // Propriedade de navegação: Uma Empresa tem muitos Fornecedores
        public ICollection<Fornecedor> Fornecedores { get; set; } = new List<Fornecedor>();
    }
}