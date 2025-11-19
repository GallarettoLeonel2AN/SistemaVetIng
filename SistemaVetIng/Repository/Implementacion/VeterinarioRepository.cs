using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using X.PagedList;
using X.PagedList.EF;

public class VeterinarioRepository : IVeterinarioRepository
{
    private readonly ApplicationDbContext _context;

    public VeterinarioRepository(ApplicationDbContext contexto)
    {
        _context = contexto;
    }

    public async Task<IEnumerable<Veterinario>> ListarTodo()
        => await _context.Veterinarios.Include(v => v.Usuario).ToListAsync();


    public async Task<Veterinario> ObtenerPorId(int id)
        => await _context.Veterinarios.Include(v => v.Usuario) .FirstOrDefaultAsync(v => v.Id == id);


    public async Task<Veterinario> ObtenerPorIdUsuario(int Usuario)
    {
        return await _context.Veterinarios.FirstOrDefaultAsync(c => c.UsuarioId == Usuario);

    }

    public async Task<bool> ExisteDniAsync(long dni)
    {
        return await _context.Veterinarios.AnyAsync(c => c.Dni == dni);
    }

    public async Task Agregar(Veterinario entity)
        => await _context.Veterinarios.AddAsync(entity);

    public void Modificar(Veterinario entity)
    {
        _context.Veterinarios.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Eliminar(Veterinario entity)
        => _context.Veterinarios.Remove(entity);

    public async Task Guardar()
        => await _context.SaveChangesAsync();

    public async Task<IPagedList<Veterinario>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null)
    {
        var query = _context.Veterinarios.Include(c => c.Usuario).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            query = query.Where(c => c.Nombre.Contains(busqueda) ||
                                     c.Apellido.Contains(busqueda) ||
                                     (c.Usuario != null && c.Usuario.Email.Contains(busqueda)));
        }

        // Ordenar antes de paginar
        query = query.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre);

        // Aplicar paginacion
        return await query.ToPagedListAsync(pageNumber, pageSize);
    }
}