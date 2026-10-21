export function mostrarMensaje(texto, tipo) {
    const mensaje = document.getElementById("mensaje");
    mensaje.textContent = texto;
    mensaje.className = `alert alert-${tipo}`;
}

export async function obtenerDatos(url) {
    const respuesta = await fetch(url);

    if (!respuesta.ok) {
        throw new Error(`La API respondió ${respuesta.status}`);
    }

    return await respuesta.json();
}