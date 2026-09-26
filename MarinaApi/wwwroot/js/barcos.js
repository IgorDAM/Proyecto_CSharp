function mostrarMensaje(texto, tipo) {
    const mensaje = document.getElementById("mensaje");
    mensaje.textContent = texto;
    mensaje.className = `alert alert-${tipo}`;
}

async function cargarBarcos() {
    try {
        const respuesta = await fetch("/api/barcos");

        if (!respuesta.ok) {
            throw new Error(`La API respondió ${respuesta.status}`);
        }

        const barcos = await respuesta.json();

        if (barcos.length === 0) {
            mostrarMensaje("No hay barcos registrados.", "info");
            return;
        }

        const tbody = document.getElementById("tabla-barcos");
        for (const barco of barcos) {
            const fila = document.createElement("tr");
            fila.innerHTML = `
                <td>${barco.id}</td>
                <td>${barco.nombre}</td>
                <td>${barco.tipo}</td>
                <td>${barco.eslora}</td>
                <td>${barco.manga}</td>
                <td>${barco.capacidad}</td>
            `;
            tbody.appendChild(fila);
        }
    } catch (error) {
        mostrarMensaje(`No se pudieron cargar los barcos: ${error.message}`, "danger");
    }
}

cargarBarcos();