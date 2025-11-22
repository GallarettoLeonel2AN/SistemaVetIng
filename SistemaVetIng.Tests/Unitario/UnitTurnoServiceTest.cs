using Moq;
using SistemaVetIng.Models.Singleton;
using SistemaVetIng.Repository.Interfaces;
using SistemaVetIng.Servicios.Implementacion;
using SistemaVetIng.Servicios.Interfaces;

namespace SistemaVetIng.Tests.Unitario
{
    public class UnitTurnoServiceTests
    {
        private readonly Mock<ITurnoRepository> _mockTurnoRepo;
        private readonly TurnoService _service;
        private readonly Mock<IVeterinariaConfigService> _mockVeterinariaConfigService;
        private readonly Mock<IConfiguracionHorarioCache> _mockCache;

        public UnitTurnoServiceTests()
        {
            // Mock es para simular nuestro repositorio y realizar las pruebas sobre esa bd que no es real
            _mockTurnoRepo = new Mock<ITurnoRepository>();
            _mockVeterinariaConfigService = new Mock<IVeterinariaConfigService>(); // Inicializamos ConfigService porque lo tenemos en el constructor real del Service
           
            _mockCache = new Mock<IConfiguracionHorarioCache>();

            // Creamos una instancia real de nuestro servicio, pero le pasamos el repositorio falso (mock).
            _service = new TurnoService(
               _mockVeterinariaConfigService.Object,
               _mockTurnoRepo.Object,
               _mockCache.Object
           );
        }

        [Fact]
        public async Task CalcularPorcentajeAusentismo_ConDatosValidos_DebeDevolverPorcentajeCorrecto()
        {
            // Preparamos escenario
            int turnosAusentes = 1;
            int turnosFinalizados = 1;
            double porcentajeEsperado = 50.0; // (1 / (1 + 1)) * 100

            // Configuramos el mock
            _mockTurnoRepo.Setup(repo => repo.ContarTurnosPorEstadoAsync("No Asistió"))
                          .ReturnsAsync(turnosAusentes);

            _mockTurnoRepo.Setup(repo => repo.ContarTurnosPorEstadoAsync("Finalizado"))
                          .ReturnsAsync(turnosFinalizados);

            // Ejecutamos metodo que queremos probar
            var resultado = await _service.CalcularPorcentajeAusentismoAsync();

            // Verificamos si el resultado es el esperado
            Assert.Equal(porcentajeEsperado, resultado);
        }

        [Fact]
        public async Task CalcularPorcentajeAusentismo_SinTurnos_DebeDevolverCero()
        {

            // Preparamos el escenario de division por cero
            int turnosAusentes = 0;
            int turnosFinalizados = 0;
            double porcentajeEsperado = 0.0; // (0 / (0 + 0)) 

            _mockTurnoRepo.Setup(repo => repo.ContarTurnosPorEstadoAsync("No Asistió"))
                          .ReturnsAsync(turnosAusentes);

            _mockTurnoRepo.Setup(repo => repo.ContarTurnosPorEstadoAsync("Finalizado"))
                          .ReturnsAsync(turnosFinalizados);

            var resultado = await _service.CalcularPorcentajeAusentismoAsync();

            Assert.Equal(porcentajeEsperado, resultado);
        }

        [Fact]
        public async Task CalcularPorcentajeAusentismo_SoloAusentes_DebeDevolverCien()
        {

            // Preparamos el escenario donde todos los turnos relevantes fueron ausencias.
            int turnosAusentes = 5;
            int turnosFinalizados = 0;
            double porcentajeEsperado = 100.0; // (5 / (5 + 0)) * 100

            _mockTurnoRepo.Setup(repo => repo.ContarTurnosPorEstadoAsync("No Asistió"))
                          .ReturnsAsync(turnosAusentes);

            _mockTurnoRepo.Setup(repo => repo.ContarTurnosPorEstadoAsync("Finalizado"))
                          .ReturnsAsync(turnosFinalizados);

            var resultado = await _service.CalcularPorcentajeAusentismoAsync();

            // Verificamos que el resultado sea 100%
            Assert.Equal(porcentajeEsperado, resultado);
        }
    }
}
