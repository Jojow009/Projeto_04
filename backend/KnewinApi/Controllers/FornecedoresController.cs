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
                query = query.Where(f => f.Nome.Contains(nome));
            }
            if (!string.IsNullOrEmpty(cpfCnpj))
            {
                // Busca pelo valor exato (sem máscara, como salvamos)
                query = query.Where(f => f.CpfCnpj == cpfCnpj);
            }
            if (dataCadastro.HasValue)
            {
                query = query.Where(f => f.DataCadastro.Date == dataCadastro.Value.Date);
            }

            var fornecedores = await query.ToListAsync();
            return Ok(fornecedores);
        }

        // POST: api/fornecedores
        // Implementa as Regras 2 (Paraná) e 3 (RG/Nascimento)
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
            bool isPessoaFisica = fornecedor.CpfCnpj.Length == 11;

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
                    var idade = hoje.Year - fornecedor.DataNascimento.Value.Year;
                    // Ajuste para quem ainda não fez aniversário no ano
                    if (fornecedor.DataNascimento.Value.Date > hoje.AddYears(-idade))
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

            // Seta a data de cadastro
            fornecedor.DataCadastro = DateTime.UtcNow;

            _context.Fornecedores.Add(fornecedor);
            await _context.SaveChangesAsync();

            // Retorna um status 201 Created com os dados do novo fornecedor
            // (Mudança: Retornando apenas "Ok" para evitar erros de rota no CreatedAtAction)
            return Ok(fornecedor);
        }
    }
}