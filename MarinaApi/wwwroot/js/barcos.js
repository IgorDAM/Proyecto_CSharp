async function cargarBarcos() {
    const respuesta = await fetch("/api/barcos");
    const barcos = await respuesta.json();
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
}

cargarBarcos();