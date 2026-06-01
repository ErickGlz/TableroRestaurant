async function cargarPedidos() {

    let response =
        await fetch("/restaurant/pedidos");

    let pedidos = await response.json();

    let lista =
        document.getElementById("listaPedidos");

    lista.innerHTML = "";

    pedidos.forEach(pedido => {

        lista.innerHTML += `
        
        <div class="pedido">

            <div class="info">

                <div class="numero">
                    ${pedido.Numero}
                </div>

                <div class="estado">
                    ${pedido.Estado}
                </div>

            </div>

            <div class="botones">

                <button class="btnListo"
                        onclick="marcarListo(${pedido.Id})">
                    Listo
                </button>

                <button class="btnEntregar"
                        onclick="entregarPedido(${pedido.Id})">
                    Entregar
                </button>

            </div>

        </div>
        
        `;
    });
}

async function agregarPedido() {

    let numero =
        document.getElementById("txtNumero").value;

    if (numero.trim() == "") {
        return;
    }

    await fetch("/restaurant/agregar", {
        method: "POST",

        headers: {
            "Content-Type": "application/json"
        },

        body: JSON.stringify({
            Numero: numero
        })
    });

    document.getElementById("txtNumero").value = "";

    cargarPedidos();
}

async function marcarListo(id) {

    await fetch("/restaurant/listo", {
        method: "POST",

        headers: {
            "Content-Type": "application/json"
        },

        body: JSON.stringify({
            Id: id
        })
    });

    cargarPedidos();
}

async function entregarPedido(id) {

    await fetch("/restaurant/entregar", {
        method: "POST",

        headers: {
            "Content-Type": "application/json"
        },

        body: JSON.stringify({
            Id: id
        })
    });

    cargarPedidos();
}

cargarPedidos();

setInterval(cargarPedidos, 2000);