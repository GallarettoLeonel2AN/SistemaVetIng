﻿using SistemaVetIng.Models;
using SistemaVetIng.ViewsModels;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IAtencionVeterinariaRepository
    {
        Task<HistoriaClinica> GetHistoriaClinicaConMascotayPropietario(int historiaClinicaId);
        Task<List<Vacuna>> GetVacunas();
        Task<List<Estudio>> GetEstudios();
        Task<Veterinario> GetVeterinarioPorId(int usuarioId);
        Task<List<Vacuna>> GetVacunaSeleccionada(IEnumerable<int> ids);
        Task<List<Estudio>> GetEstudioSeleccionado(IEnumerable<int> ids);
        Task AgregarAtencionVeterinaria(AtencionVeterinaria atencion);
        Task AgregarTratamiento(Tratamiento tratamiento);
        Task<List<AtencionVeterinaria>> ObtenerAtencionesPendientesPorCliente(int clienteId);
        Task<AtencionVeterinaria> ObtenerAtencionConCliente(int idAtencion);
        Task<AtencionVeterinaria> ObtenerPorId(int id);
        Task SaveChangesAsync();
        Task<HistoriaClinica> GetHistoriaClinicaPorId(int id);

        Task<decimal> SumarCostosAtencionesMesActualAsync();
        Task<(string Nombre, string Lote)> ObtenerVacunaMasFrecuenteAsync();
        Task<(string Nombre, decimal Precio)> ObtenerEstudioMasSolicitadoAsync();
        Task<int> CantidadAtencionesPorVeterinario(int id);
        Task<Mascota> ObtenerMascotaMasFrecuentePorVeterinario(int idVeterinario);
        Task<int> CantidadPagosPendientes(int idCliente);
        Task<int> CantidadAtenciones();
        Task<Cliente> ObtenerClienteMasFrecuenteAsync();
        Task<decimal> SumarIngresosAsync();
        Task<List<DashboardViewModel.IngresosAnualesData>> ObtenerDatosIngresosAnualesAsync(List<int> anios);
        Task<List<DashboardViewModel.ServicioCountData>> ContarTopServiciosAsync(int topN);
        Task<List<DashboardViewModel.IngresosMensualesData>> ObtenerDatosIngresosMensualesAsync(int anio);
        Task<List<DashboardViewModel.AtencionesPorVeterinarioData>> ContarAtencionesPorVeterinarioAsync(DateTime? inicio, DateTime? fin);
        Task<List<AtencionVeterinaria>> ObtenerAtencionesPorMesAsync(int anio, int mes);

        Task<List<AtencionVeterinaria>> ObtenerAtencionesPorIdCliente(List<int> ids);
        Task ActualizarAtencionesAsync(List<AtencionVeterinaria> atenciones);
        Task ActualizarAtencionAsync(AtencionVeterinaria atencion);
    }
}
