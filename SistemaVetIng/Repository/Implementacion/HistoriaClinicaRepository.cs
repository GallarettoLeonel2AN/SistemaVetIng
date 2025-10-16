﻿using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Data;
using SistemaVetIng.Models;
using SistemaVetIng.Repository.Interfaces;

namespace SistemaVetIng.Repository.Implementacion
{
    public class HistoriaClinicaRepository : IHistoriaClinicaRepository
    {
        private readonly ApplicationDbContext _context;

        public HistoriaClinicaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> GetClientesPorBusqueda(string busqueda)
        {
            var clientes = from c in _context.Clientes
                           select c;

            if (!string.IsNullOrEmpty(busqueda))
            {
                var lowerBusqueda = busqueda.ToLower();
                clientes = clientes.Where(c => c.Nombre.ToLower().Contains(lowerBusqueda) ||
                                               c.Apellido.ToLower().Contains(lowerBusqueda) ||
                                               c.Dni.ToString().Contains(lowerBusqueda));
            }

            return await clientes.OrderBy(c => c.Apellido).ToListAsync();
        }

        public async Task<Cliente> GetMascotasClientes(int clienteId)
        {
            return await _context.Clientes
                                 .Include(c => c.Mascotas)
                                 .FirstOrDefaultAsync(c => c.Id == clienteId);
        }

        public async Task<Mascota> GetHistoriaClinicaCompletaMascota(int mascotaId)
        {
            return await _context.Mascotas
                                 .Include(m => m.Propietario)
                                 .Include(m => m.HistoriaClinica)
                                     .ThenInclude(hc => hc.Atenciones)
                                         .ThenInclude(a => a.Tratamiento)
                                 .Include(m => m.HistoriaClinica)
                                     .ThenInclude(hc => hc.Atenciones)
                                         .ThenInclude(a => a.Veterinario)
                                 .Include(m => m.HistoriaClinica)
                                     .ThenInclude(hc => hc.Atenciones)
                                         .ThenInclude(a => a.Vacunas)
                                 .Include(m => m.HistoriaClinica)
                                     .ThenInclude(hc => hc.Atenciones)
                                         .ThenInclude(a => a.EstudiosComplementarios)
                                 .FirstOrDefaultAsync(m => m.Id == mascotaId);
        }

        public async Task<HistoriaClinica> ObtenerPorMascotaIdAsync(int mascotaId)
        {
            return await _context.HistoriasClinicas.FirstOrDefaultAsync(h => h.MascotaId == mascotaId);
        }
    }
}

