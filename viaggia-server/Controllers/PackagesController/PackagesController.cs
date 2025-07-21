using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using viaggia_server.Data;
using viaggia_server.Models.Package;

namespace viaggia_server.Controllers.PackagesController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly AppDbContext _context; // Contexto do banco de dados injetado via injeção de dependência

        public PackagesController(AppDbContext context) // Injeção de dependência do AppDbContext
        {
            _context = context;
        }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<Package>>> GetAll() // Endpoint para obter todos os pacotes
        {
            return await _context.Packages.ToListAsync(); // Retorna todos os pacotes
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Package>> GetById(int id) // Endpoint para obter um pacote específico por ID
        {
            var package = await _context.Packages // Busca o pacote pelo ID
                .Include(p => p.PackageDates) // Inclui as datas do pacote
                .ThenInclude(d => d.PackageDateRoomTypes) // Inclui os tipos de quarto associados a cada data do pacote
                .FirstOrDefaultAsync(p => p.PackageId == id); // Filtra pelo ID do pacote

            if (package == null) // Se o pacote não for encontrado, retorna NotFound
                return NotFound(); 

            return package;
        }

        [HttpPost]
        public async Task<ActionResult<Package>> Create(Package package) // Endpoint para criar um novo pacote
        {
            _context.Packages.Add(package); // Adiciona o pacote ao contexto
            await _context.SaveChangesAsync(); // Salva as alterações no banco de dados
            return CreatedAtAction(nameof(GetById), new { id = package.PackageId }, package); // Retorna o pacote criado com o status 201 Created
        }

        [HttpPut("{id}")] //    Endpoint para atualizar um pacote existente
        public async Task<IActionResult> Update(int id, Package package) // Recebe o ID do pacote a ser atualizado e o objeto do pacote atualizado
        {
            if (id != package.PackageId) // Verifica se o ID do pacote no corpo da requisição corresponde ao ID na URL
                return BadRequest();

            _context.Entry(package).State = EntityState.Modified; // Marca o pacote como modificado
            await _context.SaveChangesAsync(); // Salva as alterações no banco de dados
            return NoContent();
        }

        [HttpDelete("{id}")] // Endpoint para deletar um pacote existente
        public async Task<IActionResult> Delete(int id) // Recebe o ID do pacote a ser deletado
        {
            var package = await _context.Packages.FindAsync(id); // Busca o pacote pelo ID
            if (package == null) // Se o pacote não for encontrado, retorna NotFound
                return NotFound();

            _context.Packages.Remove(package); // Remove o pacote do contexto
            await _context.SaveChangesAsync(); // Salva as alterações no banco de dados
            return NoContent(); // Retorna NoContent (204) indicando que a operação foi bem-sucedida, mas não há conteúdo para retornar
        }


    }
}
