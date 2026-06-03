document.getElementById("btnAgregar")
    .addEventListener("click", agregarPedido);

async function cargarPedidos() {

    let response = await fetch("/restaurant/pedidos");
    let pedidos = await response.json();

    limpiar();

    pedidos.forEach(p => {

        const template = document.getElementById("templatePedido");
        const clone = template.content.cloneNode(true);

        clone.querySelector(".numero").textContent = p.Numero;
        clone.querySelector(".estado").textContent = p.Estado;

        clone.querySelector(".btnListo")
            .addEventListener("click", () => marcarListo(p.Id));

        clone.querySelector(".btnEntregar")
            .addEventListener("click", () => entregarPedido(p.Id));

        const card = clone.querySelector(".card");

        if (p.Estado === "PREPARANDO") {
            document.querySelector(".pendiente").appendChild(card);
        }

        if (p.Estado === "LISTO") {
            document.querySelector(".listo").appendChild(card);
        }

        if (p.Estado === "ENTREGADO") {
            document.querySelector(".entregado").appendChild(card);
        }
    });
}

function limpiar() {
    document.querySelector(".pendiente").innerHTML = "";
    document.querySelector(".listo").innerHTML = "";
    document.querySelector(".entregado").innerHTML = "";
}

async function agregarPedido() {

    let numero = document.getElementById("txtNumero").value;

    if (numero.trim() === "") return;

    await fetch("/restaurant/agregar", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Numero: numero })
    });

    document.getElementById("txtNumero").value = "";

    cargarPedidos();
}

async function marcarListo(id) {

    await fetch("/restaurant/listo", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Id: id })
    });

    cargarPedidos();
}

async function entregarPedido(id) {

    await fetch("/restaurant/entregar", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Id: id })
    });

    cargarPedidos();
}

cargarPedidos();
setInterval(cargarPedidos, 2000);