import { mostrarMensaje, obtenerDatos } from "./comun.js";

async function cargarRegatas() {
    try {
        const regatas = await obtenerDatos("/api/regatas");

        if (regatas.length === 0) {
            mostrarMensaje("No hay regatas registradas.", "info");
            return;
        }

        const tbody = document.getElementById("tabla-regatas");
        for (const regata of regatas) {
            const fila = document.createElement("tr");
            fila.innerHTML = `
                <td>${regata.id}</td>
                <td>${regata.nombre}</td>
                <td>${regata.lugar}</td>
                <td>${regata.fecha}</td>
                <td>${regata.distancia}</td>
                <td>${regata.totalBarcosInscritos}</td>
            `;
            tbody.appendChild(fila);
        }
    } catch (error) {
        mostrarMensaje(`No se pudieron cargar las regatas: ${error.message}`, "danger");
    }
}

cargarRegatas();

