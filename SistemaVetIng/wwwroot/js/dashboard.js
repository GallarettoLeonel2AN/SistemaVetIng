$(document).ready(function () {


    // Instancias de Chart.js

    let turnosStatusChart = null;
    let atencionesVetChart = null;
    let serviciosChart = null;
    let especiesChart = null;
    let ingresosChart = null;



    // Drill state - ingresos

    let currentIngresosLevel = 1; // 1: anual, 2: mensual, 3: semanal

    let selectedAnioData = null;

    let selectedMesData = null;



    // Drill state - turnos

    let currentTurnosLevel = 1;



    // ---------- Utilidades ----------

    function safeNumber(val) {

        return (val === null || val === undefined) ? 0 : Number(val);

    }



    function clearCanvasMessage(canvasId) {

        const canvas = document.getElementById(canvasId);

        if (!canvas) return;

        const ctx = canvas.getContext('2d');

        ctx.clearRect(0, 0, canvas.width, canvas.height);

    }



    function drawCanvasMessage(canvasId, message) {

        const canvas = document.getElementById(canvasId);

        if (!canvas) return;

        if (canvas && canvas.getContext) {

            const ctx = canvas.getContext('2d');

            ctx.clearRect(0, 0, canvas.width, canvas.height);

            const dpr = window.devicePixelRatio || 1;

            const width = canvas.width;

            const height = canvas.height;

            ctx.save();

            ctx.font = "16px Arial";

            ctx.fillStyle = "#6b7280";

            ctx.textAlign = "center";

            ctx.textBaseline = "middle";

            ctx.fillText(message, width / 2, height / 2);

            ctx.restore();

        }

    }



    // ============================

    // ESTADOS DE TURNOS - Nivel 1

    // ============================

    function renderTurnosStatusChart(data) {

        const canvasId = 'chartRendimiento';

        const canvas = document.getElementById(canvasId);

        if (!canvas) {

            console.error(`Canvas ${canvasId} no encontrado.`);

            return;

        }



        // destruir previos

        if (turnosStatusChart) { turnosStatusChart.destroy(); turnosStatusChart = null; }

        if (atencionesVetChart) { atencionesVetChart.destroy(); atencionesVetChart = null; }



        currentTurnosLevel = 1;



        const labels = ['Pendientes', 'Finalizados', 'Cancelados', 'No Asistió'];

        const counts = [

            safeNumber(data.totalTurnosPendientes),

            safeNumber(data.totalTurnosFinalizados),

            safeNumber(data.totalTurnosCancelados),

            safeNumber(data.totalTurnosNoAsistio)

        ];



        const colors = ['#FFC107', '#4CAF50', '#F44336', '#6c757d'];



        // Si todos cero -> mensaje

        if (counts.every(c => c === 0)) {

            drawCanvasMessage(canvasId, "No hay turnos para mostrar.");

            return;

        }



        try {

            turnosStatusChart = new Chart(canvas.getContext('2d'), {

                type: 'bar',

                data: {

                    labels: labels,

                    datasets: [{

                        label: 'Cantidad de Turnos',

                        data: counts,

                        backgroundColor: colors

                    }]

                },

                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    indexAxis: 'y',

                    scales: { x: { beginAtZero: true } },

                    plugins: {

                        legend: { display: false },

                        title: { display: true, text: 'Distribución de Turnos por Estado (Total Histórico)' }

                    },

                    onClick: (evt) => {

                        if (!turnosStatusChart) return;

                        const points = turnosStatusChart.getElementsAtEventForMode(evt, 'nearest', { intersect: true }, true);

                        if (points.length) {

                            const idx = points[0].index;

                            const labelClicked = labels[idx];

                            if (labelClicked === 'Finalizados') {

                                handleTurnosDrillDownToLevel2();

                            }

                        }

                    }

                }

            });

            $('#drillUpButton').hide();

            $('#drillDownTitle').text('Distribución de Turnos por Estado');

        } catch (err) {

            console.error("Error renderTurnosStatusChart:", err);

            drawCanvasMessage(canvasId, "Error al generar el gráfico de turnos.");

        }

    }



    // ============================

    // TURNOS FINALIZADOS POR VETERINARIOS - Nivel 2

    // ============================

    function renderAtencionesPorVeterinarioChart(data) {

        const canvasId = 'chartRendimiento';

        const canvas = document.getElementById(canvasId);

        if (!canvas) {

            console.error('Canvas para atenciones vet no encontrado.');

            return;

        }



        if (atencionesVetChart) { atencionesVetChart.destroy(); atencionesVetChart = null; }

        if (turnosStatusChart) { turnosStatusChart.destroy(); turnosStatusChart = null; }



        currentTurnosLevel = 2;



        if (!Array.isArray(data) || data.length === 0) {

            drawCanvasMessage(canvasId, "No hay datos de atenciones por veterinario.");

            return;

        }



        // ordenar por cantidad desc

        data.sort((a, b) => (b.cantidadAtenciones || 0) - (a.cantidadAtenciones || 0));



        const labels = data.map(d => d.nombreVeterinario || "Sin Nombre");

        const counts = data.map(d => safeNumber(d.cantidadAtenciones));

        const backgroundColors = ['#4299E1', '#48BB78', '#F56565', '#ED8936', '#9F7AEA', '#38B2AC'];



        try {

            atencionesVetChart = new Chart(canvas.getContext('2d'), {

                type: 'bar',

                data: {

                    labels,

                    datasets: [{

                        label: 'Atenciones Realizadas',

                        data: counts,

                        backgroundColor: backgroundColors.slice(0, counts.length)

                    }]

                },

                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    scales: { y: { beginAtZero: true } },

                    plugins: {

                        legend: { display: false },

                        title: { display: true, text: 'Atenciones Realizadas por Veterinario (Histórico)' }

                    }

                }

            });

            $('#drillUpButton').show();

            $('#drillDownTitle').text('Atenciones por Veterinario');

        } catch (err) {

            console.error("Error renderAtencionesPorVeterinarioChart:", err);

            drawCanvasMessage(canvasId, "Error al generar gráfico de atenciones.");

        }

    }



    // ============================

    // TOP SERVICIOS (VACUNAS Y ESTUDIOS) 

    // ============================

    function renderServiciosChart(data) {

        const canvasId = 'chartServicios';

        const canvas = document.getElementById(canvasId);

        if (!canvas) { console.error('chartServicios no encontrado'); return; }



        if (serviciosChart) { serviciosChart.destroy(); serviciosChart = null; }



        if (!Array.isArray(data) || data.length === 0) {

            drawCanvasMessage(canvasId, "No hay datos de servicios.");

            return;

        }



        const labels = data.map(d => d.nombreServicio || 'Sin Nombre');

        const values = data.map(d => safeNumber(d.cantidadSolicitudes));

        const backgroundColors = ['#4299E1', '#48BB78', '#F56565', '#ED8936', '#9F7AEA', '#38B2AC', '#ECC94B'];



        try {

            serviciosChart = new Chart(canvas.getContext('2d'), {

                type: 'pie',

                data: {

                    labels,

                    datasets: [{

                        label: 'Solicitudes',

                        data: values,

                        backgroundColor: backgroundColors.slice(0, values.length),

                        hoverOffset: 4

                    }]

                },

                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    plugins: {

                        title: { display: true, text: 'Top Servicios (Histórico)' },

                        legend: { position: 'bottom' }

                    }

                }

            });

        } catch (err) {

            console.error("Error renderServiciosChart:", err);

            drawCanvasMessage(canvasId, "Error al generar el gráfico de servicios.");

        }

    }



    // ============================

    // TOP ESPECIES GRAFICO DE DONA

    // ============================

    function renderEspeciesChart(data) {

        const canvasId = 'chartEspecies';

        const canvas = document.getElementById(canvasId);

        if (!canvas) { console.error('chartEspecies no encontrado'); return; }



        if (especiesChart) { especiesChart.destroy(); especiesChart = null; }



        if (!Array.isArray(data) || data.length === 0) {

            drawCanvasMessage(canvasId, "No hay mascotas registradas.");

            return;

        }



        const labels = data.map(d => d.especie || 'Sin Especie');

        const values = data.map(d => safeNumber(d.cantidad));

        const colors = [

            'rgba(75, 192, 192, 0.8)',

            'rgba(255, 159, 64, 0.8)',

            'rgba(153, 102, 255, 0.8)',

            'rgba(255, 99, 132, 0.8)',

            'rgba(54, 162, 235, 0.8)',

            'rgba(255, 206, 86, 0.8)'

        ];



        try {

            especiesChart = new Chart(canvas.getContext('2d'), {

                type: 'doughnut',

                data: {

                    labels,

                    datasets: [{

                        label: 'Mascotas',

                        data: values,

                        backgroundColor: colors.slice(0, values.length),

                        hoverOffset: 8

                    }]

                },

                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    plugins: {

                        title: { display: true, text: 'Distribución por Especie' },

                        legend: { position: 'bottom' }

                    },

                    onClick: (evt) => {

                        if (!especiesChart) return;

                        const points = especiesChart.getElementsAtEventForMode(evt, 'nearest', { intersect: true }, true);

                        if (points.length) {

                            const idx = points[0].index;

                            const especieSeleccionada = labels[idx];

                            updateRazasList(especieSeleccionada);

                        }

                    }

                }

            });

        } catch (err) {

            console.error("Error renderEspeciesChart:", err);

            drawCanvasMessage(canvasId, "Error al generar el gráfico de especies.");

        }

    }



    // ============================

    // INGRESOS - Nivel 1 (Anual)

    // ============================

    function renderIngresosNivel1Chart(data) {

        const canvasId = 'chartIngresosMensuales';

        const canvas = document.getElementById(canvasId);

        if (!canvas) { console.error('chartIngresosMensuales no encontrado'); return; }



        if (ingresosChart) { ingresosChart.destroy(); ingresosChart = null; }



        if (!Array.isArray(data) || data.length === 0) {

            drawCanvasMessage(canvasId, "No hay datos de ingresos.");

            return;

        }





        const labels = data.map(d => d.anio || d.Anio || '—');

        const values = data.map(d => safeNumber(d.ingresoRealAnual ?? d.IngresoRealAnual ?? 0));

        const metas = data.map(d => safeNumber(d.metaAnual ?? d.MetaAnual ?? 0));



        const backgroundColors = values.map((val, i) => {

            const estado = (data[i].estadoSemaforo || data[i].EstadoSemaforo || null);

            if (estado) {

                const e = estado.toString().toLowerCase();

                if (e === 'verde') return '#22c55e';

                if (e === 'amarillo') return '#facc15';

                if (e === 'rojo') return '#ef4444';

            }



            const meta = metas[i] || 0;

            if (meta > 0) {

                if (val >= meta) return '#22c55e';

                if (val >= meta * 0.8) return '#facc15';

            }

            return '#ef4444';

        });



        try {

            ingresosChart = new Chart(canvas.getContext('2d'), {

                type: 'bar',

                data: {

                    labels,

                    datasets: [

                        {

                            label: 'Ingreso Real',

                            data: values,

                            backgroundColor: backgroundColors

                        },

                        {

                            label: 'Meta Anual',

                            data: metas,

                            type: 'line',

                            borderColor: '#2563eb',

                            borderWidth: 2,

                            pointRadius: 0,

                            borderDash: [6, 4],

                            fill: false

                        }

                    ]

                },

                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    scales: { y: { beginAtZero: true } },

                    plugins: {

                        legend: { display: true },

                        title: { display: true, text: 'Ingresos por Año' }

                    },

                    onClick: (evt) => {

                        const points = ingresosChart.getElementsAtEventForMode(evt, 'nearest', { intersect: true }, true);

                        if (points.length) {

                            const idx = points[0].index;

                            selectedAnioData = data[idx];

                            currentIngresosLevel = 2;

                            // si hay ingresosMensuales, renderizarlos

                            if (selectedAnioData && selectedAnioData.ingresosMensuales && selectedAnioData.ingresosMensuales.length > 0) {

                                renderIngresosNivel2Chart(selectedAnioData.ingresosMensuales);

                                $('#ingresosDrillUpButton').show();

                                $('#ingresosDrillDownTitle').text ? $('#ingresosDrillDownTitle').text(`Ingreso ${selectedAnioData.anio}`) : null;

                            } else {

                                drawCanvasMessage(canvasId, "No hay datos mensuales para el año seleccionado.");

                            }

                        }

                    }

                }

            });

            $('#ingresosDrillUpButton').hide();

            $('#ingresosDrillDownTitle').text ? $('#ingresosDrillDownTitle').text('Rendimiento de Ingresos: Visión Anual') : null;

        } catch (err) {

            console.error("Error renderIngresosNivel1Chart:", err);

            drawCanvasMessage(canvasId, "Error al generar gráfico de ingresos anuales.");

        }

    }



    // ============================

    // INGRESOS - Nivel 2 (Mensual)

    // ============================

    function renderIngresosNivel2Chart(data) {

        const canvasId = 'chartIngresosMensuales';

        const canvas = document.getElementById(canvasId);

        if (!canvas) { console.error('chartIngresosMensuales no encontrado'); return; }



        if (ingresosChart) { ingresosChart.destroy(); ingresosChart = null; }



        if (!Array.isArray(data) || data.length === 0) {

            drawCanvasMessage(canvasId, "No hay datos mensuales.");

            return;

        }



        const labels = data.map(d => d.nombreMes || (`Mes ${d.mesNumero || '—'}`));

        const values = data.map(d => safeNumber(d.ingresoRealMensual ?? d.IngresoRealMensual ?? 0));

        const metas = data.map(d => safeNumber(d.metaMensual ?? d.MetaMensual ?? 0));



        const backgroundColors = values.map((val, i) => {

            const estado = (data[i].estadoSemaforo || data[i].EstadoSemaforo || null);

            if (estado) {

                const e = estado.toString().toLowerCase();

                if (e === 'verde') return '#22c55e';

                if (e === 'amarillo') return '#9ca3af';

                if (e === 'rojo') return '#ef4444';

            }

            const meta = metas[i] || 0;

            if (meta > 0) {

                if (val >= meta) return '#22c55e';

                if (val >= meta * 0.8) return '#9ca3af';

            }

            return '#ef4444';

        });



        try {

            ingresosChart = new Chart(canvas.getContext('2d'), {

                type: 'bar',

                data: {

                    labels,

                    datasets: [

                        {

                            label: 'Ingreso Mensual',

                            data: values,

                            backgroundColor: backgroundColors

                        },

                        {

                            label: 'Meta Mensual',

                            data: metas,

                            type: 'line',

                            borderColor: '#2563eb',

                            borderWidth: 2,

                            pointRadius: 0,

                            borderDash: [6, 4],

                            fill: false

                        }

                    ]

                },

                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    scales: { y: { beginAtZero: true } },

                    plugins: {

                        legend: { display: true },

                        title: { display: true, text: 'Ingresos Mensuales' }

                    },

                    onClick: (evt) => {

                        const points = ingresosChart.getElementsAtEventForMode(evt, 'nearest', { intersect: true }, true);

                        if (points.length) {

                            const idx = points[0].index;

                            selectedMesData = data[idx];

                            currentIngresosLevel = 3;

                            if (selectedMesData && selectedMesData.ingresosSemanales && selectedMesData.ingresosSemanales.length > 0) {

                                renderIngresosNivel3Chart(selectedMesData.ingresosSemanales);

                                $('#ingresosDrillUpButton').show();

                            } else {

                                drawCanvasMessage(canvasId, "No hay datos semanales para el mes seleccionado.");

                            }

                        }

                    }

                }

            });

            $('#ingresosDrillUpButton').show();

        } catch (err) {

            console.error("Error renderIngresosNivel2Chart:", err);

            drawCanvasMessage(canvasId, "Error al generar gráfico mensual de ingresos.");

        }

    }



    // ============================

    // INGRESOS - Nivel 3 (Semanal)

    // ============================

    function renderIngresosNivel3Chart(data) {

        const canvasId = 'chartIngresosMensuales';

        const canvas = document.getElementById(canvasId);

        if (!canvas) { console.error('chartIngresosMensuales no encontrado'); return; }

        if (ingresosChart) { ingresosChart.destroy(); ingresosChart = null; }



        if (!Array.isArray(data) || data.length === 0) {

            drawCanvasMessage(canvasId, "No hay datos semanales.");

            return;

        }



        const labels = data.map(d => d.semana || '—');

        const values = data.map(d => safeNumber(d.ingresoRealSemanal ?? d.IngresoRealSemanal ?? 0));



        try {

            ingresosChart = new Chart(canvas.getContext('2d'), {

                type: 'line',

                data: {

                    labels,

                    datasets: [{

                        label: 'Ingreso Semanal',

                        data: values,

                        fill: false,

                        tension: 0.3,

                        borderWidth: 2

                    }]

                },

                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    scales: { y: { beginAtZero: true } },

                    plugins: {

                        legend: { display: false },

                        title: { display: true, text: 'Ingresos Semanales' }

                    }

                }

            });

            $('#ingresosDrillUpButton').show();

        } catch (err) {

            console.error("Error renderIngresosNivel3Chart:", err);

            drawCanvasMessage(canvasId, "Error al generar gráfico semanal de ingresos.");

        }

    }



    // ============================

    // RAZAS - Actualizar lista (CUANDO SE HACE CLICK EN ESPECIE GRAFICO DE DONAS)

    // ============================



    function updateRazasList(especie) {



        if (typeof initialEspeciesData === 'undefined' || !Array.isArray(initialEspeciesData)) {

            $('#drillDownRazas').html('<div class="v1-empty-state small">Datos de razas no disponibles.</div>');

            return;

        }

        const found = initialEspeciesData.find(e => (e.especie || '').toString().toLowerCase() === (especie || '').toString().toLowerCase());

        if (!found || !found.razas || found.razas.length === 0) {

            $('#drillDownRazas').html('<div class="v1-empty-state small">No hay razas registradas para esta especie.</div>');

            return;

        }

        // construyo lista simple

        const listHtml = found.razas.map(r => `<div class="v1-list-item"><div>${r.nombre}</div><div class="v1-badge">${r.cantidad}</div></div>`).join('');

        $('#drillDownRazas').html(listHtml);

    }



    // ============================

    // Drill handlers

    // ============================

    function handleTurnosDrillDownToLevel2() {

        if (typeof initialAtencionesVetData !== 'undefined' && Array.isArray(initialAtencionesVetData) && initialAtencionesVetData.length > 0) {

            renderAtencionesPorVeterinarioChart(initialAtencionesVetData);

        } else {

            drawCanvasMessage('chartRendimiento', 'No hay datos detallados de atenciones.');

        }

    }



    function handleTurnosDrillUp() {

        if (currentTurnosLevel === 2) {

            if (typeof initialTurnosStatusData !== 'undefined') {

                renderTurnosStatusChart(initialTurnosStatusData);

            } else {

                drawCanvasMessage('chartRendimiento', 'Error al volver.');

            }

        }

    }

    $('#drillUpButton').on('click', handleTurnosDrillUp);



    function handleIngresosDrillUp() {

        if (currentIngresosLevel === 3) {

            // volver a mensual

            if (selectedAnioData && selectedAnioData.ingresosMensuales) {

                renderIngresosNivel2Chart(selectedAnioData.ingresosMensuales);

                currentIngresosLevel = 2;

            } else {

                renderIngresosNivel1Chart(initialIngresosAnualesData);

                currentIngresosLevel = 1;

            }

        } else if (currentIngresosLevel === 2) {

            selectedAnioData = null;

            selectedMesData = null;

            renderIngresosNivel1Chart(initialIngresosAnualesData);

            currentIngresosLevel = 1;

            $('#ingresosDrillUpButton').hide();

        }

    }

    $('#ingresosDrillUpButton').on('click', handleIngresosDrillUp);



    // ============================

    // Carga inicial

    // ============================

    function loadDashboardData() {

        // Turnos

        if (typeof initialTurnosStatusData !== 'undefined' && initialTurnosStatusData) {

            renderTurnosStatusChart(initialTurnosStatusData);

        } else {

            drawCanvasMessage('chartRendimiento', 'No se pudieron cargar los datos de turnos.');

        }



        // Servicios

        if (typeof initialServiciosData !== 'undefined' && Array.isArray(initialServiciosData) && initialServiciosData.length > 0) {

            renderServiciosChart(initialServiciosData);

        } else {

            drawCanvasMessage('chartServicios', 'No hay datos de servicios.');

        }



        // Especies

        if (typeof initialEspeciesData !== 'undefined' && Array.isArray(initialEspeciesData) && initialEspeciesData.length > 0) {

            renderEspeciesChart(initialEspeciesData);

        } else {

            drawCanvasMessage('chartEspecies', 'No hay mascotas registradas.');

        }



        // Ingresos

        if (typeof initialIngresosAnualesData !== 'undefined' && Array.isArray(initialIngresosAnualesData) && initialIngresosAnualesData.length > 0) {

            renderIngresosNivel1Chart(initialIngresosAnualesData);

        } else {

            drawCanvasMessage('chartIngresosMensuales', 'No hay datos de ingresos.');

        }

    }



    loadDashboardData();

});