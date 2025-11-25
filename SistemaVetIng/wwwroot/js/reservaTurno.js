
function initializeReservaTurno(horariosUrl) {
    // Referencias a elementos del DOM
    const primeraCitaCheckbox = $('#primeraCitaCheckbox');
    const mascotaContainer = $('#mascotaContainer');
    const mascotaSelect = $('#mascotaSelect');

    const fechaInputSelector = "#fechaTurno"; // ID del input de fecha
    const horariosDisponiblesContainer = $('#horariosDisponiblesContainer');
    const horariosOpcionesDiv = $('#horariosOpciones');
    const horarioHiddenInput = $('#horarioHiddenInput');
    const reservaForm = $('form'); // Selector genérico para el form actual

    // INICIALIZAR FLATPICKR (Calendario Moderno)
    const calendario = flatpickr(fechaInputSelector, {
        locale: "es",              // Idioma español
        minDate: "today",          // No permitir fechas pasadas
        dateFormat: "Y-m-d",       // Formato ISO para el backend
        disableMobile: "true",     // Forzar estilo desktop en móvil
        defaultDate: "today",      // Pre-seleccionar hoy

        // Se dispara al elegir fecha
        onChange: function (selectedDates, dateStr, instance) {
            cargarHorariosDisponibles(dateStr);
        }
    });

    // LÓGICA DE MASCOTA / PRIMERA CITA
    function toggleMascotaSelection() {
        if (primeraCitaCheckbox.length && primeraCitaCheckbox.is(':checked')) {
            mascotaContainer.slideUp();
            mascotaSelect.prop('required', false);
            mascotaSelect.val("");
        } else {
            mascotaContainer.slideDown();
            mascotaSelect.prop('required', true);
        }
    }

    if (primeraCitaCheckbox.length) {
        primeraCitaCheckbox.on('change', toggleMascotaSelection);
        toggleMascotaSelection(); 
    }

    // FUNCIÓN PARA CARGAR HORARIOS
    function cargarHorariosDisponibles(fechaSeleccionada) {
        if (!fechaSeleccionada) {
            fechaSeleccionada = $(fechaInputSelector).val();
        }

        // Limpiar estado previo
        horarioHiddenInput.val('');
        horariosOpcionesDiv.empty();
        horariosDisponiblesContainer.show();

        // Mostrar spinner de carga
        horariosOpcionesDiv.html('<div style="text-align:center; color:white;"><i class="fas fa-spinner fa-spin"></i> Buscando horarios...</div>');

        if (!fechaSeleccionada) return;

        $.ajax({
            url: horariosUrl,
            type: 'GET',
            data: { fecha: fechaSeleccionada },
            success: function (horarios) {
                horariosOpcionesDiv.empty(); // Quitar spinner

                let horariosValidos = horarios;

                const ahoraMismo = new Date(); 

                // Creamos objetos fecha limpios para comparar solo AÑO-MES-DIA
                // Usamos replace(/-/g, '/') para asegurar compatibilidad en todos los navegadores
                const fechaElegidaObj = new Date(fechaSeleccionada + 'T00:00:00');
                const fechaHoyObj = new Date();

                // Reseteamos horas solo para la comparación
                fechaHoyObj.setHours(0, 0, 0, 0);
                fechaElegidaObj.setHours(0, 0, 0, 0);

                // Si la fecha elegida es HOY
                if (fechaElegidaObj.getTime() === fechaHoyObj.getTime()) {

                    const hora = ahoraMismo.getHours();
                    const minutos = ahoraMismo.getMinutes();

                    // Formato HH:mm para comparar strings (ej: "15:30")
                    const horaActualString = String(hora).padStart(2, '0') + ':' + String(minutos).padStart(2, '0');

                    console.log("Es hoy. Hora actual:", horaActualString); // Para depurar

                    // Filtramos los horarios mayores a la hora actual
                    horariosValidos = horarios.filter(h => h > horaActualString);
                }

                if (horariosValidos && horariosValidos.length > 0) {
                    horariosValidos.forEach(horario => {
                        const btn = $(`<button type="button" class="booking-time-btn" data-horario="${horario}">${horario}</button>`);
                        horariosOpcionesDiv.append(btn);
                    });
                } else {
                    horariosOpcionesDiv.html(`
                        <div class="alert alert-warning">
                           No hay horarios disponibles para esta fecha.
                        </div>
                    `);
                }
            },
            error: function () {
                horariosOpcionesDiv.html('<div class="text-danger" style="text-align:center;">Error al cargar horarios. Intente otra fecha.</div>');
            }
        });
    }

    // SELECCIÓN DE HORARIO (Click en botón)
    horariosOpcionesDiv.on('click', '.booking-time-btn', function () {
        // Remover clase activa de todos
        horariosOpcionesDiv.find('.booking-time-btn').removeClass('active');
        // Agregar al clickeado
        $(this).addClass('active');
        // Guardar valor en input oculto
        horarioHiddenInput.val($(this).data('horario'));
    });

    // ENVÍO DEL FORMULARIO
    reservaForm.on('submit', function (e) {
        e.preventDefault();

        const esPrimeraCita = primeraCitaCheckbox.length ? primeraCitaCheckbox.is(':checked') : true;
        const mascotaSeleccionada = mascotaSelect.length ? mascotaSelect.val() : null;
        const horarioSeleccionado = horarioHiddenInput.val();

        // Validaciones Frontend
        if (!horarioSeleccionado) {
            toastr.warning("Por favor, selecciona un horario disponible.");
            return;
        }

        if (!esPrimeraCita && !mascotaSeleccionada) {
            toastr.warning("Debes seleccionar una mascota o marcar 'Primera Cita'.");
            return;
        }

        // Preparar envío
        const formData = new FormData(this);
        const submitButton = $(this).find('button[type="submit"]');
        const originalBtnText = submitButton.html();

        submitButton.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Procesando...');

        $.ajax({
            url: this.action,
            type: this.method,
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success("¡Turno reservado con éxito!");
                    // Redirección suave
                    setTimeout(() => window.location.href = response.redirectUrl, 1500);
                } else {
                    toastr.error(response.message || "No se pudo reservar el turno.");
                    submitButton.prop('disabled', false).html(originalBtnText);
                }
            },
            error: function () {
                toastr.error("Error de conexión. Intente nuevamente.");
                submitButton.prop('disabled', false).html(originalBtnText);
            }
        });
    });

    // Cargar horarios iniciales (por si entra con fecha pre-seleccionada)
    // Pequeño timeout para asegurar que Flatpickr esté listo
    setTimeout(() => {
        const fechaInicial = $(fechaInputSelector).val();
        if (fechaInicial) {
            cargarHorariosDisponibles(fechaInicial);
        }
    }, 100);
}

// INICIALIZADOR GLOBAL
$(document).ready(function () {
    // Buscamos el form y extraemos la URL de horarios del atributo data
    const formElement = $('form[data-horarios-url]');
    if (formElement.length) {
        const horariosUrl = formElement.data('horarios-url');
        initializeReservaTurno(horariosUrl);
    }
});