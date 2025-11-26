document.addEventListener("DOMContentLoaded", function () {
    const checkboxes = document.querySelectorAll('.role-checkbox');

    let esVeterinario = false;
    let esCliente = false;

    checkboxes.forEach(cb => {
        if (cb.dataset.roleName === 'Veterinario' && cb.defaultChecked) {
            esVeterinario = true;
        }
        if (cb.dataset.roleName === 'Cliente' && cb.defaultChecked) {
            esCliente = true;
        }
    });

    // Aplicamos las reglas de bloqueo según el perfil detectado
    checkboxes.forEach(cb => {
        const role = cb.dataset.roleName;


        if (esCliente) {
            // Su propio rol: No se lo puede quitar
            if (role === 'Cliente') {
                lockExistingRole(cb, "No se puede quitar el rol base de Cliente.");
            }
            // Otros roles (Veterinario/Veterinaria): Prohibidos
            else {
                lockCheckbox(cb, "Un Cliente no puede tener roles del personal médico o administrativo.");
            }
        }


        else if (esVeterinario) {
            // Su propio rol: No se lo puede quitar
            if (role === 'Veterinario') {
                lockExistingRole(cb, "No se puede quitar el rol base de Veterinario.");
            }
            // Rol Cliente: Prohibido
            else if (role === 'Cliente') {
                lockCheckbox(cb, "Un Veterinario no puede ser asignado como Cliente.");
            }
  
        }
    });



    function lockExistingRole(cb, message) {
        cb.addEventListener('click', function (e) {
            e.preventDefault(); // Cancela la acción de desmarcar
            toastr.warning(message);
        });

        const parent = cb.closest('label') || cb.parentElement;
        if (parent) {
            parent.style.opacity = "0.7";
            parent.style.cursor = "not-allowed";
            parent.title = message; 
        }
    }
    function lockCheckbox(cb, message) {
        cb.disabled = true; // Deshabilitado real (no se envía en el form)
        const parent = cb.closest('label') || cb.parentElement;
        if (parent) {
            parent.style.opacity = "0.4"; // Se ve apagado
            parent.style.cursor = "not-allowed";
            parent.title = message;
        }
    }
});