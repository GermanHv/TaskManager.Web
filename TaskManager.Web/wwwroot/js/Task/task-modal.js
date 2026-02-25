document.addEventListener("DOMContentLoaded", () => {

    const modal = new bootstrap.Modal(document.getElementById("taskModal"));
    const modalContent = document.getElementById("taskModalContent");

    // CREAR
    document.getElementById("btnCrearTask")
        .addEventListener("click", async () => {

            modalContent.innerHTML = spinnerHtml();

            const response = await fetch("/Tasks/CreatePartial");
            console.log(response);
            const html = await response.text();

            modalContent.innerHTML = html;
            modal.show();
        });

    // EDITAR
    document.addEventListener("click", async (e) => {
        if (e.target.matches(".btnEdit")) {

            const id = e.target.dataset.id;

            modalContent.innerHTML = spinnerHtml();

            const response = await fetch('/Tasks/EditPartial/' + id);
            const html = await response.text();

            modalContent.innerHTML = html;
            modal.show();
        }
    });

    // GUARDAR
    document.addEventListener("click", async (e) => {
        if (e.target.id === "btnSaveTask") {

            const form = document.getElementById("taskForm");
            const formData = new FormData(form);

            const data = Object.fromEntries(formData.entries());

            const isEdit = data.Id && data.Id !== "0";

            const url = isEdit
                ? '/Tasks/Edit/' + data.Id
                : '/Tasks/Create';
            console.log(JSON.stringify(data));

            const response = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                const html = await response.text();
                modalContent.innerHTML = html;
                return;
            }

            modal.hide();

            await refreshTable();
        }
    });

});

function spinnerHtml() {
    return `
        <div class="modal-body text-center">
            <div class="spinner-border text-primary"></div>
            <p>Cargando...</p>
        </div>`;
}

async function refreshTable() {
    const response = await fetch("/Tasks/LoadTablePartial");
    const html = await response.text();
    document.getElementById("taskTableContainer").innerHTML = html;
}