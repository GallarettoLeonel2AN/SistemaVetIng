﻿using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;
using X.PagedList;
using X.PagedList.EF;


namespace SistemaVetIng.Repository.Implementacion
{
    public class ClienteRepository : IClienteRepository
    {

        private readonly ApplicationDbContext _context;

        public ClienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Agregar(Cliente entity)
            => await _context.AddAsync(entity);

        public void Eliminar(Cliente entity)
            => _context.Clientes.Remove(entity);

        public Task Guardar()
            => _context.SaveChangesAsync();

        public async Task<IEnumerable<Cliente>> ListarTodo()
            => await _context.Clientes.Include(c => c.Usuario).ToListAsync();

        public void Modificar(Cliente entity)
        {
            _context.Clientes.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task<Cliente> ObtenerPorId(int id)
            => await _context.Clientes.Include(c => c.Usuario).FirstOrDefaultAsync(x => x.Id == id);

        public async Task<Cliente> ObtenerPorIdUsuario(int Usuario)
        {
            return await _context.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == Usuario);

        }

        public async Task<IPagedList<Cliente>> ListarPaginadoAsync(int pageNumber, int pageSize, string busqueda = null)
        {
            var query = _context.Clientes.Include(c => c.Usuario).AsQueryable();

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

        public async Task<int> ContarTotalClientesAsync()
        {
            return await _context.Clientes.CountAsync();
        }
    }
}
