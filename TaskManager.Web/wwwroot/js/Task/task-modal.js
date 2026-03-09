document.addEventListener("DOMContentLoaded", () => {

    const modal = new bootstrap.Modal(document.getElementById("taskModal"));
    const modalContent = document.getElementById("taskModalContent");
    const btnBuscar = document.getElementById("btnBuscarAjax");
    const contenedor = document.getElementById("taskTableContainer");
    const formulario = document.getElementById("filterForm");

    // CREAR
    document.getElementById("btnCrearTask")
        .addEventListener("click", async () => {

            modalContent.innerHTML = spinnerHtml();

            const response = await fetch("/Tasks/CreatePartial");
            const html = await response.text();

            modalContent.innerHTML = html;

            // 🔹 Cargar categorías después de insertar el HTML
            await loadCategoriesInModal(modalContent);

            modal.show();
        });

    // EDITAR
    document.addEventListener("click", async (e) => {
        if (e.target.matches(".btnEdit")) {

            const id = e.target.dataset.id;

            modalContent.innerHTML = spinnerHtml();

            const response = await fetch(`/Tasks/EditPartial/${id}`);
            const html = await response.text();

            modalContent.innerHTML = html;

            // 🔹 Cargar categorías y seleccionar la del modelo
            await loadCategoriesInModal(modalContent);

            modal.show();
        }
    });

    // GUARDAR
    document.addEventListener("click", async (e) => {
        if (e.target.id === "btnSaveTask") {

            const form = document.getElementById("taskForm");
            const formData = new FormData(form);
            //const data = Object.fromEntries(formData.entries());

            // Obtenemos el Id directamente del formData para construir la URL
            const id = formData.get("Id");
            const isEdit = id && id !== "0";

            const url = isEdit
                ? `/Tasks/EditAjax/${id}`
                : `/Tasks/CreateAjax`;

            //console.log(JSON.stringify(data));
            //const response = await fetch(url, {
            //    method: "POST",
            //    headers: { "Content-Type": "application/json" },
            //    body: JSON.stringify(data)
            //});

            // Enviamos el formData directo. 
            // C# (sin [FromBody]) lo entenderá a la perfección.
            const response = await fetch(url, {
                method: "POST",
                body: formData
            });

            if (!response.ok) {
                // Si el controlador devuelve una partial con errores:
                const html = await response.text();
                modalContent.innerHTML = html;

                //  Es importante volver a cargar las categorías después
                await loadCategoriesInModal(modalContent);

                return;
            }

            modal.hide();

            await refreshTable();
        }
    });

    // EVENTO PARA LA PAGINACIÓN CON FILTROS
    document.addEventListener("click", async (e) => {
        // Verifica si el clic fue en un enlace de paginación
        if (e.target.matches(".page-link-btn")) {
            e.preventDefault(); // Evita comportamientos por defecto si fuera un enlace normal

            const page = e.target.dataset.page; // Obtiene el número de página del atributo data-page
            if (!page) return;

            const contenedor = document.getElementById("taskTableContainer");
            const formulario = document.getElementById("filterForm");

            contenedor.innerHTML = spinnerHtml(); // Muestra el loader mientras carga

            // 1. Convertimos el formulario actual en QueryString para conservar los filtros
            const formData = new FormData(formulario);
            const query = new URLSearchParams();

            formData.forEach((value, key) => {
                if (value !== null && value !== "") {
                    query.append(key, value);
                }
            });

            // 2. Agregamos el número de página a la consulta
            query.set("Page", page);

            // 3. Construimos la URL con los filtros y la página
            const url = '/Tasks/LoadTablePartial?' + query.toString();

            try {
                // 4. Llamada AJAX
                const response = await fetch(url);

                if (!response.ok) {
                    contenedor.innerHTML = "<p>Error al cargar resultados de la página.</p>";
                    return;
                }

                const html = await response.text();

                // 5. Reemplazamos la tabla con los nuevos resultados
                contenedor.innerHTML = html;
            } catch (error) {
                console.error(error);
                contenedor.innerHTML = "<p>Error de conexión al cambiar de página.</p>";
            }
        }
    });

});

// Eliminar
document.addEventListener("click", async (e) => {
    if (e.target.matches(".btnDelete")) {

        const id = e.target.dataset.id;
        if (!id) return;

        const confirmado = confirm("¿Seguro que deseas eliminar esta tarea?");
        if (!confirmado) return;

        try {
            const response = await fetch('/Tasks/DeleteAjax/' + id, {
                method: "POST"
            });

            const result = await response.json();

            if (!response.ok || !result.success) {
                showError(result.message || "No se pudo eliminar la tarea.");
                return;
            }

            showSuccess(result.message || "La tarea fue eliminada correctamente.");

            await refreshTable();

        } catch (err) {
            console.error(err);
            showError("Error de comunicación con el servidor al eliminar la tarea.");
        }
    }
});
function spinnerHtml() {
    return `
       <div class="modal-body text-center">
           <div class="spinner-border text-primary"></div>
           <p>Cargando...</p>
       </div>`;
}

async function refreshTable() {
    const contenedor = document.getElementById("taskTableContainer");
    const formulario = document.getElementById("filterForm");

    // Muestra el spinner mientras carga la tabla actualizada
    contenedor.innerHTML = spinnerHtml();

    // Lee los filtros actuales que el usuario tiene escritos en los inputs
    const formData = new FormData(formulario);
    const query = new URLSearchParams();

    formData.forEach((value, key) => {
        if (value !== null && value !== "") {
            query.append(key, value);
        }
    });

    // Pide la tabla al servidor PERO enviándole los filtros para que los conserve
    const url = '/Tasks/LoadTablePartial?' + query.toString();

    try {
        const response = await fetch(url);
        if (!response.ok) {
            contenedor.innerHTML = "<p>Error al recargar la tabla.</p>";
            return;
        }
        const html = await response.text();
        contenedor.innerHTML = html;
    } catch (err) {
        console.error("Error recargando la tabla:", err);
    }
}

// Función para cargar las categorías en el select del modal
async function loadCategoriesInModal(modalContent) {

    const select = modalContent.querySelector("#categorySelect");
    if (!select) return;

    // Valor actual (cuando edito)
    const selectedId = select.dataset.selectedCategoryId || "";

    try {
        console.log("Actualizado");
        const response = await fetch("../Categories/Options");

        if (!response.ok) {
            console.error("Error al cargar categorías");
            return;
        }

        const categories = await response.json();

        // Limpiamos las opciones actuales excepto la primera
        const firstOption = select.querySelector("option[value='']");
        select.innerHTML = "";
        if (firstOption) {
            select.appendChild(firstOption);
        } else {
            const defaultOpt = document.createElement("option");
            defaultOpt.value = "";
            defaultOpt.textContent = "-- Seleccione una categoría --";
            select.appendChild(defaultOpt);
        }

        // Agregamos las categorías
        categories.forEach(cat => {
            const opt = document.createElement("option");
            opt.value = cat.id;
            opt.textContent = cat.name;

            if (selectedId && selectedId === String(cat.id)) {
                opt.selected = true;
            }

            select.appendChild(opt);
        });

    } catch (err) {
        console.error("Error de red al cargar categorías", err);
    }
}

// ✅ Funciones auxiliares para mostrar mensajes
function showSuccess(message) {
    const container = document.getElementById("alertContainer");
    if (!container) return;

    container.innerHTML = `
       <div class="alert alert-success alert-dismissible fade show" role="alert">
           ${message}
           <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
       </div>`;
}

function showError(message) {
    const container = document.getElementById("alertContainer");
    if (!container) return;

    container.innerHTML = `
       <div class="alert alert-danger alert-dismissible fade show" role="alert">
           ${message}
           <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
       </div>`;
}