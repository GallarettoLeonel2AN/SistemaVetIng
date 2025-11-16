document.addEventListener('DOMContentLoaded', function () {

    const vacunasContainer = document.getElementById('vacunas-container');
    const estudiosContainer = document.getElementById('estudios-container');
    const addVacunaBtn = document.getElementById('add-vacuna-btn');
    const addEstudioBtn = document.getElementById('add-estudio-btn');

    function getNextIndex(container) {
        return container.querySelectorAll('.v1-input-dynamic-group').length;
    }

    function addItem(templateId, container, namePrefix) {

        const template = document.getElementById(templateId);
        const clone = template.content.cloneNode(true);

        const selectElement = clone.querySelector('select');

        const newIndex = getNextIndex(container);

        selectElement.name = `${namePrefix}[${newIndex}]`;

        container.appendChild(clone);
    }

    if (addVacunaBtn) {
        addVacunaBtn.addEventListener('click', function () {
            addItem('vacuna-template', vacunasContainer, 'VacunasSeleccionadasIds');
        });
    }

    if (addEstudioBtn) {
        addEstudioBtn.addEventListener('click', function () {
            addItem('estudio-template', estudiosContainer, 'EstudiosSeleccionadosIds');
        });
    }

    document.addEventListener('click', function (e) {

        const removeButton = e.target.closest('.v1-btn-remove');

        if (!removeButton) return;

        e.preventDefault();

        const inputGroup = removeButton.closest('.v1-input-dynamic-group');

        if (!inputGroup) return;

        const container = inputGroup.parentNode;

        inputGroup.remove();

        reindexItems(container);
    });

    function reindexItems(container) {

        const groups = container.querySelectorAll('.v1-input-dynamic-group select');

        groups.forEach((select, index) => {

            const prefix = select.name.split('[')[0];

            select.name = `${prefix}[${index}]`;
        });
    }

});