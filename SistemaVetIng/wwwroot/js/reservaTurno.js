
// Logica principal del formulario de reserva
function initializeReservaTurno(horariosUrl) {
    const primeraCitaCheckbox = $('#primeraCitaCheckbox');
    const mascotaContainer = $('#mascotaContainer');
    const mascotaSelect = $('#mascotaSelect');

    const fechaTurnoInput = $('#fechaTurno');
    const horariosDisponiblesContainer = $('#horariosDisponiblesContainer');
    const horariosOpcionesDiv = $('#horariosOpciones');
    const horarioHiddenInput = $('#horarioHiddenInput');
    const reservaForm = $('form.main-form');

    // Logica para alternar la seleccion de mascota (Primera Cita)
    function toggleMascotaSelection() {
        if (primeraCitaCheckbox.length && primeraCitaCheckbox.is(':checked')) {
            mascotaContainer.hide();
            mascotaSelect.prop('required', false); 
            mascotaSelect.val(""); 
        } else {
            mascotaContainer.show();
            mascotaSelect.prop('required', true);
        }
    }

    if (primeraCitaCheckbox.length) {
        primeraCitaCheckbox.on('change', toggleMascotaSelection);
        toggleMascotaSelection();
    }

    // Logica para la restriccion de fecha anteriores a la de hoy 
    function getTodayDateString() {
        const today = new Date();
        const year = today.getFullYear();
        const month = String(today.getMonth() + 1).padStart(2, '0');
        const day = String(today.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }
    const minDate = getTodayDateString();
    fechaTurnoInput.attr('min', minDate);
    if (fechaTurnoInput.val() < minDate) {
        fechaTurnoInput.val(minDate);
    }

    // Logica para cargar los horarios disponibles
    function cargarHorariosDisponibles() {
        const fechaSeleccionada = fechaTurnoInput.val();

        // Siempre limpiamos el horario seleccionado al cambiar la fecha.
        horarioHiddenInput.val('');
        horariosOpcionesDiv.find('.horario-btn').removeClass('selected');

        if (fechaSeleccionada) {

            if (fechaSeleccionada < minDate) {
                horariosOpcionesDiv.empty().html('<p class="text-danger validation-error">No se pueden buscar horarios para fechas pasadas.</p>');
                horariosDisponiblesContainer.show();
                return;
            }

            horariosOpcionesDiv.empty().html('<p>Cargando horarios...</p>');
            horariosDisponiblesContainer.show();

            $.ajax({
                url: horariosUrl,
                type: 'GET',
                data: { fecha: fechaSeleccionada },
                success: function (horarios) {
                    horariosOpcionesDiv.empty();

                    let horariosValidos = horarios;

                    if (fechaSeleccionada === minDate) {
                        const ahora = new Date();
                        const horaActualString = String(ahora.getHours()).padStart(2, '0') + ':' + String(ahora.getMinutes()).padStart(2, '0');
                        horariosValidos = horarios.filter(horario => horario > horaActualString);
                    }

                    if (horariosValidos && horariosValidos.length > 0) {
                        horariosValidos.forEach(horario => {
                            const btn = `<button type="button" class="horario-btn" data-horario="${horario}">${horario}</button>`;
                            horariosOpcionesDiv.append(btn);
                        });
                    } else {
                        horariosOpcionesDiv.html('<p class="text-danger validation-error">No hay horarios disponibles para la fecha seleccionada.</p>');
                    }
                },
                error: function () {
                    horariosOpcionesDiv.html('<p class="text-danger validation-error">Ocurrió un error al cargar los horarios. Por favor, intente de nuevo.</p>');
                }
            });
        } else {
            horariosDisponiblesContainer.hide();
        }
    }

    // Logica para seleccionar un boton de horario 
    horariosOpcionesDiv.on('click', '.horario-btn', function () {
        horariosOpcionesDiv.find('.horario-btn').removeClass('selected');
        $(this).addClass('selected');
        horarioHiddenInput.val($(this).data('horario'));
    });

    fechaTurnoInput.on('change', cargarHorariosDisponibles);
    cargarHorariosDisponibles();

    // Logica para el envio del formulario con Ajax
    reservaForm.on('submit', function (e) {

        e.preventDefault();

        // Si NO existe el checkbox: es Primera Cita obligatoriamente
        const esPrimeraCita = primeraCitaCheckbox.length ? primeraCitaCheckbox.is(':checked') : true;
        const mascotaSeleccionada = mascotaSelect.length ? mascotaSelect.val() : null;
        const horarioSeleccionado = horarioHiddenInput.val();

        if (!horarioSeleccionado) {
            toastr.error("Por favor, selecciona una fecha y un horario disponible.");
            return; 
        }

        // Condición de error: NO es primera cita Y NO se seleccionó mascota
        if (!esPrimeraCita && (mascotaSeleccionada === "" || mascotaSeleccionada === null)) {
            toastr.error("Por favor, selecciona una mascota o marca la opción de 'Primera Cita'.");
            return; 
        }


        const formData = new FormData(this);

        const submitButton = $(this).find('button[type="submit"]');
        submitButton.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Reservando...');

        $.ajax({
            url: this.action,
            type: this.method,
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success("¡Turno reservado con éxito!");
                    // Redirige dspues de 2 segundos para que el usuario vea el mensaje
                    setTimeout(() => window.location.href = response.redirectUrl, 2000);

                } else {
                    toastr.error(response.message || "Ocurrió un error al reservar el turno.");
                    submitButton.prop('disabled', false).html('<i class="fa-solid fa-calendar-plus"></i> Confirmar Turno');
                }
            },
            error: function (xhr, status, error) {
                toastr.error("Ocurrió un error al conectar con el servidor. Por favor, intente de nuevo.");
                submitButton.prop('disabled', false).html('<i class="fa-solid fa-calendar-plus"></i> Confirmar Turno');
            },
        });
    });
}

// Inicializa las funciones
$(document).ready(function () {
    const formElement = $('form.main-form');
    if (formElement.length) {
        const horariosUrl = formElement.data('horarios-url');
        if (horariosUrl) {
            initializeReservaTurno(horariosUrl);
        }
    }
});