using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KnewinApi.Data;
using KnewinApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnewinApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Define a rota como /api/empresas
    public class EmpresasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmpresasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/empresas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Empresa>>> GetEmpresas()
        {
            // Inclui os fornecedores relacionados a cada empresa
            return await _context.Empresas.Include(e => e.Fornecedores).ToListAsync();
        }

        // GET: api/empresas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Empresa>> GetEmpresa(int id)
        {
            // Inclui os fornecedores ao buscar uma empresa específica
            var empresa = await _context.Empresas
                .Include(e => e.Fornecedores)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (empresa == null)
            {
                return NotFound();
            }

            return empresa;
        }

        // POST: api/empresas
        [HttpPost]
        public async Task<ActionResult<Empresa>> PostEmpresa(Empresa empresa)
        {
            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            // Retorna um status 201 Created com a nova empresa
            return CreatedAtAction(nameof(GetEmpresa), new { id = empresa.Id }, empresa);
        }

        // PUT: api/empresas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmpresa(int id, Empresa empresa)
        {
            if (id != empresa.Id)
            {
                return BadRequest();
            }

            _context.Entry(empresa).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Empresas.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/empresas/5
        // DELETE: api/empresas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmpresa(int id)
        {
            // Verifica se algum fornecedor ESTÁ USANDO esta empresa
            var temFornecedores = await _context.Fornecedores.AnyAsync(f => f.EmpresaId == id);

            if (temFornecedores)
            {
                // Retorna um erro 400 (Bad Request)
                return BadRequest("Não é possível apagar a empresa pois ela possui fornecedores cadastrados. Remova os fornecedores primeiro.");
            }

            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
            {
                return NotFound();
            }

            _context.Empresas.Remove(empresa);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}