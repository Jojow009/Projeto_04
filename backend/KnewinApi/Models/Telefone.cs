using System.Text.Json.Serialization; 

namespace KnewinApi.Models
{
    public class Telefone
    {
        public int Id { get; set; } 
        public string Numero { get; set; }

        // Relação com Fornecedor (Chave Estrangeira)
        public int FornecedorId { get; set; }

        // Propriedade de Navegação corrigida para evitar ciclo
        [JsonIgnore] 
        public Fornecedor? Fornecedor { get; set; }
    }
}