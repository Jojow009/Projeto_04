using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KnewinApi.Data;
using KnewinApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KnewinApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Define a rota como "api/fornecedores"
    public class FornecedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FornecedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/fornecedores?nome=...&cpfCnpj=...&dataCadastro=...
        // Implementa a Regra 4: Filtros
        [HttpGet]
        public async Task<IActionResult> GetFornecedores(
            [FromQuery] string? nome,
            [FromQuery] string? cpfCnpj,
            [FromQuery] DateTime? dataCadastro)
        {
            var query = _context.Fornecedores
                                .Include(f => f.Telefones) // Inclui os telefones na consulta
                                .AsQueryable();

            if (!string.IsNullOrEmpty(nome))
            {
                // ToLower para busca case-insensitive (pode depender da configuração do banco)
                query = query.Where(f => f.Nome.ToLower().Contains(nome.ToLower())); 
            }
            if (!string.IsNullOrEmpty(cpfCnpj))
            {
                // Busca pelo valor exato (sem máscara, como salvamos)
                query = query.Where(f => f.CpfCnpj == cpfCnpj);
            }
            if (dataCadastro.HasValue)
            {
                // dataCadastro é um filtro de data, compara apenas a parte da data (sem hora)
                query = query.Where(f => f.DataCadastro.Date == dataCadastro.Value.Date);
            }

            var fornecedores = await query.ToListAsync();
            return Ok(fornecedores);
        }

        // POST: api/fornecedores
        // Implementa as Regras 2 (Paraná) e 3 (RG/Nascimento) e trata o UTC
        [HttpPost]
        public async Task<IActionResult> PostFornecedor([FromBody] Fornecedor fornecedor)
        {
            if (fornecedor == null)
            {
                return BadRequest("Dados inválidos.");
            }

            // Busca a empresa selecionada
            var empresa = await _context.Empresas.FindAsync(fornecedor.EmpresaId);
            if (empresa == null)
            {
                return BadRequest("Empresa não encontrada.");
            }

            // O front-end já deve mandar o CpfCnpj limpo (sem máscara)
            bool isPessoaFisica = fornecedor.CpfCnpj?.Length == 11;

            // Tratamento de Datas de Entrada: Garantindo UTC antes de salvar
            // Se o DataNascimento veio com data, garante que o Kind seja Utc.
            if (fornecedor.DataNascimento.HasValue)
            {
                // Garante que o Kind seja Utc, assumindo que a data enviada pelo cliente é a data 'sem timezone' que ele digitou
                // Nota: Para datas de nascimento, muitas vezes o Kind é irrelevante, mas o SpecifyKind(..., Utc) garante consistência no banco.
                fornecedor.DataNascimento = DateTime.SpecifyKind(
                    fornecedor.DataNascimento.Value,
                    DateTimeKind.Utc
                );
            }

            // Regra 3: Se for pessoa física, RG e Data de Nascimento são obrigatórios
            if (isPessoaFisica)
            {
                if (string.IsNullOrEmpty(fornecedor.RG) || !fornecedor.DataNascimento.HasValue)
                {
                    return BadRequest("Para pessoa física, RG e Data de Nascimento são obrigatórios.");
                }

                // Regra 2: Se a empresa for do Paraná...
                if (empresa.UF == "PR")
                {
                    // Calcula a idade
                    var hoje = DateTime.Today;
                    // Usa a data do Fornecedor para o cálculo, que já foi tratada para HasValue acima.
                    var dataNascimento = fornecedor.DataNascimento.Value;

                    var idade = hoje.Year - dataNascimento.Year;

                    // Ajuste para quem ainda não fez aniversário no ano
                    // Compara a data de hoje sem o ano da idade subtraído, com a data de nascimento.
                    if (dataNascimento.Date > hoje.AddYears(-idade))
                    {
                        idade--;
                    }

                    if (idade < 18)
                    {
                        return BadRequest("Não é permitido cadastrar fornecedor pessoa física menor de idade para empresas do Paraná.");
                    }
                }
            }
            else // Se for Pessoa Jurídica, zera os campos de PF
            {
                fornecedor.RG = null;
                fornecedor.DataNascimento = null;
            }

            // Seta a data de cadastro com a hora atual, sempre em UTC
            fornecedor.DataCadastro = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            _context.Fornecedores.Add(fornecedor);
            await _context.SaveChangesAsync();

            // Retorna o fornecedor recém-criado
            return Ok(fornecedor);
        }
        // DELETE: api/fornecedores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFornecedor(int id)
        {
            var fornecedor = await _context.Fornecedores
                .Include(f => f.Telefones) // Precisamos incluir os telefones para apagá-los
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fornecedor == null)
            {
                return NotFound();
            }

            // 1. Remove os telefones associados (se houver)
            _context.Telefones.RemoveRange(fornecedor.Telefones);

            // 2. Remove o fornecedor
            _context.Fornecedores.Remove(fornecedor);

            await _context.SaveChangesAsync();

            return NoContent(); // Sucesso, sem conteúdo para retornar
        }
    }
}