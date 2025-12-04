$(document).ready(function () {

    // Seleccionar elementos del formulario
    const formPagos = $('#form-pagos');

    // Si el formulario no existe en esta página, no hacer nada.
    if (formPagos.length === 0) {
        return;
    }

    const totalSpan = $('#total-a-pagar');
    const pagarBtn = $('#btn-pagar-seleccionados');
    const token = formPagos.find('input[name="__RequestVerificationToken"]').val(); // Obtener el token

    /**
     * Recalcula el monto total sumando los checkboxes seleccionados.
     */
    function recalcularTotal() {
        let total = 0.0;

      
        // Buscamos los checkboxes CHEQUEADOS dentro del formulario CADA VEZ que se llama la función.
        formPagos.find('.chk-pago:checked').each(function () {
            const montoStr = $(this).data('monto').toString().replace(",", ".");
            const monto = parseFloat(montoStr);

            if (!isNaN(monto)) {
                total += monto;
            }
        });

        // Formatea como moneda (ARS)
        totalSpan.text(total.toLocaleString('es-AR', {
            style: 'currency',
            currency: 'ARS'
        }));
    }

    // --- Lógica de Interacción de la Lista ---


    formPagos.on('click', '.payment-item', function (e) {
        // Evita doble evento si se hace clic justo en el checkbox
        if ($(e.target).is('input[type="checkbox"]')) {
            return;
        }

        const checkbox = $(this).find('.chk-pago');
        checkbox.prop('checked', !checkbox.prop('checked'));
        checkbox.trigger('change'); // Dispara el evento 'change'
    });


    formPagos.on('change', '.chk-pago', function () {
        const isChecked = $(this).is(':checked');
        const parentItem = $(this).closest('.payment-item');

        // Aplica la clase para el estilo visual
        if (isChecked) {
            parentItem.addClass('is-selected');
        } else {
            parentItem.removeClass('is-selected');
        }

        // Actualiza el total
        recalcularTotal();
    });

    // --- Lógica de Envío del Formulario ---
    formPagos.on('submit', function (event) {
        event.preventDefault(); // Previene el envío normal

        const idsSeleccionados = [];
        formPagos.find('.chk-pago:checked').each(function () {
            idsSeleccionados.push($(this).val());
        });

        if (idsSeleccionados.length === 0) {
            toastr.error("Por favor, seleccione al menos una atención para pagar.");
            return;
        }

        // Deshabilitar botón
        pagarBtn.prop('disabled', true).html('<i class="fa-solid fa-spinner fa-spin"></i> Procesando...');


        $.ajax({
            url: '/Pagos/GenerarLinkDePago', 
            method: 'POST',
            headers: {
                'RequestVerificationToken': token // Token Anti-Forgery
            },
            contentType: 'application/json',
            data: JSON.stringify(idsSeleccionados), // Envía los IDs como un array JSON
            success: function (data) {
                // Si el servidor responde con una URL de redirección
                if (data && data.redirectUrl) {
                    window.location.href = data.redirectUrl;
                } else {
                    toastr.error('No se recibió una URL de pago válida.');
                    // Habilitar botón de nuevo si hay un error lógico
                    pagarBtn.prop('disabled', false).html('<i class="fa-solid fa-credit-card"></i> Pagar Seleccionados');
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                // Si la llamada AJAX falla
                console.error("Error en AJAX:", textStatus, errorThrown);
                toastr.error('Error: ' + errorThrown);

                // Habilitar botón de nuevo
                pagarBtn.prop('disabled', false).html('<i class="fa-solid fa-credit-card"></i> Pagar Seleccionados');
            }
        });
    });

    // Carga inicial del total
    recalcularTotal();

});