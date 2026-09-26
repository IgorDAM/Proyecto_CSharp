import { mostrarMensaje, obtenerDatos } from "./comun.js";

async function cargarBarcos() {
    try {
        const barcos = await obtenerDatos("/api/barcos");

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