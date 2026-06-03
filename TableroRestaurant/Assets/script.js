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

        if (p.Estado === "PREPARANDO")
            card.classList.add("preparando");

        if (p.Estado === "LISTO")
            card.classList.add("listo");

        if (p.Estado === "ENTREGADO")
            card.classList.add("entregado");

        document.getElementById("listaPedidos")
            .appendChild(clone);
    });
}

function limpiar() {
    document.getElementById("listaPedidos").innerHTML = "";
}

async function agregarPedido() {

    let numero = document.getElementById("txtNumero").value;

    let response = await fetch("/restaurant/agregar", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            Numero: numero
        })
    });

    let mensaje = await response.text();

    if (mensaje !== "OK") {
        alert(mensaje);
        return;
    }

    document.getElementById("txtNumero").value = "";

    cargarPedidos();
}

async function marcarListo(id) {

    let response = await fetch("/restaurant/listo", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            Id: id
        })
    });

    let mensaje = await response.text();

    if (mensaje !== "OK") {
        alert(mensaje);
        return;
    }

    cargarPedidos();
}

async function entregarPedido(id) {

    let response = await fetch("/restaurant/entregar", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            Id: id
        })
    });

    let mensaje = await response.text();

    if (mensaje !== "OK") {
        alert(mensaje);
        return;
    }

    cargarPedidos();
}

cargarPedidos();
setInterval(cargarPedidos, 2000);