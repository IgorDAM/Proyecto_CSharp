import { mostrarMensaje, obtenerDatos } from "./comun.js";

async function cargarTripulantes() {
    try {
        const tripulantes = await obtenerDatos("/api/tripulantes");

        if (tripulantes.length === 0) {
            mostrarMensaje("No hay tripulantes registrados.", "info");
            return;
        }

        const tbody = document.getElementById("tabla-tripulantes");
        for (const tripulante of tripulantes) {
            const fila = document.createElement("tr");
            fila.innerHTML = `
                <td>${tripulante.id}</td>
                <td>${tripulante.nombre}</td>
                <td>${tripulante.rol}</td>
                <td>${tripulante.barcoId}</td>
                `;
            tbody.appendChild(fila);
        }
    } catch (error) {
        mostrarMensaje(`No se pudieron cargar los tripulantes: ${error.message}`, "danger");
    }
}

cargarTripulantes();