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
    [Route("api/[controller]")] // Define a rota como /api/telefones
    public class TelefonesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TelefonesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/telefones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Telefone>>> GetTelefones()
        {
            return await _context.Telefones
                .Include(t => t.Fornecedor) // Inclui o fornecedor de cada telefone
                .ToListAsync();
        }

        // GET: api/telefones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Telefone>> GetTelefone(int id)
        {
            var telefone = await _context.Telefones
                .Include(t => t.Fornecedor)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (telefone == null)
            {
                return NotFound();
            }

            return telefone;
        }

        // POST: api/telefones
        [HttpPost]
        public async Task<ActionResult<Telefone>> PostTelefone(Telefone telefone)
        {
            _context.Telefones.Add(telefone);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTelefone), new { id = telefone.Id }, telefone);
        }

        // PUT: api/telefones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTelefone(int id, Telefone telefone)
        {
            if (id != telefone.Id)
            {
                return BadRequest();
            }

            _context.Entry(telefone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Telefones.Any(e => e.Id == id))
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

        // DELETE: api/telefones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTelefone(int id)
        {
            var telefone = await _context.Telefones.FindAsync(id);
            if (telefone == null)
            {
                return NotFound();
            }

            _context.Telefones.Remove(telefone);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}