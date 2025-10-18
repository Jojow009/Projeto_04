namespace KnewinApi.Models
{
    public class Telefone
    {
        public int Id { get; set; } // Chave Primária
        public string Numero { get; set; }

        // Relação com Fornecedor (Chave Estrangeira)
        public int FornecedorId { get; set; }

        // **** CORREÇÃO AQUI ****
        // Adicionado '?' para indicar que a propriedade de navegação é opcional
        // Isso corrige o erro "The fornecedor field is required."
        public Fornecedor? Fornecedor { get; set; }
    }
}


